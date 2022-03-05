namespace Worm
{
    /**
     * Attributes of AI worms
     */
    public class AIWorm : Worm
    {
        public enum AIType
        {
            FixedPath,
            Astar
        }

        private new void Awake()
        {
            base.Awake();
            wormType = ObstacleType.AIWorm;
        }
    }
}