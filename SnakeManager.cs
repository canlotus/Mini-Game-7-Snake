using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SnakeManager : MonoBehaviour
{
    public static SnakeManager Instance; 

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject difficultyPanel;
    public Slider difficultySlider;
    public TextMeshProUGUI difficultyText;

    private int score = 0;
    private int foodCount = 0;
    private int multiplier = 1;
    private int bestScore = 0;
    private bool isPaused = false;
    private int difficulty = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        bestScore = PlayerPrefs.GetInt("SnakeBestScore", 0);
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);

        if (!PlayerPrefs.HasKey("SnakeDifficulty"))
        {
            PlayerPrefs.SetInt("SnakeDifficulty", 1);
        }

        difficulty = PlayerPrefs.GetInt("SnakeDifficulty");
        difficultySlider.value = difficulty;
        UpdateDifficultyText();

        difficultySlider.onValueChanged.AddListener(delegate { UpdateDifficultyText(); });

        difficultyPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void UpdateDifficultyText()
    {
        switch ((int)difficultySlider.value)
        {
            case 0:
                difficultyText.text = "Easy";
                break;
            case 1:
                difficultyText.text = "Medium";
                break;
            case 2:
                difficultyText.text = "Hard";
                break;
        }
    }

    public void ApplyDifficulty()
    {
        difficulty = (int)difficultySlider.value;
        PlayerPrefs.SetInt("SnakeDifficulty", difficulty);
        difficultyPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public int GetDifficulty()
    {
        return PlayerPrefs.GetInt("SnakeDifficulty", 1);
    }

    public void AddScore()
    {
        foodCount++;

        int oldMultiplier = multiplier;

        if (foodCount >= 20)
            multiplier = 3;
        else if (foodCount >= 10)
            multiplier = 2;
        else
            multiplier = 1;

        score += 3 * multiplier;
        UpdateUI();

        if (multiplier != oldMultiplier)
        {
            StartCoroutine(MultiplierAnimation());
        }
    }

    private void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        multiplierText.text = multiplier + "x";
    }

    private IEnumerator MultiplierAnimation()
    {
        Vector3 originalScale = multiplierText.transform.localScale;
        multiplierText.transform.localScale = originalScale * 1.5f;

        yield return new WaitForSeconds(0.5f);

        multiplierText.transform.localScale = originalScale;
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);

        finalScoreText.text = "Score: " + score;

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("SnakeBestScore", bestScore);
        }

        highScoreText.text = "High Score: " + bestScore;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        score = 0;
        foodCount = 0;
        multiplier = 1;
        UpdateUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetGame()
    {
        score = 0;
        foodCount = 0;
        multiplier = 1;
        UpdateUI();
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void PauseGame()
    {
        if (!isPaused)
        {
            Time.timeScale = 0f;
            pausePanel.SetActive(true);
            isPaused = true;
        }
    }

    public void ContinueGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        isPaused = false;
    }
}