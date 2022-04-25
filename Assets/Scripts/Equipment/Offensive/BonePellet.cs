using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public class BonePellet : Weapon
    {
        private void Awake()
        {
            speed = 1f;
            cooldown = 1f;
            damage = 5f;
            impactType = Impact.None;

            buildingBlocks = new List<BuildingBlock>();
            buildingBlocks.Add(new BuildingBlock(BuildingBlockType.Rock, 1));
            buildingBlocks.Add(new BuildingBlock(BuildingBlockType.Wood, 1));
        }

        // create a bullet, fire the bullet
        protected override void shoot()
        {
            projectileRigidbody.AddForce(transform.forward * speed);

        }
    }
}