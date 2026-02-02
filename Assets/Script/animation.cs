using System.Collections;
using UnityEngine;

public class animation : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;
    void Start()
    {
        // 5초 뒤에 180도 회전시키는 코루틴 시작
        StartCoroutine(RotateRoutine());
    }

    IEnumerator RotateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            // Y축 회전값을 180으로 변경 (180도 돌리기)
            transform.rotation = Quaternion.Euler(0, 180, 0);

            yield return new WaitForSeconds(3f);

            transform.rotation = Quaternion.Euler(0, 0, 0);

            // 만약 다시 0으로 돌아오게 하고 싶다면?  
        }

    }
}
