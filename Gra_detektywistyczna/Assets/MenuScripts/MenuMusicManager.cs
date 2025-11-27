using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class MenuMusicManager : MonoBehaviour
{
    public static MenuMusicManager Instance;

    [SerializeField]
    private List<string> allowedScenes = new List<string>
    {
        "MainMenu",
        "Authors",
        "ContinueGame"
    };

    private AudioSource audioSrc;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSrc = GetComponent<AudioSource>();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool allowed = false;

        foreach (var s in allowedScenes)
        {
            if (scene.name == s)
            {
                allowed = true;
                break;
            }
        }

        if (allowed)
        {
            if (!audioSrc.isPlaying)
                audioSrc.Play();
        }
        else
        {
            if (audioSrc.isPlaying)
                audioSrc.Stop();
        }
    }
}
