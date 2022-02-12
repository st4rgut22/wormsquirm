using UnityEngine;

public class DirtManager : GenericSingletonClass<DirtManager>
{
    [SerializeField]
    Transform DirtParent;

    [SerializeField]
    GameObject Rock;

    [SerializeField]
    float ForceMagnitude;

    /**
     * When digging is happening, dirt should be produced
     */
    public void onDig(Vector3 digLocation, Direction digDirection)
    {
        Direction forceDirection = Dir.Base.getOppositeDirection(digDirection);
        Vector3 forceVector = Dir.CellDirection.getUnitVectorFromDirection(forceDirection);

        GameObject rock = Instantiate(Rock, digLocation, Quaternion.identity, DirtParent);
        Rigidbody rockRigidbody = rock.GetComponent<Rigidbody>();
        Vector3 force = forceVector * ForceMagnitude;
        rockRigidbody.AddForce(force);
    }
}
