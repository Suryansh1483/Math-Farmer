using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int coins = 0;
    public int xp = 0;
    public int level = 1;

    [Header("UI References")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI plotStatusText;
    public MessageManager messageManager;

    [Header("Action Buttons")]
    public Button plantButton;
    public Button waterButton;
    public Button harvestButton;
    public Button resetFarmButton;

    [Header("Soil Plots")]
    public CropController activeSoilPlot;

    [Header("Game Settings")]
    public int xpPerLevel = 100;
    public int maxLevel = 20;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateHUD();
        UpdateButtonStates();

        if (messageManager != null)
            messageManager.ShowMessage(
                "Welcome to Math Farmer! Click a soil plot to get started!");

        if (resetFarmButton != null)
        {
            resetFarmButton.onClick.RemoveAllListeners();
            resetFarmButton.onClick.AddListener(ResetFarm);
        }

        // MainMenuManager decides whether to load or start a new game.
    }

    private void Update()
    {
        UpdateButtonStates();
        UpdatePlotStatus();
    }

    // ---------------- Player Progress ----------------

    public void AddCoins(int amount)
    {
        coins += amount;

        UpdateHUD();

        if (amount > 0 && messageManager != null)
            messageManager.ShowMessage($"+{amount} Coins");
    }

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UpdateHUD();
            return true;
        }

        if (messageManager != null)
            messageManager.ShowMessage("Not enough coins!");

        return false;
    }

    public void AddXP(int amount)
    {
        xp += amount;

        while (xp >= xpPerLevel && level < maxLevel)
        {
            xp -= xpPerLevel;
            level++;

            if (messageManager != null)
                messageManager.ShowMessage($"Level Up! You are now Level {level}!");

            int levelBonus = level * 10;
            AddCoins(levelBonus);
        }

        UpdateHUD();
    }

    public void UpdateHUD()
    {
        if (coinsText != null)
            coinsText.text = $"Coins = {coins}";

        if (xpText != null)
            xpText.text = $"XP: {xp}/{xpPerLevel}";

        if (levelText != null)
            levelText.text = $"Level {level}";
    }

    // ---------------- Plot Selection ----------------

    public void SelectSoilPlot(CropController soilPlot)
    {
        activeSoilPlot = soilPlot;

        UpdateButtonStates();
        UpdatePlotStatus();

        if (messageManager != null)
            messageManager.ShowMessage($"Selected {soilPlot.gameObject.name}");
    }

    private void UpdateButtonStates()
    {
        if (activeSoilPlot == null)
        {
            SetButtonState(plantButton, false);
            SetButtonState(waterButton, false);
            SetButtonState(harvestButton, false);
            return;
        }

        SetButtonState(plantButton, activeSoilPlot.CanPlant());
        SetButtonState(waterButton, activeSoilPlot.CanWater());
        SetButtonState(harvestButton, activeSoilPlot.CanHarvest());
    }

    private void SetButtonState(Button button, bool interactable)
    {
        if (button == null) return;

        button.interactable = interactable;
    }

    private void UpdatePlotStatus()
    {
        if (plotStatusText == null)
            return;

        if (activeSoilPlot == null)
        {
            plotStatusText.text = "No plot selected";
        }
        else
        {
            plotStatusText.text =
                $"{activeSoilPlot.gameObject.name}: {activeSoilPlot.GetStateString()}";
        }
    }

    // ---------------- Gameplay Actions ----------------

    public void OnPlantButton()
    {
        if (!ValidatePlotSelection()) return;

        if (!activeSoilPlot.CanPlant())
        {
            messageManager?.ShowMessage("Can't plant here!");
            return;
        }

        QuestionManager.Instance.AskArithmeticQuestion((isCorrect) =>
        {
            if (isCorrect)
            {
                activeSoilPlot.PlantCrop();

                AddXP(10);

                messageManager?.ShowMessage("Seed planted successfully!");
            }
            else
            {
                messageManager?.ShowMessage("Incorrect answer! Try again.");
            }
        });
    }

    public void OnWaterButton()
    {
        if (!ValidatePlotSelection()) return;

        if (!activeSoilPlot.CanWater())
        {
            messageManager?.ShowMessage("Nothing to water!");
            return;
        }

        QuestionManager.Instance.AskFractionQuestion((isCorrect) =>
        {
            if (isCorrect)
            {
                activeSoilPlot.WaterCrop();

                AddXP(15);

                messageManager?.ShowMessage("Crop watered!");
            }
            else
            {
                messageManager?.ShowMessage("Incorrect answer!");
            }
        });
    }

    public void OnHarvestButton()
    {
        if (!ValidatePlotSelection()) return;

        if (!activeSoilPlot.CanHarvest())
        {
            messageManager?.ShowMessage("Crop isn't ready!");
            return;
        }

        QuestionManager.Instance.AskProfitLossQuestion((isCorrect) =>
        {
            if (isCorrect)
            {
                int baseReward = 20;
                int levelBonus = level * 5;

                int totalReward =
                    baseReward +
                    levelBonus +
                    Random.Range(0, 10);

                activeSoilPlot.HarvestCrop();

                AddCoins(totalReward);
                AddXP(25);

                messageManager?.ShowMessage(
                    $"Harvest successful! Earned {totalReward} coins!");
            }
            else
            {
                messageManager?.ShowMessage(
                    "Incorrect answer! The crop withers...");
            }
        });
    }

    private bool ValidatePlotSelection()
    {
        if (activeSoilPlot == null)
        {
            messageManager?.ShowMessage("Select a soil plot first!");
            return false;
        }

        return true;
    }

    // ---------------- Reset Farm ----------------

    public void ResetFarm()
    {
        CropController[] allPlots =
            FindObjectsOfType<CropController>();

        foreach (CropController plot in allPlots)
        {
            if (plot != null)
                plot.LoadEmpty();
        }

        activeSoilPlot = null;

        coins = 0;
        xp = 0;
        level = 1;

        if (StatisticsManager.Instance != null)
            StatisticsManager.Instance.ResetStatistics();

        SaveGame();

        UpdateHUD();
        UpdateButtonStates();
        UpdatePlotStatus();

        messageManager?.ShowMessage("Farm has been reset!");
    }

    // ---------------- Save / Load ----------------

    public void SaveGame()
    {
        CropController[] allPlots =
            FindObjectsOfType<CropController>();

        PlayerPrefs.SetInt("PlotCount", allPlots.Length);

        for (int i = 0; i < allPlots.Length; i++)
        {
            CropController.CropData data =
                allPlots[i].GetSaveData();

            PlayerPrefs.SetInt(
                $"Plot_{i}_State",
                (int)data.state);

            PlayerPrefs.SetFloat(
                $"Plot_{i}_Progress",
                data.progress);
        }

        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("XP", xp);
        PlayerPrefs.SetInt("Level", level);

        if (StatisticsManager.Instance != null)
            StatisticsManager.Instance.SaveStatistics();

        PlayerPrefs.SetInt("HasSave", 1);

        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey("HasSave"))
        {
            messageManager?.ShowMessage("No save file found!");
            return;
        }

        coins = PlayerPrefs.GetInt("Coins", 0);
        xp = PlayerPrefs.GetInt("XP", 0);
        level = PlayerPrefs.GetInt("Level", 1);

        CropController[] allPlots =
            FindObjectsOfType<CropController>();

        int plotCount =
            PlayerPrefs.GetInt("PlotCount", 0);

        for (int i = 0; i < plotCount && i < allPlots.Length; i++)
        {
            CropController.CropData data =
                new CropController.CropData
                {
                    state =
                        (CropController.CropState)
                        PlayerPrefs.GetInt($"Plot_{i}_State", 0),

                    progress =
                        PlayerPrefs.GetFloat($"Plot_{i}_Progress", 0f)
                };

            allPlots[i].LoadSaveData(data);
        }

        if (StatisticsManager.Instance != null)
        {
            StatisticsManager.Instance.LoadStatistics();
        }

        UpdateHUD();
        UpdateButtonStates();
        UpdatePlotStatus();

        messageManager?.ShowMessage("Game Loaded Successfully!");
    }

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();

        coins = 0;
        xp = 0;
        level = 1;

        CropController[] allPlots =
            FindObjectsOfType<CropController>();

        foreach (CropController plot in allPlots)
        {
            plot.LoadEmpty();
        }

        activeSoilPlot = null;

        if (StatisticsManager.Instance != null)
        {
            StatisticsManager.Instance.ResetStatistics();
            StatisticsManager.Instance.DeleteStatisticsSave();
        }

        UpdateHUD();
        UpdateButtonStates();
        UpdatePlotStatus();

        messageManager?.ShowMessage("New Game Started!");
    }

    // ---------------- Scene Management ----------------

    public void GoToMainMenu()
    {
        SaveGame();

        Instance = null;

        SceneManager.LoadScene("MainMenu");
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}