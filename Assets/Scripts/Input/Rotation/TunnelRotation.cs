using UnityEngine;

public class TunnelRotation
{
    private static Quaternion UP_ROTATION = Quaternion.Euler(0, 0, 0);
    private static Quaternion FORWARD_ROTATION = Quaternion.Euler(90, 0, 0);
    private static Quaternion BACK_ROTATION = Quaternion.Euler(-90, 0, 0);
    private static Quaternion DOWN_ROTATION = Quaternion.Euler(180, 0, 0);
    private static Quaternion RIGHT_ROTATION = Quaternion.Euler(90, 90, 0);
    private static Quaternion LEFT_ROTATION = Quaternion.Euler(90, -90, 0);

    /**
     * Return a normalized vector given a direction
     */
    public static Vector3 getUnitVectorFromDirection(Direction direction)
    {
        Quaternion rotation = getRotationFromDirection(direction);
        Vector3 unitVector = getUnitVectorFromRotation(rotation);
        return unitVector;
    }

    /**
     * Return a normalized vector used to calculate the position of a GO
     */
    public static Vector3 getUnitVectorFromRotation(Quaternion rotation)
    {
        if (rotation == UP_ROTATION)
            return Vector3.up;
        else if (rotation == RIGHT_ROTATION)
            return Vector3.right;
        else if (rotation == LEFT_ROTATION)
            return Vector3.left;
        else if (rotation == DOWN_ROTATION)
            return Vector3.down;
        else if (rotation == FORWARD_ROTATION)
            return Vector3.forward;
        else if (rotation == BACK_ROTATION)
            return Vector3.back;
        else
        {
            throw new System.Exception("there is no rotation " + rotation);
        }
    }

    /**
     * Return the direction of a GO using rotation
     */
    public static Direction getDirectionFromRotation(Quaternion rotation)
    {
        if (rotation == UP_ROTATION)
            return Direction.Up;
        else if (rotation == RIGHT_ROTATION)
            return Direction.Right;
        else if (rotation == LEFT_ROTATION)
            return Direction.Left;
        else if (rotation == DOWN_ROTATION)
            return Direction.Down;
        else if (rotation == FORWARD_ROTATION)
            return Direction.Forward;
        else if (rotation == BACK_ROTATION)
            return Direction.Back;
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
            case Direction.Up:
                return UP_ROTATION;
            case Direction.Down:
                return DOWN_ROTATION;
            case Direction.Left:
                return LEFT_ROTATION;
            case Direction.Right:
                return RIGHT_ROTATION;
            case Direction.Back:
                return BACK_ROTATION;
            case Direction.Forward:
                return FORWARD_ROTATION;
            default:
                throw new System.Exception("there is no direction " + direction);
        }
    }
}
