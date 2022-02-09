using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    namespace Factory
    {
        public class WormFactory : MonoBehaviour
        {
            public delegate void SpawnAIWorm(string wormId);
            public event SpawnAIWorm SpawnAIWormEvent;

            public delegate void SpawnPlayerWorm(string wormId);
            public event SpawnPlayerWorm SpawnPlayerWormEvent;

            private int wormCount;

            private static WormFactory instance;

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

                wormCount = 0;
            }

            private void OnEnable()
            {
                SpawnAIWormEvent += FindObjectOfType<WormAIFactory>().onSpawn;
                SpawnPlayerWormEvent += FindObjectOfType<WormPlayerFactory>().onSpawn;
            }

            private string generateWormId()
            {
                string name = "worm" + wormCount.ToString();
                return name;
            }

            public void onCreateWorm(string wormTag)
            {
                string name = generateWormId();

                if (wormTag == WormManager.WORM_AI_TAG)
                {
                    SpawnAIWormEvent(name);
                }
                else if (wormTag == WormManager.WORM_PLAYER_TAG)
                {
                    SpawnPlayerWormEvent(name);
                }
            }

            private void OnDisable()
            {
                if (FindObjectOfType<WormAIFactory>())
                {
                    SpawnAIWormEvent -= FindObjectOfType<WormAIFactory>().onSpawn;
                }
                if (FindObjectOfType<WormPlayerFactory>())
                {
                    SpawnPlayerWormEvent -= FindObjectOfType<WormPlayerFactory>().onSpawn;
                }
            }
        }
    }
}