using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleController : MonoBehaviour
{
    [Header("페이드 효과 (텍스트용)")]
    public Graphic targetGraphic;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1.0f;
    public float speed = 1.5f;

    [Header("암전 효과 (화면 전환용)")]
    public Image fadeOverlay;
    public float fadeDuration = 1.0f;

    private bool isTransitioning = false;

    void Start()
    {
        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
            fadeOverlay.gameObject.SetActive(true); // 여기서 강제로 켭니다.
        }
    }

    void Update()
    {
        if (targetGraphic != null && !isTransitioning)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * speed, 1f));
            Color newColor = targetGraphic.color;
            newColor.a = alpha;
            targetGraphic.color = newColor;
        }

        if (!isTransitioning && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
           
            StartCoroutine(FadeAndExit());
        }
    }

    IEnumerator FadeAndExit()
    {
        isTransitioning = true;
        float timer = 0f;

        
        fadeOverlay.gameObject.SetActive(true);

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            Color c = fadeOverlay.color;
            c.a = Mathf.Lerp(0f, 1f, progress);
            fadeOverlay.color = c;

            // 로그로 알파값이 실제로 오르는지 확인 가능
            // Debug.Log("현재 알파값: " + c.a); 

            yield return null;
        }

        // 마지막 고정
        Color finalColor = fadeOverlay.color;
        finalColor.a = 1f;
        fadeOverlay.color = finalColor;

        yield return new WaitForSeconds(0.7f); // 완전히 깜깜해진 걸 볼 시간

        SceneManager.LoadScene("StageSelect");
    }
}