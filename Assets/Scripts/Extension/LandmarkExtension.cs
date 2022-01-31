using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public static class LandmarkExtension
    {
        public static GameObject instantiate(this GameObject thisObj, Vector3Int cellPos, Transform obstacleNetwork)
        {
            Vector3 obstaclePos = MapUtility.getCellPos(cellPos);
            GameObject obstacle = GameObject.Instantiate(thisObj, obstaclePos, Quaternion.identity, obstacleNetwork);
            return obstacle;
        }
    }

}