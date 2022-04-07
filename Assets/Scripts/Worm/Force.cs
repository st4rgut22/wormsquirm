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
        private float brakeSpeed;       // how fast the player should decelerate

        [SerializeField]
        private float slowAngularDrag;

        private const float angularDrag = 0;

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

            float maxSpeed = rigidbody.velocity.getMaxValue();
            if (maxSpeed < maxVelocity)
            {
                //print("adding force forceVector is " + forceVector + " force magnitude is " + forceMagnitude + " product is " + (forceMagnitude * forceVector));
                rigidbody.AddForce(forceVector * forceMagnitude);
            }
            else
            {
                rigidbody.AddForce(-brakeSpeed * forceVector * forceMagnitude); // apply the brakes until velocity is under maxVelocity
            }
        }

        /**
         * When player has completed a turn, slow the velocity of turn so they dont accidentally turn while traveling straight
         */
        public void onCompleteTurn(string wormId, Direction direction)
        {
            torqueBody.angularDrag = slowAngularDrag;
        }

        /**
         * When a player makes a decision slow the max velocity to prevent the player from overshooting any holes they need to turn into
         * 
         */
        public void onDecision(bool isStraightTunnel, Direction direction, Tunnel.Tunnel tunnel)
        {
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
                torqueBody.angularDrag = angularDrag;   // restore the original angular drag while going through a turn so player doesnt get stuck on corner

                Vector3 torqueUnitVector = Dir.DirectionForce.getTorqueVectorFromDirection(dirPair);
                Vector3 torqueVector = torqueUnitVector * torqueMagnitude;
                print("apply torque vector " + torqueVector + " for dirPair " + dirPair.prevDir + " , " + dirPair.curDir + " on move " + waypoint.move);
                torqueBody.AddTorque(torqueVector);
            }
            else
            {
                torqueBody.angularDrag = slowAngularDrag; // if not going through a turn turn more slowly to avoid immediately triggering turn decision
            }
        }
    }
}