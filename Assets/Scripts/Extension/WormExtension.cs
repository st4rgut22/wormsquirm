using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public static class WormExtension
    {
        public static GameObject instantiate(this GameObject thisObj, string wormId, Transform wormContainer, float turnSpeed)
        {
            GameObject wormGO = GameObject.Instantiate(thisObj, wormContainer);
            WormBody[] wormBodies = wormGO.GetComponents<WormBody>();

            // initialize worm properties for all components that use them
            for (int i=0;i<wormBodies.Length;i++)
            {
                WormBody wormBody = wormBodies[i];
                wormBody.wormId = wormId;
                wormBody.turnSpeed = turnSpeed;
            }
            wormBodies[0].RaiseSaveWormEvent(); // save the worm id to the worm dictionary
            return wormGO;
        }
    }

}