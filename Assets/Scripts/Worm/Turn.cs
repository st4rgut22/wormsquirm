using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class Turn : WormBody
    {
        private Waypoint destinationWaypoint;

        private bool isWaypointReached; // a flag that ensures only do waypoint reached code once

        DirectionPair directionPair;

        public delegate void SetTurnCell(Vector3Int turnCell);
        public SetTurnCell SetTurnCellEvent;

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
            wormBase.setPendingTurn(false);
        }

        private new void OnEnable()
        {
            base.OnEnable();
            FollowWaypointEvent += GetComponent<Movement>().onFollowWaypoint;
            ReachWaypointEvent += GetComponent<Movement>().onReachWaypoint;
            ReachWaypointEvent += GetComponent<WormTunnelBroker>().onReachWaypoint;
            SetTurnCellEvent += GetComponent<WormTunnelBroker>().onSetTurnCell;
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
                    if (!wormBase.isStraight && !directionPair.isStraight()) // apply torque if not going in a straight direction to exit waypoint
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
            Vector3 centerWaypointPosition = getOffsetPosition(startWaypointPosition, directionPair.prevDir, Tunnel.Tunnel.CENTER_OFFSET);
            print("init waypoint list prev dir " + directionPair.prevDir + " cur dir " + directionPair.curDir + " center waypoint position is " + centerWaypointPosition);
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
        private void turn(Tunnel.Tunnel tunnel, Vector3 prevTurnCellCenter)
        {
            wormBase.setPendingTurn(false);
            wormBase.setStraight(false);
            RaiseChangeDirectionEvent(directionPair, tunnel, wormId); // rotate tunnel in the direction
            Vector3 turnCellEntrancePosition = Tunnel.Tunnel.getOffsetPosition(directionPair.prevDir, prevTurnCellCenter);
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
            wormBase.setTurnDirection(Direction.None); // reset turn direction 
            DirectionPair straightDirectionPair = new DirectionPair(direction, direction);
            RaiseChangeDirectionEvent(straightDirectionPair, tunnel, wormId);
        }

        /**
         * Setup the initial direciton
         */
        public void onInitDecision(Direction direction, string wormId, Vector3Int mappedInitialCell, Vector3Int initialCell)
        {
            directionPair.curDir = direction;
        }

        /**
         * turn has been aborted
         */
        public void onAbortDecision()
        {
            wormBase.setPendingTurn(false);
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
            wormBase.setPendingTurn(true);
            wormBase.setTurnDirection(direction);

            directionPair.prevDir = directionPair.curDir;
            directionPair.curDir = direction;
            print("decide to go in curDirection " + direction + " position is " + transform.position + " is straight tunnel " + isStraightTunnel + " tunnel name is " + tunnel.name);
            if (!isStraightTunnel) // turn immediately  if in junction or corner (dont wait for block interval event) :)
            {
                turn(tunnel, tunnel.center);
                SetTurnCellEvent(tunnel.getLastCellPosition());
            }
        }

        /**
         * Received when the active tunnel grows to a multiple of block size making it eligible for a direction change
         * 
         * @isBlockMultiple     did the straight tunnel reach a multiple of block size?
         * @prevTunnel              The tunnel PRIOR to the turn tunnel
         */
        public void onBlockInterval(bool isBlockMultiple, Vector3Int blockPosition, Vector3Int lastBlockPositionInt, Tunnel.Tunnel prevTunnel)
        {
            if (isBlockMultiple)
            {
                print("is block multiple block Position is " + blockPosition + " last block position is " + lastBlockPositionInt + " for tunnel " + prevTunnel.name + " is decision " + wormBase.isPendingTurn);
            }
            if (isBlockMultiple && wormBase.isPendingTurn) // initiate turn for straight tunnels
            {
                Vector3 prevCellCenter = Tunnel.Tunnel.getOffsetPosition(Direction.Up, lastBlockPositionInt);
                turn(prevTunnel, prevCellCenter);
                SetTurnCellEvent(blockPosition);
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

        private new void OnDisable()
        {
            base.OnDisable();
            if (FindObjectOfType<Movement>())
            {
                FollowWaypointEvent -= GetComponent<Movement>().onFollowWaypoint;
                ReachWaypointEvent -= GetComponent<Movement>().onReachWaypoint;
                ReachWaypointEvent -= GetComponent<WormTunnelBroker>().onReachWaypoint;
                TorqueEvent -= GetComponent<Force>().onTorque;
                TorqueEvent -= GetComponent<InputProcessor>().onTorque;
                SetTurnCellEvent -= GetComponent<WormTunnelBroker>().onSetTurnCell;
            }
        }
    }

}