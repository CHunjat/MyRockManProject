using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 어디서든 접근 가능하게 싱글톤 설정
    public static UIManager instance;

    [Header("HP 바 설정")]
    public Image hpBarFill; // Inspector에서 파란색 게이지 이미지를 드래그 앤 드롭 하세요.


    [Header("일시정지 설정")]
    public GameObject pauseUI;     // 하이어라키의 PauseUI를 여기에 드래그하세요!
    private bool isPaused = false; // 현재 일시정지 상태인지 체크



    void Awake()
    {
        // 싱글톤 초기화
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if(Keyboard.current.enterKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if(pauseUI == null) return; // 일시정지 UI가 설정되어 있지 않으면 아무것도 하지 않음
        
        isPaused =!isPaused; // 상태 토글

        if(isPaused)
        {
            pauseUI.SetActive(true); // 일시정지 UI 활성화
            Time.timeScale = 0f; // 게임 정지
        }
        else
        {
            pauseUI.SetActive(false); // 일시정지 UI 비활성화
            Time.timeScale = 1f; // 게임 재개
        }

    }

    /// <summary>
    /// 단순 HP 업데이트 (데미지 입었을 때 즉시 반영)
    /// </summary>
    public void UpdateHP(int currentHP, int maxHP)
    {
        if (hpBarFill == null) return;

        // Image Type이 Filled이고 Vertical로 설정되어 있어야 합니다.
        hpBarFill.fillAmount = (float)currentHP / maxHP;

    }

    /// <summary>
    /// 록맨 스타일 체력 회복 연출 (한 칸씩 띠띠띠- 차오름)
    /// </summary>
    public IEnumerator HealRoutine(int targetHP, int maxHP)
    {
        if (hpBarFill == null) yield break;

        float targetFill = (float)targetHP / maxHP;

        // 현재 게이지가 목표 게이지보다 낮을 때만 작동
        while (hpBarFill.fillAmount < targetFill)
        {
            // 한 칸(1/28)만큼 증가
            hpBarFill.fillAmount += (1f / maxHP);

            // 여기에 '띠릭' 하는 효과음을 넣으면 감성이 폭발합니다.
            // AudioSource.PlayOneShot(healSound); 

            // 아주 짧은 대기 시간 (록맨 특유의 차오르는 속도)
            yield return new WaitForSeconds(0.05f);
        }

        // 마지막 수치 보정
        hpBarFill.fillAmount = targetFill;
    }
}