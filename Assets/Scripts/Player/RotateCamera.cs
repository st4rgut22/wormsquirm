using UnityEngine;
using System;
using System.Collections.Generic;

namespace Player
{

    public class RotateCamera : MonoBehaviour
    {
        [SerializeField]
        private Transform rotateParent;

        [SerializeField]
        private float degreesPerSec = 50; // how fast to lerp the camera

        [SerializeField]
        private float moveSpeed = .001f;

        [SerializeField]
        private float maxMoveDist = .1f;

        private float distanceFromCamOrigin = 0; // distnace from camera's default position

        private bool isCollide = false;

        private void OnTriggerStay(Collider other)
        {
            if (Tunnel.Type.isTagTunnel(other.tag)) // if touching tunnel move closer to the worm and away from the tunnel
            {
                isCollide = true;
                Vector3 deltaPosInWormDir = -transform.up * moveSpeed;
                distanceFromCamOrigin += moveSpeed;
                transform.position += deltaPosInWormDir;
                rotateParent.Rotate(new Vector3(0, degreesPerSec, 0) * Time.deltaTime);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (Tunnel.Type.isTagTunnel(other.tag))
            {
                isCollide = false;
            }
        }

        private void Update()
        {
            if (!isCollide && distanceFromCamOrigin > 0) // if worm is not touching tunnel and the camera pos is close to the worm
            {
                Vector3 deltaPosAwayWormPos = transform.up * moveSpeed;
                distanceFromCamOrigin -= moveSpeed;
                transform.position += deltaPosAwayWormPos;
            }
        }
    }

}