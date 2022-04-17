using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDestroyWorm : MonoBehaviour
{
    private Transform BoneParent;

    private bool isTriggered;

    [SerializeField]
    private GameObject WormSegment;

    [SerializeField]
    private GameObject WormMidSegment;

    const float explosionRadius = 50.0f;
    const float explosionPower = 20.0f;


    // Start is called before the first frame update
    void Start()
    {
        BoneParent = transform;
        isTriggered = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private List<Rigidbody> spawnWormSegments()
    {
        List<Rigidbody> WormSegmentList = new List<Rigidbody>();
        while (BoneParent.childCount > 0)
        {
            GameObject wormSegmentGO = Instantiate(WormSegment, BoneParent.position, BoneParent.rotation);
            BoneParent = BoneParent.GetChild(0);
            print("bone parent is " + BoneParent.name);
            Rigidbody wormSegmentRigidbody = wormSegmentGO.GetComponent<Rigidbody>();
            WormSegmentList.Add(wormSegmentRigidbody);
        }
        return WormSegmentList;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "wall" && !isTriggered)
        {
            Vector3 explosionPosition = WormMidSegment.transform.position;

            List<Rigidbody> wormSegmentList = spawnWormSegments();
            isTriggered = true;
            wormSegmentList.ForEach((Rigidbody wormSegment) =>
            {
                wormSegment.AddExplosionForce(explosionPower, explosionPosition, explosionRadius);
            });
        }
    }
}
