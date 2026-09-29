using UnityEngine;

public static class GameSetting
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public const float gridSize = 1.0f; // Size of each grid cell

    // Default board dimensions (BoardGame's CellScaleController sources its
    // own columns/rows defaults from these instead of hardcoding them).
    public const int defaultColumns = 15;
    public const int defaultRows = 15;

    // Default spawn cell, as a board cell index (not world position) --
    // center of a defaultColumns x defaultRows board.
    public static readonly Vector2Int defaultBornCell = new Vector2Int(defaultColumns / 2, defaultRows / 2);
}
