using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("체력 설정")]
    public int health = 3;
    public int contactDamage = 6; // 인스펙터에서 빅 조는 6으로 설정

    [Header("체력 아이템 드롭")]
    public GameObject SmallHealthItem; // 아까 만든 HealthItem 프리팹 연결
    public GameObject BigHealthItem;

    public GameObject deathParticlePrefab; // 아까 만든 EnemyExplosion 프리팹 연결 현재 각각 상속받은 자식 클래스에서 따로 설정해주기로 했음
    protected Transform player;
    protected SpriteRenderer spriteRenderer;
    protected bool isInvincible = false; // 짧은 무적 시간 (연타 시 씹힘 방지)

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
        else
        {
            Debug.LogWarning("씬에 'Player' 태그를 가진 오브젝트가 없습니다!");
        }

    }

    public void SetPlayer(Transform p)
    {
        player = p;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isInvincible || health <= 0) return;

        health -= damage;
        //Debug.Log($"적 피격! 남은 체력: {health}");

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StopAllCoroutines(); // 이전 깜빡임이 있다면 멈추고 새로 시작
            StartCoroutine(FlashRoutine());
        }
    }

  

    IEnumerator FlashRoutine()
    {
        isInvincible = true;
       

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        // 빨간색으로 깜빡이게 해서 피격 강조
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }

        isInvincible = false;
    }

    protected virtual void Die()
    {
        // 1. 죽는 위치에 폭발 파티클 프리팹 소환
        if (deathParticlePrefab != null)
        {
            Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
        }


        SpawnItem(); // 아이템 드롭 함수 호출
        // 2. 몬스터 본체 파괴 (이제 파티클은 별개 오브젝트라 안 사라짐)
        Destroy(gameObject);
    }

    private void SpawnItem()
    {
       if( Random.Range(0f, 100f) > 100f) // 30% 확률로 아이템 드롭
       {
            return;
       }
       //30%뚫고 드랍이 되면 둘중에 뭐나올지 ㅋㅋ 60%는 작은알약 30%큰알약 
        GameObject itemToSpawn;
        if(Random.Range(0f, 100f) <= 60f) // 드롭되는 아이템 중 60%는 작은 알약, 30%는 큰 알약
        {
            itemToSpawn = SmallHealthItem;
        }
        else
        {
            itemToSpawn = BigHealthItem;
        }

        if (itemToSpawn != null)
        {
           Instantiate(itemToSpawn, transform.position, Quaternion.identity);
        }

    }

    
}