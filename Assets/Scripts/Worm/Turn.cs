using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class Turn : GenericSingletonClass<Turn>
    {
        private bool isWaitBlockEvent; // if straight tunnel, then it is true because we have to wait until tunnel length is a multiple of BLOCK_INTERVAL
        private bool isDecision; // flag to check if a decision has been made
        private bool isBlockSizeMultiple;

        private List<Waypoint> waypointList; // list of points to move to when a corner is created

        DirectionPair directionPair;

        public delegate void ChangeDirection(DirectionPair directionPair);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void FollowWaypoint(List<Waypoint> waypointList, DirectionPair directionPair);
        public event FollowWaypoint FollowWaypointEvent;

        private new void Awake()
        {
            base.Awake();
            isDecision = false;
            isWaitBlockEvent = false;
            isBlockSizeMultiple = true; // used to initialize straight tunnel
            directionPair = new DirectionPair(Direction.None, Direction.None);
            waypointList = new List<Waypoint>();
        }

        private void OnEnable()
        {
            ChangeDirectionEvent += Tunnel.CollisionManager.Instance.onChangeDirection;
            FollowWaypointEvent += FindObjectOfType<Movement>().onFollowWaypoint;
        }

        private Vector3 getOffsetPosition(Vector3 position, Direction direction, float offset)
        {
            Vector3 offsetVector = Dir.Vector.getUnitVectorFromDirection(direction) * offset;
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
            Vector3 exitWaypointPosition = getOffsetPosition(centerWaypointPosition, directionPair.curDir, Tunnel.Tunnel.CENTER_OFFSET);
            Vector3 offsetHeadWaypoint = getOffsetPosition(exitWaypointPosition, directionPair.curDir, Tunnel.TunnelManager.Instance.WORM_OFFSET);

            Waypoint startWP = new Waypoint(startWaypointPosition, MoveType.ENTRANCE);
            Waypoint cellWP = new Waypoint(centerWaypointPosition, MoveType.CENTER);
            Waypoint exitWP = new Waypoint(exitWaypointPosition, MoveType.EXIT);
            Waypoint offsetWP = new Waypoint(offsetHeadWaypoint, MoveType.OFFSET);
            waypointList = new List<Waypoint> { startWP, cellWP, exitWP };
            FollowWaypointEvent(waypointList, directionPair);
        }

        /**
         * Instruct worm to enter a junction
         *
         *@cellMove info about the junction such as center position, ingress position
         */
        public void onCreateJunction(Tunnel.Tunnel collisionTunnel, DirectionPair dirPair, Tunnel.CellMove cellMove)
        {
            Tunnel.CollisionManager.Instance.isCreatingNewTunnel = true;

            if (dirPair.isStraight()) // means we are not executing a turn into the junction, let initializeTurnWPList handle turns
            {
                float offset = Tunnel.Tunnel.BLOCK_SIZE + Tunnel.TunnelManager.Instance.WORM_OFFSET;
                Vector3 offsetPos = getOffsetPosition(cellMove.startPosition, dirPair.curDir, offset);
                Vector3 exitPos = getOffsetPosition(cellMove.startPosition, dirPair.curDir, Tunnel.Tunnel.BLOCK_SIZE);

                Waypoint exitJctWP = new Waypoint(exitPos, MoveType.EXIT);
                Waypoint offsetJctWP = new Waypoint(offsetPos, MoveType.OFFSET);

                waypointList = new List<Waypoint>() { exitJctWP }; // go the ingress position of the junction
                FollowWaypointEvent(waypointList, dirPair);
            }
        }

        /**
         * Initiate the turn if the tunnel is eligible
         */
        private void turn(Tunnel.Tunnel tunnel)
        {
            isDecision = false;
            ChangeDirectionEvent(directionPair); // rotate tunnel in the direction
            Vector3 egressPosition = Tunnel.Tunnel.getEgressPosition(directionPair.prevDir, tunnel.center);
            initializeTurnWaypointList(directionPair, egressPosition);
        }

        /**
         * Once the worm has finished navigating out of a turn
         * 
         * @direction is the exit direction
         */
        public void onCompleteTurn(Direction direction)
        {
            DirectionPair straightDirectionPair = new DirectionPair(direction, direction);
            ChangeDirectionEvent(straightDirectionPair);
            Vector3 offsetHeadWaypoint = getOffsetPosition(transform.position, direction, Tunnel.TunnelManager.Instance.WORM_OFFSET);
            Waypoint offsetWP = new Waypoint(offsetHeadWaypoint, MoveType.OFFSET);
            List<Waypoint> offsetWaypointList = new List<Waypoint>() { offsetWP };
            FollowWaypointEvent(offsetWaypointList, straightDirectionPair);
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
        public void onDecision(bool isWaitBlockEvent, Direction direction, Tunnel.Tunnel tunnel)
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
        public void onBlockInterval(bool isBlockSizeMultiple, Vector3Int blockPosition, Tunnel.Tunnel tunnel)
        {
            this.isBlockSizeMultiple = isBlockSizeMultiple;

            if (isTurning()) // initiate turn for straight tunnels
            {
                turn(tunnel);
            }
        }

        /**
         * TODO: Turn when decision is made WITHIN a non-straight tunnel
         * need to figure out the decision window for these consecutive turns maybe in Controller/InputProcessor
         */
        public void onTurn()
        {

        }

        /**
         * Check if tunnel meets conditions to be eligible for a turn from a straight tunnel
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
            if (Tunnel.CollisionManager.Instance)
            {
                ChangeDirectionEvent -= Tunnel.CollisionManager.Instance.onChangeDirection;
            }
            if (FindObjectOfType<Movement>())
            {
                FollowWaypointEvent -= FindObjectOfType<Movement>().onFollowWaypoint;
            }
        }
    }

}