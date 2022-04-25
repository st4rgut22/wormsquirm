using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public enum ItemType
    {
        BonePellet,
        NoxiousFart,
        NuclearFart,
        SlimeShower,
        GasCasket,
        TrashArmor,
        StoneSlinger,
        OuchyTrap,
        None
    }

    public class EquipmentFactory : GenericSingletonClass<EquipmentFactory>
    {
        [SerializeField]
        private GameObject BonePelletGO;

        public Item createItem(ItemType itemType)
        {
            GameObject itemPrefab = null;

            switch (itemType)
            {
                case ItemType.BonePellet:
                    itemPrefab = Instantiate(BonePelletGO);
                    break;
                case ItemType.NoxiousFart:
                    break;
                case ItemType.NuclearFart:
                    break;
                case ItemType.SlimeShower:
                    break;
                case ItemType.GasCasket:
                    break;
                case ItemType.TrashArmor:
                    break;
                case ItemType.StoneSlinger:
                    break;
                case ItemType.OuchyTrap:
                    break;
                case ItemType.None:
                    break;
            }
            return itemPrefab.GetComponent<Item>();
        }

    }
}