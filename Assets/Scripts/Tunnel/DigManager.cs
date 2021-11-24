using UnityEngine;
using System.Collections.Generic;

public class DigManager : MonoBehaviour
{
    List<DirectionPair> directionList;
    private bool isTurnEligible; // if straight tunnel is a multiple of BLOCK_SIZE it can change direction

    public delegate void CreateTunnel(DirectionPair directionPair);
    public event CreateTunnel CreateTunnelEvent;

    // Start is called before the first frame update
    void OnEnable()
    {
        FindObjectOfType<Worm.Controller>().DigEvent += onDig;
    }

    private void Awake()
    {
        directionList = new List<DirectionPair>();
        isTurnEligible = true; // initially eligible for turn because no previous tunnel exists
    }

    /**
     * Check if tunnel is multiple of BLOCK_SIZE and direction list is populated to 
     * determine direction change
     */
    private void FixedUpdate()
    {
        if (isTurning())
        {
            DirectionPair directionPair = getDirectionPair();
            CreateTunnelEvent(directionPair); // rotate tunnel in the direction
            directionList.Clear();
        }
    }

    /**
     * Get the last input direction pair when changing directions 
     */
    public DirectionPair getDirectionPair()
    {
        return directionList[directionList.Count - 1];
    }

    /**
     * Queue the direction pair <prevDir, curDir> until turn is eligible
     */
    public void addDirectionPairToList(Direction prevDirection, Direction curDirection)
    {
        directionList.Add(new DirectionPair(prevDirection, curDirection));
    }

    /**
     * Received when the active tunnel is a multiple of block size making it eligible for a direction change
     */
    public void onBlockSize(bool isBlockSizeMultiple)
    {
        isTurnEligible = isBlockSizeMultiple;
        //print("onBlockSize event received, is turn eligible? " + isTurnEligible);
    }

    /**
     * Subscribes to the user input to worm's direction
     */
    private void onDig(Direction direction, Direction prevDirection, bool isDirectionChanged)
    {
        if (isDirectionChanged)
        {
            addDirectionPairToList(prevDirection, direction);
        }
    }

    /**
     * If tunnel is multiple of block size, and input for direction change exists, return true
     */
    public bool isTurning()
    {
        return isTurnEligible && directionList.Count > 0;
    }

    public void clearDirectionList()
    {
        directionList.Clear();
    }

    void OnDisable()
    {
        if (FindObjectOfType<Worm.Controller>()) FindObjectOfType<Worm.Controller>().DigEvent -= onDig;
    }
}
