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

        private Vector3 unitVectorDirection;
        private int waypointIndex;

        private List<Vector3> waypointList;
        private List<Vector3> nextWaypointList; // queued up waypoint list if turning while navigating a corner

        public delegate void Position(Vector3 position, Direction direction);
        public event Position PositionEvent;

        public delegate void CompleteTurn(Direction direction); // when turn is completed notify Turn so we can proceed straight
        public event CompleteTurn CompleteTurnEvent;

        private const float SPEED = .05f; // Match the tunnel growth rate

        private void Awake()
        {
            waypointIndex = 0;
            waypointList = new List<Vector3>();
            nextWaypointList = new List<Vector3>();
            unitVectorDirection = Vector3.zero; // initially the worm is not moving
        }

        private void OnEnable()
        {
            PositionEvent += FindObjectOfType<Tunnel.Manager>().onPosition;
            CompleteTurnEvent += FindObjectOfType<Tunnel.Turn>().onCompleteTurn;
        }

        private void Update()
        {
            if (waypointList.Count > 0) // iterate over waypoint list
            {
                Vector3 waypoint = waypointList[waypointIndex];

                transform.position = Vector3.MoveTowards(transform.position, waypoint, SPEED);

                if (transform.position.Equals(waypoint))
                {
                    print("reached waypoint " + waypoint);
                    waypointIndex += 1;

                    if (waypointIndex >= waypointList.Count) // when last waypoint has been reached, clear waypoints from list
                    {
                        completeTurn();
                    }
                }
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
                CompleteTurnEvent(direction);
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
            transform.position += moveUnitVector * Tunnel.Tunnel.SCALED_GROWTH_RATE;
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
        }

        /**
         * When a new tunnel is added that is not straight, the worm should redirect to follow path of the tunnel
         * 
         * @tunnel the added tunnel
         * @cell the cell position of the added tunnel
         * @directionPair the ingress and egress hole of the tunnel the worm enters and exits from
         */
        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair)
        {
            print("on add tunnel");
        }

        /**
         * Follow waypoints when navigating corners. If already following waypoints, queue up the next waypoints list
         * 
         * @waypointList a list of coordinates the worm follows to navigate a corner
         */
        public void onFollowWaypoint(List<Vector3> waypointList, DirectionPair directionPair)
        {
            direction = directionPair.curDir; // save the last egress direction 

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
         * When a straight tunnel is extended, the worm should follow in the same direction
         * 
         * @tunnel the tunnel that has been extended
         */
        public void onBlockInterval(bool isBlockInterval, Vector3 blockPosition, Tunnel.Straight tunnel)
        {
            Vector3 unitVector = Dir.Vector.getUnitVectorFromDirection(tunnel.ingressDirection);
            transform.position += unitVector * (float) Tunnel.Tunnel.SCALED_GROWTH_RATE; // set position a little behind the tunnel head
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
                print("propose moving worm to " + inputPosition);

                PositionEvent(inputPosition, direction);
            }
        }

        private void OnDisable()
        {
            PositionEvent -= FindObjectOfType<Tunnel.Manager>().onPosition;
            CompleteTurnEvent -= FindObjectOfType<Tunnel.Turn>().onCompleteTurn;
        }
    }
}
