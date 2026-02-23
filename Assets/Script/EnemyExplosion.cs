using UnityEngine;

public class ParticleAutoDestroy : MonoBehaviour
{
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        // 파티클의 지속 시간 + 남은 입자 생존 시간만큼 뒤에 파괴
        Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }
}