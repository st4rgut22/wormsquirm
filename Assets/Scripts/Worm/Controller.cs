using UnityEngine;

/*
 *  Player controller and input
 */
namespace Worm
{
    public class Controller : MonoBehaviour
    {
        private Direction direction;
        
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
        }

        public void onCompleteTurn(Direction direction)
        {
            this.direction = direction;
        }

        private Direction getDirection(InputKeyPair inputKeyPair)
        {
            InputKey pressedKey = inputKeyPair.getPressedInputKey();

            if (direction == Direction.None)
            {
                direction = pressedKey.initDirection;
                return direction;
            }
            else
            {
                return Dir.Input.getChangedDirection(direction, pressedKey);
            }
        }

        void Update()
        {

            InputKeyPair inputKeyPair = InputManager.instance.getInputKeyPair();

            if (inputKeyPair != null)
            {
                Direction localDirection = getDirection(inputKeyPair); // direction with respect to tunnel

                if (PlayerInputEvent != null)
                {
                    PlayerInputEvent(localDirection);
                }
            }
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