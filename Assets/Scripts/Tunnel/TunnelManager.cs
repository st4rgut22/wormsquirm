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
        private Transform TunnelNetwork;

        private List<GameObject> TunnelList;


        public delegate void Grow();
        public event Grow GrowEvent;

        public delegate void Stop();
        public event Stop StopEvent;

        private void Awake()
        {
            TunnelList = new List<GameObject>();
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            FindObjectOfType<WormController>().DigEvent += onDig;            
        }

        // Update is called once per frame
        void Update()
        {
            
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
                return LastTunnelGO.GetComponent<TunnelController>().getTailLocation();
            }
        }

        private void createTunnel(Vector3 position, Direction direction)
        {
            Quaternion rotation = RotationManager.getRotationFromDirection(direction);
            GameObject TunnelGO = Instantiate(Tunnel, position, rotation, TunnelNetwork);
            TunnelList.Add(TunnelGO);
        }

        private void onDig(Direction direction, bool isDirectionChanged) // subscribes to the user input to worm's direction
        {
            if (isDirectionChanged)
            {
                if (StopEvent != null) StopEvent();
                Vector3 tunnelPosition = getTunnelStartPosition();
                createTunnel(tunnelPosition, direction); // rotate tunnel in the direction
            }
            else if (direction != Direction.None)
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