using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Test
{
    public class WormCollider : GenericSingletonClass<WormCollider>
    {
        public delegate void initiateAstar();
        public event initiateAstar initiateAstarEvent;

        public delegate void destroyTunnelNetwork();
        public event destroyTunnelNetwork destroyTunnelNetworkEvent;

        private bool isGameReset; // determine the state of the game

        const string GOAL_TAG = "Goal";

        private void Awake()
        {
            isGameReset = false;
        }

        private void OnEnable()
        {
            initiateAstarEvent += Map.MapLandmarks.Instance.onInitiateLandmarks;
            destroyTunnelNetworkEvent += Tunnel.TunnelManager.Instance.onDestroyTunnelNetwork;
        }

        // Start is called before the first frame update
        void Start()
        {
            initiateAstarEvent();
        }

        public void onInitCheckpointList(List<Checkpoint> checkpointList)
        {
            isGameReset = false;
        }

        private void OnCollisionEnter(Collision collision) 
        {
            if (collision.gameObject.tag == GOAL_TAG && !isGameReset) // game reset flag is used to make sure we only reset the game once
            {
                isGameReset = true;
                SceneManager.LoadScene(0);
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Map.MapLandmarks>())
            {
                initiateAstarEvent -= Map.MapLandmarks.Instance.onInitiateLandmarks;
            }
            if (FindObjectOfType<Tunnel.TunnelManager>())
            {
                destroyTunnelNetworkEvent -= Tunnel.TunnelManager.Instance.onDestroyTunnelNetwork;
            }
        }
    }

}