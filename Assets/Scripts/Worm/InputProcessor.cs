using UnityEngine;


namespace Worm
{
    public class InputProcessor : WormBody
    {
        private const float INSTANT_TURN = 1.0f;
        public static float INPUT_SPEED = INSTANT_TURN; // Tunnel.Tunnel.SCALED_GROWTH_RATE; // Match the tunnel growth rate

        private Vector3 unitVectorDirection;
        Tunnel.Tunnel changeDirTunnel;

        private bool isDecisionProcessing;

        public delegate void Decision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel);
        public event Decision DecisionEvent;
        
        public delegate void Grow(Tunnel.Tunnel tunnel, Vector3 wormPos);
        public event Grow GrowEvent;

        private void OnEnable()
        {
            DecisionEvent += FindObjectOfType<Turn>().onDecision;
            DecisionEvent += FindObjectOfType<Tunnel.Map>().onDecision;
        }

        private new void Awake()
        {
            base.Awake();
            changeDirTunnel = null;
            unitVectorDirection = Vector3.zero; // initially the worm is not moving
            isDecisionProcessing = false;
        }

        /**
         * If worm is following waypoints, prevent decision making
         */
        public void onDecisionProcessing(bool isDecisionProcessing)
        {
            this.isDecisionProcessing = isDecisionProcessing;
        }

        /**
         * Move the worm in a different direction and determine whether it has reached one of the six block decision points
         */
        public void onPlayerInput(Direction direction)
        {
            if (!isDecisionProcessing)
            {
                unitVectorDirection = Dir.Vector.getUnitVectorFromDirection(direction);

                Vector3 inputPosition = ring.position + unitVectorDirection * INPUT_SPEED;

                changeDirTunnel = Tunnel.Map.getCurrentTunnel(clit.position);

                bool isSameDirection = Tunnel.ActionPoint.instance.isDirectionAlongDecisionAxis(changeDirTunnel, direction);

                if (isSameDirection)
                {
                    bool isDecision = Tunnel.ActionPoint.instance.isDecisionBoundaryCrossed(changeDirTunnel, inputPosition, direction);

                    if (isDecision)
                    {
                        bool isStraightTunnel = changeDirTunnel.type == Tunnel.Type.Name.STRAIGHT; // if this is the first tunnel it should be straight type
                        isDecisionProcessing = true;

                        DecisionEvent += changeDirTunnel.onDecision;
                        DecisionEvent(isStraightTunnel, direction, changeDirTunnel);
                        DecisionEvent -= changeDirTunnel.onDecision;
                    }
                }

                //todo: force event here in unitVectorDir
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Turn>())
            {
                DecisionEvent -= FindObjectOfType<Turn>().onDecision;

            }
            if (FindObjectOfType<Tunnel.Map>())
            {
                DecisionEvent -= FindObjectOfType<Tunnel.Map>().onDecision;
            }
        }
    }
}