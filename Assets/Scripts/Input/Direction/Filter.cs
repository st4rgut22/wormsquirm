using System.Collections.Generic;

namespace Dir
{
    public class Filter
    {
        public System.Predicate<Direction> FilterDirectionMethod;

        // list of directions to exclude
        public static List<Direction> blacklistDirections;

        /**
         * Only include directions in a list that are positive
         * 
         * @direction  the direction to check criteria against
         */
        public static bool filterPositiveDirection(Direction direction)
        {
            return !Base.isDirectionNegative(direction);            
        }

        /**
         * Only include directions in a list that are negative
         * 
         * @direction  the direction to check criteria against
         */
        public static bool filterNegativeDirection(Direction direction)
        {
            return Base.isDirectionNegative(direction);

        }

        /**
         * Only include directions not listed in the blacklist
         * 
         * @directionList   the direction to check criteria against
         * @directionBlacklist   a list of directions NOT to include
         */
        public static bool isContainBlacklistDirection(Direction direction)
        {
            return !blacklistDirections.Contains(direction);
        }
    }

}