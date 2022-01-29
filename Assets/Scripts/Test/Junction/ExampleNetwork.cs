using UnityEngine;
using System.Collections.Generic;

namespace Test
{
    //public class Network
    //{
    //    public Direction initDir { get; private set; }
    //    public List<Checkpoint> checkpoints { get; private set;  }
    //    public Network(Direction dir, List<Checkpoint> checkpoints)
    //    {
    //        initDir = dir;
    //        this.checkpoints = checkpoints;
    //    }
    //}

    public class ExampleNetwork : MonoBehaviour
    {
        public static List<Checkpoint> threeIntersectLoopCorner;
        public static List<Checkpoint> threeIntersectLoopStraight;
        //public static Network zigzagNetwork;

        private void initThreeIntersectLoop()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 5);
            Checkpoint cp1 = new Checkpoint(Direction.Right, 5);
            Checkpoint cp2 = new Checkpoint(Direction.Up, 5);
            Checkpoint cp3 = new Checkpoint(Direction.Left, 11);
            Checkpoint cp4 = new Checkpoint(Direction.Down, 5);
            Checkpoint cp5 = new Checkpoint(Direction.Right, 5);
            threeIntersectLoopStraight = new List<Checkpoint>() { cp0, cp1, cp2, cp3, cp4, cp5 };

            initThreeIntersectLoopCorner(threeIntersectLoopStraight);
        }

        private void initThreeIntersectLoopCorner(List<Checkpoint> threeIntersectLoopStraight)
        {
            threeIntersectLoopCorner = new List<Checkpoint>(threeIntersectLoopStraight);
            threeIntersectLoopCorner.RemoveAt(threeIntersectLoopCorner.Count - 1);
            Checkpoint cp = new Checkpoint(Direction.Right, 6);
            Checkpoint cpTurn = new Checkpoint(Direction.Up, 1);
            threeIntersectLoopCorner.Add(cp);
            threeIntersectLoopCorner.Add(cpTurn);
        }

        //private void initZigzag()
        //{
        //    Checkpoint cp1 = new Checkpoint(new Vector3Int(0, 5, 0), new DirectionPair(Direction.Up, Direction.Right));
        //    Checkpoint cp2 = new Checkpoint(new Vector3Int(1, 5, 0), new DirectionPair(Direction.Right, Direction.Up));
        //    Checkpoint cp3 = new Checkpoint(new Vector3Int(1, 6, 0), new DirectionPair(Direction.Up, Direction.Right));
        //    List<Checkpoint> zigzag = new List<Checkpoint>() { cp1, cp2, cp3 };

        //    zigzagNetwork = new Network(Direction.Up, zigzag);
        //}

        // Start is called before the first frame update
        void Awake()
        {
            initThreeIntersectLoop();
            //initZigzag();
        }
    }

}