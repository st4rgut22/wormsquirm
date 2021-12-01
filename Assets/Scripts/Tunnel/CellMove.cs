using UnityEngine;

namespace Tunnel
{
    public class CellMove
    {
        public Vector3Int cellPosition { get; private set; } = Vector3Int.zero;
        public Vector3 startPosition { get; private set; } = Vector3.zero;
        public Vector3Int nextCell { get; private set; } = Vector3Int.zero;

        public CellMove(Tunnel tunnel, Direction curDirection)
        {
            if (tunnel != null)
            {
                cellPosition = tunnel.getLastCellPosition();
                startPosition = tunnel.getEgressPosition(curDirection); // get the correct egress position using the curDirection
                nextCell = Dir.Vector.getNextCellFromDirection(cellPosition, curDirection);
            }
        }

    }
}