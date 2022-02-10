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
            private GameObject WormPlayer;

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

            public override void onSpawn(string wormId)
            {
                wormGO = WormPlayer.instantiate(wormId, WormContainer, turnSpeed);
            }
        }
    }
}