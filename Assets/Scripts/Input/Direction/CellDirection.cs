using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dir
{
    public class CellDirection
    {
        public static Vector3 getUnitVectorFromDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Vector3Int.up;
                case Direction.Right:
                    return Vector3Int.right;
                case Direction.Left:
                    return Vector3Int.left;
                case Direction.Forward:
                    return Vector3Int.forward;
                case Direction.Back:
                    return Vector3Int.back;
                case Direction.Down:
                    return Vector3Int.down;
                default:
                    throw new System.Exception("Not a valid direction " + direction);
            }
        }

        public static Direction getDirectionFromUnitVector(Vector3Int unitVector)
        {
            if (unitVector.Equals(Vector3Int.up))
            {
                return Direction.Up;
            }
            else if (unitVector.Equals(Vector3Int.down))
            {
                return Direction.Down;
            }
            else if (unitVector.Equals(Vector3Int.left))
            {
                return Direction.Left;
            }
            else if (unitVector.Equals(Vector3Int.right))
            {
                return Direction.Right;
            }
            else if (unitVector.Equals(Vector3Int.forward))
            {
                return Direction.Forward;
            }
            else if (unitVector.Equals(Vector3Int.back))
            {
                return Direction.Back;
            }
            else
            {
                throw new System.Exception("Not a valid unit vector " + unitVector);
            }
        }

        /**
         * Get direction of a move from fromCell to toCell
         * 
         * @fromCell the cell being left in grid cell space
         * @toCell the destination cell in the grid
         */
        public static Direction getDirectionFromCells(Vector3Int fromCell, Vector3Int toCell)
        {
            Vector3Int moveVector = toCell - fromCell;
            Direction moveDirection = getDirectionFromUnitVector(moveVector);
            return moveDirection;
        }
    }
}
