using System.Collections;
using System.Collections.Generic;
using TurnBasedBoard;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private CellScaleController board;
    [SerializeField] private Transform player;
    [SerializeField, Min(0.1f)] private float spawnInterval = 3f;
    [SerializeField, Min(1)] private int maximumAliveEnemies = 30;
    [SerializeField] private Transform spawnParent;

    private readonly List<Vector2Int> perimeterCells = new List<Vector2Int>();
    private Coroutine spawningRoutine;

    private void OnEnable()
    {
        spawningRoutine = StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        if (spawningRoutine != null)
        {
            StopCoroutine(spawningRoutine);
            spawningRoutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        // Keep this routine alive for the lifetime of the scene. During a
        // restart, OnEnable can run just before PlayingSequence resets the
        // previous run's GameOver flag; exiting here would permanently stop
        // enemy generation in the newly loaded scene.
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (!PlayingSequence.GameOver && EnemyBehavior.AliveCount < maximumAliveEnemies)
                TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        if (enemyPrefab == null || board == null || player == null)
        {
            Debug.LogWarning("EnemySpawner needs Enemy Prefab, Board and Player references.", this);
            return;
        }

        RebuildPerimeterCells();
        if (perimeterCells.Count == 0) return;

        int firstIndex = Random.Range(0, perimeterCells.Count);
        for (int attempt = 0; attempt < perimeterCells.Count; attempt++)
        {
            Vector2Int cell = perimeterCells[(firstIndex + attempt) % perimeterCells.Count];
            if (!board.TryGetCellWorldPosition(cell.x, cell.y, out Vector3 spawnPosition))
                continue;

            // Gameplay currently moves on XY; keep every generated enemy on
            // the player's Z plane even if the board artwork sits behind it.
            spawnPosition.z = player.position.z;
            if (IsOccupied(spawnPosition))
                continue;

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity, spawnParent);
            enemy.name = "Square Enemy";
            board.AlignToCell(enemy.transform);
            return;
        }
    }

    private void RebuildPerimeterCells()
    {
        perimeterCells.Clear();
        int columns = board.Columns;
        int rows = board.Rows;

        for (int column = 0; column < columns; column++)
        {
            perimeterCells.Add(new Vector2Int(column, 0));
            if (rows > 1)
                perimeterCells.Add(new Vector2Int(column, rows - 1));
        }

        for (int row = 1; row < rows - 1; row++)
        {
            perimeterCells.Add(new Vector2Int(0, row));
            if (columns > 1)
                perimeterCells.Add(new Vector2Int(columns - 1, row));
        }
    }

    private bool IsOccupied(Vector3 worldPosition)
    {
        Vector2 cellSize = board.CellSize;
        Vector3 halfExtents = new Vector3(cellSize.x * 0.4f, cellSize.y * 0.4f, 0.5f);
        Collider[] hits = Physics.OverlapBox(worldPosition, halfExtents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            if (hit.GetComponentInParent<PlayerController>() != null ||
                hit.GetComponentInParent<EnemyBehavior>() != null)
            {
                return true;
            }
        }

        return false;
    }
}
