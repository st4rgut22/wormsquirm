using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public static class ObstacleExtension
    {
        /**
         * Instantiate a obstacle
         * 
         * @thisObstacleGO         the obstacle to instantiate
         */
        public static Obstacle instantiate(this GameObject thisObstacleGO, Transform obstacleNetwork, ObstacleType type, string obstacleId)
        {
            GameObject obstacleGO = GameObject.Instantiate(thisObstacleGO, obstacleNetwork); // rotation is Quaternion.identity
            ObstaclePrefab obstaclePrefab = obstacleGO.GetComponent<ObstaclePrefab>();
            Obstacle obstacle = new Obstacle(obstacleGO, type, obstacleId);
            obstaclePrefab.setObstacle(obstacle);
            return obstacle;
        }
    }
}