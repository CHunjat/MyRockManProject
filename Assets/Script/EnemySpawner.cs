using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnDistance = 12f;
    public float despawnDistance = 15f;

    private GameObject spawnedEnemy;
    private Transform playerTransform;

    void Start()
    {
        // 1. 여기서 플레이어를 못 찾아도 괜찮아. Update에서 StageManager로 찾을 거니까.
        FindPlayer();
    }

    void Update()
    {
        // 2. Ready 중에는 가만히 있기
        if (StageManager.Instance != null && !StageManager.Instance.isGameActive) return;

        // 3. 플레이어가 없으면 StageManager에서 가져오기 (싱글톤 활용)
        if (playerTransform == null)
        {
            FindPlayer();
            return; // 이번 프레임은 쉬고 다음 프레임부터 거리 체크
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        // 4. 소환 로직 (딱 한 번만 실행되도록 통합)
        if (dist <= spawnDistance && spawnedEnemy == null)
        {
            spawnedEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);

            // 소환 즉시 플레이어 정보 강제 주입
            Enemy enemyScript = spawnedEnemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.SetPlayer(playerTransform);
            }
        }

        // 5. 디스폰 로직
        if (dist > despawnDistance && spawnedEnemy != null)
        {
            Destroy(spawnedEnemy);
            spawnedEnemy = null;
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