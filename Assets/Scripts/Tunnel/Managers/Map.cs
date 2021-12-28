using System.Collections.Generic;
using System;
using UnityEngine;

namespace Tunnel
{
    public class Map : CollisionManager
    {

        public static Dictionary<Vector3Int, Tunnel> TunnelMapDict;
        public static List<Vector3Int> cellList;

        private bool isDecision;
        private bool isInit = true; // the first tunnel is created

        private void Awake()
        {
            isDecision = false;
            cellList = new List<Vector3Int>() { Manager.initialCell };
            TunnelMapDict = new Dictionary<Vector3Int, Tunnel>();
        }

        public void onDecision(bool isStraightTunnel, Direction direction, Tunnel tunnel)
        {
            if (isStraightTunnel)
            {
                isDecision = true;
            }
            if (tunnel != null)
            {
                isInit = false;
            }
        }

        /**
         * Adds tunnel type to the map at the provided coordinate
         */
        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Straight tunnel)
        {
            if (isBlockInterval)
            {
                if (containsCell(blockPositionInt))
                {
                    Tunnel intersectTunnel = TunnelMapDict[blockPositionInt];
                    DirectionPair dirPair = new DirectionPair(tunnel.growthDirection, tunnel.growthDirection); // go straight
                    collide(dirPair, tunnel, intersectTunnel); // send event when tunnels intersect
                }
                // or is initial straight tunnel, where decision event is created even though we are not technically turning
                else if (!isDecision || isInit) // in straight tunnel a turn decision has been made so don't add the next tunnel segment
                {
                    addCell(blockPositionInt, tunnel);
                }

                isDecision = false; // reset flag in order to add new cells until a decision is made
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

        public void onAddTunnel(Tunnel tunnel, Vector3Int cell, DirectionPair directionPair)
        {
            addCell(cell, tunnel);
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
                return null;
            }
        }
    }

}