using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class Turn : WormBody
    {
        private bool isDecision; // flag to check if a decision has been made
        private bool isBlockSizeMultiple;

        private List<Waypoint> waypointList; // list of points to move to when a corner is created

        private Waypoint destinationWaypoint;

        private bool isWaypointReached; // a flag that ensures only do waypoint reached code once

        DirectionPair directionPair;

        public delegate void ReachWaypoint(Waypoint waypoint);
        public ReachWaypoint ReachWaypointEvent;

        public delegate void FollowWaypoint(List<Waypoint> waypointList, Direction egressDirection);
        public event FollowWaypoint FollowWaypointEvent;

        public delegate void AddTorque(DirectionPair dirPair, Waypoint waypoint);
        public event AddTorque TorqueEvent;

        private new void Awake()
        {
            base.Awake();
            destinationWaypoint = null;
            isWaypointReached = true;
            isDecision = false;
            isBlockSizeMultiple = true; // used to initialize straight tunnel
            directionPair = new DirectionPair(Direction.None, Direction.None);
            waypointList = new List<Waypoint>();
        }

        private new void OnEnable()
        {
            FollowWaypointEvent += GetComponent<Movement>().onFollowWaypoint;
            ReachWaypointEvent += GetComponent<Movement>().onReachWaypoint;
            TorqueEvent += GetComponent<Force>().onTorque;
            TorqueEvent += GetComponent<InputProcessor>().onTorque;
        }

        /**
         * Determines if waypoint is reached using rigidbody position
         */
        void Update()
        {
            if (!isWaypointReached)
            {
                Direction passWaypointDirection = destinationWaypoint.dirPair.prevDir;
                bool isNegativeDir = Dir.Base.isDirectionNegative(passWaypointDirection);
                float waypointAxisPosition = Dir.Vector.getAxisScaleFromDirection(passWaypointDirection, destinationWaypoint.position);
                float ringAxisPosition = Dir.Vector.getAxisPositionFromDirection(passWaypointDirection, ring.position);

                isWaypointReached = isNegativeDir ? ringAxisPosition <= waypointAxisPosition : ringAxisPosition >= waypointAxisPosition;
                if (isWaypointReached)
                {
                    if (!wormBase.isStraight) // apply torque if not going in a straight direction to exit waypoint
                    {
                        TorqueEvent(directionPair, destinationWaypoint);
                    }
                    if (directionPair.prevDir == Direction.Forward && directionPair.curDir == Direction.Right && destinationWaypoint.move == MoveType.CENTER)
                    {
                        print("break");
                    }
                    print("re: prevDir " + directionPair.prevDir + " curDir " + directionPair.curDir + " waypoint is reached for " + destinationWaypoint.move + " destination position is " + destinationWaypoint.position + " ring position is " + ring.position);
                    ReachWaypointEvent(destinationWaypoint);
                }
            }
        }

        private Vector3 getOffsetPosition(Vector3 position, Direction direction, float offset)
        {
            Vector3 offsetVector = Dir.CellDirection.getUnitVectorFromDirection(direction) * offset;
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
            Vector3 exitWaypointPosition = getOffsetPosition(centerWaypointPosition, directionPair.curDir, Tunnel.Tunnel.CENTER_OFFSET); // 2.0, 3.5, ...

            Waypoint startWP = new Waypoint(startWaypointPosition, MoveType.ENTRANCE, directionPair);
            Waypoint cellWP = new Waypoint(centerWaypointPosition, MoveType.CENTER, directionPair);
            Waypoint exitWP = new Waypoint(exitWaypointPosition, MoveType.EXIT, directionPair);

            waypointList = new List<Waypoint> { startWP, cellWP, exitWP };
            FollowWaypointEvent(waypointList, directionPair.curDir);
        }

        /**
         * Initiate the turn if the tunnel is eligible
         */
        private void turn(Tunnel.Tunnel tunnel)
        {
            isDecision = false;
            wormBase.isStraight = false;
            RaiseChangeDirectionEvent(directionPair, wormId); // rotate tunnel in the direction
            Vector3 egressPosition = Tunnel.Tunnel.getEgressPosition(directionPair.prevDir, tunnel.center);
            initializeTurnWaypointList(directionPair, egressPosition);          
        }

        /**
         * Once the worm has finished navigating out of a turn, append a straight tunnel to the corner
         * 
         * @direction is the exit direction
         */
        public void onExitTurn(Direction direction)
        {
            wormBase.isStraight = true;
            DirectionPair straightDirectionPair = new DirectionPair(direction, direction);
            RaiseChangeDirectionEvent(straightDirectionPair, wormId);
            //Vector3 offsetHeadWaypoint = getOffsetPosition(ring.position, direction, Tunnel.TunnelManager.Instance.START_TUNNEL_RING_OFFSET);
            //Waypoint offsetWP = new Waypoint(offsetHeadWaypoint, MoveType.OFFSET, direction);
            //List<Waypoint> offsetWaypointList = new List<Waypoint>() { offsetWP };
            //FollowWaypointEvent(offsetWaypointList, straightDirectionPair);
        }

        /**
         * Setup the initial direciton
         */
        public void onInitDecision(Direction direction, string wormId, Vector3Int initialCell)
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
            print("decide to go in curDirection " + direction + " is straight tunnel " + isStraightTunnel + " tunnel name is " + tunnel.name);
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
            destinationWaypoint = waypoint;
            isWaypointReached = false;
        }

        /**
         * Check if tunnel meets conditions to be eligible for a turn from a straight tunnel
         */
        public bool isTurning()
        {
            if (isBlockSizeMultiple && isDecision)
            {
                return true;
            }
            return false;            
        }

        private new void OnDisable()
        {
            if (FindObjectOfType<Movement>())
            {
                FollowWaypointEvent -= GetComponent<Movement>().onFollowWaypoint;
                ReachWaypointEvent -= GetComponent<Movement>().onReachWaypoint;
                TorqueEvent -= GetComponent<Force>().onTorque;
                TorqueEvent -= GetComponent<InputProcessor>().onTorque;
            }
        }
    }

}