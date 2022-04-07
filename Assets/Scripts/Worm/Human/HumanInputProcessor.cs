namespace Worm
{
    public class HumanInputProcessor : InputProcessor
    {
        public delegate void InputTorque(DirectionPair dirPair, float torqueMagnitude);
        public event InputTorque InputTorqueEvent;

        private Direction lastTorqueEventDirection; // the direction in which the last torque event happend, None value indicates absence of torque event

        private new void Awake()
        {
            base.Awake();
            lastTorqueEventDirection = Direction.None;
        }

        private new void OnEnable()
        {
            base.OnEnable();
            DecisionEvent += GetComponent<Controller>().onDecision;
            InputTorqueEvent += GetComponent<Force>().onInputTorque;            
        }

        /**
         * Torque received while turning through a corner (as opposed to straight travel)
         */
        public override void onTorque(DirectionPair dirPair, Waypoint waypoint)
        {
            lastTorqueEventDirection = Direction.None;
        }

        // / set the flag for input torque event (and not from waypoints). Indicates that we should change direction
        public override void onPlayerInput(Direction direction)
        {
            if (!isDecisionProcessing && isReadyForInput)
            {
                bool isSameDirection = Tunnel.ActionPoint.instance.isDirectionAlongDecisionAxis(currentTunnel, direction);
                if (!isSameDirection)
                {
                    throw new System.Exception("Input should result in a direction that is not in the same direction (or opposite direction) of travel " + direction);
                }
                applyTorque(direction);
            }
        }

        /**
         * Apply torque to make the transition from current direction to the final direction
         * 
         * @direction final direction
         */
        private void applyTorque(Direction direction)
        {
            DirectionPair dirPair = new DirectionPair(wormBase.direction, direction);
            print("apply torque in direction " + direction);
            if (!dirPair.isStraight())
            {
                InputTorqueEvent(dirPair, turnSpeed);
                lastTorqueEventDirection = direction;
            }
        }

        void Update()
        {
            if (!isDecisionProcessing && isReadyForInput)
            {
                if (lastTorqueEventDirection != Direction.None)
                {
                    Direction decisionDirection = Tunnel.ActionPoint.instance.getDirectionDecisionBoundaryCrossed(currentTunnel, head.position, wormBase.direction, lastTorqueEventDirection);
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
            if (FindObjectOfType<Controller>())
            {
                DecisionEvent += GetComponent<Controller>().onDecision;
            }
            if (FindObjectOfType<Force>())
            {
                InputTorqueEvent -= GetComponent<Force>().onInputTorque;
            }
        }
    }
}