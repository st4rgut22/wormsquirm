using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dir
{
    public class Base
    {
        public static List<Direction> directionList = new List<Direction>()
        {
            Direction.Up, Direction.Right, Direction.Left, Direction.Forward, Direction.Back, Direction.Down
        };

        /**
         * Determine whether direction is positive along an axis
         */
        public static bool isDirectionNegative(Direction direction)
        {
            if (direction == Direction.Up || direction == Direction.Forward || direction == Direction.Right)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static Direction getOppositeDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Right:
                    return Direction.Left;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Forward:
                    return Direction.Back;
                case Direction.Back:
                    return Direction.Forward;
                case Direction.Down:
                    return Direction.Up;
                default:
                    throw new System.Exception("Not a valid direction " + direction);
            }
        }
    }
}