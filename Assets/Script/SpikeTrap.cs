using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    private int spikeDamage = 99; // 가시 트랩이 주는 데미지
    private bool isDeath = true; // 즉사 모드 여부

    //

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // 즉사 모드면 현재 체력만큼 데미지를 줘서 바로 보냄
                int damageToGive = isDeath ? playerHealth.maxHealth : spikeDamage;

                playerHealth.TakeDamage(damageToGive, transform.position);
            }
        }
    }
}
