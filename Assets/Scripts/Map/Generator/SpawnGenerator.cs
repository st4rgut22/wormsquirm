using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public abstract class SpawnGenerator : ObstacleGenerator
    {
        public delegate void ChangeDirection(DirectionPair directionPair, Tunnel.Tunnel tunnel, string wormId, Tunnel.CellMove cellMove, bool isCreatingTunnel);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void RemoveWorm(string wormId);
        public static event RemoveWorm RemoveWormEvent;

        public delegate void SpawnBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel);
        public static event SpawnBlockInterval SpawnBlockIntervalEvent;

        static List<Obstacle> wormObstacleList; // list of spawned worm obstacles

        public static Dictionary<Vector3Int, Obstacle> WormObstacleDict { get; private set; }
        public static Dictionary<Obstacle, Vector3Int> SwappedWormObstacleDict { get; private set; }

        [SerializeField]
        protected string chaseWormId; // worm that is being chased

        /**
         * Listener is fired to start spawning worms when the game is started
         * 
         * @gameMode        the type of game being played determines whether player, ai or both worm types are spawned
         */
        public abstract void onStartGame(GameMode gameMode);

        protected new void Awake()
        {
            base.Awake();
            WormObstacleDict = new Dictionary<Vector3Int, Obstacle>();
            SwappedWormObstacleDict = new Dictionary<Obstacle, Vector3Int>(new ObstacleComparer());
            wormObstacleList = new List<Obstacle>();            
        }

        protected void OnEnable()
        {
        }

        /**
         * Direection change should trigger worm obstacle to update to a turning cell (or out of a turning cell)
         */
        public static void onChangeDirection(DirectionPair directionPair, Tunnel.Tunnel tunnel, string wormId, Tunnel.CellMove cellMove, bool isCreatingTunnel)
        {
            if (!cellMove.isInit)
            {
                Obstacle obstacle = getObstacle(cellMove.lastCellPosition, WormObstacleDict); // get obstacle from the last tunnel position
                updateObstacle(obstacle, WormObstacleDict, SwappedWormObstacleDict, cellMove.lastCellPosition, cellMove.cell, false); // update obstacle's position to next cell position
            }
        }

        /**
         * Complements listener onChangeDirection to update cell position for new straight junction segments
         */
        public static void onCreateJunctionOnCollision(Tunnel.Tunnel collisionTunnel, DirectionPair dirPair, Tunnel.CellMove cellMove, string playerId)
        {
        if (!cellMove.isInit && dirPair.isStraight()) // let onChangeDirection handle turns
            {
                Obstacle obstacle = getObstacle(cellMove.lastCellPosition, WormObstacleDict); // get obstacle from the last tunnel position
                updateObstacle(obstacle, WormObstacleDict, SwappedWormObstacleDict, cellMove.lastCellPosition, cellMove.cell, false); // update obstacle's position to next cell position
            }
        }

        /**
         * When a tunnel is entered listen for worm interval events, and forward this to the onBlockListener
         */
        public static void onWormInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel)
        {
            bool isTunnelSame = Tunnel.TunnelMap.isTunnelSame(blockPositionInt, lastBlockPositionInt);
            onBlockInterval(isBlockInterval, blockPositionInt, lastBlockPositionInt, tunnel, isTunnelSame, false, false); // last 2 arguments dont matter
        }

        protected void RaiseSpawnIntervalEvent(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Straight tunnel)
        {
            SpawnBlockIntervalEvent(isBlockInterval, blockPositionInt, lastBlockPositionInt, tunnel);
        }

        /**
         * Block interval event forwarded to the correct worm. Get the worm from the cell that the tunnel just occupied, and update the location of the worm
         * 
         * @lastBlockPositionInt        the most recently saved position of the worm
         * @isStraight                  whether going straight
         */
        public static void onBlockInterval(bool isBlockMultiple, Vector3Int blockPosition, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel, bool isTunnelSame, bool isTurn, bool isCollide)
        {
            Obstacle WormObstacle = getObstacle(lastBlockPositionInt, WormObstacleDict);

            if (WormObstacle != null)
            {
                // when a new cell has been reached, update the map with the worm's new cell position only if new cell belongs to a straight tunnel
                if (isBlockMultiple && isTunnelSame) 
                {
                    updateObstacle(WormObstacle, WormObstacleDict, SwappedWormObstacleDict, lastBlockPositionInt, blockPosition, false);
                    print("in onBlockInterval update lastblockpos " + lastBlockPositionInt + " to " + blockPosition);
                }
                GameObject WormGO = WormObstacle.obstacleObject;
                SpawnBlockIntervalEvent += WormGO.GetComponent<Worm.Turn>().onBlockInterval; // subscribe turn to the BlockSize event
                SpawnBlockIntervalEvent(isBlockMultiple, blockPosition, lastBlockPositionInt, tunnel);
                SpawnBlockIntervalEvent -= WormGO.GetComponent<Worm.Turn>().onBlockInterval; // subscribe turn to the BlockSize event
            }
            else
            {
                print("WARNING!!! no obstacle found at " + lastBlockPositionInt + " make sure it was destroyed!");
            }
        }

        /**
         * Acknowledge successful spawning of worm. Add the new worm to the list on game start OR as they are spawned
         * 
         * @worm        the type fo worm that has been created
         * @wormGO      the gameobject of the spawned worm
         */
        public static void onInitWorm(Worm.Worm worm, Astar wormAstar, string wormId)
        {
            Obstacle wormObstacle = new Obstacle(worm.gameObject, worm.wormType, wormId);
            List<Obstacle>singleWormObstacleList = new List<Obstacle>() { wormObstacle };
            wormObstacleList.Add(wormObstacle);
            initializeObstacleDict(WormObstacleDict, SwappedWormObstacleDict, singleWormObstacleList);
            initializeInitialCells(wormObstacle);
        }

        /**
         * After the obstacle dicts are initialized, update the initial cell property of each worm
         * 
         * @obstacles       use the cell position of the obstacle to update the worm's initialCell property
         */
        private static void initializeInitialCells(Obstacle obstacle)
        {
            obstacle.obstacleObject.GetComponent<Worm.WormBase>().setInitialCell(obstacle.obstacleCell);
        }

        protected override List<Obstacle> getObstacleList()
        {
            return wormObstacleList;
        }

        /**
         * When game ends, destroy all worms
         */
        public void onDestroyGame()
        {
            foreach (KeyValuePair<Vector3Int, Obstacle> WormObstacleEntry in WormObstacleDict)
            {
                Vector3Int wormCell = WormObstacleEntry.Key;
                onRemoveWorm(wormCell);
            }
        }

        /**
         * Destroy a single worm
         * 
         * @currentCell     the cell the worm died in
         * @wormId          the id of the worm is not neeeded to index the dictionary, so null is passed in
         */
        public static void onRemoveWorm(Vector3Int currentCell)
        {
            string wormId = WormObstacleDict[currentCell].obstacleId;

            RemoveWormEvent += Worm.WormManager.Instance.onRemoveWorm;
            RemoveWormEvent(wormId);
            RemoveWormEvent -= Worm.WormManager.Instance.onRemoveWorm;

            destroyObstacle(WormObstacleDict, SwappedWormObstacleDict, currentCell, wormObstacleList);
        }

        protected void OnDisable()
        {
        }
    }

}