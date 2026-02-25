using UnityEngine;

/// <summary>
/// 록맨의 '텔리' 몬스터 구현 스크립트.
/// 회전은 애니메이터(Animator)에서 처리하므로 이동 및 충돌 로직만 포함합니다.
/// </summary>
public class Telly : Enemy
{
    [Header("Telly Movement")]
    [SerializeField] private float moveSpeed = 1.2f;      // 이동 속도
    [SerializeField] private bool followPlayer = true;   // 플레이어 추적 여부

    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab; // 사망/충돌 시 생성할 폭발 프리팹

    private Rigidbody2D rb;

    protected override void Awake()
    {
        // 1. 부모 클래스(Enemy)의 Awake 실행
        base.Awake();

        // 2. 컴포넌트 참조 및 물리 설정
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;      // 공중 부양
        }

    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 플레이어가 없거나 추적 모드가 꺼져있으면 리턴
        if (player == null || !followPlayer) return;

        // 플레이어 방향 벡터 계산
        Vector2 direction = (player.position - transform.position).normalized;

        // 부드러운 이동 (회전은 애니메이션이 담당하므로 위치만 이동)
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// 플레이어와 충돌했을 때 실행
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 부모(Enemy)에 선언된 contactDamage 변수를 그대로 사용
            var player = collision.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(contactDamage, transform.position);
            }

            // 충돌 즉시 폭발
            Die();
        }
    }

    /// <summary>
    /// 플레이어의 총알 등에 맞아 체력이 0이 되었을 때 (부모 메서드 오버라이드)
    /// </summary>
    protected override void Die()
    {
       base.Die(); // 부모의 Die() 실행 (체력 0 체크 및 파티클 생성 포함)
        // 추가로 텔리만의 폭발 효과를 생성할 수 있습니다.
        
    }

    /// <summary>
    /// 폭발 효과 생성 및 객체 파괴
    /// </summary>
    //private void Explode()
    //{
    //    if (explosionPrefab != null)
    //    {
    //        // 폭발 파티클 생성
    //        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    //    }

    //    // 텔리 제거
    //    Destroy(gameObject);
    //}
}