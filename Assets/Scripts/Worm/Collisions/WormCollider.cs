using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormCollider : MonoBehaviour
    {
        public delegate void WormCollide(string objectTag);
        public event WormCollide WormCollideEvent;

        private void OnEnable()
        {
            WormCollideEvent += FindObjectOfType<WormColliderManager>().onWormCollide;
        }

        private void OnCollisionEnter(Collision collision)
        {
            WormCollideEvent(collision.transform.tag);
        }

        private void OnDisable()
        {
            if (FindObjectOfType<WormColliderManager>())
            {
                WormCollideEvent -= FindObjectOfType<WormColliderManager>().onWormCollide;
            }
        }
    }
}