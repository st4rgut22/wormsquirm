using UnityEngine;

namespace Tunnel
{
    public class CornerTunnel : Tunnel
    {
        /**
         * Sets the position  of the egress, defined as the center of the exiting hole
         */
        public void setEgressPosition(Direction ingressTunnelDirection, Direction egressTunnelDirection)
        {
            Vector3 ingressUnitVector = CornerRotation.getUnitVectorFromDirection(ingressTunnelDirection);
            Vector3 egressUnitVector = CornerRotation.getUnitVectorFromDirection(egressTunnelDirection);
            Vector3 offset = (ingressUnitVector + egressUnitVector) / 2;
            egressPosition = transform.position + offset;
            print("egress position is " + egressPosition);
        }
    }
}