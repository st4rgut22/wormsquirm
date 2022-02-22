using UnityEngine;
using System.Collections.Generic;

namespace Test
{
    public class ExampleNetwork : MonoBehaviour
    {
        [SerializeField]
        private Network tunnelNetwork;

        public enum Network
        {
            initThreeIntersectLoop,
            initThreeLoopCorner,
            initZigzag,
            initConsecutiveTurns,
            initFirstSegmentLengthOne,
            initZigZagPerpendicular,
            initImmediateTurn,
            initSimpleTurn,
            initDifferentDirection,
            snakeEatsHeadTurn,
            snakeEatsHeadStraight,
            snakeEatsHeadImmediateTurn,
            snakeEatsHeadCornerTurn,
            debugTurn
        }

        /**
         * Get the selected tunnel network for the worm to generate
         */
        public List<Checkpoint> getNetwork()
        {
            switch(tunnelNetwork)
            {
                case Network.initThreeIntersectLoop:
                    return initThreeIntersectLoop();
                case Network.initZigzag:
                    return initZigzag();
                case Network.initConsecutiveTurns:
                    return initConsecutiveTurns();
                case Network.initFirstSegmentLengthOne:
                    return initFirstSegmentLengthOne();
                case Network.initZigZagPerpendicular:
                    return initZigZagPerpendicular();
                case Network.initImmediateTurn:
                    return initImmediateTurn();
                case Network.initSimpleTurn:
                    return initSimpleTurn();
                case Network.debugTurn:
                    return debugTurn();
                case Network.initDifferentDirection:
                    return initDifferentDirection();
                case Network.snakeEatsHeadTurn:
                    return snakeEatsHead();
                case Network.snakeEatsHeadStraight:
                    return snakeEatsHeadStraight();
                case Network.snakeEatsHeadImmediateTurn:
                    return snakeEatsHeadImmediateTurn();
                case Network.snakeEatsHeadCornerTurn:
                    return snakeEatsHeadCornerJunction();
                default:
                    throw new System.Exception(tunnelNetwork + " is not a valid tunnel network");
            }
        }

        private List<Checkpoint> initThreeIntersectLoop()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 5);
            Checkpoint cp1 = new Checkpoint(Direction.Right, 5);
            Checkpoint cp2 = new Checkpoint(Direction.Up, 5);
            Checkpoint cp3 = new Checkpoint(Direction.Left, 11);
            Checkpoint cp4 = new Checkpoint(Direction.Down, 5);
            Checkpoint cp5 = new Checkpoint(Direction.Right, 5);
            Checkpoint cp6 = new Checkpoint(Direction.Up, 11);
            List<Checkpoint> threeIntersectLoopStraight = new List<Checkpoint>() { cp0, cp1, cp2, cp3, cp4, cp5, cp6 };

            return threeIntersectLoopStraight;
        }

        private List<Checkpoint> initZigzag()
        { 
            Checkpoint cp0 = new Checkpoint(Direction.Up, 5);
            Checkpoint cp1 = new Checkpoint(Direction.Right, 1);
            Checkpoint cp2 = new Checkpoint(Direction.Up, 1);
            List<Checkpoint> zigzag = new List<Checkpoint>() { cp0, cp1, cp2 };
            return zigzag;
        }

        private List<Checkpoint> initConsecutiveTurns()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 5);
            Checkpoint cp1 = new Checkpoint(Direction.Right, 0);
            Checkpoint cp2 = new Checkpoint(Direction.Up, 0);
            Checkpoint cp3 = new Checkpoint(Direction.Right, 0);
            Checkpoint cp4 = new Checkpoint(Direction.Forward, 0);
            Checkpoint cp5 = new Checkpoint(Direction.Up, 0);
            Checkpoint cp6 = new Checkpoint(Direction.Right, 0);
            Checkpoint cp7 = new Checkpoint(Direction.Forward, 0);
            Checkpoint cp8 = new Checkpoint(Direction.Up, 0);
            Checkpoint cp9 = new Checkpoint(Direction.Right, 0);
            Checkpoint cp10 = new Checkpoint(Direction.Forward, 0);
            List<Checkpoint> zigzag = new List<Checkpoint>() { cp0, cp1, cp2, cp3, cp4, cp5, cp6, cp7, cp8, cp9, cp10 };
            return zigzag;
        }

        private List<Checkpoint> initFirstSegmentLengthOne()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 1);
            Checkpoint cp1 = new Checkpoint(Direction.Right, 5);
            Checkpoint cp2 = new Checkpoint(Direction.Up, 1);
            Checkpoint cp3 = new Checkpoint(Direction.Right, 4);
            List<Checkpoint> zigzag = new List<Checkpoint>() { cp0, cp1, cp2, cp3 };
            return zigzag;
        }

        private List<Checkpoint> initZigZagPerpendicular()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 2);
            Checkpoint cp1 = new Checkpoint(Direction.Right, 0);
            Checkpoint cp2 = new Checkpoint(Direction.Forward, 0);
            Checkpoint cp3 = new Checkpoint(Direction.Up, 5);
            List<Checkpoint> zigzag = new List<Checkpoint>() { cp0, cp1, cp2, cp3 };
            return zigzag;
        }

        // start off going right instead of up
        private List<Checkpoint> initDifferentDirection()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Right, 5);
            Checkpoint cp1 = new Checkpoint(Direction.Up, 5);
            List<Checkpoint> differentDirection = new List<Checkpoint>() { cp0, cp1 };
            return differentDirection;
        }

        // start off going right instead of up
        private List<Checkpoint> snakeEatsHead()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 6);
            Checkpoint cp1 = new Checkpoint(Direction.Left, 3);
            Checkpoint cp2 = new Checkpoint(Direction.Down, 4);
            Checkpoint cp3 = new Checkpoint(Direction.Right, 3);
            Checkpoint cp4 = new Checkpoint(Direction.Up, 2);
            Checkpoint cp5 = new Checkpoint(Direction.Left, 5);
            List<Checkpoint> snakeEatsHeadList = new List<Checkpoint>() { cp0, cp1, cp2, cp3, cp4, cp5 };
            return snakeEatsHeadList;
        }

        // start off going right instead of up
        private List<Checkpoint> snakeEatsHeadStraight()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 3);
            Checkpoint cp1 = new Checkpoint(Direction.Left, 3);
            Checkpoint cp2 = new Checkpoint(Direction.Down, 1);
            Checkpoint cp3 = new Checkpoint(Direction.Right, 3);
            Checkpoint cp4 = new Checkpoint(Direction.Up, 6);
            List<Checkpoint> snakeEatsHeadList = new List<Checkpoint>() { cp0, cp1, cp2, cp3, cp4 };
            return snakeEatsHeadList;
        }

        // snake immediately turns immediately after entering existing tunnel
        private List<Checkpoint> snakeEatsHeadImmediateTurn()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 3);
            Checkpoint cp1 = new Checkpoint(Direction.Left, 3);
            Checkpoint cp2 = new Checkpoint(Direction.Down, 1);
            Checkpoint cp3 = new Checkpoint(Direction.Right, 3);
            Checkpoint cp4 = new Checkpoint(Direction.Up, 0);
            Checkpoint cp5 = new Checkpoint(Direction.Left, 5);
            List<Checkpoint> snakeEatsHeadList = new List<Checkpoint>() { cp0, cp1, cp2, cp3, cp4, cp5 };
            return snakeEatsHeadList;
        }
        
        // start off going right instead of up
        private List<Checkpoint> snakeEatsHeadCornerJunction()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 3);
            Checkpoint cp1 = new Checkpoint(Direction.Left, 3);
            Checkpoint cp2 = new Checkpoint(Direction.Down, 1);
            Checkpoint cp3 = new Checkpoint(Direction.Right, 3);
            Checkpoint cp4 = new Checkpoint(Direction.Up, 1);
            Checkpoint cp5 = new Checkpoint(Direction.Right, 3);
            List<Checkpoint> snakeEatsHeadList = new List<Checkpoint>() { cp0, cp1, cp2, cp3, cp4, cp5 };
            return snakeEatsHeadList;
        }

        private List<Checkpoint> initImmediateTurn()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 1);
            Checkpoint cp1 = new Checkpoint(Direction.Right, 10);
            List<Checkpoint> immediateTurn = new List<Checkpoint>() { cp0, cp1 };
            return immediateTurn;
        }

        private List<Checkpoint> initSimpleTurn()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 5);
            Checkpoint cp1 = new Checkpoint(Direction.Forward, 5);
            List<Checkpoint> problemTurn = new List<Checkpoint>() { cp0, cp1 };
            return problemTurn;
        }

        private List<Checkpoint> debugTurn()
        {
            Checkpoint cp0 = new Checkpoint(Direction.Up, 1);
            Checkpoint cp1 = new Checkpoint(Direction.Right, 1);
            Checkpoint cp2 = new Checkpoint(Direction.Up,0);
            Checkpoint cp3 = new Checkpoint(Direction.Right, 2);
            //Checkpoint cp4 = new Checkpoint(Direction.Up, 1);
            List<Checkpoint> problemTurn = new List<Checkpoint>() { cp0, cp1, cp2, cp3 };//, cp4 };
            return problemTurn;
        }
    }
}