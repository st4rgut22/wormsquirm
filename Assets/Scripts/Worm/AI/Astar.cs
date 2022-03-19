using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Map
{
    public class Astar : MonoBehaviour
    {
        public delegate void astarPath(List<Vector3Int> gridCellPathList, Worm.TunnelMaker tunnelMaker, Worm.WormTunnelBroker wormTunnelBroker, bool isInitPath);
        public event astarPath astarPathEvent;

        private bool isDestinationReceived; // flag set when landmark info is received, which is required for pathplanning

        const int MAP_LENGTH = 10; // distance from origin to one edge of the map
        Vector3Int mapOffset;

        const int DIMENSION_LEN = MAP_LENGTH * 2 + 1;
        const int MAX_LENGTH = 10000000;
        const int STEP_COST = 1;

        Item[,,] CostMap = new Item[DIMENSION_LEN, DIMENSION_LEN, DIMENSION_LEN];

        Vector3Int objectiveLocation;

        Worm.TunnelMaker currentTunnelMaker;
        Worm.WormTunnelBroker currentTunnelBroker;

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

        /**
         * Initialize objective location in the map
         * 
         * @goalLocation        the cell containing the objective (game termination condition), 
         *                      which  we use to generate a attack path to player or path to reward
         */
        public void onInitObjective(Vector3Int objectiveLocation)
        {
            this.objectiveLocation = objectiveLocation;
            isDestinationReceived = true;
        }

        /**
         * Get the initial cell the path should use
         *
         *@wormTunnelBroker     worm class with info about tunnel position
         */
        private Vector3Int getCell(Worm.WormTunnelBroker wormTunnelBroker)
        {
            Vector3Int pathInitialCell;
            if (wormTunnelBroker.isTunnelCreated)
            {
                pathInitialCell = wormTunnelBroker.getNextCell();
                print("choose initial cell as next cell " + pathInitialCell);
            }
            else
            {
                pathInitialCell = wormTunnelBroker.getCurrentCell();
                print("choose initial cell as current cell " + pathInitialCell);
            }
            return pathInitialCell;
        }

        public void onFollowPath(Worm.TunnelMaker tunnelMaker, Worm.WormTunnelBroker wormTunnelBroker)
        {
            currentTunnelMaker = tunnelMaker;
            currentTunnelBroker = wormTunnelBroker;
            Vector3Int initialCell = getCell(wormTunnelBroker);
            astar(initialCell, wormTunnelBroker.isTunnelCreated);
        }

        /**
         * Start is called before the first frame update
         * 
         * @startingCell    the cell the worm is currently in
         */
        private void astar(Vector3Int startingCell, bool isPathInitialized)
        {
            if (!isDestinationReceived) // if destination not received dont follow path because startingCell will be stale
            {
                return;
                //throw new System.Exception("Worm is supposed to create a path but the destination is not known");
            }

            initializeCostMap();
            print("find path from " + startingCell + " to objective " + objectiveLocation);
            Item startItem = new Item(0, startingCell, objectiveLocation);
            HashSet<Item> unknownPathSet = new HashSet<Item>();

            unknownPathSet.Add(startItem);
            List<Item> unknownPathList = new List<Item>() { startItem };
            findShortestPath(unknownPathList, unknownPathSet, startingCell, isPathInitialized);
            isDestinationReceived = false;
        }

        /**
         * Get the shortest path from start cell to goal cell
         */
        List<Vector3Int> getShortestPath(Vector3Int startingCell)
        {
            List<Vector3Int> shortestPath = new List<Vector3Int>();

            Vector3Int goalArrayPos = objectiveLocation + mapOffset;
            Item item = CostMap[goalArrayPos.x, goalArrayPos.y, goalArrayPos.z];
            while (!item.cell.Equals(startingCell))
            {
                print("shortest path to goalLocation " + item.cell);
                shortestPath.Insert(0, item.cell);
                item = item.cameFrom;
            }
            shortestPath.Insert(0, startingCell);

            return shortestPath;
        }

        /**
         * When finding the shortest path avoid any obstacles along the path (unless the obstacle is self or goal)
         * 
         * @startingCell        the cell the path starts at
         * @cell                the current cell to check
         */
        private bool isCellFree(Vector3Int startingCell, Vector3Int cell)
        {
            if (startingCell.Equals(cell) || cell.Equals(objectiveLocation))
            {
                return true;
            }
            else
            {
                return !ObstacleGenerator.obstacleDict.ContainsKey(cell);
            }
        }

        void findShortestPath(List<Item> unknownPathList, HashSet<Item> unknownPathSet, Vector3Int startingCell, bool isPathInitialized)
        {
            while (unknownPathList.Count > 0)
            {
                Item item = findClosestItem(unknownPathList);

                if (item.cell.Equals(objectiveLocation))
                {
                    print("shortest distance is " + item.totalCost);
                    List<Vector3Int> shortestPath = getShortestPath(startingCell);
                    astarPathEvent(shortestPath, currentTunnelMaker, currentTunnelBroker, isPathInitialized);
                    return;
                }

                unknownPathList.Remove(item);
                List<Vector3Int> neighborCells = getNeighbors(item.cell);

                foreach (Vector3Int neighbor in neighborCells)
                {
                    if (isCellWithinBoundaries(neighbor))
                    {
                        if (isCellFree(startingCell, neighbor))
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
                        else
                        {
                            print("uoh");
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
                        CostMap[i, j, k] = new Item(MAX_LENGTH, cellPos, objectiveLocation);
                    }
                }
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Map.AstarNetwork>())
            {
                astarPathEvent -= FindObjectOfType<AstarNetwork>().onAstarPath;
            }
            if (FindObjectOfType<Test.AstarVisualizer>())
            {
                astarPathEvent -= FindObjectOfType<Test.AstarVisualizer>().onAstarPath;
            }
        }

    }

}
