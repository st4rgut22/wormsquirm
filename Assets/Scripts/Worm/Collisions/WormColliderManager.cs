using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormColliderManager : WormBody
    {
        [SerializeField]
        private Transform BoneParent;

        [SerializeField]
        private GameObject WormSegment;

        [SerializeField]
        private GameObject WormMidSegment;

        const float explosionRadius = 50.0f;
        const float explosionPower = .01f;

        private List<Rigidbody> spawnWormSegments()
        {
            List<Rigidbody> WormSegmentList = new List<Rigidbody>();
            while (BoneParent.childCount > 0)
            {
                GameObject wormSegmentGO = Instantiate(WormSegment, BoneParent.position, BoneParent.rotation);
                BoneParent = BoneParent.GetChild(0);
                print("bone parent is " + BoneParent.name);
                Rigidbody wormSegmentRigidbody = wormSegmentGO.GetComponent<Rigidbody>();
                WormSegmentList.Add(wormSegmentRigidbody);
            }
            return WormSegmentList;
        }

        /**
         * Explode body parts everywhere
         */
        private void die()
        {
            Vector3 explosionPosition = WormMidSegment.transform.position;

            List<Rigidbody> wormSegmentList = spawnWormSegments();
            wormSegmentList.ForEach((Rigidbody wormSegment) =>
            {
                wormSegment.AddExplosionForce(explosionPower, explosionPosition, explosionRadius);
            });
            RaiseRemoveSelfEvent();
        }

        public void onWormCollide(string collidedWithTag)
        {
            print("Collided with " + collidedWithTag);
            if (collidedWithTag == "wall" || collidedWithTag == "rock")
            {
                die();
            }
        }
    }

}