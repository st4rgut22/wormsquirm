using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public enum MoveType
    {
        ENTRANCE,
        CENTER,
        EXIT,
        OFFSET
    }

    public class Waypoint
    {
        public Vector3 position { get; private set; }
        public MoveType move { get; private set; }
        public Waypoint(Vector3 position, MoveType move)
        {
            this.position = position;
            this.move = move;
        }
    }

}