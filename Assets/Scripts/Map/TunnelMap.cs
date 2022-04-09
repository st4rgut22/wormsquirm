using System.Collections.Generic;
using System;
using UnityEngine;

namespace Tunnel
{
    public class TunnelMap : MonoBehaviour
    {
        public delegate void CollideTunnel(DirectionPair directionPair, Tunnel curTunnel, Tunnel nextTunnel, Vector3Int collisionCell, bool isTunnelNew);
        private event CollideTunnel CollideTunnelEvent;

        public static Dictionary<Vector3Int, Tunnel> TunnelMapDict;
        public static List<Vector3Int> cellList;

        public static Vector3Int startingCell; // the cell location of the first tunnel

        private const float XZ_INTERVAL_OFFSET = .5f;

        private void Awake()
        {
            cellList = new List<Vector3Int>(); // { TunnelManager.Instance.initialCell };
            TunnelMapDict = new Dictionary<Vector3Int, Tunnel>();
        }

        private void OnEnable()
        {
            CollideTunnelEvent += CollisionManager.Instance.onCollide;
        }

        /**
         * Store the first cell added
         */
        public void onStartMove(Direction direction)
        {
            startingCell = Dir.Vector.getNextCellFromDirection(TunnelManager.Instance.initialCell, direction);
        }

        /**
         * When tunnel network is reset clear the map
         */
        public void onDestroyGame()
        {
            TunnelMapDict.Clear();
            cellList.Clear();
        }

        /**
         * Return whether the tunnels are the same in different cells
         */
        public static bool isTunnelSame(Vector3Int cell1, Vector3Int cell2)
        {
            return TunnelMapDict[cell1] == TunnelMapDict[cell2];
        }

        /**
         * Adds tunnel type to the map at the provided coordinate
         * 
         * @isCellSameTunnel        whether the block interval marks the beginning of a new tunnel segment (eg corner) or is a continuation of existing tunnel
         */
        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Vector3Int lastBlockPositionInt, Straight tunnel, bool isTunnelSame, bool isTurn, bool isCollide)
        {
            if (isBlockInterval)
            {
                // before traversing cell check that the next cell is empty to decide whether to intersect it
                if (!isTurn && containsCell(blockPositionInt)) // exclude turn tunnel segments from part of the straight tunnel
                {
                    print("collision occurred with straight tunnel at " + blockPositionInt);
                    Tunnel intersectTunnel = TunnelMapDict[blockPositionInt];
                    DirectionPair dirPair = new DirectionPair(tunnel.growthDirection, tunnel.growthDirection); // go straight
                    CollideTunnelEvent(dirPair, tunnel, intersectTunnel, blockPositionInt, true); // send event when tunnels intersect AND not turning
                }
                else if (isTunnelSame)
                {
                    addCell(blockPositionInt, tunnel);
                }
            }
        }

        /**
          * Use worm's rounded position as the cell position. Add an offset on the XZ directions to get the map cell positions
          * 
          * @position clit position
          */
        public static Vector3Int getCellPos(Vector3 position)
        {
            //print("clit position before " + position);
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
            position.y = Mathf.FloorToInt(position.y); // round down y to the closest integer value to ensure consistency between positive and negative positions
            Vector3Int cellPos = position.castToVector3Int();
            return cellPos;
        }

        /**
         * Get the current tunnel worm is in using its rigidbody position
         */
        public static Tunnel getCurrentTunnel(Vector3 position)
        {
            Vector3Int cellPos = getCellPos(position);
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

        /**
         * 
         */
        public void onAddTunnel(Tunnel tunnel, CellMove cellMove, DirectionPair directionPair, string wormId)
        {
            if (cellMove != null) // if null, it is a slice tunnel event and the t unnel type remains the same
            {
                Vector3Int cell = cellMove.cell;
                addCell(cell, tunnel);
            }
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
                CollideTunnelEvent -= FindObjectOfType<CollisionManager>().onCollide;
            }
        }
    }

}