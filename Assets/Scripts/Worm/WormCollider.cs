using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Test
{
    public class WormCollider : MonoBehaviour
    {
        public delegate void initiatePathfinding();
        public event initiatePathfinding initiatePathfindingEvent;

        public delegate void resetTunnelNetwork();
        public event resetTunnelNetwork resetTunnelNetworkEvent;

        private bool isGameReset; // determine the state of the game

        const string GOAL_TAG = "Goal";

        private void Awake()
        {
            isGameReset = false;
        }

        private void OnEnable()
        {
            initiatePathfindingEvent += Map.MapLandmarks.Instance.onInitiateLandmarks;
            resetTunnelNetworkEvent += Tunnel.TunnelManager.Instance.onResetTunnelNetwork;
        }

        // Start is called before the first frame update
        void Start()
        {
            initiatePathfindingEvent();
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
                resetTunnelNetworkEvent();
                SceneManager.LoadScene(0);
            }
        }

        private void OnDisable()
        {
            if (FindObjectOfType<Map.MapLandmarks>())
            {
                initiatePathfindingEvent -= Map.MapLandmarks.Instance.onInitiateLandmarks;
            }
            if (FindObjectOfType<Tunnel.TunnelManager>())
            {
                resetTunnelNetworkEvent -= Tunnel.TunnelManager.Instance.onResetTunnelNetwork;
            }
        }
    }

}