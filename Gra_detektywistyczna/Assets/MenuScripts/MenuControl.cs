using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DTOModel;
using System.Collections.Generic;
using System;
using System.Linq;


public class MenuControl : MonoBehaviour
{
    public static Dictionary<string, string> CollectedCharacters = new Dictionary<string, string>();

    public static MenuControl Instance;
    public static int CurrentSceneNumber { get; private set; }
    public static string CurrentScenarioName { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image backgroundImage;

    private async void Awake()
    {
        await DialogueEngineManager.InitializeManagerAsync(gameObject);
        Instance = this;
        DontDestroyOnLoad(gameObject);
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

   public async void NewGameScene()
{
    string scenarioName = GetRandomScenarioFromFolder();
    
    Debug.Log($"Używam scenariusza: {scenarioName}");
    
    string[] parameters = { scenarioName, "1" };
    CurrentScenarioName = scenarioName;
    CurrentSceneNumber = 1;

    SceneScriptDTO scene = await DialogueEngineManager.Instance.GetSceneAsync(parameters);

    if (scene != null)
    {
        MenuControl.CollectedCharacters.Clear();   
        foreach (var character in scene.Npcs)
        {
            if (!MenuControl.CollectedCharacters.ContainsKey(character.name))
            {
                MenuControl.CollectedCharacters.Add(character.name, character.protrait);
            }
        }

        SceneManager.LoadScene("NewGame");
    }
    else
    {
        Debug.LogError($"Nie można załadować scenariusza: {scenarioName}");
        SceneManager.LoadScene("MainMenu");
    }
}

private string GetRandomScenarioFromFolder()
{
    string databasePath = Application.dataPath + "/Database/";
    
    if (!System.IO.Directory.Exists(databasePath))
    {
        Debug.LogError("Brak folderu Database!");
        return "scenerio_apollo";
    }
    
    var files = System.IO.Directory.GetFiles(databasePath, "*.json")
        .Select(f => System.IO.Path.GetFileNameWithoutExtension(f))
        .Where(f => f != "Settings" && f != "SavedGames")
        .ToList();
    
    if (files.Count == 0)
    {
        return "scenerio_apollo";
    }
    
    System.Random random = new System.Random();
    return files[random.Next(0, files.Count)];
}

    public async Task NextScene()
    {
        string[] parameters = { CurrentScenarioName, (++CurrentSceneNumber).ToString() };
        SceneScriptDTO scene = await DialogueEngineManager.Instance.GetSceneAsync(parameters);


        if (scene != null)
        {

            Debug.Log("Nazwa tla to: " + scene.Background);


            SetBackground(scene.Background);

            Debug.Log("Next scene: " + CurrentSceneNumber);
        }
        else
        {
            Debug.Log("Wykryto koniec scenariusza (null). Zmieniam scenę na podsumowanie.");

            UnityEngine.SceneManagement.SceneManager.LoadScene("EndGameSummary");
        }
    }

    private void SetBackground(string backgroundName)
    {

        Sprite newSprite = Resources.Load<Sprite>(backgroundName);
        if (backgroundImage == null)
        {
            return;
        }

        if (newSprite != null)
        {
            backgroundImage.sprite = newSprite;
        }
        else
        {
            Debug.LogError($"Nie znaleziono grafiki o nazwie: {backgroundName} w Resources");
        }
    }
    public void ContinueGameScene()
    {
        SceneManager.LoadScene("ContinueGame");
    }

    public void AuthorsScene()
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
