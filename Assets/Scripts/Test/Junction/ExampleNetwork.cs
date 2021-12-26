using UnityEngine;
using System.Collections.Generic;

namespace Test
{
    public class ExampleNetwork : MonoBehaviour
    {
        public static List<Checkpoint> threeIntersectLoop;

        // Start is called before the first frame update
        void Awake()
        {
            Checkpoint cp1 = new Checkpoint(new Vector3Int(0, 5, 0), new DirectionPair(Direction.Up, Direction.Right));
            Checkpoint cp2 = new Checkpoint(new Vector3Int(5, 5, 0), new DirectionPair(Direction.Right, Direction.Up));
            Checkpoint cp3 = new Checkpoint(new Vector3Int(5, 10, 0), new DirectionPair(Direction.Up, Direction.Left));
            Checkpoint cp4 = new Checkpoint(new Vector3Int(-5, 10, 0), new DirectionPair(Direction.Left, Direction.Down));
            Checkpoint cp5 = new Checkpoint(new Vector3Int(-5, 5, 0), new DirectionPair(Direction.Down, Direction.Right));
            threeIntersectLoop = new List<Checkpoint>() { cp1, cp2, cp3, cp4, cp5 };
        }
    }

}