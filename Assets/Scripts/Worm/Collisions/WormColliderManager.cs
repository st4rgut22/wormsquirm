using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormColliderManager : WormBody
    {
        public delegate void ConsumeReward(Equipment.Block block, string wormId);
        public event ConsumeReward ConsumeRewardEvent;

        [SerializeField]
        private Transform BoneParent;

        [SerializeField]
        private GameObject WormSegment;

        [SerializeField]
        private GameObject WormMidSegment;

        const float explosionRadius = 50.0f;
        const float explosionPower = .01f;

        const string WALL_TAG = "wall";
        const string ROCK_TAG = "rock";
        const string REWARD_TAG = "reward";

        private new void OnEnable()
        {
            base.OnEnable();
            ConsumeRewardEvent += GetComponent<WormInventory>().onAddToInventory;
            ConsumeRewardEvent += FindObjectOfType<Map.SpawnGenerator>().onConsumeObstacle;
        }

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

        public void onWormCollide(GameObject otherGO)
        {
            string collidedWithTag = otherGO.tag;
            print("Collided with " + collidedWithTag);
            if (collidedWithTag == WALL_TAG || collidedWithTag == ROCK_TAG)
            {
                die();
            }
            else if (collidedWithTag == REWARD_TAG)
            {
                ConsumeRewardEvent(otherGO.GetComponent<Equipment.Block>(), wormBase.wormId);
            }
        }

        private new void OnDisable()
        {
            base.OnDisable();
            if (GetComponent<WormInventory>())
            {
                ConsumeRewardEvent -= GetComponent<WormInventory>().onAddToInventory;
                ConsumeRewardEvent -= FindObjectOfType<Map.SpawnGenerator>().onConsumeObstacle;
            }
        }
    }

}