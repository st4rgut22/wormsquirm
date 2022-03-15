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
                InitWormEvent += Map.SpawnGenerator.onInitWorm;
                InitWormEvent += WormManager.Instance.onInitWorm;
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
                    InitWormEvent -= Map.SpawnGenerator.onInitWorm;
                }
                if (WormManager.Instance)
                {
                    InitWormEvent -= WormManager.Instance.onInitWorm;
                }
            }
        }
    }

}
