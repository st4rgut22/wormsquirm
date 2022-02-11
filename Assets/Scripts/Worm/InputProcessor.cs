using UnityEngine;


namespace Worm
{
    public class InputProcessor : WormBody
    {
        protected Tunnel.Tunnel currentTunnel;

        protected bool isDecisionProcessing;

        public delegate void Decision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel);
        public event Decision DecisionEvent;
        
        public delegate void Grow(Tunnel.Tunnel tunnel, Vector3 wormPos);
        public event Grow GrowEvent;

        protected void OnEnable()
        {
            DecisionEvent += FindObjectOfType<Tunnel.Map>().onDecision;

            FindObjectOfType<Tunnel.NewTunnelFactory>().AddTunnelEvent += onAddTunnel;
            FindObjectOfType<Tunnel.ModTunnelFactory>().AddTunnelEvent += onAddTunnel;

            DecisionEvent += GetComponent<Turn>().onDecision;
        }

        new void Awake()
        {
            base.Awake();
            currentTunnel = null;
            isDecisionProcessing = false;
        }

        public virtual void onPlayerInput(Direction direction)
        {
        }

        public virtual void onTorque(DirectionPair dirPair, Waypoint waypoint)
        {
        }

        protected void RaiseDecisionEvent(bool isStraightTunnel, Direction decisionDirection)
        {
            DecisionEvent += currentTunnel.onDecision;
            DecisionEvent(isStraightTunnel, decisionDirection, currentTunnel);
            DecisionEvent -= currentTunnel.onDecision;
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

        protected void OnDisable()
        {
            if (FindObjectOfType<Tunnel.Map>())
            {
                DecisionEvent -= FindObjectOfType<Tunnel.Map>().onDecision;
            }
            if (FindObjectOfType<Turn>())
            {
                DecisionEvent -= GetComponent<Turn>().onDecision;
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