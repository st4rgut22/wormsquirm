using UnityEngine;

public static class Vector
{
    /**
     * Convert position to integer to mark the cell position of next cell
     * 
     * @direction direction of travel influences the cell position. 
     * If in negative direction then position use Math.Floor to get next cell
     * if in positive direction use Math.ceil to get next cell
     */
    public static Vector3Int castToVector3Int(this Vector3 thisObj, Direction direction)
    {
        if (Dir.Base.isDirectionNegative(direction))
        {
            return new Vector3Int(Mathf.FloorToInt(thisObj.x), Mathf.FloorToInt(thisObj.y), Mathf.FloorToInt(thisObj.z));
        }
        else
        {
            return new Vector3Int(Mathf.CeilToInt(thisObj.x), Mathf.CeilToInt(thisObj.y), Mathf.CeilToInt(thisObj.z));
        }
    }

    /**
     * Get the next cell using the current direction
     */
    public static Vector3Int getNextVector3Int(this Vector3Int thisObj, Direction direction)
    {
        if (Dir.Base.isDirectionNegative(direction))
        {
            if (direction == Direction.Left)
            {
                return new Vector3Int(--thisObj.x, thisObj.y, thisObj.z);
            }
            else if (direction == Direction.Down)
            {
                return new Vector3Int(thisObj.x, --thisObj.y, thisObj.z);
            }
            else
            {
                return new Vector3Int(thisObj.x, thisObj.y, --thisObj.z);
            }
        }
        else
        {
            if (direction == Direction.Right)
            {
                return new Vector3Int(++thisObj.x, thisObj.y, thisObj.z);
            }
            else if (direction == Direction.Up)
            {
                return new Vector3Int(thisObj.x, ++thisObj.y, thisObj.z);
            }
            else
            {
                return new Vector3Int(thisObj.x, thisObj.y, ++thisObj.z);
            }
        }
    }
}