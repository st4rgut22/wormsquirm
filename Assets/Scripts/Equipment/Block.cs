using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public class Block : ObstaclePrefab
    {
        public delegate void RemoveReward(Block block, string wormId);
        public event RemoveReward RemoveRewardEvent;

        public delegate void TriggerConsume(GameObject collidedGO);
        public event TriggerConsume TriggerConsumeEvent;

        public BuildingBlockType buildingBlockType;

        private void OnEnable()
        {
            RemoveRewardEvent += FindObjectOfType<Map.RewardGenerator>().onRemoveReward;
        }

        private void OnCollisionEnter(Collision collision)
        {
            string tag = collision.gameObject.tag;
            if (tag == Map.SpawnGenerator.WORM_TAG)
            {
                Worm.WormColliderManager wormColliderManager = collision.gameObject.GetComponentInParent<Worm.WormColliderManager>();

                RemoveRewardEvent(this, collision.gameObject.name); // remove from map

                TriggerConsumeEvent += wormColliderManager.onWormCollide;
                TriggerConsumeEvent(gameObject); // send event indicating it collided with this block
                TriggerConsumeEvent -= wormColliderManager.onWormCollide;

                Destroy(gameObject); // disappear on contact with a worm
            }
        }

        private void OnDisable()
        {
            RemoveRewardEvent -= FindObjectOfType<Map.RewardGenerator>().onRemoveReward;
        }
    }   
}