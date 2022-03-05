using UnityEngine;

namespace Worm
{
    public class WormBase : WormBody
    {
        [SerializeField]
        public Vector3Int initialCell;

        public Vector3Int curCell { get; private set; } // the current cell worm is in using ring position as reference

        public Direction direction { get; private set; }

        public bool isStraight = true;

        public bool isInitialized { get; private set; }

        public bool isCreatingTunnel { get; private set; }

        public bool isDecision { get; private set; } // flag to check if a decision has been made

        public bool isChangingDirection { get; private set; } // flag to check if worm is changing direction in a cell

        public Worm WormDescription { get; private set; }

        private new void Awake()
        {
            base.Awake();
            isInitialized = false;
            setInitialCell(initialCell);
            setDirection(Direction.None);
        }

        /**
         * Call the OnEnable method fo parent ONCE to set up delegates that can be called by all subclasses
         */
        private new void OnEnable()
        {
            base.OnEnable();
        }

        /**
         * Set the boolean flag true to indicate the worm has started moving
         */
        public void initializeWorm()
        {
            isInitialized = true;
        }

        public void setStraight(bool isStraight)
        {
            this.isStraight = isStraight;
        }

        /**
         * Set the attributes of worm, which includes type of worm
         */
        public new void setWormDescription(Worm WormDescription)
        {
            this.WormDescription = WormDescription;
        }

        /**
         * Set the current cell of the worm
         */
        public void setCurCell(Vector3Int curCell)
        {
            this.curCell = curCell;
        }

        /**
         * Set a boolean flag indicating if worm is changing direction
         */
        public void setChangingDirection(bool isChangingDirection)
        {
            this.isChangingDirection = isChangingDirection;
        }

        /**
         * Set a boolean flag indicating if worm is about to turn
         */
        public void setDecision(bool isDecision)
        {
            this.isDecision = isDecision;
        }

        /**
         * Set the boolean flag indicating if worm is creating a tunnel or in an existing tunnel
         * 
         * @isCreating      if true means the worm is creating a new tunnel
         */
        public void setIsCreatingTunnel(bool isCreating)
        {
            isCreatingTunnel = isCreating;
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

        private new void OnDisable()
        {
            base.OnDisable();
        }
    }
}
