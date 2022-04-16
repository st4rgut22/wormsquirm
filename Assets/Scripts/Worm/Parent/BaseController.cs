using UnityEngine;

namespace Worm
{
    public class BaseController : WormBody
    {
        public delegate void InitDecision(Direction direction, string wormId, Vector3Int mappedInitialCell, Vector3Int initialCell);
        public event InitDecision InitDecisionEvent;

        public delegate void PlayerInput(Direction direction);
        public event PlayerInput PlayerInputEvent;

        protected bool isGameStart;

        private new void Awake()
        {
            base.Awake();
            isGameStart = true;
        }

        protected new void OnEnable()
        {
            base.OnEnable();
            InitDecisionEvent += Tunnel.CollisionManager.Instance.onInitDecision;
            InitDecisionEvent += GetComponent<Turn>().onInitDecision;
            print(gameObject.name);
            PlayerInputEvent += GetComponent<InputProcessor>().onPlayerInput;
        }

        /**
         * Get the cell the worm starts in
         */
        public Vector3Int getInitialCell()
        {
            return wormBase.initialCell;
        }

        /**
         * Indicate that a worm wants to execute an event. However a turn event wont happen until straight block has reached an interval length
         * 
         * @direction the direction to turn
         */
        protected void RaisePlayerInputEvent(Direction direction)
        {
            PlayerInputEvent(direction);
        }

        /**
         * Call remove self event from grandchild of wormBody
         */
        protected void RaiseRaiseRemoveSelfEvent()
        {
            RaiseRemoveSelfEvent();
        }

        /**
         * Signal the initial movement made in a specified direction starting at predetermined cell
         * 
         * @localDirection the direction the worm starts traveling
         */
        protected void RaiseInitDecisionEvent(Direction localDirection)
        {
            wormBase.initializeWorm(localDirection);
            InitDecisionEvent(localDirection, wormId, wormBase.mappedInitialCell, wormBase.initialCell);
        }

        protected new void OnDisable()
        {
            base.OnDisable();
            if (Tunnel.CollisionManager.Instance)
            {
                InitDecisionEvent -= Tunnel.CollisionManager.Instance.onInitDecision;
            }
            if (GetComponent<InputProcessor>())
            {
                PlayerInputEvent -= GetComponent<InputProcessor>().onPlayerInput;
            }
            if (FindObjectOfType<Map.Cubemap>())
            {
                InitDecisionEvent -= FindObjectOfType<Map.Cubemap>().onSetInitDirection;
            }

            InitDecisionEvent -= GetComponent<Turn>().onInitDecision;
        }
    }
}