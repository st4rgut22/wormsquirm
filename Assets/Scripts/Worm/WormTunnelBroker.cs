using UnityEngine;

namespace Worm
{
    public class WormTunnelBroker : WormBody
    {
        public delegate void AbortTurn();
        public event AbortTurn AbortTurnEvent;

        public delegate void WormInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel);
        public event WormInterval WormIntervalEvent;

        public delegate void CollideTunnel(DirectionPair directionPair, Tunnel.Tunnel curTunnel, Tunnel.Tunnel nextTunnel, Vector3Int collisionCell, bool isTunnelNew);
        public event CollideTunnel CollideTunnelEvent;

        public delegate void Grow(Vector3 ringPosition);
        public event Grow GrowEvent;

        public delegate void Move(Vector3 ringPosition, Direction direction);
        public event Move MoveEvent;

        public Vector3Int turnCell { get; private set; }            // 
        public Vector3Int cell { get; private set; }                // current cell the worm is in (using clit.position for measurement)
        private Vector3Int enterExistingCell;   // cell that is the entry to the existing tunnel, while worm is in this cell DONT update current tunnel because it will replace junction with previous tunnel

        protected Waypoint lastReachedWaypoint; // the last waypoint the player has reached. It is reset after a check

        private bool isDecisionProcessing;

        Tunnel.Straight prevStraightTunnel;
        protected Tunnel.Tunnel prevTunnel;               // the previous tunnel, useful for checking when is the start of a new straight segment

        public bool isTunnelCreated { get; private set; }       // whether the worm has created a tunnel yet
        
        protected new void OnEnable()
        {
            base.OnEnable();
            CollideTunnelEvent += Tunnel.CollisionManager.Instance.onCollide;
            WormIntervalEvent += GetComponent<InputProcessor>().onWormInterval;
            WormIntervalEvent += GetComponent<Turn>().onBlockInterval;
            WormIntervalEvent += Map.SpawnGenerator.onWormInterval;
            AbortTurnEvent += GetComponent<Turn>().onAbortDecision;
        }

        // Start is called before the first frame update
        protected new void Awake()
        {
            base.Awake();

            cell = wormBase.defaultCell;
            enterExistingCell = cell;

            wormBase.setIsCreatingTunnel(true);
            prevStraightTunnel = null;
            isDecisionProcessing = false;
            isTunnelCreated = false;
        }

        /**
         * Get the current cell depending on whether the worm is starting a path or already following a path
         */
        public Vector3Int getCurrentCell()
        {
            Vector3Int curCell = cell.Equals(wormBase.defaultCell) ? wormBase.initialCell : cell;
            return curCell;
        }

        /**
         * Get the next cell using the worm's current cell and current direction
         */
        public Vector3Int getNextCell()
        {
            if (wormBase.isChangingDirection)
            {                
                Vector3Int nextCell = Dir.Vector.getNextCellFromDirection(turnCell, wormBase.turnDirection);
                print("is changing direction in " + turnCell + " next cell is " + nextCell);
                return nextCell;
            }
            else // if is pending turn (in which case the turn will be aborted) or going straight, just get the next cell in the current direction
            {
                Vector3Int curCell;
                if (isTunnelCreated)
                {
                    Tunnel.Tunnel curTunnel = Tunnel.TunnelMap.getCurrentTunnel(ring.position); // use ring.position to check upcoming cell
                    curCell = curTunnel.getLastCellPosition(); // use the last added cell of the tunnel as position because blocks are added ahead of worm cell
                    print("getting the current cell using the current tunnel for worm " + gameObject.name + " which is " + curTunnel.name + " at loc " + curCell);
                }
                else
                {
                    curCell = getCurrentCell(); // use the worm's position to get the current cell
                    print("getting next current using worm's position in existing tunnel for worm " + gameObject.name + " which is loc " + curCell);
                }
                Vector3Int nextCell = Dir.Vector.getNextCellFromDirection(curCell, wormBase.direction);
                print("going straight in dir " + wormBase.direction + " next cell is " + nextCell);
                return nextCell;
            }
        }

        /**
         * Listener is fired when worm enters an existing tunnel, for example by colliding with another tunnel
         */
        public void onEnterExistingTunnel()
        {
            wormBase.setIsCreatingTunnel(false);
        }

        /**
         * Set the cell the turn is happening in
         */
        public void onSetTurnCell(Vector3Int turnCell)
        {
            print("set turn cell which is " + turnCell);
            this.turnCell = turnCell;
        }

        /**
         * Listener is fired when player has reached a waypoint (usually along path of a turn)
         * 
         * @waypoint        the waypoint that was reached
         */
        public void onReachWaypoint(Waypoint waypoint)
        {
            if (waypoint.move == MoveType.CENTER)       // get the dirPair from the center waypoint to avoid any timing issues at block start
            {
                lastReachedWaypoint = waypoint;
            }
        }

        /**
         * Abort the next turn
         */
        public void RaiseAbortTurnEvent()
        {
            AbortTurnEvent();
        }

        /**
         * Determine whether the current cell should trigger the wormInterval event
         */
        protected virtual bool isSendWormIntervalEvent(Vector3Int curCell, Tunnel.Tunnel curTunnel, bool isNewBlock)
        {
            return !curCell.Equals(enterExistingCell);
        }

        /**
         * Get the current cell of the worm
         * 
         * @position    worm's midsegment position
         */
        public static Vector3Int getCurrentCell(Vector3 position)
        {
            return Tunnel.TunnelMap.getCellPos(position);
        }

        public Direction getDirection()
        {
            return wormBase.direction;
        }

        private void Update()
        {
            if (!isTunnelCreated)
            {
                Tunnel.Tunnel curTunnel = Tunnel.TunnelMap.getCurrentTunnel(wormBase.initialCell);
                if (curTunnel != null)
                {
                    isTunnelCreated = true;
                }
            }

            Vector3Int curCell = getCurrentCell(ring.position);

            if (GrowEvent != null)
            {
                GrowEvent(ring.position);
            }
            if (!wormBase.isCreatingTunnel && !isDecisionProcessing)
            {

                Tunnel.Tunnel curTunnel = Tunnel.TunnelMap.getCurrentTunnel(ring.position); // use ring.position to check upcoming cell
                if (curTunnel == null) // if no tunnel exists at ring position, we are not in an existing tunnel (need to confirm)
                {
                    return;
                }

                bool isNewBlock = !curCell.Equals(cell);

                if (isSendWormIntervalEvent(curCell, curTunnel, isNewBlock))    // avoid sending interval event in some scenarios
                {
                    WormIntervalEvent(isNewBlock, curCell, cell, curTunnel); // initialize a turn etc before issuing collision event
                }
                 
                if (isNewBlock && !wormBase.isChangingDirection) // entered a new cell, check if new tunnel needs to be modified based off worm direction
                {
                    collideOnStraightDir(curCell, curTunnel);
                }
                curTunnel = Tunnel.TunnelMap.getCurrentTunnel(ring.position); // update the current tunnel if it has been changed
                if (!curTunnel.containsCell(curCell))
                {
                    throw new System.Exception("current tunnel " + curTunnel.name + " does not contain cell " + curCell + " even though it is part of the tunnel");
                }
                prevTunnel = curTunnel;
            }
            if (isTunnelCreated)
            {
                cell = curCell; // dont update cell until tunnel has been created
            }
        }

        /**
         * if no decision to turn is made, then check if the current tunnel aligns with the worm's forward direction
         * for example if going straight in a corner cell it would not align and a junction must be created
         * the method does not necessarily mean the player will collide, it is just a check
         */
        private void collideOnStraightDir(Vector3Int curCell, Tunnel.Tunnel curTunnel)
        {
            print("current cell is " + curCell + " ring position is " + ring.position);
            print("no turn decision has been made check if collision is occuring in direction " + wormBase.direction + " of cell " + curCell);
            DirectionPair straightDirectionPair = new DirectionPair(wormBase.direction, wormBase.direction);
            curTunnel.setWormCreatorId(wormId);
            print("collide with tunnel " + curTunnel.name + " at cell " + curCell);
            CollideTunnelEvent(straightDirectionPair, curTunnel, curTunnel, curCell, false);
        }

        /**
         * Decision must be processed before worm direction is set. To get the most up-to-date direction, wait for decision to be processed before determining worm position
         */
        public void onDecisionProcessing(bool isDecisionProcessing, Waypoint waypoint)
        {
            this.isDecisionProcessing = isDecisionProcessing;
        }

        /**
         * Listen for when a tunnel is added to unregister and register the GrowEvent listener
         * 
         * @tunnel      the newly added tunnel
         */
        public void onAddTunnel(Tunnel.Tunnel tunnel, Tunnel.CellMove cellMove, DirectionPair directionPair, string wormId)
        {
            bool isTunnelStraight = Tunnel.Type.isTypeStraight(tunnel.type);
            bool isTunnelJunction = Tunnel.Type.isTypeJunction(tunnel.type);

            //isTunnelCreated = true;
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
                    cell = Tunnel.TunnelMap.getCellPos(ring.position); // setting the cell position will prevent collision from repeating when entering an existing tunnel
                    enterExistingCell = cell;
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

        protected new void OnDisable()
        {
            base.OnDisable();
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
                AbortTurnEvent -= GetComponent<Turn>().onAbortDecision;
                WormIntervalEvent -= GetComponent<Turn>().onBlockInterval;
            }
            if (FindObjectOfType<Map.SpawnGenerator>())
            {
                WormIntervalEvent -= Map.SpawnGenerator.onWormInterval;
            }
        }
    }
}