using UnityEngine;

namespace Tunnel
{
    public class Slicer : System
    {

        /**
         * Event listener for the tunnel collision. Tunnel name is name of GO that caused collision
         * On intersect with a tunnel segment, slice that segment into 2 shorter segments and a junction 
         */
        public void onSlice(Tunnel curTunnel, Collision collision)
        {
            GameObject collisionGO = collision.gameObject;
            if (collisionGO.name == "Tunnel 0" && curTunnel.name == "Tunnel 3")
            {
                print("slice tunnel 0");
                Tunnel collidedTunnel = collisionGO.GetComponent<Tunnel>();
                Tunnel duplicateTunnel = collidedTunnel.copy(TunnelNetwork);

                sliceTunnelPair(curTunnel, collidedTunnel, duplicateTunnel);
            }

        }

        /**
         * Slice the tunnel 
         */
        private void sliceTunnelPair(Tunnel curTunnel, Tunnel collidedTunnel, Tunnel duplicateTunnel)
        {
            curTunnel.setEgressPosition(); // set the point of contact as the egress position
            Vector3 contactPosition = curTunnel.egressPosition; // point of contact between cur tunnel and collided tunnel used to instantiate junction

            Direction ingressDirection = curTunnel.ingressDirection;
            Direction sliceDirection1 = collidedTunnel.ingressDirection;
            Direction sliceDirection2 = Dir.getOppositeDirection(sliceDirection1);

            Vector3 slicedPosition1 = getSlicedPosition(ingressDirection, sliceDirection1, contactPosition);
            Vector3 slicedPosition2 = getSlicedPosition(ingressDirection, sliceDirection2, contactPosition);

            print("sliced position 1 is " + slicedPosition1 + " sliced position 2 is " + slicedPosition2);
            slice(collidedTunnel, collidedTunnel.ingressPosition, slicedPosition2);
            slice(duplicateTunnel, slicedPosition1, collidedTunnel.egressPosition);

            Instantiate(FourwayTunnel, contactPosition, Quaternion.identity, TunnelNetwork);
        }

        /**
         * slice individual tunnel given the start and end positions
         */
        private void slice(Tunnel tunnel, Vector3 startSlicePosition, Vector3 endSlicePosition)
        {
            float length = Vector3.Distance(startSlicePosition, endSlicePosition);
            float scaleY = length / Tunnel.BLOCK_SIZE;
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
            Vector3 offset = CornerRotation.getOffsetFromDirections(ingressDirection, egressDirection);
            return contactPosition + offset;
        }

    }

}