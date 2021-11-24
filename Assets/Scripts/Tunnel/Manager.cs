using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Manager : System
    {
        private static int cornerCount;

        private List<GameObject> TunnelList; // list consisting of straight tunnels and corner tunnels
        private List<GameObject> StraightTunnelList;

        public delegate void Grow();
        public event Grow GrowEvent;

        public delegate void Stop();
        public event Stop StopEvent;

        private void Awake()
        {
            cornerCount = 0;
            TunnelList = new List<GameObject>();
            StraightTunnelList = new List<GameObject>();
        }

        private void OnEnable()
        {
            FindObjectOfType<DigManager>().CreateTunnelEvent += onCreateTunnel;
        }

        private void FixedUpdate()
        {
            if (GrowEvent != null)
            {
                GrowEvent();
            }
        }

        private GameObject getLastTunnel(List<GameObject> tunnelList)
        {
            return tunnelList[tunnelList.Count - 1];
        }

        private Vector3 getTunnelStartPosition()
        {
            if (TunnelList.Count == 0) // if no tunnels exist, create the first tunnel at the origin
                return Vector3.zero;
            else
            {
                GameObject LastTunnelGO = getLastTunnel(TunnelList);
                return LastTunnelGO.GetComponent<Tunnel>().egressPosition;
            }
        }

        public void onSlice(Tunnel tunnelGO, Collision collision)
        {
            if (StopEvent != null && collision.gameObject.name == "Tunnel 0" && tunnelGO.name == "Tunnel 3")
            {
                print("stop tunnel 0");
                StopEvent(); // Stop the last growing tunnel if a collision occurred
            }
        }

        private void onCreateTunnel(DirectionPair directionPair)
        {
            Direction prevTunnelDirection = directionPair.prevDir;
            Direction tunnelDirection = directionPair.curDir;

            if (StopEvent != null)
            {
                StopEvent(); // Stop the last growing tunnel
            }

            Vector3 tunnelPosition = getTunnelStartPosition(); // get the egress position of the last straight tunnel (or use origin if creating the first tunnel)

            if (prevTunnelDirection != Direction.None) // A pair of tunnels must exist to create a corner
            {
                Quaternion cornerRotation = CornerRotation.getRotationFromDirection(prevTunnelDirection, tunnelDirection);
                GameObject CornerGO = Instantiate(Corner, tunnelPosition, cornerRotation, TunnelNetwork);
                CornerGO.GetComponent<Corner>().setDirection(prevTunnelDirection, tunnelDirection);
                CornerGO.name = "Corner " + cornerCount;

                TunnelList.Add(CornerGO);
                tunnelPosition = getTunnelStartPosition(); // if a corner tunnel precedes a straight tunnel, the straight tunnel's ingress position is the egress hole
            }

            Quaternion tunnelRotation = TunnelRotation.getRotationFromDirection(tunnelDirection);
            GameObject TunnelGO = Instantiate(StraightTunnel, tunnelPosition, tunnelRotation, TunnelNetwork);

            TunnelList.Add(TunnelGO);
            TunnelGO.name = "Tunnel " + cornerCount;
            StraightTunnelList.Add(TunnelGO);
            cornerCount += 1;
        }

        private void OnDisable()
        {
            if (FindObjectOfType<DigManager>())
            {
                FindObjectOfType<DigManager>().CreateTunnelEvent -= onCreateTunnel;
            }            
        }
    }
}