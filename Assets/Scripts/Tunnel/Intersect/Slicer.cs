using UnityEngine;

namespace Intersect
{
    public class Slicer : MonoBehaviour
    {
        public delegate void AddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId);
        public event AddTunnel AddTunnelEvent;

        private void OnEnable()
        {
            AddTunnelEvent += Tunnel.TunnelManager.Instance.onAddTunnel;
        }

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
            Tunnel.Straight duplicateTunnel = collidedTunnel.copy(Tunnel.Type.instance.TunnelNetwork);

            Direction oppGrowthDirection = Dir.Base.getOppositeDirection(collidedTunnel.growthDirection);
            Vector3 sliceOppGrowthDirFace = getSlicedPosition(ingressDirection, oppGrowthDirection, contactPosition); // position of slice opposite growth direction
            updateTunnel(collidedTunnel, collidedTunnel.ingressPosition, sliceOppGrowthDirFace);

            Vector3 sliceGrowthDirFace = getSlicedPosition(ingressDirection, duplicateTunnel.growthDirection, contactPosition);  // position of slice in growth direction
            Vector3 collidedTunnelEgressPosition = duplicateTunnel.getEgressPosition(duplicateTunnel.growthDirection);
            updateTunnel(duplicateTunnel, sliceGrowthDirFace, collidedTunnelEgressPosition);

            AddTunnelEvent(duplicateTunnel, Vector3Int.zero, null, null); // add the duplicated segment to the list of tunnels in the network
        }

        /**
         * Update the tunnel's ingress/egress position
         */
        private void updateTunnel(Tunnel.Straight tunnel, Vector3 startSlicePosition, Vector3 endSlicePosition)
        {
            if (startSlicePosition.Equals(endSlicePosition) && tunnel.gameObject != null)
            {
                Destroy(tunnel.gameObject); // delete the tunnel if the slice results in a tunnel of zero length
            }
            else
            {
                tunnel.setIngressPosition(startSlicePosition);
                tunnel.updateCellPositionList(startSlicePosition.castToVector3Int(), endSlicePosition.castToVector3Int()); // updates cell position list and the new egress position
                trimTunnel(tunnel, startSlicePosition, endSlicePosition);
            }
        }

        /**
         * slice individual tunnel given the start and end positions
         * 
         * @tunnel              the tunnel being updated
         * @startSlicePosition  the start of the sliced tunnel
         * @endSlicePosition    the end of the sliced tunnel
         */
        private void trimTunnel(Tunnel.Straight tunnel, Vector3 startSlicePosition, Vector3 endSlicePosition)
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

        private void OnDisable()
        {
            if(Tunnel.TunnelManager.Instance)
            {
                AddTunnelEvent -= Tunnel.TunnelManager.Instance.onAddTunnel;
            }
        }

    }

}