using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisXZ : Axis
{
    public AxisXZ(int movement) : base(movement)
    {
    }

    public override Direction getDirectionAlongPlane(Direction direction)
    {
        if (movement == Move.CLOCKWISE)
        {
            if (direction == Direction.Left) return Direction.Back;
            else if (direction == Direction.Back) return Direction.Right;
            else if (direction == Direction.Right) return Direction.Forward;
            else if (direction == Direction.Forward) return Direction.Left;
            else
            {
                return direction;
            }
        }
        else
        {
            if (direction == Direction.Left) return Direction.Forward;
            else if (direction == Direction.Back) return Direction.Left;
            else if (direction == Direction.Right) return Direction.Back;
            else if (direction == Direction.Forward) return Direction.Right;
            else
            {
                return direction;
            }
        }
    }
}
