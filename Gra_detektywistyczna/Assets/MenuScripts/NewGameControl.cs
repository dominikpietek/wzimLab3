using DTOModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.MenuScripts
{
    public class NewGameControl : MonoBehaviour
    {
        private async void Awake()
        {
            NPCResponseDTO npcResponseDTO = new NPCResponseDTO()
            {
                NpcName = "John",
                NpcRole = "Admin",
                PlayerText = "Hi!",
                SceneContext = "No context"
            };
            string response = await DialogueEngineManager.Instance.AskNPCAsync(npcResponseDTO);
            Debug.Log(response);


        }

        public AudioClip sound;

        void Start()
        {
            AudioManager.Instance.PlaySFX(sound);
        }
    }
}
