using UnityEngine;

namespace Worm
{
    public class WormTunnelBroker : WormBody
    {
        public delegate void Grow(Vector3 ringPosition);
        public event Grow GrowEvent;

        public delegate void Move(Vector3 ringPosition, Direction direction);
        public event Move MoveEvent;

        Tunnel.Straight prevStraightTunnel;

        private void OnEnable()
        {
            FindObjectOfType<Tunnel.NewTunnelFactory>().AddTunnelEvent += onAddTunnel;
            FindObjectOfType<Tunnel.ModTunnelFactory>().AddTunnelEvent += onAddTunnel;
        }

        // Start is called before the first frame update
        private new void Awake()
        {
            base.Awake();
            prevStraightTunnel = null;
        }

        private void Update()
        {
        }

        /**
         * Issue an event that grows the current tunnel
         */
        private void FixedUpdate()
        {
            if (GrowEvent != null)
            {
                GrowEvent(ring.position);
            }
        }

        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            if (wormId == this.wormId)
            {
                bool isTunnelStraight = tunnel.isTunnelType(Tunnel.Type.Name.STRAIGHT);

                // unregister the previous tunnel's grow event, and register the new one
                if (prevStraightTunnel != null)
                {
                    GrowEvent -= prevStraightTunnel.onGrow;
                    prevStraightTunnel = null;
                }
                if (isTunnelStraight)
                {
                    prevStraightTunnel = (Tunnel.Straight) tunnel;
                    GrowEvent += prevStraightTunnel.onGrow;
                }
                else if (!directionPair.isStraight())// if the tunnel is not straight (eg one direction) then it is turnable
                {
                    Tunnel.TurnableTunnel turnableTunnel = (Tunnel.TurnableTunnel)tunnel;
                }
                else
                {
                    GetComponent<Movement>().goStraightThroughJunction(tunnel); // create junction here 
                }
            }
        }

        private void OnDisable()
        {
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