using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelManager : MonoBehaviour
{
    public static SettingsPanelManager Instance;

    [Header("Assign the panel GameObject (already in scene)")]
    public GameObject panel; // przypisz panel z Hierarchy

    private Button closeBtn;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        
        if (panel != null)
        {
            
            closeBtn = panel.GetComponentInChildren<Button>(true);
            if (closeBtn != null)
            {
                
                closeBtn.onClick.RemoveListener(ClosePanel);
                closeBtn.onClick.AddListener(ClosePanel);
            }
        }

        
        if (panel != null)
            panel.SetActive(false);
    }

    public void OpenPanel()
    {
        if (panel == null)
        {
            Debug.LogWarning("SettingsPanelManager: panel nie jest przypisany w inspectorze.");
            return;
        }

        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        if (panel == null) return;
        panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (closeBtn != null)
            closeBtn.onClick.RemoveListener(ClosePanel);
    }
}