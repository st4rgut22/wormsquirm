using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormManager : GenericSingletonClass<WormManager>
    {
        public delegate void AddTunnel(Tunnel.Tunnel tunnel, DirectionPair directionPair);
        public event AddTunnel AddTunnelEvent;

        public Dictionary<string, GameObject> wormDictionary; // <wormId, worm GO>

        public static string WORM_AI_TAG = "AI";
        public static string WORM_PLAYER_TAG = "Human";

        private new void Awake()
        {
            base.Awake();
            wormDictionary = new Dictionary<string, GameObject>();
        }

        public void onRemoveWorm(string wormId, GameObject wormGO)
        {
            if (wormDictionary.ContainsKey(wormId))
            {
                Destroy(wormDictionary[wormId]);
            }
            else
            {
                throw new System.Exception("Trying to remove wormId " + wormId + " that does not exist");
            }
        }

        /**
         * Receive event indicating that a tunnel has been created
         * 
         * @wormId      the id of the worm responsible for the collision
         */
        public void onAddTunnel(Tunnel.Tunnel tunnel, Vector3Int cell, DirectionPair directionPair, string wormId)
        {
            GameObject wormGO = wormDictionary[wormId];
            if (wormGO == null)
            {
                throw new System.Exception("Failed to create junction. Worm " + wormId + " could not be found in the list of worms");
            }
            WormTunnelBroker wormTunnelBroker = wormGO.GetComponent<WormTunnelBroker>();

            AddTunnelEvent += wormTunnelBroker.onAddTunnel;
            AddTunnelEvent(tunnel, directionPair);
            AddTunnelEvent -= wormTunnelBroker.onAddTunnel;
        }

        public void onSave(string wormId, GameObject wormGO)
        {
            wormDictionary[wormId] = wormGO;
        }
    }

}
