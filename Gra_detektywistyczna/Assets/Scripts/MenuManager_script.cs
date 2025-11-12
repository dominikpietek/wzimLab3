using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenuManager : MonoBehaviour
{
    public void StartNewGame()
    {
        SceneManager.LoadScene("NewGameScene");
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene("ContinueScene");
    }

    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingsScene");
    }

    public void OpenAuthors()
    {
        SceneManager.LoadScene("AuthorsScene");
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}