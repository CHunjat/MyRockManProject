using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageSelectManager : MonoBehaviour
{
    [Header("암전 효과 설정")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;
    public string nextSceneName = "InGame"; // 다음에 이동할 씬 이름

    private bool isTransitioning = false;

    void Start()
    {
        // 시작할 때 암전 레이어 초기화 (꺼두기)
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 엔터를 누르면 암전 후 다음 씬으로
        if (!isTransitioning && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            StartCoroutine(FadeAndNextScene());
        }
    }

    IEnumerator FadeAndNextScene()
    {
        isTransitioning = true;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                fadeCanvasGroup.alpha = timer / fadeDuration;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(nextSceneName);
    }
}