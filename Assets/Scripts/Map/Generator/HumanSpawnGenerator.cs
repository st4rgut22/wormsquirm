using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class HumanSpawnGenerator : SpawnGenerator
    {
        public delegate void SpawnPlayerWorm(string wormId, string chaseWormId);
        public event SpawnPlayerWorm SpawnPlayerWormEvent;

        [SerializeField]
        private int playerCount; // the # of human players (equals 1 until multiplayer support arrives)

        private Vector3Int initLocation; // if undefined will use random location

        private int humanSpawnCount; // used to id human controlled worms TODO: multiplayer will have multiple human-controlled worms

        public const string HUMAN_WORM_ID = "Player";

        private new void Awake()
        {
            base.Awake();
            humanSpawnCount = 0;
        }

        private void Start()
        {
            initPositionDict[HUMAN_WORM_ID] = new Vector3Int(5, 0, 5);
        }

        private new void OnEnable()
        {
            SpawnPlayerWormEvent += FindObjectOfType<Worm.Factory.WormPlayerFactory>().onSpawn;
        }

        public override void onStartGame(GameMode gameMode)
        {
            if (playerCount > 0)
            {
                SpawnPlayerWormEvent(HUMAN_WORM_ID, chaseWormId);
            }
        }

        /**
         * Player is destroyed and spawned again
         */
        public void onRespawnHuman(string wormId)
        {
            onRemoveWorm(wormId); // wait for removal confirmation before spawning again
            onHumanSpawn(wormId);
        }

        public void onHumanSpawn(string id)
        {
            SpawnPlayerWormEvent(id, chaseWormId);
        }

        private new void OnDisable()
        {
            SpawnPlayerWormEvent -= FindObjectOfType<Worm.Factory.WormPlayerFactory>().onSpawn;
        }
    }

}