using UnityEngine;

namespace Tunnel
{
    public class CellMove
    {
        public Vector3 startPosition { get; private set; }
        public Vector3 center { get; private set; }

        public Vector3Int lastCellPosition { get; private set; }    // THE CELL WE ARE LEAVING
        public Vector3Int cell { get; private set; }                // THE CELL WE ARE ABOUT TO ENTER
        public Vector3Int nextCell { get; private set; }            // THE CELL FOLLOWING THAT WHICH WE ARE ABOUT TO ENTER

        public bool isCellUpdated { get; private set; } // whether the cell's position has been updated, if yes update the cell maps
        public bool isInit;

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

        /**
         * Initialize the first cell
         * 
         * @mappedInitialCell           The cell's location in the map, which may be translated from the original cell location
         * @initialCell                 The cell location used to initialize the cell's position
         */
        public static CellMove getInitialCellMove(Direction direction, Vector3Int mappedInitialCell, Vector3Int initialCell)
        {
            return new CellMove(direction, mappedInitialCell, initialCell); // on game start, there is no previous direction so use current direction                
        }

        /**
         * @initialCell     the cell prior to the one we will turn in (calculated using clit position)
         */
        public static CellMove getExistingCellMove(DirectionPair directionPair, Vector3Int initialCell)
        {
            return new CellMove(directionPair, initialCell); // on game start, there is no previous direction so use current direction                
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
            Debug.Log("add cella " + cell + " last cell " + lastCellPosition + " for tunnel " + tunnel.gameObject.name);
        }

        /**
         * Constructor for getting cell position information for existing tunnels
         * 
         * @curCell         the current cell worm is in, the turning cell (this.cell) is the next cell. The same as lastCellPosition
         * @directionPair   the current and next direction of the worm
         */
        public CellMove(DirectionPair directionPair, Vector3Int curCell)
        {
            lastCellPosition = curCell;
            cell = Dir.Vector.getNextCellFromDirection(curCell, directionPair.prevDir);
            nextCell = Dir.Vector.getNextCellFromDirection(cell, directionPair.curDir);
            center = Tunnel.initializeCenter(lastCellPosition);
            startPosition = Tunnel.getOffsetPosition(directionPair.prevDir, center); // offset from center in oppDir
            isInit = false;
        }

        /**
         * Get the cell coordinates of the tunnel generated when worm is first created
         */
        public CellMove(Direction initialDirection, Vector3Int mappedInitialCell, Vector3Int originalCell)
        {
            cell = mappedInitialCell;
            lastCellPosition = mappedInitialCell; // there is no last cell so initialize it with cell about to enter
            Debug.Log("starting cell is " + cell + " last cell is " + mappedInitialCell + " initial direction is " + initialDirection);

            center = Tunnel.initializeCenter(mappedInitialCell); // center of the cell
             
            Direction oppDir = Dir.Base.getOppositeDirection(initialDirection);            
            startPosition = Tunnel.getInitialOffsetPosition(oppDir, originalCell); // offset from center in oppDir

            Debug.Log("initial start position is " + startPosition);
            nextCell = Dir.Vector.getNextCellFromDirection(cell, initialDirection);
            isInit = true;
        }
    }
}