namespace Worm
{
    public class HumanWorm : Worm
    {
        private new void Awake()
        {
            base.Awake();
            wormType = ObstacleType.PlayerWorm;
        }
    }
}