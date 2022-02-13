using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Map
{
    public class Astar : MonoBehaviour
    {
        public delegate void astarPath(List<Vector3Int> gridCellPathList, Worm.TunnelMaker tunnelMaker);
        public event astarPath astarPathEvent;

        private bool isDestinationReceived; // flag set when landmark info is received, which is required for pathplanning

        const int MAP_LENGTH = 10; // distance from origin to one edge of the map
        Vector3Int mapOffset;

        const int DIMENSION_LEN = MAP_LENGTH * 2 + 1;
        const int MAX_LENGTH = 10000000;
        const int STEP_COST = 1;

        Item[,,] CostMap = new Item[DIMENSION_LEN, DIMENSION_LEN, DIMENSION_LEN];

        Dictionary<Vector3Int, GameObject> obstacleDict;
        Vector3Int goalLocation;

        Worm.TunnelMaker currentTunnelMaker;
        private Vector3Int initialCell;

        public class Item
        {
            public int totalCost;
            public int fromStartCost;
            public int toEndCost;
            public Vector3Int cell;
            public Item cameFrom;

            public Item(int fromStartCost, Vector3Int cell, Vector3Int goalLocation)
            {
                this.cell = cell;
                this.fromStartCost = fromStartCost;
                toEndCost = calculateManhattanDist(cell, goalLocation);
                calculateTotalCost();
            }

            public void calculateTotalCost()
            {
                totalCost = fromStartCost + toEndCost;
            }

            public int calculateManhattanDist(Vector3Int fromCell, Vector3Int destCell)
            {
                return Mathf.Abs(fromCell.x - destCell.x) + Mathf.Abs(fromCell.y - destCell.y) + Mathf.Abs(fromCell.z - destCell.z);
            }

        }

        private void OnEnable()
        {
            astarPathEvent += FindObjectOfType<AstarNetwork>().onAstarPath;

            if (FindObjectOfType<Test.AstarVisualizer>())
            {
                astarPathEvent += FindObjectOfType<Test.AstarVisualizer>().onAstarPath;
            }
        }

        private void Awake()
        {
            isDestinationReceived = false;
            mapOffset = new Vector3Int(MAP_LENGTH, MAP_LENGTH, MAP_LENGTH); // offset to convert a cell position to a array position
        }

        public void onInitLandmark(Dictionary<Vector3Int, GameObject> obstacleDict, Vector3Int goalLocation)
        {
            this.obstacleDict = obstacleDict;
            this.goalLocation = goalLocation;
            isDestinationReceived = true;
        }

        public void onFollowPath(Worm.TunnelMaker tunnelMaker)
        {            
            currentTunnelMaker = tunnelMaker;
            initialCell = currentTunnelMaker.getInitialCell();
            StartCoroutine(astar());
        }

        // Start is called before the first frame update
        private IEnumerator astar()
        {
            while (!isDestinationReceived) // wait for destination before starting path planning
            {
                yield return null;
            }

            initializeCostMap();

            Item startItem = new Item(0, initialCell, goalLocation);
            HashSet<Item> unknownPathSet = new HashSet<Item>();

            unknownPathSet.Add(startItem);
            List<Item> unknownPathList = new List<Item>() { startItem };
            findShortestPath(obstacleDict, unknownPathList, unknownPathSet);
        }

        /**
         * Get the shortest path from start cell to goal cell
         */
        List<Vector3Int> getShortestPath()
        {
            List<Vector3Int> shortestPath = new List<Vector3Int>();

            Vector3Int goalArrayPos = goalLocation + mapOffset;
            Item item = CostMap[goalArrayPos.x, goalArrayPos.y, goalArrayPos.z];
            while (!item.cell.Equals(initialCell))
            {
                print("shortest path to goalLocation " + item.cell);
                shortestPath.Insert(0, item.cell);
                item = item.cameFrom;
            }
            shortestPath.Insert(0, initialCell);

            return shortestPath;
        }

        void findShortestPath(Dictionary<Vector3Int, GameObject> obstacleDict, List<Item> unknownPathList, HashSet<Item> unknownPathSet)
        {
            while (unknownPathList.Count > 0)
            {
                Item item = findClosestItem(unknownPathList);

                if (item.cell.Equals(goalLocation))
                {
                    print("shortest distance is " + item.totalCost);
                    List<Vector3Int> shortestPath = getShortestPath();
                    astarPathEvent(shortestPath, currentTunnelMaker);
                    return;
                }

                unknownPathList.Remove(item);
                List<Vector3Int> neighborCells = getNeighbors(item.cell);

                foreach (Vector3Int neighbor in neighborCells)
                {
                    if (isCellWithinBoundaries(neighbor))
                    {
                        if (!obstacleDict.ContainsKey(neighbor))
                        {
                            Vector3Int pos = neighbor + mapOffset;
                            Item neighborItem = CostMap[pos.x, pos.y, pos.z];
                            int fromCurCellDist = item.fromStartCost + STEP_COST;
                            if (fromCurCellDist < neighborItem.fromStartCost)
                            {
                                neighborItem.fromStartCost = fromCurCellDist;
                                neighborItem.cameFrom = item;
                                neighborItem.calculateTotalCost();
                                if (!unknownPathSet.Contains(neighborItem))
                                {
                                    unknownPathSet.Add(neighborItem);
                                    unknownPathList.Add(neighborItem);
                                }
                            }
                        }
                    }
                    else
                    {
                        print("cell " + neighbor + " is not within boundaries");
                    }
                }
            }
        }



        bool isCellWithinBoundaries(Vector3Int cell)
        {
            return Mathf.Abs(cell.x) <= MAP_LENGTH && Mathf.Abs(cell.y) <= MAP_LENGTH && Mathf.Abs(cell.z) <= MAP_LENGTH;
        }

        /**
         * Get the cells it is possible to travel to from cell (look 2 ahead)
         */
        List<Vector3Int> getNeighbors(Vector3Int cell)
        {
            Vector3Int upVector = cell + Vector3Int.up;
            Vector3Int downVector = cell + Vector3Int.down;
            Vector3Int leftVector = cell + Vector3Int.left;
            Vector3Int rightVector = cell + Vector3Int.right;
            Vector3Int backVector = cell + Vector3Int.back;
            Vector3Int forwardVector = cell + Vector3Int.forward;
            return new List<Vector3Int>() { upVector, downVector, rightVector, leftVector, backVector, forwardVector };
        }

        Item findClosestItem(List<Item> shortestPathSet)
        {
            int minDist = MAX_LENGTH;
            Item closestItem = null;
            for (int i = 0; i < shortestPathSet.Count; i++)
            {
                Item item = shortestPathSet[i];
                if (item.totalCost < minDist)
                {
                    minDist = item.totalCost;
                    closestItem = item;
                }
            }
            if (closestItem == null)
            {
                throw new System.Exception("not a valid item");
            }
            return closestItem;
        }

        /**
         * All routes are equally unfeasible in the beginning
         */
        private void initializeCostMap()
        {
            for (int i = 0; i < DIMENSION_LEN; i++)
            {
                for (int j = 0; j < DIMENSION_LEN; j++)
                {
                    for (int k = 0; k < DIMENSION_LEN; k++)
                    {
                        Vector3Int arrayPos = new Vector3Int(i, j, k);
                        Vector3Int cellPos = arrayPos - mapOffset;
                        CostMap[i, j, k] = new Item(MAX_LENGTH, cellPos, goalLocation);
                    }
                }
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Map.AstarNetwork>())
            {
                astarPathEvent -= FindObjectOfType<Map.AstarNetwork>().onAstarPath;
            }
            if (FindObjectOfType<Test.AstarVisualizer>())
            {
                astarPathEvent -= FindObjectOfType<Test.AstarVisualizer>().onAstarPath;
            }
        }

    }

}
