using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormManager : GenericSingletonClass<WormManager>
    {
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

        public void onSave(string wormId, GameObject wormGO)
        {
            wormDictionary[wormId] = wormGO;
        }
    }

}
