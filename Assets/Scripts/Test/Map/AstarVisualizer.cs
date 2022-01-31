using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class AstarVisualizer : MonoBehaviour
    {
        private void OnEnable()
        {
            if (FindObjectOfType<Astar>())
            {
                FindObjectOfType<Astar>().astarPathEvent += onAstarPath;
            }
        }

        /**
         * Visualize the astar path the worm will take to get to the goal
         * 
         * @gridCellPathList is the list of cell posiitons along the path
         * @nullCell initially set to a default value, which is the default null value
         */
        private void visualizePath(List<Vector3Int> gridCellPathList, Vector3 nullCell)
        {
            Vector3 lastCell = nullCell;
            foreach (Vector3Int gridCell in gridCellPathList)
            {
                // convert cellPos to center of cell
                Vector3 cellPos = MapUtility.getCellPos(gridCell);
                if (!lastCell.Equals(nullCell))
                {
                    Debug.DrawLine(lastCell, cellPos, Color.green, 60);
                }
                lastCell = cellPos;
            }
        }

        public void onAstarPath(List<Vector3Int>gridCellPathList, Vector3 nullCell)
        {
            visualizePath(gridCellPathList, nullCell);
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Astar>())
            {
                FindObjectOfType<Astar>().astarPathEvent -= onAstarPath;
            }
        }
    }
}