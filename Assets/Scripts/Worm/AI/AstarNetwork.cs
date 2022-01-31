using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class AstarNetwork : MonoBehaviour
    {
        public delegate void initCheckpoint(List<Checkpoint> checkpointList);
        public event initCheckpoint initCheckpointEvent;

        private void OnEnable()
        {
            if (FindObjectOfType<Astar>())
            {
                FindObjectOfType<Astar>().astarPathEvent += onAstarPath;
            }
            initCheckpointEvent += FindObjectOfType<Test.TunnelMaker>().onInitCheckpointList;
        }

        /**
         * Convert the list of cell positions to a list of directions for TunnelMaker to process
         * 
         * @gridCellPathList is the list of cell posiitons along the path
         * @nullCell initially set to a default value, which is the default null value
         */
        private List<Checkpoint> getCheckpointListFromPath(List<Vector3Int> gridCellPathList, Vector3 nullCell)
        {
            List<Checkpoint> astarCheckpointList = new List<Checkpoint>();
            Direction prevDirection = Direction.None;
            int lastCellIdx = gridCellPathList.Count - 1;

            int stepCounter = 1;

            for (int i=0;i<gridCellPathList.Count;i++)
            {
                Vector3Int gridCell = gridCellPathList[i];
                if (i > 0)
                {
                    Vector3Int prevCell = gridCellPathList[i - 1];
                    Direction curDirection = Dir.CellDirection.getDirectionFromCells(prevCell, gridCell);
                    bool isContinueStraight = curDirection == prevDirection || prevDirection == Direction.None;

                    if (isContinueStraight)
                    {
                        stepCounter += 1;
                    }
                    if (!isContinueStraight || i == lastCellIdx)
                    {
                        Checkpoint checkpoint = new Checkpoint(prevDirection, stepCounter);
                        astarCheckpointList.Add(checkpoint);
                        stepCounter = 1;
                    }
                    prevDirection = curDirection;
                }
            }
            return astarCheckpointList;
        }

        public void onAstarPath(List<Vector3Int> gridCellPathList, Vector3 nullCell)
        {
            List<Checkpoint> checkpointList = getCheckpointListFromPath(gridCellPathList, nullCell);
            initCheckpointEvent(checkpointList);
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Astar>())
            {
                FindObjectOfType<Astar>().astarPathEvent -= onAstarPath;
            }
            if (FindObjectOfType<Test.TunnelMaker>())
            {
                initCheckpointEvent -= FindObjectOfType<Test.TunnelMaker>().onInitCheckpointList;
            }
        }
    }

}