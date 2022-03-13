using UnityEngine;

namespace Tunnel
{
    public abstract class Factory : MonoBehaviour
    {
        public delegate void AddTunnel(Tunnel tunnel, CellMove cellMove, DirectionPair directionPair, string wormId); // wormId is the id of the worm that created the tunnel
        public event AddTunnel AddTunnelEvent;

        protected CellMove cellMove;
        protected DirectionPair directionPair;

        public abstract Tunnel getTunnel();

        protected void OnEnable()
        {
            AddTunnelEvent += TunnelManager.Instance.onAddTunnel;
            AddTunnelEvent += FindObjectOfType<TunnelMap>().onAddTunnel;
            AddTunnelEvent += Worm.WormManager.Instance.onAddTunnel;
        }

        /**
         * Get the name of the tunnel
         * 
         * @tunnelType the tunnel's type eg Corner, Straight
         * @tunnelCount the number of instances of this tunnel type
         */
        protected string getTunnelId(string tunnelType, int tunnelCount)
        {
            return tunnelType + " " + tunnelCount;

        }

        /**
         * Adds tunnel to list of tunnels and the map, and subscribes/unsubscribes tunnels from the addTunnel event
         * 
         * @tunnel newly created tunnel
         */
        protected void addTunnel(Tunnel tunnel, string wormId)
        {

            AddTunnelEvent += tunnel.onAddTunnel;
            AddTunnelEvent(tunnel, cellMove, directionPair, wormId);
            AddTunnelEvent -= tunnel.onAddTunnel;
        }

        protected void OnDisable()
        {
            if (FindObjectOfType<TunnelMap>())
            {
                AddTunnelEvent -= FindObjectOfType<TunnelMap>().onAddTunnel;
            }
            if (TunnelManager.Instance)
            {
                AddTunnelEvent -= TunnelManager.Instance.onAddTunnel;
            }
            if (Worm.WormManager.Instance)
            {
                AddTunnelEvent -= Worm.WormManager.Instance.onAddTunnel;
            }
        }
    }
}