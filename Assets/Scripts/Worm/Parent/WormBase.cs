using UnityEngine;

namespace Worm
{
    public class WormBase : WormBody
    {
        public delegate void SaveWorm(string wormId, GameObject wormGO);
        public event SaveWorm SaveWormEvent;

        public Direction direction { get; private set; }

        public bool isStraight = true;

        public Vector3Int initialCell { get; private set; }

        public bool isInitialized { get; private set; }



        private void OnEnable()
        {
            SaveWormEvent += WormManager.Instance.onSave;
        }

        private new void Awake()
        {
            base.Awake();
            isInitialized = false;
            Vector3Int testInitialCell = Vector3Int.zero; // new Vector3Int(1, 1, 1);
            setInitialCell(testInitialCell);

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
