using UnityEngine;

public class ExplosionParticle : MonoBehaviour
{
    public int damage = 2; // 파티클에 닿았을 때 입힐 데미지

    // 파티클 입자가 무언가(플레이어 등)에 부딪힐 때 유니티가 자동으로 실행함
    private void OnParticleCollision(GameObject other)
    {
        // 닿은 대상이 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // 기존에 사용하던 TakeDamage 호출! 
                // transform.position은 폭발이 일어난 중심점입니다.
                ph.TakeDamage(damage, transform.position);

            }
        }
    }
}