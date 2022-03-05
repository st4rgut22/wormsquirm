using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public static class ObstacleExtension
    {
        /**
         * Instantiate a rock obstacle
         * 
         * @thisObj         the obstacle script attached to the gameobject Rock
         */
        public static Obstacle instantiate(this GameObject thisObstacleGO, Transform obstacleNetwork, ObstacleType type, string obstacleId)
        {
            GameObject obstacleGO = GameObject.Instantiate(thisObstacleGO, obstacleNetwork); // rotation is Quaternion.identity
            Obstacle obstacle = new Obstacle(obstacleGO, type, obstacleId);
            return obstacle;
        }
    }
}