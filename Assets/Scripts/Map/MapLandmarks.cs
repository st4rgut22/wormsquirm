using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class MapLandmarks : GenericSingletonClass<MapLandmarks>
    {
        [SerializeField]
        private bool enableLandmarks;

        [SerializeField]
        private GameObject Goal;

        [SerializeField]
        private GameObject Obstacle;

        [SerializeField]
        private Transform ObstacleNetwork;

        public delegate void initLandmarks(Dictionary<Vector3Int, GameObject> obstacleDict, Vector3Int goalLocation);
        public initLandmarks initLandmarksEvent;

        const int MAP_LENGTH = 10; // distance from origin to one edge of the map
        const int OBSTACLE_COUNT = 1500; // # of obstacles increases with difficulty
        const float MINIMUM_GOAL_DIST = 7; // minimum distance the goal is from the origin. Must be positive

        Dictionary<Vector3Int, GameObject> obstacleDict; // cells the worm is not allowed to enter
        public Vector3Int goal { get; private set; }

        /**
         * Initialize landmarks in map
         */
        private new void Awake()
        {
            base.Awake();
            if (enableLandmarks)
            {
                obstacleDict = new Dictionary<Vector3Int, GameObject>();
                initializeLandmarks(); // populate obstacle dictionary
                goal = new Vector3Int(MAP_LENGTH, MAP_LENGTH, MAP_LENGTH); // initializeGoal(obstacleDict);
                Goal.instantiate(goal, ObstacleNetwork);
                initLandmarksEvent(obstacleDict, goal);
            }
        }

        /**
         * Check whether a cell contains an obstacle
         * 
         * @cell the cell the worm want to enter
         */
        public bool isCellObstacle(Vector3Int cell)
        {
            return obstacleDict.ContainsKey(cell);
        }

        private int getRandomPosition()
        {
            return Random.Range(-MAP_LENGTH, MAP_LENGTH);
        }

        private Vector3Int getRandomCell()
        {
            int randX = getRandomPosition();
            int randY = getRandomPosition();
            int randZ = getRandomPosition();
            Vector3Int randCell = new Vector3Int(randX, randY, randZ);
            return randCell;
        }



        /**
         * Randomly generate landmarks within the confines of the map
         */
        private void initializeLandmarks()
        {
            for (int i = 0; i < OBSTACLE_COUNT; i++)
            {
                Vector3Int randObstaclePosition = getRandomCell();
                bool isBlockingOrigin = randObstaclePosition.isAdjacent(Tunnel.TunnelManager.Instance.initialCell);

                if (!obstacleDict.ContainsKey(randObstaclePosition) && !isBlockingOrigin)
                {
                    GameObject obstacleGO = Obstacle.instantiate(randObstaclePosition, ObstacleNetwork);
                    print("Created obstacle at " + randObstaclePosition);
                    obstacleDict[randObstaclePosition] = obstacleGO;
                }
            }
        }

        /**
         * Initialize the location of the goal subject to constraints like min dist from starting point
         */
        private void initializeRandomGoal(Dictionary<Vector3Int, GameObject>prohibitedCellDict)
        {
            float originDist = 0;

            while (originDist < MINIMUM_GOAL_DIST)
            {
                goal = getRandomCell();
                if (!prohibitedCellDict.ContainsKey(goal))
                {
                    originDist = Vector3Int.Distance(goal, Tunnel.TunnelManager.Instance.initialCell);
                }
            }
            Goal.instantiate(goal, ObstacleNetwork);
        }
    }
}

