using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public enum BuildingBlockType
    {
        Plutonium,
        Pesticide,
        Rock,
        RottenEgg,
        Wood
    }

    public class BuildingBlock
    {
        public BuildingBlockType type;
        public int quantity;

        public BuildingBlock(BuildingBlockType type, int quantity)
        {
            this.type = type;
            this.quantity = quantity;
        }
    }
}