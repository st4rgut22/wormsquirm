using UnityEngine;

namespace Worm
{
    public class WormBase : WormBody
    {
        public delegate void SaveWorm(string wormId, GameObject wormGO);
        public event SaveWorm SaveWormEvent;

        public Direction direction { get; private set; }

        public bool isStraight = true;

        public void setDirection(Direction direction)
        {
            print("direction in WormBase is " + direction);
            this.direction = direction;
        }

        private void OnEnable()
        {
            SaveWormEvent += WormManager.Instance.onSave;
        }

        private new void Awake()
        {
            base.Awake();
            setDirection(Direction.None);
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
