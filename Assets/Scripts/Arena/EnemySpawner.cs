using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Références")]
    public GameObject enemyPrefab;
    public Transform playerTransform;

    [Header("Vagues — base")]
    public int baseEnemiesPerWave = 5;
    public float timeBetweenWaves = 5f;
    public float spawnRadius = 8f;
    public int maxEnemiesAlive = 20;

    [Header("Scaling")]
    [Tooltip("Multiplicateur du nombre d'ennemis par vague")]
    public float waveCountMultiplier = 1.4f;   // Vague 1=5, 2=7, 3=10, 4=14...
    [Tooltip("% vitesse ajoutée par vague (cappée à maxSpeedBonus)")]
    public float speedBonusPerWave = 0.02f;  // 2% par vague
    public float maxSpeedBonus = 0.30f;  // cap à +30%
    [Tooltip("% HP ajoutés par vague")]
    public float hpBonusPerWave = 0.15f;  // +15% HP par vague

    private int _currentWave = 0;
    private int _aliveCount = 0;

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
        yield return new WaitForSeconds(2f);

        while (true)
        {
            yield return new WaitForSeconds(timeBetweenWaves);

            if (_aliveCount >= maxEnemiesAlive)
                yield return new WaitUntil(() => _aliveCount < maxEnemiesAlive / 2);

            _currentWave++;
            int count = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(waveCountMultiplier, _currentWave - 1));
            count = Mathf.Min(count, maxEnemiesAlive);
            SpawnWave(count);
        }
    }

    private void SpawnWave(int count)
    {
        float speedMult = 1f + Mathf.Min(speedBonusPerWave * (_currentWave - 1), maxSpeedBonus);
        float hpMult = 1f + hpBonusPerWave * (_currentWave - 1);

        for (int i = 0; i < count; i++)
        {
            if (_aliveCount >= maxEnemiesAlive) break;
            var go = Instantiate(enemyPrefab, GetSpawnPosition(), Quaternion.identity);
            var ec = go.GetComponent<EnemyController>();
            if (ec != null)
            {
                ec.moveSpeed *= speedMult;
                ec.chaseSpeed *= speedMult;
                ec.maxHP *= hpMult;
            }
            var tracker = go.AddComponent<EnemyDeathTracker>();
            tracker.OnDeath += () => _aliveCount--;
            _aliveCount++;
        }
    }

    private Vector2 GetSpawnPosition()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        float dist = spawnRadius + Random.Range(0f, 2f);
        Vector2 base2 = playerTransform != null ? (Vector2)playerTransform.position : Vector2.zero;
        return base2 + dir * dist;
    }
}

public class EnemyDeathTracker : MonoBehaviour
{
    public System.Action OnDeath;
    private void OnDestroy() => OnDeath?.Invoke();
}