using UnityEngine;
using System.Collections.Generic;

namespace Test
{
    public class ExampleNetwork : MonoBehaviour
    {
        public delegate void initCheckpoint(List<Checkpoint> checkpointList);
        public event initCheckpoint initCheckpointEvent;

        private void OnEnable()
        {
            initCheckpointEvent += FindObjectOfType<TunnelMaker>().onInitCheckpointList;
        }

        private List<Checkpoint> initThreeIntersectLoop()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 5);
            Checkpoint cp1 = new Checkpoint(Direction.Right, 5);
            Checkpoint cp2 = new Checkpoint(Direction.Up, 5);
            Checkpoint cp3 = new Checkpoint(Direction.Left, 11);
            Checkpoint cp4 = new Checkpoint(Direction.Down, 5);
            Checkpoint cp5 = new Checkpoint(Direction.Right, 5);
            List<Checkpoint> threeIntersectLoopStraight = new List<Checkpoint>() { cp0, cp1, cp2, cp3, cp4, cp5 };

            List<Checkpoint> checkpointList = initThreeIntersectLoopCorner(threeIntersectLoopStraight);
            return checkpointList;
        }

        private List<Checkpoint> initThreeIntersectLoopCorner(List<Checkpoint> threeIntersectLoopStraight)
        {
            List<Checkpoint> threeIntersectLoopCorner = new List<Checkpoint>(threeIntersectLoopStraight);
            threeIntersectLoopCorner.RemoveAt(threeIntersectLoopCorner.Count - 1);
            Checkpoint cp = new Checkpoint(Direction.Right, 6);
            Checkpoint cpTurn = new Checkpoint(Direction.Up, 1);
            threeIntersectLoopCorner.Add(cp);
            threeIntersectLoopCorner.Add(cpTurn);
            return threeIntersectLoopCorner;
        }

        private List<Checkpoint> initZigzag()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 5);
            Checkpoint cp1 = new Checkpoint(Direction.Right, 1);
            Checkpoint cp2 = new Checkpoint(Direction.Up, 1);
            List<Checkpoint> zigzag = new List<Checkpoint>() { cp0, cp1, cp2 };
            return zigzag;
        }

        // Start is called before the first frame update
        void Start()
        {
            //List<Checkpoint> checkpointList = initThreeIntersectLoop();
            List<Checkpoint> checkpointList = initZigzag();
            initCheckpointEvent(checkpointList);
        }


        private void OnDisable()
        {
            if (FindObjectOfType<TunnelMaker>())
            {
                initCheckpointEvent -= FindObjectOfType<TunnelMaker>().onInitCheckpointList;
            }
        }
    }

}