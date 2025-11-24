using AIClient;
using DTOModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogueEngine
{
    public class DialogueEngine
    {
        private readonly string _databasePath = AppDomain.CurrentDomain.BaseDirectory + "../../../../../Gra_detektywistyczna/Assets/Database";
        private readonly AICommunication _aICommunication = new AICommunication();

        public string GetGamesToContinue(string[] parameters)
        {  
            string games = File.ReadAllText(_databasePath + "/SavedGames.json");

            if (games == null || games == "")
                return JsonConvert.SerializeObject(new GamesToContinueDTO());

            GamesToContinueDTO gameDTOs = JsonConvert.DeserializeObject<GamesToContinueDTO>(games);

            var settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            return JsonConvert.SerializeObject(gameDTOs, settings);
        }

        public string GetSettings(string[] parameters)
        {
            string options = File.ReadAllText(_databasePath + "/Settings.json");
            return options;
        }

        public string SaveSettings(string[] parameters)
        {
            if (parameters.Length == 0) return string.Empty;

            File.WriteAllText(_databasePath + "/Settings.json", parameters[0]);

            return string.Empty;
        }

        public async Task<string> AskNPC(string[] parameters) 
        {
            if (parameters.Length == 0) return string.Empty;

            NPCResponseDTO nPCResponseDTO = JsonConvert.DeserializeObject<NPCResponseDTO>(parameters[0]);

            return await _aICommunication.GenerateNPCResponseAsync(nPCResponseDTO);
        }

        public async Task<string> GenerateNewScene(string[] parameters)
        {
            if (parameters.Length == 0) return string.Empty;

            SceneDTO sceneDTO = JsonConvert.DeserializeObject<SceneDTO>(parameters[0]);

            return await _aICommunication.GenerateNewSceneAsync(sceneDTO);
        }
    }
}
