using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public class WormInventory : WormBody
    {
        Equipment.Item equippedItem;

        List<Equipment.Item> ItemList; // list of item created from materials
        Dictionary<Equipment.BuildingBlockType, int> BuildingBlockDict; // <building block type, block count>

        private new void Awake()
        {
            base.Awake();
            BuildingBlockDict = new Dictionary<Equipment.BuildingBlockType, int>();
        }

        public void onAddToInventory(Equipment.Block block, string wormId)
        {
            if (!BuildingBlockDict.ContainsKey(block.buildingBlockType))
            {
                BuildingBlockDict[block.buildingBlockType] = 1;
            }
            else
            {
                BuildingBlockDict[block.buildingBlockType] += 1;
            }
        }

        // return boolean indicating if enough ammunition to fire
        private bool useBuildingBlocks()
        {
            return true; // TESTING!!!
            bool isAmmoSufficient = true;
            equippedItem.buildingBlocks.ForEach((Equipment.BuildingBlock buildingBlock) => {
                if (!BuildingBlockDict.ContainsKey(buildingBlock.type) || BuildingBlockDict[buildingBlock.type] <= 0)
                {
                    isAmmoSufficient = false;
                }
            });
            if (isAmmoSufficient)
            {
                equippedItem.buildingBlocks.ForEach((Equipment.BuildingBlock buildingBlock) => {
                    BuildingBlockDict[buildingBlock.type] -= 1;
                });
            }
            return isAmmoSufficient;
        }

        public void onUseItem()
        {
            bool isAmmoEnough = useBuildingBlocks();
            if (isAmmoEnough)
            {
                equippedItem.use();
            }
        }

        /**
         * Received event to use the equipped item
         */
        public void onEquip(Equipment.Item item)
        {
            // show target, fire on input
            equippedItem = item;
        }
    }
}