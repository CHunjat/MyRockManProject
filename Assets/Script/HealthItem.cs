using UnityEngine;

public class HealthItem : MonoBehaviour
{
    [Header("체력아이템 수치조절")]
    public int healAmount = 2; // 작은 알약은 2, 큰 건 10 정도 //유니티에서 각자 설정
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
            //플레이어가 넉백중에는 아이템이 먹어지지 않는다

            if (ph == null || ph.IsHitted)
            {
               return;
            }

            ph.Heal(healAmount);
            Destroy(gameObject);



        }
    }
}