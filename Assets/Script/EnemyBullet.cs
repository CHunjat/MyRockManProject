using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 1;
    private Rigidbody2D rb;
    private float moveDir = 0f;

    // AI가 총알을 만들고 이 함수를 호출해서 방향을 정해줄 겁니다.
    public void SetDirection(float dir)
    {
        moveDir = dir;

        // Rigidbody가 아직 안 잡혔을 수도 있으니 여기서 한 번 더 체크
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // 방향에 따라 속도 설정 (dir이 -1이면 오른쪽, 1이면 왼쪽)
        // 만약 반대로 날아간다면 아래 -dir 로 수정하면 됩니다.
        rb.linearVelocity = new Vector2(-moveDir * speed, 0);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, 3f);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // 록맨의 TakeDamage 호출 (데미지, 총알위치)
                playerHealth.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}