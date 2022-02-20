using UnityEngine;

namespace Worm
{
    public class BaseController : WormBody
    {
        public delegate void InitDecision(Direction direction, string wormId, Vector3Int initialCell);
        public event InitDecision InitDecisionEvent;

        public delegate void PlayerInput(Direction direction);
        public event PlayerInput PlayerInputEvent;

        protected bool isGameStart;

        private new void Awake()
        {
            base.Awake();
            isGameStart = true;
        }

        protected void OnEnable()
        {
            InitDecisionEvent += Tunnel.CollisionManager.Instance.onInitDecision;
            InitDecisionEvent += GetComponent<Turn>().onInitDecision;
            if (GetComponent<InputProcessor>())
            {
                PlayerInputEvent += GetComponent<InputProcessor>().onPlayerInput;
            }
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
         * Signal the initial movement made in a specified direction starting at predetermined cell
         * 
         * @localDirection the direction the worm starts traveling
         */
        protected void RaiseInitDecisionEvent(Direction localDirection)
        {
            InitDecisionEvent(localDirection, wormId, wormBase.initialCell);
            wormBase.initializeWorm();
        }

        protected void OnDisable()
        {
            if (Tunnel.CollisionManager.Instance)
            {
                InitDecisionEvent -= Tunnel.CollisionManager.Instance.onInitDecision;
            }
            if (GetComponent<InputProcessor>())
            {
                PlayerInputEvent -= GetComponent<InputProcessor>().onPlayerInput;
            }
            InitDecisionEvent -= GetComponent<Turn>().onInitDecision;
        }
    }

}