using System.Collections.Generic;
using System;
using UnityEngine;

namespace Map
{
    public class Manager : MonoBehaviour
    {
        public delegate void Slice(DirectionPair exitDirectionPair, Tunnel.Straight curTunnel, Tunnel.Tunnel nextTunnel);
        private event Slice SliceEvent;

        public static Dictionary<Vector3Int, Tunnel.Tunnel> TunnelMapDict;
        public static List<Vector3Int> cellList;

        private bool isDecision;
        private bool isInit = true; // the first tunnel is created

        private void Awake()
        {
            isDecision = false;
            cellList = new List<Vector3Int>() { Tunnel.Manager.initialCell };
            TunnelMapDict = new Dictionary<Vector3Int, Tunnel.Tunnel>();
        }

        private void OnEnable()
        {
            SliceEvent += FindObjectOfType<Intersect.Manager>().onSlice;
        }

        public void onDecision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel)
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
        public void onBlockInterval(bool isBlockInterval, Vector3Int blockPositionInt, Tunnel.Straight tunnel)
        {
            if (isBlockInterval)
            {
                if (containsCell(blockPositionInt))
                {
                    Tunnel.Tunnel intersectTunnel = TunnelMapDict[blockPositionInt];
                    DirectionPair dirPair = new DirectionPair(tunnel.growthDirection, tunnel.growthDirection);
                    SliceEvent(dirPair, tunnel, intersectTunnel); // send event when tunnels intersect
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

        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair)
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
        public static void addCell(Vector3Int cellLocation, Tunnel.Tunnel tunnel)
        {
            print("debug add cell " + cellLocation + " belonging to tunnel " + tunnel.name);
            TunnelMapDict[cellLocation] = tunnel;
            cellList.Add(cellLocation);
        }

        /**
         * Get the tunnel type at a particular location in map
         */
        public static Tunnel.Tunnel getTunnelFromDict(Vector3Int cellLocation)
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

        private void OnDisable()
        {
            if (FindObjectOfType<Intersect.Manager>())
            {
                SliceEvent -= FindObjectOfType<Intersect.Manager>().onSlice;
            }
        }
    }

}