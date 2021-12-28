using UnityEngine;
using System.Collections.Generic;

namespace Tunnel
{    
    public class Junction : Tunnel
    {
        public override Vector3 getContactPosition(DirectionPair dirPair)
        {
            return getEgressPosition(dirPair.curDir, center);
        }

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