using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class AstarNetwork : MonoBehaviour
    {
        public delegate void initCheckpoint(List<Checkpoint> checkpointList);
        public event initCheckpoint initCheckpointEvent;

        const int TURN_STEP = 1;

        private void OnEnable()
        {
            initCheckpointEvent += FindObjectOfType<Test.TunnelMaker>().onInitCheckpointList;
            initCheckpointEvent += FindObjectOfType<Test.WormCollider>().onInitCheckpointList;
        }

        /**
         * Convert the list of cell positions to a list of directions for TunnelMaker to process
         * 
         * @gridCellPathList is the list of cell posiitons along the path
         * @nullCell initially set to a default value, which is the default null value
         */
        private List<Checkpoint> getCheckpointListFromPath(List<Vector3Int> gridCellPathList)
        {
            List<Checkpoint> astarCheckpointList = new List<Checkpoint>();
            Direction prevDirection = Direction.None;
            int lastCellIdx = gridCellPathList.Count - 1;

            int stepCounter = 1;

            for (int i=1;i<gridCellPathList.Count;i++)
            {
                Vector3Int gridCell = gridCellPathList[i];
                Vector3Int prevCell = gridCellPathList[i - 1];
                Direction curDirection = Dir.CellDirection.getDirectionFromCells(prevCell, gridCell);

                bool isContinueStraight = curDirection == prevDirection || prevDirection == Direction.None;

                if (isContinueStraight)
                {
                    stepCounter += 1;
                }
                if (!isContinueStraight || i == lastCellIdx)
                {
                    Checkpoint straightTunnelCheckpoint = new Checkpoint(prevDirection, stepCounter - TURN_STEP);
                    astarCheckpointList.Add(straightTunnelCheckpoint);
                    stepCounter = 1;
                }
                prevDirection = curDirection;
            }
            return astarCheckpointList;
        }

        public void onAstarPath(List<Vector3Int> gridCellPathList)
        {
            List<Checkpoint> checkpointList = getCheckpointListFromPath(gridCellPathList);
            initCheckpointEvent(checkpointList);
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Test.TunnelMaker>())
            {
                initCheckpointEvent -= FindObjectOfType<Test.TunnelMaker>().onInitCheckpointList;
            }
            if (FindObjectOfType<Test.WormCollider>())
            {
                initCheckpointEvent += FindObjectOfType<Test.WormCollider>().onInitCheckpointList;
            }
        }
    }

}