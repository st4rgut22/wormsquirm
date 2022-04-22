using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public class EquipmentManager : GenericSingletonClass<EquipmentManager>
    {
        List<Item> ItemList;

        private new void Awake()
        {
            base.Awake();
            ItemList = new List<Item>();


        }
    }

}