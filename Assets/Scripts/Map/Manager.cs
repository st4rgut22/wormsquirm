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

        private void Awake()
        {
            TunnelMapDict = new Dictionary<Vector3Int, Tunnel.Tunnel>();
        }

        private void OnEnable()
        {
            SliceEvent += FindObjectOfType<Intersect.Manager>().onSlice;
        }

        /**
         * Adds tunnel type to the map at the provided coordinate
         */
        public void onBlockInterval(bool isBlockInterval, Vector3 blockPosition, Tunnel.Straight tunnel)
        {
            if (isBlockInterval)
            {
                Vector3Int blockPositionInt = blockPosition.castToVector3Int(tunnel.growthDirection);
                print("block position is " + blockPosition + " blockPosInt is " + blockPositionInt + " for tunnel " + tunnel.name);
                if (containsCell(blockPositionInt))
                {
                    Tunnel.Tunnel intersectTunnel = TunnelMapDict[blockPositionInt];
                    print("debug onSlice " + blockPositionInt);
                    DirectionPair dirPair = new DirectionPair(tunnel.growthDirection, tunnel.growthDirection);
                    SliceEvent(dirPair, tunnel, intersectTunnel); // send event when tunnels intersect
                }
                else
                {
                    addCellToDict(blockPositionInt, tunnel);
                }
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
            addCellToDict(cell, tunnel);
        }

        public static bool containsCell(Vector3Int cell)
        {
            return TunnelMapDict.ContainsKey(cell);
        }

        /**
         * Save tunnel type at a particular location in map
         */
        public static void addCellToDict(Vector3Int cellLocation, Tunnel.Tunnel tunnel)
        {
            print("debug add cell " + cellLocation + " belonging to tunnel " + tunnel.name);
            TunnelMapDict[cellLocation] = tunnel;
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