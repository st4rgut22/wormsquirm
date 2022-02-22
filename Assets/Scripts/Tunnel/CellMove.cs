using UnityEngine;

namespace Tunnel
{
    public class CellMove
    {
        public Vector3Int lastCellPosition { get; private set; }
        public Vector3 startPosition { get; private set; }
        public Vector3 center { get; private set; }
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
            return new CellMove(tunnel, directionPair.prevDir); // get cell position using ingress direction going into the next cell
        }

        public static CellMove getInitialCellMove(Direction direction, Vector3Int initialCell)
        {
            return new CellMove(direction, initialCell); // on game start, there is no previous direction so use current direction                
        }

        /**
         * Append tunnel to end of previous tunnel
         * 
         * @tunnel the previous tunnel
         * @curDirection ingress direction of current tunnel
         */
        public CellMove(Tunnel tunnel, Direction curDirection)
        {
            center = tunnel.center;
            lastCellPosition = tunnel.getLastCellPosition(); // start at the end of prev tunnel
            
            startPosition = Tunnel.getOffsetPosition(curDirection, tunnel.center); // get the correct egress position using the curDirection
            cell = Dir.Vector.getNextVector3Int(lastCellPosition, curDirection);

            nextCell = Dir.Vector.getNextCellFromDirection(cell, curDirection);
            isInit = false;
        }

        public CellMove(Direction initialDirection, Vector3Int cell)
        {
            this.cell = cell;

            center = Tunnel.initializeCenter(cell);

            Direction oppDir = Dir.Base.getOppositeDirection(initialDirection);

            startPosition = Tunnel.getOffsetPosition(oppDir, center); // offset from center in oppDir

            nextCell = Dir.Vector.getNextCellFromDirection(cell, initialDirection);
            isInit = true;
        }
    }
}