using System.Collections.Generic;
using System;
using UnityEngine;

namespace Tunnel
{
    public class Map : MonoBehaviour
    {
        public delegate void Collide(DirectionPair directionPair, Tunnel curTunnel, Tunnel nextTunnel);
        private event Collide CollideEvent;

        public static Dictionary<Vector3Int, Tunnel> TunnelMapDict;
        public static List<Vector3Int> cellList;

        public static Vector3Int startingCell; // the cell location of the first tunnel

        private bool isTurnDecision;

        private const float XZ_INTERVAL_OFFSET = .5f;

        private void Awake()
        {
            cellList = new List<Vector3Int>(); // { TunnelManager.Instance.initialCell };
            TunnelMapDict = new Dictionary<Vector3Int, Tunnel>();
            isTurnDecision = false;
        }

        private void OnEnable()
        {
            CollideEvent += CollisionManager.Instance.onCollide;
        }

        /**
         * Store the first cell added
         */
        public void onStartMove(Direction direction)
        {
            startingCell = Dir.Vector.getNextCellFromDirection(TunnelManager.Instance.initialCell, direction);
        }

        /**
         * When a decision has been made set a flag in order to prevent junction creation on collision. Instead, let 
         * ChangeDirectionEvent() trigger junction creation
         */
        public void onDecision(bool isStraightTunnel, Direction direction, Tunnel tunnel)
        {
            isTurnDecision = true;
        }

        /**
         * When tunnel network is reset clear the map
         */
        public void onResetTunnelNetwork()
        {
            TunnelMapDict.Clear();
            cellList.Clear();
        }

        /**
         * Adds tunnel type to the map at the provided coordinate
         */
        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Straight tunnel)
        {
            if (isBlockInterval && !isTurnDecision)
            {
                // before traversing cell check that the next cell is empty to decide whether to intersect it
                if (containsCell(blockPositionInt)) // exclude turn tunnel segments from part of the straight tunnel
                {
                    print("collision occurred with straight tunnel at " + blockPositionInt);
                    Tunnel intersectTunnel = TunnelMapDict[blockPositionInt];
                    DirectionPair dirPair = new DirectionPair(tunnel.growthDirection, tunnel.growthDirection); // go straight
                    CollideEvent(dirPair, tunnel, intersectTunnel); // send event when tunnels intersect AND not turning
                }
                else
                {
                    addCell(blockPositionInt, tunnel);
                }
            }
            else if (isBlockInterval)
            {
                isTurnDecision = false; // reset isTurnDecision after reaching a block interval in order to permit collisions with straight tunnel
            }
        }

        /**
          * Use worm's rounded position as the cell position. Add an offset on the XZ directions to get the map cell positions
          * 
          * @position clit position
          */
        public static Tunnel getCurrentTunnel(Vector3 position)
        {
            print("clit position before " + position);
            if (position.x > XZ_INTERVAL_OFFSET)
            {
                position.x += XZ_INTERVAL_OFFSET;
            }
            else if (position.x < -XZ_INTERVAL_OFFSET)
            {
                position.x -= XZ_INTERVAL_OFFSET;
            }
            if (position.z > XZ_INTERVAL_OFFSET)
            {
                position.z += XZ_INTERVAL_OFFSET;
            }
            else if (position.z < -XZ_INTERVAL_OFFSET)
            {
                position.z -= XZ_INTERVAL_OFFSET;
            }
            Vector3Int cellPos = Dir.Vector.castToVector3Int(position);
            print("clit position after adjustment " + position + " equals cell position " + cellPos);
            Tunnel tunnel = getTunnelFromDict(cellPos);
            return tunnel;
        }

        /**
         * Gets the rounded block size
         */
        static double getRoundedDistance(double distance)
        {
            double roundedScale = Math.Round(distance * 1000f) / 1000f;
            return roundedScale;
        }

        /**
         * Returns true if distance along axis is a multiple of 1
         */
        public static bool isDistanceMultiple(double distance)
        {
            double roundedDistance = getRoundedDistance(distance);
            return (roundedDistance % 1) == 0;
        }

        public void onAddTunnel(Tunnel tunnel, Vector3Int cellLocation, DirectionPair directionPair, string wormId)
        {
            addCell(cellLocation, tunnel);
        }

        public static bool containsCell(Vector3Int cell)
        {
            return TunnelMapDict.ContainsKey(cell);
        }

        /**
         * Save tunnel type at a particular location in map
         */
        public static void addCell(Vector3Int cellLocation, Tunnel tunnel)
        {
            TunnelMapDict[cellLocation] = tunnel;
            cellList.Add(cellLocation);
            print("add cell " + cellLocation + " belonging to " + tunnel.gameObject.name + " to map"); // should not be adding a cell
        }

        /**
         * Get the tunnel type at a particular location in map
         */
        public static Tunnel getTunnelFromDict(Vector3Int cellLocation)
        {
            if (containsCell(cellLocation))
            {
                return TunnelMapDict[cellLocation];
            }
            else
            {
                if (cellLocation == null)
                {
                    throw new Exception("Missing cell " + cellLocation);
                }
                return null;
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<CollisionManager>())
            {
                CollideEvent -= FindObjectOfType<CollisionManager>().onCollide;
            }
        }
    }

}