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
                
        protected WormDir wormDir; // stores shared variables across worm prefab classes

        protected string wormId = "fakeId"; // TEMPORARY, later assign ids from worm manager

        public delegate void ChangeDirection(DirectionPair directionPair, string wormId);
        public event ChangeDirection ChangeDirectionEvent;

        protected void OnEnable()
        {
        }

        protected void Awake()
        {
            wormDir = GetComponent<WormDir>();
        }

        protected void RaiseChangeDirectionEvent(DirectionPair directionPair, string wormId)
        {
            if (ChangeDirectionEvent == null)
            {
                ChangeDirectionEvent += Tunnel.CollisionManager.Instance.onChangeDirection;
            }            
            ChangeDirectionEvent(directionPair, wormId);
        }

        protected void OnDisable()
        {
            if (Tunnel.CollisionManager.Instance && ChangeDirectionEvent != null)
            {
                ChangeDirectionEvent -= Tunnel.CollisionManager.Instance.onChangeDirection;
            }
        }
    }
}
