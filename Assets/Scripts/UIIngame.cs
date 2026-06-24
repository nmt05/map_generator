using UnityEngine;
using UnityEngine.UI;
public class UIIngame : MonoBehaviour
{
    [SerializeField] public Text scoreText;
    [SerializeField] public Text levelText;
    private int score = 0;

    void Awake()
    {
        CreateLevelTextIfNeeded();
        UpdateScoreText();

        LevelManager levelManager = FindObjectOfType<LevelManager>();
        if (levelManager != null)
        {
            UpdateLevelText(levelManager.currentLevel);
        }
    }

    void OnEnable()
    {
        Monster2Controller.DieEvent += UpdateScoreOnce;
        LevelManager.LevelChanged += UpdateLevelText;
    }

    void OnDisable()
    {
        Monster2Controller.DieEvent -= UpdateScoreOnce;
        LevelManager.LevelChanged -= UpdateLevelText;
    }

    private void CreateLevelTextIfNeeded()
    {
        if (levelText != null || scoreText == null) return;

        levelText = Instantiate(scoreText, scoreText.transform.parent);
        levelText.name = "Level";
        levelText.text = "Level: 1";

        RectTransform rect = levelText.GetComponent<RectTransform>();
        RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
        if (rect != null && scoreRect != null)
        {
            rect.anchorMin = scoreRect.anchorMin;
            rect.anchorMax = scoreRect.anchorMax;
            rect.pivot = scoreRect.pivot;
            rect.sizeDelta = scoreRect.sizeDelta;
            rect.anchoredPosition = scoreRect.anchoredPosition + new Vector2(0, -35f);
        }
    }

    private void UpdateScoreOnce()
    {
        Debug.Log("Score+");
        score++;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText == null) return;
        scoreText.text = "Score: " + score.ToString();
    }

    private void UpdateLevelText(int level)
    {
        score = 0;
        UpdateScoreText();

        if (levelText == null) return;
        levelText.text = "Level: " + level.ToString();
    }
}
