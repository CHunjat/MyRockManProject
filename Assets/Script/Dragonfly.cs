using UnityEngine;

public class Dragonfly : Enemy
{
    [Header("비행 설정")]
    public float moveSpeed = 3f;      // 투하 후 직진 속도
    public float chaseSpeed = 2f;     // 발견 후 추격 속도
    public float waveFrequency = 4f;  // 물결 속도
    public float waveAmplitude = 1.2f; // 물결 높이

    [Header("탐지 설정")]
    public float detectionRange = 7f; // 록맨을 발견하는 거리
    private bool isChasing = false;   // 현재 추격 중인지 여부

    [Header("폭탄 설정")]
    public GameObject bombPrefab;
    public Transform dropPoint;

    [Header("정찰 설정")]
    public float patrolRange = 2f;    // 좌우로 움직일 범위
    public float patrolSpeed = 2f;    // 좌우로 움직이는 속도

    private Animator anim;
    private bool hasDropped = false;
    private float startTime;
    private float currentX;
    private float startY;
    private float moveDir = -1f;
    private Vector2 startPos;         // 기준점 저장을 위한 변수 추가

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
        startTime = Time.time;
        currentX = transform.position.x;
        startPos = transform.position;
        startY = transform.position.y;

    }

    void Update()
    {
        if (player == null) return;

        // 1. 상태 결정 로직
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 아직 발견 전이고, 록맨이 사거리 안에 들어오면 추격 시작
        if (!isChasing && distanceToPlayer <= detectionRange)
        {
            isChasing = true;
            Debug.Log("잠자리가 록맨을 발견했습니다!");
        }

        // 2. 이동 및 공격 로직
        if (!isChasing)
        {
           
            float patrolX = startPos.x + Mathf.Sin(Time.time * patrolSpeed) * patrolRange;
            currentX = patrolX;
        }
        else if (!hasDropped)
        {
            // [상태 2: 추격 비행] 록맨의 X축을 향해 이동
            currentX = Mathf.MoveTowards(currentX, player.position.x, chaseSpeed * Time.deltaTime);

            // 머리 위 도착 시 폭탄 투하
            if (Mathf.Abs(currentX - player.position.x) < 0.2f)
            {
                moveDir = (player.position.x < transform.position.x) ? -1f : 1f;
                DropBomb();
                OnBecameInvisible();
            }
        }
        else
        {
            // [상태 3: 투하 후 퇴장] 결정된 방향으로 직진
            currentX += moveDir * moveSpeed * Time.deltaTime;
        }

        // Y축 물결 운동은 어떤 상태든 항상 적용 (록맨 6 느낌)
        float waveY = startY + Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        transform.position = new Vector3(currentX, waveY, 0);
    }

    void DropBomb()
    {
        hasDropped = true;
        if (anim != null) anim.SetBool("hasBomb", false);

        if (bombPrefab != null)
        {
            Instantiate(bombPrefab, dropPoint.position, Quaternion.identity);
        }
    }

    private void OnBecameInvisible()
    {
        if (hasDropped)
        {
            Destroy(gameObject, 5f);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}

   