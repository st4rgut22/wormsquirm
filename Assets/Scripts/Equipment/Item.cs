using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public abstract class Item : MonoBehaviour
    {
        [SerializeField]
        protected GameObject itemGO;

        protected ItemType itemType;

        protected GameObject itemPrefab;

        public List<BuildingBlock> buildingBlocks;

        public abstract void use();

        public void equip(Transform parent)
        {
            itemPrefab = Instantiate(itemGO, parent);
        }
    }

}