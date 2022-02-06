using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tunnel
{
    public class Obstacle : MonoBehaviour
    {

        // Start is called before the first frame update
        void Start()
        {
            Color transparentColor = new Color(250, 0, 0, 1f);
            gameObject.GetComponent<Renderer>().material.color = transparentColor;
        }
    }

}