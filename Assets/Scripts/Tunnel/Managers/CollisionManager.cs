using UnityEngine;

namespace Tunnel
{
    public class CollisionManager : MonoBehaviour
    {
        public delegate void SliceTunnel(Straight collidedTunnel, Direction ingressDirection, Vector3 contactPosition);
        private event SliceTunnel SliceTunnelEvent; // fired when current tunnel intersects another tunnel

        public delegate void Stop();
        public event Stop StopEvent;

        public delegate void CreateJunction(Tunnel collisionTunnel, DirectionPair dirPair, CellMove cellMove);
        private event CreateJunction CreateJunctionEvent;

        // Start is called before the first frame update
        protected void OnEnable()
        {
            SliceTunnelEvent += FindObjectOfType<Intersect.Slicer>().sliceTunnel;
            CreateJunctionEvent += FindObjectOfType<ModTunnelFactory>().onCreateJunction;
            CreateJunctionEvent += FindObjectOfType<Worm.Movement>().onCreateJunction;
        }

        /**
         * Event listener for the tunnel collision. Tunnel.Tunnel name is name of GO that caused collision
         * 
         * On intersect with a tunnel segment, create a junction and slice segment (if necessary)
         * Notify worm about new tunnel so it can keep track of blockInterval instead of tunnel
         */
        protected void collide(DirectionPair directionPair, Tunnel curTunnel, Tunnel nextTunnel)
        {
            Direction exitDirection = directionPair.curDir;

            Vector3 contactPosition = curTunnel.getContactPosition(directionPair);
            CellMove cellMove = CellMove.getCellMove(curTunnel, directionPair);
            if (!cellMove.startPosition.Equals(contactPosition))
            {
                throw new System.Exception("vectors are not equivalent");
            }

            if (nextTunnel != null)
            {
                if (nextTunnel.type == Type.Name.STRAIGHT)
                {
                    SliceTunnelEvent((Straight)nextTunnel, exitDirection, contactPosition);
                }
                CreateJunctionEvent(nextTunnel, directionPair, cellMove);
            }
            StopEvent();
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Intersect.Slicer>())
            {
                SliceTunnelEvent -= FindObjectOfType<Intersect.Slicer>().sliceTunnel;
            }
            if (FindObjectOfType<ModTunnelFactory>())
            {
                CreateJunctionEvent -= FindObjectOfType<ModTunnelFactory>().onCreateJunction;
            }
            if (FindObjectOfType<Worm.Movement>())
            {
                CreateJunctionEvent -= FindObjectOfType<Worm.Movement>().onCreateJunction;
            }
        }
    }

}