using UnityEngine;

namespace Tunnel
{    
    public class Straight : Tunnel
    {
        private Vector3 orientation;

        public delegate void BlockSize(bool isBlockSize);
        public event BlockSize BlockSizeEvent;

        // Start is called before the first frame update
        void OnEnable()
        {
            base.OnEnable();
            FindObjectOfType<Manager>().GrowEvent += onGrow;
            FindObjectOfType<Manager>().StopEvent += onStop;
        }

        new private void Awake()
        {
            base.Awake();
            isStopped = false;
            ingressPosition = transform.position;
            print("headLocation of " + gameObject.name + " is " + ingressPosition);
            orientation = TunnelRotation.getUnitVectorFromRotation(transform.rotation);

            Direction direction = TunnelRotation.getDirectionFromRotation(transform.rotation);
            setDirection(direction, direction); // ingress dir is same as egress dir          

            print("orientation of " + gameObject.name + " is " + orientation);

            BlockSizeEvent += FindObjectOfType<DigManager>().onBlockSize; // subscribe dig manager to the BlockSize event

        }

        private void FixedUpdate()
        {
            if (!isStopped && BlockSizeEvent != null)
            {
                if (isBlockSizeMultiple())
                {
                    print("sending block size event");
                    BlockSizeEvent(true);
                }
                else
                {
                    BlockSizeEvent(false);
                }
            }
        }

        /**
         * Set position at end of tunnel
         */
        public override void setEgressPosition()
        {
            float length = getLength();
            Vector3 deltaPosition = length * orientation;
            Vector3 egressPosition = ingressPosition + deltaPosition;

            this.egressPosition = egressPosition;
        }

        /**
         * Stop subscribing to onGrow event
         */
        private void onStop()
        {
            if (!isStopped) // only unsubscribe from GrowEvent once
            {
                isStopped = true;
                setEgressPosition();

                FindObjectOfType<Manager>().GrowEvent -= onGrow; // unsubscribe from Tunnel.Manager's Grow event
                BlockSizeEvent -= FindObjectOfType<DigManager>().onBlockSize; // unsubscribe dig manager to the BlockSize event
            }            
        }

        private void setLength(float length)
        {
            transform.localScale = new Vector3(transform.localScale.x, length, transform.localScale.z);
        }

        private float getLength()
        {
            return transform.localScale.y * BLOCK_SIZE; // scale of 1 : 2 meters or 0.5 : 1 meter
        }

        private void onGrow()
        {
            if (!isStopped)
            {
                Vector3 curScale = transform.localScale;
                float length = (float)(curScale.y + GROWTH_RATE);
                transform.localScale = new Vector3(curScale.x, length, curScale.z);
            }
        }

        void OnDisable()
        {
            if (FindObjectOfType<Manager>()) // check if TunnelManager hasn't been deleted before this GO
            {
                FindObjectOfType<Manager>().StopEvent -= onStop;
                FindObjectOfType<Manager>().GrowEvent -= onGrow;
            }            
        }
    }

}