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

        /**
         * Applies a force on the Worm's' head.
         * 
         * @forceVector direction of the force
         * @isDecision whether worm head has crossed a decision plane that results in the creation of corner on next block
         */
        public void onForce(Rigidbody rigidbody, Vector3 forceVector)
        {
            print("applying force in direction " + forceVector);

            float maxSpeed = rigidbody.velocity.getMaxValue();
            if (maxSpeed < maxVelocity)
            {
                print("adding force forceVector is " + forceVector + " force magnitude is " + forceMagnitude + " product is " + (forceMagnitude * forceVector));
                rigidbody.AddForce(forceVector * forceMagnitude);
            }
            else
            {
                print("braking too fast!"); // prevents the worm from exceeding its straight line velocity
            }
        }

        /**
         * Applies a force from user input to the Worm's head.
         * Similar to onTorque
         */
        public void onInputTorque(DirectionPair dirPair, float torqueMagnitude)
        {
            Vector3 torqueUnitVector = Dir.DirectionForce.getTorqueVectorFromDirection(dirPair);
            Vector3 torque = torqueMagnitude * torqueUnitVector;
            torqueBody.AddTorque(torque);
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