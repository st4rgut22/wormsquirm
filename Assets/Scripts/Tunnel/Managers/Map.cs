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

        private void Awake()
        {
            cellList = new List<Vector3Int>() { TunnelManager.Instance.initialCell };
            TunnelMapDict = new Dictionary<Vector3Int, Tunnel>();
        }

        private void OnEnable()
        {
            if (FindObjectOfType<CollisionManager>())
            {
                CollideEvent += FindObjectOfType<CollisionManager>().onCollide;
            }            
        }

        /**
         * Store the first cell added
         */
        public void onStartMove(Direction direction)
        {
            startingCell = Dir.Vector.getNextCellFromDirection(TunnelManager.Instance.initialCell, direction);
        }

        /**
         * Adds tunnel type to the map at the provided coordinate
         */
        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Straight tunnel)
        {
            if (isBlockInterval)
            {
                // before traversing cell check that the next cell is empty to decide whether to intersect it
                Vector3Int nextBlockPositionInt = blockPositionInt.getNextVector3Int(tunnel.growthDirection);
                if (containsCell(nextBlockPositionInt))
                {
                    Tunnel intersectTunnel = TunnelMapDict[nextBlockPositionInt];
                    DirectionPair dirPair = new DirectionPair(tunnel.growthDirection, tunnel.growthDirection); // go straight
                    CollideEvent(dirPair, tunnel, intersectTunnel); // send event when tunnels intersect
                }
                // or is initial straight tunnel, where decision event is created even though we are not technically turning
                addCell(blockPositionInt, tunnel);
            }
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

        public void onAddTunnel(Tunnel tunnel, Vector3Int cellLocation, DirectionPair directionPair)
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
            print("debug add cell " + cellLocation + " belonging to tunnel " + tunnel.name);
            TunnelMapDict[cellLocation] = tunnel;
            cellList.Add(cellLocation);
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