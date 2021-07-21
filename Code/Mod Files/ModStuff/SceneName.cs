using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModStuff
{
    public static class SceneName
    {
        const string DICTIONARY_PATH = "scenes_data.json";
        static readonly Dictionary<string, string> _sceneNames = BuildDictionary();
        
        static Dictionary<string, string> BuildDictionary()
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            string jsonString = ModMaster.RandomizerDataPath + DICTIONARY_PATH;
            if (System.IO.File.Exists(jsonString))
            {
                try
                {
                    string allText = System.IO.File.ReadAllText(jsonString);
                    JSON_SceneName list = JsonUtility.FromJson<JSON_SceneName>(allText);
                    foreach (var item in list.list)
                    {
                        output.Add(item.originalName, item.nickName);
                    }
                }
                catch (Exception ex)
                {
                    output.Clear();
                }
            }
            return output;
        }

        public static string GetName(string name)
        {
            if (_sceneNames.TryGetValue(name, out string nickName))
            {
                return nickName;
            }
            return name;
        }

        [Serializable]
        class JSON_SceneName
        {
            public JSONSceneEntry[] list;

            [Serializable]
            public class JSONSceneEntry
            {
                public string originalName;
                public string nickName;
            }
        }
    }
}
