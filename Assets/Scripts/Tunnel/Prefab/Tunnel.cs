using System;
using UnityEngine;

namespace Tunnel
{
    public abstract class Tunnel : MonoBehaviour
    {
        protected double GROWTH_RATE; // must be a divisor of 1 so tunnel length will be a multiple of BLOCK_SIZE
        private int GROWTH_EXP = 1;

        public const int BLOCK_SIZE = 2;
        public const float OFFSET_SIZE = 0.5f;

        public bool isStopped;

        public static string TAG = "tunnel";

        public Vector3 ingressPosition { get; protected set; }
        public Vector3 egressPosition { get; protected set; }

        public Direction ingressDirection { get; protected set; }
        public Direction egressDirection { get; protected set; }

        public abstract void setEgressPosition();

        public delegate void Collide(Tunnel tunnelGO, Collision collision);
        public event Collide CollideEvent;

        protected void Awake()
        {
            GROWTH_RATE = 1 / (Math.Pow(10, GROWTH_EXP));
        }

        protected void OnEnable()
        {
            CollideEvent += FindObjectOfType<Slicer>().onSlice; // slicer is listening for collide events
            CollideEvent += FindObjectOfType<Manager>().onSlice;
        }

        public Tunnel copy(Transform tunnelParent)
        {
            GameObject tunnelCopy = Instantiate(gameObject, tunnelParent);
            Tunnel copiedTunnel = tunnelCopy.GetComponent<Tunnel>();
            copiedTunnel.isStopped = true; // copied tunnel should not grow
            copiedTunnel.ingressPosition = ingressPosition;
            copiedTunnel.egressPosition = egressPosition;
            return copiedTunnel;
        }

        public virtual void setDirection(Direction ingressDirection, Direction egressDirection)
        {
            this.ingressDirection = ingressDirection;
            this.egressDirection = egressDirection;
        }

        /**
         * Returns true if length of tunnel is a multiple of 1 and hence the BLOCK_SIZE
         */
        protected bool isBlockSizeMultiple()
        {
            double roundedScale = Math.Round(transform.localScale.y / GROWTH_RATE) * GROWTH_RATE;
            print("local scale y is " + transform.localScale.y + " rounded scale is " + roundedScale);
            return (roundedScale % 1) == 0;
        }

        /**
         * When a tunnel has collided with another notify slice
         */
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == TAG) // if collided with another tunnel
            {
                if (CollideEvent != null)
                {
                    print("collision occurred between " + gameObject.name + " and " + collision.gameObject.name);
                    CollideEvent(this, collision);
                }
            }
        }

        private void OnDisable()
        {
            CollideEvent -= FindObjectOfType<Slicer>().onSlice;
            CollideEvent -= FindObjectOfType<Manager>().onSlice;
        }
    }
}