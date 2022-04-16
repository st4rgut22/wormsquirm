using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public abstract class ObstacleGenerator : MonoBehaviour
    {
        // a dictionary comprised of all obstacles in the map       <Cell Location, Obstacle containing GameObject>
        public static Dictionary<Vector3Int, Obstacle> obstacleDict;
        // a dictionary comprised of all obstacles in the map       <Obstacle containing GameObject, Cell Location>
        public static Dictionary<Obstacle, Vector3Int> swappedObstacleDict;
        // a dictionary of <obstacle id, initial obstacle position> pairs

        protected static Dictionary<string, Vector3Int> initPositionDict;

        protected List<Obstacle> obstacleList;
        protected string obstacleType;

        // TODO: Add abstract methods for initializeObstacleList()

        const int MAP_LENGTH = 10; // distance from origin to one edge of the map
        const int OBSTACLE_MIN_SPACING = 1; // minimum space between one obstacle and any other

        protected void Awake()
        {
            obstacleDict = new Dictionary<Vector3Int, Obstacle>();
            swappedObstacleDict = new Dictionary<Obstacle, Vector3Int>(new ObstacleComparer());
            initPositionDict = new Dictionary<string, Vector3Int>();
        }

        protected abstract List<Obstacle> getObstacleList();


        protected class ObstacleComparer : IEqualityComparer<Obstacle>
        {
            public bool Equals(Obstacle obstacle1, Obstacle obstacle2) // used to check for equality when there is a has collision
            {
                return obstacle1.obstacleId == obstacle2.obstacleId;
            }

            public int GetHashCode(Obstacle obstacle) // used to get an object with a key
            {
                return obstacle.obstacleId.GetHashCode();
            }
        }


        /**
            * Check whether a cell contains an obstacle
            * 
            * @cell the cell the worm want to enter
            */
        public bool isCellObstacle(Vector3Int cell)
        {
            return obstacleDict.ContainsKey(cell);
        }

        private static int getRandomPosition()
        {
            return Random.Range(-MAP_LENGTH, MAP_LENGTH);
        }

        /**
         * Checks whether cell is in bounds of map
         *
         * @cell    the vector3Int cell
         */
        private static bool isInBounds(Vector3Int cell)
        {
            if (Mathf.Abs(cell.x) > MAP_LENGTH || Mathf.Abs(cell.y) > MAP_LENGTH || Mathf.Abs(cell.z) > MAP_LENGTH)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private static Vector3Int getRandomCell()
        {
            int randX = getRandomPosition();
            int randY = getRandomPosition();
            int randZ = getRandomPosition();
            Vector3Int randCell = new Vector3Int(randX, randY, randZ);
            return randCell;            
        }

        /**
         * Set the obstacle position at the center of the cell
         */
        protected virtual void positionObstacles()
        {
            for (int i=0; i < obstacleList.Count; i++)
            {
                Obstacle obstacle = obstacleList[i];
                obstacle.obstacleObject.transform.position = obstacle.obstacleCell.getCellCenter();
            }
        }

        /**
         * Destroy obstacle at a position and remove references to it in the map
         */
        protected static void destroyObstacle(Dictionary<Vector3Int, Obstacle> specificObstacleDict, Dictionary<Obstacle, Vector3Int> swapSpecificObstacleDict, Vector3Int currentPosition, List<Obstacle> specificObstacleList)
        {
            Obstacle obstacle = obstacleDict[currentPosition];
            specificObstacleList.Remove(obstacle);
            print("destroy obstacle " + obstacle.obstacleId + " at cell " + currentPosition);
            GameObject obstacleGO = obstacle.obstacleObject;
            obstacleDict.Remove(currentPosition);
            swappedObstacleDict.Remove(obstacle);
            specificObstacleDict.Remove(currentPosition);
            swapSpecificObstacleDict.Remove(obstacle);
            Destroy(obstacleGO);
        }

        /**
         * Get the obstacle at a cell and check both the object-specific dict and combined-dict are synced. 
         * Return null if the cell doesnt contain an obstalce (for instance if it was destroyed)
         */
        protected static Obstacle getObstacle(Vector3Int currentPosition, Dictionary<Vector3Int, Obstacle> specificObstacleDict) 
        {
            if (obstacleDict.ContainsKey(currentPosition))
            {
                Obstacle Obstacle = obstacleDict[currentPosition];
                if (Obstacle != specificObstacleDict[currentPosition])
                {
                    throw new System.Exception("Obstacle " + Obstacle.obstacleType + " at " + currentPosition + ", does not match specific obstacle dict entry: " + specificObstacleDict[currentPosition].obstacleType);
                }
                return Obstacle;
            }
            else
            {
                return null;
            }
        }

        /**
         * Update the position of obstacles that can move (eg worms)
         * 
         * @currentPosition                 the cell the obstacle is in currently
         * @nextPosition                    the updated cell position
         * @specificObstaclePosDict         dictionary entry <position, obstacle>
         * @swapSpecificObstaclePosDict     swap dictionary entry <obstacle, position>
         * @deleteCurCell                   whether the tunnel at currentPosition should be removed (eg if it does not exist)
         */
        protected static Obstacle updateObstacle(Obstacle ObstacleToUpdate, Dictionary<Vector3Int, Obstacle> specificObstacleDict, Dictionary<Obstacle, Vector3Int> swapSpecificObstacleDict, Vector3Int oldPosition, Vector3Int nextPosition, bool isDeleteCurCell)
        {
            if (obstacleDict.ContainsKey(nextPosition))
            {
                Obstacle obstacle = obstacleDict[nextPosition];
                if (obstacle != null) // if an obstacle exists in the cell worm is about to enter
                {
                    throw new System.Exception("Next position at " + nextPosition + " is occupied by " + obstacleDict[nextPosition].obstacleType + " but " + ObstacleToUpdate.obstacleType + " is trying to move to it. Collide event should have been fired and dealt with");
                }
            }
            print("update obstacle dict with " + ObstacleToUpdate.obstacleId + " at next position " + nextPosition);
            obstacleDict[nextPosition] = ObstacleToUpdate;
            specificObstacleDict[nextPosition] = ObstacleToUpdate;
            obstacleDict[oldPosition] = null; // remove reference for the previous key (aka cell position)
            specificObstacleDict[oldPosition] = null;

            if (swappedObstacleDict[ObstacleToUpdate] != swapSpecificObstacleDict[ObstacleToUpdate])
            {
                throw new System.Exception("Swapped obstacle dict's positions dont match for obstacle " + ObstacleToUpdate.obstacleType);
            }
            swappedObstacleDict[ObstacleToUpdate] = nextPosition;
            swapSpecificObstacleDict[ObstacleToUpdate] = nextPosition;

            if (isDeleteCurCell)
            {
                obstacleDict.Remove(oldPosition);
                specificObstacleDict.Remove(oldPosition);
            }
            return ObstacleToUpdate;
        }

        /**
         * Tests placement of worms
         */
        private static Vector3Int getTestCellLocationForAIChaseWorm(Obstacle obstacle)
        {
            if (obstacle.obstacleId == "Player")
            {
                return new Vector3Int(8, -5, 6);
            }
            else
            {
                return new Vector3Int(-4, 1, -3);
            }
        }

        /**
         * Randomly generate landmarks within the confines of the map
         * 
         * @specificObstacleDict            the obstacle dictionary for a specific obstacle type (eg rocks, players)
         * @obstacleList                    the list of gameobjects of a specific obstacle type
         * @obstacleType                    the name of the obstacle
         */
        protected void initializeObstacleDict(Dictionary<Vector3Int, Obstacle> specificObstacleDict, Dictionary<Obstacle, Vector3Int> swappedSpecificObstacleDict, List<Obstacle>obstacleList)
        {
            for (int i=0;i<obstacleList.Count;i++)
            {
                Obstacle obstacle = obstacleList[i];

                Vector3Int obstaclePosition;

                if (!initPositionDict.ContainsKey(obstacle.obstacleId)) // if no predefined starting cel, use a random cell
                {
                    obstaclePosition = getRandomEligibleCell();
                    //randObstaclePosition = getTestCellLocationForAIChaseWorm(obstacle); // TESTING
                }
                else // if not randomly placed use the pre-set cell
                {
                    if (obstacle.obstacleCell == null)
                    {
                        throw new System.Exception("obstacle cell for obstacle " + obstacle.obstacleId + " is not defined");
                    }
                    obstaclePosition = initPositionDict[obstacle.obstacleId];
                }
                obstacle.setObstacleCell(obstaclePosition);

                addEntryToObstacleDict(obstaclePosition, obstacle, obstacleDict, swappedObstacleDict);
                addEntryToObstacleDict(obstaclePosition, obstacle, specificObstacleDict, swappedSpecificObstacleDict);
            }
            this.obstacleList = obstacleList;
        }

        /**
         * Add <CellPos, Obstacle> entry to dictionary and its swapped version to allow bidirection O(1) access between CellPos <-> Obstacle/
         * 
         * @obstacleCellPos         cell int position of obstacle
         * @obstacle                the obstacle which contains gameobject and type
         * @obstacleDict            dictionary allow look-up of obstacles by cell pos
         * @swappedObstacleDict     dictionary allow look-up of cell pos by obstacle
         */
        private static void addEntryToObstacleDict(Vector3Int obstacleCellPos, Obstacle obstacle, Dictionary<Vector3Int, Obstacle> obstacleDict, Dictionary<Obstacle, Vector3Int> swappedObstacleDict)
        {
            if (obstacle.obstacleType == ObstacleType.AIWorm)
            {
                print("add entry to obstacle dict at cell " + obstacleCellPos + " for obstacle " + obstacle.obstacleId);
            }            
            obstacleDict[obstacleCellPos] = obstacle;
            swappedObstacleDict[obstacle] = obstacleCellPos;
        }

        /**
         * Choose a random cell in the map that satisfies conditions of obstacle placement such as distance to nearest obstacle
         */
        public static Vector3Int getRandomEligibleCell()
        {
            while (true)
            {
                Vector3Int randObstaclePosition = getRandomCell();
                bool isCellAvailable = isObstacleLocationEligible(randObstaclePosition);
                if (isCellAvailable)
                {
                    return randObstaclePosition;
                }
            }
        }

        /**
         * Because bi-directional lookup of <Obstacle, Obstacle Cell Pos> would be usefule, create a new dictionary that contains the same 
         * contents as the old, but with key-value pairs swapped
         * 
         * @obstacleDictionary      the original dictionary that supports lookup by Obstacle Cell Position
         */
        private Dictionary<Obstacle, Vector3Int> swapObstacleDictionary(Dictionary<Vector3Int, Obstacle> obstacleDictionary)
        {
            Dictionary<Obstacle, Vector3Int> SwappedObstacleDictionary = new Dictionary<Obstacle, Vector3Int>();

            foreach (KeyValuePair<Vector3Int, Obstacle> obstacleEntry in obstacleDictionary)
            {
                SwappedObstacleDictionary[obstacleEntry.Value] = obstacleEntry.Key;
            }
            return SwappedObstacleDictionary;
        }

        /**
         * Check whether an area surrounding the proposed location is free of obstacles
         *
         *@originalCell     the proposed location of an obstacle
         */
        private static bool isObstacleLocationEligible(Vector3Int originalCell)
        {
            int checkCellArea = OBSTACLE_MIN_SPACING * 2 + 1; // check cells a certain distance away in all directions (x,y,z)
            Vector3Int minSpacingVector = new Vector3Int(OBSTACLE_MIN_SPACING, OBSTACLE_MIN_SPACING, OBSTACLE_MIN_SPACING);
            Vector3Int bottomLeftCell = originalCell - minSpacingVector;
            for (int x = 0; x < checkCellArea; x++)
            {
                for (int y = 0; y < checkCellArea; y++)
                {
                    for (int z = 0; z < checkCellArea; z++)
                    {
                        Vector3Int cell = new Vector3Int(bottomLeftCell.x + x, originalCell.y + y, originalCell.z + z);
                        bool isCellInBounds = isInBounds(cell);
                        bool isCellBlocked = obstacleDict.ContainsKey(cell);
                        if (isCellBlocked || !isCellInBounds)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}

