using UnityEngine;
using TMPro;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance;

    [Header("Statistics")]
    public int questionsAnswered;
    public int correctAnswers;
    public int wrongAnswers;

    [Header("Statistics Panel")]
    public GameObject statisticsPanel;

    [Header("Statistics Text")]
    public TextMeshProUGUI questionsAnsweredText;
    public TextMeshProUGUI correctAnswersText;
    public TextMeshProUGUI wrongAnswersText;
    public TextMeshProUGUI accuracyText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (statisticsPanel != null)
            statisticsPanel.SetActive(false);

        LoadStatistics();
        UpdateStatisticsUI();
    }

    // ---------------------------
    // Record Statistics
    // ---------------------------
    public void RecordCorrectAnswer()
    {
        questionsAnswered++;
        correctAnswers++;

        UpdateStatisticsUI();
    }

    public void RecordWrongAnswer()
    {
        questionsAnswered++;
        wrongAnswers++;

        UpdateStatisticsUI();
    }

    // ---------------------------
    // Accuracy
    // ---------------------------
    public float GetAccuracy()
    {
        if (questionsAnswered == 0)
            return 0f;

        return ((float)correctAnswers / questionsAnswered) * 100f;
    }

    // ---------------------------
    // UI
    // ---------------------------
    public void UpdateStatisticsUI()
    {
        if (questionsAnsweredText != null)
            questionsAnsweredText.text =
                $"Questions Answered: {questionsAnswered}";

        if (correctAnswersText != null)
            correctAnswersText.text =
                $"Correct Answers: {correctAnswers}";

        if (wrongAnswersText != null)
            wrongAnswersText.text =
                $"Wrong Answers: {wrongAnswers}";

        if (accuracyText != null)
            accuracyText.text =
                $"Accuracy: {GetAccuracy():F1}%";
    }

    public void OpenStatisticsPanel()
    {
        UpdateStatisticsUI();

        if (statisticsPanel != null)
            statisticsPanel.SetActive(true);
    }

    public void CloseStatisticsPanel()
    {
        if (statisticsPanel != null)
            statisticsPanel.SetActive(false);
    }

    // ---------------------------
    // Reset Statistics
    // ---------------------------
    public void ResetStatistics()
    {
        questionsAnswered = 0;
        correctAnswers = 0;
        wrongAnswers = 0;

        UpdateStatisticsUI();
    }

    // ---------------------------
    // Save Statistics
    // ---------------------------
    public void SaveStatistics()
    {
        PlayerPrefs.SetInt("Stats_QuestionsAnswered", questionsAnswered);
        PlayerPrefs.SetInt("Stats_CorrectAnswers", correctAnswers);
        PlayerPrefs.SetInt("Stats_WrongAnswers", wrongAnswers);

        PlayerPrefs.Save();
    }

    // ---------------------------
    // Load Statistics
    // ---------------------------
    public void LoadStatistics()
    {
        questionsAnswered =
            PlayerPrefs.GetInt("Stats_QuestionsAnswered", 0);

        correctAnswers =
            PlayerPrefs.GetInt("Stats_CorrectAnswers", 0);

        wrongAnswers =
            PlayerPrefs.GetInt("Stats_WrongAnswers", 0);

        UpdateStatisticsUI();
    }

    // ---------------------------
    // Delete Statistics Save
    // ---------------------------
    public void DeleteStatisticsSave()
    {
        PlayerPrefs.DeleteKey("Stats_QuestionsAnswered");
        PlayerPrefs.DeleteKey("Stats_CorrectAnswers");
        PlayerPrefs.DeleteKey("Stats_WrongAnswers");

        PlayerPrefs.Save();
    }
}