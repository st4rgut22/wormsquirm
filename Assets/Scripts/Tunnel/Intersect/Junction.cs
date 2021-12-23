using UnityEngine;
using System.Collections.Generic;

namespace Tunnel
{    
    public class Junction : Tunnel
    {

        /**
         * Add the hole created from the ingress direction to the holes of the previous junction
         */
        public override void setHoleDirections(DirectionPair dirPair)
        {
            Direction newHoleDirection = Dir.Base.getOppositeDirection(dirPair.prevDir);
            holeDirectionList.Add(newHoleDirection);
        }

        new private void Awake()
        {
            base.Awake();
        }
    }

}