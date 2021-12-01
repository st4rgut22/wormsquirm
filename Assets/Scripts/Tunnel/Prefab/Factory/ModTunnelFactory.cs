using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class ModTunnelFactory : Factory
    {

        /**
         * Given the previous direction, current direction and current tunnel create the next tunnel segment
         */
        public Tunnel getTunnel(DirectionPair directionPair, Tunnel curTunnel, GameObject gameObject, CellMove cellMove)
        {

            // get egress position from current tunnel and prevDirection to use as tunnel start position for new segment

            // rotate tunnel to line up with the egress Direction

            return null; // if the tunnel aready exists
        }
    }

}