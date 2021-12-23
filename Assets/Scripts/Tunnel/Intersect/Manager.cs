using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Intersect
{
    public class Manager : MonoBehaviour
    {

        public delegate void SliceTunnel(Tunnel.Straight collidedTunnel, Direction ingressDirection, Vector3 contactPosition);
        private event SliceTunnel SliceTunnelEvent; // fired when current tunnel intersects another tunnel

        public delegate void CreateJunction(Tunnel.Tunnel collisionTunnel, DirectionPair dirPair, Tunnel.CellMove cellMove);
        private event CreateJunction CreateJunctionEvent;

        public delegate void Stop();
        public event Stop StopEvent;

        // Start is called before the first frame update
        void OnEnable()
        {
            SliceTunnelEvent += FindObjectOfType<Slicer>().sliceTunnel;
            CreateJunctionEvent += FindObjectOfType<Tunnel.ModTunnelFactory>().onCreateJunction;
        }

        /**
         * Event listener for the tunnel collision. Tunnel.Tunnel name is name of GO that caused collision
         * On intersect with a tunnel segment, create a junction and slice segment (if necessary)
         * Notify worm about new tunnel so it can keep track of blockInterval instead of tunnel
         */
        public void onSlice(DirectionPair directionPair, Tunnel.Tunnel curTunnel, Tunnel.Tunnel nextTunnel)
        {
            Direction exitDirection = directionPair.curDir;
            
            Vector3 contactPosition = Tunnel.Tunnel.getEgressPosition(exitDirection, curTunnel.center);
            Tunnel.CellMove cellMove = Tunnel.CellMove.getCellMove(curTunnel, directionPair);
            if (!cellMove.startPosition.Equals(contactPosition))
            {
                throw new System.Exception("vectors are not equivalent");
            }

            StopEvent(); // stop the currently growing tunnel

            if (nextTunnel != null)
            {
                if (nextTunnel.type == Tunnel.Type.Name.STRAIGHT)
                {
                    SliceTunnelEvent((Tunnel.Straight) nextTunnel, exitDirection, contactPosition);
                }
                CreateJunctionEvent(nextTunnel, directionPair, cellMove);
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Slicer>())
            {
                SliceTunnelEvent -= FindObjectOfType<Slicer>().sliceTunnel;
            }
            if (FindObjectOfType<Tunnel.ModTunnelFactory>())
            {
                CreateJunctionEvent -= FindObjectOfType<Tunnel.ModTunnelFactory>().onCreateJunction;
            }
        }
    }

}