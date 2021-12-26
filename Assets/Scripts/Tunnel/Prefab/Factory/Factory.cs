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
            AddTunnelEvent += FindObjectOfType<Map.Manager>().onAddTunnel;
            AddTunnelEvent += FindObjectOfType<Manager>().onAddTunnel;
        }

        /**
         * Adds tunnel to list of tunnels and the map
         * 
         * @tunnel newly created tunnel
         */
        protected void addTunnel(Tunnel tunnel)
        {
            AddTunnelEvent(tunnel, cellMove.cell, directionPair);
            if (tunnel.tag == Type.STRAIGHT)
            {
                AddTunnelEvent(tunnel, cellMove.nextCell, directionPair); // add the next tunnel too because blockIntervalEvent doesn't trigger when initializing straight tunnel
            }
        }

        protected void OnDisable()
        {
            if (FindObjectOfType<Map.Manager>())
            {
                AddTunnelEvent -= FindObjectOfType<Map.Manager>().onAddTunnel;
            }
            if (FindObjectOfType<Manager>())
            {
                AddTunnelEvent -= FindObjectOfType<Manager>().onAddTunnel;
            }
        }
    }
}