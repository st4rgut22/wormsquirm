using UnityEngine;
using System.Collections.Generic;

namespace Test
{
    public class ExampleNetwork : MonoBehaviour
    {
        public static List<Checkpoint> threeIntersectLoopCorner;
        public static List<Checkpoint> threeIntersectLoopStraight;
        public static List<Checkpoint> zigzag;

        private void initThreeIntersectLoop()
        {
            Checkpoint cp1 = new Checkpoint(new Vector3Int(0, 5, 0), new DirectionPair(Direction.Up, Direction.Right));
            Checkpoint cp2 = new Checkpoint(new Vector3Int(5, 5, 0), new DirectionPair(Direction.Right, Direction.Up));
            Checkpoint cp3 = new Checkpoint(new Vector3Int(5, 10, 0), new DirectionPair(Direction.Up, Direction.Left));
            Checkpoint cp4 = new Checkpoint(new Vector3Int(-5, 10, 0), new DirectionPair(Direction.Left, Direction.Down));
            Checkpoint cp5 = new Checkpoint(new Vector3Int(-5, 5, 0), new DirectionPair(Direction.Down, Direction.Right));
            Checkpoint cp6 = new Checkpoint(new Vector3Int(0, 5, 0), new DirectionPair(Direction.Right, Direction.Right));
            threeIntersectLoopStraight = new List<Checkpoint>() { cp1, cp2, cp3, cp4, cp5, cp6 };
            initThreeIntersectLoopCorner();
        }

        private void initThreeIntersectLoopCorner()
        {            
            threeIntersectLoopCorner = new List<Checkpoint>(threeIntersectLoopStraight);
            Checkpoint cpLast = threeIntersectLoopCorner[threeIntersectLoopCorner.Count - 1];
            Checkpoint cpFirst = threeIntersectLoopCorner[0];
            Direction egressDir = Dir.Base.getOppositeDirection(cpFirst.dirPair.prevDir);
            Checkpoint cpTurn = new Checkpoint(cpLast.cell, new DirectionPair(cpLast.dirPair.prevDir, egressDir));
            threeIntersectLoopCorner.Remove(cpLast);
            threeIntersectLoopCorner.Add(cpTurn);
        }

        private void initZigzag()
        {
            Checkpoint cp1 = new Checkpoint(new Vector3Int(0, 5, 0), new DirectionPair(Direction.Up, Direction.Right));
            Checkpoint cp2 = new Checkpoint(new Vector3Int(1, 5, 0), new DirectionPair(Direction.Right, Direction.Up));
            Checkpoint cp3 = new Checkpoint(new Vector3Int(1, 6, 0), new DirectionPair(Direction.Up, Direction.Right));
            zigzag = new List<Checkpoint>() { cp1, cp2, cp3 };
        }

        // Start is called before the first frame update
        void Awake()
        {
            initThreeIntersectLoop();
            initZigzag();
        }
    }

}