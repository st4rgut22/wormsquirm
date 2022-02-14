using UnityEngine;

namespace Tunnel
{
    public class CellMove
    {
        public Vector3Int lastCellPosition { get; private set; }
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
            lastCellPosition = tunnel.getLastCellPosition(); // start at the end of prev tunnel

            startPosition = Tunnel.getEgressPosition(curDirection, tunnel.center); // get the correct egress position using the curDirection
            cell = Dir.Vector.getNextVector3Int(lastCellPosition, curDirection);

            //Debug.Log("last cell position " + lastCellPosition + " start position " + startPosition + " cell " + cell);
            nextCell = Dir.Vector.getNextCellFromDirection(cell, curDirection);
            isInit = false;
        }

        /**
         * Initialize tunnel at beginning of game
         * 
         * @initialDirection the direction the worm first decides to go in
         * @cell the cell coordinates of the first tunnel
         */
        public CellMove(Direction initialDirection, Vector3Int cell)
        {
            this.cell = cell;

            Vector3 initialCenter = Tunnel.initializeCenter(cell);

            Direction oppDir = Dir.Base.getOppositeDirection(initialDirection);

            startPosition = Tunnel.getEgressPosition(oppDir, initialCenter); // offset from center in oppDir

            nextCell = Dir.Vector.getNextCellFromDirection(cell, initialDirection);
            isInit = true;
        }
    }
}