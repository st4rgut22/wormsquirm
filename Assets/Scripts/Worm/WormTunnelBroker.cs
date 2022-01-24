using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormTunnelBroker : WormBody
    {
        public delegate void Grow(Vector3 ringPosition);
        public event Grow GrowEvent;

        Tunnel.Straight prevStraightTunnel;
        Tunnel.Tunnel prevTunnel;

        private void OnEnable()
        {
            Tunnel.CollisionManager.Instance.CreateJunctionEvent += onCreateJunction;
            FindObjectOfType<Tunnel.Factory>().AddTunnelEvent += onAddTunnel;
        }

        // Start is called before the first frame update
        void Start()
        {
            prevStraightTunnel = null;
            prevTunnel = null;
        }

        /**
         * Issue an event that grows the current tunnel
         */
        private void FixedUpdate()
        {
            if (GrowEvent != null)
            {
                GrowEvent(ring.position);
            }
        }

        public Tunnel.Tunnel getCurTunnel(Direction direction)
        {
            if (!isLeadingTunnel)
            {
                return Tunnel.TunnelManager.Instance.getCurrentTunnel(ring.position, direction);
            }
            else
            {
                return prevTunnel;
            }
        }


        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            if (wormId == this.wormId)
            {
                isLeadingTunnel = true; // worm is creating a tunnel so it is leading it

                prevTunnel = tunnel;
                bool isTunnelStraight = tunnel.isTunnelType(Tunnel.Type.Name.STRAIGHT);
                bool isTunnelCorner = tunnel.isTunnelType(Tunnel.Type.Name.CORNER);

                // unregister the previous tunnel's grow event, and register the new one
                if (prevStraightTunnel != null)
                {
                    GrowEvent -= prevStraightTunnel.onGrow;
                    prevStraightTunnel = null;
                }
                if (isTunnelStraight)
                {
                    prevStraightTunnel = (Tunnel.Straight)(tunnel);
                    GrowEvent += prevStraightTunnel.onGrow;
                }
                if (isTunnelCorner)
                {
                    Tunnel.Corner corner = (Tunnel.Corner)(tunnel);
                    GetComponent<Movement>().setCompleteTurnDelegate(corner); // set handler for complete turn
                }
            }
        }

        /**
         * If worm created a junction, it is entering an existing tunnel so set leading flag to false
         * 
         * @currentTunnel is the tunnel prior to the junction, whose creator is also entering the junction
         */
        public void onCreateJunction(Tunnel.Tunnel collisionTunnel, DirectionPair dirPair, Tunnel.CellMove cellMove, Tunnel.Tunnel prevTunnel)
        {
            if (prevTunnel.wormCreatorId == wormId)
            {
                isLeadingTunnel = false;
            }
        }


        private void OnDisable()
        {
            if (FindObjectOfType<Tunnel.Factory>())
            {
                FindObjectOfType<Tunnel.Factory>().AddTunnelEvent -= onAddTunnel;
            }
            if (Tunnel.CollisionManager.Instance)
            {
                Tunnel.CollisionManager.Instance.CreateJunctionEvent -= onCreateJunction;
            }
        }
    }

}