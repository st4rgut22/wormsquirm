using System;
using System.Collections.Generic;
using UnityEngine;


namespace Tunnel
{
    /**
     * Determines what action should be taken by tunnel manager given worm's position
     */
    public class ActionPoint : MonoBehaviour
    {
        public static ActionPoint instance;

        private static Dictionary<Direction, float> decisionPointBoundary;

        private void Awake()
        {
            instance = this;
        }

        /**
         * Decide if player movement should trigger changes to the tunnel network such as tunnel creation
         * 
         * @tunnel          the current tunnel
         * @position        the position of the player
         * @curDir          the current direction of the player
         * @torqueDirection the direction in which torque event happened
         * @returns         the direction in which decision is made
         */
        public Direction getDirectionDecisionBoundaryCrossed(Tunnel tunnel, Vector3 position, Direction curDir, Direction torqueDirection)
        {
            Direction oppDir = Dir.Base.getOppositeDirection(curDir);
            decisionPointBoundary = new Dictionary<Direction, float>();
            setBoundaryPoints(tunnel.center);

            bool isDecision = isDecisionBoundaryCrossed(position, torqueDirection);
            bool isAlongDecisionAxis = torqueDirection == curDir || torqueDirection == oppDir;
            if (isAlongDecisionAxis)
            {
                throw new Exception("torque should never occur in same (or opposite) direction as worm which is " + curDir + " and " + oppDir);
            }

            Direction decisionDirection = isDecision ? torqueDirection : Direction.None;
            return decisionDirection;
        }

        private bool isDecisionBoundaryCrossed(Vector3 position, Direction direction)
        {
            print("position of ring is is " + position);
            float axisPosition = Dir.Vector.getAxisPositionFromDirection(direction, position);
            return isTriggerDecision(axisPosition, direction);
        }

        /**
         * Decisions involve creating tunnel pieces and only apply to non-straight tunnels that cannot scale.
         * Check that direction is along a non-scalable axis
         */
        public bool isDirectionAlongDecisionAxis(Tunnel tunnel, Direction direction)
        {
            if (tunnel.type != Type.Name.STRAIGHT)
            {
                return true;
            }
            else
            {
                Direction growthDir = ((Straight)tunnel).growthDirection;
                Direction growthOppDir = Dir.Base.getOppositeDirection(growthDir);

                if (direction != growthDir && direction != growthOppDir) // check for movement perpendicular to  growth axis in a straight tunnel.
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /**
         * Check if player triggered a decision by crossing a boundary line
         */
        private bool isTriggerDecision(float position, Direction direction)
        {
            float decisionBoundary = decisionPointBoundary[direction];

            if (Dir.Base.isDirectionNegative(direction))
            {
                if (position <= decisionBoundary)
                {
                    return true;
                }
            }
            else
            {
                if (position >= decisionBoundary)
                {
                    return true;
                }
            }
            return false;
        }

        /**
         * Get the distance along a decision axis
         * @direction: direction from center of tunnel
         * @tunnel: tunnel getting distance along axis
         */
        private float getDecisionBoundary(Direction direction, Vector3 tunnelCenter)
        {
            float offset = Tunnel.INNER_WALL_OFFSET - Worm.WormBody.WORM_BODY_THICKNESS;
            Vector3 centerOffset = Dir.CellDirection.getUnitVectorFromDirection(direction) * offset;

            float offsetAlongAxis = Dir.Vector.getAxisPositionFromDirection(direction, centerOffset);
            float positionAlongAxis = Dir.Vector.getAxisPositionFromDirection(direction, tunnelCenter);
            float decisionBoundary = offsetAlongAxis + positionAlongAxis;

            return decisionBoundary;
        }

        /**
         * Set the boundaries lines that will trigger a tunnel action. 
         */
        private void setBoundaryPoints(Vector3 tunnelCenter)
        {
            Dir.Base.directionList.ForEach((Direction direction) =>
            {
                decisionPointBoundary[direction] = getDecisionBoundary(direction, tunnelCenter);
            });
        }
    }
}