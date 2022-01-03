using UnityEngine;
using System.Collections.Generic;

namespace Worm
{
    /**
     * Worm movement
     */
    public class Movement: MonoBehaviour
    {
        private Direction direction; // direction of worm travel
        private Direction egressWaypointDirection; // direction exiting a corner, saved on receipt of followWaypoints event

        private int waypointIndex;

        private List<Waypoint> waypointList;
        private List<Waypoint> nextWaypointList; // queued up waypoint list if turning while navigating a corner

        public delegate void CompleteTurn(Direction direction); // when turn is completed notify Turn so we can proceed straight
        public event CompleteTurn CompleteTurnEvent;

        public delegate void Grow();
        public event Grow GrowEvent;

        public delegate void BlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel);
        public event BlockInterval BlockIntervalEvent;

        public delegate void DecisionProcessing(bool isDecisionProcessing);
        public event DecisionProcessing DecisionProcessingEvent;

        private const float SPEED = Tunnel.Tunnel.SCALED_GROWTH_RATE; // Match the tunnel growth rate

        private void Awake()
        {
            waypointIndex = 0;
            waypointList = new List<Waypoint>();
            nextWaypointList = new List<Waypoint>();
            transform.position = Tunnel.TunnelManager.Instance.initialCell;
            direction = Direction.None;
        }

        private void OnEnable() 
        {
            CompleteTurnEvent += Turn.Instance.onCompleteTurn;
            CompleteTurnEvent += FindObjectOfType<Controller>().onCompleteTurn;
            CompleteTurnEvent += FindObjectOfType<Rotation>().onCompleteTurn;
            DecisionProcessingEvent += FindObjectOfType<InputProcessor>().onDecisionProcessing;
        }

        public void onInitWormPosition(Vector3 initPos, Direction direction)
        {
            float offset = Tunnel.TunnelManager.Instance.WORM_OFFSET;
            Vector3 offsetVector = Dir.Vector.getUnitVectorFromDirection(direction);
            transform.position = initPos + offset * offsetVector;
        }

        private void FixedUpdate()
        {
            if (waypointList.Count > 0) // iterate over waypoint list
            {
                Waypoint waypoint = waypointList[waypointIndex];
                Vector3 waypointPos = waypoint.position;

                transform.position = Vector3.MoveTowards(transform.position, waypointPos, SPEED);

                if (transform.position.Equals(waypointPos))
                {
                    waypointIndex += 1;

                    if (waypoint.move == MoveType.CENTER)
                    {
                        DecisionProcessingEvent(false); // allow decisions to be made again
                    }
                    if (waypoint.move == MoveType.EXIT) 
                    {
                        completeTurn(waypoint);
                    }
                    if (waypoint.move == MoveType.OFFSET)
                    {
                        GrowEvent();
                        clearWaypoints(waypointList);
                    }
                }
            }
            else if (direction != Direction.None)
            {
                Vector3 unitVector = Dir.Vector.getUnitVectorFromDirection(direction);
                transform.position += unitVector * (float)Tunnel.Tunnel.SCALED_GROWTH_RATE; // position moves at the same rate as the straight tunnel
            }
        }

        private void clearWaypoints(List<Waypoint> waypoints)
        {
            waypoints.Clear();
            waypointIndex = 0;
        }

        /**
         * Finish a turn and check if any turns are queued up 
         * 
         * @waypoint The waypoint that completes the turn
         */
        private void completeTurn(Waypoint waypoint)
        {
            if (waypointIndex != waypointList.Count)
            {
                throw new System.Exception("completing turn should be the last action in waypoint list");
            }

            if (nextWaypointList.Count > 0) // additional turns
            {
                waypointList = new List<Waypoint>(nextWaypointList);
                clearWaypoints(nextWaypointList);
            }
            else
            {
                clearWaypoints(waypointList);
                CompleteTurnEvent(egressWaypointDirection); // goes straight
            }
            direction = egressWaypointDirection;
        }

        /**
         * After player's input is validated (ie it is along a plane not already traveling in) then issue the move command
         *
         *@deltaPosition is the change in position
         */
        public void onMove(Direction direction)
        {
            Vector3 moveUnitVector = Dir.Vector.getUnitVectorFromDirection(direction);
            transform.position += moveUnitVector * SPEED;
        }

        /**
         * Issue grow event for first tunnel because worm is initially offset from HEAD
         */
        public void onInitDecision(Direction direction)
        {
            GrowEvent();
        }

        /**
         * Follow waypoints when navigating corners. If already following waypoints, queue up the next waypoints list
         * 
         * @waypointList a list of coordinates the worm follows to navigate a corner
         */
        public void onFollowWaypoint(List<Waypoint> waypointList, DirectionPair directionPair)
        {
            egressWaypointDirection = directionPair.curDir; // save the last egress directionPair

            if (this.waypointList.Count > 0)
            {
                print("queue up next waypoint list");
                nextWaypointList = waypointList;
            }
            else
            {
                this.waypointList = waypointList;
            }
        }

        /**
         * When a straight tunnel is extended, the worm should follow in the same direction
         * 
         * @tunnel the tunnel that has been extended
         */
        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPosition, Tunnel.Straight tunnel)
        {
            direction = tunnel.growthDirection;
        }

        private void OnDisable()
        {
            if (Turn.Instance)
            {
                CompleteTurnEvent -= Turn.Instance.onCompleteTurn;
            }
            if (FindObjectOfType<Controller>())
            {
                CompleteTurnEvent -= FindObjectOfType<Controller>().onCompleteTurn;
            }
            if (FindObjectOfType<Rotation>())
            {
                CompleteTurnEvent -= FindObjectOfType<Rotation>().onCompleteTurn;
            }
            if (FindObjectOfType<InputProcessor>())
            {
                DecisionProcessingEvent -= FindObjectOfType<InputProcessor>().onDecisionProcessing;
            }
        }
    }
}
