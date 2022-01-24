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

        public static int BLOCK_SIZE = 1;    
        public const int SCALE_TO_LENGTH = 2; // scale of 1 : 2 world units
        public const float SCALED_GROWTH_RATE = GROWTH_RATE * SCALE_TO_LENGTH;

        public static float CENTER_OFFSET = BLOCK_SIZE / 2.0f;

        public bool isStopped;

        public Vector3 ingressPosition { get; protected set; }

        public List<Direction> holeDirectionList;

        public Vector3 center { get; private set; }
        public List<Vector3Int> cellPositionList; // a list of coordinates the tunnel traverses

        public Type.Name type;

        public string wormCreatorId { get; private set; }

        public abstract void setHoleDirections(DirectionPair dirPair);
        public abstract Vector3 getContactPosition(DirectionPair dirPair); // get point of contact with the NEXT tunnel

        public int holeCount;

        [SerializeField]
        private Vector3Int lastCellPos;

        protected virtual void OnEnable()
        {
            FindObjectOfType<NewTunnelFactory>().AddTunnelEvent += onAddTunnel;
        }

        protected void Awake()
        {
            holeDirectionList = new List<Direction>();
            cellPositionList = new List<Vector3Int>();
        }

        public void setWormCreatorId(string wormId)
        {
            wormCreatorId = wormId;
        }

        public bool isTunnelType(Type.Name tunnelType)
        {
            return type == tunnelType;
        }

        public void addCellToList(Vector3Int cellPosition)
        {
            lastCellPos = cellPosition;
            cellPositionList.Add(cellPosition);
        }

        public Vector3Int getLastCellPosition()
        {
            return cellPositionList[cellPositionList.Count - 1];
        }

        /**
         * When the tunnel is initialized, add it to the cellPosList because the first couple blocks won't trigger blockIntervalEvent 
         */
        public virtual void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            addCellToList(cell);
        }

        /**
         * Used by the slicer to chop up a tunnel segment into 2 pieces
         */
        public Tunnel copy(Transform tunnelParent)
        {
            GameObject tunnelCopy = Instantiate(gameObject, tunnelParent);
            Straight copiedTunnel = tunnelCopy.GetComponent<Straight>();
            copiedTunnel.ingressPosition = ingressPosition;
            copiedTunnel.holeDirectionList = holeDirectionList;
            return copiedTunnel;
        }

        /** 
         * When a tunnel at beginning of game, initialize center
         * 
         * @direction worm direction
         */
        public static Vector3 initializeCenter(Direction direction, Vector3 center)
        {
            Vector3 unitVector = Dir.Vector.getUnitVectorFromDirection(direction);
            Vector3 initialCenter = center + unitVector * CENTER_OFFSET;
            return initialCenter;
        }

        /**
         * Get exit position on tunnel using worm's direction
         * 
         * @direction worm direction
         * @center center of tunnel
         */
        public static Vector3 getEgressPosition(Direction direction, Vector3 center)
        {
            Vector3 unitVector = Dir.Vector.getUnitVectorFromDirection(direction);
            Vector3 egressPosition =  center + unitVector * CENTER_OFFSET;
            return egressPosition;
        }

        /**
         * Get the center of the last block in this tunnel
         * 
         * @direction is the direction the tunnel is entered
         * @distToEnd is distance to the opposite end of the tunnel
         */
        public void setCenter(float distToEnd, Direction direction)
        {
            Vector3 unitVector = Dir.Vector.getUnitVectorFromDirection(direction);
            Vector3 blockEndPosition = transform.position + distToEnd * unitVector;
            Vector3 centerOffsetVector = unitVector * CENTER_OFFSET;
            center = blockEndPosition - centerOffsetVector;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawCube(center, new Vector3(.1f, .1f, .1f));
        }

        protected virtual void OnDisable()
        {
            if (FindObjectOfType<NewTunnelFactory>())
            {
                FindObjectOfType<NewTunnelFactory>().AddTunnelEvent -= onAddTunnel;
            }
        }
    }
}