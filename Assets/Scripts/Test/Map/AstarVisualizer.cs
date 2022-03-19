using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class AstarVisualizer : MonoBehaviour
    {
        /**
         * Visualize the astar path the worm will take to get to the goal
         * 
         * @gridCellPathList is the list of cell posiitons along the path
         * @nullCell initially set to a default value, which is the default null value
         */
        private void visualizePath(List<Vector3Int> gridCellPathList)
        {
            for (int i=1;i<gridCellPathList.Count;i++)
            {
                Vector3Int gridCell = gridCellPathList[i];
                Vector3Int prevGridCell = gridCellPathList[i - 1];
                print("grid cell is " + gridCell);
                // convert cellPos to center of cell
                Vector3 cellPos = MapUtility.getCellPos(gridCell);
                Vector3 prevGridCellPos = MapUtility.getCellPos(prevGridCell);
                if (i > 0)
                {
                    Debug.DrawLine(prevGridCellPos, cellPos, Color.green, 20);
                }
            }
        }

        public void onAstarPath(List<Vector3Int> gridCellPathList, Worm.TunnelMaker tunnelMaker, Worm.WormTunnelBroker wormTunnelBroker, bool isInitPath)
        {
            visualizePath(gridCellPathList);
        }
    }
}