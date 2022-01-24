using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputAddForce : MonoBehaviour
{
    [SerializeField]
    Rigidbody head;

    [SerializeField]
    float forceMagnitude;

    private void Start()
    {
        forceMagnitude = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            print("apply force");
            Vector3 force = new Vector3(forceMagnitude, 0, 0);
            head.AddForce(force, ForceMode.Acceleration);
        }
    }
}
