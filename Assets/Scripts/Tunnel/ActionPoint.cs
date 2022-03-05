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

        private static Dictionary<Direction, float> innerWallBoundary;

        private const float DECISION_BOUNDARY_TOLERANCE = 0.1f; // the distance from inner wall (aka decision boundary) that a turn will be triggered. The bigger it is, the more likely to turn

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
            innerWallBoundary = new Dictionary<Direction, float>();
            setBoundaryPoints(tunnel.center);

            bool isDecision = isInnerWallBoundaryCrossed(position, torqueDirection);
            print("decision boundary crossed? " + isDecision);
            bool isAlongDecisionAxis = torqueDirection == curDir || torqueDirection == oppDir;
            if (isAlongDecisionAxis)
            {
                throw new Exception("torque should never occur in same (or opposite) direction as worm which is " + curDir + " and " + oppDir);
            }

            Direction decisionDirection = isDecision ? torqueDirection : Direction.None;
            return decisionDirection;
        }

        private bool isInnerWallBoundaryCrossed(Vector3 position, Direction direction)
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
            float decisionBoundary = innerWallBoundary[direction];
            float distanceFromDecisionBoundary = Mathf.Abs(position - decisionBoundary);

            print("distance from decision boundary is " + distanceFromDecisionBoundary + " position is " + position + " decision boundary is " + decisionBoundary + " in direction " + direction);
            if (distanceFromDecisionBoundary < DECISION_BOUNDARY_TOLERANCE)
            {
                return true;
            }
            return false;
        }

        /**
         * Get the distance along a decision axis
         * @direction: direction from center of tunnel
         * @tunnel: tunnel getting distance along axis
         */
        private float getInnerWallDecisionBoundary(Direction direction, Vector3 tunnelCenter)
        {
            float offset = Tunnel.INNER_WALL_OFFSET;
            Vector3 centerOffset = Dir.CellDirection.getUnitVectorFromDirection(direction) * offset;

            float offsetAlongAxis = Dir.Vector.getAxisPositionFromDirection(direction, centerOffset);   // same value regardless of direction
            float positionAlongAxis = Dir.Vector.getAxisPositionFromDirection(direction, tunnelCenter);
            float innerWallBoundary = offsetAlongAxis + positionAlongAxis; // add the offset to the center of tunnel along same axis

            return innerWallBoundary;
        }

        /**
         * Set the boundaries lines that will trigger a tunnel action.
         * 
         * @tunnelCenter        the center of the tunnel
         */
        private void setBoundaryPoints(Vector3 tunnelCenter)
        {
            Dir.Base.directionList.ForEach((Direction direction) =>
            {
                innerWallBoundary[direction] = getInnerWallDecisionBoundary(direction, tunnelCenter);
            });
        }
    }
}