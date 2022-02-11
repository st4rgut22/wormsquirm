using UnityEngine;


namespace Worm
{
    public class InputProcessor : WormBody
    {
        private Vector3 unitVectorDirection;
        Tunnel.Tunnel currentTunnel;

        private bool isDecisionProcessing;

        public delegate void Decision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel);
        public event Decision DecisionEvent;
        
        public delegate void Grow(Tunnel.Tunnel tunnel, Vector3 wormPos);
        public event Grow GrowEvent;

        public delegate void InputTorque(DirectionPair dirPair, float torqueMagnitude);
        public event InputTorque InputTorqueEvent;

        private bool isLastTorqueEventInput = true; // a flag for the type of torque event used to determine whether user should turn. If it is from input (and not from waypoints) then should change direction

        private void OnEnable()
        {
            DecisionEvent += FindObjectOfType<Turn>().onDecision;

            DecisionEvent += FindObjectOfType<Tunnel.Map>().onDecision;

            FindObjectOfType<Tunnel.NewTunnelFactory>().AddTunnelEvent += onAddTunnel;
            FindObjectOfType<Tunnel.ModTunnelFactory>().AddTunnelEvent += onAddTunnel;
            InputTorqueEvent += GetComponent<Force>().onInputTorque;
        }

        private new void Awake()
        {
            base.Awake();
            currentTunnel = null;
            unitVectorDirection = Vector3.zero; // initially the worm is not moving
            isDecisionProcessing = false;
        }

        private void Update()
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

                        DecisionEvent += currentTunnel.onDecision;
                        DecisionEvent(isStraightTunnel, decisionDirection, currentTunnel);
                        DecisionEvent -= currentTunnel.onDecision;
                    }
                }
                else
                {
                    print("last torque event is not input, dont change direction");
                }
            }
        }

        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            if (wormId == this.wormId)
            {
                currentTunnel = tunnel;
            }
            else
            {
                throw new System.Exception("during testing there should only be one worm id " + wormId + " and " + this.wormId + " dont match");
            }
        }

        /**
         * If worm is following waypoints, prevent decision making, and when finished signal readiness for more decision-making
         */
        public void onDecisionProcessing(bool isDecisionProcessing, Waypoint waypoint)
        {
            this.isDecisionProcessing = isDecisionProcessing;
            if (!isDecisionProcessing) // set new direction of worm once worm is available to make new turns
            {
                wormBase.setDirection(waypoint.dirPair.curDir); // set the worm direction as the direction coming out of a turn
            }
        }

        /**
         * Torque received while turning through a corner (as opposed to straight travel)
         */
        public void onTorque(DirectionPair dirPair, Waypoint waypoint)
        {
            isLastTorqueEventInput = false;
        }

        /**
         * Move the worm in a different direction and determine whether it has reached one of the six block decision points
         */
        public void onPlayerInput(Direction direction)
        {
            if (!isDecisionProcessing)
            {
                bool isSameDirection = Tunnel.ActionPoint.instance.isDirectionAlongDecisionAxis(currentTunnel, direction);
                if (!isSameDirection)
                {
                    throw new System.Exception("Input should result in a direction that is not in the same direction (or opposite direction) of travel " + direction);
                }
                DirectionPair dirPair = new DirectionPair(wormBase.direction, direction);
                InputTorqueEvent(dirPair, turnSpeed);
                isLastTorqueEventInput = true;
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Force>())
            {
                InputTorqueEvent -= GetComponent<Force>().onInputTorque;
            }
            if (FindObjectOfType<Tunnel.Map>())
            {
                DecisionEvent -= FindObjectOfType<Tunnel.Map>().onDecision;
            }
            if (FindObjectOfType<Turn>())
            {
                DecisionEvent -= FindObjectOfType<Turn>().onDecision;
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