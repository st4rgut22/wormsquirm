using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePrefab : MonoBehaviour
{
    protected Obstacle obstacle;

    public void setObstacle(Obstacle obstacle)
    {
        this.obstacle = obstacle;
    }

    public string getObstacleId()
    {
        return obstacle.obstacleId;
    }
}
