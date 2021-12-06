using System;
using UnityEngine;
using System.Collections.Generic;

namespace Tunnel
{
    public abstract class Tunnel : MonoBehaviour
    {
        public const float GROWTH_RATE = .05f; // must be a divisor of 1 so tunnel length will be a multiple of BLOCK_SIZE

        public static int BLOCK_SIZE = 1;    
        public const int SCALE_TO_LENGTH = 2; // scale of 1 : 2 world units
        public const float SCALED_GROWTH_RATE = GROWTH_RATE * SCALE_TO_LENGTH;

        public static float CENTER_OFFSET = BLOCK_SIZE / 2.0f;

        public bool isStopped;

        public Vector3 ingressPosition { get; protected set; }

        public List<Direction> holeDirectionList { get; protected set; }

        public Vector3 center { get; private set; }
        public List<Vector3Int> cellPositionList; // a list of coordinates the tunnel traverses

        public Rotation.IRotation rotation;

        public Type.Name type;

        protected void Awake()
        {
            holeDirectionList = new List<Direction>();
            cellPositionList = new List<Vector3Int>();
        }

        public abstract void setHoleDirections(DirectionPair directionPair);

        public void rotate(DirectionPair dirPair)
        {
            transform.rotation = rotation.rotate(dirPair);
        }

        public void addCellToList(Vector3Int cellPosition)
        {
            cellPositionList.Add(cellPosition);
        }

        public Vector3Int getLastCellPosition()
        {
            return cellPositionList[cellPositionList.Count - 1];
        }

        /**
         * Used by the slicer to chop up a tunnel segment into 2 pieces
         */
        public Tunnel copy(Transform tunnelParent)
        {
            GameObject tunnelCopy = Instantiate(gameObject, tunnelParent);
            Tunnel copiedTunnel = tunnelCopy.GetComponent<Tunnel>();
            copiedTunnel.isStopped = true; // copied tunnel should not grow
            copiedTunnel.ingressPosition = ingressPosition;
            copiedTunnel.holeDirectionList = holeDirectionList;
            return copiedTunnel;
        }

        /**
         * When a tunnel at beginning of game, initialize center
         * 
         * @direction worm direction
         */
        public static Vector3 initializeCenter(Direction direction)
        {
            Vector3 unitVector = Dir.Vector.getUnitVectorFromDirection(direction);
            Vector3 initialCenter = unitVector * CENTER_OFFSET;
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
            print("center of " + gameObject.name + " is " + center + " in direction " + direction + " position is " + transform.position + " distToEnd is " + distToEnd + " unitVector is " + unitVector);
        }
    }
}