using UnityEngine;

namespace Worm
{
    public class WormBase : WormBody
    {
        public delegate void SaveWorm(string wormId, GameObject wormGO);
        public event SaveWorm SaveWormEvent;

        public Direction direction = Direction.None;

        public bool isStraight = true;

        private void OnEnable()
        {
            SaveWormEvent += WormManager.Instance.onSave;
        }

        private void Start()
        {
            SaveWormEvent(wormId, gameObject);
        }

        private void OnDisable()
        {
            SaveWormEvent -= WormManager.Instance.onSave;
        }
    }
}
