public abstract class Axis
{
    protected float movement;

    protected Axis(int movement)
    {
        this.movement = movement;
    }

    public abstract Direction getDirectionAlongPlane(Direction direction);
}