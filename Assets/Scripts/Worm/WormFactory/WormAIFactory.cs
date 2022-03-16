using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    namespace Factory
    {
        // setup a AI worm
        public class WormAIFactory : WormFactoryProperties
        {
            public delegate void FollowPath(TunnelMaker tunnelMaker, WormTunnelBroker wormTunnelBroker);
            public event FollowPath FollowPathEvent;

            public delegate void InitCheckpoint(List<Checkpoint> checkpointList);
            public event InitCheckpoint InitCheckpointEvent;

            [SerializeField]
            private AIWorm AiWorm;

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

            private new void OnEnable()
            {
                base.OnEnable();
                InitWormEvent += FindObjectOfType<Map.RewardGenerator>().OnInitWorm;
            }

            /**
             * Initialize the path of the AI
             * 
             * @wormGO      the gameobject worm to initialize the path of
             * @wormAstar   the worm's path finder
             */
            private void initializeWormPath(Map.Astar wormAstar, GameObject wormGO)
            {
                TunnelMaker tunnelMaker = wormGO.GetComponent<TunnelMaker>();
                WormTunnelBroker wormTunnelBroker = wormGO.GetComponent<WormTunnelBroker>();

                switch (GameManager.Instance.gameMode)
                {
                    case GameMode.Chase:
                        FollowPathEvent += wormAstar.onFollowPath;
                        FollowPathEvent(tunnelMaker, wormTunnelBroker);
                        FollowPathEvent -= wormAstar.onFollowPath;
                        break;
                    case GameMode.TestAutoPath:
                        FollowPathEvent += wormAstar.onFollowPath;
                        FollowPathEvent(tunnelMaker, wormTunnelBroker);
                        FollowPathEvent -= wormAstar.onFollowPath;
                        break;
                    case GameMode.TestFixedPath:
                        followExamplePath(tunnelMaker);
                        break;
                }
            }

            private void followExamplePath(TunnelMaker tunnelMaker)
            {
                List<Checkpoint> examplePath = FindObjectOfType<Test.ExampleNetwork>().getNetwork();

                InitCheckpointEvent += tunnelMaker.onInitCheckpointList;
                InitCheckpointEvent(examplePath);
                InitCheckpointEvent -= tunnelMaker.onInitCheckpointList;
            }

            public void onSpawn(string wormId)
            {
                wormGO = AiWorm.instantiate(wormId, WormContainer, turnSpeed);
                Map.Astar wormAstar = wormGO.GetComponent<Map.Astar>(); // generate path using astar
                RaiseInitWormEvent(wormGO, wormAstar, wormId); //  add worm to the map

                initializeWormPath(wormAstar, wormGO);
            }

            private new void OnDisable()
            {
                base.OnDisable();
                if (FindObjectOfType<Map.RewardGenerator>())
                {
                    InitWormEvent -= FindObjectOfType<Map.RewardGenerator>().OnInitWorm;
                }
            }
        }
    }
}