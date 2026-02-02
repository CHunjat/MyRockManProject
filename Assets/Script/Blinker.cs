using TMPro;
using UnityEngine;

public class Blinker : MonoBehaviour
{
    public float blinkInterval = 0.1f; // 깜빡이는 속도 (초 단위)
    private TextMeshProUGUI _text;
    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        // 반복 실행: (함수이름, 시작시간, 반복간격)
        InvokeRepeating(nameof(ToggleText), 0, blinkInterval);
    }

    void ToggleText()
    {
        // 텍스트가 켜져있으면 끄고, 꺼져있으면 켬
        _text.enabled = !_text.enabled;
    }
}
