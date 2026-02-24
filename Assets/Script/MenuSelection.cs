using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSelection : MonoBehaviour
{
    [Header("커서 설정")]
    public RectTransform cursor; // 화살표 이미지의 RectTransform
    public Vector2[] cursorPositions; // 선택지별 화살표 위치 (0: Continue, 1: Stage Select)

    private int selectedIndex = 0; // 현재 선택된 인덱스

    void Update()
    {
        // 1. 키보드 위/아래 입력으로 인덱스 조절
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = cursorPositions.Length - 1;
            UpdateCursorPosition();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex++;
            if (selectedIndex >= cursorPositions.Length) selectedIndex = 0;
            UpdateCursorPosition();
        }

        // 2. 엔터키로 씬 전환
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ConfirmSelection();
        }
    }

    void UpdateCursorPosition()
    {
        // 화살표 위치 변경 (효과음 넣으면 더 좋습니다!)
        cursor.anchoredPosition = cursorPositions[selectedIndex];
    }

    void ConfirmSelection()
    {
        switch (selectedIndex)
        {
            case 0:
                SceneManager.LoadScene("Stage_01"); // 저장된 위치부터 시작하는 로직 필요
                break;
            case 1:
                SceneManager.LoadScene("StageSelect"); // 스테이지 셀렉트 씬으로 이동
                break;
        }
    }
}