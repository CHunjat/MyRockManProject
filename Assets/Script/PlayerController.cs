using UnityEngine;
using UnityEngine.InputSystem; // ���� �Է� �ʼ�

public class PlayerController : MonoBehaviour
{
    [Header("애니메이션 파라미터 업데이트")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;

    [Header("공격 설정")]
    public float shootDelay = 0.3f; // 공격 간격 (0.2초 정도가 적당)
    private float lastShootTime;

    [Header("땅체크 설정")]
    public Transform groundCheck; // �߹� ��ġ (�� ������Ʈ)
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer; // Ground ���̾� ���� �ʿ�

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    private Animator anim;
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
        // 5. 공격
        if (keyboard.cKey.wasPressedThisFrame) // 키를 누른 그 순간에만 실행
        {
            if (Time.time - lastShootTime > shootDelay)
            {
                lastShootTime = Time.time; // 현재 시간 기록

                Shoot();
                anim.ResetTrigger("atShoot");
                anim.SetTrigger("atShoot");
            }
        }

        bool isActuallyMoving = moveInput != 0;
        bool isCKeyHeld = keyboard.cKey.isPressed;

        // 이동 중이거나, C키를 누르고 있는 동안에는 isMoving을 true로 고정
        //anim.SetBool("isMoving", isActuallyMoving || isCKeyHeld);
        //anim.SetBool("isGrounded", isGrounded);


        // 이동 중인지 여부 (속도의 절대값이 0보다 크면 걷는 중)
        anim.SetBool("isMoving", moveInput != 0);

        // 바닥에 있는지 여부 전달
        anim.SetBool("isGrounded", isGrounded);
    }
    void Shoot()
    {
        Debug.Log("총뿅뿅!");
       
    }
}