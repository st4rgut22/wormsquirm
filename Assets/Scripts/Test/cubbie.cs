using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubbie : MonoBehaviour
{
    private int rayLength = 10;

    private Vector3 raycastOrigin = new Vector3(0, .5f, -1.5f);

    // Start is called before the first frame update
    void Start()
    {
        RaycastHit hit;
        Vector3 raycastDir = Vector3.forward;
        bool isHit = Physics.Raycast(raycastOrigin, raycastDir, out hit);
        Debug.DrawRay(raycastOrigin, raycastDir * rayLength, Color.green, 30);

        if (!isHit) // clear path, holes are aligned hopefully  
        {
            print("didn't hit anything");
        }
        else
        {
            print("hit " + hit.transform.name);
        }

    }
}
