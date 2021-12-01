using UnityEngine;
using System;

namespace Tunnel
{    
    public class Straight : Tunnel
    {

        public delegate void BlockInterval(bool isBlockInterval, Vector3 blockPosition, Straight tunnel);
        public event BlockInterval BlockIntervalEvent;

        // Start is called before the first frame update
        void OnEnable()
        {
            FindObjectOfType<Manager>().GrowEvent += onGrow;
            FindObjectOfType<Manager>().StopEvent += onStop;
            BlockIntervalEvent += FindObjectOfType<Map.Manager>().onBlockInterval; // subscribe dig manager to the BlockSize event
            BlockIntervalEvent += FindObjectOfType<Turn>().onBlockInterval; // subscribe turn to the BlockSize event
            BlockIntervalEvent += FindObjectOfType<Worm.Movement>().onBlockInterval;  // subscribe worm so it can go to the center of the created block
        }

        new private void Awake()
        {
            base.Awake();

            rotation = new Rotation.Straight();

            isStopped = false;
            ingressPosition = transform.position;
            print("headLocation of " + gameObject.name + " is " + ingressPosition);
        }

        private void FixedUpdate()
        {
            if (!isStopped)
            {
                float length = getLength(); // transforms scale to length

                double roundedPosition = getRoundedPosition(length);
                bool isBlockMultiple = isRoundedPositionMultiple(roundedPosition);

                Vector3 unitVectorInDir = Dir.Vector.getUnitVectorFromDirection(ingressDirection);

                Vector3 position = transform.position;
                Vector3 deltaPosition = length * unitVectorInDir;

                Vector3 cellPosition = position + deltaPosition;


                if (isBlockMultiple)
                {
                    addCellToList(cellPosition.castToVector3Int()); // add new position to list of cell positions covered by straight tunnel
                    setCenter(length); // adjust the center to the new block
                    BlockIntervalEvent(isBlockMultiple, cellPosition, this);
                }
                else
                {
                    BlockIntervalEvent(isBlockMultiple, cellPosition, this);
                }
            }
        }

        /**
         * Stop subscribing to onGrow event
         */
        private void onStop()
        {
            if (!isStopped) // only unsubscribe from GrowEvent once
            {
                float scale = transform.localScale.y;
                isStopped = true;

                FindObjectOfType<Manager>().GrowEvent -= onGrow; // unsubscribe from Tunnel.Manager's Grow event
                BlockIntervalEvent -= FindObjectOfType<Map.Manager>().onBlockInterval; // unsubscribe map manager to the BlockSize event
                BlockIntervalEvent -= FindObjectOfType<Worm.Movement>().onBlockInterval;
            }            
        }

        private float getLength()
        {
            return (transform.localScale.y * BLOCK_SIZE * SCALE_TO_LENGTH); // scale of 1 : 2 meters or 0.5 : 1 meter
        }

        /**
         * Gets the rounded block size
         */
        protected double getRoundedPosition(float length)
        {
            double roundedScale = Math.Round(length * 1000f) / 1000f;
            return roundedScale;
        }

        /**
         * Returns true if distance along axi is a multiple of 1
         */
        protected bool isRoundedPositionMultiple(double roundedPosition)
        {
            return (roundedPosition % 1) == 0;
        }

        private void onGrow()
        {
            if (!isStopped)
            {
                Vector3 curScale = transform.localScale;
                float length = curScale.y + GROWTH_RATE;
                float roundedLength = (float)Math.Round(length, 2);
                transform.localScale = new Vector3(curScale.x, roundedLength, curScale.z);
            }
        }

        void OnDisable()
        {
            if (FindObjectOfType<Manager>()) // check if TunnelManager hasn't been deleted before this GO
            {
                FindObjectOfType<Manager>().StopEvent -= onStop;
                FindObjectOfType<Manager>().GrowEvent -= onGrow;
            }
            if (FindObjectOfType<Map.Manager>())
            {
                BlockIntervalEvent -= FindObjectOfType<Map.Manager>().onBlockInterval;
            }
            if (FindObjectOfType<Worm.Movement>())
            {
                BlockIntervalEvent -= FindObjectOfType<Worm.Movement>().onBlockInterval;
            }
        }
    }

}