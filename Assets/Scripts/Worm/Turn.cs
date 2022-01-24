using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class Turn : WormBody
    {
        private bool isWaitBlockEvent; // if straight tunnel, then it is true because we have to wait until tunnel length is a multiple of BLOCK_INTERVAL
        private bool isDecision; // flag to check if a decision has been made
        private bool isBlockSizeMultiple;

        private List<Waypoint> waypointList; // list of points to move to when a corner is created

        private Waypoint destinationWaypoint;

        private bool isWaypointReached; // a flag that ensures only do waypoint reached code once

        DirectionPair directionPair;

        public delegate void ReachWaypoint(Waypoint waypoint);
        public ReachWaypoint ReachWaypointEvent;

        public delegate void ChangeDirection(DirectionPair directionPair, string wormId);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void FollowWaypoint(List<Waypoint> waypointList, DirectionPair directionPair);
        public event FollowWaypoint FollowWaypointEvent;

        public delegate void AddTorque(DirectionPair dirPair, Waypoint waypoint);
        public event AddTorque TorqueEvent;

        private new void Awake()
        {
            destinationWaypoint = null;
            isWaypointReached = true;
            isDecision = false;
            isWaitBlockEvent = false;
            isBlockSizeMultiple = true; // used to initialize straight tunnel
            directionPair = new DirectionPair(Direction.None, Direction.None);
            waypointList = new List<Waypoint>();
        }

        private void OnEnable()
        {
            Tunnel.CollisionManager.Instance.CreateJunctionEvent += onCreateJunction;
            ChangeDirectionEvent += Tunnel.CollisionManager.Instance.onChangeDirection;
            ChangeDirectionEvent += GetComponent<InputProcessor>().onChangeDirection;
            FollowWaypointEvent += FindObjectOfType<Movement>().onFollowWaypoint;
            ReachWaypointEvent += GetComponent<Movement>().onReachWaypoint;
            TorqueEvent += GetComponent<Force>().onTorque;
        }

        /**
         * Determines if waypoint is reached using rigidbody position
         */
        void Update()
        {
            if (!isWaypointReached)
            {
                Direction passWaypointDirection = destinationWaypoint.direction;
                bool isNegativeDir = Dir.Base.isDirectionNegative(passWaypointDirection);
                float waypointAxisPosition = Dir.Vector.getAxisScaleFromDirection(passWaypointDirection, destinationWaypoint.position);
                float ringAxisPosition = Dir.Vector.getAxisPositionFromDirection(passWaypointDirection, ring.position);

                isWaypointReached = isNegativeDir ? ringAxisPosition <= waypointAxisPosition : ringAxisPosition >= waypointAxisPosition;
                if (isWaypointReached)
                {
                    print("waypoint is reached for " + destinationWaypoint.move);
                    TorqueEvent(directionPair, destinationWaypoint);
                    ReachWaypointEvent(destinationWaypoint);
                }
            }
        }

        private Vector3 getOffsetPosition(Vector3 position, Direction direction, float offset)
        {
            Vector3 offsetVector = Dir.Vector.getUnitVectorFromDirection(direction) * offset;
            return position + offsetVector;
        }

        /**
         * Initialize waypoint list with two points: the center of the corner and the exit point
         * 
         * @directionPair, the ingress and egress direction of the worm
         */
        private void initializeTurnWaypointList(DirectionPair directionPair, Vector3 startWaypointPosition)
        {
            print("init waypoint list prev dir " + directionPair.prevDir + " cur dir " + directionPair.curDir);
            Vector3 centerWaypointPosition = getOffsetPosition(startWaypointPosition, directionPair.prevDir, Tunnel.Tunnel.CENTER_OFFSET);
            Vector3 exitWaypointPosition = getOffsetPosition(centerWaypointPosition, directionPair.curDir, Tunnel.Tunnel.CENTER_OFFSET);

            Waypoint startWP = new Waypoint(startWaypointPosition, MoveType.ENTRANCE, directionPair.prevDir);
            Waypoint cellWP = new Waypoint(centerWaypointPosition, MoveType.CENTER, directionPair.curDir);
            Waypoint exitWP = new Waypoint(exitWaypointPosition, MoveType.EXIT, directionPair.curDir);

            waypointList = new List<Waypoint> { startWP, cellWP, exitWP };
            FollowWaypointEvent(waypointList, directionPair);
        }

        /**
         * Instruct worm to enter a junction
         *
         *@cellMove info about the junction such as center position, ingress position
         */
        public void onCreateJunction(Tunnel.Tunnel collisionTunnel, DirectionPair dirPair, Tunnel.CellMove cellMove, Tunnel.Tunnel currentTunnel)
        {
            if (currentTunnel.wormCreatorId == wormId)
            {
                Tunnel.CollisionManager.Instance.isCreatingNewTunnel = true;

                if (dirPair.isStraight()) // means we are not executing a turn into the junction, let initializeTurnWPList handle turns
                {
                    Vector3 exitPos = getOffsetPosition(cellMove.startPosition, dirPair.curDir, Tunnel.Tunnel.BLOCK_SIZE);

                    Waypoint exitJctWP = new Waypoint(exitPos, MoveType.EXIT, dirPair.curDir);

                    waypointList = new List<Waypoint>() { exitJctWP }; // go the ingress position of the junction
                    FollowWaypointEvent(waypointList, dirPair);
                }
            }
        }

        /**
         * Initiate the turn if the tunnel is eligible
         */
        private void turn(Tunnel.Tunnel tunnel)
        {
            isDecision = false;
            ChangeDirectionEvent(directionPair, wormId); // rotate tunnel in the direction
            Vector3 egressPosition = Tunnel.Tunnel.getEgressPosition(directionPair.prevDir, tunnel.center);
            initializeTurnWaypointList(directionPair, egressPosition);          
        }

        /**
         * Once the worm has finished navigating out of a turn
         * 
         * @direction is the exit direction
         */
        public void onCompleteTurn(Direction direction)
        {
            DirectionPair straightDirectionPair = new DirectionPair(direction, direction);
            ChangeDirectionEvent(straightDirectionPair, wormId);
            Vector3 offsetHeadWaypoint = getOffsetPosition(ring.position, direction, Tunnel.TunnelManager.Instance.START_TUNNEL_RING_OFFSET);
            Waypoint offsetWP = new Waypoint(offsetHeadWaypoint, MoveType.OFFSET, direction);
            List<Waypoint> offsetWaypointList = new List<Waypoint>() { offsetWP };
            FollowWaypointEvent(offsetWaypointList, straightDirectionPair);
        }

        /**
         * Setup the initial direciton
         */
        public void onInitDecision(Direction direction, string wormId)
        {
            directionPair.curDir = direction;
        }

        /**
         * Called on receipt of decision to change direction
         * 
         * @isWaitBlockEvent before executing turn, should wait for blockIntervalEvent? (applicable for straight tunnels)
         * @direction the new direction
         * @tunnel The tunnel the decision to change direction is made from
         */
        public void onDecision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel)
        {
            isDecision = true;
            directionPair.prevDir = directionPair.curDir;
            directionPair.curDir = direction;
            print("decide to go in curDirection " + direction);
            if (!isStraightTunnel) // turn immediately (dont wait for block interval event) if in junction or corner
            {
                turn(tunnel); 
            }
        }

        /**
         * Received when the active tunnel is a multiple of block size making it eligible for a direction change
         * 
         * @isBlockSizeMultiple did the straight tunnel reach a multiple of block size?
         * @tunnel The tunnel the decision to change direction is made from
         */
        public void onBlockInterval(bool isBlockSizeMultiple, Vector3Int blockPosition, Tunnel.Tunnel tunnel)
        {
            this.isBlockSizeMultiple = isBlockSizeMultiple;

            if (isTurning()) // initiate turn for straight tunnels
            {
                turn(tunnel);
            }
        }

        /**
         * Directs forces to reach a waypoint position
         * 
         * @waypointPosition the position of a waypoint
         */
        public void onMoveToWaypoint(Waypoint waypoint)
        {
            if (isWaypointReached)
            {
                if (waypoint.move == MoveType.OFFSET)
                {
                    print("break");
                }
                isWaypointReached = false;
                destinationWaypoint = waypoint;
            }
        }

        /**
         * Check if tunnel meets conditions to be eligible for a turn from a straight tunnel
         */
        public bool isTurning()
        {
            if (this.isBlockSizeMultiple && isDecision)
            {
                return true;
            }
            return false;            
        }

        private void OnDisable()
        {
            if (Tunnel.CollisionManager.Instance != null)
            {
                ChangeDirectionEvent -= Tunnel.CollisionManager.Instance.onChangeDirection;
                ChangeDirectionEvent -= GetComponent<InputProcessor>().onChangeDirection;
                Tunnel.CollisionManager.Instance.CreateJunctionEvent -= onCreateJunction;
            }
            if (FindObjectOfType<Movement>())
            {
                FollowWaypointEvent -= FindObjectOfType<Movement>().onFollowWaypoint;
                ReachWaypointEvent -= GetComponent<Movement>().onReachWaypoint;
                TorqueEvent -= GetComponent<Force>().onTorque;
            }
        }
    }

}