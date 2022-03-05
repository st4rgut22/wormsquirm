using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    // This class provides methods for PlayerWormExtension and AiWormExtension
    public static class WormExtension
    {
        /**
         * Initialize a AI or Player worm
         * 
         * @prefabGO        the prefab for either an AI or Player
         */
        private static GameObject initializeWorm(GameObject prefabGO, string wormId, Transform wormContainer, float turnSpeed)
        {
            GameObject wormGO = GameObject.Instantiate(prefabGO, wormContainer);

            WormBody[] wormBodies = wormGO.GetComponents<WormBody>();

            // initialize worm properties for all components that use them
            for (int i = 0; i < wormBodies.Length; i++)
            {
                WormBody wormBody = wormBodies[i];
                wormBody.wormId = wormId;
                wormBody.turnSpeed = turnSpeed;
            }
            return wormGO;
        }

        /**
         * Instantiate an AI worm
         * 
         * @thisObj     the input processor component of the ai worm
         */
        public static GameObject instantiate(this AIWorm thisObj, string wormId, Transform wormContainer, float turnSpeed)
        {
            GameObject aiWormGO = initializeWorm(thisObj.gameObject, wormId, wormContainer, turnSpeed);
            return aiWormGO;
        }

        /**
         * Instantiate a player worm
         * 
         * @thisObj     the inputu processor component of the player worm
         */
        public static GameObject instantiate(this HumanWorm thisObj, string wormId, Transform wormContainer, float turnSpeed)
        {
            GameObject humanWormGO = initializeWorm(thisObj.gameObject, wormId, wormContainer, turnSpeed);
            return humanWormGO;
        }
    }
}