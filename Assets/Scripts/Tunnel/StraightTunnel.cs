using UnityEngine;

namespace Tunnel
{    
    public class StraightTunnel : Tunnel
    {
        [SerializeField]
        private float GROWTH_RATE;

        private Vector3 orientation;

        private bool isStopped;

        // Start is called before the first frame update
        void OnEnable()
        {
            FindObjectOfType<TunnelManager>().GrowEvent += onGrow;
            FindObjectOfType<TunnelManager>().StopEvent += onStop;
        }

        private void Awake()
        {
            isStopped = false;
            ingressPosition = transform.position;
            print("headLocation of " + gameObject.name + " is " + ingressPosition);
            orientation = TunnelRotation.getUnitVectorFromRotation(transform.rotation);
            egressDirection = ingressDirection = TunnelRotation.getDirectionFromRotation(transform.rotation);
            print("orientation of " + gameObject.name + " is " + orientation);
        }

        public void setEgressPosition(Vector3 deltaPosition)
        {
            egressPosition = ingressPosition + deltaPosition;
        }

        /**
         * Stop subscribing to onGrow event
         */
        private void onStop()
        {
            if (!isStopped) // only unsubscribe from GrowEvent once
            {
                isStopped = true;
                setTailLocation();
                FindObjectOfType<TunnelManager>().GrowEvent -= onGrow;
            }            
        }

        /**
         * Position of the end of the tunnel
         */
        private void setTailLocation()
        {
            float length = getLength();
            Vector3 deltaPosition = length * orientation;
            setEgressPosition(deltaPosition);
            print("tail location of " + gameObject.name + " is " + egressPosition);
        }

        private float getLength()
        {
            return transform.localScale.y * Tunnel.BLOCK_SIZE; // scale of 1 : 2 meters or 0.5 : 1 meter
        }

        private void onGrow()
        {
            Vector3 curScale = transform.localScale;
            float length = curScale.y + GROWTH_RATE;
            transform.localScale = new Vector3(curScale.x, length, curScale.z);
        }

        void OnDisable()
        {
            if (FindObjectOfType<TunnelManager>()) // check if TunnelManager hasn't been deleted before this GO
            {
                FindObjectOfType<TunnelManager>().StopEvent -= onStop;
                FindObjectOfType<TunnelManager>().GrowEvent -= onGrow;
            }            
        }
    }

}