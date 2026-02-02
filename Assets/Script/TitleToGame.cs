using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // 1. 최신 입력 시스템 도구 추가

public class TitleToGame : MonoBehaviour
{
    public string nextSceneName = "StageSelect";
    private bool _isStarting = false;

    void Update()
    {
        if (_isStarting) return;

        // 2. 최신 방식: 아무 키나 눌렸는지 감지
        if (Keyboard.current.anyKey.wasPressedThisFrame ||
            Pointer.current?.press.wasPressedThisFrame == true)
        {
            _isStarting = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }
}