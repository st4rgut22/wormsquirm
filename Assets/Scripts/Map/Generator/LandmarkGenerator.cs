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
        private GameObject Wall;

        [SerializeField]
        private Transform WallTransform;

        [SerializeField]
        private int rockObstacleCount; // originally 1500

        private Dictionary<Vector3Int, Obstacle> RockObstacleDict;
        private Dictionary<Obstacle, Vector3Int> SwapRockObstacleDict;

        private int rockCount;

        private const float WALL_WIDTH = 1f;

        private const string ROCK_ID = "Rock";

        private List<GameObject> WallList;

        private Quaternion X_ORIENTATION = Quaternion.Euler(0, 90, 0);
        private Quaternion Y_ORIENTATION = Quaternion.Euler(90, 0, 0);
        private Quaternion Z_ORIENTATION = Quaternion.Euler(0, 0, 0);

        private new void Awake()
        {
            base.Awake();
            rockCount = 0;
            WallList = new List<GameObject>();
            RockObstacleDict = new Dictionary<Vector3Int, Obstacle>();
            SwapRockObstacleDict = new Dictionary<Obstacle, Vector3Int>();
        }

        private void Start()
        {
            initPositionDict[getObstacleId(ROCK_ID, 0)] = new Vector3Int(7, 2, 7);
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
                List<Obstacle> RockObstacleList = getObstacleList(rockObstacleCount, ROCK_ID);
                initializeObstacleList(RockObstacleDict, SwapRockObstacleDict, RockObstacleList);
                positionObstacles(); // place obstacles in center of cells
            }
            createWalls();
        }

        private GameObject instantiateWall(Direction direction)
        {
            Quaternion orientation;
            if (direction == Direction.Up || direction == Direction.Down)
            {
                orientation = Y_ORIENTATION;
            }
            else if (direction == Direction.Left || direction == Direction.Right)
            {
                orientation = X_ORIENTATION;
            }
            else if (direction == Direction.Forward || direction == Direction.Back)
            {
                orientation = Z_ORIENTATION;
            }
            else
            {
                throw new System.Exception("no direction " + direction);
            }
            float wallDistance = GameManager.MAP_LENGTH + WALL_WIDTH / 2;
            Vector3 dirVector = direction.getVectorFromDirection();
            Vector3 wallPosition = wallDistance * dirVector;

            GameObject wallInstance = Instantiate(Wall, wallPosition, orientation);
            wallInstance.transform.localScale = new Vector3(GameManager.MAP_LENGTH * 2, GameManager.MAP_LENGTH * 2, 1);
            wallInstance.transform.parent = WallTransform;
            return wallInstance;
        }

        private void createWalls()
        {
            WallList.Add(instantiateWall(Direction.Up));
            WallList.Add(instantiateWall(Direction.Down));
            WallList.Add(instantiateWall(Direction.Left));
            WallList.Add(instantiateWall(Direction.Right));
            WallList.Add(instantiateWall(Direction.Back));
            WallList.Add(instantiateWall(Direction.Forward));
        }

        public void onDestroyGame()
        {
            WallList.ForEach((GameObject wallGO) =>
            {
                Destroy(wallGO);
            });
        }
    }

}