using UnityEngine;
using System.Collections;

public class SniperJoeSimple : Enemy
{
    public float detectRange = 8f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    private Animator anim;
    private bool isAttacking = false;

    protected override void Awake()
    {
        // [이유] 부모의 Awake를 실행해야 GameObject.FindGameObjectWithTag("Player") 로직이 작동해서
        // 'player' 변수에 록맨의 위치 정보가 담깁니다.
        base.Awake();
        anim = GetComponent<Animator>();
    }

    // [이유] 부모에 Update가 없으므로 평소처럼 void Update()를 사용합니다.
    // override를 붙이지 않아도 되지만, 부모의 player 정보를 안전하게 체크합니다.
    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        FacePlayer();

        if (dist <= detectRange && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    void FacePlayer()
    {
        if (player == null) return;

        float dir = player.position.x - transform.position.x;
        float targetScaleX = Mathf.Abs(transform.localScale.x);
        float targetScaleY = transform.localScale.y;

        // 플레이어 위치에 따라 좌우 반전
        float finalScaleX = dir > 0 ? -targetScaleX : targetScaleX;
        transform.localScale = new Vector3(finalScaleX, targetScaleY, 1);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        anim.SetBool("isTargeting", true);

        yield return new WaitForSeconds(0.5f);

        // 3연발 쏘기
        for (int i = 0; i < 3; i++)
        {
            // [이유] 만약 총을 쏘는 도중에 록맨 버스터에 맞아 체력이 0이 되면,
            // 더 이상 총알을 생성하지 않고 코루틴을 즉시 종료(yield break)합니다.
            if (health <= 0) yield break;

            anim.SetTrigger("Attack");
            Fire();
            yield return new WaitForSeconds(0.3f);
        }

        anim.SetBool("isTargeting", false);
        yield return new WaitForSeconds(2f);
        isAttacking = false;
    }

    void Fire()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            EnemyBullet bullet = bulletObj.GetComponent<EnemyBullet>();

            if (bullet != null)
            {
                // 현재 조의 보는 방향을 총알에 전달
                float moveDir = transform.localScale.x;
                bullet.SetDirection(moveDir);
            }
        }
    }

   
    protected override void Die()
    {
        // 공격 코루틴을 강제로 멈춥니다. 안 멈추면 죽고 나서 폭발 이펙트만 남았는데 총알이 뿅 나갑니다.
        StopAllCoroutines();

        // [핵심] 부모의 Die()를 호출해야 폭발 파티클이 생기고 Destroy(gameObject)가 실행됩니다.
        base.Die();
    }
}