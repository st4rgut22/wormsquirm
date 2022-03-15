using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class HumanSpawnGenerator : SpawnGenerator
    {
        public delegate void SpawnPlayerWorm(string wormId);
        public event SpawnPlayerWorm SpawnPlayerWormEvent;

        [SerializeField]
        private int playerCount; // the # of human players (equals 1 until multiplayer support arrives)

        private int humanSpawnCount; // used to id human controlled worms TODO: multiplayer will have multiple human-controlled worms

        private const string HUMAN_WORM_ID = "Player";

        private new void Awake()
        {
            base.Awake();
            humanSpawnCount = 0;
        }

        private new void OnEnable()
        {
            base.OnEnable();
            SpawnPlayerWormEvent += FindObjectOfType<Worm.Factory.WormPlayerFactory>().onSpawn;
        }

        public override void onStartGame(GameMode gameMode)
        {
            if (gameMode == GameMode.Solo) // create a single human worm, no ai worms (TESTING MODE)
            {
                SpawnPlayerWormEvent(HUMAN_WORM_ID);
            }
            else if (gameMode == GameMode.ReachTheGoal) // create human worm and ai worms
            {
                SpawnPlayerWormEvent(HUMAN_WORM_ID);
            }
            else
            {
                throw new System.Exception("the game mode " + gameMode + " is not supported yet");
            }
        }

        /**
         * Player is destroyed and spawned again
         */
        public void onRespawnHuman(Vector3Int currentCell)
        {
            Obstacle WormObstacle = WormObstacleDict[currentCell];
            onRemoveWorm(currentCell); // wait for removal confirmation before spawning again
            onHumanSpawn(WormObstacle.obstacleType, WormObstacle.obstacleId);
        }

        public void onHumanSpawn(ObstacleType wormType, string id)
        {
            SpawnPlayerWormEvent(id);
        }

        private new void OnDisable()
        {
            base.OnDisable();
            SpawnPlayerWormEvent -= FindObjectOfType<Worm.Factory.WormPlayerFactory>().onSpawn;
        }
    }

}