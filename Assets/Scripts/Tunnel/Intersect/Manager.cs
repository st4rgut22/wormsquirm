using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Intersect
{
    public class Manager : MonoBehaviour
    {

        public delegate void SliceTunnel(Tunnel.Straight collidedTunnel, Direction ingressDirection, Vector3 contactPosition);
        private event SliceTunnel SliceTunnelEvent; // fired when current tunnel intersects another tunnel

        public delegate void Stop();
        public event Stop StopEvent;

        // Start is called before the first frame update
        void OnEnable()
        {
            SliceTunnelEvent += FindObjectOfType<Slicer>().sliceTunnel;
        }

        /**
         * Event listener for the tunnel collision. Tunnel.Tunnel name is name of GO that caused collision
         * On intersect with a tunnel segment, create a junction and slice segment (if necessary)
         * Notify worm about new tunnel so it can keep track of blockInterval instead of tunnel
         */
        public void onSlice(Tunnel.Straight curTunnel, Tunnel.Tunnel nextTunnel)
        {
            Tunnel.Factory.modTunnelFactory.getTunnel(curTunnel, nextTunnel); // create the appropriate junction

            Direction exitDirection = curTunnel.growthDirection;
            Vector3 contactPosition = Tunnel.Tunnel.getEgressPosition(exitDirection, curTunnel.center);

            StopEvent(); // stop the currently growing tunnel

            if (nextTunnel != null)
            {
                if (nextTunnel.type == Tunnel.Type.Name.STRAIGHT)
                {
                    SliceTunnelEvent((Tunnel.Straight) nextTunnel, exitDirection, contactPosition);
                }
                else
                {

                }
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Slicer>())
            {
                SliceTunnelEvent -= FindObjectOfType<Slicer>().sliceTunnel;
            }
        }
    }

}