using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHealth = 28;
    public int currentHealth;

    [Header("넉백 설정")]
    public float knockbackForceX = 7f;
    public float knockbackForceY = 8f;
    public float knockbackDuration = 0.5f;

    [Header("무적 설정")]
    public float invincibilityTime = 1.5f;
    public float flashSpeed = 0.08f;

    private bool isInvincible = false;
    public bool IsHitted { get; private set; } // Controller에서 참조 가능

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private PlayerController playerController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        // UI 초기화
        if (UIManager.instance != null)
            UIManager.instance.UpdateHP(currentHealth, maxHealth);
    }

    // 피격 로직
    public void TakeDamage(int damage, Vector2 enemyPos)
    {
        if (isInvincible) return;

        // 슬라이딩 중 피격 시 슬라이딩 중단
        playerController.StopSlide();

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // UI 업데이트
        if (UIManager.instance != null)
            UIManager.instance.UpdateHP(currentHealth, maxHealth);

        // 사망 체크
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 넉백 및 무적 시작
        StartCoroutine(KnockbackRoutine(enemyPos));
    }

    IEnumerator KnockbackRoutine(Vector2 enemyPos)
    {
        IsHitted = true;
        isInvincible = true;
        anim.SetBool("isInvincible", true);
        anim.Play("Hit", 0, 0f);

        // 넉백 방향 계산
        float pushDir = transform.position.x < enemyPos.x ? -1f : 1f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(pushDir * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        IsHitted = false;
        anim.SetBool("isInvincible", false);

        // 무적 시간 동안 깜빡임
        float timer = 0;
        while (timer < (invincibilityTime - knockbackDuration))
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(flashSpeed);
            timer += flashSpeed;
        }

        sr.enabled = true;
        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("록맨 사망!");
        // 여기에 사망 애니메이션이나 리로드 로직 추가
    }

    private void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision);
    private void OnCollisionStay2D(Collision2D collision) => HandleCollision(collision);

    private void HandleCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
        {
            TakeDamage(1, collision.transform.position);
        }
    }
}