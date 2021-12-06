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
            rotation = new Rotation.Corner();
        }

        public override void setHoleDirections(DirectionPair directionPair)
        {
            holeDirectionList = new List<Direction>() { directionPair.prevDir, directionPair.curDir };
        }
    }
}