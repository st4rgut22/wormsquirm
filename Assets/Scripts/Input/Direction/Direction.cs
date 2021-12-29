using UnityEngine;

namespace Dir
{
    public class Vector
    {
        /**
         * Combine normalized vectors for each direction to get offset
         */
        public static Vector3 getOffsetFromDirections(Direction direction1, Direction direction2)
        {
            Vector3 unitVector1 = getUnitVectorFromDirection(direction1);
            Vector3 unitVector2 = getUnitVectorFromDirection(direction2);
            Vector3 offset = (unitVector1 + unitVector2) / 2;
            return offset;
        }


        public static Vector3Int getNextCellFromDirection(Vector3Int cellPosition, Direction direction)
        {
            Vector3 unitVector = getUnitVectorFromDirection(direction);
            Vector3Int unitVectorInt = new Vector3Int((int)unitVector.x, (int)unitVector.y, (int)unitVector.z);
            return cellPosition + unitVectorInt;
        }

        /**
         * Get the position along the axis that matches the direction
         */
        public static float getAxisPositionFromDirection(Direction direction, Vector3 position)
        {
            if (direction == Direction.Up || direction == Direction.Down)
            {
                return position.y;
            }
            if (direction == Direction.Forward || direction == Direction.Back)
            {
                return position.z;
            }
            if (direction == Direction.Left || direction == Direction.Right)
            {
                return position.x;
            }
            throw new System.Exception("Not a valid direction " + direction);
        }

        public static Vector3Int substituteVector3FromDirection(Direction direction, Vector3Int original, Vector3Int substitute)
        {
            Vector3Int substitudedVector3 = new Vector3Int(original.x, original.y, original.z);
            if (direction == Direction.Up || direction == Direction.Down)
            {
                substitudedVector3.y = substitute.y;
            }
            else if (direction == Direction.Forward || direction == Direction.Back)
            {
                substitudedVector3.z = substitute.z;
            }
            else if (direction == Direction.Right || direction == Direction.Left)
            {
                substitudedVector3.x = substitute.x;
            }
            else
            {
                throw new System.Exception("Not a valid direction " + direction);
            }
            return substitudedVector3;
        }

        public static Vector3 getUnitVectorFromDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Vector3.up;
                case Direction.Right:
                    return Vector3.right;
                case Direction.Left:
                    return Vector3.left;
                case Direction.Forward:
                    return Vector3.forward;
                case Direction.Back:
                    return Vector3.back;
                case Direction.Down:
                    return Vector3.down;
                default:
                    throw new System.Exception("Not a valid direction " + direction);
            }
        }

        public static float getAxisScaleFromDirection(Direction direction, Vector3 vector)
        {
            if (direction == Direction.Up || direction == Direction.Down)
            {
                return vector.y;
            }
            else if (direction == Direction.Right || direction == Direction.Left)
            {
                return vector.x;
            }
            else if (direction == Direction.Back || direction == Direction.Forward)
            {
                return vector.z;
            }
            else
            {
                throw new System.Exception("Not a valid direction " + direction);
            }
        }
    }
}