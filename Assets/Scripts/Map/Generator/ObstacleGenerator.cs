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
        public Dictionary<Obstacle, Vector3Int> swappedObstacleDict; 

        protected string obstacleType;

        [SerializeField]
        protected bool isRandomPlacement; // whether the obstacles are placed randomy or manually specified (locations would be defined in subclasses)

        // TODO: Add abstract methods for initializeObstacleList()

        const int MAP_LENGTH = 10; // distance from origin to one edge of the map
        const int OBSTACLE_MIN_SPACING = 1; // minimum space between one obstacle and any other

        protected void Awake()
        {
            obstacleDict = new Dictionary<Vector3Int, Obstacle>();
            swappedObstacleDict = new Dictionary<Obstacle, Vector3Int>();
        }

        protected abstract List<Obstacle> getObstacleList();

        /**
         * Check whether a cell contains an obstacle
         * 
         * @cell the cell the worm want to enter
         */
        public bool isCellObstacle(Vector3Int cell)
        {
            return obstacleDict.ContainsKey(cell);
        }

        private int getRandomPosition()
        {
            return Random.Range(-MAP_LENGTH, MAP_LENGTH);
        }

        /**
         * Checks whether cell is in bounds of map
         *
         * @cell    the vector3Int cell
         */
        private bool isInBounds(Vector3Int cell)
        {
            if (Mathf.Abs(cell.x) > MAP_LENGTH || Mathf.Abs(cell.y) > MAP_LENGTH || Mathf.Abs(cell.z) > MAP_LENGTH)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Vector3Int getRandomCell()
        {
            int randX = getRandomPosition();
            int randY = getRandomPosition();
            int randZ = getRandomPosition();
            Vector3Int randCell = new Vector3Int(randX, randY, randZ);
            return randCell;
        }

        /**
         * Destroy obstacle at a position and remove references to it in the map
         */
        protected void destroyObstacle(Dictionary<Vector3Int, Obstacle> specificObstacleDict, Dictionary<Obstacle, Vector3Int> swapSpecificObstacleDict, Vector3Int currentPosition, List<Obstacle> specificObstacleList)
        {
            Obstacle obstacle = obstacleDict[currentPosition];
            specificObstacleList.Remove(obstacle);
            GameObject obstacleGO = obstacle.obstacleObject;
            obstacleDict.Remove(currentPosition);
            swappedObstacleDict.Remove(obstacle);
            specificObstacleDict.Remove(currentPosition);
            swapSpecificObstacleDict.Remove(obstacle);
            Destroy(obstacleGO);
        }

        /**
         * Get the obstacle at a cell and check both the object-specific dict and combined-dict are synced
         */
        protected Obstacle getObstacle(Vector3Int currentPosition, Dictionary<Vector3Int, Obstacle> specificObstacleDict) 
        {
            Obstacle Obstacle = obstacleDict[currentPosition];
            if (Obstacle != specificObstacleDict[currentPosition])
            {
                throw new System.Exception("Obstacle " + Obstacle.obstacleType + " at " + currentPosition + ", does not match specific obstacle dict entry: " + specificObstacleDict[currentPosition].obstacleType);
            }
            return Obstacle;
        }

        /**
         * Update the position of obstacles that can move (eg worms)
         * 
         * @currentPosition                 the cell the obstacle is in currently
         * @nextPosition                    the updated cell position
         * @specificObstaclePosDict         dictionary entry <position, obstacle>
         * @swapSpecificObstaclePosDict     swap dictionary entry <obstacle, position>
         */
        protected Obstacle updateObstacle(Obstacle ObstacleToUpdate, Dictionary<Vector3Int, Obstacle> specificObstacleDict, Dictionary<Obstacle, Vector3Int> swapSpecificObstacleDict, Vector3Int oldPosition, Vector3Int nextPosition)
        {
            if (obstacleDict.ContainsKey(nextPosition))
            {
                Obstacle obstacle = obstacleDict[nextPosition];
                if (obstacle != null) // if an obstacle exists in the cell worm is about to enter
                {
                    throw new System.Exception("Next position at " + nextPosition + " is occupied by " + obstacleDict[nextPosition].obstacleType + " but " + ObstacleToUpdate.obstacleType + " is trying to move to it. Collide event should have been fired and dealt with");
                }
            }
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
            return ObstacleToUpdate;
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
            obstacleDict = new Dictionary<Vector3Int, Obstacle>();

            for (int i=0;i<obstacleList.Count;i++)
            {
                Obstacle obstacle = obstacleList[i];

                Vector3Int randObstaclePosition;

                if (isRandomPlacement)
                {
                    randObstaclePosition = getRandomEligibleCell();
                    obstacle.setObstacleCell(randObstaclePosition);
                }
                else // if not randomly placed use the pre-set cell
                {
                    if (obstacle.obstacleCell == null)
                    {
                        throw new System.Exception("obstacle cell for obstacle " + obstacle.obstacleId + " is not defined");
                    }
                    randObstaclePosition = obstacle.obstacleCell;
                }

                addEntryToObstacleDict(randObstaclePosition, obstacle, obstacleDict, swappedObstacleDict);
                addEntryToObstacleDict(randObstaclePosition, obstacle, specificObstacleDict, swappedSpecificObstacleDict);
            }
        }

        /**
         * Add <CellPos, Obstacle> entry to dictionary and its swapped version to allow bidirection O(1) access between CellPos <-> Obstacle/
         * 
         * @obstacleCellPos         cell int position of obstacle
         * @obstacle                the obstacle which contains gameobject and type
         * @obstacleDict            dictionary allow look-up of obstacles by cell pos
         * @swappedObstacleDict     dictionary allow look-up of cell pos by obstacle
         */
        private void addEntryToObstacleDict(Vector3Int obstacleCellPos, Obstacle obstacle, Dictionary<Vector3Int, Obstacle> obstacleDict, Dictionary<Obstacle, Vector3Int> swappedObstacleDict)
        {
            obstacleDict[obstacleCellPos] = obstacle;
            swappedObstacleDict[obstacle] = obstacleCellPos;
        }

        /**
         * Choose a random cell in the map that satisfies conditions of obstacle placement such as distance to nearest obstacle
         */
        public Vector3Int getRandomEligibleCell()
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
        private bool isObstacleLocationEligible(Vector3Int originalCell)
        {
            int checkCellArea = OBSTACLE_MIN_SPACING * 2 + 1; // check cells a certain distance away in all directions (x,y,z)
            Vector3Int minSpacingVector = new Vector3Int(checkCellArea, checkCellArea, checkCellArea);
            Vector3Int bottomLeftCell = minSpacingVector - originalCell;
            for (int x = 0; x < OBSTACLE_MIN_SPACING; x++)
            {
                for (int y = 0; y < OBSTACLE_MIN_SPACING; y++)
                {
                    for (int z = 0; z < OBSTACLE_MIN_SPACING; z++)
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

