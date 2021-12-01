using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Turn : MonoBehaviour
    {
        private bool isWaitBlockEvent; // if straight tunnel, then it is true because we have to wait until tunnel length is a multiple of BLOCK_INTERVAL
        private bool isDecision; // flag to check if a decision has been made
        private bool isBlockSizeMultiple;

        private List<Vector3> waypointList; // list of points to move to when a corner is created

        Direction curDirection; // direction of the current turn
        Direction prevDirection; // direction of the previous turn used for rotating tunnels

        public delegate void ChangeDirection(DirectionPair directionPair);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void FollowWaypoint(List<Vector3> waypointList, DirectionPair directionPair);
        public event FollowWaypoint FollowWaypointEvent;

        private void Awake()
        {
            this.isDecision = false;
            this.isWaitBlockEvent = false;
            this.isBlockSizeMultiple = true; // used to initialize straight tunnel
            prevDirection = curDirection = Direction.None;
            this.waypointList = new List<Vector3>();
        }

        private void OnEnable()
        {
            ChangeDirectionEvent += FindObjectOfType<Manager>().onChangeDirection;
            FollowWaypointEvent += FindObjectOfType<Worm.Movement>().onFollowWaypoint;
        }

        /**
         * Initialize waypoint list with two points: the center of the corner and the exit point
         * 
         * @directionPair, the ingress and egress direction of the worm
         */
        private void initializeTurnWaypointList(DirectionPair directionPair, Vector3 startWaypointPosition)
        {
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
            if (isTurning())
            {
                DirectionPair directionPair = new DirectionPair(prevDirection, curDirection);
                prevDirection = curDirection;
                curDirection = Direction.None;
                this.isDecision = false;
                ChangeDirectionEvent(directionPair); // rotate tunnel in the direction

                if (tunnel != null) // check the tunnel exists
                {
                    Vector3 egressPosition = tunnel.getEgressPosition(directionPair.prevDir);
                    initializeTurnWaypointList(directionPair, egressPosition);
                }
            }
        }

        /**
         * Once the worm has navigated a turn, time to signal creation of new blocks
         * 
         * @direction is the exit direction
         */
        public void onCompleteTurn(Direction direction)
        {
            DirectionPair straightDirection = new DirectionPair(direction, direction);
            ChangeDirectionEvent(straightDirection);
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
            this.isWaitBlockEvent = isWaitBlockEvent;
            this.curDirection = direction;
            print("decide to go in curDirection " + curDirection);
            turn(tunnel);
        }

        /**
         * Received when the active tunnel is a multiple of block size making it eligible for a direction change
         * 
         * @isBlockSizeMultiple did the straight tunnel reach a multiple of block size?
         * @tunnel The tunnel the decision to change direction is made from
         */
        public void onBlockInterval(bool isBlockSizeMultiple, Vector3 blockPosition, Tunnel tunnel)
        {
            this.isBlockSizeMultiple = isBlockSizeMultiple;

            if (isBlockSizeMultiple) // initiate turn for straight tunnels
            {
                turn(tunnel);
            }
        }

        /**
         * Check if tunnel meets conditions to be eligible for a turn
         */
        public bool isTurning()
        {
            if (isDecision)
            {
                if (isWaitBlockEvent) // certain tunnels (eg straight) require certain length to turn
                {
                    return isBlockSizeMultiple;
                }
                else
                {
                    return true;
                }
            }
            return false;            
        }

        private void OnDisable()
        {
            ChangeDirectionEvent -= FindObjectOfType<Manager>().onChangeDirection;
            FollowWaypointEvent -= FindObjectOfType<Worm.Movement>().onFollowWaypoint;
        }
    }

}