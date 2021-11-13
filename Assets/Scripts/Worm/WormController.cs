using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  Controls worm movement
 */
namespace Worm
{
    public class WormController : MonoBehaviour
    {
        private Direction direction;
        private Direction prevDirection;

        public delegate void Dig(Direction direction, bool isDirectionChanged);
        public event Dig DigEvent;

        // Start is called before the first frame update
        void Awake()
        {
            direction = Direction.None;
            prevDirection = direction;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W))
            {
                direction = Direction.North;
            }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A))
            {
                direction = Direction.West;
            }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.S))
            {
                direction = Direction.South;
            }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D))
            {
                direction = Direction.East;
            }

            bool isDirectionChanged = direction != prevDirection; // onStart the direction will be changed, so a new tunnel will be created

            if (DigEvent != null)
            {
                DigEvent(direction, isDirectionChanged);
                prevDirection = direction;
            }                
        }
    }

}