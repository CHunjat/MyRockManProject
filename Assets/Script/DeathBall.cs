using UnityEngine;
using System.Collections; // 코루틴을 위해 필요!

public class DeathBall : MonoBehaviour
{
    public float speed = 5f;
    public float blinkInterval = 0.05f; // 반짝이는 속도 (매우 빠르게)
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 외부(Player)에서 방향을 정해줄 함수
    public void SetDirection(Vector2 dir)
    {
        rb.linearVelocity = dir.normalized * speed;
        StartCoroutine(BlinkEffect());
    }

    private IEnumerator BlinkEffect()
    {
        while (true) // 파괴될 때까지 무한 반복
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.color = new Color(0.4f, 0.7f, 1f); // 하늘색
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    void Start()
    {
        // 1.5초 뒤에 자동으로 사라짐
        Destroy(gameObject, 1.5f);
    }
}