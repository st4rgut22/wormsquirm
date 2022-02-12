using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class TunnelManager : GenericSingletonClass<TunnelManager>
    {
        public delegate void Stop();
        public event Stop StopEvent;

        private List<Tunnel> TunnelList; // list consisting of straight tunnels and corner tunnels
        private List<Straight> GrowingTunnelList; // list of straight tunnels that are currently growing

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
            GrowingTunnelList = new List<Straight>();
        }

        /**
         * Reset the tunnel network state by stopping all growing tunnels and clearing tunnel network
         */
        public void onResetTunnelNetwork()
        {
            GrowingTunnelList.ForEach((Straight tunnel) =>
            {
                stopTunnel(tunnel);
            });

            TunnelList.ForEach((Tunnel tunnel) =>
            {
                if (tunnel != null) // check if tunnel hasn't been already destroyed
                {
                    Destroy(tunnel.gameObject);
                }
            });

            GrowingTunnelList.Clear();
            TunnelList.Clear();
        }

        /**
         * Remove stopped tunnel from list of growing tunnels, and notify the affected tunnel
         */
        public void onStop(Straight straightTunnel)
        {
            if (GrowingTunnelList.Contains(straightTunnel))
            {
                print("removing " + straightTunnel.name + " from growing list");
                GrowingTunnelList.Remove(straightTunnel);
                stopTunnel(straightTunnel);
            }
        }

        private void stopTunnel(Straight tunnel)
        {
            StopEvent += tunnel.onStop;
            StopEvent();
            StopEvent -= tunnel.onStop;
        }

        public void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            TunnelList.Add(tunnel);

            if (tunnel.type == Type.Name.STRAIGHT)
            {
                print("adding tunnel " + tunnel.name + " to growing tunnel listT");
                GrowingTunnelList.Add((Straight) tunnel);
            }
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