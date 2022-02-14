using UnityEngine;

namespace Worm
{
    public class WormBase : WormBody
    {
        public delegate void SaveWorm(string wormId, GameObject wormGO);
        public event SaveWorm SaveWormEvent;

        [SerializeField]
        public Vector3Int initialCell;

        public Direction direction { get; private set; }

        public bool isStraight = true;

        public bool isInitialized { get; private set; }



        private void OnEnable()
        {
            SaveWormEvent += WormManager.Instance.onSave;
        }

        private new void Awake()
        {
            base.Awake();
            isInitialized = false;
            setInitialCell(initialCell);

            setDirection(Direction.None);
        }

        /**
         * Set the boolean flag true to indicate the worm has started moving
         */
        public void initializeWorm()
        {
            isInitialized = true;
        }

        public void setInitialCell(Vector3Int initialCell)
        {
            this.initialCell = initialCell;
        }        

        public void setDirection(Direction direction)
        {
            print("direction in WormBase is " + direction);
            this.direction = direction;
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
