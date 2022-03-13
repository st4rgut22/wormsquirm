using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUtility : MonoBehaviour
{
    private static float CENTER_Y_OFFSET = 0.5f;

    /*
     * Get center of a cell
     * 
     * @cellPos the grid cell position in integers
     */
    public static Vector3 getCellPos(Vector3Int cellPos)
    {
        float offsetY = cellPos.y + CENTER_Y_OFFSET;
        Vector3 obstaclePos = new Vector3(cellPos.x, offsetY, cellPos.z);
        return obstaclePos;
    }
}
