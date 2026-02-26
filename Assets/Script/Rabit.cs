using UnityEngine;
using System.Collections;

public class BigRabbit : Enemy
{
    [Header("점프 설정")]
    public float jumpForceX = 5f;
    public float jumpForceY = 12f;
    public float jumpDelay = 1.5f;

    [Header("지면 체크")]
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private bool isJumping = false;
    private Coroutine flashRoutine; // 깜빡이 코루틴 전용 변수

    protected override void Awake()
    {
        base.Awake(); // 부모(Enemy)가 플레이어 찾아줌
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        // 바닥 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // 바닥에 있고 점프 중이 아닐 때만 실행
        if (isGrounded && !isJumping)
        {
            StartCoroutine(JumpRoutine());
        }
    }

    public override void TakeDamage(int damage)
    {
        if (isInvincible) return;

        health -= damage;

        if (health <= 0)
        {
            base.Die();
        }
        else
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(RabbitFlashRoutine());
        }
    }

    // 래빗 전용 깜빡이 (부모의 FlashRoutine과 비슷하지만 별도로 관리)
    IEnumerator RabbitFlashRoutine()
    {
        isInvincible = true;
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        isInvincible = false;
    }

    IEnumerator JumpRoutine()
    {
        isJumping = true;

        // 1. 점프 전 잠깐의 대기 (준비 모션이 없으므로 시간만 가짐)
        yield return new WaitForSeconds(0.3f);

        // 2. 플레이어 방향 보기 (왼쪽+, 오른쪽-)
        float dir = player.position.x - transform.position.x;
        float targetScaleX = Mathf.Abs(transform.localScale.x);
        float finalScaleX = dir > 0 ? -targetScaleX : targetScaleX;
        transform.localScale = new Vector3(finalScaleX, transform.localScale.y, 1);

        // 3. 점프 실행 및 애니메이션 재생
        anim.SetTrigger("Jump"); // 애니메이터의 Jump 트리거 실행

        float jumpDir = dir > 0 ? 1f : -1f;
        rb.AddForce(new Vector2(jumpDir * jumpForceX, jumpForceY), ForceMode2D.Impulse);

        // 4. 점프 후 바닥에 닿을 때까지 대기
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => isGrounded);

        // 5. 착지 후 딜레이
        yield return new WaitForSeconds(jumpDelay);

        isJumping = false;
    }
}