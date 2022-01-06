using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rotation
{
    public class SixJctRot : Rotation
    {
        Dictionary<List<Direction>, Quaternion> upDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Right, Direction.Left, Direction.Back, Direction.Up, Direction.Forward }, Quaternion.Euler(0, 0, 0) }

        };

        Dictionary<List<Direction>, Quaternion> downDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Right, Direction.Left, Direction.Back, Direction.Down, Direction.Forward }, Quaternion.Euler(0, 0, 180) }
        };

        Dictionary<List<Direction>, Quaternion> leftDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Left, Direction.Forward, Direction.Back }, Quaternion.Euler(0, 0, 90) }
        };

        Dictionary<List<Direction>, Quaternion> rightDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Back, Direction.Right, Direction.Forward }, Quaternion.Euler(0, 0, 270) }
        };

        Dictionary<List<Direction>, Quaternion> forwardDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Forward, Direction.Left, Direction.Down, Direction.Up, Direction.Right }, Quaternion.Euler(0, 90, 90) }
        };

        Dictionary<List<Direction>, Quaternion> backDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Back, Direction.Left, Direction.Right }, Quaternion.Euler(0, 90, 270) }
        };


        private new void Awake()
        {
            base.Awake();
            rotationDict.Add(Direction.Up, upDirListDict);
            rotationDict.Add(Direction.Down, downDirListDict);
            rotationDict.Add(Direction.Left, leftDirListDict);
            rotationDict.Add(Direction.Right, rightDirListDict);
            rotationDict.Add(Direction.Forward, forwardDirListDict);
            rotationDict.Add(Direction.Back, backDirListDict);
        }
    }
}

