using TurnBasedBoard;
using UnityEngine;

// Validates a proposed grid destination: must stay inside the play area
// (checked against both the real visual board, CellScaleController, and the
// abstract GridMap if one also exists -- either is optional, but at least
// one should be present or bounds aren't enforced at all) and must not land
// on a cell an enemy's collider currently overlaps. Uses a box covering the
// full cell footprint (not a small sphere at the exact center) because
// enemies move continuously rather than snapping to grid points, so a
// narrow check could miss one sitting anywhere else in that same cell.
public static class MoveChecker
{
    public static bool IsValidMove(Vector3 destination, Collider mover = null)
    {
        if (GridMap.Instance != null && !GridMap.Instance.Contains(destination))
            return false;

        if (CellScaleController.Instance != null && !CellScaleController.Instance.Contains(destination))
            return false;

        Vector3 halfExtents = Vector3.one * (GameSetting.gridSize * 0.5f);
        Collider[] hits = Physics.OverlapBox(destination, halfExtents);
        foreach (Collider hit in hits)
        {
            if (hit == mover) continue;
            if (hit.GetComponent<EnemyBehavior>() != null)
                return false;
        }

        return true;
    }
}
