using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// 생존시간 측정, UI 표시, Game Over 상태와 재시작을 관리한다.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI survivalTimeText;
    [SerializeField] private GameObject gameOverPanel;

    public bool IsGameOver { get; private set; }
    public float SurvivalTime { get; private set; }

    void Awake()
    {
        Instance = this;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (IsGameOver)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                Restart();
            return;
        }

        SurvivalTime += Time.deltaTime;
        UpdateTimeText();
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        UpdateTimeText();
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        Debug.Log("Game Over");
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateTimeText()
    {
        if (survivalTimeText != null)
            survivalTimeText.text = $"Time: {SurvivalTime:F1}s";
    }
}
