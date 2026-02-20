using UnityEngine;
using System.Collections;

public class SniperJoeSimple : Enemy
{
    public float detectRange = 8f;
    public GameObject bulletPrefab; // 적 총알 프리팹
    public Transform firePoint;     // 총구 위치 (빈 오브젝트로 만들어 캡슐 자식으로 두세요)

    private Animator anim;
    private bool isAttacking = false;

    protected override void Awake() // 부모의 Awake를 덮어씌움(Override)
    {
        base.Awake(); // ★매우 중요: 이게 있어야 부모(Enemy)가 플레이어를 찾아옵니다!
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        FacePlayer();

        // 사거리 안이면 공격 시작
        if (dist <= detectRange && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    void FacePlayer()
    {
        if (player == null) return;

        // 플레이어와의 상대적 위치 계산
        float dir = player.position.x - transform.position.x;

        // 현재 스케일의 원래 크기(양수값)를 가져옵니다.
        float targetScaleX = Mathf.Abs(transform.localScale.x);
        float targetScaleY = transform.localScale.y;

        // [중요] dir > 0 (플레이어가 오른쪽)이면 마이너스 스케일, 아니면 플러스 스케일
        // 만약 반대로 보고 있다면 -targetScaleX와 targetScaleX의 위치를 서로 바꾸세요!
        float finalScaleX = dir > 0 ? -targetScaleX : targetScaleX;

        transform.localScale = new Vector3(finalScaleX, targetScaleY, 1);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        anim.SetBool("isTargeting", true); // Ready 자세로!

        yield return new WaitForSeconds(0.5f); // 쏘기 전 딜레이

        // 3연발 쏘기
        for (int i = 0; i < 3; i++)
        {
            anim.SetTrigger("attack"); // Shoot 자세로!
            Fire();
            yield return new WaitForSeconds(0.3f);
        }

        anim.SetBool("isTargeting", false); // 다시 Idle로!
        yield return new WaitForSeconds(2f); // 다음 공격까지 쉼표
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
                // 현재 조의 scale.x가 -1이면 오른쪽, 1이면 왼쪽을 보고 있는 상태이므로
                // 그 값을 그대로 총알의 '방향'으로 전달합니다.
                float moveDir = transform.localScale.x;
                bullet.SetDirection(moveDir);
            }
        }
    }
}