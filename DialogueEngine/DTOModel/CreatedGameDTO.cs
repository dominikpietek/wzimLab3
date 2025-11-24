using System;
using Newtonsoft.Json;

namespace DTOModel
{
    [Serializable]
    public class CreatedGameDTO
    {
        public string Title;
        public int CurrentSceneNumber;
        public int MaxSceneNumber;
        public string GameHistory;
        public string LastSaveDate;

    }
}
