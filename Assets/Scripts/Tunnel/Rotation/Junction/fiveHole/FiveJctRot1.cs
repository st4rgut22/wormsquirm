using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rotation
{
    public class FiveJctRot1 : Rotation
    {
        Dictionary<List<Direction>, Quaternion> upDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Right, Direction.Left, Direction.Back, Direction.Forward }, Quaternion.Euler(270, 0, 0) }
        };

        Dictionary<List<Direction>, Quaternion> downDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Right, Direction.Left, Direction.Back, Direction.Forward }, Quaternion.Euler(90, 0, 0) }
        };

        Dictionary<List<Direction>, Quaternion> leftDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Back, Direction.Forward }, Quaternion.Euler(0, 270, 0) }
        };

        Dictionary<List<Direction>, Quaternion> rightDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Back, Direction.Forward }, Quaternion.Euler(0, 90, 0) }
        };

        Dictionary<List<Direction>, Quaternion> forwardDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Right, Direction.Left, Direction.Down, Direction.Up }, Quaternion.Euler(0, 0, 0) }
        };

        Dictionary<List<Direction>, Quaternion> backDirListDict = new Dictionary<List<Direction>, Quaternion>(new DirectionList())
        {
            { new List<Direction>(){ Direction.Down, Direction.Up, Direction.Right, Direction.Left }, Quaternion.Euler(0, 180, 0) }
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

