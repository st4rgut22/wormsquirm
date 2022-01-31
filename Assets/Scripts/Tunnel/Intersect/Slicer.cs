using UnityEngine;

namespace Intersect
{
    public class Slicer : MonoBehaviour
    {

        /**
         * Slice the tunnel that has been hit
         * 
         * @collidedTunnel the tunnel that has been hit
         * @duplicateTunnel a duplicate of the hit tunnel
         * @ingressDirection direction of the hit tunnel
         * @contactPosition the position where the tunnel has been hit
         */
        public void sliceTunnel(Tunnel.Straight collidedTunnel, Direction ingressDirection, Vector3 contactPosition)
        {
            Tunnel.Tunnel duplicateTunnel = collidedTunnel.copy(Tunnel.Type.instance.TunnelNetwork);

            Direction sliceDirection1 = collidedTunnel.growthDirection;
            Vector3 slicedPosition1 = getSlicedPosition(ingressDirection, sliceDirection1, contactPosition);
                
            Direction sliceDirection2 = Dir.Base.getOppositeDirection(sliceDirection1);
            Vector3 slicedPosition2 = getSlicedPosition(ingressDirection, sliceDirection2, contactPosition);

            Vector3 collidedTunnelEgressPosition = Tunnel.Tunnel.getEgressPosition(collidedTunnel.growthDirection, collidedTunnel.center);

            trimTunnel(collidedTunnel, collidedTunnel.ingressPosition, slicedPosition2);
            trimTunnel(duplicateTunnel, slicedPosition1, collidedTunnelEgressPosition);
        }

        /**
         * slice individual tunnel given the start and end positions
         */
        private void trimTunnel(Tunnel.Tunnel tunnel, Vector3 startSlicePosition, Vector3 endSlicePosition)
        {
            float length = Vector3.Distance(startSlicePosition, endSlicePosition);
            float scaleY = length / (Tunnel.Tunnel.BLOCK_SIZE * Tunnel.Tunnel.SCALE_TO_LENGTH);
            print("start slice at " + startSlicePosition + " end slice at " + endSlicePosition + " scaleY is " + scaleY);
            tunnel.transform.position = startSlicePosition;
            tunnel.transform.localScale = new Vector3(tunnel.transform.localScale.x, scaleY, tunnel.transform.localScale.z);
        }

        /**
         * Get the position of the new end of the tunnel resulting from slice, using direction from perspective of sliced segment
         * 
         * @egressDirection is direction worm exits the junction
         * @ingressDirection is the direction worm enters the junction
         * @contactPosition is the point where current tunnel made contact with collided tunnel
         */
        private Vector3 getSlicedPosition(Direction ingressDirection, Direction egressDirection, Vector3 contactPosition)
        {
            Vector3 offset = Dir.Vector.getOffsetFromDirections(ingressDirection, egressDirection);
            return contactPosition + offset;
        }

    }

}