using UnityEngine;

namespace Worm
{
    /**
     * Attributes used to instantiate a worm whether its an AI or player. Must be able to assign these attributes BEFORE
     * the gameobject is instantiated
     */
    public class Worm : WormBody
    {
        public ObstacleType wormType { get; protected set; }
        public string wormId { get; protected set; }

        private new void Awake()
        {
            base.Awake();
            setWormDescription(this);
        }
    }

}