using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // 신형 입력 시스템 사용을 위해 필수!

public class StageSelectManager : MonoBehaviour
{
    [Header("설정")]
    public string targetSceneName = "Stage_01"; // 이동할 씬 이름
    private bool isSelected = false;

    void Update()
    {
        // 1. 키보드 상태 가져오기
        var keyboard = Keyboard.current;
        if (keyboard == null) return; // 키보드 연결 안됨 방지

        // 2. Enter 키 또는 Z 키가 이번 프레임에 눌렸는지 확인
        if (!isSelected && (keyboard.enterKey.wasPressedThisFrame || keyboard.zKey.wasPressedThisFrame))
        {
            isSelected = true;
            Debug.Log("스테이지 선택됨! 이동합니다.");
            SceneManager.LoadScene(targetSceneName);
        }
    }
}