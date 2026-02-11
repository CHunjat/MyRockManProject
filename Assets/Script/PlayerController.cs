using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Mathematics;

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

    [Header("슬라이딩 콜라이더 조절")]
    [SerializeField] private float slideSizeY = 1.0f;
    [SerializeField] private float slideOffsetY = -0.5f;

    [Header("사격 설정")]
    public float shootDelay = 0.15f;
    public float shootPoseDuration = 0.4f; // 팔 뻗고 있는 시간
    private float lastShootTime;
    private float shootTimer = 0f;

    [Header("차지 샷 설정")]
    public GameObject mediumShotPrefab;
    public GameObject chargeShotPrefab;
    public float fullChargeTime = 1.2f;

    [SerializeField] private float chargeTimer = 0f;
    private bool isCharging = false;

    [Header("차지 시각 효과 (색상)")]
    public Color mediumColor = Color.yellow;
    public Color fullChargeColor = Color.cyan;
    public float FlashSpeedValue = 20f;

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

        // 1. 이동
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

        if (keyboard.xKey.wasPressedThisFrame && isGrounded)
        {
            if (isSliding) StopSlide();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 3. 차지 및 사격 통합 로직
        // 차징 시작
        if (keyboard.cKey.wasPressedThisFrame && !isSliding)
        {
            isCharging = true;
            chargeTimer = 0f;
        }

        // 차징 중
        if (keyboard.cKey.isPressed && isCharging && !isSliding)
        {
            chargeTimer += Time.deltaTime;
            float flashing = Mathf.PingPong(Time.time * FlashSpeedValue, 1f);

            if (chargeTimer >= fullChargeTime)
                sr.color = Color.Lerp(fullChargeColor, Color.blue, flashing);
            else if (chargeTimer >= fullChargeTime / 2) 
                sr.color = Color.Lerp(mediumColor, Color.cyan, flashing);
        }

        // 발사 (차지 샷 또는 일반 샷 결정)
        if (keyboard.cKey.wasReleasedThisFrame && isCharging)
        {
            HandheldChargeShot();

            isCharging = false;
            chargeTimer = 0f;
            sr.color = Color.white;
        }

        // 4. 슬라이딩
        if (keyboard.zKey.wasPressedThisFrame && isGrounded && !isSliding)
        {
            slideRoutine = StartCoroutine(SlideRoutine());
        }

        UpdateAnimations();
    }

    void HandheldChargeShot()
    {
        Transform target = isGrounded ? shootPoint : jumpShootPoint;
        float direction = transform.localScale.x;

        // [핵심] 차지 단계에 상관없이 쏘는 순간 애니메이션 타이머를 작동시킴
        if (chargeTimer >= fullChargeTime)
        {
            // 3단계
            GameObject shot = Instantiate(chargeShotPrefab, target.position, Quaternion.identity);
            if (direction < 0) shot.transform.rotation = Quaternion.Euler(0, 180, 0);
            StartShootAnimation();
        }
        else if (chargeTimer >= fullChargeTime / 2)
        {
            // 2단계
            GameObject mediumBullet = Instantiate(mediumShotPrefab, target.position, Quaternion.identity);
            if (direction < 0) mediumBullet.transform.rotation = Quaternion.Euler(0, 180, 0);
            StartShootAnimation();
        }
        else
        {
            // 1단계 (일반 사격)
            if (CanShoot())
            {
                OnShoot();
                StartShootAnimation();
            }
        }
    }

    // 애니메이션을 위해 타이머를 세팅하고 코루틴을 돌리는 공통 함수
    void StartShootAnimation()
    {
        lastShootTime = Time.time;
        shootTimer = shootPoseDuration; // 이 값을 0보다 크게 만들어야 UpdateAnimations에서 인식함
        if (shootRoutine != null) StopCoroutine(shootRoutine);
        shootRoutine = StartCoroutine(ShootStopTimer());
    }

    IEnumerator SlideRoutine()
    {
        isSliding = true;
        anim.SetBool("isSliding", true);
        myCol.size = new Vector2(originalSize.x, originalSize.y * 0.5f);

        float slideDir = transform.localScale.x > 0 ? 1f : -1f;
        float timer = 0f;

        while (timer < slideDuration)
        {
            timer += Time.deltaTime;
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

        bool isMoving = Mathf.Abs(moveInput) > 0.01f && !isSliding;
        bool isShooting = shootTimer > 0; // shootTimer가 돌아가고 있어야 true가 됨

        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isShooting", isShooting);
    }

    IEnumerator ShootStopTimer()
    {
        // UpdateAnimations가 isShooting을 true로 인식하도록 shootTimer를 깎는 루프
        while (shootTimer > 0)
        {
            shootTimer -= Time.deltaTime;
            yield return null;
        }
        shootTimer = 0;
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