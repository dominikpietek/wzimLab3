using DTOModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogueEngine
{
    public class DialogueEngine
    {
        private string _databasePath = AppDomain.CurrentDomain.BaseDirectory + "../../../../../Gra_detektywistyczna/Assets/Database";

        public string GetGamesToContinue()
        {
            string games = File.ReadAllText(_databasePath + "/SavedGames.json");
            GamesToContinueDTO createdGameDTOs = JsonConvert.DeserializeObject<GamesToContinueDTO>(games);
            return JsonConvert.SerializeObject(createdGameDTOs);
        }
    }
}
