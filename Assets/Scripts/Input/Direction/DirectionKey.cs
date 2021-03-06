using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionPair
{
    public Direction prevDir;
    public Direction curDir;

    public DirectionPair(Direction prevDir, Direction curDir)
    {
        this.prevDir = prevDir;
        this.curDir = curDir;
    }

    public bool isStraight()
    {
        return prevDir == curDir;
    }
}

public enum Direction
{
    Back,
    Down,
    Forward,
    Left,
    Right,
    Up,
    None,
    Same
}


public class DirectionKey : MonoBehaviour
{
    public static void combineDirection(List<Direction> dirList)
    {
        dirList.Sort();
    }
}
