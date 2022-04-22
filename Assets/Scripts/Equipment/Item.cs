using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public abstract class Item : MonoBehaviour
    {
        public List<BuildingBlock> buildingBlocks;

        public abstract void use(Vector3 position);
    }

}