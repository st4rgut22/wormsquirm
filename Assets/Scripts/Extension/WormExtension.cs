using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public static class WormExtension
    {
        public static GameObject instantiate(this GameObject thisObj, string wormId, Transform wormContainer)
        {
            GameObject wormGO = GameObject.Instantiate(thisObj, wormContainer);
            WormBody[] wormBodies = wormGO.GetComponents<WormBody>();

            // initialize worm id for all components that use it
            for (int i=0;i<wormBodies.Length;i++)
            {
                WormBody wormBody = wormBodies[i];
                wormBody.wormId = wormId;
            }

            return wormGO;
        }
    }

}