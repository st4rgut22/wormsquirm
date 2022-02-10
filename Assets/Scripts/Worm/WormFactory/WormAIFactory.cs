using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    namespace Factory
    {
        // setup a AI worm
        public class WormAIFactory : WormFactoryProperties
        {
            [SerializeField]
            private GameObject WormAI;

            public delegate void FollowPath(TunnelMaker tunnelMaker);
            public event FollowPath FollowPathEvent;

            public delegate void InitCheckpoint(List<Checkpoint> checkpointList);
            public event InitCheckpoint InitCheckpointEvent;

            private static WormAIFactory instance;

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

                turnSpeed = 1000f; // instantaneous turn
            }

            private void OnEnable()
            {
                FollowPathEvent += FindObjectOfType<Map.Astar>().onFollowPath;
            }

            private void initializeWormPath(GameObject wormGO)
            {
                TunnelMaker tunnelMaker = wormGO.GetComponent<TunnelMaker>();
                switch (GameManager.Instance.gameMode)
                {
                    case GameMode.TestAutoPath:
                        FollowPathEvent(tunnelMaker);
                        break;
                    case GameMode.TestFixedPath:
                        followExamplePath(tunnelMaker);
                        break;
                    default:
                        throw new System.Exception("Game mode is invalid for AI worm " + GameManager.Instance.gameMode);
                }
            }

            private void followExamplePath(TunnelMaker tunnelMaker)
            {
                List<Checkpoint> examplePath = FindObjectOfType<Test.ExampleNetwork>().getNetwork();

                InitCheckpointEvent += tunnelMaker.onInitCheckpointList;
                InitCheckpointEvent(examplePath);
                InitCheckpointEvent -= tunnelMaker.onInitCheckpointList;
            }

            public override void onSpawn(string wormId)
            {
                wormGO = WormAI.instantiate(wormId, WormContainer, turnSpeed);
                initializeWormPath(wormGO);
            }

            private void OnDisable()
            {
                if (FindObjectOfType<Map.Astar>())
                {
                    FollowPathEvent -= FindObjectOfType<Map.Astar>().onFollowPath;
                }
            }
        }
    }
}