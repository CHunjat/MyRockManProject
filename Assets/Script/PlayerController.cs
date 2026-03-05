using UnityEngine;

using UnityEngine.InputSystem;

using System.Collections;

using System;



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



    [Header("차지 시각 효과")]

    public Color mediumColor = Color.yellow;

    public Color fullChargeColor = Color.cyan;

    public float FlashSpeedValue = 20f;



    [Header("지면 체크")]

    public Transform groundCheck;

    public float groundCheckRadius = 0.1f;

    public LayerMask groundLayer;



    [Header("전투 오브젝트")]

    public GameObject bulletPrefab;

    public Transform shootPoint;

    public Transform jumpShootPoint;

    public int maxBulletCount = 3;



    [Header("사다리 컨트롤러")]

    public float climbSpeed = 1.3f;

    public bool iscliming = false;

    public bool canUseLadder = false;

    public float ladderSnapDistance = 0.3f; // 사다리 중앙 근처 체크용

    private Collider2D currentLadderCollider; // 현재 닿은 사다리 정보









    private Rigidbody2D rb;

    private Animator anim;

    private SpriteRenderer sr;

    private PlayerHealth healthScript; // 분리된 체력 스크립트 참조



    private bool isGrounded;

    private float moveInput;

    private Coroutine shootRoutine;

    private Coroutine slideRoutine;

    public bool firePending;

    void Start()

    {

        rb = GetComponent<Rigidbody2D>();

        anim = GetComponent<Animator>();

        sr = GetComponent<SpriteRenderer>();

        healthScript = GetComponent<PlayerHealth>(); // 반드시 PlayerHealth가 같이 붙어있어야 합니다.



        myCol = GetComponent<CapsuleCollider2D>();

        if (myCol != null)

        {

            originalOffset = myCol.offset;

            originalSize = myCol.size;

        }

    }



    void Update()

    {

        // 피격(넉백) 중에는 조작 불가

        if (healthScript != null && healthScript.IsHitted)

        {

            // 1. 상태 해제 (사다리에서 즉시 떨어짐)

            if (iscliming)

            {

                StopClimbing(); // 여기서 rb.gravityScale = 1f와 iscliming = false가 처리됨

            }



            if (isSliding) StopSlide();



            shootTimer = 0f;

            if (shootRoutine != null) { StopCoroutine(shootRoutine); shootRoutine = null; }



            // 2. 애니메이터 모든 파라미터 "청소"

            // 사다리 타는 중이었더라도 피격 시에는 모든 상태를 초기화해야 함

            anim.SetBool("isMoving", false);

            anim.SetBool("isShooting", false);

            anim.SetBool("JumpShoot", false);

            anim.SetBool("isClimbing", false); // [추가] 사다리 애니메이션 강제 종료



            // 3. 차지 및 시각 효과 초기화

            isCharging = false;

            chargeTimer = 0f;

            sr.color = Color.white;



            // 4. 피격 애니메이션(Hit) 적용을 위해 호출

            UpdateAnimations();

            return;

        }



        var keyboard = Keyboard.current;

        if (keyboard == null) return;



        // --- [플레이어 조작 로직] ---



        // 0. 사다리 올라가기



        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);



        if (canUseLadder && !iscliming && !isSliding && currentLadderCollider != null)

        {

            float xladder = Mathf.Abs(transform.position.x - currentLadderCollider.bounds.center.x);



            if (xladder <= ladderSnapDistance)

            {

                if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.upArrowKey.isPressed)

                {

                    transform.position = new Vector3(currentLadderCollider.bounds.center.x, transform.position.y, transform.position.z);

                    StartClimbing();

                }

            }







        }

        if (iscliming)

        {

            HandleClimbing(keyboard); // 이제 여기서 위/아래 이동을 처리

            HandleClimingShoot(keyboard); // 사다리에서의 사격 처리

            UpdateAnimations();

            return;

        }









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



        //점프시 x키 떼면 상승속도 감소 (슬라이딩 중엔 안되게)

        if (keyboard.xKey.wasReleasedThisFrame && rb.linearVelocity.y > 0)

        {

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.4f);

        }



        //슬라이딩 및 점프와 동시 입력시 안되게

        else if (keyboard.zKey.wasPressedThisFrame && isGrounded && !isSliding && Mathf.Abs(rb.linearVelocity.y) < 0.01f)

        { //점프때 y값이 변경되는걸 이용해서 막아봄...

            slideRoutine = StartCoroutine(SlideRoutine());

        }





        // 3. 차지 및 사격 로직

        if (keyboard.cKey.wasPressedThisFrame && !isSliding)

        {

            isCharging = true;

            chargeTimer = 0f;

            firePending = false;

        }



        if (keyboard.cKey.isPressed || firePending)

        {

            chargeTimer += Time.deltaTime;

            float flashing = Mathf.PingPong(Time.time * FlashSpeedValue, 1f);



            if (chargeTimer >= fullChargeTime)

                sr.color = Color.Lerp(fullChargeColor, Color.blue, flashing);

            else if (chargeTimer >= fullChargeTime / 2)

                sr.color = Color.Lerp(mediumColor, Color.cyan, flashing);

        }

        else if (!isCharging)

        {

            sr.color = Color.white;

        }



        if (keyboard.cKey.wasReleasedThisFrame && isCharging)

        {

            if (isSliding)

            {

                firePending = true;

            }

            else

            {

                HandheldChargeShot();

                ResetChargeStatus();



            }

        }



        // 4. 슬라이딩





        UpdateAnimations();









    }



    private void HandleClimingShoot(Keyboard keyboard)

    {

        if (keyboard.cKey.wasPressedThisFrame && !isSliding)

        {

            isCharging = true;

            chargeTimer = 0f;

            firePending = false;

        }



        if (keyboard.cKey.isPressed || firePending)

        {

            chargeTimer += Time.deltaTime;

            float flashing = Mathf.PingPong(Time.time * FlashSpeedValue, 1f);



            if (chargeTimer >= fullChargeTime)

                sr.color = Color.Lerp(fullChargeColor, Color.blue, flashing);

            else if (chargeTimer >= fullChargeTime / 2)

                sr.color = Color.Lerp(mediumColor, Color.cyan, flashing);

        }

        else if (!isCharging)

        {

            sr.color = Color.white;

        }



        if (keyboard.cKey.wasReleasedThisFrame && isCharging)

        {

            if (isSliding) firePending = true;

            else { HandheldChargeShot(); ResetChargeStatus(); }

        }

    }



    //사다리 올라가는 함수 (사다리 태그 필요)

    public void StartClimbing()

    {

        iscliming = true;

        rb.gravityScale = 0f; // 중력 제거

        rb.linearVelocity = Vector2.zero; // 사다리 타는 순간 속도 초기화



        shootTimer = 0f;

        if (shootRoutine != null) { StopCoroutine(shootRoutine); shootRoutine = null; } //세부조절 점프슛중 사격할때 사다리를타게될때 애니메이션 꼬이는거 방지



        anim.SetBool("JumpShoot", false); // 점프 사격 애니메이션이 켜져있다면 끔

        anim.SetBool("isClimbing", true); // 사다리 애니메이션 켜기

        anim.Play("Climb", 0, 0f);



    }



    //사다리 올라가는 중 입력 처리 함수 (사다리 태그 필요)

    void HandleClimbing(Keyboard keyboard)

    {



        //사격중에 이동불가 할지, 가능하게할지 고민인데...일단 사격중에 못움직이게 해봄

        if (shootTimer > 0)

        {

            rb.linearVelocity = Vector2.zero;

            return;

        }





        float climbInput = 0;

        if (keyboard.upArrowKey.isPressed) climbInput = 1;

        if (keyboard.downArrowKey.isPressed) climbInput = -1;



        rb.linearVelocity = new Vector2(0, climbInput * climbSpeed);







        //사다리에서 방향전환(사격하기위함)

        if (keyboard.leftArrowKey.wasPressedThisFrame) transform.localScale = new Vector3(-1, 1, 1);

        else if (keyboard.rightArrowKey.wasPressedThisFrame) transform.localScale = new Vector3(1, 1, 1);



        if (climbInput < 0 && isGrounded)

        {

            StopClimbing();

            anim.SetBool("isClimbing", false);

            return;

        }





        // 사다리에서 점프 시 탈출

        if (keyboard.xKey.wasPressedThisFrame)

        {

            StopClimbing();

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.5f);

        }





    }



    IEnumerator ClimbOutRoutine(Vector3 targetPos)

    {



        float ladderTopY = currentLadderCollider.bounds.max.y;

        float ladderCenterX = currentLadderCollider.bounds.center.x;



        iscliming = false;

        rb.linearVelocity = Vector2.zero;

        rb.gravityScale = 0f;

        myCol.enabled = false;

        anim.Play("ClimbOut"); // 

        yield return new WaitForSeconds(0.6f);



        myCol.enabled = true;



        rb.gravityScale = 1f;

        rb.linearVelocity = new Vector2(0, -2f);

        isGrounded = true;

        UpdateAnimations();

    }



    //멈추는함수

    public void StopClimbing()

    {

        iscliming = false;

        rb.gravityScale = 1f; // 중력 복구

        anim.SetBool("isClimbing", false);

    }



    //사다리 트리거 감지 함수 (사다리 태그 필요)

    private void OnTriggerEnter2D(Collider2D collision)

    {

        if (collision.CompareTag("Ladder"))

        {

            canUseLadder = true;

            currentLadderCollider = collision; // 현재 닿은 사다리 정보 저장

        }

    }

    //사다리에서 벗어날 때 감지 함수 (사다리 태그 필요)

    private void OnTriggerExit2D(Collider2D collision)

    {

        if (collision.CompareTag("Ladder"))

        {

            canUseLadder = false;

            currentLadderCollider = null; // 사다리 정보 초기화

            StopClimbing();

        }

    }







    void HandheldChargeShot()

    {

        Transform target = (!iscliming || isGrounded) ? shootPoint : jumpShootPoint;

        float direction = transform.localScale.x;



        if (chargeTimer >= fullChargeTime)

        {

            GameObject shot = Instantiate(chargeShotPrefab, target.position, Quaternion.identity);

            if (direction < 0) shot.transform.rotation = Quaternion.Euler(0, 180, 0);

            StartShootAnimation();

        }

        else if (chargeTimer >= fullChargeTime / 2)

        {

            GameObject mediumBullet = Instantiate(mediumShotPrefab, target.position, Quaternion.identity);

            if (direction < 0) mediumBullet.transform.rotation = Quaternion.Euler(0, 180, 0);

            StartShootAnimation();

        }

        else

        {

            if (CanShoot())

            {

                OnShoot();

                StartShootAnimation();

            }

        }

    }



    void StartShootAnimation()

    {

        lastShootTime = Time.time;

        shootTimer = shootPoseDuration;

        if (shootRoutine != null) StopCoroutine(shootRoutine);

        shootRoutine = StartCoroutine(ShootStopTimer());

    }



    public void ResetChargeStatus()

    {

        isCharging = false;

        firePending = false;

        chargeTimer = 0f;

        sr.color = Color.white; // 반짝이던 색상을 흰색으로 강제 복구

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

            if (healthScript != null && healthScript.IsHitted) break;

            rb.linearVelocity = new Vector2(slideDir * slideSpeed, rb.linearVelocity.y);

            yield return null;

        }



        //IEumerator와 yield return null 세트



        StopSlide();

    }



    public void StopSlide()

    {

        isSliding = false;

        if (slideRoutine != null) { StopCoroutine(slideRoutine); slideRoutine = null; }



        anim.SetBool("isSliding", false);

        myCol.size = originalSize;

        myCol.offset = originalOffset;



        if (firePending && isCharging)

        {

            HandheldChargeShot();

            ResetChargeStatus();



        }



    }



    bool CanShoot()

    {

        int currentBullets = GameObject.FindGameObjectsWithTag("Bullet").Length;

        return currentBullets < maxBulletCount && Time.time - lastShootTime > shootDelay;

    }



    void UpdateAnimations()

    {

        //if (healthScript != null && healthScript.IsHitted)

        //{

        //    anim.speed = 1f;

        //    anim.Play("Hit");

        //    return;

        //}



        var keyboard = Keyboard.current;



        if (iscliming)

        {

            anim.SetBool("isClimbing", true);

            anim.SetBool("isShooting", shootTimer > 0); // 사격 여부 전달



            // [수정] 사격 중이면 속도 1(고정), 아니면 움직일 때만 1

            if (shootTimer > 0)

            {

                anim.speed = 1f;

                anim.SetBool("isShooting", true);

            }

            else

            {

                anim.SetBool("isShooting", false);

                bool isMovingOnLadder = keyboard.upArrowKey.isPressed || keyboard.downArrowKey.isPressed;

                anim.speed = isMovingOnLadder ? 1f : 0f;

            }

        }

        else

        {

            anim.speed = 1f;

            anim.SetBool("isClimbing", false);

            anim.SetBool("isMoving", Mathf.Abs(moveInput) > 0.01f && !isSliding);

            anim.SetBool("isGrounded", isGrounded);

            anim.SetBool("isShooting", shootTimer > 0);

            anim.SetBool("isSliding", isSliding);

        }

    }



    IEnumerator ShootStopTimer()

    {

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

        Transform target = (iscliming || !isGrounded) ? shootPoint : jumpShootPoint;

        if (iscliming) target = jumpShootPoint; // 사다리 사격 위치

        GameObject bullet = Instantiate(bulletPrefab, target.position, Quaternion.identity);

        if (transform.localScale.x < 0) bullet.transform.rotation = Quaternion.Euler(0, 180, 0);

    }





}

