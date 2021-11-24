using UnityEngine;

public enum Direction
{
    Up,
    Right,
    Left,
    Forward,
    Back,
    Down,
    None,
}

public class DirectionPair
{
    public Direction prevDir { get; private set; }
    public Direction curDir { get; private set; }

    public DirectionPair(Direction prevDir, Direction curDir)
    {
        this.prevDir = prevDir;
        this.curDir = curDir;
    }
}

public static class Dir
{
    public static Direction getOppositeDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Right:
                return Direction.Left;
            case Direction.Left:
                return Direction.Right;
            case Direction.Forward:
                return Direction.Back;
            case Direction.Back:
                return Direction.Forward;
            case Direction.Down:
                return Direction.Up;
            default:
                throw new System.Exception("Not a valid direction " + direction);
        }
    }

    public static Direction getChangedDirection(Direction direction, InputKey key)
    {
        switch (direction)
        {
            case Direction.Right:
                return mapWASDToDirection(key, Direction.Up, Direction.Back, Direction.Down, Direction.Forward);
            case Direction.Back:
                return mapWASDToDirection(key, Direction.Up, Direction.Left, Direction.Down, Direction.Right);
            case Direction.Left:
                return mapWASDToDirection(key, Direction.Down, Direction.Forward, Direction.Up, Direction.Back);
            case Direction.Forward:
                return mapWASDToDirection(key, Direction.Up, Direction.Right, Direction.Down, Direction.Left);
            case Direction.Up:
                return mapWASDToDirection(key, Direction.Left, Direction.Left, Direction.Right, Direction.Right);
            case Direction.Down:
                return mapWASDToDirection(key, Direction.Right, Direction.Right, Direction.Left, Direction.Left);
            default:
                throw new System.Exception("not a valid direction " + direction);
        }
    }

    /**
     * Decide the changed direction based off of user input 
     */
    public static Direction mapWASDToDirection(InputKey key, Direction northDir, Direction westDir, Direction southDir, Direction eastDir)
    {
        switch (key.keyCode)
        {
            case KeyCode.W:
                return northDir;
            case KeyCode.S:
                return southDir;
            case KeyCode.A:
                return westDir;
            case KeyCode.D:
                return eastDir;
            default:
                throw new System.Exception("Not a valid keycode " + key.keyCode);
        }
    }
}