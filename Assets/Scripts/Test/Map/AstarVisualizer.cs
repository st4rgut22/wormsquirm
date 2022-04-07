using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class AstarVisualizer : MonoBehaviour
    {
        private void Awake()
        {
            visualizeMapBoundaries();
        }

        /**
         * Visualize the edges of map, beyond which player is out of bounds.
         * The cube map has 12 edges so 12 lines must be drawn between 8 vertices
         */
        private void visualizeMapBoundaries()
        {
            int mapLength = GameManager.MAP_LENGTH;
            Vector3 downBottomLeft = new Vector3(-mapLength, -mapLength, -mapLength);
            Vector3 downBottomRight = new Vector3(mapLength, -mapLength, -mapLength);
            Vector3 downTopLeft = new Vector3(-mapLength, -mapLength, mapLength);
            Vector3 downTopRight = new Vector3(mapLength, -mapLength, mapLength);

            Vector3 upBottomLeft = new Vector3(-mapLength, mapLength, -mapLength);
            Vector3 upBottomRight = new Vector3(mapLength, mapLength, -mapLength);
            Vector3 upTopLeft = new Vector3(-mapLength, mapLength, mapLength);
            Vector3 upTopRight = new Vector3(mapLength, mapLength, mapLength);

            drawFaceX(downBottomLeft, downBottomRight, downTopLeft, downTopRight);
            drawFaceX(downBottomLeft, downBottomRight, upBottomLeft, upBottomRight);
            drawFaceX(downBottomRight, downTopRight, upBottomRight, upTopRight);
            drawFaceX(downTopLeft, downBottomLeft, upTopLeft, upBottomLeft);
            drawFaceX(upBottomLeft, upBottomRight, upTopLeft, upTopRight);
            drawFaceX(downTopLeft, downTopRight, upTopLeft, upTopRight);

            connectFaceEdges(downBottomLeft, downBottomRight, downTopLeft, downTopRight);
            connectFaceEdges(upBottomLeft, upBottomRight, upTopLeft, upTopRight);

            connectFaces(upBottomLeft, upBottomRight, upTopLeft, upTopRight, downBottomLeft, downBottomRight, downTopLeft, downTopRight);
        }

        /**
         * Draw the X on a cube  face marking outer boundary
         * @bl bottom left vertex of face ...
         */
        private void drawFaceX(Vector3 bl, Vector3 br, Vector3 tl, Vector3 tr)
        {
            Debug.DrawLine(bl, tr, Color.blue, 180);
            Debug.DrawLine(br, tl, Color.blue, 180);
        }

        /**
         * Draw the lines defining a single face (square)
         */
        private void connectFaceEdges(Vector3 bl, Vector3 br, Vector3 tl, Vector3 tr)
        {
            Debug.DrawLine(bl, br, Color.blue, 180);
            Debug.DrawLine(bl, tl, Color.blue, 180);
            Debug.DrawLine(tl, tr, Color.blue, 180);
            Debug.DrawLine(tr, br, Color.blue, 180);
        }

        /**
         * Draw the lines connecting the top and bottom face
         */
        private void connectFaces(Vector3 ubl, Vector3 ubr, Vector3 utl, Vector3 utr, Vector3 dbl, Vector3 dbr, Vector3 dtl, Vector3 dtr)
        {
            Debug.DrawLine(ubl, dbl, Color.blue, 180);
            Debug.DrawLine(ubr, dbr, Color.blue, 180);
            Debug.DrawLine(utl, dtl, Color.blue, 180);
            Debug.DrawLine(utr, dtr, Color.blue, 180);
        }

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