using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PerformanceEnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnedEnemiesParent;

    [Header("Spawn Timing")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int spawnBatchSize = 1;
    [SerializeField] private int maxSpawnedEnemies = 25;

    [Header("Spawn Area")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Vector2 randomSpawnAreaSize = new(6f, 4f);
    [SerializeField] private bool useRandomPointWhenNoSpawnPoints = true;

    [Header("Manual Despawn")]
    [SerializeField] private Key despawnBatchKey = Key.P;
    [SerializeField] private int despawnBatchSize = 5;
    [SerializeField] private bool repeatDespawnWhileKeyHeld = true;
    [SerializeField] private float despawnRepeatInterval = 0.25f;
    [SerializeField] private bool logDespawnEvents = true;

    private readonly List<GameObject> spawnedEnemies = new();
    private Coroutine spawnRoutine;
    private int nextSpawnPointIndex;
    private float nextHeldDespawnTime;
    private bool warnedMissingPrefab;
    private bool maxSpawnedEnemiesReached;

    public int SpawnedCount => spawnedEnemies.Count;
    public bool MaxSpawnedEnemiesReached => maxSpawnedEnemiesReached;

    private void Start()
    {
        if (spawnOnStart)
        {
            StartSpawning();
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        KeyControl despawnKeyControl = despawnBatchKey == Key.P
            ? keyboard.pKey
            : keyboard[despawnBatchKey];

        float now = Time.unscaledTime;

        if (despawnKeyControl.wasPressedThisFrame)
        {
            nextHeldDespawnTime = now + despawnRepeatInterval;
            DespawnEnemyBatch();
            return;
        }

        if (repeatDespawnWhileKeyHeld && despawnKeyControl.isPressed && now >= nextHeldDespawnTime)
        {
            nextHeldDespawnTime = now + despawnRepeatInterval;
            DespawnEnemyBatch();
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

        if (maxSpawnedEnemiesReached || spawnedEnemies.Count >= maxSpawnedEnemies)
        {
            maxSpawnedEnemiesReached = true;
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

        if (spawnedEnemies.Count >= maxSpawnedEnemies)
        {
            maxSpawnedEnemiesReached = true;
        }
    }

    [ContextMenu("Spawn Enemy Batch")]
    public void SpawnEnemyBatch()
    {
        RemoveDestroyedEnemies();

        if (maxSpawnedEnemiesReached)
        {
            return;
        }

        int countToSpawn = Mathf.Min(spawnBatchSize, maxSpawnedEnemies - spawnedEnemies.Count);

        for (int i = 0; i < countToSpawn; i++)
        {
            SpawnEnemy();
        }
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

    [ContextMenu("Reset Max-Reached Flag")]
    public void ResetMaxReachedFlag()
    {
        maxSpawnedEnemiesReached = false;
    }

    [ContextMenu("Despawn Enemy Batch")]
    public void DespawnEnemyBatch()
    {
        RemoveDestroyedEnemies();

        int countToDespawn = Mathf.Min(despawnBatchSize, spawnedEnemies.Count);

        if (countToDespawn <= 0)
        {
            if (logDespawnEvents)
            {
                Debug.Log("PerformanceEnemySpawner despawn requested, but no spawned enemies are currently tracked.", this);
            }

            return;
        }

        for (int i = 0; i < countToDespawn; i++)
        {
            int lastIndex = spawnedEnemies.Count - 1;
            GameObject enemy = spawnedEnemies[lastIndex];
            spawnedEnemies.RemoveAt(lastIndex);

            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        if (logDespawnEvents)
        {
            Debug.Log($"PerformanceEnemySpawner despawned {countToDespawn} enemies. Remaining tracked enemies: {spawnedEnemies.Count}.", this);
        }
    }

    private IEnumerator SpawnLoop()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        while (enabled)
        {
            SpawnEnemyBatch();
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
        spawnBatchSize = Mathf.Max(1, spawnBatchSize);
        maxSpawnedEnemies = Mathf.Max(1, maxSpawnedEnemies);
        despawnBatchSize = Mathf.Max(1, despawnBatchSize);
        despawnRepeatInterval = Mathf.Max(0.05f, despawnRepeatInterval);
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
