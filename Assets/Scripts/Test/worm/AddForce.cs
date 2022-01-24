using UnityEngine;

public class AddForce : MonoBehaviour
{
    [SerializeField]
    private Rigidbody head;

    [SerializeField]
    private float FORCE_MAGNITUDE = 1f;

    [SerializeField]
    private Transform destination;

    Vector3 directionalVector = Vector3.zero;
    float lastSqrMag = -1;

    private void FixedUpdate()
    {
        print("direction vector is " + directionalVector);
        head.velocity = directionalVector * FORCE_MAGNITUDE;
    }

    // Update is called once per frame
    void Update()
    {
        // check the current sqare magnitude
        float sqrMag = (destination.position - transform.position).sqrMagnitude;

        // check this against the lastSqrMag
        // if this is greater than the last,
        // rigidbody has reached target and is now moving past it
        if (sqrMag > lastSqrMag)
        {
            // rigidbody has reached target and is now moving past it
            // stop the rigidbody by setting the velocity to zero
            directionalVector = Vector3.zero;
        }
        else
        {
            directionalVector = (destination.position - transform.position).normalized;
        }

        // make sure you update the lastSqrMag
        lastSqrMag = sqrMag;
    }
}
