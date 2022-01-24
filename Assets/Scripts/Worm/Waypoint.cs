using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Worm
{
    public enum MoveType
    {
        STRAIGHT,
        ENTRANCE,
        CENTER,
        EXIT,
        OFFSET
    }

    public class Waypoint
    {
        public Vector3 position { get; private set; }
        public MoveType move { get; private set; }
        public Direction direction { get; private set; } // direction of movement to pass the waypoint

        public Waypoint(Vector3 position, MoveType move, Direction direction)
        {
            this.position = position;
            this.move = move;
            this.direction = direction;
        }
    }

}