using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnDistance = 12f;
    public float despawnDistance = 15f;

    private GameObject spawnedEnemy;
    private bool isDeadInThisCycle = false; // [수정] 이번 접근에서 이미 죽었는지 체크
    private Transform playerTransform;

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (StageManager.Instance != null && !StageManager.Instance.isGameActive) return;

        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        // 1. 소환 로직: 거리 안에 있고, 소환된 적이 없고, 아직 죽지 않았을 때만 소환
        if (dist <= spawnDistance && spawnedEnemy == null && !isDeadInThisCycle)
        {
            SpawnEnemy();
        }

        // 2. 적이 있었는데 사라졌다면? (죽었거나 수동 삭제됨)
        if (spawnedEnemy == null && isDeadInThisCycle == false && dist <= spawnDistance)
        {
            // 사실 위 1번 조건에서 spawnedEnemy가 생성되므로, 
            // 여기 들어왔다는 건 소환됐던 놈이 죽어서 null이 됐다는 뜻이야.
            // (아래 3번 로직에서 자동으로 감지되니 생략 가능하지만 흐름상 이해를 돕기 위해 언급!)
        }

        // 실시간 체크: 소환됐던 적이 죽었는지 감지
        // 소환은 했었는데(isDeadInThisCycle가 true인데) 객체가 null이면 '죽었다'고 판단
        // (단, 디스폰 거리가 아닐 때만 죽은 걸로 간주)
        if (isDeadInThisCycle && spawnedEnemy == null && dist <= despawnDistance)
        {
            // 이 상태를 유지해서 재소환을 막음
        }

        // 3. 디스폰 및 리셋 로직
        if (dist > despawnDistance)
        {
            // 화면 밖으로 나가면 모든 상태 리셋 (다시 다가오면 소환 가능하게)
            if (spawnedEnemy != null)
            {
                Destroy(spawnedEnemy);
                spawnedEnemy = null;
            }

            isDeadInThisCycle = false; // [핵심] 이제야 다시 소환 가능한 상태가 됨!
        }
        else if (spawnedEnemy == null && isDeadInThisCycle == false && dist <= spawnDistance)
        {
            // 이 구문은 사실 1번 조건과 같아. 
            // 핵심은 spawnedEnemy가 생성된 후 null이 되면, 
            // dist > despawnDistance가 되기 전까지 isDeadInThisCycle를 false로 안 만드는 거야.
        }
    }

    private void SpawnEnemy()
    {
        spawnedEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        isDeadInThisCycle = true; // [핵심] 소환하는 순간 "이번 턴은 끝났다"고 표시

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