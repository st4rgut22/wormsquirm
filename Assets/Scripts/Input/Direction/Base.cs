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

        /**
         * Determine if directions are adjacent, if opposites are present then return false
         */
        public static bool areDirectionsAdjacent(int holeCount, List<Direction> directionList)
        {
            if (holeCount == 3)
            {
                return isHolesAdjacent(directionList, 1); // find 1 pair of opposite sides
            }
            else if (holeCount == 4)
            {
                return isHolesAdjacent(directionList, 2); // find 2 pairs of opposite sides
            }
            return true;
        }

        /**
         * Count the numbner of opposite hole pairs to determine what kind of orientation to use
         */
        private static bool isHolesAdjacent(List<Direction> directionList, int totalOppositePairCount)
        {
            int oppositeCount = 0;
            List<Direction> excludeDirections = new List<Direction>();
            foreach (Direction dir in directionList)
            {
                if (!excludeDirections.Contains(dir))
                {
                    Direction oppDir = getOppositeDirection(dir);
                    if (directionList.Contains(oppDir))
                    {
                        oppositeCount += 1;
                        excludeDirections.Add(dir);
                        excludeDirections.Add(oppDir);
                    }
                }
            }
            return oppositeCount != totalOppositePairCount; // not opposite means it is adjacent
        }

        /**
         * Filter directions using a method with specific criteria
         * 
         * @funcToRun       a method specifing the filter criteria
         * @directionList   the list of directions to filter
         */
        public static List<Direction> filterDirections(System.Predicate<Direction> filterDirectionMethod, List<Direction> directionList)
        {
            List<Direction> filteredDirectionList = new List<Direction>(directionList.FindAll(filterDirectionMethod));
            return filteredDirectionList;
        }

        /**
         * Get the four directions that are perpendicular to a direction
         */
        public static List<Direction> getPerpendicularDirections(Direction direction)
        {
            List<Direction> perpendicularDirArr;
            switch (direction)
            {
                case Direction.Up:
                    perpendicularDirArr = new List<Direction>() { Direction.Right, Direction.Back, Direction.Left, Direction.Forward };
                    break;
                case Direction.Right:
                    perpendicularDirArr = new List<Direction>() { Direction.Back, Direction.Up, Direction.Forward, Direction.Down };
                    break;
                case Direction.Left:
                    perpendicularDirArr = new List<Direction>() { Direction.Up, Direction.Back, Direction.Down, Direction.Forward };
                    break;
                case Direction.Forward:
                    perpendicularDirArr = new List<Direction>() { Direction.Up, Direction.Left, Direction.Down, Direction.Right };
                    break;
                case Direction.Back:
                    perpendicularDirArr = new List<Direction>() { Direction.Right, Direction.Up, Direction.Left, Direction.Down };
                    break;
                case Direction.Down:
                    perpendicularDirArr = new List<Direction>() { Direction.Right, Direction.Forward, Direction.Left, Direction.Back };
                    break;
                default:
                    throw new System.Exception("Not a valid direction " + direction);
            }
            return perpendicularDirArr;
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