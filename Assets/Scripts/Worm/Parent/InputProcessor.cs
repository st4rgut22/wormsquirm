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

        protected bool isReadyForInput; // flag indicating if worm position, movement have been initialized

        protected void OnEnable()
        {

            FindObjectOfType<Tunnel.NewTunnelFactory>().AddTunnelEvent += onAddTunnel;
            FindObjectOfType<Tunnel.ModTunnelFactory>().AddTunnelEvent += onAddTunnel;

            DecisionEvent += FindObjectOfType<Tunnel.Map>().onDecision;
            DecisionEvent += GetComponent<Turn>().onDecision;
            DecisionEvent += GetComponent<Force>().onDecision;
        }

        new void Awake()
        {
            base.Awake();
            currentTunnel = null;
            isDecisionProcessing = false;
            isReadyForInput = false;
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

        /**
         * Receive event of new block interval when worm is inside an existing tunnel. Use to update the existing tunnel
         */
        public void onWormInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Tunnel.Tunnel tunnel)
        {
            if (currentTunnel != tunnel)
            {
                currentTunnel = tunnel;
            }
        }

        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            if (wormId == this.wormId)
            {
                if (tunnel.type == Tunnel.Type.Name.STRAIGHT)
                {
                    isReadyForInput = true;
                }
                currentTunnel = tunnel;
            }
            else
            {
                throw new System.Exception("during testing there should only be one worm id associated with tunnel " + tunnel.name + " worms " + wormId + " and " + this.wormId + " dont match");
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
                DecisionEvent -= GetComponent<Force>().onDecision;
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