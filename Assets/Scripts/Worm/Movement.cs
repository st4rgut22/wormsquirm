using UnityEngine;
using System.Collections.Generic;

namespace Worm
{
    /**
     * Worm movement
     */
    public class Movement : MonoBehaviour
    {
        private Direction direction; // direction of worm travel
        private Direction egressWaypointDirection; // direction exiting a corner, saved on receipt of followWaypoints event

        private Vector3 unitVectorDirection;
        private int waypointIndex;

        private List<Vector3> waypointList;
        private List<Vector3> nextWaypointList; // queued up waypoint list if turning while navigating a corner

        public delegate void Position(Vector3 position, Direction direction);
        public event Position PositionEvent;

        public delegate void CompleteTurn(Direction direction); // when turn is completed notify Turn so we can proceed straight
        public event CompleteTurn CompleteTurnEvent;

        public delegate void BlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel);
        public event BlockInterval BlockIntervalEvent;

        private const float SPEED = Tunnel.Tunnel.SCALED_GROWTH_RATE; // Match the tunnel growth rate
        private bool isInExistingTunnel = false;

        private void Awake()
        {
            waypointIndex = 0;
            waypointList = new List<Vector3>();
            nextWaypointList = new List<Vector3>();
            unitVectorDirection = Vector3.zero; // initially the worm is not moving
            transform.position = Tunnel.Manager.initialCell;
            direction = Direction.None;
        }

        private void OnEnable() 
        {
            PositionEvent += FindObjectOfType<Tunnel.Manager>().onPosition;
            CompleteTurnEvent += FindObjectOfType<Tunnel.Turn>().onCompleteTurn;
            CompleteTurnEvent += FindObjectOfType<Controller>().onCompleteTurn;
            CompleteTurnEvent += FindObjectOfType<Rotation>().onCompleteTurn;
        }

        public void onInitWormPosition(Vector3 initPos)
        {
            transform.position = initPos;
        }

        private void FixedUpdate()
        {
            if (waypointList.Count > 0) // iterate over waypoint list
            {
                Vector3 waypoint = waypointList[waypointIndex];

                transform.position = Vector3.MoveTowards(transform.position, waypoint, SPEED);

                if (transform.position.Equals(waypoint))
                {
                    waypointIndex += 1;

                    if (waypointIndex >= waypointList.Count) // when last waypoint has been reached, clear waypoints from list
                    {
                        completeTurn();
                    }
                }
            }
            else if (direction != Direction.None)
            {
                Vector3 unitVector = Dir.Vector.getUnitVectorFromDirection(direction);
                transform.position += unitVector * (float)Tunnel.Tunnel.SCALED_GROWTH_RATE; // position moves at the same rate as the straight tunnel
            }
        }

        /**
         * Finish a turn and check if any turns are queued up 
         */
        private void completeTurn()
        {
            waypointList = nextWaypointList;
            if (waypointList.Count == 0)
            {
                CompleteTurnEvent(egressWaypointDirection);
            }
            waypointIndex = 0;
            nextWaypointList.Clear();
        }


        /**
         * Ignore player input if waypoints list is being followed and first waypoint has not been reached yet
         */
        private bool isEligibleForInput()
        {
            return waypointList.Count == 0 || waypointIndex > 0;
        }

        /**
         * After player's input is validated (ie it is along a plane not already traveling in) then issue the move command
         *
         *@deltaPosition is the change in position
         */
        public void onMove(Direction direction)
        {
            this.direction = direction;
            Vector3 moveUnitVector = Dir.Vector.getUnitVectorFromDirection(direction);
            transform.position += moveUnitVector * SPEED;
        }

        /**
         * A TEST delegate method to center the worm after it has reached a decision point. In reality, this will be controlled
         * by the animation of the worm's head
         */
        public void onDecision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel)
        {
            print("re-center worm");
            if (tunnel != null)
            {
                transform.position = tunnel.center;
            }
            else
            {
                throw new System.Exception("no tunnel for worm to center in");
            }
        }

        /**
         * Follow waypoints when navigating corners. If already following waypoints, queue up the next waypoints list
         * 
         * @waypointList a list of coordinates the worm follows to navigate a corner
         */
        public void onFollowWaypoint(List<Vector3> waypointList, DirectionPair directionPair)
        {
            egressWaypointDirection = directionPair.curDir; // save the last egress directionPair

            if (this.waypointList.Count > 0)
            {
                nextWaypointList = waypointList;
            }
            else
            {
                this.waypointList = waypointList;
            }
        }

        /**
         * When junction has been created it triggers the worm to send out block interval events instead of the tunnel
         *
         *@cellMove info about the junction such as center position, ingress position
         */
        public void onCreateJunction(Tunnel.Tunnel collisionTunnel, DirectionPair dirPair, Tunnel.CellMove cellMove)
        {
            isInExistingTunnel = true;
            if (waypointList.Count == 0) // means we are not executing a turn into the junction
            {
                waypointList.Add(cellMove.startPosition); // go the ingress position of the junction
                egressWaypointDirection = direction; // direction remains same as the straight tunnel worm is currently in
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

        /**
         * Move the worm in a different direction and determine whether it has reached one of the six block decision points
         */
        public void onPlayerInput(Direction direction)
        {
            if (isEligibleForInput())
            {
                unitVectorDirection = Dir.Vector.getUnitVectorFromDirection(direction);
                Vector3 inputPosition = transform.position + unitVectorDirection * SPEED;
                PositionEvent(inputPosition, direction);
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Tunnel.Manager>())
            {
                PositionEvent -= FindObjectOfType<Tunnel.Manager>().onPosition;
            }
            if (FindObjectOfType<Tunnel.Turn>())
            {
                CompleteTurnEvent -= FindObjectOfType<Tunnel.Turn>().onCompleteTurn;                
            }
            if (FindObjectOfType<Controller>())
            {
                CompleteTurnEvent -= FindObjectOfType<Controller>().onCompleteTurn;
            }
            if (FindObjectOfType<Rotation>())
            {
                CompleteTurnEvent -= FindObjectOfType<Rotation>().onCompleteTurn;
            }
        }
    }
}
