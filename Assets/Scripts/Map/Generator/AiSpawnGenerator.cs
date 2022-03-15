using UnityEngine;

namespace Map
{
    public class AiSpawnGenerator : SpawnGenerator
    {
        public delegate void SpawnAIWorm(string wormId);
        public event SpawnAIWorm SpawnAIWormEvent;

        [SerializeField]
        private int aiCount;

        private int aiSpawnCount; // used to id ai-controlled worms

        private const string AI_WORM_ID = "AI";

        private new void Awake()
        {
            base.Awake();
            aiSpawnCount = 0;
        }

        private new void OnEnable()
        {
            base.OnEnable();
            SpawnAIWormEvent += FindObjectOfType<Worm.Factory.WormAIFactory>().onSpawn;
        }

        public override void onStartGame(GameMode gameMode)
        { 
            // TODO: Configure types of worms such as attack, patrol
            createAiWorms(aiCount);
        }

        /**
         * AI worms turn ON block intervals (instead of in the preceding cell). 
         * TunnelMaker must be notified to make a turn decision ahead of the onBlockInterval listener
         */
        public void onAiBlockInterval(bool isBlockMultiple, Vector3Int blockPosition, Vector3Int lastBlockPositionInt, Tunnel.Straight tunnel)
        {
            Obstacle WormObstacle = getObstacle(lastBlockPositionInt, WormObstacleDict);
            if (WormObstacle.obstacleType == ObstacleType.AIWorm) // if worm is AI send an additional event to advance to the next checkpoint
            {
                GameObject WormGO = WormObstacle.obstacleObject;

                SpawnBlockIntervalEvent += WormGO.GetComponent<Worm.TunnelMaker>().onBlockInterval;
                RaiseSpawnIntervalEvent(isBlockMultiple, blockPosition, lastBlockPositionInt, tunnel);
                SpawnBlockIntervalEvent -= WormGO.GetComponent<Worm.TunnelMaker>().onBlockInterval;
            }
        }

        /**
         * Player is destroyed and spawned again
         */
        public void onRespawnAi(Vector3Int currentCell)
        {
            Obstacle WormObstacle = WormObstacleDict[currentCell];
            onRemoveWorm(currentCell); // wait for removal confirmation before spawning again
            onAiSpawn(WormObstacle.obstacleType, WormObstacle.obstacleId);
        }

        public void onAiSpawn(ObstacleType wormType, string id)
        {
            SpawnAIWormEvent(id);
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

        private new void OnDisable()
        {
            base.OnDisable();
            SpawnAIWormEvent -= FindObjectOfType<Worm.Factory.WormAIFactory>().onSpawn;
        }
    }
}