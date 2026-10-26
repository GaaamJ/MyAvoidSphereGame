using TMPro;
using UnityEngine;

// 생존시간 측정, UI 표시, Game Over 상태를 관리한다.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI survivalTimeText;

    public bool IsGameOver { get; private set; }
    public float SurvivalTime { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (IsGameOver) return;

        SurvivalTime += Time.deltaTime;
        UpdateTimeText();
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        UpdateTimeText();
        Debug.Log("Game Over");
    }

    void UpdateTimeText()
    {
        if (survivalTimeText != null)
            survivalTimeText.text = $"Time: {SurvivalTime:F1}s";
    }
}
