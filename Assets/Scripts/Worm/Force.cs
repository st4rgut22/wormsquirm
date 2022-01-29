using UnityEngine;

namespace Worm
{
    public class Force : WormBody
    {
        [SerializeField]
        private Rigidbody torqueBody;

        [SerializeField]
        private float forceMagnitude;

        [SerializeField]
        private float torqueMagnitude;

        private Rigidbody movingRigidbody;

        [SerializeField]
        private float maxVelocity;

        private Vector3 directionalVector = Vector3.zero;

        // Start is called before the first frame update
        void Start()
        {
            movingRigidbody = ring;
        }

        private void addForce(Vector3 forceVector, Rigidbody rigidbody)
        {
            float maxSpeed = rigidbody.velocity.getMaxValue();
            if (maxSpeed < maxVelocity)
            {
                print("adding force forceVector is " + forceVector + " force magnitude is " + forceMagnitude + " product is " + (forceMagnitude * forceVector));
                rigidbody.AddForce(forceVector * forceMagnitude);
            }
            else
            {
                print("braking too fast!");
            }
        }

        private void addTorque(Vector3 torqueVector, Rigidbody rigidbody)
        {
            print("adding torque " + torqueVector);
            rigidbody.AddTorque(torqueVector);
        }

        /**
         * Applies a force on the Worm's' head.
         * 
         * @forceVector direction of the force
         * @isDecision whether worm head has crossed a decision plane that results in the creation of corner on next block
         */
        public void onForce(Rigidbody rigidbody, Vector3 forceVector)
        {
            addForce(forceVector, rigidbody);
        }

        /**
         * Applies a rotational force on the Worm's head and increases the upperbound of speed to get worm past the corner
         */
        public void onTorque(DirectionPair dirPair, Waypoint waypoint)
        {
            if (waypoint.move != MoveType.EXIT && waypoint.move != MoveType.OFFSET)
            {       
                Vector3 torqueUnitVector = Dir.DirectionForce.getTorqueVectorFromDirection(dirPair);
                Vector3 torqueVector = torqueUnitVector * torqueMagnitude;
                print("apply torque vector " + torqueVector);
                addTorque(torqueVector, torqueBody);
            }
        }   
    }
}