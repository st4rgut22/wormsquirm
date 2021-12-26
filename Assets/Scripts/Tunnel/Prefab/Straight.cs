using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tunnel
{    
    public class Straight : Tunnel
    {
        public Direction growthDirection;
        private bool isDecision;

        public delegate void BlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Straight tunnel);
        public event BlockInterval BlockIntervalEvent;

        // Start is called before the first frame update
        void OnEnable()
        {
            FindObjectOfType<Manager>().GrowEvent += onGrow;

            FindObjectOfType<Manager>().StopEvent += onStop;
            FindObjectOfType<Intersect.Manager>().StopEvent += onStop;
            FindObjectOfType<Manager>().DecisionEvent += onDecision;

            BlockIntervalEvent += FindObjectOfType<Map.Manager>().onBlockInterval; // subscribe dig manager to the BlockSize event
            BlockIntervalEvent += FindObjectOfType<Turn>().onBlockInterval; // subscribe turn to the BlockSize event
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
            isDecision = false;
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
                float length = getLength(); // transforms scale to length

                bool isBlockMultiple = Map.Manager.isDistanceMultiple(length);

                Vector3 unitVectorInDir = Dir.Vector.getUnitVectorFromDirection(growthDirection);

                Vector3 position = transform.position;

                Vector3 deltaPosition = length * unitVectorInDir;
                Vector3 cellPosition = position + deltaPosition;

                if (isBlockMultiple)
                {
                    Vector3Int cell = getLastCellPosition().getNextVector3Int(growthDirection);

                    if (!isDecision)
                    {
                        addCellToList(cell); // add new position to list of cell positions covered by straight tunnel
                    }                    

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
         * When a turn is being made in the next cell set a flag, so we don't add a straight segment at the next cell interval (we will add a corner instaed)
         */
        public void onDecision(bool isStraightTunnel, Direction direction, Tunnel tunnel)
        {
            isDecision = true;
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
                FindObjectOfType<NewTunnelFactory>().AddTunnelEvent -= onAddTunnel;
                FindObjectOfType<Manager>().DecisionEvent -= onDecision;

                BlockIntervalEvent -= FindObjectOfType<Map.Manager>().onBlockInterval; // unsubscribe map manager to the BlockSize event
                BlockIntervalEvent -= FindObjectOfType<Worm.Movement>().onBlockInterval;
            }            
        }

        private float getLength()
        {
            return (transform.localScale.y * BLOCK_SIZE * SCALE_TO_LENGTH); // scale of 1 : 2 meters or 0.5 : 1 meter
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
            if (FindObjectOfType<Intersect.Manager>())
            {
                FindObjectOfType<Intersect.Manager>().StopEvent -= onStop;
            }
            if (FindObjectOfType<Test.TunnelMaker>())
            {
                BlockIntervalEvent -= FindObjectOfType<Test.TunnelMaker>().onBlockInterval;
            }
        }
    }

}