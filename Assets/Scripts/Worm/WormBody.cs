using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormBody : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody ring;

    [SerializeField]
    protected Rigidbody head;

    [SerializeField]
    protected Rigidbody neck;

    protected bool isLeadingTunnel = true; // if worm is actively creating a new tunnel (instead of traveling in a pre-existing tunnel)

    protected string wormId = "fakeId"; // TEMPORARY, later assign ids from worm manager
}
