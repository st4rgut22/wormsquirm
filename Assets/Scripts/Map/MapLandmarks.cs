using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class MapLandmarks : MonoBehaviour
    {
        [SerializeField]
        private bool isLandmarksEnabled;

        [SerializeField]
        private bool isResetLandmarks;

        [SerializeField]
        private GameObject Goal;

        [SerializeField]
        private Vector3Int goal; // originally <10,10,10>

        [SerializeField]
        private GameObject Obstacle;

        [SerializeField]
        private Transform ObstacleNetwork;

        [SerializeField]
        private int obstacleCount; // originally 1500

        public static GameObject GoalInstance;

        public delegate void initLandmarks(Dictionary<Vector3Int, GameObject> obstacleDict, Vector3Int goalLocation);
        public initLandmarks initLandmarksEvent;

        const int MAP_LENGTH = 10; // distance from origin to one edge of the map

        Vector3Int defaultGoal = Vector3Int.zero;

        public Dictionary<Vector3Int, GameObject> obstacleDict; // cells the worm is not allowed to enter

        private void Awake()
        {
            obstacleDict = new Dictionary<Vector3Int, GameObject>();
        }

        private void OnEnable()
        {
            initLandmarksEvent += FindObjectOfType<Astar>().onInitLandmark;
        }

        private void Start()
        {
            if (isLandmarksEnabled)
            {
                initializeLandmarks();
            }

            initLandmarksEvent(obstacleDict, goal);
        }

        /**
         * One scene re-load it is possible to re-initialize the goal and obstacles to generate a new path for worms
         */
        public void onResetTunnelNetwork()
        {
            if (isLandmarksEnabled)
            {
                if (isResetLandmarks)
                {
                    Destroy(GoalInstance);

                    foreach (KeyValuePair<Vector3Int, GameObject> obstacleEntry in obstacleDict) // destroy all pre-existing obstacle gameobjects
                    {
                        Destroy(obstacleEntry.Value);
                    }
                    initializeLandmarks();
                }
            }
            initLandmarksEvent(obstacleDict, goal);
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
            obstacleDict = new Dictionary<Vector3Int, GameObject>();

            for (int i = 0; i < obstacleCount; i++)
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

            GoalInstance = Goal.instantiate(goal, ObstacleNetwork);
        }

        /**
         * Initialize the location of the goal subject to constraints like min dist from starting point
         */
        private void initializeRandomGoal(Dictionary<Vector3Int, GameObject>prohibitedCellDict)
        {
            while (true)
            {
                goal = getRandomCell();
                if (!prohibitedCellDict.ContainsKey(goal))
                {
                    break;
                }
            }
            Goal.instantiate(goal, ObstacleNetwork);
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Astar>())
            {
                initLandmarksEvent -= FindObjectOfType<Astar>().onInitLandmark;
            }            
        }
    }
}

