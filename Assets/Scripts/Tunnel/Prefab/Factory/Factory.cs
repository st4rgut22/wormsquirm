using UnityEngine;

namespace Tunnel
{
    public abstract class Factory : MonoBehaviour
    {
        public delegate void AddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId); // wormId is the id of the worm that created the tunnel
        public event AddTunnel AddTunnelEvent;

        protected CellMove cellMove;
        protected DirectionPair directionPair;

        public abstract Tunnel getTunnel();

        protected void OnEnable()
        {
            AddTunnelEvent += TunnelManager.Instance.onAddTunnel;
            AddTunnelEvent += FindObjectOfType<Map>().onAddTunnel;
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
            AddTunnelEvent(tunnel, cellMove.cell, directionPair, wormId);
            AddTunnelEvent -= tunnel.onAddTunnel;
        }

        protected void OnDisable()
        {
            if (FindObjectOfType<Map>())
            {
                AddTunnelEvent -= FindObjectOfType<Map>().onAddTunnel;
            }
            if (TunnelManager.Instance)
            {
                AddTunnelEvent -= TunnelManager.Instance.onAddTunnel;
            }
        }
    }
}