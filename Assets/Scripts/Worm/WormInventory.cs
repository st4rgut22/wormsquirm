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

        public void useBuildingBlocks()
        {
            equippedItem.buildingBlocks.ForEach((Equipment.BuildingBlock buildingBlock) => {
                if (BuildingBlockDict[buildingBlock.type] <= 0)
                {
                    throw new System.Exception("No more building blocks of type " + buildingBlock.type + " but trying to use it");
                }
                BuildingBlockDict[buildingBlock.type] -= 1;
            });
        }

        /**
         * Received event to use the equipped item
         */
        public void onEquip()
        {
            useBuildingBlocks();
            equippedItem.use(head.position);

        }
    }
}