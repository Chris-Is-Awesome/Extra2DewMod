using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.ItemRandomizer
{
    [System.Serializable]
    public class JSON_ItemConfig
    {
        public string info;
        public string[] globalTags;
        public string[] initialItems;
        public ItemGroup[] itemGroups;
        public JSON_RandomizationPool[] pools;

        [System.Serializable]
        public class Shuffles
        {
            public int start = 0;
            public int end = 0;
        }

        [System.Serializable]
        public class ItemGroup
        {
            public string groupName;
            public string[] items;
        }

        [System.Serializable]
        public class JSON_RandomizationPool
        {
            public string name;
            public string[] tags;
            public Shuffles[] allShuffles;
            public string[] defaultItems;
            public string[] itemsList;
        }
    }
}