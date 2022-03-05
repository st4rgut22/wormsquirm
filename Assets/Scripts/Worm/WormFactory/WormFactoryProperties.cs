using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    namespace Factory
    {
        // tracks the worms in the scene, and cleans up event handlers when the worms are destroyed
        public abstract class WormFactoryProperties : MonoBehaviour
        {
            public delegate void InitWorm(Worm worm, Map.Astar wormAstar, string wormId);
            public event InitWorm InitWormEvent;

            [SerializeField]
            protected Transform WormContainer;

            protected GameObject wormGO;

            protected string wormTag;

            protected float turnSpeed;

            protected void OnEnable()
            {
                InitWormEvent += FindObjectOfType<Map.SpawnGenerator>().onInitWorm;
            }

            protected void RaiseInitWormEvent(GameObject wormGO, Map.Astar wormAstar, string wormId)
            {
                Worm worm = wormGO.GetComponent<Worm>();
                InitWormEvent(worm, wormAstar, wormId);
            }

            protected void OnDisable()
            {
                if (GetComponent<Map.SpawnGenerator>())
                {
                    InitWormEvent -= FindObjectOfType<Map.SpawnGenerator>().onInitWorm;
                }
            }
        }
    }

}
