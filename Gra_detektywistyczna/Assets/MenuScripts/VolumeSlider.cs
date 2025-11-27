using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType { Music, SFX }
    public VolumeType volumeType;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        
        slider.value = volumeType == VolumeType.Music
            ? AudioManager.Instance.GetMusicVolume()
            : AudioManager.Instance.GetSFXVolume();

        
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        if (volumeType == VolumeType.Music)
            AudioManager.Instance.SetMusicVolume(value);
        else
            AudioManager.Instance.SetSFXVolume(value);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}
