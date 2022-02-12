using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    const float existDuration = 3; // rock will exist for 3 seconds before disappearing

    private float timer;

    private void Awake()
    {
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > existDuration)
        {
            Destroy(gameObject);
        }
    }
}
