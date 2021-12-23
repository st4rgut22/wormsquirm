using UnityEngine;

namespace Tunnel
{
    public abstract class Factory : MonoBehaviour
    {
        public delegate void AddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair);
        public event AddTunnel AddTunnelEvent;

        public delegate void InitWormPosition(Vector3 position);
        public event InitWormPosition InitWormPositionEvent;

        protected CellMove cellMove;
        protected DirectionPair directionPair;

        public abstract Tunnel getTunnel();

        protected void OnEnable()
        {
            AddTunnelEvent += FindObjectOfType<Map.Manager>().onAddTunnel;
            AddTunnelEvent += FindObjectOfType<Manager>().onAddTunnel;
            InitWormPositionEvent += FindObjectOfType<Worm.Movement>().onInitWormPosition;
        }

        /**
         * Adds tunnel to list of tunnels and the map
         * 
         * @tunnel newly created tunnel
         */
        protected void addTunnel(Tunnel tunnel)
        {
            AddTunnelEvent(tunnel, cellMove.cell, directionPair);
            if (cellMove.isInit) // at the beginning of the game add the cell we automatically started off with
            {
                InitWormPositionEvent(cellMove.startPosition);
                AddTunnelEvent(tunnel, cellMove.nextCell, directionPair);
            }
        }

        protected void OnDisable()
        {
            if (FindObjectOfType<Map.Manager>())
            {
                AddTunnelEvent -= FindObjectOfType<Map.Manager>().onAddTunnel;
            }
            if (FindObjectOfType<Worm.Movement>())
            {
                InitWormPositionEvent -= FindObjectOfType<Worm.Movement>().onInitWormPosition;
            }
            if (FindObjectOfType<Manager>())
            {
                AddTunnelEvent -= FindObjectOfType<Manager>().onAddTunnel;
            }
        }
    }
}