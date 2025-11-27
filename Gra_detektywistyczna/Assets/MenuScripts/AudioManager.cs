using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer mixer;
    public AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ApplySavedVolumes();
    }

    public void SetMusicVolume(float value)
    {
        float clamped = Mathf.Clamp(value, 0.0001f, 1f);
        mixer.SetFloat("MusicVolume", Mathf.Log10(clamped) * 20f);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        float clamped = Mathf.Clamp(value, 0.0001f, 1f);
        mixer.SetFloat("SFXVolume", Mathf.Log10(clamped) * 20f);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public float GetMusicVolume() => PlayerPrefs.GetFloat("MusicVolume", 1f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat("SFXVolume", 1f);

    public void ApplySavedVolumes()
    {
        SetMusicVolume(GetMusicVolume());
        SetSFXVolume(GetSFXVolume());
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, GetSFXVolume());
        }
    }
}
