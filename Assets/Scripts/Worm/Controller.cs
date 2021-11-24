using UnityEngine;

/*
 *  Controls worm movement
 */
namespace Worm
{
    public class Controller : MonoBehaviour
    {
        private Direction direction;
        private Direction prevDirection;

        private bool isMoving;

        public delegate void Dig(Direction direction, Direction prevDirection, bool isDirectionChanged);
        public event Dig DigEvent;

        // Start is called before the first frame update
        void Awake()
        {
            isMoving = false;
            direction = Direction.None;
            prevDirection = direction;
        }

        private Direction getDirection(InputKeyPair inputKeyPair)
        {
            InputKey pressedKey = inputKeyPair.getPressedInputKey();

            if (direction == Direction.None)
            {
                return pressedKey.initDirection;
            }
            else
            {
                return Dir.getChangedDirection(direction, pressedKey);
            }
        }

        private bool isDirectionChanged(Direction newDirection)
        {
            if (newDirection == Direction.None)
            {
                return false;
            }
            else
            {
                return direction != newDirection;
            }
        }

        void Update()
        {
            Direction newDirection = direction;

            InputKeyPair inputKeyPair = InputManager.instance.getInputKeyPair();

            if (inputKeyPair != null)
            {
                isMoving = true;
                newDirection = getDirection(inputKeyPair);
            }

            if (isMoving)
            {
                if (DigEvent != null)
                {
                    bool isDirectionChanged = this.isDirectionChanged(newDirection);

                    prevDirection = direction;
                    direction = newDirection;

                    DigEvent(direction, prevDirection, isDirectionChanged);                 
                }
            }
        }
    }

}