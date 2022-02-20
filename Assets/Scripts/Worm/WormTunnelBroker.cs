using UnityEngine;

namespace Worm
{
    public class WormTunnelBroker : WormBody
    {
        public delegate void WormInterval(Tunnel.Tunnel tunnel, Vector3 tunnelCenter);
        public event WormInterval WormIntervalEvent;

        public delegate void CollideTunnel(DirectionPair directionPair, Tunnel.Tunnel curTunnel, Tunnel.Tunnel nextTunnel, Vector3Int collisionCell, bool isTunnelNew);
        public event CollideTunnel CollideTunnelEvent;

        public delegate void Grow(Vector3 ringPosition);
        public event Grow GrowEvent;

        public delegate void Move(Vector3 ringPosition, Direction direction);
        public event Move MoveEvent;

        private Vector3Int cell; // current cell the worm is in (using clit.position for measurement)

        private bool isDecisionProcessing;

        Tunnel.Straight prevStraightTunnel;

        private void OnEnable()
        {
            WormIntervalEvent += GetComponent<Turn>().onWormTurnInterval;
            CollideTunnelEvent += Tunnel.CollisionManager.Instance.onCollide;
        }

        // Start is called before the first frame update
        private new void Awake()
        {
            base.Awake();

            cell = Vector3Int.zero;

            wormBase.setIsCreatingTunnel(true);
            prevStraightTunnel = null;
            isDecisionProcessing = false;
        }

        private void Update()
        {
            if (!wormBase.isCreatingTunnel && !isDecisionProcessing)
            {
                Vector3Int curCell = Tunnel.Map.getCellPos(ring.position);
                Tunnel.Tunnel curTunnel = Tunnel.Map.getCurrentTunnel(ring.position);

                if (!curCell.Equals(cell)) // entered a new cell, check if new tunnel needs to be modified based off worm direction
                {
                    print("current cell is " + curCell + " ring position is " + ring.position);

                    if (!wormBase.isChangingDirection) // if no decision to turn is made, then check if the current tunnel aligns with the worm's forward direction
                    {    // for example if going straight in a corner cell it would not align and a junction must be created
                        print("no turn decision has been made check if collision is occuring in direction " + wormBase.direction + " of cell " + curCell);
                        DirectionPair straightDirectionPair = new DirectionPair(wormBase.direction, wormBase.direction);
                        curTunnel.setWormCreatorId(wormId);
                        print("collide with tunnel " + curTunnel.name + " at cell " + curCell);
                        CollideTunnelEvent(straightDirectionPair, curTunnel, curTunnel, curCell, false);
                    }
                }
                cell = curCell;
            }
        }

        /**
         * Decision must be processed before worm direction is set. To get the most up-to-date direction, wait for decision to be processed before determining worm position
         */
        public void onDecisionProcessing(bool isDecisionProcessing, Waypoint waypoint)
        {
            this.isDecisionProcessing = isDecisionProcessing;
        }

        /**
         * Issue an event that grows the current tunnel
         */
        private void FixedUpdate()
        {
            if (GrowEvent != null)
            {
                GrowEvent(ring.position);
            }
        }

        /**
         * Listen for when a tunnel is added to unregister and register the GrowEvent listener
         * 
         * @tunnel      the newly added tunnel
         */
        public void onAddTunnel(Tunnel.Tunnel tunnel, DirectionPair directionPair)
        {

            bool isTunnelStraight = Tunnel.Type.isTypeStraight(tunnel.type);
            bool isTunnelJunction = Tunnel.Type.isTypeJunction(tunnel.type);

            // unregister the previous tunnel's grow event, and register the new one
            if (prevStraightTunnel != null)
            {
                GrowEvent -= prevStraightTunnel.onGrow;
                prevStraightTunnel = null;
            }
            if (isTunnelJunction)// if the tunnel is not straight type but it is a junction that worm can travel straight through
            {
                if (directionPair.isStraight())
                {
                    GetComponent<Movement>().goStraightThroughJunction(tunnel); // create junction here 
                }
                if (wormBase.isCreatingTunnel) // worm enters pre-existing tunnel from a new tunnel
                {
                    cell = Tunnel.Map.getCellPos(ring.position); // setting the cell position will prevent collision from repeating when entering an existing tunnel
                    wormBase.setIsCreatingTunnel(false); // enter a pre-existing tunnel
                }
            }
            else
            {
                if (isTunnelStraight)
                {
                    prevStraightTunnel = (Tunnel.Straight)tunnel;
                    GrowEvent += prevStraightTunnel.onGrow;
                }
                wormBase.setIsCreatingTunnel(true); // create a corner or straight tunnel
            }
        }

        private void OnDisable()
        {
            if (prevStraightTunnel != null)
            {
                GrowEvent -= prevStraightTunnel.onGrow;
            }
            if (Tunnel.CollisionManager.Instance)
            {
                CollideTunnelEvent -= Tunnel.CollisionManager.Instance.onCollide;
            }
            if (GetComponent<Turn>())
            {
                WormIntervalEvent -= GetComponent<Turn>().onWormTurnInterval;
            }
        }
    }
}