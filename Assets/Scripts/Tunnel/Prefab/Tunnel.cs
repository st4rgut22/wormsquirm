using System;
using UnityEngine;
using System.Collections.Generic;

namespace Tunnel
{
    public abstract class Tunnel : MonoBehaviour
    {
        [SerializeField]
        protected GameObject DeadEnd;

        protected GameObject DeadEndInstance;

        public const float GROWTH_RATE = .01f; // .02f; // must be a divisor of 1 so tunnel length will be a multiple of BLOCK_SIZE

        protected const float MARGIN_OF_ERROR = .07f;
        protected const float WALL_THICKNESS = .25f;
        public static float TUNNEL_MIDPOINT = .5f; // closest distance from tunnel center to side

        public static float BLOCK_SIZE = 1;
        public const float SCALE_TO_LENGTH = 2; // scale of 1 : 2 world units
        public const float SCALED_GROWTH_RATE = GROWTH_RATE * SCALE_TO_LENGTH;

        public static float CENTER_OFFSET = BLOCK_SIZE / 2.0f;
        public static float INNER_WALL_OFFSET = .25f;

        public bool isStopped;
        protected bool isTurning;

        public Vector3 ingressPosition { get; set; }
        public List<Direction> holeDirectionList;
        public List<Direction> wallDirectionList;

        public Vector3 center { get; private set; }
        public List<Vector3Int> cellPositionList; // a list of coordinates the tunnel traverses

        public Type.Name type;

        public string wormCreatorId { get; private set; }
        protected Vector3 wormPosition;

        protected bool isCollision;

        public int holeCount;

        public abstract void setHoleDirections(DirectionPair dirPair);

        protected void Awake()
        {
            holeDirectionList = new List<Direction>();
            cellPositionList = new List<Vector3Int>();
            wormPosition = Vector3.zero;
            isTurning = false;
            isCollision = false;
        }

        /**
         * Set the sides of the tunnel with walls (eg non-hole sides)
         */
        protected void setWallDirections()
        {
            wallDirectionList = new List<Direction>();
            Dir.Base.directionList.ForEach((Direction direction) =>
            {
                if (!holeDirectionList.Contains(direction))
                {
                    wallDirectionList.Add(direction);
                }
            });
        }

        public void setIngressPosition(Vector3 ingressPosition)
        {
            this.ingressPosition = ingressPosition;
        }

        /**
         * Get the dead end tunnel object associated with this tunnel segment
         */
        public GameObject getDeadEnd()
        {
            return DeadEndInstance;
        }

        public void replaceHoleDirections(List<Direction> holeDirections)
        {
            holeDirectionList = holeDirections;
            setWallDirections();
        }

        /**
         * Get the exit position
         */
        protected virtual Vector3 getExit(Direction exitDirection)
        {
            Vector3 exitPos = getOffsetPosition(exitDirection, center);
            return exitPos;
        }

        /**
         * If a player is making a turn or going straight into this tunnel ,check whether the tunnel needs to be modified by seeing if the holes line up with player
         * 
         * @directionPair the ingress and egress direction of the worm
         */
        public bool isDirectionPairInHoleList(DirectionPair directionPair)
        {
            Direction tunnelIngressDir = Dir.Base.getOppositeDirection(directionPair.prevDir); // ingress hole direction from tunnel perspective is opposite that of worm
            Direction tunnelEgressDir = directionPair.curDir;
            return holeDirectionList.Contains(tunnelIngressDir) && holeDirectionList.Contains(tunnelEgressDir);
        }

        public void setWormCreatorId(string wormId)
        {
            print("set worm creator id for tunnel " + gameObject.name + " as id " + wormId);
            wormCreatorId = wormId;
        }

        /**
         * Get the egress position defined as the side of the last cell facing the player
         * 
         * @faceDirection       the side of the cell facing the player
         */
        public Vector3 getEgressPosition(Direction faceDirection)
        {
            if (isStopped)
            {
                Vector3Int lastCellPos = getLastCellPosition();
                Vector3 centerCellPos = getOffsetPosition(Direction.Up, lastCellPos);
                return getOffsetPosition(faceDirection, centerCellPos);
            }
            else // straight tunnel
            {
                if (type != Type.Name.STRAIGHT)
                {
                    throw new Exception("Tunnel is not stopped, should be of type STRAIGHT but is type " + gameObject.name);
                }
                print("getting egress position of a growing tunnel " + gameObject.name + " in direction " + faceDirection);
                return getExit(faceDirection);
            }
        }

        /**
         * When a tunnel is created, add its original position to its list of cells
         * 
         * @cellPosition starting cell position of tunnel segment
         */
        public void addCellToList(Vector3Int cellPosition)
        {
            cellPositionList.Add(cellPosition);
        }

        public bool containsCell(Vector3Int cell)
        {
            return cellPositionList.Contains(cell);
        }

        public Vector3Int getLastCellPosition()
        {
            return cellPositionList[cellPositionList.Count - 1];
        }

        public void onDecision(bool isStraightTunnel, Direction direction, Tunnel tunnel)
        {
            isTurning = true;
        }

        /**
         * When the tunnel is initialized, add it to the cellPosList because the first couple blocks won't trigger blockIntervalEvent 
         */
        public virtual void onAddTunnel(Tunnel tunnel, CellMove cellMove, DirectionPair directionPair, string wormId)
        {
            addCellToList(cellMove.cell);
        }

        /** 
         * When a tunnel is created at beginning of game, initialize center
         * 
         * @direction worm direction
         */
        public static Vector3 initializeCenter(Vector3 cellPos)
        {
            Vector3 unitVector = Dir.CellDirection.getUnitVectorFromDirection(Direction.Up); // center is offset from cell location in up direction

            Vector3 initialCenter = cellPos + unitVector * CENTER_OFFSET;
            return initialCenter;
        }

        /**
         * Get offset position on tunnel using worm's direction
         * 
         * @direction   worm direction
         * @position    position in tunnel to get offset from
         */
        public static Vector3 getOffsetPosition(Direction direction, Vector3 position)
        {
            Vector3 unitVector = Dir.CellDirection.getUnitVectorFromDirection(direction);
            Vector3 offsetPosition = position + unitVector * CENTER_OFFSET;
            return offsetPosition;
        }

        /**
         * Get position of the starting tunnel. It must be offset such that others tunnels can reach it
         * 
         * @direction       the direction the tunnel grows in
         * @position        tunnel cell coordinate integer
         */
        public static Vector3 getInitialOffsetPosition(Direction direction, Vector3 originalCell)
        {
            Vector3 offsetPosition = originalCell;
            Vector3 unitVector = Vector3.zero;
            Vector3 yOffset = Vector3.zero;

            if (direction == Direction.Left || direction == Direction.Right)
            {
                unitVector = Dir.CellDirection.getUnitVectorFromDirection(Direction.Left); // offset in the negative direction
                yOffset = Vector3.up * CENTER_OFFSET;
            }
            if (direction == Direction.Forward || direction == Direction.Back)
            {
                unitVector = Dir.CellDirection.getUnitVectorFromDirection(Direction.Back); // offset in the negative direction
                yOffset = Vector3.up * CENTER_OFFSET;
            }

            offsetPosition = offsetPosition + unitVector * CENTER_OFFSET + yOffset;
            return offsetPosition;
        }

        /**
         * Get the center of the last block in this tunnel
         * 
         * @direction is the direction the tunnel is entered
         * @distToEnd is distance to the opposite end of the tunnel
         */
        public void setCenter(float distToEnd, Direction direction)
        {
            Vector3 unitVector = Dir.CellDirection.getUnitVectorFromDirection(direction);
            Vector3 blockEndPosition = transform.position + distToEnd * unitVector;
            Vector3 centerOffsetVector = unitVector * CENTER_OFFSET;
            center = blockEndPosition - centerOffsetVector;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(center, new Vector3(.1f, .1f, .1f));
        }
    }
}