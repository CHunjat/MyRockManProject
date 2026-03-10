using UnityEngine;

public class LifeItem : MonoBehaviour
{
    public float lifeTime = 30f; // 안 먹으면 30초 뒤 소멸

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
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

            if(PlayerHealth.LifeCount < 9 )
                PlayerHealth.LifeCount++; // 목숨 수 증가 최대 9개까지 제한

            //Debug.Log("<color=green>목숨 아이템 획득! 현재 목숨 수: " + ph.LifeCount + "</color>");

            // UI 업데이트: 목숨 수가 변경될 때마다 UI를 갱신하도록 호출
            if (UIManager.instance != null)
            {
                UIManager.instance.UpdateLifeUI(PlayerHealth.LifeCount);
            }
            Destroy(gameObject);



        }
    }
}
