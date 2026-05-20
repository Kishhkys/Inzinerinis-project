using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceEnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnedEnemiesParent;

    [Header("Spawn Timing")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxSpawnedEnemies = 25;

    [Header("Spawn Area")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Vector2 randomSpawnAreaSize = new(6f, 4f);
    [SerializeField] private bool useRandomPointWhenNoSpawnPoints = true;

    private readonly List<GameObject> spawnedEnemies = new();
    private Coroutine spawnRoutine;
    private int nextSpawnPointIndex;
    private bool warnedMissingPrefab;

    public int SpawnedCount => spawnedEnemies.Count;

    private void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null)
        {
            return;
        }

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null)
        {
            return;
        }

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    [ContextMenu("Spawn Enemy Now")]
    public void SpawnEnemy()
    {
        RemoveDestroyedEnemies();

        if (spawnedEnemies.Count >= maxSpawnedEnemies)
        {
            return;
        }

        if (enemyPrefab == null)
        {
            if (!warnedMissingPrefab)
            {
                Debug.LogWarning("PerformanceEnemySpawner has no enemy prefab assigned.", this);
                warnedMissingPrefab = true;
            }

            return;
        }

        GameObject enemy = Instantiate(
            enemyPrefab,
            GetSpawnPosition(),
            Quaternion.identity,
            spawnedEnemiesParent
        );

        enemy.name = $"{enemyPrefab.name}_Perf_{spawnedEnemies.Count + 1:00}";
        spawnedEnemies.Add(enemy);
    }

    [ContextMenu("Despawn Spawned Enemies")]
    public void DespawnSpawnedEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] != null)
            {
                Destroy(spawnedEnemies[i]);
            }
        }

        spawnedEnemies.Clear();
    }

    private IEnumerator SpawnLoop()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        while (enabled)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }

        spawnRoutine = null;
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[nextSpawnPointIndex % spawnPoints.Length];
            nextSpawnPointIndex++;

            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }
        }

        if (!useRandomPointWhenNoSpawnPoints)
        {
            return transform.position;
        }

        Vector2 halfSize = randomSpawnAreaSize * 0.5f;
        Vector2 offset = new(
            Random.Range(-halfSize.x, halfSize.x),
            Random.Range(-halfSize.y, halfSize.y)
        );

        return transform.position + (Vector3)offset;
    }

    private void RemoveDestroyedEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    private void OnValidate()
    {
        startDelay = Mathf.Max(0f, startDelay);
        spawnInterval = Mathf.Max(0.1f, spawnInterval);
        maxSpawnedEnemies = Mathf.Max(1, maxSpawnedEnemies);
        randomSpawnAreaSize.x = Mathf.Max(0f, randomSpawnAreaSize.x);
        randomSpawnAreaSize.y = Mathf.Max(0f, randomSpawnAreaSize.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.25f);
                }
            }

            return;
        }

        Gizmos.DrawWireCube(transform.position, randomSpawnAreaSize);
    }
}
