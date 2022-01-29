using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Corner : TurnableTunnel
    {
        private new void Awake()
        {
            base.Awake();
            isStopped = true;
            type = Type.Name.CORNER;
        }

        public override Vector3 getContactPosition(DirectionPair dirPair)
        {
            return getEgressPosition(dirPair.curDir, center);
        }

        public override void setHoleDirections(DirectionPair dirPair)
        {
            Direction oppDir = Dir.Base.getOppositeDirection(dirPair.prevDir); // new hole faces opposite the ingress direction
            holeDirectionList = new List<Direction>() { oppDir, dirPair.curDir };
        }
    }
}