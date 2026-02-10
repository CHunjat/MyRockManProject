using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    public float speed = 10f;
    [SerializeField]
    public float lifeTiem = 1.1f; // 오타까지 형님 스타일대로 보존했습니다! (원래 LifeTime)
    [SerializeField]
    public int damage = 1; // 데미지 수치 추가

    private void Start()
    {
        // 시간이 지나면 삭제
        Destroy(gameObject, lifeTiem);
    }

    private void Update()
    {
        // 총알 속도 앞으로 나아가기
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 벽(Wall)에 닿으면 삭제
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        // 2. 적(Enemy)에 닿으면 데미지 주고 삭제
        else if (collision.CompareTag("Enemy"))
        {
            // 적의 Enemy 스크립트를 가져와서 데미지 입히기
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}