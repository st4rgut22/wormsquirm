using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{    
    public class TunnelController : MonoBehaviour
    {
        [SerializeField]
        private float GROWTH_RATE;

        private Vector3 headLocation;
        private Vector3 tailLocation;
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
            headLocation = transform.position;
            print("headLocation of " + gameObject.name + " is " + headLocation);
            orientation = Rotation.getDirectionFromRotation(transform.rotation);
            print("orientation of " + gameObject.name + " is " + orientation);
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

        public Vector3 getTailLocation()
        {
            return tailLocation;
        }

        /**
         * Position of the end of the tunnel
         */
        private void setTailLocation()
        {
            float length = getLength();
            Vector3 deltaPosition = length * orientation;
            tailLocation = headLocation + deltaPosition;
            print("tail location of " + gameObject.name + " is " + tailLocation);
        }

        private float getLength()
        {
            return transform.localScale.y;
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