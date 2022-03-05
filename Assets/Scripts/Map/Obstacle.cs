using UnityEngine;

public enum ObstacleType
{
    Rock,
    Goal,
    PlayerWorm,
    AIWorm
}

/**
 * Initial position and orientation for obstacles
 */
public class Obstacle
{
    public GameObject obstacleObject { get; private set; }  // the gameobject instance of obstacle
    public ObstacleType obstacleType { get; private set; }  // the type of obstacle
    public string obstacleId { get; private set; }          // the id of the obstacle
    public Vector3Int obstacleCell { get; private set; }    // the cell containing the obstacle, might be randomly generated

    public Obstacle(GameObject obstacleObject, ObstacleType obstacleType, string obstacleId)
    {
        this.obstacleObject = obstacleObject;
        this.obstacleType = obstacleType;
        this.obstacleId = obstacleId;
    }

    /**
     * Set the cell the obstacle is in
     */
    public void setObstacleCell(Vector3Int cell)
    {
        obstacleCell = cell;
    }
}
