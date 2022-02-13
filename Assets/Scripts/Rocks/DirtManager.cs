using System.Collections.Generic;
using UnityEngine;

public class DirtManager : GenericSingletonClass<DirtManager>
{
    [SerializeField]
    Transform DirtParent;

    [SerializeField]
    GameObject Rock;

    [SerializeField]
    private float ForceMagnitude;

    [SerializeField]
    private float RandomNoise; // the randomness of rock force vectors, must be between 0 (no randomness), 1 (max Randomness)

    //TODO: save reference to rock component to reduce compute

    const int EJECT_FORCE_DIR_COUNT = 8; // the number of perpendicular directions

    int ejectRockDirIdx; // tracks which direction rocks should be ejected, should take turns ejecting in different directions
    Dictionary<Direction, List<Vector3>> ForceDirectionVectorDictionary; // <Direction of tunnel (opposite worm travel), trajectory of rocks>

    private new void Awake()
    {
        base.Awake();
        ForceDirectionVectorDictionary = new Dictionary<Direction, List<Vector3>>();
        ejectRockDirIdx = 0;
        initializeEjectVectors();
    }

    /**
     * Precalculate the general ejection direction of rocks for every direction the tunnels may grow. 
     * For each growth direction calculate 8 ejection direction vectors pointing at the edges and centers of each side of the tunnel
     */
    private void initializeEjectVectors()
    {
        Dir.Base.directionList.ForEach((Direction tunnelDirection) =>
        {
            Direction[] perpendicularDirections = Dir.Base.getPerpendicularDirections(tunnelDirection);
            ForceDirectionVectorDictionary[tunnelDirection] = new List<Vector3>();

            Direction prevPerpendicularDirection = Direction.None; 
            for (int i=0;i<perpendicularDirections.Length;i++)
            {
                Direction prependicularDirection = perpendicularDirections[i];
                if (prevPerpendicularDirection != Direction.None)
                {
                    Vector3 cornerEjectForceUnitVector = calculateDirectionVector(prevPerpendicularDirection, prependicularDirection);
                    ForceDirectionVectorDictionary[tunnelDirection].Add(cornerEjectForceUnitVector);
                }
                if (i == perpendicularDirections.Length - 1)
                {
                    Vector3 finalCornerEjectForceUnitVector = calculateDirectionVector(prependicularDirection, perpendicularDirections[0]);
                    ForceDirectionVectorDictionary[tunnelDirection].Add(finalCornerEjectForceUnitVector);
                }
                prevPerpendicularDirection = prependicularDirection;
                Vector3 perpendicularEjectForceUnitVector = calculateDirectionVector(tunnelDirection, prependicularDirection);
                ForceDirectionVectorDictionary[tunnelDirection].Add(perpendicularEjectForceUnitVector);
            }
        });
    }

    /**
     * Calculate a new direction vector as the sum of two previous vectors
     */
    private Vector3 calculateDirectionVector(Direction direction1, Direction direction2)
    {
        Vector3 dirVector1 = Dir.CellDirection.getUnitVectorFromDirection(direction1);
        Vector3 dirVector2 = Dir.CellDirection.getUnitVectorFromDirection(direction2);
        return (dirVector1 + dirVector2).normalized;
    }

    /**
     * Gets the direction the rock should be ejected by adding the opposite of tunnel growth direction and one of its perpendicular vectors
     * 
     * @direction the direction opposite of travel
     */
    private Vector3 getEjectRockDirVector(Direction direction)
    {
        ejectRockDirIdx = (ejectRockDirIdx + 1) % EJECT_FORCE_DIR_COUNT;
        Vector3 forceDirVector = ForceDirectionVectorDictionary[direction][ejectRockDirIdx];
        float noiseFactor = Random.Range(1.0f - RandomNoise, 1.0f);
        Vector3 randomizedForceVector = new Vector3(forceDirVector.x + noiseFactor, forceDirVector.y + noiseFactor, forceDirVector.z + noiseFactor);
        return randomizedForceVector;
    }

    /**
     * When digging is happening, dirt should be produced
     */
    public void onDig(Vector3 digLocation, Direction digDirection)
    {
        Vector3 forceDirForceVector = getEjectRockDirVector(digDirection);
        GameObject rock = Instantiate(Rock, digLocation, Quaternion.identity, DirtParent);
        Rigidbody rockRigidbody = rock.GetComponent<Rigidbody>();
        Vector3 force = forceDirForceVector * ForceMagnitude;
        rockRigidbody.AddForce(force);
    }
}
