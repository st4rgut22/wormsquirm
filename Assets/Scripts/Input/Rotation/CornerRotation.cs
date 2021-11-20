using UnityEngine;
using System.Collections.Generic;

namespace Tunnel
{
    public class CornerRotation: TunnelRotation
    {

        // Dictionary<ingress hole direction, Dictionary<egress hold direction, rotation>>()
        private static Dictionary<Direction, Dictionary<Direction, Quaternion>> cornerRotationDict =
            new Dictionary<Direction, Dictionary<Direction, Quaternion>>
            {
                {
                    Direction.Up, new Dictionary<Direction, Quaternion>
                    {
                        { Direction.Left, Quaternion.Euler(-90, 180, 0) },
                        { Direction.Right, Quaternion.Euler(-90, 0, 0) },
                        { Direction.Back, Quaternion.Euler(-90, 90, 0) },
                        { Direction.Forward, Quaternion.Euler(-90, -90, 0) }
                    }
                },
                {
                    Direction.Down, new Dictionary<Direction, Quaternion>
                    {
                        { Direction.Left, Quaternion.Euler(90, 180, 0) },
                        { Direction.Right, Quaternion.Euler(90, 0, 0) },
                        { Direction.Back, Quaternion.Euler(90, 90, 0) },
                        { Direction.Forward, Quaternion.Euler(90, -90, 0) }
                    }
                },
                {
                    Direction.Forward, new Dictionary<Direction, Quaternion>
                    {
                        { Direction.Right, Quaternion.Euler(0, 0, 0) },
                        { Direction.Left, Quaternion.Euler(0, 0, 180) },
                        { Direction.Up, Quaternion.Euler(0, 0, 90) },
                        { Direction.Down, Quaternion.Euler(0, 0, -90) },
                    }
                },
                {
                    Direction.Back, new Dictionary<Direction, Quaternion>
                    {
                        { Direction.Left, Quaternion.Euler(180, 0, 180) },
                        { Direction.Right, Quaternion.Euler(180, 0, 0) },
                        { Direction.Down, Quaternion.Euler(180, 0, 90) },
                        { Direction.Up, Quaternion.Euler(180, 0, -90) }
                    }
                },
                {
                    Direction.Left, new Dictionary<Direction, Quaternion>
                    {
                        { Direction.Forward, Quaternion.Euler(0, -90, 0) },
                        { Direction.Up, Quaternion.Euler(0, -90, 90) },
                        { Direction.Back, Quaternion.Euler(0, -90, 180) },
                        { Direction.Down, Quaternion.Euler(0, -90, -90) }
                    }
                },
                {
                    Direction.Right, new Dictionary<Direction, Quaternion>
                    {
                        { Direction.Back, Quaternion.Euler(180, -90, 180) },
                        { Direction.Up, Quaternion.Euler(0, 90, 90) },
                        { Direction.Forward, Quaternion.Euler(0, 90, 180) },
                        { Direction.Down, Quaternion.Euler(0, 90, -90) }
                    }
                }
            };

        /**
         * Use the previous and current tunnel directiosn to get rotation and offset of the corner piece
         */
        public static Quaternion getRotationFromDirection(Direction prevTunnelDirection, Direction currentTunnelDirection)
        {
            Quaternion cornerRotation = cornerRotationDict[prevTunnelDirection][currentTunnelDirection];
            Debug.Log("cur " + currentTunnelDirection + " prev " + prevTunnelDirection + " euler angles is " + cornerRotation.eulerAngles);
            return cornerRotation;
        }
    }
}