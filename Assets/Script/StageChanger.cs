using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageChanger : MonoBehaviour
{
    public CanvasGroup fadeGroup;
    public float fadeSpeed = 2.0f; // 숫자가 작을수록 천천히 어두워집니다.

    private bool isTransitioning = false;

    void Update()
    {
        if (isTransitioning) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 바로 SceneManager를 부르지 않고, 코루틴을 통해 '기다림'을 줍니다.
            StartCoroutine(FadeAndExit());
        }
    }

    IEnumerator FadeAndExit()
    {
        isTransitioning = true;

        // 1. 암전 시작 (A값을 0에서 1까지 서서히 올림)
        float alpha = 0;
        while (alpha < 255f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeGroup.alpha = alpha;
            yield return null; // 다음 프레임까지 대기 (이게 있어야 '서서히' 보임)
        }

        // 2. 확실하게 1(255)로 만들어서 암전 완료
        fadeGroup.alpha = 255f;

        // 3. 잠시 검은 화면을 유지 (록맨 특유의 정적)
        yield return new WaitForSeconds(2.1f);

        // 4. 이제서야 씬을 넘깁니다.
        SceneManager.LoadScene("Stage_01");
    }
}