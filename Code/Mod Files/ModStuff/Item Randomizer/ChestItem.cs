using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.ItemRandomizer
{
    //Stores the data of a chest and its conditions. Later on is used by the game to give the player items
    public class ChestItem
    {
        [Flags]
        public enum Tags
        {
            None = 0,
            DungeonKey = 1 << 0,
            SecretKey = 1 << 1,
            LockPick = 1 << 2,
            Card = 1 << 3,
            Outfit = 1 << 4,
            RaftPiece = 1 << 5,
            ClueScroll = 1 << 6,
            Shard = 1 << 7,
            Crayon = 1 << 8,

            Overworld = 1 << 9,
            Dungeon = 1 << 10,
            Cave = 1 << 11,
            PortalWorld = 1 << 12,
            DreamWorld = 1 << 13,

            MainItem = 1 << 14,
            SecondaryItem = 1 << 15,
            Collectable = 1 << 16,
            ModSpawned = 1 << 17,
            Weapon = 1 << 18,
            Standing = 1 << 19,
            Chest = 1 << 20,
            SecretDungeon = 1 << 21,
            Boss = 1 << 22,
            Hidden = 1 << 23
        }

        static Dictionary<string, Tags> _stringToTags;
        static Dictionary<string, Tags> StringToTags
        {
            get
            {
                if(_stringToTags == null)
                {
                    _stringToTags = new Dictionary<string, Tags>
                    {
                        { "DungeonKey", Tags.DungeonKey },
                        { "SecretKey", Tags.SecretKey},
                        { "LockPick", Tags.LockPick},
                        { "Card", Tags.Card},
                        { "Outfit", Tags.Outfit},
                        { "RaftPiece", Tags.RaftPiece},
                        { "ClueScroll", Tags.ClueScroll},
                        { "Shard", Tags.Shard},
                        { "Crayon", Tags.Crayon},
                        { "Overworld", Tags.Overworld},
                        { "Dungeon", Tags.Dungeon},
                        { "Cave", Tags.Cave},
                        { "PortalWorld", Tags.PortalWorld},
                        { "DreamWorld", Tags.DreamWorld},
                        { "MainItem", Tags.MainItem},
                        { "SecondaryItem", Tags.SecondaryItem},
                        { "Collectible", Tags.Collectable},
                        { "ModSpawned", Tags.ModSpawned},
                        { "Weapon", Tags.Weapon},
                        { "Standing", Tags.Standing},
                        { "Chest", Tags.Chest},
                        { "SecretDungeon", Tags.SecretDungeon},
                        { "Boss", Tags.Boss},
                        { "Hidden", Tags.Hidden}
                    };
                }
                return _stringToTags;
            }
        }

        public string name = "";
        public string scene = "";
        public string room = "";
        public string saveId = "";
        public string originalItem = "";
        public string assignedItem = "";
        public bool isVanilla = false;
        public int chestID = -1;
        public string setFlag = null;
        public Tags tags = Tags.None;
        public Condition[] allConditions;
        public Condition areaCondition;

        public static ChestItem GetChestItem(JSON_ChestItemList.JSON_ChestItem jsonItem, ItemRandomizationState state, out string errors)
        {
            errors = "";
            ChestItem output = new ChestItem();
            output.name = jsonItem.name;
            output.scene = jsonItem.scene;
            output.room = jsonItem.room;
            output.saveId = jsonItem.saveId;
            output.originalItem = jsonItem.originalItem;
            output.setFlag = jsonItem.setFlag;

            //Fill the bitwise tag flags
            for (int i = 0; i < jsonItem.tags.Length; i++)
            {
                if(StringToTags.TryGetValue(jsonItem.tags[i], out Tags tag))
                {
                    output.tags = tag | output.tags;
                }
                else
                {
                    errors += "Warning: tag " + jsonItem.tags[i] + " is an invalid tag";
                }
            }

            //Setup all the conditions
            output.allConditions = new Condition[jsonItem.allConditions.Length];
            for(int i = 0; i < output.allConditions.Length; i++)
            {
                output.allConditions[i] = new Condition(jsonItem.allConditions[i], state);
                errors += output.allConditions[i].GetErrors();
            }

            //Setup area condition
            if(jsonItem.areaCondition != null && jsonItem.areaCondition.condition != null && jsonItem.areaCondition.condition.Length > 0)
                output.areaCondition = new Condition(jsonItem.areaCondition, state);

            return output;
        }
        
        public override string ToString()
        {
            string[] conditionsOutput = new string[allConditions.Length];
            for(int i = 0; i < allConditions.Length; i++)
            {
                conditionsOutput[i] = (i + 1).ToString() + "- " + allConditions[i].ToString();
            }
            string areaConditionString = areaCondition != null ? areaCondition.ToString() : "None";
            string originalItemString = ModItemHandler.GetNickName(originalItem);
            string assigned;
            string chestSetFlag = "";
            if (string.IsNullOrEmpty(assignedItem))
            {
                assigned = "None";
            }
            else
            {
                assigned = ModItemHandler.GetNickName(assignedItem);
            }
            if (!string.IsNullOrEmpty(setFlag))
            {
                chestSetFlag = "\nSetFlag: " + ModItemHandler.GetNickName(assignedItem);
            }
            return string.Format("Name: {0}\nAssigned Item: {7}\nScene: {1}\nRoom: {2}\nSave ID: {3}\nOriginal item: {4}\nChest ID: {10}\nTags: {5}\nConditions:\n{6}\nArea conditions: {8}{9}", name, SceneName.GetName(scene), room, saveId, originalItemString, GetTagsString(tags), string.Join("\n", conditionsOutput), assigned, areaConditionString, chestSetFlag, chestID);
        }
        //Convert.ToString((int)tags, 2).PadLeft(32, '0')
        string GetTagsString(Tags tags)
        {
            List<string> tagsUsed = new List<string>();
            foreach(KeyValuePair<string, Tags> dict in StringToTags)
            {
                if ((tags & dict.Value) != 0) tagsUsed.Add(dict.Key);
            }
            return string.Join(" ", tagsUsed.ToArray());
        }

        public string GetShortInfo()
        {
            string assigned = string.IsNullOrEmpty(assignedItem) ? "None" : assignedItem;
            return string.Format("* {0} *\nItem: {1}", name, assigned);
        }

        public bool CheckAllConditions()
        {
            if (allConditions.Length == 0) return true;

            if (areaCondition != null && !areaCondition.Check()) return false;

            for (int i = 0; i < allConditions.Length; i++)
            {
                if (allConditions[i].Check()) return true;
            }
            return false;
        }

        public class Condition
        {
            ItemRandomizationState.ConditionFunc[] _conditions;
            object[] _arguments;
            ItemRandomizationState _state;
            string _name;
            string _errors = "";

            public Condition(JSON_ChestItemList.JSON_ChestItem.JSON_ChestItemConditions jsonConditions, ItemRandomizationState state)
            {
                _conditions = new ItemRandomizationState.ConditionFunc[jsonConditions.condition.Length];
                _arguments = new object[jsonConditions.condition.Length];
                _state = state;
                string[] names = new string[jsonConditions.condition.Length];

                for (int i = 0; i < _conditions.Length; i++)
                {
                    if(!string.IsNullOrEmpty(jsonConditions.condition[i]))
                    {
                        _conditions[i] = state.GetCondition(jsonConditions.condition[i], out string error, out _arguments[i], out string nickName);
                        if (string.IsNullOrEmpty(error))
                        {
                            names[i] = nickName;
                        }
                        else
                        {
                            names[i] = jsonConditions.condition[i] + "(INVALID)";
                        }
                    }
                }

                _name = string.Join(" && ", names);
            }

            public bool Check()
            {
                for (int i = 0; i < _conditions.Length; i++)
                {
                    if (!_conditions[i].Invoke(_arguments[i]))
                    {
                        _state.ResetKeyTags();
                        return false;
                    }
                }
                _state.ResetKeyTags();
                return true;
            }

            public string GetErrors()
            {
                return _errors;
            }

            public override string ToString()
            {
                return _name;
            }
        }
    }
}
