using UnityEngine;

/*
 *  Player controller and input
 */
namespace Worm
{
    public class Controller : WormBody
    {      
        public delegate void PlayerInput(Direction direction);
        public event PlayerInput PlayerInputEvent;

        public delegate void InitDecision(Direction direction, string wormId, Vector3Int initialCell);
        public event InitDecision InitDecisionEvent;

        private bool isGameStart;

        private void OnEnable()
        {
            InitDecisionEvent += Tunnel.CollisionManager.Instance.onInitDecision;
            InitDecisionEvent += GetComponent<Turn>().onInitDecision;
            PlayerInputEvent += GetComponent<InputProcessor>().onPlayerInput;
        }

        // Start is called before the first frame update
        new void Awake()
        {
            base.Awake();
            isGameStart = true;
        }

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

                if (isGameStart)
                {
                    InitDecisionEvent(localDirection, wormId, initialCell);
                    isGameStart = false;
                }
                else if (PlayerInputEvent != null)
                {
                    PlayerInputEvent(localDirection);
                }
            }
        }

        private void OnDisable()
        {
            InitDecisionEvent -= Tunnel.CollisionManager.Instance.onInitDecision;
            InitDecisionEvent -= GetComponent<Turn>().onInitDecision;
            PlayerInputEvent -= GetComponent<InputProcessor>().onPlayerInput;
        }
    }

}