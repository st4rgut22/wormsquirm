using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dir
{
    public class DirectionForce
    {
        private static Dictionary<Direction, Dictionary<Direction, Vector3>> TorqueVectorDictionary = new Dictionary<Direction, Dictionary<Direction, Vector3>>()
        {
            {
                Direction.Forward, new Dictionary<Direction, Vector3>()
                {
                    { Direction.Up, Vector3.left },
                    { Direction.Down, Vector3.right },
                    { Direction.Left, Vector3.down },
                    { Direction.Right, Vector3.up }
                }
            },
            {
                Direction.Back, new Dictionary<Direction, Vector3>()
                {
                    { Direction.Up, Vector3.right },
                    { Direction.Down, Vector3.left },
                    { Direction.Left, Vector3.up },
                    { Direction.Right, Vector3.down }
                }
            },
            {
                Direction.Up, new Dictionary<Direction, Vector3>()
                {
                    { Direction.Forward, Vector3.right },
                    { Direction.Back, Vector3.left },
                    { Direction.Left, Vector3.forward },
                    { Direction.Right, Vector3.back }
                }
            },
            {
                Direction.Down, new Dictionary<Direction, Vector3>()
                {
                    { Direction.Forward, Vector3.left }, // update
                    { Direction.Back, Vector3.right }, // update
                    { Direction.Left, Vector3.back },
                    { Direction.Right, Vector3.forward }
                }
            },
            {
                Direction.Left, new Dictionary<Direction, Vector3>()
                {
                    { Direction.Forward, Vector3.up },
                    { Direction.Back, Vector3.down },
                    { Direction.Up, Vector3.back },
                    { Direction.Down, Vector3.forward }
                }
            },
            {
                Direction.Right, new Dictionary<Direction, Vector3>()
                {
                    { Direction.Forward, Vector3.down },
                    { Direction.Back, Vector3.up },
                    { Direction.Up, Vector3.forward },
                    { Direction.Down, Vector3.back }
                }
            },
        };

        public static Vector3 getTorqueVectorFromDirection(DirectionPair directionPair)
        {
            return TorqueVectorDictionary[directionPair.prevDir][directionPair.curDir];
        }
    }

}