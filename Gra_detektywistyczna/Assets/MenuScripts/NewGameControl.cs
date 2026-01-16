using DTOModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;


namespace Assets.MenuScripts
{
    public class NewGameControl : MonoBehaviour
    {
        public GameObject dialogueHistoryPanel; // przypnij w Inspectorze
        
        private async void Awake()
        {
            Debug.Log("New Game Loaded");
            if (!CurrentSaveManager.Instance.IsNewGame())
            {
                Debug.Log("Gra jest kontynuowana");
                ShowPanel();
                CurrentSaveManager.Instance.SetIsNewGame(true);
                return;
            }
            Debug.Log("Gra jest nowa");

        }

        public AudioClip sound;

        void Start()
        {
            
        }

        public void ShowPanel()
        {
            dialogueHistoryPanel.SetActive(true);  // włącz widoczność
        }

        public void HidePanel()
        {
            dialogueHistoryPanel.SetActive(false); // wyłącz widoczność
        }
    }
}
