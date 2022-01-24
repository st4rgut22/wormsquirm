using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rotation
{
    public class DeadEndRot : Rotation
    {
        private static Quaternion UP_ROTATION = Quaternion.Euler(0, 0, 0);
        private static Quaternion DOWN_ROTATION = Quaternion.Euler(0, 0, 180);
        private static Quaternion FORWARD_ROTATION = Quaternion.Euler(0, 90, 90);
        private static Quaternion BACK_ROTATION = Quaternion.Euler(0, -90, 90);
        private static Quaternion RIGHT_ROTATION = Quaternion.Euler(0, 0, -90);
        private static Quaternion LEFT_ROTATION = Quaternion.Euler(0, 0, 90);

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

}