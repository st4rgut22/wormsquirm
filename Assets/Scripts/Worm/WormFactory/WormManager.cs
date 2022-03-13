using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormManager : GenericSingletonClass<WormManager>
    {
        public delegate void EnterExistingTunnel();
        public event EnterExistingTunnel EnterExistingTunnelEvent;

        public delegate void AddTunnel(Tunnel.Tunnel tunnel, DirectionPair directionPair);
        public event AddTunnel AddTunnelEvent;

        public Dictionary<string, GameObject> wormDictionary; // <wormId, worm GO>

        private new void Awake()
        {
            base.Awake();
            wormDictionary = new Dictionary<string, GameObject>();
        }

        /**
         * Signals the tunnel worm is in has collided. Relay the enter existing tunnel message to the affected worm.
         * 
         * @wormId      the id of the worm whose tunnel has collided, thus entering an existing tunnel
         */
        public void onCollide(Vector3Int cell)
        {
            Obstacle WormObstacle = Map.SpawnGenerator.WormObstacleDict[cell];
            GameObject wormGO = WormObstacle.obstacleObject;

            WormTunnelBroker wormTunnelBroker = wormGO.GetComponent<WormTunnelBroker>(); 

            EnterExistingTunnelEvent += wormTunnelBroker.onEnterExistingTunnel;
            EnterExistingTunnelEvent();
            EnterExistingTunnelEvent -= wormTunnelBroker.onEnterExistingTunnel;
        }

        /**
         * Receive event indicating that a tunnel has been created
         * 
         * @wormId      the id of the worm responsible for the collision
         */
        public void onAddTunnel(Tunnel.Tunnel tunnel, Tunnel.CellMove cellMove, DirectionPair directionPair, string wormId)
        {
            Vector3Int cell = cellMove.isInit ? cellMove.cell : cellMove.lastCellPosition; // if it is the first cell 
            Obstacle WormObstacle = Map.SpawnGenerator.WormObstacleDict[cell];
            GameObject wormGO = WormObstacle.obstacleObject;
            if (wormGO == null)
            {
                throw new System.Exception("Failed to create junction. Worm " + wormId + " could not be found in the list of worms");
            }
            WormTunnelBroker wormTunnelBroker = wormGO.GetComponent<WormTunnelBroker>();

            AddTunnelEvent += wormTunnelBroker.onAddTunnel;
            AddTunnelEvent(tunnel, directionPair);
            AddTunnelEvent -= wormTunnelBroker.onAddTunnel;
        }
    }
}
