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
        protected GameObject ProjectilePrefab;

        protected float speed;

        protected float cooldown; // time to wait between shots

        protected float damage;

        protected Impact impactType;

        protected abstract void shoot();

        public override void use(Vector3 position)
        {
            shoot();
        }
    }

}
