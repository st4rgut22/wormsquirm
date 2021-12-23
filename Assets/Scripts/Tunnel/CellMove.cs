using UnityEngine;

namespace Tunnel
{
    public class CellMove
    {
        public Vector3Int cellPosition { get; private set; }
        public Vector3 startPosition { get; private set; }
        public Vector3Int cell { get; private set; }

        public bool isInit;
        public Vector3Int nextCell { get; private set; }

        /**
         * Get next cell positioning information
         *
         * @tunnel the old tunnel being left
         * @directionPair directions to enter/exit the next tunnel segment
         */
        public static CellMove getCellMove(Tunnel tunnel, DirectionPair directionPair)
        {
            if (tunnel != null)
            {
                return new CellMove(tunnel, directionPair.prevDir); // get cell position using ingress direction going into the next cell
            }
            else
            {
                return new CellMove(directionPair.curDir); // on game start, there is no previous direction so use current direction                
            }
        }

        /**
         * Append tunnel to end of previous tunnel
         * 
         * @tunnel the previous tunnel
         * @curDirection ingress direction of current tunnel
         */
        public CellMove(Tunnel tunnel, Direction curDirection)
        {
            isInit = false;
            cellPosition = tunnel.getLastCellPosition(); // start at the end of prev tunnel
            startPosition = Tunnel.getEgressPosition(curDirection, tunnel.center); // get the correct egress position using the curDirection
            cell = Dir.Vector.getNextCellFromDirection(cellPosition, curDirection);
        }

        /**
         * Initialize tunnel at beginning of game
         */
        public CellMove(Direction initialDirection)
        {
            isInit = true;
            Vector3 initialCenter = Tunnel.initializeCenter(initialDirection);
            startPosition = Tunnel.getEgressPosition(initialDirection, initialCenter);
            cell = initialCenter.castToVector3Int(initialDirection);
            nextCell = Dir.Vector.getNextCellFromDirection(cell, initialDirection);
        }
    }
}