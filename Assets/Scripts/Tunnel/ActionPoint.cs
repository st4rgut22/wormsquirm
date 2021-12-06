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

        public delegate void Move(Direction direction);
        public event Move MoveEvent;

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            MoveEvent += FindObjectOfType<Worm.Movement>().onMove;
        }

        /**
         * Decide if player movement should trigger changes to the tunnel network such as tunnel creation
         */
        public bool isDecisionBoundaryCrossed(Tunnel tunnel, Vector3 position, Direction direction)
        {
            if (tunnel == null)
            {
                return true;
            }

            if (isDirectionAlongDecisionAxis(tunnel, direction))
            {
                MoveEvent(direction); // send the move command to the worm because move is along a decision axis (ie not in same direction of travel)

                decisionPointBoundary = new Dictionary<Direction, float>();
                setBoundaryPoints(tunnel.center);

                float axisPosition = Dir.Vector.getAxisPositionFromDirection(direction, position);

                if (isTriggerDecision(axisPosition, direction))
                {
                    print("decision is triggered");
                    return true;
                }
            }
            return false;
        }

        /**
         * Decisions involve creating tunnel pieces and only apply to non-straight tunnels that cannot scale.
         * Check that direction is along a non-scalable axis
         */
        private bool isDirectionAlongDecisionAxis(Tunnel tunnel, Direction direction)
        {
            if (tunnel.type != Type.Name.STRAIGHT)
            {
                return true;
            }
            else
            {
                if (direction != ((Straight)tunnel).growthDirection) // check for movement perpendicular to  growth axis in a straight tunnel.
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
            Vector3 centerOffset = Dir.Vector.getUnitVectorFromDirection(direction) * Tunnel.CENTER_OFFSET;

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

        private void OnDisable()
        {
            if (FindObjectOfType<Worm.Movement>())
            {
                MoveEvent -= FindObjectOfType<Worm.Movement>().onMove;
            }
        }

    }
}