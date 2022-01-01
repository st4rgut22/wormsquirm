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

        private Vector3 unitVectorDirection;
        private int waypointIndex;

        private List<Vector3> waypointList;
        private List<Vector3> nextWaypointList; // queued up waypoint list if turning while navigating a corner

        public delegate void CompleteTurn(Direction direction); // when turn is completed notify Turn so we can proceed straight
        public event CompleteTurn CompleteTurnEvent;

        public delegate void BlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel);
        public event BlockInterval BlockIntervalEvent;

        public delegate void DecisionProcessing(bool isDecisionProcessing);
        public event DecisionProcessing DecisionProcessingEvent;

        private const float SPEED = Tunnel.Tunnel.SCALED_GROWTH_RATE; // Match the tunnel growth rate

        private void Awake()
        {
            waypointIndex = 0;
            waypointList = new List<Vector3>();
            nextWaypointList = new List<Vector3>();
            unitVectorDirection = Vector3.zero; // initially the worm is not moving
            transform.position = Tunnel.TunnelManager.Instance.initialCell;
            direction = Direction.None;
        }

        private void OnEnable() 
        {
            CompleteTurnEvent += Tunnel.Turn.Instance.onCompleteTurn;
            CompleteTurnEvent += FindObjectOfType<Controller>().onCompleteTurn;
            CompleteTurnEvent += FindObjectOfType<Rotation>().onCompleteTurn;
            DecisionProcessingEvent += FindObjectOfType<InputProcessor>().onDecisionProcessing;
        }

        public void onInitWormPosition(Vector3 initPos, Direction direction)
        {
            float offset = Tunnel.TunnelManager.Instance.getHeadOffset();
            Vector3 offsetVector = Dir.Vector.getUnitVectorFromDirection(direction);
            transform.position = initPos + offset * offsetVector;
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
            print("complete turn");
            waypointList = nextWaypointList;
            if (waypointList.Count == 0)
            {

                CompleteTurnEvent(egressWaypointDirection);
                DecisionProcessingEvent(false);
            }
            waypointIndex = 0;
            nextWaypointList.Clear();
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
         * Follow waypoints when navigating corners. If already following waypoints, queue up the next waypoints list
         * 
         * @waypointList a list of coordinates the worm follows to navigate a corner
         */
        public void onFollowWaypoint(List<Vector3> waypointList, DirectionPair directionPair)
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
         * When junction has been created it triggers the worm to send out block interval events instead of the tunnel
         *
         *@cellMove info about the junction such as center position, ingress position
         */
        public void onCreateJunction(Tunnel.Tunnel collisionTunnel, DirectionPair dirPair, Tunnel.CellMove cellMove)
        {
            Tunnel.CollisionManager.Instance.isCreatingNewTunnel = true;

            if (waypointList.Count == 0) // means we are not executing a turn into the junction
            {
                waypointList.Add(cellMove.startPosition); // go the ingress position of the junction
                egressWaypointDirection = dirPair.curDir; // direction remains same as the straight tunnel worm is currently in
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
            if (FindObjectOfType<InputProcessor>())
            {
                DecisionProcessingEvent -= FindObjectOfType<InputProcessor>().onDecisionProcessing;
            }
        }
    }
}
