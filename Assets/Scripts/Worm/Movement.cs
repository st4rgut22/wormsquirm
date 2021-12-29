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

        public delegate void Decision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel);
        public event Decision DecisionEvent;

        private const float SPEED = Tunnel.Tunnel.SCALED_GROWTH_RATE; // Match the tunnel growth rate

        Tunnel.Tunnel changeDirTunnel;
        private bool isInit = true;

        private void Awake()
        {
            waypointIndex = 0;
            changeDirTunnel = null;
            waypointList = new List<Vector3>();
            nextWaypointList = new List<Vector3>();
            unitVectorDirection = Vector3.zero; // initially the worm is not moving
            transform.position = Tunnel.Manager.initialCell;
            direction = Direction.None;
        }

        private void OnEnable() 
        {
            CompleteTurnEvent += FindObjectOfType<Tunnel.Turn>().onCompleteTurn;
            CompleteTurnEvent += FindObjectOfType<Controller>().onCompleteTurn;
            CompleteTurnEvent += FindObjectOfType<Rotation>().onCompleteTurn;
            DecisionEvent += FindObjectOfType<Tunnel.Turn>().onDecision;
            DecisionEvent += FindObjectOfType<Tunnel.Map>().onDecision;
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
            direction = egressWaypointDirection;
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
                nextWaypointList = waypointList;
            }
            else
            {
                this.waypointList = waypointList;
            }
        }

        /**
         * Tunnel direction change triggers creation or modification of the next tunnel. 
         * 
         * @directionPair indicates direction of travel and determines type of tunnel to create
         */
        public void onChangeDirection(DirectionPair directionPair)
        {
            Tunnel.CollisionManager.Instance.changeDirection(directionPair, changeDirTunnel);
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

        /**
         * Move the worm in a different direction and determine whether it has reached one of the six block decision points
         */
        public void onPlayerInput(Direction turnDirection)
        {
            if (isEligibleForInput())
            {
                unitVectorDirection = Dir.Vector.getUnitVectorFromDirection(turnDirection);
                Vector3 inputPosition = transform.position + unitVectorDirection * SPEED;

                Vector3Int cell = inputPosition.getNextVector3(direction);
                print("input position is " + inputPosition + " next cell (HEAD) is " + cell);
                changeDirTunnel = Tunnel.Map.getTunnelFromDict(cell);
                bool isDecision = Tunnel.ActionPoint.instance.isDecisionBoundaryCrossed(changeDirTunnel, inputPosition, turnDirection);

                if (isDecision)
                {
                    if (changeDirTunnel != null)
                    {
                        transform.position = changeDirTunnel.center; // center the worm after making a decision
                    }
                    bool isStraightTunnel = changeDirTunnel == null || changeDirTunnel.type == Tunnel.Type.Name.STRAIGHT; // if this is the first tunnel it should be straight type
                    DecisionEvent(isStraightTunnel, turnDirection, changeDirTunnel);
                }
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Tunnel.Map>())
            {
                DecisionEvent -= FindObjectOfType<Tunnel.Map>().onDecision;
            }
            if (FindObjectOfType<Tunnel.Turn>())
            {
                DecisionEvent -= FindObjectOfType<Tunnel.Turn>().onDecision;
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
