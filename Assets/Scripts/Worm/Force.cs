using System.Collections;
using UnityEngine;

namespace Worm
{
    public class Force : WormBody
    {
        [SerializeField]
        private Rigidbody torqueBody;

        [SerializeField]
        private Rigidbody torqueBody2;

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

        [SerializeField]
        private float brakeSpeed;       // how fast the player should decelerate

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
            print("MAX SPEED IS " + maxSpeed);
            if (maxSpeed < maxVelocity)
            {
                print("adding force forceVector is " + forceVector + " force magnitude is " + forceMagnitude + " product is " + (forceMagnitude * forceVector));
                rigidbody.AddForce(forceVector * forceMagnitude);
            }
            else
            {
                print("going to fast applying brakes");
                rigidbody.AddForce(-brakeSpeed * forceVector * forceMagnitude); // apply the brakes until velocity is under maxVelocity
            }
        }

        /**
         * When player has completed a turn restore the original max velocity
         */
        public void onCompleteTurn(string wormId, Direction direction)
        {
            //forceFactor = 1.0f;
        }

        /**
         * When a player makes a decision slow the max velocity to prevent the player from overshooting any holes they need to turn into
         */
        public void onDecision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel)
        {
            //forceFactor = forceFactor;
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
            if (waypoint.move == MoveType.ENTRANCE || waypoint.move == MoveType.CENTER || waypoint.move == MoveType.EXIT) // || waypoint.move == MoveType.MIDPOINT)
            {
                Vector3 torqueUnitVector = Dir.DirectionForce.getTorqueVectorFromDirection(dirPair);
                Vector3 torqueVector = torqueUnitVector * torqueMagnitude;
                print("apply torque vector " + torqueVector + " for dirPair " + dirPair.prevDir + " , " + dirPair.curDir + " on move " + waypoint.move);
                torqueBody.AddTorque(torqueVector);
            }
        }   
    }
}