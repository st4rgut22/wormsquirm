using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dir
{
    public class Input
    {
        public static Direction getChangedDirection(Direction direction, InputKey key)
        {
            switch (direction)
            {
                case Direction.Right:
                    return mapWASDToDirection(key, Direction.Up, Direction.Back, Direction.Down, Direction.Forward);
                case Direction.Back:
                    return mapWASDToDirection(key, Direction.Down, Direction.Left, Direction.Up, Direction.Right);
                case Direction.Left:
                    return mapWASDToDirection(key, Direction.Down, Direction.Forward, Direction.Up, Direction.Back);
                case Direction.Forward:
                    return mapWASDToDirection(key, Direction.Up, Direction.Right, Direction.Down, Direction.Left);
                case Direction.Up:
                    return mapWASDToDirection(key, Direction.Back, Direction.Left, Direction.Forward, Direction.Right);
                case Direction.Down:
                    return mapWASDToDirection(key, Direction.Forward, Direction.Right, Direction.Back, Direction.Left);
                default:
                    throw new System.Exception("not a valid direction " + direction);
            }
        }

        /**
         * Decide the changed direction based off of user input 
         */
        public static Direction mapWASDToDirection(InputKey key, Direction northDir, Direction westDir, Direction southDir, Direction eastDir)
        {
            switch (key.keyCode)
            {
                case KeyCode.W:
                    return northDir;
                case KeyCode.S:
                    return southDir;
                case KeyCode.A:
                    return westDir;
                case KeyCode.D:
                    return eastDir;
                default:
                    throw new System.Exception("Not a valid keycode " + key.keyCode);
            }
        }
    }

}