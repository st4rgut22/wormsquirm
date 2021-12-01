using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class Manager : MonoBehaviour
    {
        public static Dictionary<Vector3Int, Tunnel.Tunnel> TunnelMapDict;

        private void Awake()
        {
             TunnelMapDict = new Dictionary<Vector3Int, Tunnel.Tunnel>();
        }


        /**
         * Adds tunnel type to the map at the provided coordinate
         */
        public void onBlockInterval(bool isBlockInterval, Vector3 blockPosition, Tunnel.Tunnel tunnel)
        {
            Vector3Int blockPositionInt = blockPosition.castToVector3Int();
            addCellToDict(blockPositionInt, tunnel);
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
            TunnelMapDict[cellLocation] = tunnel;
        }

        /**
         * Get the tunnel type at a particular location in map
         */
        public static Tunnel.Tunnel getTunnelFromDict(Vector3Int cellLocation)
        {
            return TunnelMapDict[cellLocation];
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}