namespace Worm
{
    public class HumanInputProcessor : InputProcessor
    {
        public delegate void InputTorque(DirectionPair dirPair, float torqueMagnitude);
        public event InputTorque InputTorqueEvent;

        private bool isLastTorqueEventInput = true;

        private new void OnEnable()
        {
            base.OnEnable();
            InputTorqueEvent += GetComponent<Force>().onInputTorque;            
        }

        /**
         * Torque received while turning through a corner (as opposed to straight travel)
         */
        public override void onTorque(DirectionPair dirPair, Waypoint waypoint)
        {
            isLastTorqueEventInput = false;
        }

        // / set the flag for input torque event (and not from waypoints). Indicates that we should change direction
        public override void onPlayerInput(Direction direction)
        {
            if (!isDecisionProcessing)
            {
                bool isSameDirection = Tunnel.ActionPoint.instance.isDirectionAlongDecisionAxis(currentTunnel, direction);
                if (!isSameDirection)
                {
                    throw new System.Exception("Input should result in a direction that is not in the same direction (or opposite direction) of travel " + direction);
                }
                DirectionPair dirPair = new DirectionPair(wormBase.direction, direction);
                isLastTorqueEventInput = true;
                InputTorqueEvent(dirPair, turnSpeed);
            }
        }

        void Update()
        {
            if (!isDecisionProcessing && currentTunnel != null)
            {
                if (isLastTorqueEventInput)
                {
                    Direction decisionDirection = Tunnel.ActionPoint.instance.getDirectionDecisionBoundaryCrossed(currentTunnel, head.position, wormBase.direction);
                    if (decisionDirection != Direction.None)
                    {
                        print("processing decision in direction " + decisionDirection);
                        bool isStraightTunnel = currentTunnel.type == Tunnel.Type.Name.STRAIGHT; // if this is the first tunnel it should be straight type
                        isDecisionProcessing = true;
                        RaiseDecisionEvent(isStraightTunnel, decisionDirection);
                    }
                }
                else
                {
                    print("last torque event is not input, dont change direction");
                }
            }
        }

        private new void OnDisable()
        {
            base.OnDisable();

            if (FindObjectOfType<Force>())
            {
                InputTorqueEvent -= GetComponent<Force>().onInputTorque;
            }
        }
    }
}