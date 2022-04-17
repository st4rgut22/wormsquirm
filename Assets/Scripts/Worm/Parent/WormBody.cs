using UnityEngine;

namespace Worm
{
    public class WormBody : MonoBehaviour
    {
        public delegate void ChangeDirection(DirectionPair directionPair, Tunnel.Tunnel tunnel, string wormId, Tunnel.CellMove cellMove, bool isCreatingTunnel);
        public event ChangeDirection ChangeDirectionEvent;

        public delegate void RemoveSelf(string wormId);
        public event RemoveSelf RemoveSelfEvent;

        public delegate void SpawnAi(string wormId);
        public event SpawnAi SpawnAiEvent;

        public delegate void SpawnHuman(string wormId);
        public event SpawnHuman SpawnHumanEvent;

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
            SpawnAiEvent += FindObjectOfType<Map.AiSpawnGenerator>().onAiSpawn;
            SpawnHumanEvent += FindObjectOfType<Map.HumanSpawnGenerator>().onHumanSpawn;

            ChangeDirectionEvent += Tunnel.CollisionManager.Instance.onChangeDirection;
            ChangeDirectionEvent += Map.SpawnGenerator.onChangeDirection; // DO NOT MOVE !!! Order delegates are called MUST BE RESPECTED for proper updating of cells
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
            RemoveSelfEvent += Map.SpawnGenerator.onRemoveWorm;
            RemoveSelfEvent(wormBase.wormId);
            RemoveSelfEvent -= Map.SpawnGenerator.onRemoveWorm;
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
                Vector3Int curCell = Tunnel.TunnelMap.getCellPos(clit.position);
                cellMove = Tunnel.CellMove.getExistingCellMove(directionPair, curCell);
            }
            print("Player " + wormId + " change direction. Current cell is " + cellMove.cell + " Next cell is " + cellMove.nextCell);
            ChangeDirectionEvent(directionPair, tunnel, wormId, cellMove, wormBase.isCreatingTunnel);
        }

        protected void OnDisable()
        {
            if (FindObjectOfType<Map.SpawnGenerator>())
            {                
                ChangeDirectionEvent -= Map.SpawnGenerator.onChangeDirection;

                SpawnAiEvent -= FindObjectOfType<Map.AiSpawnGenerator>().onAiSpawn;
                SpawnHumanEvent -= FindObjectOfType<Map.HumanSpawnGenerator>().onHumanSpawn;
            }
            if (Tunnel.CollisionManager.Instance)
            {
                ChangeDirectionEvent -= Tunnel.CollisionManager.Instance.onChangeDirection;
            }
        }
    }
}
