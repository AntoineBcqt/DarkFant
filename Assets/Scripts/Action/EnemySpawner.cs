using System.Collections;
using UnityEngine;

/// <summary>
/// Spawner de vagues d'ennemis style Hadès.
/// Spawn des ennemis autour du joueur à intervalles réguliers.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Références")]
    public GameObject enemyPrefab;
    public Transform  playerTransform;

    [Header("Vagues")]
    public int   enemiesPerWave   = 4;
    public float timeBetweenWaves = 5f;
    public float spawnRadius      = 8f;
    public int   maxEnemiesAlive  = 10;

    [Header("Progression")]
    public int   enemiesPerWaveIncrement = 1;  // +1 ennemi par vague
    public float waveSpeedMultiplier     = 1.1f; // ennemis 10% plus rapides par vague

    private int   _currentWave   = 0;
    private int   _aliveCount    = 0;

    private void Start()
    {
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(2f); // délai initial

        while (true)
        {
            yield return new WaitForSeconds(timeBetweenWaves);

            if (_aliveCount >= maxEnemiesAlive)
            {
                yield return new WaitUntil(() => _aliveCount < maxEnemiesAlive / 2);
            }

            _currentWave++;
            int count = enemiesPerWave + (_currentWave - 1) * enemiesPerWaveIncrement;
            SpawnWave(count);
        }
    }

    private void SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_aliveCount >= maxEnemiesAlive) break;

            Vector2 spawnPos = GetSpawnPosition();
            var go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Augmenter la vitesse selon la vague
            var ec = go.GetComponent<EnemyController>();
            if (ec != null)
            {
                ec.moveSpeed  *= Mathf.Pow(waveSpeedMultiplier, _currentWave - 1);
                ec.chaseSpeed *= Mathf.Pow(waveSpeedMultiplier, _currentWave - 1);
            }

            // Tracker la mort
            var tracker = go.AddComponent<EnemyDeathTracker>();
            tracker.OnDeath += () => _aliveCount--;
            _aliveCount++;
        }
    }

    private Vector2 GetSpawnPosition()
    {
        // Spawn hors de l'écran mais pas trop loin
        Vector2 dir = Random.insideUnitCircle.normalized;
        float dist  = spawnRadius + Random.Range(0f, 2f);
        Vector2 basePos = playerTransform != null
            ? (Vector2)playerTransform.position
            : Vector2.zero;
        return basePos + dir * dist;
    }
}

/// <summary>Petit composant qui notifie quand l'ennemi est détruit.</summary>
public class EnemyDeathTracker : MonoBehaviour
{
    public System.Action OnDeath;
    private void OnDestroy() => OnDeath?.Invoke();
}
