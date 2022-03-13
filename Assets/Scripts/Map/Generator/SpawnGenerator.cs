using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class SpawnGenerator : ObstacleGenerator
    {
        public delegate void ChangeDirection(DirectionPair directionPair, Tunnel.Tunnel tunnel, string wormId, Tunnel.CellMove cellMove, bool isCreatingTunnel);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void SpawnAIWorm(string wormId);
        public event SpawnAIWorm SpawnAIWormEvent;

        public delegate void RemoveWorm(string wormId);
        public event RemoveWorm RemoveWormEvent;

        public delegate void SpawnPlayerWorm(string wormId);
        public event SpawnPlayerWorm SpawnPlayerWormEvent;

        public delegate void SpawnBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Straight tunnel);
        public event SpawnBlockInterval SpawnBlockIntervalEvent;

        [SerializeField]
        private int aiCount;

        [SerializeField]
        private int playerCount; // the # of human players (equals 1 until multiplayer support arrives)

        private int aiSpawnCount; // used to id ai-controlled worms

        private int humanSpawnCount; // used to id human controlled worms TODO: multiplayer will have multiple human-controlled worms

        List<Obstacle> wormObstacleList; // list of spawned worm obstacles

        public static Dictionary<Vector3Int, Obstacle> WormObstacleDict { get; private set; }
        public Dictionary<Obstacle, Vector3Int> SwappedWormObstacleDict { get; private set; }

        private const string HUMAN_WORM_ID = "Player";
        private const string AI_WORM_ID = "AI";

        private new void Awake()
        {
            base.Awake();
            WormObstacleDict = new Dictionary<Vector3Int, Obstacle>();
            SwappedWormObstacleDict = new Dictionary<Obstacle, Vector3Int>();
            wormObstacleList = new List<Obstacle>();
            humanSpawnCount = aiSpawnCount = 0;
        }

        private void OnEnable()
        {
            RemoveWormEvent += Worm.WormManager.Instance.onRemoveWorm;
            SpawnAIWormEvent += FindObjectOfType<Worm.Factory.WormAIFactory>().onSpawn;
            SpawnPlayerWormEvent += FindObjectOfType<Worm.Factory.WormPlayerFactory>().onSpawn;
        }

        /**
         * Update the position of obstacle if not done so in onBlockInterval listener (coming out of turns, etc)
         * 
         * @currentCellPosition     the cell the worm is currently in
         * @nextCellPosition        the cell the worm wil lbe in in next
         * @isDeleteCurCell         whether the old cell should be deleted
         */
        public void onUpdateObstacle(Vector3Int curCellPos, Vector3Int nextCellPos, bool isDeleteCurCell)
        {
            print("update to " + nextCellPos);
            Obstacle obstacle = getObstacle(curCellPos, WormObstacleDict); // get obstacle from the last tunnel position
            updateObstacle(obstacle, WormObstacleDict, SwappedWormObstacleDict, curCellPos, nextCellPos, isDeleteCurCell); // update obstacle's position to next cell position
        }

        /**
         * Direection change should trigger worm obstacle to update to a turning cell (or out of a turning cell)
         */
        public void onChangeDirection(DirectionPair directionPair, Tunnel.Tunnel tunnel, string wormId, Tunnel.CellMove cellMove, bool isCreatingTunnel)
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
        public void onCreateJunctionOnCollision(Tunnel.Tunnel collisionTunnel, DirectionPair dirPair, Tunnel.CellMove cellMove, string playerId)
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
        public void onWormInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel)
        {
            bool isTunnelSame = Tunnel.TunnelMap.isTunnelSame(blockPositionInt, lastBlockPositionInt);
            onBlockInterval(isBlockInterval, blockPositionInt, lastBlockPositionInt, (Tunnel.Straight) tunnel, isTunnelSame);
        }

        /**
         * Block interval event forwarded to the correct worm. Get the worm from the cell that the tunnel just occupied, and update the location of the worm
         * 
         * @lastBlockPositionInt        the most recently saved position of the worm
         * @isCellSameTUnnel            whether the cell belongs to the same tunnel
         */
        public void onBlockInterval(bool isBlockMultiple, Vector3Int blockPosition, Vector3Int lastBlockPositionInt, Tunnel.Straight tunnel, bool isCellSameTunnel)
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
         * AI worms turn ON block intervals (instead of in the preceding cell). TunnelMaker must be notified to make a turn decision ahead of the onBlockInterval listener
         */
        public void onAiBlockInterval(bool isBlockMultiple, Vector3Int blockPosition, Vector3Int lastBlockPositionInt, Tunnel.Straight tunnel)
        {
            Obstacle WormObstacle = getObstacle(lastBlockPositionInt, WormObstacleDict);
            if (WormObstacle.obstacleType == ObstacleType.AIWorm) // if worm is AI send an additional event to advance to the next checkpoint
            {
                GameObject WormGO = WormObstacle.obstacleObject;

                SpawnBlockIntervalEvent += WormGO.GetComponent<Worm.TunnelMaker>().onBlockInterval;
                SpawnBlockIntervalEvent(isBlockMultiple, blockPosition, lastBlockPositionInt, tunnel);
                SpawnBlockIntervalEvent -= WormGO.GetComponent<Worm.TunnelMaker>().onBlockInterval;
            }
        }

        /**
         * Acknowledge successful spawning of worm. Add the new worm to the list on game start OR as they are spawned
         * 
         * @worm        the type fo worm that has been created
         * @wormGO      the gameobject of the spawned worm
         */
        public void onInitWorm(Worm.Worm worm, Astar wormAstar, string wormId)
        {
            Obstacle wormObstacle = new Obstacle(worm.gameObject, worm.wormType, wormId);
            List<Obstacle>singleWormObstacleList = new List<Obstacle>() { wormObstacle };
            wormObstacleList.Add(wormObstacle);
            initializeObstacleDict(WormObstacleDict, SwappedWormObstacleDict, singleWormObstacleList);
            initializeInitialCells(wormObstacle);
        }

        /**
         * Listener fired when a worm is spawned after being destroyed
         * 
         * @worm        the type of the worm
         * @id          the id of the worm that is spawning
         */
        public void onSpawn(ObstacleType wormType, string id)
        {
            if (wormType == ObstacleType.AIWorm)
            {
                SpawnAIWormEvent(id);
            }
            else if (wormType == ObstacleType.PlayerWorm)
            {
                SpawnPlayerWormEvent(id);
            }
            else
            {
                throw new System.Exception("not a valid worm type: " + wormType);
            }
        }

        /**
         * Create worms controlled by AI on game start
         * 
         * @humanWorm   the type human worm
         * @count       the number of AI worms to create
         */
        public void createAiWorms(int count)
        {
            for (int i = aiSpawnCount; i < aiSpawnCount + count; i++)
            {
                string id = AI_WORM_ID + " " + aiSpawnCount.ToString();
                SpawnAIWormEvent(id);
            }
        }

        /**
         * Listener is fired to start spawning worms when the game is started
         * 
         * @gameMode        the type of game being played determines whether player, ai or both worm types are spawned
         */
        public void onStartGame(GameMode gameMode)
        {
            if (gameMode == GameMode.Solo) // create a single human worm, no ai worms (TESTING MODE)
            {
                aiCount = 0;
                SpawnPlayerWormEvent(HUMAN_WORM_ID);
            }
            else if (gameMode == GameMode.ReachTheGoal) // create human worm and ai worms
            {
                createAiWorms(aiCount);
                SpawnPlayerWormEvent(HUMAN_WORM_ID);
            }
            else if (gameMode == GameMode.TestFixedPath) // create a ai worm but no human worm (TESTING MODE)
            {
                playerCount = 0; // temp
                createAiWorms(aiCount);
            }
            else if (gameMode == GameMode.TestAutoPath) // create a ai worm but no human worm (TESTING MODE)
            {
                playerCount = 0; // temp
                createAiWorms(aiCount);
            }
            else
            {
                throw new System.Exception("the game mode " + gameMode + " is not supported yet");
            }
            int totalSpawnCount = aiCount + playerCount;
            if (totalSpawnCount != wormObstacleList.Count)
            {
                throw new System.Exception("the number of wormObstacles created: " + wormObstacleList.Count + ", does not match expected count: " + totalSpawnCount);
            }
        }

        /**
         * After the obstacle dicts are initialized, update the initial cell property of each worm
         * 
         * @obstacles       use the cell position of the obstacle to update the worm's initialCell property
         */
        private void initializeInitialCells(Obstacle obstacle)
        {
            obstacle.obstacleObject.GetComponent<Worm.WormBase>().setInitialCell(obstacle.obstacleCell);
        }

        protected override List<Obstacle> getObstacleList()
        {
            return wormObstacleList;
        }

        /**
         * Player is destroyed and spawned again
         */
        public void onRespawn(Vector3Int currentCell)
        {
            Obstacle WormObstacle = WormObstacleDict[currentCell];
            onRemoveWorm(currentCell); // wait for removal confirmation before spawning again
            onSpawn(WormObstacle.obstacleType, WormObstacle.obstacleId);
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
        public void onRemoveWorm(Vector3Int currentCell)
        {
            string wormId = WormObstacleDict[currentCell].obstacleId;
            RemoveWormEvent(wormId);
            destroyObstacle(WormObstacleDict, SwappedWormObstacleDict, currentCell, wormObstacleList);
        }

        private void OnDisable()
        {
            if (Worm.WormManager.Instance)
            {
                RemoveWormEvent += Worm.WormManager.Instance.onRemoveWorm;
            }
            if (FindObjectOfType<Worm.Factory.WormAIFactory>())
            {
                SpawnAIWormEvent -= FindObjectOfType<Worm.Factory.WormAIFactory>().onSpawn;
            }
            if (FindObjectOfType<Worm.Factory.WormPlayerFactory>())
            {
                SpawnPlayerWormEvent -= FindObjectOfType<Worm.Factory.WormPlayerFactory>().onSpawn;
            }
        }
    }

}