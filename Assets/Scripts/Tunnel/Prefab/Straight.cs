using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tunnel
{    
    public class Straight : Tunnel
    {
        private GameObject DeadEndInstance;
        public Direction growthDirection;

        private bool isSliced;

        int lastBlockLen; // last block added used to retroactively add blocks that did not fall on an interval

        public delegate void BlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Straight tunnel);
        public event BlockInterval BlockIntervalEvent;

        public delegate void Dig(Vector3 digLocation, Direction digDirection);
        public event Dig DigEvent;

        // Start is called before the first frame update
        void OnEnable()
        {
            BlockIntervalEvent += FindObjectOfType<Map>().onBlockInterval; // subscribe dig manager to the BlockSize event
            BlockIntervalEvent += FindObjectOfType<Worm.Turn>().onBlockInterval; // subscribe turn to the BlockSize event

            DigEvent += DirtManager.Instance.onDig;
            if (FindObjectOfType<Worm.TunnelMaker>())
            {
                BlockIntervalEvent += FindObjectOfType<Worm.TunnelMaker>().onBlockInterval;
            }
        }

        new private void Awake()
        {
            base.Awake();
            type = Type.Name.STRAIGHT;
            ingressPosition = transform.position;
            isStopped = false;
            isSliced = false;
            lastBlockLen = -1;
        }

        private void Start()
        {
            if (!isSliced)
            {
                growthDirection = holeDirectionList[0];

                Quaternion deadEndRotation = Rotation.DeadEndRot.getRotationFromDirection(growthDirection);
                DeadEndInstance = Instantiate(DeadEnd, transform.position, deadEndRotation, Type.instance.TunnelNetwork);
                DeadEndInstance.name = gameObject.name + "DEADEND";
            }
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
         * Unsubscribe tunnel from addTunnelEvent after being added
         */
        public override void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            base.onAddTunnel(tunnel, cell, directionPair, wormId);
            FindObjectOfType<NewTunnelFactory>().AddTunnelEvent -= onAddTunnel;
        }

        /**
         * Whether the straight tunnel is a prexisting tunnel that is sliced
         */
        public void setIsSliced()
        {
            isSliced = true;
        }

        /**
         * Subscribe to grow events from the worm
         * 
         * @position is the point of reference used for scaling the tunnel's head
         */
        public void onGrow(Vector3 position)
        {
            if (!isStopped && DeadEndInstance != null)
            {
                DigEvent(DeadEndInstance.transform.position, growthDirection);
                setTunnelScale(position); // set tunnel scale using worm's position
                
                float length = getLength(); // get length of tunnel
                Vector3 deadEndPosition = getExitPosition(length);
                DeadEndInstance.transform.position = deadEndPosition;

                bool isBlockMultiple = Map.isDistanceMultiple(length);
                Vector3 unitVectorInDir = Dir.CellDirection.getUnitVectorFromDirection(growthDirection);
                int curBlockLen = (int)length;

                if (isBlockMultiple)
                {
                    print("cur block " + curBlockLen + " last block " + lastBlockLen + " position is " + position);
                    if (curBlockLen >= 1 && curBlockLen > lastBlockLen) // the first block is already added on tunnel creation 
                    {
                        lastBlockLen = curBlockLen;
                        setCenter(length, growthDirection); // adjust the center to the new block

                        Vector3Int lastCellPosition = getLastCellPosition();
                        Vector3Int curCell = Dir.Vector.getNextVector3Int(lastCellPosition, growthDirection);

                        bool isCollision = Map.getTunnelFromDict(curCell) != null;

                        if (!isTurning && !isCollision)
                        {
                            addCellToList(curCell);
                        }

                        BlockIntervalEvent(isBlockMultiple, curCell, this);
                    }
                }
                else // notify listeners that there is not a block multple
                {
                    BlockIntervalEvent(isBlockMultiple, Vector3Int.zero, this);
                }
            }
        }

        /**
         * Stop subscribing to events
         */
        public void onStop()
        {
            isStopped = true;
            print("destroy deadEnd " + DeadEndInstance.name);
            destroyDeadEnd();
            BlockIntervalEvent -= FindObjectOfType<Map>().onBlockInterval; // unsubscribe map manager to the BlockSize event
        }

        private void destroyDeadEnd()
        {
            Destroy(DeadEndInstance); // remove the end cap

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

            float offsetOriginDist = Dir.Vector.getAxisPositionFromDirection(growthDirection, ingressPosition);
            float dist = Dir.Vector.getAxisScaleFromDirection(growthDirection, position);
            float distToRing = Mathf.Abs(dist - offsetOriginDist); // distance between position of worm (ahead of tunnel) and tunnel
            float totalDist = distToRing + TunnelManager.Instance.TUNNEL_OFFSET;

            if (roundedLength != 0 && roundedLength > lastBlockLen) // skipped block interval
            {
                print("prev block is " + roundedLength + " length is " + getLength() + " last added block is " + lastBlockLen);
                totalDist = roundedLength;
            }

            float scale = getScale(totalDist);

            print("worm ring position is " + position.y + " rounded scale is " + scale.ToString("F4"));

            Vector3 curScale = transform.localScale;
            transform.localScale = new Vector3(curScale.x, scale, curScale.z);
        }

        void OnDisable()
        {
            if (!isStopped)
            {
                BlockIntervalEvent -= FindObjectOfType<Map>().onBlockInterval;
            }

            if (FindObjectOfType<Worm.Movement>())
            {
                BlockIntervalEvent -= FindObjectOfType<Worm.Turn>().onBlockInterval;
            }
            if (FindObjectOfType<Worm.TunnelMaker>())
            {
                BlockIntervalEvent -= FindObjectOfType<Worm.TunnelMaker>().onBlockInterval;
            }
            if (DirtManager.Instance)
            {
                DigEvent -= DirtManager.Instance.onDig;
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