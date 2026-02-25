using UnityEngine;

public class SkullBomb : MonoBehaviour
{
    public GameObject explosionEffect;
    public int damage = 3;
    private bool hasExploded = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded) return;

        if (collision.CompareTag("Player"))
        {
            // [직접 맞는 경우]
            PlayerHealth ph = collision.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage, transform.position);
            }
            Explode();
        }
        else if (collision.CompareTag("Wall"))
        {
            // [벽이나 땅에 맞는 경우]
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;

        // 폭탄의 콜라이더와 이미지를 즉시 꺼서 중복 충돌 방지
        GetComponent<Collider2D>().enabled = false;

        if (explosionEffect != null)
        {
            // 파티클 생성 (이 파티클 안에는 OnParticleCollision 스크립트가 있어야 함)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}