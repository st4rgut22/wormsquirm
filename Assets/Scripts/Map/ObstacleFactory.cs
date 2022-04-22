using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Map
{
    public class ObstacleFactory : GenericSingletonClass<ObstacleFactory>
    {
        [SerializeField]
        private Transform RockObstacleNetwork;

        [SerializeField]
        private Transform WormPrefabNetwork;

        [SerializeField]
        private Transform RewardObstacleNetwork;

        [SerializeField]
        private GameObject GoalPrefab;

        [SerializeField]
        private Transform RockObstacleParent;

        [SerializeField]
        private List<GameObject> RockPrefabList;

        [SerializeField]
        private List<GameObject> RewardPrefabList;

        private GameObject getRandomObstacle(List<GameObject> ObstacleGOList) // use the same thing for obstacle fo type rock before
        {
            int randObstacleIndex = Random.Range(0, ObstacleGOList.Count - 1);
            return ObstacleGOList[randObstacleIndex];
        }

        /**
         * Get the obstacle using the obstacle's type
         */
        public Obstacle getObstacle(ObstacleType obstacleType, string obstacleId)
        {
            Obstacle obstacle;
            switch (obstacleType)
            {
                case ObstacleType.Rock:
                    GameObject RockGO = getRandomObstacle(RockPrefabList);
                    obstacle = RockGO.instantiate(RockObstacleNetwork, obstacleType, obstacleId);
                    break;
                case ObstacleType.Reward:
                    GameObject RewardGO = getRandomObstacle(RewardPrefabList);
                    obstacle = RewardGO.instantiate(RewardObstacleNetwork, obstacleType, obstacleId);
                    break;
                case ObstacleType.Goal:
                    obstacle = GoalPrefab.instantiate(RewardObstacleNetwork, obstacleType, obstacleId);
                    break;
                default:
                    throw new System.Exception("Not a valid obstacle type: " + obstacleType);
            }
            return obstacle;
        }
    }

}
