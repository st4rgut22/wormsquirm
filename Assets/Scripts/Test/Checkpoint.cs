using UnityEngine;

public class Checkpoint
{
    public Direction direction { get; private set; }
    public int length { get; private set; }

    public Checkpoint(Direction direction, int length)
    {
        this.direction = direction;
        this.length = length;
    }
}
