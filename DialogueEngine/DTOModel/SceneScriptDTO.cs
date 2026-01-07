using System;
using System.Collections.Generic;
using System.Text;

namespace DTOModel
{
    [Serializable]
    public class SceneScriptDTO
    {
        public string Name;

        public string Description;

        public string Background;

        public NPCDTO[] Npcs;

        public ItemDTO[] Items;
    }

}