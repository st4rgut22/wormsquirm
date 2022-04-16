using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class Cubemap : MonoBehaviour
    {
        [SerializeField]
        Vector3 cubemapPosition;

        [SerializeField]
        GameObject cubeWorm;

        [SerializeField]
        private Transform CubeCells;

        [SerializeField]
        private float degreesPerSec = 10f;

        Renderer CubemapRenderer;

        const float EDGE_LENGTH = .85f; // excludes the edge itself which has thickness
        float cellLength;

        Dictionary<string, GameObject> wormCellDict; // <worm id, cube cell>

        Quaternion rotation;

        private string playerId;

        private static Quaternion UP_ROTATION = Quaternion.Euler(90, 0, 0);
        private static Quaternion DOWN_ROTATION = Quaternion.Euler(-90, 0, 0);
        private static Quaternion FORWARD_ROTATION = Quaternion.Euler(0, 0, 0);
        private static Quaternion BACK_ROTATION = Quaternion.Euler(0, 180, 0);
        private static Quaternion RIGHT_ROTATION = Quaternion.Euler(0, -90, 0);
        private static Quaternion LEFT_ROTATION = Quaternion.Euler(0, 90, 0);

        private void Awake()
        {
            wormCellDict = new Dictionary<string, GameObject>();
            cellLength = EDGE_LENGTH / (GameManager.MAP_LENGTH * 2);
        }

        private void OnEnable()
        {
            SpawnGenerator.UpdateObstacleEvent += updateMap;
        }

        // Start is called before the first frame update
        void Start()
        {
            CubemapRenderer = gameObject.GetComponent<Renderer>();
            CubemapRenderer.material.color = new Color(0, 0, 0, .7f);
        }

        public void setPlayerId(string wormId)
        {
            playerId = wormId; // player should be different color block
        }

        private Quaternion getRotationFromDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return UP_ROTATION;
                case Direction.Down:
                    return DOWN_ROTATION;
                case Direction.Right:
                    return RIGHT_ROTATION;
                case Direction.Left:
                    return LEFT_ROTATION;
                case Direction.Back:
                    return BACK_ROTATION;
                case Direction.Forward:
                    return FORWARD_ROTATION;
                default:
                    throw new System.Exception("No rotation " + direction);
            }
        }

        public void onSetInitDirection(Direction direction, string wormId, Vector3Int mappedInitialCell, Vector3Int initialCell)
        {
            rotation = getRotationFromDirection(direction);
        }

        /**
         * Set the map orientation given worm's direction on initialization or after complete turn
         */
        public void onSetDirection(Direction direction)
        {
            rotation = getRotationFromDirection(direction);
        }

        /**
         * Update the cube map as worms update cell position
         */
        public void updateMap(string wormId, Vector3Int wormCell)
        {
            if (wormCellDict.ContainsKey(wormId))
            {
                Destroy(wormCellDict[wormId]);
            }
            Vector3 cubemapPos = new Vector3(wormCell.x * cellLength, wormCell.y * cellLength, wormCell.z * cellLength);

            GameObject cubeWormInstance = Instantiate(cubeWorm);
            cubeWormInstance.transform.parent = CubeCells;
            cubeWormInstance.transform.localEulerAngles = Vector3.zero;
            cubeWormInstance.transform.localScale = new Vector3(cellLength, cellLength, cellLength);
            cubeWormInstance.transform.localPosition = cubemapPos;

            wormCellDict[wormId] = cubeWormInstance;
        }

        private void Update()
        {
            float speed = Time.deltaTime * degreesPerSec;
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotation, speed);
        }

        private void OnDisable()
        {
            SpawnGenerator.UpdateObstacleEvent -= updateMap;
        }
    }
}