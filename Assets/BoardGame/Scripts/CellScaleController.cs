using System;
using UnityEngine;

namespace TurnBasedBoard
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Grid))]
    public sealed class CellScaleController : MonoBehaviour
    {
        public static CellScaleController Instance { get; private set; }

        private const int MaximumColumns = 15;
        private const int MaximumRows = 15;

        [Header("Board dimensions")]
        [Range(1, MaximumColumns)]
        [Tooltip("Number of visible columns. The board keeps a pool of 15 columns and hides the unused cells.")]
        [SerializeField] private int columns = GameSetting.defaultColumns;

        [Range(1, MaximumRows)]
        [Tooltip("Number of visible rows. The board keeps a pool of 15 rows and hides the unused cells.")]
        [SerializeField] private int rows = GameSetting.defaultRows;

        [Header("Automatic layout")]
        [Tooltip("When enabled, changing Grid Cell Size or Cell Gap automatically rearranges all existing Cell objects.")]
        [SerializeField] private bool autoLayout = true;

        [Header("Grid alignment")]
        [Tooltip("Warn if this board's Grid.cellSize doesn't match GameSetting.gridSize, so gameplay grid math (RangeCheck, chamoving, MoveChecker) and the visual board don't silently drift apart.")]
        [SerializeField] private bool warnOnCellSizeMismatch = true;

        private Grid boardGrid;
        private Vector3 lastCellSize;
        private Vector3 lastCellGap;
        private int lastColumns = -1;
        private int lastRows = -1;
        private int lastChildCount = -1;
        private bool hasLayoutSnapshot;

        public int Columns => columns;
        public int Rows => rows;

        public Vector2 CellSize
        {
            get
            {
                Grid grid = GetBoardGrid();
                return grid != null
                    ? new Vector2(grid.cellSize.x, grid.cellSize.y)
                    : Vector2.one * GameSetting.gridSize;
            }
        }

        private void Awake()
        {
            Instance = this;

            CheckCellSizeAlignment();

            if (Application.isPlaying)
            {
                ApplyGridLayout();
            }
        }

        private void OnEnable()
        {
            hasLayoutSnapshot = false;

            if (Application.isPlaying)
            {
                ApplyGridLayout();
            }
        }

        private void OnValidate()
        {
            columns = Mathf.Clamp(columns, 1, MaximumColumns);
            rows = Mathf.Clamp(rows, 1, MaximumRows);

            hasLayoutSnapshot = false;

            CheckCellSizeAlignment();
        }

        private void Update()
        {
            if (!autoLayout)
            {
                return;
            }

            Grid grid = GetBoardGrid();
            if (grid == null)
            {
                return;
            }

            if (!hasLayoutSnapshot ||
                grid.cellSize != lastCellSize ||
                grid.cellGap != lastCellGap ||
                columns != lastColumns ||
                rows != lastRows ||
                transform.childCount != lastChildCount)
            {
                ApplyGridLayout();
            }
        }

        [ContextMenu("Apply Grid Layout")]
        public void ApplyGridLayout()
        {
            Grid grid = GetBoardGrid();
            if (grid == null)
            {
                return;
            }

            columns = Mathf.Clamp(columns, 1, MaximumColumns);
            rows = Mathf.Clamp(rows, 1, MaximumRows);

            Vector3 safeCellSize = grid.cellSize;
            safeCellSize.x = Mathf.Max(0.01f, safeCellSize.x);
            safeCellSize.y = Mathf.Max(0.01f, safeCellSize.y);

            Vector3 firstCenter = grid.GetCellCenterLocal(Vector3Int.zero);
            Vector3 lastCenter = grid.GetCellCenterLocal(new Vector3Int(columns - 1, rows - 1, 0));
            Vector3 boardCenter = (firstCenter + lastCenter) * 0.5f;

            for (int index = 0; index < transform.childCount; index++)
            {
                Transform child = transform.GetChild(index);
                if (!TryReadCellCoordinates(child.name, out int column, out int row))
                {
                    continue;
                }

                bool shouldBeVisible = column >= 0 && column < columns && row >= 0 && row < rows;
                if (child.gameObject.activeSelf != shouldBeVisible)
                {
                    child.gameObject.SetActive(shouldBeVisible);
                }

                if (!shouldBeVisible)
                {
                    continue;
                }

                Vector3 position = grid.GetCellCenterLocal(new Vector3Int(column, row, 0)) - boardCenter;
                position.z = child.localPosition.z;
                child.localPosition = position;

                SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                    float scaleX = safeCellSize.x / Mathf.Max(0.0001f, spriteSize.x);
                    float scaleY = safeCellSize.y / Mathf.Max(0.0001f, spriteSize.y);
                    child.localScale = new Vector3(scaleX, scaleY, 1f);
                }
            }

            lastCellSize = grid.cellSize;
            lastCellGap = grid.cellGap;
            lastColumns = columns;
            lastRows = rows;
            lastChildCount = transform.childCount;
            hasLayoutSnapshot = true;
        }

        // Scales `target` so its renderer bounds fit exactly one grid cell
        // -- same idea as the per-cell sprite scaling in ApplyGridLayout,
        // but for a character/enemy piece rather than a board cell.
        public void AlignToCell(Transform target)
        {
            if (target == null)
            {
                return;
            }

            Grid grid = GetBoardGrid();
            Vector3 cellSize = grid != null ? grid.cellSize : GameSetting.gridSize * Vector3.one;
            cellSize.x = Mathf.Max(0.01f, cellSize.x);
            cellSize.y = Mathf.Max(0.01f, cellSize.y);

            if (!TryGetUnscaledSize(target, out Vector2 unscaledSize, out bool isSprite))
            {
                return;
            }

            Vector3 currentScale = target.localScale;

            if (isSprite)
            {
                // Flat sprite: fine to stretch non-uniformly to exactly fill the cell.
                float scaleX = unscaledSize.x > 0.0001f ? cellSize.x / unscaledSize.x : currentScale.x;
                float scaleY = unscaledSize.y > 0.0001f ? cellSize.y / unscaledSize.y : currentScale.y;
                target.localScale = new Vector3(scaleX, scaleY, currentScale.z);
            }
            else
            {
                // 3D mesh: one uniform factor so the shape can't distort --
                // take the smaller of the two so it never overflows the cell.
                float scaleX = unscaledSize.x > 0.0001f ? cellSize.x / unscaledSize.x : 1f;
                float scaleY = unscaledSize.y > 0.0001f ? cellSize.y / unscaledSize.y : 1f;
                float uniformScale = Mathf.Min(scaleX, scaleY);
                target.localScale = new Vector3(uniformScale, uniformScale, uniformScale);
            }
        }

        // World position of GameSetting.defaultBornCell on this board --
        // reuses the same board-center math as ApplyGridLayout so it stays
        // consistent with where the cells themselves are actually drawn.
        public Vector3 GetDefaultBornWorldPosition()
        {
            Grid grid = GetBoardGrid();
            if (grid == null)
            {
                return transform.position;
            }

            Vector3 firstCenter = grid.GetCellCenterLocal(Vector3Int.zero);
            Vector3 lastCenter = grid.GetCellCenterLocal(new Vector3Int(columns - 1, rows - 1, 0));
            Vector3 boardCenter = (firstCenter + lastCenter) * 0.5f;

            Vector3Int bornCell = new Vector3Int(GameSetting.defaultBornCell.x, GameSetting.defaultBornCell.y, 0);
            Vector3 bornLocal = grid.GetCellCenterLocal(bornCell) - boardCenter;
            return transform.TransformPoint(bornLocal);
        }

        public bool TryGetCellWorldPosition(int column, int row, out Vector3 worldPosition)
        {
            worldPosition = transform.position;

            Grid grid = GetBoardGrid();
            if (grid == null || column < 0 || column >= columns || row < 0 || row >= rows)
            {
                return false;
            }

            Vector3 firstCenter = grid.GetCellCenterLocal(Vector3Int.zero);
            Vector3 lastCenter = grid.GetCellCenterLocal(new Vector3Int(columns - 1, rows - 1, 0));
            Vector3 boardCenter = (firstCenter + lastCenter) * 0.5f;
            Vector3 localPosition = grid.GetCellCenterLocal(new Vector3Int(column, row, 0)) - boardCenter;
            worldPosition = transform.TransformPoint(localPosition);
            return true;
        }

        public Bounds GetWorldBounds()
        {
            Grid grid = GetBoardGrid();
            if (grid == null)
            {
                return new Bounds(transform.position, Vector3.one * GameSetting.gridSize);
            }

            Vector2 size = CellSize;
            Vector3 halfSize = new Vector3(columns * size.x * 0.5f, rows * size.y * 0.5f, 0.05f);
            Vector3[] corners =
            {
                transform.TransformPoint(new Vector3(-halfSize.x, -halfSize.y, 0f)),
                transform.TransformPoint(new Vector3(-halfSize.x, halfSize.y, 0f)),
                transform.TransformPoint(new Vector3(halfSize.x, -halfSize.y, 0f)),
                transform.TransformPoint(new Vector3(halfSize.x, halfSize.y, 0f))
            };

            Bounds bounds = new Bounds(corners[0], Vector3.zero);
            for (int index = 1; index < corners.Length; index++)
            {
                bounds.Encapsulate(corners[index]);
            }

            bounds.Expand(new Vector3(0f, 0f, 0.1f));
            return bounds;
        }

        // Whether worldPosition falls inside this board's actual
        // columns x rows footprint -- reuses the same board-center math as
        // ApplyGridLayout/GetDefaultBornWorldPosition so it can't drift out
        // of sync with where the cells are actually drawn.
        public bool Contains(Vector3 worldPosition)
        {
            Grid grid = GetBoardGrid();
            if (grid == null)
            {
                return true;
            }

            // No boardCenter subtraction here: ApplyGridLayout already
            // re-centers every cell's local position around this
            // transform's own origin (cellCenterLocal - boardCenter), so
            // the visual board is already symmetric around local (0,0).
            // Subtracting boardCenter again (as this used to) shifted the
            // valid region by roughly half the board's width.
            Vector3 local = transform.InverseTransformPoint(worldPosition);

            float halfWidth = columns * grid.cellSize.x * 0.5f;
            float halfHeight = rows * grid.cellSize.y * 0.5f;
            const float epsilon = 0.01f;

            return local.x >= -halfWidth - epsilon && local.x <= halfWidth + epsilon
                && local.y >= -halfHeight - epsilon && local.y <= halfHeight + epsilon;
        }

        private void CheckCellSizeAlignment()
        {
            if (!warnOnCellSizeMismatch)
            {
                return;
            }

            Grid grid = GetBoardGrid();
            if (grid == null)
            {
                return;
            }

            if (Mathf.Abs(grid.cellSize.x - GameSetting.gridSize) > 0.001f ||
                Mathf.Abs(grid.cellSize.y - GameSetting.gridSize) > 0.001f)
            {
                Debug.LogWarning($"CellScaleController: Grid.cellSize ({grid.cellSize}) doesn't match GameSetting.gridSize ({GameSetting.gridSize}). Gameplay grid math (RangeCheck/chamoving/MoveChecker) and the visual board will be out of sync.");
            }
        }

        private static bool TryGetUnscaledSize(Transform target, out Vector2 size, out bool isSprite)
        {
            // GetComponent, not GetComponentInChildren: a nested child (e.g.
            // a floating health-bar sprite) would otherwise get picked up
            // instead of the character's own body, scaling the whole
            // transform to match completely unrelated bounds.
            SpriteRenderer spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                size = spriteRenderer.sprite.bounds.size;
                isSprite = true;
                return true;
            }

            MeshFilter meshFilter = target.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                size = meshFilter.sharedMesh.bounds.size;
                isSprite = false;
                return true;
            }

            size = default;
            isSprite = false;
            return false;
        }

        private Grid GetBoardGrid()
        {
            if (boardGrid == null)
            {
                boardGrid = GetComponent<Grid>();
            }

            return boardGrid;
        }

        private static bool TryReadCellCoordinates(string objectName, out int column, out int row)
        {
            column = -1;
            row = -1;

            if (string.IsNullOrEmpty(objectName) || !objectName.StartsWith("Cell_", StringComparison.Ordinal))
            {
                return false;
            }

            string[] parts = objectName.Split('_');
            return parts.Length >= 3 &&
                   int.TryParse(parts[1], out column) &&
                   int.TryParse(parts[2], out row);
        }
    }
}
