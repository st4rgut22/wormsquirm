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

        public delegate void SpawnBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Straight tunnel);
        public static event SpawnBlockInterval SpawnBlockIntervalEvent;

        static List<Obstacle> wormObstacleList; // list of spawned worm obstacles

        public static Dictionary<Vector3Int, Obstacle> WormObstacleDict { get; private set; }
        public static Dictionary<Obstacle, Vector3Int> SwappedWormObstacleDict { get; private set; }

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
            SwappedWormObstacleDict = new Dictionary<Obstacle, Vector3Int>();
            wormObstacleList = new List<Obstacle>();
        }

        protected void OnEnable()
        {
        }

        /**
         * Update the position of obstacle if not done so in onBlockInterval listener (coming out of turns, etc)
         * 
         * @currentCellPosition     the cell the worm is currently in
         * @nextCellPosition        the cell the worm wil lbe in in next
         * @isDeleteCurCell         whether the old cell should be deleted
         */
        public static void onUpdateObstacle(Vector3Int curCellPos, Vector3Int nextCellPos, bool isDeleteCurCell)
        {
            print("update to " + nextCellPos);
            Obstacle obstacle = getObstacle(curCellPos, WormObstacleDict); // get obstacle from the last tunnel position
            updateObstacle(obstacle, WormObstacleDict, SwappedWormObstacleDict, curCellPos, nextCellPos, isDeleteCurCell); // update obstacle's position to next cell position
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
            onBlockInterval(isBlockInterval, blockPositionInt, lastBlockPositionInt, (Tunnel.Straight) tunnel, isTunnelSame);
        }

        protected void RaiseSpawnIntervalEvent(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Straight tunnel)
        {
            SpawnBlockIntervalEvent(isBlockInterval, blockPositionInt, lastBlockPositionInt, tunnel);
        }

        /**
         * Block interval event forwarded to the correct worm. Get the worm from the cell that the tunnel just occupied, and update the location of the worm
         * 
         * @lastBlockPositionInt        the most recently saved position of the worm
         * @isCellSameTUnnel            whether the cell belongs to the same tunnel
         */
        public static void onBlockInterval(bool isBlockMultiple, Vector3Int blockPosition, Vector3Int lastBlockPositionInt, Tunnel.Straight tunnel, bool isCellSameTunnel)
        {
            Obstacle WormObstacle = getObstacle(lastBlockPositionInt, WormObstacleDict);

            if (WormObstacle != null)
            {
                if (isBlockMultiple && isCellSameTunnel) // when a new cell has been reached, update the map with the worm's new cell position
                {                                        // if not the same tunnel, the straight tunnel ends here and shouldn't update the worm cell to the next block position (do this in onAddTunnel instead)
                    updateObstacle(WormObstacle, WormObstacleDict, SwappedWormObstacleDict, lastBlockPositionInt, blockPosition, false);
                    print("in onBlockInterval update lastblockpos " + lastBlockPositionInt + " to " + blockPosition);
                }
                if (!isCellSameTunnel)
                {
                    print("in onBlockInterval not same cell!");
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