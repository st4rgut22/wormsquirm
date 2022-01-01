using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tunnel
{    
    public class Straight : Tunnel
    {
        public Direction growthDirection;

        public delegate void BlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Straight tunnel);
        public event BlockInterval BlockIntervalEvent;

        // Start is called before the first frame update
        new void OnEnable()
        {
            base.OnEnable();
            FindObjectOfType<CollisionManager>().StopEvent += onStop;

            BlockIntervalEvent += FindObjectOfType<Map>().onBlockInterval; // subscribe dig manager to the BlockSize event
            BlockIntervalEvent += Turn.Instance.onBlockInterval; // subscribe turn to the BlockSize event
            BlockIntervalEvent += FindObjectOfType<Worm.Movement>().onBlockInterval;  // subscribe worm so it can go to the center of the created block

            if (FindObjectOfType<Test.TunnelMaker>())
            {
                BlockIntervalEvent += FindObjectOfType<Test.TunnelMaker>().onBlockInterval;
            }
        }

        new private void Awake()
        {
            base.Awake();

            type = Type.Name.STRAIGHT;
            isStopped = false;
            ingressPosition = transform.position;
        }

        private void Start()
        {
            growthDirection = holeDirectionList[0];
        }

        private void FixedUpdate()
        {
            if (!isStopped)
            {
                grow();

                float length = getLength(); // transforms scale to length

                bool isBlockMultiple = Map.isDistanceMultiple(length);

                Vector3 unitVectorInDir = Dir.Vector.getUnitVectorFromDirection(growthDirection);

                Vector3 position = transform.position;

                Vector3 deltaPosition = length * unitVectorInDir;
                Vector3 cellPosition = position + deltaPosition;

                if (isBlockMultiple)
                {
                    Vector3Int cell = getLastCellPosition().getNextVector3Int(growthDirection);
                    addCellToList(cell); // add new position to list of cell positions covered by straight tunnel

                    setCenter(length, growthDirection); // adjust the center to the new block
                    BlockIntervalEvent(isBlockMultiple, cell, this);
                }
                else
                {
                    BlockIntervalEvent(isBlockMultiple, Vector3Int.zero, this);
                }
            }
        }

        public override void setHoleDirections(DirectionPair dirPair)
        {
            growthDirection = dirPair.curDir;
            Direction oppositeGrowthDirection = Dir.Base.getOppositeDirection(growthDirection);
            holeDirectionList = new List<Direction>() { growthDirection, oppositeGrowthDirection };
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
                FindObjectOfType<NewTunnelFactory>().AddTunnelEvent -= onAddTunnel;

                BlockIntervalEvent -= FindObjectOfType<Map>().onBlockInterval; // unsubscribe map manager to the BlockSize event
                BlockIntervalEvent -= FindObjectOfType<Worm.Movement>().onBlockInterval;
            }            
        }

        private float getLength()
        {
            return (transform.localScale.y * BLOCK_SIZE * SCALE_TO_LENGTH); // scale of 1 : 2 meters or 0.5 : 1 meter
        }

        /**
         *  Scale in direction of growth
         */
        private void grow()
        {
            Vector3 curScale = transform.localScale;
            float length = curScale.y + GROWTH_RATE;
            float roundedLength = (float)Math.Round(length, 2);
            transform.localScale = new Vector3(curScale.x, roundedLength, curScale.z);
        }

        void OnDisable()
        {
            if (FindObjectOfType<CollisionManager>()) // check if TunnelManager hasn't been deleted before this GO
            {
                FindObjectOfType<CollisionManager>().StopEvent -= onStop;
            }
            if (FindObjectOfType<Map>())
            {
                BlockIntervalEvent -= FindObjectOfType<Map>().onBlockInterval;
            }
            if (FindObjectOfType<Worm.Movement>())
            {
                BlockIntervalEvent -= FindObjectOfType<Worm.Movement>().onBlockInterval;
            }
            if (FindObjectOfType<Test.TunnelMaker>())
            {
                BlockIntervalEvent -= FindObjectOfType<Test.TunnelMaker>().onBlockInterval;
            }
        }

        /**
         * When collide with another tunnel get the point of contact
         */
        public override Vector3 getContactPosition(DirectionPair dirPair)
        {
            return getEgressPosition(dirPair.prevDir, center);
        }
    }

}