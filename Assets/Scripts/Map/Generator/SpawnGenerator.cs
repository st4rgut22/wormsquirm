using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class SpawnGenerator : ObstacleGenerator
    {
        public delegate void SpawnAIWorm(string wormId);
        public event SpawnAIWorm SpawnAIWormEvent;

        public delegate void SpawnPlayerWorm(string wormId);
        public event SpawnPlayerWorm SpawnPlayerWormEvent;

        public delegate void SpawnBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Straight tunnel);
        public event SpawnBlockInterval SpawnBlockIntervalEvent;

        [SerializeField]
        private int aiCount;

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
            SpawnAIWormEvent += FindObjectOfType<Worm.Factory.WormAIFactory>().onSpawn;
            SpawnPlayerWormEvent += FindObjectOfType<Worm.Factory.WormPlayerFactory>().onSpawn;
        }

        /**
         * Update the position of obstacle if not done so in onBlockInterval listener (coming out of turns, etc)
         * 
         * @currentCellPosition     the cell the worm is currently in
         * @nextCellPosition        the cell the worm wil lbe in in next
         */
        public void onUpdateObstacle(Vector3Int curCellPos, Vector3Int nextCellPos)
        {
            print("update to " + nextCellPos);
            Obstacle obstacle = getObstacle(curCellPos, WormObstacleDict); // get obstacle from the last tunnel position
            updateObstacle(obstacle, WormObstacleDict, SwappedWormObstacleDict, curCellPos, nextCellPos); // update obstacle's position to next cell position
        }

        // replace above
        public void onAddTunnel(Tunnel.Tunnel tunnel, Tunnel.CellMove cellMove, DirectionPair directionPair, string wormId)
        {
            if (!cellMove.isInit)
            {
                print("timme;l last cel position " + cellMove.lastCellPosition + " worm cur cell position " + cellMove.cell);
                Obstacle obstacle = getObstacle(cellMove.lastCellPosition, WormObstacleDict); // get obstacle from the last tunnel position
                updateObstacle(obstacle, WormObstacleDict, SwappedWormObstacleDict, cellMove.lastCellPosition, cellMove.cell); // update obstacle's position to next cell position
            }
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
            if (isBlockMultiple && isCellSameTunnel) // when a new cell has been reached, update the map with the worm's new cell position
            {                                        // if not the same tunnel, the straight tunnel ends here and shouldn't update the worm cell to the next block position (do this in onAddTunnel instead)
                updateObstacle(WormObstacle, WormObstacleDict, SwappedWormObstacleDict, lastBlockPositionInt, blockPosition);
                print("in onBlockInterval update lastblockpos " + lastBlockPositionInt + " to " + blockPosition);
            }
            if (!isCellSameTunnel)
            {
                print("in onBlockInterval not same cell!");
            }
            GameObject WormGO = WormObstacle.obstacleObject;
            if (WormObstacle.obstacleType == ObstacleType.AIWorm) // if worm is AI send an additional event to advance to the next checkpoint
            {
                SpawnBlockIntervalEvent += WormGO.GetComponent<Worm.TunnelMaker>().onBlockInterval;
                SpawnBlockIntervalEvent(isBlockMultiple, blockPosition, lastBlockPositionInt, tunnel);
                SpawnBlockIntervalEvent -= WormGO.GetComponent<Worm.TunnelMaker>().onBlockInterval;
            }
            SpawnBlockIntervalEvent += WormGO.GetComponent<Worm.Turn>().onBlockInterval; // subscribe turn to the BlockSize event
            SpawnBlockIntervalEvent(isBlockMultiple, blockPosition, lastBlockPositionInt, tunnel);
            SpawnBlockIntervalEvent -= WormGO.GetComponent<Worm.Turn>().onBlockInterval; // subscribe turn to the BlockSize event
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
            wormObstacleList.Add(wormObstacle);
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
            if (gameMode == GameMode.Solo) // create a single human worm, no ai worms
            {
                SpawnPlayerWormEvent(HUMAN_WORM_ID);
            }
            else if (gameMode == GameMode.ReachTheGoal) // create human worm and ai worms
            {
                createAiWorms(aiCount);
                SpawnPlayerWormEvent(HUMAN_WORM_ID);
            }
            else if (gameMode == GameMode.TestFixedPath) // create a ai worm but no human worm
            {
                createAiWorms(aiCount);
            }
            else
            {
                throw new System.Exception("the game mode " + gameMode + " is not supported yet");
            }
            int totalSpawnCount = aiCount + 1;
            if (totalSpawnCount != wormObstacleList.Count)
            {
                throw new System.Exception("the number of wormObstacles created: " + wormObstacleList.Count + ", does not match expected count: " + totalSpawnCount);
            }
            List<Obstacle> obstacles = getObstacleList();
            initializeObstacleDict(WormObstacleDict, SwappedWormObstacleDict, obstacles);
            initializeInitialCells(obstacles);
        }

        /**
         * After the obstacle dicts are initialized, update the initial cell property of each worm
         * 
         * @obstacles       use the cell position of the obstacle to update the worm's initialCell property
         */
        private void initializeInitialCells(List<Obstacle> obstacles)
        {
            obstacles.ForEach((Obstacle obstacle) => obstacle.obstacleObject.GetComponent<Worm.WormBase>().setInitialCell(obstacle.obstacleCell));
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
            onRemoveWorm(currentCell);
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
                destroyObstacle(WormObstacleDict, SwappedWormObstacleDict, wormCell);
            }
        }

        /**
         * Destroy a single worm
         * 
         * @currentCell     the cell the worm died in
         */
        public void onRemoveWorm(Vector3Int currentCell)
        {
            destroyObstacle(WormObstacleDict, SwappedWormObstacleDict, currentCell);
        }

        private void OnDisable()
        {
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