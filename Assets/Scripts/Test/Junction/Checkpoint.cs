using UnityEngine;

namespace Test
{
    /**
     * A checkpoint simulates a turn made by the user
     */
    public class Checkpoint
    {
        public Vector3Int decisionCell;
        public DirectionPair dirPair;

        public Checkpoint(Vector3Int cell, DirectionPair dirPair)
        {
            Direction ingressDir = dirPair.prevDir;
            Direction oppDir = Dir.Base.getOppositeDirection(ingressDir);
            decisionCell = cell.getNextVector3Int(oppDir);
            this.dirPair = dirPair;
        }
    }

}