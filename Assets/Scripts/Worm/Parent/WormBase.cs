using UnityEngine;

namespace Worm
{
    public class WormBase : WormBody
    {
        public delegate void UpdateCell(Vector3Int curCellPos, Vector3Int nextCellPos, bool isDeleteCurCell);
        private event UpdateCell UpdateCellEvent;

        [SerializeField]
        public Vector3Int initialCell; // used to place tunnel

        public Vector3Int mappedInitialCell; // initial cell translated if necessary in the map

        public Direction direction { get; private set; }

        public Direction turnDirection { get; private set; } // the next direction is received as the turn begins

        public bool isStraight = true;

        public bool isInitialized { get; private set; }

        public bool isCreatingTunnel { get; private set; }

        public bool isDecision { get; private set; } // flag to check if a decision has been made

        public bool isChangingDirection { get; private set; } // flag to check if worm is changing direction in a cell

        public Worm WormDescription { get; private set; }

        public Vector3Int defaultCell = new Vector3Int(-100, -100, -100);

        private new void Awake()
        {
            base.Awake();
            isInitialized = false;
            setInitialCell(initialCell);
            setDirection(Direction.None);
            setTurnDirection(Direction.None);
        }

        /**
         * Call the OnEnable method fo parent ONCE to set up delegates that can be called by all subclasses
         */
        private new void OnEnable()
        {
            base.OnEnable();
            UpdateCellEvent += Map.SpawnGenerator.onUpdateObstacle;
        }

        /**
         * Initialize the worm's initial cell
         */
        public void initializeWorm(Direction initialDirection)
        {
            isInitialized = true;
            setDirection(initialDirection);
            mappedInitialCell = initialCell;
            // if in negative direction (eg down) modify initialCell so that the tunnel starts one cell earlier.
            // this is because when going in negative direction tunnel cell Vector3Int positionmust be offset in the opposite direction to occupy the same
            // space as when worm is traveling in the positive direction
            if (Dir.Base.isDirectionNegative(initialDirection))
            {
                initialCell = Dir.Vector.getNextCellFromOppDirection(initialCell, initialDirection);                
            }
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

        public void setTurnDirection(Direction direction)
        {
            turnDirection = direction;
        }

        public void setDirection(Direction direction)
        {
            print("direction in WormBase is " + direction);
            this.direction = direction;
        }

        private new void OnDisable()
        {
            base.OnDisable();
            if (FindObjectOfType<Map.SpawnGenerator>())
            {
                UpdateCellEvent -= Map.SpawnGenerator.onUpdateObstacle;;
            }
        }
    }
}
