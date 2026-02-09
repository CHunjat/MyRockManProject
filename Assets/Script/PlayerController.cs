using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 점프 설정")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;

    [Header("공격 설정")]
    public float shootDelay = 0.5f; // 연타 간격
    public float shootPoseDuration = 0.4f; // 사격 포즈 유지 시간
    private float lastShootTime;

    [Header("땅체크 설정")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    private Animator anim;
    private Coroutine shootRoutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 1. 이동 입력 및 물리 처리
        moveInput = 0;
        if (keyboard.leftArrowKey.isPressed) moveInput = -1;
        if (keyboard.rightArrowKey.isPressed) moveInput = 1;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // 2. 캐릭터 방향 전환
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        // 3. 지면 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 4. 점프 로직
        if (keyboard.xKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 5. 공격 (지상/공중 통합)
        if (keyboard.cKey.wasPressedThisFrame)
        {
            // 실제 사격 간격 체크
            if (Time.time - lastShootTime > shootDelay)
            {
                lastShootTime = Time.time;
                Shoot();

                // 이미 실행 중인 코루틴이 있다면 멈추고 새로 시작 (연타 시 포즈 유지 시간 갱신)
                if (shootRoutine != null) StopCoroutine(shootRoutine);
                shootRoutine = StartCoroutine(ShootStopTimer());
            }
        }

        // 6. 애니메이터 파라미터 업데이트
        anim.SetBool("isMoving", moveInput != 0);
        anim.SetBool("isGrounded", isGrounded);
        // isShooting은 코루틴에서 별도로 제어됩니다.
    }

    // 사격 상태를 일정 시간 유지해주는 코루틴
    IEnumerator ShootStopTimer()
    {
        // 사격 애니메이션 On
        anim.SetBool("isShooting", true);

        // 이 시간 동안은 애니메이션이 '사격' 상태를 유지합니다.
        yield return new WaitForSeconds(shootPoseDuration);

        // 시간 종료 후 사격 애니메이션 Off (다시 일반 걷기/점프 모션으로 복귀)
        anim.SetBool("isShooting", false);
        shootRoutine = null;
    }

    void Shoot()
    {
        Debug.Log("총뿅뿅!");
        // 여기에 실제 총알 생성 코드를 넣으세요.
    }
}