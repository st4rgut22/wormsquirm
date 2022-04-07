using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class LandmarkGenerator : ObstacleGenerator
    {
        public delegate void initRocks(Dictionary<Vector3Int, Obstacle> obstacleDict);
        public initRocks initRocksEvent;

        [SerializeField]
        private bool isRockObstacleEnabled;

        [SerializeField]
        private Obstacle RockObstaclePrefab;

        [SerializeField]
        private int rockObstacleCount; // originally 1500

        private Dictionary<Vector3Int, Obstacle> RockObstacleDict;
        private Dictionary<Obstacle, Vector3Int> SwapRockObstacleDict;

        private int rockCount;

        private new void Awake()
        {
            base.Awake();
            rockCount = 0;
            RockObstacleDict = new Dictionary<Vector3Int, Obstacle>();
            SwapRockObstacleDict = new Dictionary<Obstacle, Vector3Int>();
        }

        /**
         * Listener fired when the game starts
         * 
         * @gameMode        type of game can influence how many obstacles generated
         */
        public void onStartGame(GameMode gameMode)
        {
            if (gameMode != GameMode.DebugFixedPath && isRockObstacleEnabled)    // TestFixedPath is the only situation it doesnt make sene to have obstacles
            {
                List<Obstacle> RockObstacleList = getObstacleList();
                initializeObstacleDict(RockObstacleDict, SwapRockObstacleDict, RockObstacleList);
            }
        }

        public void onDestroyGame()
        {

        }

        /**
         * Get a list of rock obstacle objects
         */
        protected override List<Obstacle> getObstacleList()
        {
            List<Obstacle> RockObstacleList = new List<Obstacle>();

            for (int i=0;i<rockObstacleCount;i++)
            {
                string rockId = rockCount.ToString();
                Obstacle rockObstacle = ObstacleFactory.Instance.getObstacle(ObstacleType.Rock, rockId);
                RockObstacleList.Add(rockObstacle);
                rockCount += 1;
            }
            return RockObstacleList;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}