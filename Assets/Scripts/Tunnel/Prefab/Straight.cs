using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tunnel
{    
    public class Straight : Tunnel
    {
        private GameObject DeadEndInstance;
        public Direction growthDirection;

        Vector3Int lastCell;
        int lastBlockLen; // last block added used to retroactively add blocks that did not fall on an interval

        public delegate void BlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Straight tunnel);
        public event BlockInterval BlockIntervalEvent;

        // Start is called before the first frame update
        new void OnEnable()
        {
            base.OnEnable();
            FindObjectOfType<CollisionManager>().StopEvent += onStop;

            BlockIntervalEvent += FindObjectOfType<Map>().onBlockInterval; // subscribe dig manager to the BlockSize event
            BlockIntervalEvent += FindObjectOfType<Worm.Turn>().onBlockInterval; // subscribe turn to the BlockSize event

            if (FindObjectOfType<Test.TunnelMaker>())
            {
                BlockIntervalEvent += FindObjectOfType<Test.TunnelMaker>().onBlockInterval;
            }
        }

        new private void Awake()
        {
            base.Awake();

            type = Type.Name.STRAIGHT;
            ingressPosition = transform.position;
            isStopped = false;
            lastBlockLen = -1;
            lastCell = TunnelManager.Instance.initialCell;
        }

        private void Start()
        {
            growthDirection = holeDirectionList[0];

            Quaternion deadEndRotation = Rotation.DeadEndRot.getRotationFromDirection(growthDirection);
            DeadEndInstance = Instantiate(DeadEnd, transform.position, deadEndRotation, Type.instance.TunnelNetwork);
        }

        public override void setHoleDirections(DirectionPair dirPair)
        {
            growthDirection = dirPair.curDir;
            Direction oppositeGrowthDirection = Dir.Base.getOppositeDirection(growthDirection);
            holeDirectionList = new List<Direction>() { growthDirection, oppositeGrowthDirection };
        }

        /**
         * Get exit position of a tunnel to position a dead end cap
         */
        private Vector3 getExitPosition(float length)
        {
            float originAxisPosition = Dir.Vector.getAxisPositionFromDirection(growthDirection, transform.position);
            float exitAxisPosition = Dir.Base.isDirectionNegative(growthDirection) ? originAxisPosition - length : originAxisPosition + length;
            Vector3 deadEndPosition = Dir.Vector.substituteVector3FromDirection(growthDirection, transform.position, exitAxisPosition);
            return deadEndPosition;
        }

        /**
         * Subscribe to grow events from the worm
         * 
         * @position is the point of reference used for scaling the tunnel's head
         */
        public void onGrow(Vector3 position)
        {
            if (!isStopped)
            {
                setTunnelScale(position); // set tunnel scale using worm's position
                
                float length = getLength(); // get length of tunnel
                Vector3 deadEndPosition = getExitPosition(length);
                DeadEndInstance.transform.position = deadEndPosition;

                bool isBlockMultiple = Map.isDistanceMultiple(length);
                Vector3 unitVectorInDir = Dir.Vector.getUnitVectorFromDirection(growthDirection);

                if (isBlockMultiple)
                {
                    int curBlockLen = (int)length;
                    if (curBlockLen > lastBlockLen)
                    {
                        lastBlockLen = curBlockLen;
                        if (!lastCell.Equals(TunnelManager.Instance.initialCell))
                        {
                            addCellToList(lastCell); // add new position to list of cell positions covered by straight tunnel
                            setCenter(length, growthDirection); // adjust the center to the new block
                            BlockIntervalEvent(isBlockMultiple, lastCell, this);
                        }
                        Vector3Int lastCellPosition = getLastCellPosition();
                        lastCell = Dir.Vector.getNextVector3Int(lastCellPosition, growthDirection);
                    }
                }
                else
                {
                    BlockIntervalEvent(isBlockMultiple, Vector3Int.zero, this);
                }
            }
        }

        /**
         * Stop subscribing to events
         */
        private void onStop(string tunnelId)
        {
            if (gameObject.name == tunnelId)
            {
                isStopped = true;
                FindObjectOfType<CollisionManager>().StopEvent -= onStop;
                FindObjectOfType<NewTunnelFactory>().AddTunnelEvent -= onAddTunnel;
                Destroy(DeadEndInstance); // remove the end cap

                BlockIntervalEvent -= FindObjectOfType<Map>().onBlockInterval; // unsubscribe map manager to the BlockSize event
            }
        }

        private float getScale(float length)
        {
            return length / (BLOCK_SIZE * SCALE_TO_LENGTH); // scale of 1 : 2 meters or 0.5 : 1 meter
        }

        private float getLength()
        {
            return (transform.localScale.y * BLOCK_SIZE * SCALE_TO_LENGTH); // scale of 1 : 2 meters or 0.5 : 1 meter
        }

        /**
         * Sets the tunnel's scale so it is offset in front of the worm
         */
        private void setTunnelScale(Vector3 position)
        {
            int roundedLength = (int) getLength();

            float dist = Dir.Vector.getAxisScaleFromDirection(growthDirection, position);
            float totalDist = Mathf.Abs(dist) + TunnelManager.Instance.RING_OFFSET;

            if (roundedLength != -1 && roundedLength > lastBlockLen) // skipped block interval
            {
                print("prev block is " + roundedLength + " length is " + getLength() + " last added block is " + lastBlockLen);
                totalDist = roundedLength;
            }

            float scale = getScale(totalDist);

            print("worm ring position is " + position.y + " rounded scale is " + scale);

            Vector3 curScale = transform.localScale;
            transform.localScale = new Vector3(curScale.x, scale, curScale.z);
        }

        void OnDisable()
        {
            base.OnDisable();
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
                BlockIntervalEvent -= FindObjectOfType<Worm.Turn>().onBlockInterval;
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