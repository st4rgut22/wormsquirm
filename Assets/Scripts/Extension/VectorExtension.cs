using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtension
{
    const float HALF_CELL = 0.5f;

    public static Vector3Int castToVector3Int(this Vector3 thisObj)
    {
        Vector3Int castedVector = new Vector3Int((int)thisObj.x, (int)thisObj.y, (int)thisObj.z);
        return castedVector;
    }

    /**
     * The distance between two cells is the maximum distance along an axis
     * 
     * @direction       get distance from origin cell to current cell along this direction
     */
    public static int distToVector3Int(this Vector3Int curCellPos, Vector3Int destCellPos, Direction direction)
    {
        int destCellAxisPos = (int) Dir.Vector.getAxisPositionFromDirection(direction, destCellPos);
        int curCellAxisPos = (int)Dir.Vector.getAxisPositionFromDirection(direction, curCellPos);
        int absDist = Mathf.Abs(destCellAxisPos - curCellAxisPos);
        return absDist;
    }

    public static Vector3 getCellCenter(this Vector3Int cellPos)
    {
        return new Vector3(cellPos.x, cellPos.y + HALF_CELL, cellPos.z);
    }
}
