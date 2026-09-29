using UnityEngine;

public class RangeCheck : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static int measureGrid(Vector3 start, Vector3 end)
    {
        int d_x = Mathf.RoundToInt((end.x - start.x) / GameSetting.gridSize);
        // CellScaleController, chamoving and the visible board all use the
        // XY plane. Keep attack range on that same plane so vertical rows
        // contribute to distance exactly like horizontal columns.
        int d_y = Mathf.RoundToInt((end.y - start.y) / GameSetting.gridSize);
        int distance = Mathf.Abs(d_x) + Mathf.Abs(d_y);

        return distance;
    }
}
