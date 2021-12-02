using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class Rotation : MonoBehaviour
    {

        // Dictionary<ingress hole direction, Dictionary<egress hold direction, rotation>>()
        private static Dictionary<Direction, Quaternion> wormRotationDict =
            new Dictionary<Direction, Quaternion>
            {
                { Direction.Up, Quaternion.Euler(0, 0, 0) },
                { Direction.Down, Quaternion.Euler(0, 0, 180) },
                { Direction.Back, Quaternion.Euler(0, -90, 90) },
                { Direction.Forward, Quaternion.Euler(0, 90, 90) },
                { Direction.Right, Quaternion.Euler(0, 0, -90) },
                { Direction.Left, Quaternion.Euler(0, 0, 90) }
            };

        public void onCompleteTurn(Direction direction)
        {
            transform.rotation = wormRotationDict[direction];
        }
    }
}
