using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    namespace Factory
    {
        public class WormPlayerFactory : WormFactoryProperties
        {
            [SerializeField]
            private HumanWorm HumanWorm;

            private static WormPlayerFactory instance;

            private void Awake()
            {
                if (instance == null)
                {
                    instance = this;
                    DontDestroyOnLoad(instance);
                }
                else if (instance != this)
                {
                    Destroy(gameObject);
                }

                turnSpeed = 100f;
            }

            private new void OnEnable()
            {
                base.OnEnable();
            }

            public void onSpawn(string wormId)
            {
                wormGO = HumanWorm.instantiate(wormId, WormContainer, turnSpeed);
                Map.Astar wormAstar = wormGO.GetComponent<Map.Astar>();             // generate path using astar
                InitWormEvent += FindObjectOfType<AiPathFinder>().onInitPlayerWorm;
                RaiseInitWormEvent(wormGO, wormAstar, wormId);
                InitWormEvent -= FindObjectOfType<AiPathFinder>().onInitPlayerWorm;
            }

            private new void OnDisable()
            {
                base.OnDisable();
            }
        }
    }
}