using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class Turn : WormBody
    {
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
            directionPair = new DirectionPair(Direction.None, Direction.None);
            wormBase.setDecision(false);
        }

        private new void OnEnable()
        {
            FollowWaypointEvent += GetComponent<Movement>().onFollowWaypoint;
            ReachWaypointEvent += GetComponent<Movement>().onReachWaypoint;
            ReachWaypointEvent += GetComponent<WormTunnelBroker>().onReachWaypoint;
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
                Direction passWaypointDirection = destinationWaypoint.getPassWaypointDirection();
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
                    if (destinationWaypoint.move == MoveType.EXIT)
                    {
                        print("dslkjflkds");
                    }
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
            List<Waypoint> waypointList = new List<Waypoint> { startWP, cellWP, exitWP };
            FollowWaypointEvent(waypointList, directionPair.curDir);
        }

        /**
         * Initiate the turn if the tunnel is eligible
         * 
         * @tunnel          the tunnel turn is initiated in
         * @turnCellCenter  the center of the cell turn will happen in
         */
        private void turn(Tunnel.Tunnel tunnel, Vector3 turnCellCenter)
        {
            wormBase.setDecision(false);
            wormBase.setStraight(false);
            RaiseChangeDirectionEvent(directionPair, tunnel, wormId); // rotate tunnel in the direction
            Vector3 turnCellEntrancePosition = Tunnel.Tunnel.getOffsetPosition(directionPair.prevDir, turnCellCenter);
            initializeTurnWaypointList(directionPair, turnCellEntrancePosition);
        }

        /**
         * Once the worm has finished navigating out of a turn, append a straight tunnel to the corner
         * 
         * @direction is the exit direction
         */
        public void onExitTurn(Direction direction, Tunnel.Tunnel tunnel)
        {
            wormBase.setStraight(true);
            DirectionPair straightDirectionPair = new DirectionPair(direction, direction);
            RaiseChangeDirectionEvent(straightDirectionPair, tunnel, wormId);
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
            wormBase.setDecision(true);
            directionPair.prevDir = directionPair.curDir;
            directionPair.curDir = direction;
            print("decide to go in curDirection " + direction + " is straight tunnel " + isStraightTunnel + " tunnel name is " + tunnel.name);
            if (!isStraightTunnel) // turn immediately  if in junction or corner (dont wait for block interval event) :)
            {
                turn(tunnel, tunnel.center); 
            }
        }

        /**
         * Received when the active tunnel grows to a multiple of block size making it eligible for a direction change
         * 
         * @isBlockMultiple     did the straight tunnel reach a multiple of block size?
         * @prevTunnel              The tunnel PRIOR to the tunnel the decision to change direction will be made from
         */
        public void onBlockInterval(bool isBlockMultiple, Vector3Int blockPosition, Vector3Int lastBlockPositionInt, Tunnel.Tunnel prevTunnel)
        {
            if (isBlockMultiple && wormBase.isDecision) // initiate turn for straight tunnels
            {
                Vector3 prevCellCenter = Tunnel.Tunnel.getOffsetPosition(Direction.Up, lastBlockPositionInt);
                turn(prevTunnel, prevCellCenter);          
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

        private void OnDisable()
        {
            if (FindObjectOfType<Movement>())
            {
                FollowWaypointEvent -= GetComponent<Movement>().onFollowWaypoint;
                ReachWaypointEvent -= GetComponent<Movement>().onReachWaypoint;
                ReachWaypointEvent -= GetComponent<WormTunnelBroker>().onReachWaypoint;
                TorqueEvent -= GetComponent<Force>().onTorque;
                TorqueEvent -= GetComponent<InputProcessor>().onTorque;
            }
        }
    }

}