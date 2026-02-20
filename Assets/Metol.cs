using System.Collections;
using UnityEngine;

public class Metool : Enemy
{
    [Header("메툴 설정")]
    public float detectRange = 5f;
    public float moveSpeed = 1.5f;

    private bool isAwake = false;
    private Animator anim;
    private float baseScaleX;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 1. 거리 체크: 이 변수 하나로 애니메이션과 판정을 모두 관리합니다.
        isAwake = (distance <= detectRange);
        anim.SetBool("isAwake", isAwake);

        // 2. 일어난 상태(isAwake)일 때의 로직
        if (isAwake)
        {
            FacePlayer();

            // 이동 애니메이션 상태일 때만 실제 이동
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("MetolMove"))
            {
                MoveTowardsPlayer();
            }
        }
    }

    public override void TakeDamage(int damage)
    {
        // [핵심] 깨어있지 않으면(숨어있으면) 무조건 팅!
        if (!isAwake)
        {
            Debug.Log("팅! 숨어있는 상태라 데미지 없음");
            return;
        }

        // 깨어있을 때만 부모의 피격 로직 실행
        base.TakeDamage(damage);
    }

    void MoveTowardsPlayer()
    {
        float direction = (player.position.x > transform.position.x) ? 1f : -1f;
        transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0);
    }

    void FacePlayer()
    {
        float dir = player.position.x - transform.position.x;
        float targetScaleX = dir > 0 ? -baseScaleX : baseScaleX;
        transform.localScale = new Vector3(targetScaleX, transform.localScale.y, 1);
    }
}