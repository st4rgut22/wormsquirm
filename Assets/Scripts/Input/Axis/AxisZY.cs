using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisZY : Axis
{
    public AxisZY(int movement) : base(movement)
    {
    }

    public override Direction getDirectionAlongPlane(Direction direction)
    {
        if (movement == Move.CLOCKWISE)
        {
            if (direction == Direction.Forward) return Direction.Up;
            else if (direction == Direction.Up) return Direction.Back;
            else if (direction == Direction.Back) return Direction.Down;
            else if (direction == Direction.Down) return Direction.Forward;
            else
            {
                return direction;
            }
        }
        else
        {
            if (direction == Direction.Forward) return Direction.Down;
            else if (direction == Direction.Down) return Direction.Back;
            else if (direction == Direction.Back) return Direction.Up;
            else if (direction == Direction.Up) return Direction.Forward;
            else
            {
                return direction;
            }
        }
    }
}
