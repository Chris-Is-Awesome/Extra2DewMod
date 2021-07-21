using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.ItemRandomizer
{
    [System.Serializable]
    public class JSON_ChestItemList
    {
        public JSON_ChestItem[] list;

        [System.Serializable]
        public class JSON_ChestItem
        {
            public string name = "";
            public string scene = "";
            public string room = "";
            public string saveId = "";
            public string originalItem = "";
            public string setFlag = "";
            public string[] tags;
            public JSON_ChestItemConditions[] allConditions;
            public JSON_ChestItemConditions areaCondition;

            [System.Serializable]
            public class JSON_ChestItemConditions
            {
                public string[] condition;
            }
        }
    }
}
