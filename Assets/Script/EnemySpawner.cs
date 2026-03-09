using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnDistance = 12f;
    public float despawnDistance = 15f;
    public float spawnInterval = 3f;

    [Header("Respawn Settings")]
    [SerializeField] private bool isInfiniteRespawn = false; // [추가] 텔리처럼 무한 보충할 건지 체크!

    private float timer = 0f;
    private GameObject spawnedEnemy;
    private bool isDeadInThisCycle = false;
    private Transform playerTransform;

    void Start()
    {
        FindPlayer();
        timer = spawnInterval;
    }

    void Update()
    {
        if (StageManager.Instance != null && !StageManager.Instance.isGameActive) return;
        if (playerTransform == null) { FindPlayer(); return; }

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        // 1. 소환 로직
        // 무한 모드라면 'isDeadInThisCycle'을 무시하고, 일반 모드라면 체크함
        bool canSpawn = isInfiniteRespawn ? true : !isDeadInThisCycle;

        if (dist <= spawnDistance && spawnedEnemy == null && canSpawn)
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                SpawnEnemy();
                timer = 0f;
            }
        }
        else if (spawnedEnemy != null)
        {
            timer = 0f; // 살아있으면 타이머 리셋
        }

        // 2. 디스폰 및 리셋 로직
        if (dist > despawnDistance)
        {
            if (spawnedEnemy != null)
            {
                Destroy(spawnedEnemy);
                spawnedEnemy = null;
            }

            // 공통 리셋
            isDeadInThisCycle = false;
            timer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        spawnedEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

        // [수정] 무한 모드가 아닐 때만 "이번 사이클 종료" 표시를 함
        if (!isInfiniteRespawn)
        {
            isDeadInThisCycle = true;
        }

        Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetPlayer(playerTransform);
        }
    }

    private void FindPlayer()
    {
        if (StageManager.Instance != null && StageManager.Instance.player != null)
        {
            playerTransform = StageManager.Instance.player.transform;
        }
    }
}