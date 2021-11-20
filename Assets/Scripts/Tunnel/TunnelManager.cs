using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Worm;

namespace Tunnel
{
    public class TunnelManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject Tunnel;

        [SerializeField]
        private GameObject Corner;

        [SerializeField]
        private Transform TunnelNetwork;

        private static int cornerCount;

        private List<GameObject> TunnelList;

        public delegate void Grow();
        public event Grow GrowEvent;

        public delegate void Stop();
        public event Stop StopEvent;

        private void Awake()
        {
            cornerCount = 0;
            TunnelList = new List<GameObject>();
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            FindObjectOfType<WormController>().DigEvent += onDig;            
        }

        private GameObject getLastTunnel()
        {
            return TunnelList[TunnelList.Count - 1];
        }

        private Vector3 getTunnelStartPosition()
        {
            if (TunnelList.Count == 0) // if no tunnels exist, create the first tunnel at the origin
                return Vector3.zero;
            else
            {
                GameObject LastTunnelGO = getLastTunnel();
                return LastTunnelGO.GetComponent<Tunnel>().egressPosition;
            }
        }

        private void createTunnel(Direction tunnelDirection, Direction prevTunnelDirection)
        {
            Vector3 tunnelPosition = getTunnelStartPosition(); // get the egress position of the last straight tunnel (or use origin if creating the first tunnel)

            if (prevTunnelDirection != Direction.None) // A pair of tunnels must exist to create a corner
            {
                Quaternion cornerRotation = CornerRotation.getRotationFromDirection(prevTunnelDirection, tunnelDirection);
                GameObject CornerGO = Instantiate(Corner, tunnelPosition, cornerRotation, TunnelNetwork);
                CornerGO.GetComponent<CornerTunnel>().setEgressPosition(prevTunnelDirection, tunnelDirection);
                CornerGO.name = "Corner " + cornerCount;

                TunnelList.Add(CornerGO);
                tunnelPosition = getTunnelStartPosition(); // if a corner tunnel precedes a straight tunnel, the straight tunnel's ingress position is the egress hole
            }

            Quaternion tunnelRotation = TunnelRotation.getRotationFromDirection(tunnelDirection);
            GameObject TunnelGO = Instantiate(Tunnel, tunnelPosition, tunnelRotation, TunnelNetwork);
            TunnelList.Add(TunnelGO);
        }

        private void onDig(Direction direction, Direction prevDirection, bool isDirectionChanged) // subscribes to the user input to worm's direction
        {
            if (isDirectionChanged)
            {
                if (StopEvent != null) StopEvent();
                createTunnel(direction, prevDirection); // rotate tunnel in the direction
            }
            else
            {
                if (GrowEvent != null) GrowEvent();
            }
        }

        void OnDisable()
        {
            if (FindObjectOfType<WormController>()) FindObjectOfType<WormController>().DigEvent -= onDig;
        }

    }

}