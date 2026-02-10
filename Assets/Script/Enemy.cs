using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("체력 설정")]
    public int health = 3;

    private SpriteRenderer spriteRenderer;
    private bool isInvincible = false; // 짧은 무적 시간 (연타 시 씹힘 방지)

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        health -= damage;
        Debug.Log($"적 피격! 남은 체력: {health}");

        if (health <= 0)
        {
            Die();
        }
        else
        {
            StopAllCoroutines(); // 이전 깜빡임이 있다면 멈추고 새로 시작
            StartCoroutine(FlashRoutine());
        }
    }

    IEnumerator FlashRoutine()
    {
        isInvincible = true;

        // 빨간색으로 깜빡이게 해서 피격 강조
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.1f);

        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("적 처치 완료!");
        Destroy(gameObject);
    }
}