using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationManager : MonoBehaviour
{
    private static Quaternion NORTH_ROTATION = Quaternion.Euler(0, 0, 0);
    private static Quaternion EAST_ROTATION = Quaternion.Euler(90, 0, 0);
    private static Quaternion WEST_ROTATION = Quaternion.Euler(-90, 0, 0);
    private static Quaternion SOUTH_ROTATION = Quaternion.Euler(180, 0, 0);

    /**
     * Return a normalized vector used to calculate the position of a GO
     */
    public static Vector3 getDirectionFromRotation(Quaternion rotation)
    {
        if (rotation == NORTH_ROTATION)
            return Vector3.up;
        else if (rotation == EAST_ROTATION)
            return Vector3.right;
        else if (rotation == WEST_ROTATION)
            return Vector3.left;
        else if (rotation == SOUTH_ROTATION)
            return Vector3.down;
        else
        {
            throw new System.Exception("there is no rotation " + rotation);
        }
    }

    /**
     * Get rotation of a GO from one of four directions
     */
    public static Quaternion getRotationFromDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                return NORTH_ROTATION;
            case Direction.East:
                return EAST_ROTATION;
            case Direction.West:
                return WEST_ROTATION;
            case Direction.South:
                return SOUTH_ROTATION;
            default:
                throw new System.Exception("there is no direction " + direction);
        }
    }
}
