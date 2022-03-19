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

        public Dictionary<string, GameObject> WormDictionary; // <wormId, worm GO>
        public Dictionary<string, WormTunnelBroker> WormTunnelBrokerDict; // <wormId, WormTunnelBroker>
        public Dictionary<string, TunnelMaker> WormTunnelMakerDict; // <wormId, WormTunnelMaker> only stores AI references

        public List<string> WormIdList; // List<wormId>
        private string playerWormId;

        private new void Awake()
        {
            base.Awake();
            WormDictionary = new Dictionary<string, GameObject>();
            WormTunnelBrokerDict = new Dictionary<string, WormTunnelBroker>();
            WormTunnelMakerDict = new Dictionary<string, TunnelMaker>();
            WormIdList = new List<string>();
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

        public string getPlayerWormId()
        {
            return playerWormId;
        }

        public void onRemoveWorm(string wormId)
        {
            if (!WormDictionary.ContainsKey(wormId))
            {
                throw new System.Exception("Cannot remove worm with id " + wormId + " because it doesn't exist");
            }
            WormDictionary.Remove(wormId);
            WormTunnelBrokerDict.Remove(wormId);
            WormTunnelMakerDict.Remove(wormId);
        }

        /**
         * Add a worm to the dictionary of worms when it is first instantiated
         */
        public void onInitWorm(Worm worm, Map.Astar wormAstar, string wormId)
        {
            if (WormDictionary.ContainsKey(wormId))
            {
                throw new System.Exception("Cannot init worm with id " + wormId + " because it already exists");
            }
            if (worm.wormType == ObstacleType.PlayerWorm)
            {
                playerWormId = wormId;
            }

            WormIdList.Add(wormId);

            WormTunnelBroker wormTunnelBroker = worm.gameObject.GetComponent<WormTunnelBroker>();
            WormDictionary.Add(wormId, worm.gameObject);
            WormTunnelBrokerDict.Add(wormId, wormTunnelBroker);

            if (worm.wormType == ObstacleType.AIWorm)
            {
                TunnelMaker tunnelMaker = worm.gameObject.GetComponent<TunnelMaker>();
                WormTunnelMakerDict.Add(wormId, tunnelMaker);
            }
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
