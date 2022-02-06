using System.Collections;
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

        [SerializeField]
        private float maxVelocity;

        [SerializeField]
        private float angularDrag;

        [SerializeField]
        private float originalAngularDrag;

        [SerializeField]
        private bool isAngularDragSet;

        private new void Awake()
        {
            base.Awake();
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
            if (waypoint.move == MoveType.ENTRANCE || waypoint.move == MoveType.CENTER || waypoint.move == MoveType.EXIT)
            {
                Vector3 torqueUnitVector = Dir.DirectionForce.getTorqueVectorFromDirection(dirPair);
                Vector3 torqueVector = torqueUnitVector * torqueMagnitude;
                print("apply torque vector " + torqueVector + " for dirPair " + dirPair.prevDir + " , " + dirPair.curDir + " on move " + waypoint.move);
                torqueBody.AddTorque(torqueVector);
            }
        }   
    }
}