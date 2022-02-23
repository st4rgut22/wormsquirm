using UnityEngine;

/*
 *  Player controller and input
 */
namespace Worm
{
    public class Controller : BaseController
    {
        Direction prevDirection;
        InputKey prevInputKey;
        InputKey pressedKey;

        private new void Awake()
        {
            base.Awake();
            prevDirection = Direction.None;
            prevInputKey = pressedKey = null;
        }

        /**
         * Listener for decision made event resulting from human input. Set the previous direction after a decision has been made because
         * some player input does not result in a turn decision (just wiggling)
         */
        public void onDecision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel)
        {
            print("on decision prevDirection is " + direction + " prevInputKey is " + pressedKey + " wormbase direction is " + wormBase.direction);
            if (prevDirection != direction) // only save current direction if it has changed (eg dont save it if 
            {
                prevDirection = wormBase.direction;
                prevInputKey = pressedKey;
            }
            else
            {
                throw new System.Exception("when a decision is made it should be in a different direction but it is still the same: " + wormBase.direction);
            }
        }

        private Direction getDirection(InputKeyPair inputKeyPair)
        {            
            pressedKey = inputKeyPair.getPressedInputKey();

            Direction direction;
            if (wormBase.direction == Direction.None)
            {
                direction = pressedKey.initDirection;
            }
            else
            {
                print("worm dir " + wormBase.direction + " prev direction " + prevDirection + " pressed key " + pressedKey + " prev press key " + prevInputKey);
                direction = Dir.Input.getChangedDirection(wormBase.direction, prevDirection, pressedKey, prevInputKey);
            }
           
            return direction;
        }

        void Update()
        {
            InputKeyPair inputKeyPair = InputManager.instance.getInputKeyPair();

            if (inputKeyPair != null)
            {
                Direction localDirection = getDirection(inputKeyPair); // direction with respect to tunnel

                print("emit player input event in direction " + localDirection);
                RaisePlayerInputEvent(localDirection);

                if (!wormBase.isInitialized) // flag to make sure the initDecision is only  issued once
                {
                    RaiseInitDecisionEvent(localDirection);
                }
            }
        }
    }

}