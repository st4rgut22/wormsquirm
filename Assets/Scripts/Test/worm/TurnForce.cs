using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnForce : MonoBehaviour
{
    [SerializeField]
    Rigidbody head;

    [SerializeField]
    float forceMagnitude;

    [SerializeField]
    float torqueMagnitude;

    [SerializeField]
    Rigidbody ring;

    Rigidbody moveRigidbody;

    Vector3 startPosition;

    Vector3 forceVector;

    private void Start()
    {
        forceVector = Vector3.zero;
        moveRigidbody = head;
        startPosition = head.position;
    }

    private void setForceVector()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            forceVector = Vector3.up;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            forceVector = Vector3.down;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            forceVector = Vector3.left;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            forceVector = Vector3.right;
        }
        else 
        {
            forceVector = Vector3.zero;
        }
    }

    private void setRigidbody()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            moveRigidbody = head;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            moveRigidbody = ring;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        setRigidbody();
        if (Input.GetKeyDown(KeyCode.T))
        {
            moveRigidbody.AddTorque(new Vector3(100, 0, 0));
        }
    }
}
