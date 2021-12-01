using UnityEngine;

/*
 *  Player controller and input
 */
namespace Worm
{
    public class Controller : MonoBehaviour
    {
        private Direction direction;
        private Direction prevDirection;
        
        public delegate void PlayerInput(Direction direction);
        public event PlayerInput PlayerInputEvent;

        private void OnEnable()
        {
            PlayerInputEvent += FindObjectOfType<Movement>().onPlayerInput;
        }

        // Start is called before the first frame update
        void Awake()
        {
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
                return Dir.Input.getChangedDirection(direction, pressedKey);
            }
        }

        void Update()
        {
            Direction newDirection = direction;

            InputKeyPair inputKeyPair = InputManager.instance.getInputKeyPair();

            if (inputKeyPair != null)
            {
                newDirection = getDirection(inputKeyPair);

                if (PlayerInputEvent != null)
                {
                    PlayerInputEvent(newDirection);
                }
            }
        }

        /**
         * When worm has moved enough to trigger change in tunnel direction, save the new direction as reference for next player input
         */
        public void onDecision(bool isStraightTunnel, Direction newDirection, Tunnel.Tunnel tunnel)
        {
            prevDirection = direction;
            direction = newDirection;
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Movement>())
            {
                PlayerInputEvent -= FindObjectOfType<Movement>().onPlayerInput;
            }
        }
    }

}