using UnityEngine;


namespace Worm
{
    public class InputProcessor : WormBody
    {
        private const float INSTANT_TURN = 1.0f;
        public static float INPUT_SPEED = INSTANT_TURN; // Tunnel.Tunnel.SCALED_GROWTH_RATE; // Match the tunnel growth rate

        private Vector3 unitVectorDirection;
        Tunnel.Tunnel currentTunnel;

        private bool isDecisionProcessing;

        public delegate void Decision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel);
        public event Decision DecisionEvent;
        
        public delegate void Grow(Tunnel.Tunnel tunnel, Vector3 wormPos);
        public event Grow GrowEvent;

        private void OnEnable()
        {
            DecisionEvent += FindObjectOfType<Turn>().onDecision;
            DecisionEvent += FindObjectOfType<Tunnel.Map>().onDecision;

            FindObjectOfType<Tunnel.NewTunnelFactory>().AddTunnelEvent += onAddTunnel;
            FindObjectOfType<Tunnel.ModTunnelFactory>().AddTunnelEvent += onAddTunnel;
        }

        private new void Awake()
        {
            base.Awake();
            currentTunnel = null;
            unitVectorDirection = Vector3.zero; // initially the worm is not moving
            isDecisionProcessing = false;
        }

        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            if (wormId == this.wormId)
            {
                currentTunnel = tunnel;
            }
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
            print("received player input event in dir " + direction + " decision processing is " + isDecisionProcessing + " clit position is " + clit.position);

            if (!isDecisionProcessing)
            {
                unitVectorDirection = Dir.CellDirection.getUnitVectorFromDirection(direction);

                Vector3 inputPosition = ring.position + unitVectorDirection * INPUT_SPEED;

                bool isSameDirection = Tunnel.ActionPoint.instance.isDirectionAlongDecisionAxis(currentTunnel, direction);
                print("isSameDecision is " + isSameDirection);
                if (isSameDirection)
                {
                    bool isDecision = Tunnel.ActionPoint.instance.isDecisionBoundaryCrossed(currentTunnel, inputPosition, direction);

                    print("isDecision is " + isDecision);
                    if (isDecision)
                    {
                        bool isStraightTunnel = currentTunnel.type == Tunnel.Type.Name.STRAIGHT; // if this is the first tunnel it should be straight type
                        isDecisionProcessing = true;

                        DecisionEvent += currentTunnel.onDecision;
                        DecisionEvent(isStraightTunnel, direction, currentTunnel);
                        DecisionEvent -= currentTunnel.onDecision;
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
            if (FindObjectOfType<Tunnel.NewTunnelFactory>())
            {
                FindObjectOfType<Tunnel.NewTunnelFactory>().AddTunnelEvent -= onAddTunnel;
            }
            if (FindObjectOfType<Tunnel.ModTunnelFactory>())
            {
                FindObjectOfType<Tunnel.ModTunnelFactory>().AddTunnelEvent -= onAddTunnel;
            }
        }
    }
}