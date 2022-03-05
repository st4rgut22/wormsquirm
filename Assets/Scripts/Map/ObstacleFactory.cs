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
        private GameObject RockObstaclePrefab;

        [SerializeField]
        private Transform RewardObstacleNetwork;

        [SerializeField]
        private GameObject GoalPrefab;

        /**
         * Get the obstacle using the obstacle's type
         */
        public Obstacle getObstacle(ObstacleType obstacleType, string obstacleId)
        {
            Obstacle obstacle;
            switch (obstacleType)
            {
                case ObstacleType.Rock:
                    obstacle = RockObstaclePrefab.instantiate(RockObstacleNetwork, obstacleType, obstacleId);
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
