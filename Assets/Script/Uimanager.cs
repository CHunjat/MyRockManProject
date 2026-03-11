using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 어디서든 접근 가능하게 싱글톤 설정
    public static UIManager instance;

    [Header("HP 바 설정")]
    public Image hpBarFill; // Inspector

    [Header("일시정지용 HP 바 설정")]
    public Image pauseHpBarFill; // 일시정지 화면에서 HP 바로 사용할 이미지 

    [Header("일시정지 설정")]
    public GameObject pauseUI;     // 하이어라키의 PauseUI
    private bool isPaused = false; // 현재 일시정지 상태인지 체크


    [Header("상태창 설정")]
    public TMP_Text lifeText;
    public TMP_Text eCanText;




    void Start()
    {
        UpdateLifeUI(PlayerHealth.LifeCount);
    }
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
            PlayerHealth ph = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();

            if (ph != null) UpdateLifeUI(PlayerHealth.LifeCount);
            if (ph != null) UpdateHP(ph.currentHealth, ph.maxHealth); // 일시정지 시 HP 바로 업데이트


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
        if (maxHP <= 0) return;

        float fillAmount = (float)currentHP / maxHP;

        // [중요] 각 이미지 슬롯이 비어있는지(None) 체크해서 에러를 원천 차단합니다.
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = fillAmount;
        }

        if (pauseHpBarFill != null)
        {
            pauseHpBarFill.fillAmount = fillAmount;
        }


    }

    public void UpdateLifeUI(int currentLife)
    {
        if (lifeText != null)
        {
            // 록맨 감성 x3,x4 이런식 ㅋㅋ
            lifeText.text = "x" + currentLife.ToString();
        }
    }

    public void UseEcan()
    {
       // Debug.Log("1. UseEcan 함수 진입 성공!"); // 콘솔에 이게 뜨는지 확인!
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return; 

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null) return; 


        if (PlayerHealth.ECanCount > 0 && ph.currentHealth < ph.maxHealth)
        {
            PlayerHealth.ECanCount--;

            // 미리 값을 복사해둠 (117번 라인 Null 방어)
            int mHealth = ph.maxHealth;

            ph.Heal(mHealth); // 회복 시작
            // E캔 개수 UI는 즉시 갱신
            if (eCanText != null)
            {
                UpdateECanUI(PlayerHealth.ECanCount);
            }
        }
        else
        {
        }
    }
    public void UpdateECanUI(int amount)
    {
        if (eCanText != null)
        {
            eCanText.text = "x" + amount.ToString();
        }
    }

    /// <summary>
    /// 록맨 스타일 체력 회복 연출 (한 칸씩 띠띠띠- 차오름)
    /// </summary>
    
}