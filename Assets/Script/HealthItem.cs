using UnityEngine;

public class HealthItem : MonoBehaviour
{
    [Header("체력아이템 수치조절")]
    public int healAmount = 2; // 작은 알약은 2, 큰 건 10 정도
    public float lifeTime = 5f; // 안 먹으면 5초 뒤 소멸

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth ph = collision.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.Heal(healAmount);
                Destroy(gameObject);
            }
        }
    }
}