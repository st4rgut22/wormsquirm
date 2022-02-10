using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormBody : MonoBehaviour
    {
        [SerializeField]
        protected Rigidbody ring;

        [SerializeField]
        protected Rigidbody head;

        [SerializeField]
        protected Rigidbody clit;

        public string wormId;

        protected WormBase wormBase; // stores shared variables across worm prefab classes

        public static float WORM_BODY_THICKNESS = 0.1f;

        public static Vector3Int initialCell = Vector3Int.zero;

        public float turnSpeed;

        public delegate void ChangeDirection(DirectionPair directionPair, string wormId);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void RemoveSelf(string wormId, GameObject wormGO);
        public event RemoveSelf RemoveSelfEvent;

        public delegate void ObjectiveReached();
        public event ObjectiveReached ObjectiveReachedEvent;

        public delegate void Spawn(string wormId);
        public event Spawn SpawnEvent;

        protected void Awake()
        {
            wormBase = GetComponent<WormBase>();
        }

        /**
         * Worm has reached a goal indicating the game is over
         */
        protected void RaiseObjectiveReachedEvent()
        {
            ObjectiveReachedEvent += GameManager.Instance.onObjectiveReached;
            ObjectiveReachedEvent();
            ObjectiveReachedEvent -= GameManager.Instance.onObjectiveReached;
        }

        /**
         * Tells the worm to destroy itself
         */
        protected void RaiseRemoveSelfEvent()
        {
            RemoveSelfEvent += WormManager.Instance.onRemoveWorm;
            RemoveSelfEvent(wormId, gameObject);
            RemoveSelfEvent -= WormManager.Instance.onRemoveWorm;
        }

        /**
         * When a worm is destroy it, issue a respawn event
         */
        protected void RaiseSpawnEvent()
        {
            SpawnEvent += FindObjectOfType<Factory.WormFactory>().onCreateWorm;
            SpawnEvent(gameObject.tag);
            SpawnEvent -= FindObjectOfType<Factory.WormFactory>().onCreateWorm;
        }

        protected void RaiseChangeDirectionEvent(DirectionPair directionPair, string wormId)
        {
            ChangeDirectionEvent += Tunnel.CollisionManager.Instance.onChangeDirection;
            ChangeDirectionEvent(directionPair, wormId);
            ChangeDirectionEvent -= Tunnel.CollisionManager.Instance.onChangeDirection;
        }
    }
}
