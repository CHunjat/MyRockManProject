using UnityEngine;

public class SkullBomb : MonoBehaviour
{
    // 만약 trigger가 아니라 물리 충돌을 원하시면 OnCollisionEnter2D를 쓰세요.
    // 여기서는 기존 코드(OnTriggerEnter2D)를 보강합니다.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"폭탄 충돌 발생: {collision.name}"); // 로그가 찍히는지 확인!

        if (collision.CompareTag("Player"))
        {
            // 록맨의 스크립트를 가져와서 데미지 주기
            // collision.GetComponent<PlayerController>().TakeDamage(damage);
            Explode();
        }
        else if (collision.CompareTag("Wall"))
        {
            Explode();
        }
    }

    void Explode()
    {
        // 펑 터지는 이펙트 생성 로직 추가 가능
        Destroy(gameObject);
    }
}