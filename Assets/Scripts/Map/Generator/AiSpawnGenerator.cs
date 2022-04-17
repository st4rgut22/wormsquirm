using UnityEngine;

namespace Map
{
    public class AiSpawnGenerator : SpawnGenerator
    {
        public delegate void SpawnAIWorm(string wormId, string chaseWormId);
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

        private void Start()
        {
            initWormPositions();
        }

        private new void OnEnable()
        {
            SpawnAIWormEvent += FindObjectOfType<Worm.Factory.WormAIFactory>().onSpawn;
        }

        // initialize positions of a couple worms, the rest are rand generated
        private void initWormPositions()
        {
            // DEBUG FIXED PATH MODE
            //initPositionDict[getAIWormId(0)] = new Vector3Int(0, 0, 0);
            //initPositionDict[getAIWormId(1)] = new Vector3Int(-3, -3, 3);

            // CHASE MODE
            initPositionDict[getObstacleId(AI_WORM_ID, 0)] = new Vector3Int(7, 7, 7);
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
            print("block position is " + blockPosition + " last block position is " + lastBlockPositionInt);
            Obstacle WormObstacle = getObstacle(lastBlockPositionInt, WormObstacleDict);
            GameObject WormGO = WormObstacle.obstacleObject;

            SpawnBlockIntervalEvent += WormGO.GetComponent<Worm.TunnelMaker>().onBlockInterval;
            RaiseSpawnIntervalEvent(isBlockMultiple, blockPosition, lastBlockPositionInt, tunnel);
            SpawnBlockIntervalEvent -= WormGO.GetComponent<Worm.TunnelMaker>().onBlockInterval;
        }

        /**
         * Player is destroyed and spawned again
         */
        public void onRespawnAi(string wormId)
        {
            onRemoveWorm(wormId); // wait for removal confirmation before spawning again
            onAiSpawn(wormId);
        }

        public void onAiSpawn(string obstacleId)
        {
            SpawnAIWormEvent(obstacleId, chaseWormId);
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
                string id = getObstacleId(AI_WORM_ID, i);
                SpawnAIWormEvent(id, chaseWormId);
            }
        }

        private new void OnDisable()
        {
            SpawnAIWormEvent -= FindObjectOfType<Worm.Factory.WormAIFactory>().onSpawn;
        }
    }
}