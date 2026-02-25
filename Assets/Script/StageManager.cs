using UnityEngine;
using TMPro;
using System.Collections;

public class StageManager : MonoBehaviour
{

    public static StageManager Instance; // 몬스터들이 참조할 수 있게 싱글톤 추가 / 제어

    [Header("UI & Objects")]
    public GameObject readyText;
    public GameObject teleportBeam;
    public GameObject player;
    public GameObject healthBar; 

    [Header("Positions")]
    public Transform spawnPoint;
    public Transform landPoint;

    [Header("Game State")]
    public bool isGameActive = false; // 게임이 시작되었는지 여부

    [Header("Speed Settings")] // 유니티 창에서 조절할 변수들
    [SerializeField] private float readyBlinkSpeed = 0.4f; // READY 깜빡임 간격
    [SerializeField] private float beamDropSpeed = 25f;    // 빔 낙하 속도
    [SerializeField] private float appearanceDelay = 0.2f; // 착지 후 록맨이 나타나기 전 아주 잠깐의 대기시간


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(StageStartRoutine());
    }

    IEnumerator StageStartRoutine()
    {
        isGameActive = false; // 게임 시작 전에는 false로 설정
        player.SetActive(false);
        teleportBeam.SetActive(false);
        readyText.SetActive(false);
        if(healthBar != null)
            healthBar.SetActive(false);

        // 1. READY 깜빡이기
        readyText.SetActive(true);
        for (int i = 0; i < 3; i++)
        {
            readyText.GetComponent<TextMeshProUGUI>().enabled = true;
            yield return new WaitForSeconds(readyBlinkSpeed);
            readyText.GetComponent<TextMeshProUGUI>().enabled = false;
            yield return new WaitForSeconds(readyBlinkSpeed);
        }
        readyText.SetActive(false);

        // 2. 빔 낙하 연출
        teleportBeam.transform.position = spawnPoint.position;
        teleportBeam.SetActive(true);

        // 빔이 정확히 landPoint에 도착할 때까지 이동
        while (Vector3.Distance(teleportBeam.transform.position, landPoint.position) > 0.1f)
        {
            teleportBeam.transform.position = Vector3.MoveTowards(
                teleportBeam.transform.position, landPoint.position, beamDropSpeed * Time.deltaTime);
            yield return null;
        }

        // 3. 착지 후 아주 잠깐의 딜레이 (선택 사항 - 더 자연스러운 연출을 위해)
        yield return new WaitForSeconds(appearanceDelay);

        // 4. 바톤 터치 (변신!)
        teleportBeam.SetActive(false);
        player.transform.position = landPoint.position;
        player.SetActive(true);

        //4-1 체력바 활성화
        if(healthBar != null)
            healthBar.SetActive(true);

        isGameActive = true; // 게임이 시작되었음을 알림
        // 5. 조작 활성화
        if (player.TryGetComponent<PlayerController>(out var controller))
        {
            controller.enabled = true;
        }
    }
}