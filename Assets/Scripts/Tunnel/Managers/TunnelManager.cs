using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class TunnelManager : GenericSingletonClass<TunnelManager>
    {
        private List<Tunnel> TunnelList; // list consisting of straight tunnels and corner tunnels

        public Vector3Int initialCell = Vector3Int.zero; // initial cell

        [SerializeField]
        public float RING_OFFSET = .5f; 

        public float START_TUNNEL_RING_OFFSET;

        public Vector3Int startingCell;

        private new void Awake()
        {
            base.Awake();
            START_TUNNEL_RING_OFFSET = 1 - RING_OFFSET; // Distance between start of tunnel and worm ring
            TunnelList = new List<Tunnel>();
        }

        /**
         * Reset the tunnel network state by destroying all tunnels
         */
        public void onDestroyTunnelNetwork()
        {
            TunnelList.ForEach((Tunnel tunnel) =>
            {
                Destroy(tunnel.gameObject);
            });
        }

        public void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            TunnelList.Add(tunnel);
        }

        public Tunnel getLastTunnel()
        {
            if (TunnelList.Count == 0)
            {
                return null;
            }
            else
            {
                return TunnelList[TunnelList.Count - 1];
            }
        }

        /**
         * Get the prefab with the correct orientation
         */
        public Transform GetPrefabFromHoleList(Direction ingressDir, List<Direction> holeDirList, Transform rotationParent)
        {

            foreach (Transform prefabOrientation in rotationParent)
            {
                Rotation.Rotation rotation = prefabOrientation.gameObject.GetComponent<Rotation.Rotation>();
                if (rotation.isRotationInRotationDict(ingressDir, holeDirList))
                {
                    return prefabOrientation;
                }
            }
            throw new System.Exception("no prefab exists with ingressdir " + ingressDir + " hole list " + holeDirList.ToString());
        }
    }
}