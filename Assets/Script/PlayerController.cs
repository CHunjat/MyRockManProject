using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 점프")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;

    [Header("슬라이딩 설정")]
    public float slideSpeed = 12f;
    public float slideDuration = 0.4f;
    private bool isSliding = false;
    private CapsuleCollider2D myCol;
    private Vector2 originalOffset;
    private Vector2 originalSize;

    [Header("슬라이딩 콜라이더 조절 (인스펙터에서 맞추세요)")]
    [SerializeField] private float slideSizeY = 1.0f;    // 슬라이딩 시 캡슐 높이
    [SerializeField] private float slideOffsetY = -0.5f; // 슬라이딩 시 캡슐 중심점(Y)

    [Header("사격 설정")]
    public float shootDelay = 0.15f;
    public float shootPoseDuration = 0.4f;
    private float lastShootTime;
    private float shootTimer = 0f;

    [Header("피격 및 무적")]
    public int health = 3;
    public float knockbackForceX = 7f;
    public float knockbackForceY = 8f;
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private float invincibilityTime = 1.5f;
    [SerializeField] private float flashSpeed = 0.08f;

    [Header("지면 체크")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("전투 오브젝트")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public Transform jumpShootPoint;
    public int maxBulletCount = 3;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private bool isGrounded;
    private bool isHitted = false;
    private bool isInvincible = false;
    private float moveInput;
    private Coroutine shootRoutine;
    private Coroutine slideRoutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // 슬라이딩 시 콜라이더 크기 조절을 위한 초기값 저장
        myCol = GetComponent<CapsuleCollider2D>();
        if (myCol != null)
        {
            originalOffset = myCol.offset;
            originalSize = myCol.size;
        }
    }

    void Update()
    {
        if (isHitted) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 1. 이동 (슬라이딩 중에는 방향 전환 및 이동 입력 잠금)
        if (!isSliding)
        {
            moveInput = 0;
            if (keyboard.leftArrowKey.isPressed) moveInput = -1;
            if (keyboard.rightArrowKey.isPressed) moveInput = 1;
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

            if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
            else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
        }

        // 2. 지면 체크 및 점프
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 점프 시 슬라이딩 즉시 종료
        if (keyboard.xKey.wasPressedThisFrame && isGrounded)
        {
            if (isSliding) StopSlide();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 3. 사격 로직 (C키)
        if (keyboard.cKey.wasPressedThisFrame && !isSliding)
        {
            if (CanShoot())
            {
                lastShootTime = Time.time;
                OnShoot();
                shootTimer = shootPoseDuration;
                if (shootRoutine == null)
                    shootRoutine = StartCoroutine(ShootStopTimer());
            }
        }

        // 4. 슬라이딩 로직 (Z키)
        if (keyboard.zKey.wasPressedThisFrame && isGrounded && !isSliding)
        {
            slideRoutine = StartCoroutine(SlideRoutine());
        }

        UpdateAnimations();
    }

    IEnumerator SlideRoutine()
    {
        isSliding = true;
        anim.SetBool("isSliding", true);

        // 콜라이더 높이 절반으로 축소
        myCol.size = new Vector2(originalSize.x, originalSize.y * 0.5f);
        //myCol.offset = new Vector2(originalOffset.x, originalOffset.y - (originalSize.y * 0.25f));

        float slideDir = transform.localScale.x > 0 ? 1f : -1f;
        float timer = 0f;

        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
            // 슬라이딩 중 피격 시 즉시 중단
            if (isHitted) break;

            rb.linearVelocity = new Vector2(slideDir * slideSpeed, rb.linearVelocity.y);
            yield return null;
        }

        StopSlide();
    }

    void StopSlide()
    {
        if (slideRoutine != null) { StopCoroutine(slideRoutine); slideRoutine = null; }

        isSliding = false;
        anim.SetBool("isSliding", false);

        // 콜라이더 복구
        myCol.size = originalSize;
        myCol.offset = originalOffset;
    }

    bool CanShoot()
    {
        int currentBullets = GameObject.FindGameObjectsWithTag("Bullet").Length;
        return currentBullets < maxBulletCount && Time.time - lastShootTime > shootDelay;
    }

    void UpdateAnimations()
    {
        if (isHitted) return;
        anim.SetBool("isMoving", Mathf.Abs(moveInput) > 0.01f);
        anim.SetBool("isGrounded", isGrounded);
    }

    IEnumerator ShootStopTimer()
    {
        anim.SetBool("isShooting", true);
        while (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
            yield return null;
        }
        anim.SetBool("isShooting", false);
        shootRoutine = null;
    }

    void OnShoot()
    {
        Transform target = isGrounded ? shootPoint : jumpShootPoint;
        GameObject bullet = Instantiate(bulletPrefab, target.position, Quaternion.identity);
        if (transform.localScale.x < 0) bullet.transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision) => HandleCollision(collision);
    private void OnCollisionStay2D(Collision2D collision) => HandleCollision(collision);
    private void HandleCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
            TakeDamage(1, collision.transform.position);
    }

    public void TakeDamage(int damage, Vector2 enemyPos)
    {
        if (isSliding) StopSlide();

        health -= damage;
        shootTimer = 0;
        anim.SetBool("isShooting", false);
        if (shootRoutine != null) { StopCoroutine(shootRoutine); shootRoutine = null; }

        isInvincible = true;
        anim.SetBool("isInvincible", true);
        anim.Play("Hit", 0, 0f);
        StartCoroutine(KnockbackRoutine(enemyPos));
    }

    IEnumerator KnockbackRoutine(Vector2 enemyPos)
    {
        isHitted = true;
        float pushDir = transform.position.x < enemyPos.x ? -1f : 1f;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(pushDir * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);
        isHitted = false;
        anim.SetBool("isInvincible", false);

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
}