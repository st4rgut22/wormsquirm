using UnityEngine;

namespace Tunnel
{
    public abstract class Factory : MonoBehaviour
    {
        public delegate void AddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair);
        public event AddTunnel AddTunnelEvent;

        protected CellMove cellMove;
        protected DirectionPair directionPair;

        public abstract Tunnel getTunnel();

        protected void OnEnable()
        {
            AddTunnelEvent += FindObjectOfType<Map>().onAddTunnel;
            AddTunnelEvent += FindObjectOfType<Manager>().onAddTunnel;

            if (FindObjectOfType<Test.TunnelMaker>())
            {
                AddTunnelEvent += FindObjectOfType<Test.TunnelMaker>().onAddTunnel;
            }
        }

        /**
         * Adds tunnel to list of tunnels and the map
         * 
         * @tunnel newly created tunnel
         */
        protected void addTunnel(Tunnel tunnel)
        {
            AddTunnelEvent(tunnel, cellMove.cell, directionPair);
        }

        protected void OnDisable()
        {
            if (FindObjectOfType<Map>())
            {
                AddTunnelEvent -= FindObjectOfType<Map>().onAddTunnel;
            }
            if (FindObjectOfType<Manager>())
            {
                AddTunnelEvent -= FindObjectOfType<Manager>().onAddTunnel;
            }
            if (FindObjectOfType<Test.TunnelMaker>())
            {
                AddTunnelEvent -= FindObjectOfType<Test.TunnelMaker>().onAddTunnel;
            }
        }
    }
}