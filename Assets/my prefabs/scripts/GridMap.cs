using UnityEngine;

// Defines the fixed play-area bounds, in grid cells, centered on this
// GameObject's position. width/height should be odd so there's a true
// center cell for the character to spawn on. Movement happens on the XY
// plane (matching chamoving/RangeCheck's existing convention), so bounds
// are checked against x/y, not z.
public class GridMap : MonoBehaviour
{
    public static GridMap Instance { get; private set; }

    [Tooltip("Number of grid cells along X. Should be odd so there's a center column.")]
    public int width = 9;

    [Tooltip("Number of grid cells along Y. Should be odd so there's a center row.")]
    public int height = 9;

    void Awake()
    {
        Instance = this;

        if (width % 2 == 0 || height % 2 == 0)
            Debug.LogWarning("GridMap width/height should both be odd so the map has a true center cell.");
    }

    public Vector3 CenterWorldPosition => transform.position;

    public bool Contains(Vector3 worldPosition)
    {
        Vector3 local = worldPosition - transform.position;
        float halfWidth = (width - 1) / 2f * GameSetting.gridSize;
        float halfHeight = (height - 1) / 2f * GameSetting.gridSize;
        const float epsilon = 0.01f;

        return local.x >= -halfWidth - epsilon && local.x <= halfWidth + epsilon
            && local.y >= -halfHeight - epsilon && local.y <= halfHeight + epsilon;
    }
}
