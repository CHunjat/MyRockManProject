using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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


    [Header("죽음")]
    public GameObject deathBallprefab;

    private bool isInvincible = false;
    public bool IsHitted { get; private set; }

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private PlayerController playerController;

    public static int LifeCount = 3;

    public static int ECanCount = 0; // 초기 보유량 0개
    public const int MaxECan = 3;   // 최대 3개까지 소지 가능

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
        if (UIManager.instance != null)
            UIManager.instance.UpdateHP(currentHealth, maxHealth);
    }

    // --- [피격 처리 핵심 로직] ---

    // 1. 트리거 진입 시 (몸체에 닿거나 총알이 트리거일 때)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 어떤 레이어든, 어떤 태그든 '물리적 영역'에 닿기만 하면 무조건 찍혀야 함
        //Debug.Log("<color=red>물리적 접촉 발생!</color> 대상: " + collision.name);

        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                TakeDamage(enemy.contactDamage, collision.transform.position);
            }
        }
    }

    // 2. 트리거 체류 시 (적 몸 안에서 무적이 풀리는 즉시 다시 맞게 함)
    private void OnTriggerStay2D(Collider2D collision)
    {
        HandleDamageLogic(collision.gameObject, collision.transform.position);
    }

    // [데미지 판정 공통 함수]
    private void HandleDamageLogic(GameObject obj, Vector2 contactPos)
    {
        // 무적 상태면 무시
        if (isInvincible) return;

        // 적 몸체(Enemy) 혹은 적 총알(EnemyBullet) 태그 확인
        if (obj.CompareTag("Enemy") || obj.CompareTag("EnemyBullet"))
        {
            // 디버그 로그: 어떤 물체와 닿았는지 콘솔창에 표시
            //Debug.Log("피격 감지! 대상: " + obj.name + " | 태그: " + obj.tag);

            // 데미지 수치 결정 (총알은 2, 몸빵은 3)
            int damage = obj.CompareTag("EnemyBullet") ? 2 : 3;

            // 피격 함수 호출
            TakeDamage(damage, contactPos);

            // 총알이었다면 제거
            if (obj.CompareTag("EnemyBullet"))
            {
                Destroy(obj);
            }
        }
    }

    // --- [기존 피격 시스템] ---

    public void TakeDamage(int damage, Vector2 enemyPos)
    {
        if (isInvincible) return;

        if (playerController != null)
        {
            playerController.StopClimbing();
            playerController.StopSlide();
            playerController.firePending = false;
            playerController.ResetChargeStatus();
        }

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        if (UIManager.instance != null)
            UIManager.instance.UpdateHP(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(KnockbackRoutine(enemyPos));
    }

    IEnumerator KnockbackRoutine(Vector2 enemyPos)
    {
        IsHitted = true;
        isInvincible = true;

        if (anim != null)
        {
            anim.SetBool("isInvincible", true);
            anim.Play("Hit", 0, 0f);
        }

        float pushDir = transform.position.x < enemyPos.x ? -1f : 1f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(pushDir * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        IsHitted = false;
        if (anim != null) anim.SetBool("isInvincible", false);

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

    public void Heal(int amount)
    {
        if(currentHealth >= maxHealth) return;

        StartCoroutine(HealRoutine(amount));
    }

    IEnumerator HealRoutine(int amount)
    {
        Time.timeScale = 0f; // 게임 일시정지

        int targetHealth = Mathf.Min(currentHealth + amount, maxHealth);

        while (currentHealth < targetHealth)
        {
            currentHealth++; // 1칸 증가
            // UI 갱신 (UIManager에 현재 수치 전달)
            if (UIManager.instance != null)
                UIManager.instance.UpdateHP(currentHealth, maxHealth);


            // [효과음] 넣기

            // SoundManager.Instance.PlaySFX("HealTick"); 



            // 3. 게임이 멈춰있으므로 '실제 시간' 기준으로 대기 (약 0.05초)
            yield return new WaitForSecondsRealtime(0.1f);

        }
        Time.timeScale = 1f; // 다시 게임시작


    }


    public void Die()
    {
        Vector2[] directions =
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right,
            new Vector2(1, 1),
            new Vector2(1, -1),
            new Vector2(-1, 1),
            new Vector2(-1, -1)
        };

        foreach (Vector2 dir in directions)
        {
            GameObject ball = Instantiate(deathBallprefab, transform.position, Quaternion.identity);
            ball.GetComponent<DeathBall>().SetDirection(dir);
            //반짝거리는 연출 추가       
        }

        sr.enabled = false;           // 1. 캐릭터 이미지만 안 보이게 함
        rb.simulated = false;
        LifeCount--;

        if (UIManager.instance != null)
            UIManager.instance.UpdateLifeUI(LifeCount);

        if (LifeCount >= 0) //LFECOUNT이 0보다 클 때만 목숨 감소시키기
        {
            
            StartCoroutine(RestartStageAfterDelay(3f)); //문법 외워두기, 3초 뒤에 RestartStageAfterDelay 코루틴 실행해서 씬 재시작
        }  
        else if(LifeCount < 0) //LIFECOUNT이 0보다 작을때
        {
            Time.timeScale = 1f;
            
            StartCoroutine(GameOverAfterDelay(3f)); //문법 외워두기, 3초 뒤에 GameOverAfterDelay 코루틴 실행해서 게임오버 씬으로 이동
        }
    }

    //코루틴 사용 이너머레이터랑 같이 ㄱㄱㄱㄱ
    private IEnumerator RestartStageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 현재 씬을 다시 로드 (처음부터 시작)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator GameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }
}