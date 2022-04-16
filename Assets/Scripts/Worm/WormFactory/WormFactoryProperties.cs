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

            abstract public void onSpawn(string wormId, string chaseWormId);

            protected GameObject wormGO;

            protected string wormTag;

            protected float turnSpeed;

            protected void OnEnable()
            {
                InitWormEvent += FindObjectOfType<Map.SpawnGenerator>().onInitWorm;
                InitWormEvent += WormManager.Instance.onInitWorm;
            }

            protected void RaiseInitWormEvent(GameObject wormGO, Map.Astar wormAstar, string wormId, string chaseWormId)
            {
                if (wormId == chaseWormId)
                {
                    InitWormEvent += FindObjectOfType<AiPathFinder>().onInitChaseWorm; // instruct ai's path finder to chas ethis worm
                }

                Worm worm = wormGO.GetComponent<Worm>();
                InitWormEvent(worm, wormAstar, wormId);

                if (wormId == chaseWormId)
                {
                    InitWormEvent -= FindObjectOfType<AiPathFinder>().onInitChaseWorm; // instruct ai's path finder to chas ethis worm
                }
            }

            protected void OnDisable()
            {
                if (FindObjectOfType<Map.SpawnGenerator>())
                {
                    InitWormEvent -= FindObjectOfType<Map.SpawnGenerator>().onInitWorm;
                }
                if (WormManager.Instance)
                {
                    InitWormEvent -= WormManager.Instance.onInitWorm;
                }
            }
        }
    }

}
