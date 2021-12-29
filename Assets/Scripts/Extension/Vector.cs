using UnityEngine;

public static class Vector
{
    /**
     * Convert position to integer
     * 
     * @direction direction of travel influences the cell position. 
     * If in negative direction then position use Math.Floor to get next cell
     * if in positive direction use Math.ceil to get next cell
     */
    public static Vector3Int castToVector3Int(this Vector3 thisObj)
    {
        return new Vector3Int((int)thisObj.x, (int)thisObj.y, (int)thisObj.z);
    }

    /**
     * Get the next cell using the current direction
     */
    public static Vector3Int getNextVector3Int(this Vector3Int thisObj, Direction direction)
    {
        Vector3 vector = getNextVector(thisObj, direction);
        Vector3Int cellLoc = castToVector3Int(vector);
        return cellLoc;
    }

    public static Vector3Int getNextVector3(this Vector3 thisObj, Direction direction)
    {
        Vector3 vector = getNextVector(thisObj, direction);
        Vector3Int cellLoc = vector.castToVector3Int();
        return cellLoc;
    }

    private static Vector3 getNextVector(Vector3 vector, Direction direction)
    {
        if (Dir.Base.isDirectionNegative(direction))
        {
            if (direction == Direction.Left)
            {
                return new Vector3(--vector.x, vector.y, vector.z);
            }
            else if (direction == Direction.Down)
            {
                return new Vector3(vector.x, --vector.y, vector.z);
            }
            else
            {
                return new Vector3(vector.x, vector.y, --vector.z);
            }
        }
        else
        {
            if (direction == Direction.Right)
            {
                return new Vector3(++vector.x, vector.y, vector.z);
            }
            else if (direction == Direction.Up)
            {
                return new Vector3(vector.x, ++vector.y, vector.z);
            }
            else
            {
                return new Vector3(vector.x, vector.y, ++vector.z);
            }
        }
    }
}