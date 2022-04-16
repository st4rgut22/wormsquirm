using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DirectionExtension
{
    public static Vector3 getVectorFromDirection(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector3.up;
            case Direction.Down:
                return Vector3.down;
            case Direction.Right:
                return Vector3.right;
            case Direction.Left:
                return Vector3.left;
            case Direction.Forward:
                return Vector3.forward;
            case Direction.Back:
                return Vector3.back;
            default:
                throw new System.Exception("Not a valid direction " + direction);
        }
    }
}
