using System.Collections.Generic;
using UnityEngine;

namespace Dir
{

    /**
     * On input the changed direction depends on previous key press and previous direction. This is because user would expect consistent behavior based on their last keypress.
     * For example if user presses "W" and the player rotates along counterclockwise along the X axis, the user would expect the same behavior on pressing W again. 
     * This way we don't have to hardcode rules for the different types of keys pressed, instead rely on consistent behavior conditioned on intent to continue rotating 
     * about axis or changing to a different axis of rotation
     */
    public class Input
    {
        /**
         * Get the changed direction on user input
         * 
         * @direction       the current direction of the player
         * @prevDirection   the previous direction of the player
         * @key             the key pressed to initiate direction change
         * @prevKey         the previous key pressed
         */
        public static Direction getChangedDirection(Direction direction, Direction prevDirection, InputKey key, InputKey prevKey)
        {
            Direction finalDirection; // by default set to direction in case prevDirection is None just go in the specified direction

            if (prevDirection != Direction.None)
            {
                Direction oppositeDirection = Base.getOppositeDirection(prevDirection);
                Debug.Log("opposite direction is " + oppositeDirection);
                if (key == prevKey)
                {
                    finalDirection = oppositeDirection;
                }
                else
                {
                    finalDirection = getDirectionAlongNewAxisOfRotation(key, direction, prevDirection, oppositeDirection);
                }
            }
            else
            {
                finalDirection = getInitialDirectionChange(direction, key);
            }
            return finalDirection;
        }

        /**
         * If prevDir is defined but is None, then hardcode the next direction. Once prevDir != None no need to use this function. 
         * It is only used once on second user input
         * 
         * @direction   the current direction
         * @key         the pressed key
         */
        public static Direction getInitialDirectionChange(Direction direction, InputKey key)
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
         * Map the keycode to the changed direction
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

        /**
         * Get the direction along a new axis of rotation (meaning not rotating along the same axis as the last direction change)
         * Accomplish this by excluding the directions made on the previous axis (prevDir and oppPrevDir)
         * Next, filter the directions along new axis by the sign of direction, if negative choose the negative direction along new axis
         * 
         * @key                 the key pressed
         * @direction           the current direction
         * @previousDirection   the previous direction
         * @oppositeDirection   the opposite direction of the previous direction (eg if prevDir is South opposite is North)
         */
        private static Direction getDirectionAlongNewAxisOfRotation(InputKey key, Direction direction, Direction previousDirection, Direction oppositeDirection)
        {
            List<Direction> perpendicularDirections = Base.getPerpendicularDirections(direction);

            System.Predicate<Direction> signDirectionsFilter;
            if (key.isPositive) 
            {
                signDirectionsFilter = Filter.filterPositiveDirection;
            }
            else
            {
                signDirectionsFilter = Filter.filterNegativeDirection;
            }
            List<Direction> directionsFilteredBySign = Base.filterDirections(signDirectionsFilter, perpendicularDirections);
            Filter.blacklistDirections = new List<Direction>() { oppositeDirection, previousDirection };
            List<Direction> filteredDirections = Base.filterDirections(Filter.isContainBlacklistDirection, directionsFilteredBySign);
            if (filteredDirections.Count != 1)
            {
                throw new System.Exception("Should only be one direction left after filtering, but got " + filteredDirections.Count);
            }
            return filteredDirections[0];
        }
    }
}