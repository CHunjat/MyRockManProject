using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections; // 코루틴 사용을 위해 필요!

public class StageSelectManager : MonoBehaviour
{
    [Header("설정")]
    public string targetSceneName = "Stage_01";
    public Animator fadeAnimator; // 페이드 애니메이터 연결
    public float delayTime = 1.0f; // 암전 유지 시간

    private bool isSelected = false;

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (!isSelected && (keyboard.enterKey.wasPressedThisFrame || keyboard.zKey.wasPressedThisFrame))
        {
            StartCoroutine(TransitionToScene());
        }
    }

    IEnumerator TransitionToScene()
    {
        isSelected = true;

        // 1. 애니메이터의 Trigger 실행 (미리 설정한 FadeOut 애니메이션)
        if (fadeAnimator != null)
        {
            fadeAnimator.SetTrigger("StartFadeOut");
        }

        Debug.Log("암전 시작...");

        // 2. 지정된 시간만큼 대기
        yield return new WaitForSeconds(delayTime);

        // 3. 씬 이동
        Debug.Log("스테이지 이동!");
        SceneManager.LoadScene(targetSceneName);
    }
}