using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using DTOModel;
using System.Collections.Generic;

public class MenuControl : MonoBehaviour
{
    private async void Awake()
    {
        await DialogueEngineManager.InitializeManagerAsync(gameObject);
    }

    private async void OnApplicationQuit()
    {
        if (DialogueEngineManager.Instance != null)
        {
            await DialogueEngineManager.Instance.QuitManagerAsync();
        }
    }

    public MenuControl()
    {
        Debug.Log("Start programu!");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void NewGameScene()
    {
        SceneManager.LoadScene("NewGame");
    }

    public void ContinueGameScene()
    {
        SceneManager.LoadScene("ContinueGame");
    }

    public void  AuthorsScene()
    {
        SceneManager.LoadScene("Authors");
    }

    public void SettingsScene()
    {
        SceneManager.LoadScene("Settings");
    }

    public void QuickGame()
    {
        Application.Quit();
#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
#endif

    }
    
}
