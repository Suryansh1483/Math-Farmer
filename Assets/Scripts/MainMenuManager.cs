using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject howToPlayPanel;

    // --------------------------------------------------
    // New Game
    // --------------------------------------------------
    public void NewGame()
    {
        PlayerPrefs.DeleteAll();

        SceneManager.sceneLoaded += HandleNewGameLoaded;
        SceneManager.LoadScene("Farm");
    }

    private void HandleNewGameLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Farm")
            return;

        GameManager.Instance?.NewGame();
        StatisticsManager.Instance?.ResetStatistics();

        SceneManager.sceneLoaded -= HandleNewGameLoaded;
    }

    // --------------------------------------------------
    // Load Game
    // --------------------------------------------------
    public void LoadGame()
    {
        SceneManager.sceneLoaded += HandleLoadGameLoaded;
        SceneManager.LoadScene("Farm");
    }

    private void HandleLoadGameLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Farm")
            return;

        GameManager.Instance?.LoadGame();
        StatisticsManager.Instance?.LoadStatistics();

        SceneManager.sceneLoaded -= HandleLoadGameLoaded;
    }

    // --------------------------------------------------
    // How To Play
    // --------------------------------------------------
    public void OpenHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    // --------------------------------------------------
    // Main Menu
    // --------------------------------------------------
    public void GoToMainMenu()
    {
        GameManager.Instance?.SaveGame();

        SceneManager.LoadScene("MainMenu");
    }

    // --------------------------------------------------
    // Exit Game
    // --------------------------------------------------
    public void ExitGame()
    {
        GameManager.Instance?.SaveGame();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}