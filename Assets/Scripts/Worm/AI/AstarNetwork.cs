using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class AstarNetwork : MonoBehaviour
    {
        public delegate void initCheckpoint(List<Checkpoint> checkpointList, bool isInitPath, Worm.WormTunnelBroker WormTunnelBroker);
        public event initCheckpoint initCheckpointEvent;

        const int TURN_STEP = 1;

        /**
         * Convert the list of cell positions to a list of directions for TunnelMaker to process
         * 
         * @isModifyPath        is the worm modifying a path
         * @gridCellPathList    is the list of cell posiitons along the path
         * @initialDirection    the current direction of the worm
         * @curCell             the previous cell of the worm
         */
        private List<Checkpoint> getCheckpointListFromPath(List<Vector3Int> gridCellPathList, bool isModifyPath, Direction initialDirection, Vector3Int previousCell)
        {
            List<Checkpoint> astarCheckpointList = new List<Checkpoint>();
            Direction prevDirection = Direction.None;
            int lastCellIdx = gridCellPathList.Count - 1;

            int stepCounter = 1; // if initial cell is turn, set step counter = 0, in the loop below, the initial cell is assumed to be a straight segment so no need to reset step counter to 0

            for (int i = 1; i < gridCellPathList.Count; i++)
            {
                Vector3Int gridCell = gridCellPathList[i];
                Vector3Int prevCell = gridCellPathList[i - 1];

                Direction curDirection = Dir.CellDirection.getDirectionFromCells(prevCell, gridCell);

                bool isContinueStraight = curDirection == prevDirection || prevDirection == Direction.None;

                if (isContinueStraight)
                {
                    stepCounter += 1;
                }
                if (!isContinueStraight || i == lastCellIdx) // if a turn is made or it is the final index, add the previous straight segment
                {
                    Direction direction = prevDirection == Direction.None ? curDirection : prevDirection; // if 2 cells in path prevDir = None will be added
                    Checkpoint straightTunnelCheckpoint = new Checkpoint(direction, stepCounter - TURN_STEP);
                    astarCheckpointList.Add(straightTunnelCheckpoint);
                    stepCounter = 1;
                }
                if (!isContinueStraight && i == lastCellIdx) // if a turn is made at the last index, add a new, final turn
                {
                    if (stepCounter != TURN_STEP)
                    {
                        throw new System.Exception("The final turn should have length 0, but stepCounter is " + stepCounter);
                    }
                    Checkpoint finalTurnCheckpoint = new Checkpoint(curDirection, stepCounter - TURN_STEP);
                    astarCheckpointList.Add(finalTurnCheckpoint);
                }
                prevDirection = curDirection;
            }
            if (isModifyPath && astarCheckpointList.Count > 0)
            {
                Checkpoint firstCheckpoint = astarCheckpointList[0];
                if (firstCheckpoint.direction != initialDirection) // exclude the turning cell from the modified path
                {
                    print("first checkpoint direction " + firstCheckpoint.direction + " does not match initial direction " + initialDirection + " shortening dist by 1 to " + (firstCheckpoint.length - 1));
                    //astarCheckpointList[0] = new Checkpoint(firstCheckpoint.direction, firstCheckpoint.length - 1);
                }
                else
                {
                    print("first checkpoint direction " + firstCheckpoint.direction + " is the same as initial direction");
                }
            }
            return astarCheckpointList;
        }

        public void onAstarPath(List<Vector3Int> gridCellPathList, Worm.TunnelMaker tunnelMaker, Worm.WormTunnelBroker wormTunnelBroker, bool isInitPath)
        {
            Direction curDirection = wormTunnelBroker.getDirection();
            Vector3Int curCell = wormTunnelBroker.getCurrentCell();

            print("initial direction on follow new path is " + curDirection);
            List<Checkpoint> checkpointList = getCheckpointListFromPath(gridCellPathList, isInitPath, curDirection, curCell);
            if (checkpointList.Count > 0)
            {
                initCheckpointEvent += tunnelMaker.onInitCheckpointList;
                initCheckpointEvent(checkpointList, isInitPath, wormTunnelBroker);
                initCheckpointEvent -= tunnelMaker.onInitCheckpointList;
            }
        }
    }
}