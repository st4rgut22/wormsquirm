using UnityEngine;

namespace Worm
{
    public class WormTunnelBroker : WormBody
    {
        public delegate void WormInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel);
        public event WormInterval WormIntervalEvent;

        public delegate void CollideTunnel(DirectionPair directionPair, Tunnel.Tunnel curTunnel, Tunnel.Tunnel nextTunnel, Vector3Int collisionCell, bool isTunnelNew);
        public event CollideTunnel CollideTunnelEvent;

        public delegate void Grow(Vector3 ringPosition);
        public event Grow GrowEvent;

        public delegate void Move(Vector3 ringPosition, Direction direction);
        public event Move MoveEvent;

        private Vector3Int cell;                // current cell the worm is in (using clit.position for measurement)
        private Vector3Int enterExistingCell;   // cell that is the entry to the existing tunnel, while worm is in this cell DONT update current tunnel because it will replace junction with previous tunnel

        private bool isDecisionProcessing;

        Tunnel.Straight prevStraightTunnel;

        protected void OnEnable()
        {            
            CollideTunnelEvent += Tunnel.CollisionManager.Instance.onCollide;
            WormIntervalEvent += GetComponent<InputProcessor>().onWormInterval;
            WormIntervalEvent += GetComponent<Turn>().onBlockInterval;
        }

        // Start is called before the first frame update
        protected new void Awake()
        {
            base.Awake();

            cell = Vector3Int.zero;
            enterExistingCell = cell;

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

                bool isNewBlock = !curCell.Equals(cell);

                if (!curCell.Equals(enterExistingCell))
                {
                    sendWormIntervalEvent(isNewBlock, curCell, cell, curTunnel); // initialize a turn etc before issuing collision event
                }

                if (isNewBlock) // entered a new cell, check if new tunnel needs to be modified based off worm direction
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
                    else // do not advance to next checkpoint during a turn.  
                    {
                        print("what happens?");
                    }
                }
                if (!curTunnel.containsCell(curCell))
                {
                    throw new System.Exception("current tunnel " + curTunnel.name + " does not contain cell " + curCell + " even though it is part of the tunnel");
                }
                cell = curCell;
            }
        }

        protected void RaiseWormIntervalEvent(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel)
        {
            WormIntervalEvent(isBlockInterval, blockPositionInt, lastBlockPositionInt, tunnel); // dont raise this event if block interval is the last 
        }

        /* Like tunnel's blockInterval event, worm should also signal when it has reached a block so other components can keep track of its position
        * in an existing tunnel.The event emitted differs between human-controlled and AI-controlled worms so it is marked virtual
        */
        protected virtual void sendWormIntervalEvent(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel)
        {
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
        protected void FixedUpdate()
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
                    enterExistingCell = cell;
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

        protected void OnDisable()
        {
            if (prevStraightTunnel != null)
            {
                GrowEvent -= prevStraightTunnel.onGrow;
            }
            if (Tunnel.CollisionManager.Instance)
            {
                CollideTunnelEvent -= Tunnel.CollisionManager.Instance.onCollide;
            }
            if (GetComponent<InputProcessor>())
            {
                WormIntervalEvent -= GetComponent<InputProcessor>().onWormInterval;
            }
            if (GetComponent<Turn>())
            {
                WormIntervalEvent -= GetComponent<Turn>().onBlockInterval;
            }
        }
    }
}