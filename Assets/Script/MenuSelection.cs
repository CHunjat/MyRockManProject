using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSelection : MonoBehaviour
{

    public enum MenuOption { GameOver, Pause }

    [Header("메뉴 설정")]
    public MenuOption menuType; // 메뉴 종류 설정 (인스펙터에서 선택)



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

     
        if (menuType == MenuOption.GameOver)
        {
            // [조건 2] 게임오버 씬에서는 엔터로 결정 (스테이지 이동 등)
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                pauseORgameover();  
            }
        }
        else if (menuType == MenuOption.Pause)
        {
            // [조건 3] 일시정지 메뉴에서는 C키로 E캔 사용
            // (엔터는 UIManager에서 메뉴를 닫는 용도로 사용함)
            if (Input.GetKeyDown(KeyCode.C))
            {
                pauseORgameover();
            }
        }
    }

    void UpdateCursorPosition()
    {
        // 화살표 위치 변경 (효과음 넣으면 더 좋습니다!)
        cursor.anchoredPosition = cursorPositions[selectedIndex];
    }

    void pauseORgameover()
    {
        if (menuType == MenuOption.GameOver)
        {
            ConfirmSelection();
        }
        else if (menuType == MenuOption.Pause)
        {
            PauseMenu();
        }
    }

    void ConfirmSelection()
    {
        switch (selectedIndex)
        {
            case 0:
                PlayerHealth.LifeCount = 3;
                SceneManager.LoadScene("Stage_01"); // 저장된 위치부터 시작하는 로직 필요
                break;
            case 1:
                PlayerHealth.LifeCount = 3;
                SceneManager.LoadScene("StageSelect"); // 스테이지 셀렉트 씬으로 이동
                break;
        }
    }

    void PauseMenu()
    {
        switch(selectedIndex)
        {
            case 0: // E-Can 아이콘 위치
                if (UIManager.instance != null)
                {
                    UIManager.instance.UseEcan(); // 아까 만든 E캔 사용 함수 호출! 스테이지처럼 lifecount 초기화x 어차피 먹기전에 0개이기 때문
                }  
                break;
        }
    }
}