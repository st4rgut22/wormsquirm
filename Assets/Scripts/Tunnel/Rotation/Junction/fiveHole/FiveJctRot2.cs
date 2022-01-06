using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rotation
{
    public class FiveJctRot2 : Rotation
    {
        Dictionary<List<Direction>, Quaternion> upDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Right, Direction.Left, Direction.Back, Direction.Up }, Quaternion.Euler(0, 0, 0) },
            { new List<Direction>(){ Direction.Forward, Direction.Left, Direction.Back, Direction.Up }, Quaternion.Euler(0, 90, 0) },
            { new List<Direction>(){ Direction.Right, Direction.Left, Direction.Forward, Direction.Up }, Quaternion.Euler(0, 180, 0) },
            { new List<Direction>(){ Direction.Right, Direction.Back, Direction.Forward, Direction.Up }, Quaternion.Euler(0, 270, 0) }

        };

        Dictionary<List<Direction>, Quaternion> downDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Right, Direction.Left, Direction.Back, Direction.Down }, Quaternion.Euler(0, 0, 180) },
            { new List<Direction>(){ Direction.Forward, Direction.Left, Direction.Back, Direction.Down }, Quaternion.Euler(0, 90, 180) },
            { new List<Direction>(){ Direction.Forward, Direction.Left, Direction.Right, Direction.Down }, Quaternion.Euler(0, 180, 180) },
            { new List<Direction>(){ Direction.Forward, Direction.Back, Direction.Right, Direction.Down }, Quaternion.Euler(0, 270, 180) },
        };

        Dictionary<List<Direction>, Quaternion> leftDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Left, Direction.Forward }, Quaternion.Euler(0, 180, 270) },
            { new List<Direction>(){ Direction.Back, Direction.Up, Direction.Left, Direction.Forward }, Quaternion.Euler(90, 270, 0) },
            { new List<Direction>(){ Direction.Back, Direction.Up, Direction.Left, Direction.Down }, Quaternion.Euler(0, 0, 90) },
            { new List<Direction>(){ Direction.Back, Direction.Forward, Direction.Left, Direction.Down }, Quaternion.Euler(270, 90, 0) }
        };

        Dictionary<List<Direction>, Quaternion> rightDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Back, Direction.Right }, Quaternion.Euler(0, 0, 270) },
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Forward, Direction.Right }, Quaternion.Euler(0, 180, 90) },
            { new List<Direction>(){ Direction.Back, Direction.Up, Direction.Forward, Direction.Right }, Quaternion.Euler(90, 90, 0) },
            { new List<Direction>(){ Direction.Back, Direction.Down, Direction.Forward, Direction.Right }, Quaternion.Euler(270, 270, 0) },
        };

        Dictionary<List<Direction>, Quaternion> forwardDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Forward, Direction.Left, Direction.Down, Direction.Up }, Quaternion.Euler(0, 90, 90) },
            { new List<Direction>(){ Direction.Forward, Direction.Right, Direction.Down, Direction.Up }, Quaternion.Euler(0, 270, 270) },
            { new List<Direction>(){ Direction.Forward, Direction.Right, Direction.Left, Direction.Up }, Quaternion.Euler(90, 0, 0) },
            { new List<Direction>(){ Direction.Forward, Direction.Right, Direction.Left, Direction.Down }, Quaternion.Euler(270, 180, 0) }
        };

        Dictionary<List<Direction>, Quaternion> backDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Back, Direction.Left }, Quaternion.Euler(0, 90, 270) },
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Back, Direction.Right }, Quaternion.Euler(0, 270, 90) },
            { new List<Direction>(){ Direction.Left, Direction.Up, Direction.Back, Direction.Right }, Quaternion.Euler(90, 180, 0) },
            { new List<Direction>(){ Direction.Left, Direction.Down, Direction.Back, Direction.Right }, Quaternion.Euler(270, 0, 0) }
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

