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

        Renderer CubemapRenderer;
        Camera mainCamera;

        const float EDGE_LENGTH = .85f; // excludes the edge itself which has thickness
        float cellLength;

        Dictionary<string, GameObject> wormCellDict; // <worm id, cube cell>

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
            cubeWormInstance.transform.parent = gameObject.transform;
            cubeWormInstance.transform.localEulerAngles = Vector3.zero;
            cubeWormInstance.transform.localScale = new Vector3(cellLength, cellLength, cellLength);
            cubeWormInstance.transform.localPosition = cubemapPos;

            wormCellDict[wormId] = cubeWormInstance;            
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void OnDisable()
        {
            SpawnGenerator.UpdateObstacleEvent -= updateMap;
        }
    }
}