using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    namespace Factory
    {
        public abstract class WormFactoryProperties : MonoBehaviour
        {
            // tracks the worms in the scene, and cleans up event handlers when the worms are destroyed

            [SerializeField]
            protected Transform WormContainer;

            protected GameObject wormGO;

            public abstract void onSpawn(string wormId);

            protected string wormTag;

            protected float turnSpeed;

        }
    }

}
