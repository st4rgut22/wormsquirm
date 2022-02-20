using System;
using UnityEngine;
using System.Collections.Generic;

namespace Tunnel
{
    public abstract class Tunnel : MonoBehaviour
    {
        [SerializeField]
        protected GameObject DeadEnd;

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

        public Vector3 center { get; private set; }
        public List<Vector3Int> cellPositionList; // a list of coordinates the tunnel traverses

        public Type.Name type;

        public string wormCreatorId { get; private set; }
        protected Vector3 wormPosition;
        protected Direction wormDirection;

        protected bool isCollision;

        public abstract void setHoleDirections(DirectionPair dirPair);

        public int holeCount;

        protected void Awake()
        {
            holeDirectionList = new List<Direction>();
            cellPositionList = new List<Vector3Int>();
            wormPosition = Vector3.zero;
            isTurning = false;
            isCollision = false;
        }

        public Vector3 getContactPosition(DirectionPair dirPair) // get point of contact with the NEXT tunnel
        {
            return getOffsetPosition(dirPair.prevDir, center);
        }

        public void setIngressPosition(Vector3 ingressPosition)
        {
            this.ingressPosition = ingressPosition;
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
            Vector3Int lastCellPos = getLastCellPosition();
            Vector3 centerCellPos = getOffsetPosition(Direction.Up, lastCellPos);
            return getOffsetPosition(faceDirection, centerCellPos);
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
        public virtual void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            addCellToList(cell);
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