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
         * @curCell             the current cell of the worm
         */
        private List<Checkpoint> getCheckpointListFromPath(List<Vector3Int> gridCellPathList, bool isModifyPath, Direction initialDirection, Vector3Int curCell)
        {
            List<Checkpoint> astarCheckpointList = new List<Checkpoint>();
            Direction prevDirection = Direction.None;
            int lastCellIdx = gridCellPathList.Count - 1;

            int stepCounter = 1;

            for (int i = 1; i < gridCellPathList.Count; i++)
            {
                Vector3Int gridCell = gridCellPathList[i];
                Vector3Int prevCell = gridCellPathList[i - 1];
                print("prev cell is " + prevCell + " curCell is " + gridCell);

                Direction curDirection = Dir.CellDirection.getDirectionFromCells(prevCell, gridCell);

                bool isContinueStraight = curDirection == prevDirection || prevDirection == Direction.None;

                if (isContinueStraight)
                {
                    stepCounter += 1;
                }
                if (!isContinueStraight || i == lastCellIdx) // if a turn is made or it is the final index, add the previous straight segment
                {
                    Checkpoint straightTunnelCheckpoint = new Checkpoint(prevDirection, stepCounter - TURN_STEP);
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
            if (isModifyPath)
            {
                Checkpoint firstCheckpoint = astarCheckpointList[0];
                if (firstCheckpoint.direction != initialDirection) // exclude the turning cell from the modified path
                {
                    astarCheckpointList[0] = new Checkpoint(firstCheckpoint.direction, firstCheckpoint.length - 1);
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

            initCheckpointEvent += tunnelMaker.onInitCheckpointList;
            initCheckpointEvent(checkpointList, isInitPath, wormTunnelBroker);
            initCheckpointEvent -= tunnelMaker.onInitCheckpointList;
        }
    }
}