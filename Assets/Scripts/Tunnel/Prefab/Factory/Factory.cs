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
            if (FindObjectOfType<Map>())
            {
                AddTunnelEvent += FindObjectOfType<Map>().onAddTunnel;
            }
            if (FindObjectOfType<TunnelManager>())
            {
                AddTunnelEvent += FindObjectOfType<TunnelManager>().onAddTunnel;
            }
            if (FindObjectOfType<Test.TunnelMaker>())
            {
                AddTunnelEvent += FindObjectOfType<Test.TunnelMaker>().onAddTunnel;
            }
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
            if (FindObjectOfType<TunnelManager>())
            {
                AddTunnelEvent -= FindObjectOfType<TunnelManager>().onAddTunnel;
            }
            if (FindObjectOfType<Test.TunnelMaker>())
            {
                AddTunnelEvent -= FindObjectOfType<Test.TunnelMaker>().onAddTunnel;
            }
        }
    }
}