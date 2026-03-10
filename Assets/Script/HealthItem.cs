using UnityEngine;

public class HealthItem : MonoBehaviour
{
    public enum ItemType 
    { 
        Small, Large, ecan 
    }


    [Header("아이템 설정")]
    public ItemType itemType; // 아이템 종류 설정 (인스펙터에서 선택)
    public int healAmount = 2; // 작은 알약은 2, 큰 건 10 정도 //유니티에서 각자 설정
    public float lifeTime = 5f; // 안 먹으면 5초 뒤 소멸


    void Start()
    {
        if (itemType != ItemType.ecan)
        {
            Destroy(gameObject, lifeTime);
        }
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

            if (itemType == ItemType.ecan)
            {
                // 1. E캔일 때: 인벤토리 저장 (최대 3개)
                if (PlayerHealth.ECanCount < PlayerHealth.MaxECan)
                {
                    PlayerHealth.ECanCount++;

                    // UI 업데이트 (UIManager에 ECan 관련 함수가 있어야 함)
                    if (UIManager.instance != null)
                        UIManager.instance.UpdateECanUI(PlayerHealth.ECanCount);
                    Destroy(gameObject);


                }
            }
            else
            {
                // 2. 일반 힐템일 때: 즉시 회복 (이미 풀피면 안 먹어짐)
                if (ph.currentHealth < ph.maxHealth)
                {
                    ph.Heal(healAmount);
                }
                Destroy(gameObject);

            }
        }
    }
}