using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Corner : Tunnel
    {
        private new void Awake()
        {
            base.Awake();
            isStopped = true;
            type = Type.Name.CORNER;
        }

        public override void setHoleDirections(DirectionPair dirPair)
        {
            holeDirectionList = new List<Direction>() { dirPair.prevDir, dirPair.curDir };
        }
    }
}