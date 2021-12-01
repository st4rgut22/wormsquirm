using UnityEngine;

namespace Tunnel
{
    public class Slicer : MonoBehaviour
    {
        /**
         * Event listener for the tunnel collision. Tunnel name is name of GO that caused collision
         * On intersect with a tunnel segment, slice that segment into 2 shorter segments and a junction 
         */
        public void onSlice(Tunnel curTunnel, Tunnel nextTunnel, DirectionPair dirPair)
        {
            GameObject collisionGO = nextTunnel.gameObject;
            if (collisionGO.name == "Tunnel 0" && curTunnel.name == "Tunnel 3")
            {
                Tunnel collidedTunnel = collisionGO.GetComponent<Tunnel>();
                Tunnel duplicateTunnel = collidedTunnel.copy(Env.instance.TunnelNetwork);

                sliceTunnelPair(curTunnel, collidedTunnel, duplicateTunnel, dirPair);
            }

        }

        /**
         * Slice the tunnel 
         */
        private void sliceTunnelPair(Tunnel curTunnel, Tunnel collidedTunnel, Tunnel duplicateTunnel, DirectionPair dirPair)
        {
            Vector3 contactPosition = curTunnel.getEgressPosition(dirPair.curDir) ; // point of contact between cur tunnel and collided tunnel used to instantiate junction

            Direction ingressDirection = curTunnel.ingressDirection;
            Direction sliceDirection1 = collidedTunnel.ingressDirection;
            Direction sliceDirection2 = Dir.Base.getOppositeDirection(sliceDirection1);

            Vector3 slicedPosition1 = getSlicedPosition(ingressDirection, sliceDirection1, contactPosition);
            Vector3 slicedPosition2 = getSlicedPosition(ingressDirection, sliceDirection2, contactPosition);

            Vector3 collidedTunnelEgressPosition= collidedTunnel.getEgressPosition(collidedTunnel.ingressDirection);

            print("sliced position 1 is " + slicedPosition1 + " sliced position 2 is " + slicedPosition2);
            slice(collidedTunnel, collidedTunnel.ingressPosition, slicedPosition2);
            slice(duplicateTunnel, slicedPosition1, collidedTunnelEgressPosition);

            Instantiate(Env.instance.FourwayTunnel, contactPosition, Quaternion.identity, Env.instance.TunnelNetwork);
        }

        /**
         * slice individual tunnel given the start and end positions
         */
        private void slice(Tunnel tunnel, Vector3 startSlicePosition, Vector3 endSlicePosition)
        {
            float length = Vector3.Distance(startSlicePosition, endSlicePosition);
            float scaleY = length / (Tunnel.BLOCK_SIZE * Tunnel.SCALE_TO_LENGTH);
            print("start slice at " + startSlicePosition + " end slice at " + endSlicePosition + " scaleY is " + scaleY);
            tunnel.transform.position = startSlicePosition;
            tunnel.transform.localScale = new Vector3(tunnel.transform.localScale.x, scaleY, tunnel.transform.localScale.z);
        }

        /**
         * Get the position of the new end of the tunnel resulting from slice, using direction from perspective of sliced segment
         * contactPosition is the point where current tunnel made contact with collided tunnel
         */
        private Vector3 getSlicedPosition(Direction ingressDirection, Direction egressDirection, Vector3 contactPosition)
        {
            Vector3 offset = Dir.Vector.getOffsetFromDirections(ingressDirection, egressDirection);
            return contactPosition + offset;
        }

    }

}