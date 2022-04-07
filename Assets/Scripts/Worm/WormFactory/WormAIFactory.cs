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

            public delegate void InitCheckpoint(List<Checkpoint> checkpointList, bool isInitPath, WormTunnelBroker wormTunnelBroker);
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
             * @chaseWormId the id of the worm being chased
             */
            private void initializeWormPath(Map.Astar wormAstar, GameObject wormGO, string chaseWormId)
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
                    case GameMode.DebugFixedPath:
                        if (tunnelMaker.wormId == chaseWormId) // only the worm being chase should follow fixed path, other worms follow auto path.
                        {
                            followExamplePath(tunnelMaker, wormTunnelBroker);
                        }
                        break;
                }
            }

            private void followExamplePath(TunnelMaker tunnelMaker, WormTunnelBroker wormTunnelBroker)
            {
                List<Checkpoint> examplePath = FindObjectOfType<Test.ExampleNetwork>().getNetwork();

                InitCheckpointEvent += tunnelMaker.onInitCheckpointList;
                InitCheckpointEvent(examplePath, false, wormTunnelBroker); // isInitPath = true because always starting over and re-initializing the worm after reaching the goal                                                                        
                InitCheckpointEvent -= tunnelMaker.onInitCheckpointList;
            }

            public override void onSpawn(string wormId, string chaseWormId)
            {
                wormGO = AiWorm.instantiate(wormId, WormContainer, turnSpeed);
                Map.Astar wormAstar = wormGO.GetComponent<Map.Astar>(); // generate path using astar
                RaiseInitWormEvent(wormGO, wormAstar, wormId, chaseWormId); //  add worm to the map

                initializeWormPath(wormAstar, wormGO, chaseWormId);
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