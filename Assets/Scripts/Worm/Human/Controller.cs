using UnityEngine;

/*
 *  Player controller and input
 */
namespace Worm
{
    public class Controller : BaseController
    {      
        private Direction getDirection(InputKeyPair inputKeyPair)
        {            
            InputKey pressedKey = inputKeyPair.getPressedInputKey();

            if (wormBase.direction == Direction.None)
            {
                return pressedKey.initDirection;
            }
            else
            {
                return Dir.Input.getChangedDirection(wormBase.direction, pressedKey);
            }
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