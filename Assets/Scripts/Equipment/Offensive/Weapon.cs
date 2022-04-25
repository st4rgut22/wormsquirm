using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Impact
{
    Explosion,
    None
}

namespace Equipment
{
    public abstract class Weapon : Item
    {
        [SerializeField]
        protected GameObject projectileGO;

        protected GameObject projectilePrefab;
        protected Rigidbody projectileRigidbody;

        ItemType weaponType;

        protected float speed;

        protected float cooldown; // time to wait between shots

        protected float damage;

        protected Impact impactType;

        protected abstract void shoot();

        public override void use()
        {
            projectilePrefab = Instantiate(projectileGO, itemGO.transform);
            projectileRigidbody = projectilePrefab.GetComponent<Rigidbody>();
            shoot();
        }
    }

}
