using UnityEngine;

/*
 *  Player controller and input
 */
namespace Worm
{
    public class Controller : BaseController
    {
        InputKey prevPressedKey;
        Direction prevDirection;

        private new void Awake()
        {
            base.Awake();
            prevPressedKey = null;
            prevDirection = Direction.None;
        }

        private Direction getDirection(InputKeyPair inputKeyPair)
        {            
            InputKey pressedKey = inputKeyPair.getPressedInputKey();

            Direction direction;
            if (wormBase.direction == Direction.None)
            {
                direction = pressedKey.initDirection;
            }
            else
            {
                direction = Dir.Input.getChangedDirection(wormBase.direction, prevDirection, pressedKey, prevPressedKey);
            }

            prevDirection = wormBase.direction;
            prevPressedKey = pressedKey;
            
            return direction;
        }

        void Update()
        {
            InputKeyPair inputKeyPair = InputManager.instance.getInputKeyPair();

            if (inputKeyPair != null)
            {
                Direction localDirection = getDirection(inputKeyPair); // direction with respect to tunnel

                RaisePlayerInputEvent(localDirection);
                if (!wormBase.isInitialized) // flag to make sure the initDecision is only  issued once
                {
                    RaiseInitDecisionEvent(localDirection);
                }
            }
        }
    }

}