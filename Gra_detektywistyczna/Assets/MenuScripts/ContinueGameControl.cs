using DTOModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.MenuScripts
{
    public class ContinueGameControl : MonoBehaviour
    {
        private async void Awake()
        {
            GamesToContinueDTO gamesToContinue = await DialogueEngineManager.Instance.GetGamesToContinueAsync();
            foreach (CreatedGameDTO game in gamesToContinue.GamesToContinue)
            {
                Debug.Log($"{game.Title}");
            }
        }
    }
}
