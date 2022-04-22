using UnityEngine;
using System.Collections.Generic;

namespace Tunnel
{    
    public class Straight : Tunnel
    {
        public Direction growthDirection;

        private bool isSliced;
        private bool isTunnelCreatedByPlayer;

        int lastBlockLen; // last block added used to retroactively add blocks that did not fall on an interval

        public delegate void BlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Straight tunnel, bool isTunnelSame, bool isTurning, bool isCollide);
        public event BlockInterval BlockIntervalEvent;

        public delegate void AIBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Straight tunnel);
        public event AIBlockInterval AIBlockIntervalEvent;

        public delegate void Dig(Vector3 digLocation, Direction digDirection);
        public event Dig DigEvent;

        // Start is called before the first frame update
        void OnEnable()
        {
            AIBlockIntervalEvent += FindObjectOfType<Map.AiSpawnGenerator>().onAiBlockInterval;

            BlockIntervalEvent += Map.SpawnGenerator.onBlockInterval;
            BlockIntervalEvent += FindObjectOfType<TunnelMap>().onBlockInterval; // subscribe dig manager to the BlockSize event
            BlockIntervalEvent += FindObjectOfType<Map.RewardGenerator>().onBlockInterval;
            DigEvent += DirtManager.Instance.onDig;
        }

        new private void Awake()
        {
            base.Awake();
            type = Type.Name.STRAIGHT;
            ingressPosition = transform.position;
            isStopped = false;
            isSliced = false;
            isTunnelCreatedByPlayer = false;
            lastBlockLen = -1;
        }

        private void Start()
        {
            if (!isSliced) // if the straight tunnel is not the result of slicing a longer straight tunnel
            {
                growthDirection = holeDirectionList[0];

                Quaternion deadEndRotation = Rotation.DeadEndRot.getRotationFromDirection(growthDirection);
                DeadEndInstance = Instantiate(DeadEnd, transform.position, deadEndRotation, Type.instance.TunnelNetwork);
                DeadEndInstance.name = gameObject.name + "DEADEND";
            }
        }

        /**
         * Update the cells that are part of the tunnel
         * 
         * @newStartCell   the cell that the tunnel starts at
         * @newEndCell     the cell the tunnel ends at
         */
        public void updateCellPositionList(Vector3Int newStartCell, Vector3Int newEndCell)
        {
            cellPositionList = new List<Vector3Int>();
            Vector3Int newCell = newStartCell;

            while (!newCell.Equals(newEndCell))
            {
                addCellToList(newCell); // TODO: PROBLEM INVESTIAGATE, STRAIGHT 13 HAS AN EXTRA CELL APPENDED AT (-5, 5, ...) WHY???
                TunnelMap.addCell(newCell, this); // update the cells in the map
                newCell = Dir.Vector.getNextCellFromDirection(newCell, growthDirection);
            }
            addCellToList(newEndCell);
            TunnelMap.addCell(newEndCell, this);
        }

        /**
         * Used by the slicer to chop up a tunnel segment into 2 pieces
         */
        public Straight copy(Transform tunnelParent)
        {
            Straight copiedTunnel = gameObject.instantiateSliced(tunnelParent, this);
            return copiedTunnel;
        }

        public override void setHoleDirections(DirectionPair dirPair)
        {
            if (gameObject.name == "Straight 1")
            {
                print(" KDESWJFl;iksdejl;");
            }
            growthDirection = dirPair.curDir;
            Direction oppositeGrowthDirection = Dir.Base.getOppositeDirection(growthDirection);
            holeDirectionList = new List<Direction>() { growthDirection, oppositeGrowthDirection };
            setWallDirections();
        }

        /** 
         * Unsubscribe tunnel from addTunnelEvent after being added
         */
        public override void onAddTunnel(Tunnel tunnel, CellMove cellMove, DirectionPair directionPair, string wormId)
        {
            isTunnelCreatedByPlayer = Worm.WormManager.Instance.isWormIdPlayer(wormId);
            base.onAddTunnel(tunnel, cellMove, directionPair, wormId);
            FindObjectOfType<NewTunnelFactory>().AddTunnelEvent -= onAddTunnel;
        }

        /**
         * When a straight tunnel is slice the last recorded length should be adjusted
         * 
         * @sliceLen    new length of tunnel
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


                Vector3 deadEndPosition = getExit(growthDirection);
                DeadEndInstance.transform.position = deadEndPosition;

                Vector3 unitVectorInDir = Dir.CellDirection.getUnitVectorFromDirection(growthDirection);

                //GridLayout gridLayout = transform.parent.GetComponentInParent<GridLayout>();
                //Vector3Int cellPosition = gridLayout.WorldToCell(position);
                float length = getLength();
                int curBlockLen = (int)length;
                //bool isBlockMultiple = TunnelMap.isDistanceMultiple(length);
                if (isSliced)
                {
                    lastBlockLen = curBlockLen;
                    isSliced = false;
                }
                bool isNewBlock = curBlockLen >= 1 && curBlockLen > lastBlockLen;


                if (isNewBlock) //if (isBlockMultiple && (isNewBlock || isSliced))
                {
                    isSliced = false;
                    lastBlockLen = curBlockLen;
                    setCenter(length, growthDirection); // adjust the center to the new block

                    Vector3Int lastCellPosition = getLastCellPosition();
                    Vector3Int curCell = Dir.Vector.getNextVector3Int(lastCellPosition, growthDirection);

                    bool isCollision = TunnelMap.getTunnelFromDict(curCell) != null;

                    if (!isTunnelCreatedByPlayer) // only emit ai block interval event if tunnel is created by an AI worm
                    {
                        AIBlockIntervalEvent(true, curCell, lastCellPosition, this);
                    }
                    if (!isTurning && !isCollision) // if turn will be made or there is a collision, the cell should not be added to the tunnell's list of cells
                    {
                        addCellToList(curCell);
                    }
                    bool isTunnelSame = !isTurning && !isCollision;
                    BlockIntervalEvent(true, curCell, lastCellPosition, this, isTunnelSame, isTurning, isCollision);
                }
                else // notify listeners that there is not a block multple
                {
                    Vector3Int lastCellPosition = getLastCellPosition();
                    BlockIntervalEvent(false, lastCellPosition, lastCellPosition, this, !isCollision, isTurning, isCollision);
                }
            }
        }

        /**
         * Stop subscribing to events
         */
        public void onStop()
        {
            isStopped = true;
            if (DeadEndInstance != null)
            {
                destroyDeadEnd();
            }
            BlockIntervalEvent -= FindObjectOfType<TunnelMap>().onBlockInterval; // unsubscribe map manager to the BlockSize event
        }

        /**
         * Get exit position of a tunnel to position a dead end cap
         */
        protected override Vector3 getExit(Direction growthDirection)
        {
            float length = getLength();
            float originAxisPosition = Dir.Vector.getAxisPositionFromDirection(growthDirection, transform.position);
            float exitAxisPosition = Dir.Base.isDirectionNegative(growthDirection) ? originAxisPosition - length : originAxisPosition + length;
            Vector3 deadEndPosition = Dir.Vector.substituteVector3FromDirection(growthDirection, transform.position, exitAxisPosition);
            return deadEndPosition;

        } 

        private void destroyDeadEnd()
        {
            Destroy(DeadEndInstance); // remove the end cap

        }

        private float getScale(float length)
        {
            return length / (BLOCK_SIZE * SCALE_TO_LENGTH); // scale of 1 : 2 meters or 0.5 : 1 meter
        }

        /**
         * Get the number of cells in the straight tunnel
         */
        public int getCellCount()
        {
            return cellPositionList.Count;
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

            //print("worm ring position is " + position.y + " rounded scale is " + scale.ToString("F4"));

            Vector3 curScale = transform.localScale;
            transform.localScale = new Vector3(curScale.x, scale, curScale.z);
        }

        void OnDisable()
        {
            if (!isStopped)
            {
                BlockIntervalEvent -= FindObjectOfType<TunnelMap>().onBlockInterval;
            }
            if (FindObjectOfType<Map.RewardGenerator>())
            {
                BlockIntervalEvent -= FindObjectOfType<Map.RewardGenerator>().onBlockInterval;
            }
            if (FindObjectOfType<Map.SpawnGenerator>())
            {
                AIBlockIntervalEvent -= FindObjectOfType<Map.AiSpawnGenerator>().onAiBlockInterval;
                BlockIntervalEvent -= Map.SpawnGenerator.onBlockInterval;
            }
            if (DirtManager.Instance)
            {
                DigEvent -= DirtManager.Instance.onDig;
            }
        }
    }
}