using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Turn : GenericSingletonClass<Turn>
    {
        private bool isWaitBlockEvent; // if straight tunnel, then it is true because we have to wait until tunnel length is a multiple of BLOCK_INTERVAL
        private bool isDecision; // flag to check if a decision has been made
        private bool isBlockSizeMultiple;

        private List<Vector3> waypointList; // list of points to move to when a corner is created

        DirectionPair directionPair;

        public delegate void ChangeDirection(DirectionPair directionPair);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void FollowWaypoint(List<Vector3> waypointList, DirectionPair directionPair);
        public event FollowWaypoint FollowWaypointEvent;

        private new void Awake()
        {
            base.Awake();
            this.isDecision = false;
            this.isWaitBlockEvent = false;
            this.isBlockSizeMultiple = true; // used to initialize straight tunnel
            directionPair = new DirectionPair(Direction.None, Direction.None);
            this.waypointList = new List<Vector3>();
        }

        private void OnEnable()
        {
            ChangeDirectionEvent += CollisionManager.Instance.onChangeDirection;
            FollowWaypointEvent += FindObjectOfType<Worm.Movement>().onFollowWaypoint;
        }

        /**
         * Initialize waypoint list with two points: the center of the corner and the exit point
         * 
         * @directionPair, the ingress and egress direction of the worm
         */
        private void initializeTurnWaypointList(DirectionPair directionPair, Vector3 startWaypointPosition)
        {
            print("init waypoint list prev dir " + directionPair.prevDir + " cur dir " + directionPair.curDir);
            Vector3 centerWaypointOffset = Dir.Vector.getUnitVectorFromDirection(directionPair.prevDir) * Tunnel.CENTER_OFFSET;
            Vector3 ExitWaypointOffset = Dir.Vector.getUnitVectorFromDirection(directionPair.curDir) * Tunnel.CENTER_OFFSET;

            Vector3 centerWaypoint = startWaypointPosition + centerWaypointOffset;
            Vector3 exitWaypoint = centerWaypoint + ExitWaypointOffset;

            waypointList = new List<Vector3> { centerWaypoint, exitWaypoint };
            FollowWaypointEvent(waypointList, directionPair);
        }

        /**
         * Initiate the turn if the tunnel is eligible
         */
        private void turn(Tunnel tunnel)
        {
            isDecision = false;
            ChangeDirectionEvent(directionPair); // rotate tunnel in the direction
            Vector3 egressPosition = Tunnel.getEgressPosition(directionPair.prevDir, tunnel.center);
            initializeTurnWaypointList(directionPair, egressPosition);
        }

        /**
         * Once the worm has navigated a turn, time to signal creation of new blocks
         * 
         * @direction is the exit direction
         */
        public void onCompleteTurn(Direction direction)
        {
            DirectionPair straightDirectionPair = new DirectionPair(direction, direction);
            ChangeDirectionEvent(straightDirectionPair);
        }

        /**
         * Setup the initial direciton
         */
        public void onInitDecision(Direction direction)
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
        public void onDecision(bool isWaitBlockEvent, Direction direction, Tunnel tunnel)
        {
            this.isDecision = true;
            directionPair.prevDir = directionPair.curDir;
            directionPair.curDir = direction;
            print("decide to go in curDirection " + direction);
        }

        /**
         * Received when the active tunnel is a multiple of block size making it eligible for a direction change
         * 
         * @isBlockSizeMultiple did the straight tunnel reach a multiple of block size?
         * @tunnel The tunnel the decision to change direction is made from
         */
        public void onBlockInterval(bool isBlockSizeMultiple, Vector3Int blockPosition, Tunnel tunnel)
        {
            this.isBlockSizeMultiple = isBlockSizeMultiple;

            if (isTurning()) // initiate turn for straight tunnels
            {
                turn(tunnel);
            }
        }

        /**
         * Check if tunnel meets conditions to be eligible for a turn
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
            if (FindObjectOfType<Worm.Movement>())
            {
                ChangeDirectionEvent -= FindObjectOfType<CollisionManager>().onChangeDirection;
                FollowWaypointEvent -= FindObjectOfType<Worm.Movement>().onFollowWaypoint;
            }            
        }
    }

}