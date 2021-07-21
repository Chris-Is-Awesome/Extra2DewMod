using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.ItemRandomizer
{
    [System.Serializable]
    public class JSON_DungeonKeyList
    {
        public JSON_DungeonKey[] list;

        [System.Serializable]
        public class JSON_DungeonKey
        {
            public string nickName = "";
            public string name = "";
            public string scene = "";
            public string room = "";
            public string saveId = "";
            public string keyTag = "";
        }
    }
}
