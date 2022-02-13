using UnityEngine;

public static class Vector
{
    /**
     * Return the biggest coordinate, irrespective of sign
     */
    public static float getMaxValue(this Vector3 thisObj)
    {
        float x = Mathf.Abs(thisObj.x);
        float y = Mathf.Abs(thisObj.y);
        float z = Mathf.Abs(thisObj.z);
        float big = x > y ? x : y;
        float biggest = big > z ? big : z;
        return biggest;
    }

    /**
     * Check if two cells are adjacent
     */
    public static bool isAdjacent(this Vector3Int thisCell, Vector3Int otherCell)
    {
        return Vector3Int.Distance(thisCell, otherCell) <= 1;
    }
}