using UnityEngine;

namespace Dir
{
    public class Vector
    {
        /**
         * Get the next cell using the current direction
         */
        public static Vector3Int getNextVector3Int(Vector3Int thisObj, Direction direction)
        {
            Vector3 vector = getNextVector(thisObj, direction);
            Vector3Int cellLoc = castToVector3Int(vector);
            return cellLoc;
        }

        private static Vector3 getNextVector(Vector3 vector, Direction direction)
        {
            if (Base.isDirectionNegative(direction))
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

        /**
         * Convert position to integer
         */
        public static Vector3Int castToVector3Int(Vector3 thisObj)
        {
            Vector3Int castedVector = new Vector3Int((int)thisObj.x, (int)thisObj.y, (int)thisObj.z);
            Debug.Log("cast vector " + thisObj + " is " + castedVector);
            return castedVector;
        }

        /**
         * Combine normalized vectors for each direction to get offset
         */
        public static Vector3 getOffsetFromDirections(Direction direction1, Direction direction2)
        {
            Vector3 unitVector1 = Dir.CellDirection.getUnitVectorFromDirection(direction1);
            Vector3 unitVector2 = Dir.CellDirection.getUnitVectorFromDirection(direction2);
            Vector3 offset = (unitVector1 + unitVector2) / 2;
            return offset;
        }


        public static Vector3Int getNextCellFromDirection(Vector3Int cellPosition, Direction direction)
        {
            Vector3 unitVector = Dir.CellDirection.getUnitVectorFromDirection(direction);
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

        public static Vector3 substituteVector3FromDirection(Direction direction, Vector3 original, float substitute)
        {
            Vector3 substitudedVector3 = new Vector3(original.x, original.y, original.z);
            if (direction == Direction.Up || direction == Direction.Down)
            {
                substitudedVector3.y = substitute;
            }
            else if (direction == Direction.Forward || direction == Direction.Back)
            {
                substitudedVector3.z = substitute;
            }
            else if (direction == Direction.Right || direction == Direction.Left)
            {
                substitudedVector3.x = substitute;
            }
            else
            {
                throw new System.Exception("Not a valid direction " + direction);
            }
            return substitudedVector3;
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