using UnityEngine;

namespace Tunnel
{
    public class Corner : Tunnel
    {
        private void Awake()
        {
            isStopped = true;
        }

        /**
         * Sets the position  of the egress, defined as the center of the exiting hole
         */
        public override void setEgressPosition()
        {
            Vector3 offset = CornerRotation.getOffsetFromDirections(ingressDirection, egressDirection);
            egressPosition = transform.position + offset;
            print("egress position is " + egressPosition);
        }

        public override void setDirection(Direction ingressDirection, Direction egressDirection)
        {
            base.setDirection(ingressDirection, egressDirection);
            setEgressPosition();
        }
    }
}