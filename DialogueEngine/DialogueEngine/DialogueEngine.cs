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

            NPCRequestDTO nPCRequestDTO = JsonConvert.DeserializeObject<NPCRequestDTO>(parameters[0]);
            
            return await _aICommunication.GenerateNPCResponseAsync(nPCRequestDTO);
        }

        public async Task<string> GenerateNewScene(string[] parameters)
        {
            if (parameters.Length == 0) return string.Empty;

            SceneDTO sceneDTO = JsonConvert.DeserializeObject<SceneDTO>(parameters[0]);

            return await _aICommunication.GenerateNewSceneAsync(sceneDTO);
        }

        public string GetScene(string[] parameters)
        {
            int sceneNumber = Convert.ToInt32(parameters[1]);

            string scenes = File.ReadAllText(_databasePath + "/" + parameters[0] + ".json");
            if (scenes == null || scenes == "") return null;

            ScenesScriptDTO scenesDTO = JsonConvert.DeserializeObject<ScenesScriptDTO>(scenes);

            if (scenesDTO == null) return null;
            if (sceneNumber > scenesDTO.Scenes.Length || sceneNumber <= 0) return null;

            var settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            return JsonConvert.SerializeObject(scenesDTO.Scenes[sceneNumber - 1], settings);
        }

        public string SaveGame(string[] parameters)
        {
            try
            {
                if (parameters.Length == 0) return "Error: No parameters";

                CreatedGameDTO newGame = JsonConvert.DeserializeObject<CreatedGameDTO>(parameters[0]);

                string gamesJsonPath = _databasePath + "/SavedGames.json";
                List<CreatedGameDTO> gamesList = new List<CreatedGameDTO>();

                if (File.Exists(gamesJsonPath))
                {
                    string gamesJson = File.ReadAllText(gamesJsonPath);
                    if (!string.IsNullOrEmpty(gamesJson))
                    {
                        var existingGames = JsonConvert.DeserializeObject<GamesToContinueDTO>(gamesJson);
                        if (existingGames != null && existingGames.GamesToContinue != null)
                        {
                            gamesList = existingGames.GamesToContinue.ToList();
                        }
                    }
                }

                gamesList.Add(newGame);

                GamesToContinueDTO allGames = new GamesToContinueDTO
                {
                    GamesToContinue = gamesList.ToArray()
                };

                string updatedJson = JsonConvert.SerializeObject(allGames, Formatting.Indented);
                File.WriteAllText(gamesJsonPath, updatedJson);

                return "Game saved successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
