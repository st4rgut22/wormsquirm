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
            Obstacle obstacle = new Obstacle(obstacleGO, type, obstacleId);
            return obstacle;
        }

        /**
         * Instantiate a obstacle from a list
         * 
         * @thisObstacleGO         the parent of a list of obstacles to choose randomly from
         */
        public static Obstacle instantiateRandom(this Transform thisObstacleGO, Transform obstacleNetwork, ObstacleType type, string obstacleId)
        {
            int totalObstacles = thisObstacleGO.childCount;
            int randIndex = Random.Range(0, totalObstacles - 1);
            GameObject obstaclePrefab = thisObstacleGO.GetChild(randIndex).gameObject;
            GameObject obstacleGO = GameObject.Instantiate(obstaclePrefab, obstacleNetwork);
            obstacleGO.SetActive(true); // a prefab from a list will be inactive
            Obstacle obstacle = new Obstacle(obstacleGO, type, obstacleId);
            return obstacle;
        }
    }
}