using System;
using UnityEngine;
using System.Collections.Generic;

namespace Tunnel
{
    public abstract class Tunnel : MonoBehaviour
    {
        public const float GROWTH_RATE = .05f; // must be a divisor of 1 so tunnel length will be a multiple of BLOCK_SIZE

        public const int BLOCK_SIZE = 1;    
        public const int SCALE_TO_LENGTH = 2; // scale of 1 : 2 world units
        public const float SCALED_GROWTH_RATE = GROWTH_RATE * SCALE_TO_LENGTH;

        public const float CENTER_OFFSET = BLOCK_SIZE / 2.0f;

        public bool isStopped;

        public DirectionPair directionPair { get; private set; } // passed in on initialization to set the tunnel rotation

        public Vector3 ingressPosition { get; protected set; }
        public Vector3[] egressPosition { get; protected set; }

        public Direction ingressDirection { get; protected set; }
        public Direction[] egressDirection { get; protected set; }

        public Vector3 center { get; private set; }
        public List<Vector3Int> cellPositionList; // a list of coordinates the tunnel traverses

        public Rotation.IRotation rotation;

        protected void Awake()
        {
            cellPositionList = new List<Vector3Int>();
        }

        protected void Start()
        {
            if (directionPair == null)
            {
                throw new Exception("direction pair should have been initialized on Instantiation");
            }
            if (directionPair.prevDir == Direction.None)
            {
                ingressDirection = directionPair.curDir;
            }
            else
            {
                ingressDirection = directionPair.prevDir;
            }
            print("ingress direction of " + gameObject.name + " is " + ingressDirection);

            print("direction cur of " + gameObject.name + " is " + directionPair.curDir + " prev is " + directionPair.prevDir);

            setCenter(BLOCK_SIZE); // tunnels are initially of length BLOCK_SIZE, so distance to opposite end of tunnel is equal to BLOCK_SIZE
        }

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
            copiedTunnel.egressPosition = egressPosition;
            return copiedTunnel;
        }

        public void setDirectionPair(DirectionPair directionPair)
        {
            this.directionPair = directionPair;
        }

        public Vector3 getEgressPosition(Direction direction)
        {
            Vector3 unitVector = Dir.Vector.getUnitVectorFromDirection(direction);
            Vector3 egressPosition =  center + unitVector * CENTER_OFFSET;
            return egressPosition;
        }

        /**
         * Get the center of the last block in this tunnel
         * 
         * @ingressDirection is the direction the tunnel is entered
         * @distToEnd is distance to the opposite end of the tunnel
         */
        protected void setCenter(float distToEnd)
        {
            Vector3 unitVector = Dir.Vector.getUnitVectorFromDirection(ingressDirection);
            Vector3 blockEndPosition = transform.position + distToEnd * unitVector;
            Vector3 centerOffsetVector = unitVector * CENTER_OFFSET;
            center = blockEndPosition - centerOffsetVector;
            print("center of " + gameObject.name + " is " + center + " in direction " + ingressDirection + " position is " + transform.position + " distToEnd is " + distToEnd + " unitVector is " + unitVector);
        }
    }
}