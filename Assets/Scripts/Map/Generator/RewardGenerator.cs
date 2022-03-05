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

        private new void Awake()
        {
            base.Awake();
            RewardObstacleDict = new Dictionary<Vector3Int, Obstacle>();
        }  

        /**
         * Listener fired when the game starts
         * 
         * @gameMode        type of game can the type of rewards in the map
         */
        public void onStartGame(GameMode gameMode)
        {
            if (gameMode == GameMode.ReachTheGoal)
            {
                List<Obstacle> RewardObstacleList = getObstacleList();
                initializeObstacleDict(RewardObstacleDict, SwapRewardObstacleDict, RewardObstacleList);
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
            InitObjectiveEvent += wormAstar.onInitObjective;
            InitObjectiveEvent(goalCell);
            InitObjectiveEvent -= wormAstar.onInitObjective;
        }

        /**
         * Initialize the goal and return a wrapper class containing the goal. 
         * TODO: In addition to Goal, add other types of reward obstacles
         */
        protected override List<Obstacle> getObstacleList()
        {
            goalCell = getRandomEligibleCell();
            Obstacle GoalObstacle = ObstacleFactory.Instance.getObstacle(ObstacleType.Goal, GOAL_ID);
            List<Obstacle> ObstacleList = new List<Obstacle> { GoalObstacle };
            return ObstacleList;
        }
    }
}
