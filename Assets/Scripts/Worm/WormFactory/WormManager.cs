using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormManager : GenericSingletonClass<WormManager>
    {
        public delegate void EnterExistingTunnel();
        public event EnterExistingTunnel EnterExistingTunnelEvent;

        public delegate void AddTunnel(Tunnel.Tunnel tunnel, Tunnel.CellMove cellMove, DirectionPair directionPair, string wormId);
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

        public void onRemoveWorm(string wormId)
        {
            if (!wormDictionary.ContainsKey(wormId))
            {
                throw new System.Exception("Cannot remove worm with id " + wormId + " because it doesn't exist");
            }
            wormDictionary.Remove(wormId);
        }

        /**
         * Add a worm to the dictionary of worms when it is first instantiated
         */
        public void onInitWorm(Worm worm, Map.Astar wormAstar, string wormId)
        {
            if (wormDictionary.ContainsKey(wormId))
            {
                throw new System.Exception("Cannot init worm with id " + wormId + " because it already exists");
            }
            wormDictionary.Add(wormId, worm.gameObject);
        }

        /**
         * Receive event indicating that a tunnel has been created
         * 
         * @wormId      the id of the worm responsible for the collision
         */
        public void onAddTunnel(Tunnel.Tunnel tunnel, Tunnel.CellMove cellMove, DirectionPair directionPair, string wormId)
        {
            Vector3Int cell = cellMove.isInit ? cellMove.cell : cellMove.lastCellPosition; // if it is the first cell
            if (!Map.SpawnGenerator.WormObstacleDict.ContainsKey(cell))
            {
                throw new System.Exception("Tunnel not created at cell " + cell);
            }
            if (Map.SpawnGenerator.WormObstacleDict[cell] == null)
            {
                throw new System.Exception("Worm " + wormId + " does not exist in cell " + cell);
            }
            Obstacle WormObstacle = Map.SpawnGenerator.WormObstacleDict[cell];
            GameObject wormGO = WormObstacle.obstacleObject;
            if (wormGO == null)
            {
                throw new System.Exception("Failed to create junction. Worm " + wormId + " could not be found in the list of worms");
            }
            WormTunnelBroker wormTunnelBroker = wormGO.GetComponent<WormTunnelBroker>();
            InputProcessor wormInputProcessor = wormGO.GetComponent<InputProcessor>();

            AddTunnelEvent += wormInputProcessor.onAddTunnel;
            AddTunnelEvent += wormTunnelBroker.onAddTunnel;
            AddTunnelEvent(tunnel, cellMove, directionPair, wormId);
            AddTunnelEvent -= wormTunnelBroker.onAddTunnel;
            AddTunnelEvent -= wormInputProcessor.onAddTunnel;

        }
    }
}
