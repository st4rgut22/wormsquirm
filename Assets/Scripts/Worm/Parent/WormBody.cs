using UnityEngine;

namespace Worm
{
    public class WormBody : MonoBehaviour
    {
        public delegate void ChangeDirection(DirectionPair directionPair, Tunnel.Tunnel tunnel, string wormId, Tunnel.CellMove cellMove, bool isCreatingTunnel);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void RemoveSelf(Vector3Int currentCell);
        public event RemoveSelf RemoveSelfEvent;

        public delegate void Spawn(ObstacleType wormType, string wormId);
        public event Spawn SpawnEvent;

        public delegate void SaveWorm(string wormId, GameObject wormGO);
        public event SaveWorm SaveWormEvent;

        [SerializeField]
        protected Rigidbody ring;

        [SerializeField]
        protected Rigidbody head;

        [SerializeField]
        protected Rigidbody clit;

        public string wormId;

        protected WormBase wormBase; // stores shared variables across worm prefab classes

        public static float WORM_BODY_THICKNESS = 0.1f;

        public float turnSpeed;

        protected void Awake()
        {
            wormBase = GetComponent<WormBase>();
        }

        protected void OnEnable()
        {
            RemoveSelfEvent += FindObjectOfType<Map.SpawnGenerator>().onRemoveWorm;
            SpawnEvent += FindObjectOfType<Map.SpawnGenerator>().onSpawn;
            ChangeDirectionEvent += Tunnel.CollisionManager.Instance.onChangeDirection;
            ChangeDirectionEvent += FindObjectOfType<Map.SpawnGenerator>().onChangeDirection;
        }

        /**
         * Set the attributes of the worm via subclass
         */
        protected void setWormDescription(Worm worm)
        {
            wormBase.setWormDescription(worm);
        }

        /**
         * Tells the worm to destroy itself. This is NOT called when the game has ended. Only when the player dies and respawns
         */
        protected void RaiseRemoveSelfEvent()
        {
            Vector3Int currentCell = WormTunnelBroker.getCurrentCell(clit.position);
            RemoveSelfEvent(currentCell);
        }

        /**
         * When a worm is destroy it, issue a respawn event. This is NOT called when the game is started. Only when a player dies and respawns
         */
        protected void RaiseSpawnEvent()
        {
            Worm worm = GetComponent<Worm>();
            SpawnEvent(worm.wormType, wormBase.wormId); // spawn 1 worm of the current worm's type
        }

        /**
         * Emit change direction event
         * 
         * @directionPair       the current and next direction of the player
         * @tunnel              the current tunnel player is in
         * @wormId              the id of the player
         */
        protected void RaiseChangeDirectionEvent(DirectionPair directionPair, Tunnel.Tunnel tunnel, string wormId)
        {
            if (directionPair.isStraight())
            {
                wormBase.setChangingDirection(false);
            }
            else
            {
                wormBase.setChangingDirection(true);
            }

            Tunnel.CellMove cellMove;
            if (wormBase.isCreatingTunnel) // if new tunnel use the tunnel's leading cell to get the Cell info
            {
                cellMove = Tunnel.CellMove.getCellMove(tunnel, directionPair); // get cell from map, check if cell in the egress direction is already occupied
            }
            else // if existing tunnel use the worm position to get the Cell info
            {
                //if (wormBase.isChangingDirection)
                //{
                    Vector3Int curCell = Tunnel.TunnelMap.getCellPos(clit.position);
                    cellMove = Tunnel.CellMove.getExistingCellMove(directionPair, curCell);
                //}
                //else
                //{
                //    Vector3Int curCell = Tunnel.TunnelMap.getCellPos(ring.position);
                //    cellMove = new Tunnel.CellMove(wormBase.direction, curCell); // if not making a consecutive turn, ring position will be in the same cell as the turn
                //}
            }
            ChangeDirectionEvent(directionPair, tunnel, wormId, cellMove, wormBase.isCreatingTunnel);
        }

        protected void OnDisable()
        {
            if (FindObjectOfType<Map.SpawnGenerator>())
            {
                RemoveSelfEvent -= FindObjectOfType<Map.SpawnGenerator>().onRemoveWorm;
                SpawnEvent -= FindObjectOfType<Map.SpawnGenerator>().onSpawn;
                ChangeDirectionEvent -= FindObjectOfType<Map.SpawnGenerator>().onChangeDirection;
            }
            if (Tunnel.CollisionManager.Instance)
            {
                ChangeDirectionEvent -= Tunnel.CollisionManager.Instance.onChangeDirection;
            }
        }
    }
}
