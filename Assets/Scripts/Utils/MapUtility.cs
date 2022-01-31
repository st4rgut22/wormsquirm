using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUtility : MonoBehaviour
{
    private static float CENTER_Y_OFFSET = 0.5f;

    /**
     * Transform position of obstacle along Y axis to align with tunnel coordinate system
     * 
     * @cellAxisPos cell count from the origin along Y axis
     */
    private static float getYOffsetPosition(int cellAxisPos)
    {
        if (cellAxisPos > 0)
        {
            return cellAxisPos + CENTER_Y_OFFSET;
        }
        else
        {
            return cellAxisPos - CENTER_Y_OFFSET;
        }
    }
    /*
     * Get center of a cell
     * 
     * @cellPos the grid cell position in integers
     */
    public static Vector3 getCellPos(Vector3Int cellPos)
    {
        float yPos = getYOffsetPosition(cellPos.y);
        Vector3 obstaclePos = new Vector3(cellPos.x, yPos, cellPos.z);
        return obstaclePos;
    }
}
