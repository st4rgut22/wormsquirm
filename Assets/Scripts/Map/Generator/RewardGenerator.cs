using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class RewardGenerator : ObstacleGenerator
    {
        public delegate void InitObjective(Vector3Int objectiveCell);
        public event InitObjective InitObjectiveEvent;

        [SerializeField]
        private Vector3Int goalCell; // originally <10,10,10>

        private Dictionary<Vector3Int, Obstacle> RewardObstacleDict;    // <reward cell location, reward obstacle>
        private Dictionary<Obstacle, Vector3Int> SwapRewardObstacleDict;    // <reward cell location, reward obstacle>

        public static GameObject GoalInstance;

        Vector3Int defaultGoal = Vector3Int.zero;

        private const string GOAL_ID = "Goal";
        private const string REWARD_ID = "Reward";

        private new void Awake()
        {
            base.Awake();
            RewardObstacleDict = new Dictionary<Vector3Int, Obstacle>();
            SwapRewardObstacleDict = new Dictionary<Obstacle, Vector3Int>();
        }  

        /**
         * Listener fired when the game starts
         * 
         * @gameMode        type of game can the type of rewards in the map
         */
        public void onStartGame(GameMode gameMode)
        {
            if (gameMode == GameMode.Chase)
            {
                Obstacle GoalObstacle = ObstacleFactory.Instance.getObstacle(ObstacleType.Goal, GOAL_ID);
                initializeObstacle(RewardObstacleDict, SwapRewardObstacleDict, GoalObstacle);
                positionObstacle(GoalObstacle);
            }
        }

        public void onRemoveReward(Equipment.Block block, string collidedWithId)
        {
            string rewardId = block.getObstacleId();
            destroyObstacle(RewardObstacleDict, SwapRewardObstacleDict, rewardId);
        }

        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Straight tunnel, bool isTunnelSame, bool isTurning, bool isCollide)
        {
            if (isBlockInterval && !isTurning)
            {
                totalObstacleCount += 1;
                Direction dir = Dir.CellDirection.getDirectionFromCells(lastBlockPositionInt, blockPositionInt);

                string rewardObstacleId = getObstacleId(REWARD_ID, totalObstacleCount);
                Vector3Int rewardCell = Dir.Vector.getNextCellFromDirection(blockPositionInt, dir);

                initPositionDict[rewardObstacleId] = rewardCell;
                Obstacle rewardObstacle = ObstacleFactory.Instance.getObstacle(ObstacleType.Reward, rewardObstacleId);

                initializeObstacle(RewardObstacleDict, SwapRewardObstacleDict, rewardObstacle);
                positionObstacle(rewardObstacle);
            }
        }

        /**
         * Once the worm has spawned, send it its destination cell
         * 
         * @worm        the type of worm that has been created
         * @wormGO      the gameobject of the spawned worm
         */
        public void OnInitWorm(Worm.Worm worm, Astar wormAstar, string wormId)
        {
            //InitObjectiveEvent += wormAstar.onInitObjective;
            //InitObjectiveEvent(goalCell);// TESTING
            //InitObjectiveEvent -= wormAstar.onInitObjective;
        }
    }
}
