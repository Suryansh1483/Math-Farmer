using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance;

    [Header("UI References")]
    public GameObject questionPanel;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI resultText;
    public TMP_InputField answerInput;
    public Button submitButton;

    private System.Action<bool> callback;

    private float correctAnswerFloat;
    private int correctAnswerInt;
    private bool isFloatAnswer;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (questionPanel != null)
            questionPanel.SetActive(false);
    }

    private void Update()
    {
        if (questionPanel != null &&
            questionPanel.activeSelf &&
            Input.GetKeyDown(KeyCode.Return))
        {
            CheckAnswer();
        }
    }

    // --------------------------------------------------
    // Arithmetic Questions
    // --------------------------------------------------
    public void AskArithmeticQuestion(System.Action<bool> resultCallback)
    {
        int playerLevel = GameManager.Instance.level;

        int a, b, correctAns;
        string operation;

        if (playerLevel <= 2)
        {
            a = Random.Range(1, 20);
            b = Random.Range(1, 20);

            operation = "+";
            correctAns = a + b;
        }
        else if (playerLevel <= 4)
        {
            if (Random.Range(0, 2) == 0)
            {
                a = Random.Range(10, 50);
                b = Random.Range(1, 30);

                operation = "+";
                correctAns = a + b;
            }
            else
            {
                a = Random.Range(20, 100);
                b = Random.Range(1, a);

                operation = "-";
                correctAns = a - b;
            }
        }
        else
        {
            int opChoice = Random.Range(0, 4);

            switch (opChoice)
            {
                case 0:
                    a = Random.Range(25, 100);
                    b = Random.Range(25, 100);

                    operation = "+";
                    correctAns = a + b;
                    break;

                case 1:
                    a = Random.Range(50, 200);
                    b = Random.Range(10, a);

                    operation = "-";
                    correctAns = a - b;
                    break;

                case 2:
                    a = Random.Range(2, 12);
                    b = Random.Range(2, 12);

                    operation = "×";
                    correctAns = a * b;
                    break;

                default:
                    b = Random.Range(2, 10);
                    correctAns = Random.Range(2, 15);

                    a = b * correctAns;

                    operation = "÷";
                    break;
            }
        }

        SetupQuestion(
            $"Solve: {a} {operation} {b} = ?",
            correctAns,
            resultCallback
        );
    }

    // --------------------------------------------------
    // Fraction Questions
    // --------------------------------------------------
    public void AskFractionQuestion(System.Action<bool> resultCallback)
    {
        int playerLevel = GameManager.Instance.level;

        string questionStr;
        float correctAns;

        if (playerLevel <= 3)
        {
            int[] nums = { 1, 1, 1, 1, 3 };
            int[] dens = { 2, 4, 5, 10, 4 };

            int i = Random.Range(0, nums.Length);

            int num = nums[i];
            int den = dens[i];

            correctAns = (float)num / den;

            questionStr =
                $"Convert to decimal: {num}/{den}";
        }
        else
        {
            if (Random.Range(0, 2) == 0)
            {
                int num = Random.Range(1, 20);
                int den = Random.Range(2, 20);

                correctAns =
                    Mathf.Round(((float)num / den) * 100f) / 100f;

                questionStr =
                    $"Convert to decimal (2 dp): {num}/{den}";
            }
            else
            {
                int den = Random.Range(2, 8);

                int num1 = Random.Range(1, den);
                int num2 = Random.Range(1, den - num1);

                correctAns =
                    (float)(num1 + num2) / den;

                questionStr =
                    $"Add and convert: {num1}/{den} + {num2}/{den}";
            }
        }

        SetupQuestion(
            questionStr,
            correctAns,
            resultCallback
        );
    }

    // --------------------------------------------------
    // Profit & Loss Questions
    // --------------------------------------------------
    public void AskProfitLossQuestion(System.Action<bool> resultCallback)
    {
        int playerLevel = GameManager.Instance.level;

        string questionStr;
        float correctAns;

        if (playerLevel <= 3)
        {
            int cost = Random.Range(10, 30);
            int profit = Random.Range(5, 15);

            int sell = cost + profit;

            correctAns = profit;

            questionStr =
                $"Bought for ${cost}, sold for ${sell}.\nProfit = ?";
        }
        else
        {
            int cost = Random.Range(20, 80);
            int profit = Random.Range(5, 30);

            int sell = cost + profit;

            correctAns =
                Mathf.RoundToInt(((float)profit / cost) * 100f);

            questionStr =
                $"Bought for ${cost}, sold for ${sell}.\nProfit % = ?";
        }

        SetupQuestion(
            questionStr,
            correctAns,
            resultCallback
        );
    }

    // --------------------------------------------------
    // Setup Helpers
    // --------------------------------------------------
    private void SetupQuestion(
        string q,
        int correctAns,
        System.Action<bool> resultCallback)
    {
        isFloatAnswer = false;
        correctAnswerInt = correctAns;

        SetupQuestionUI(q, resultCallback);
    }

    private void SetupQuestion(
        string q,
        float correctAns,
        System.Action<bool> resultCallback)
    {
        isFloatAnswer = true;
        correctAnswerFloat = correctAns;

        SetupQuestionUI(q, resultCallback);
    }

    private void SetupQuestionUI(
        string q,
        System.Action<bool> resultCallback)
    {
        questionPanel.SetActive(true);

        questionText.text = q;

        if (resultText != null)
            resultText.text = "";

        answerInput.text = "";
        answerInput.ActivateInputField();

        callback = resultCallback;

        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(CheckAnswer);
    }

    // --------------------------------------------------
    // Check Answer
    // --------------------------------------------------
    private void CheckAnswer()
    {
        bool correct = false;

        string userInput = answerInput.text.Trim();

        if (string.IsNullOrEmpty(userInput))
        {
            if (resultText != null)
                resultText.text = "Please enter an answer!";

            return;
        }

        if (isFloatAnswer)
        {
            if (float.TryParse(userInput, out float userAns))
            {
                correct =
                    Mathf.Abs(userAns - correctAnswerFloat) < 0.01f;
            }
            else
            {
                if (resultText != null)
                    resultText.text = "Enter a valid number!";

                return;
            }
        }
        else
        {
            if (int.TryParse(userInput, out int userAns))
            {
                correct = userAns == correctAnswerInt;
            }
            else
            {
                if (resultText != null)
                    resultText.text = "Enter a valid whole number!";

                return;
            }
        }

        // WRONG ANSWER
        if (!correct)
        {
            if (resultText != null)
                resultText.text = "Incorrect! Try Again.";

            if (StatisticsManager.Instance != null)
                StatisticsManager.Instance.RecordWrongAnswer();

            answerInput.text = "";
            answerInput.ActivateInputField();

            return;
        }

        // CORRECT ANSWER
        if (resultText != null)
            resultText.text = "Correct!";

        if (StatisticsManager.Instance != null)
            StatisticsManager.Instance.RecordCorrectAnswer();

        GameManager.Instance.AddXP(5);

        questionPanel.SetActive(false);

        callback?.Invoke(true);
    }
}