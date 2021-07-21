using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.ItemRandomizer
{
    //This static class is responsible of giving the player items in game and when starting a file. If an object is not referenced here, it will not be
    //given to the player.
    //Part of the functionality of "ItemToRandomize" is here. I fragmented the code too much and it is quite obtuse, but it is functional and 
    //adding more items doesnt require much work, so I'm not refactoring unless things get dire
    public static class ModItemHandler
    {
        const string KEYS_PATH = "keys_data.json";
        const string CARD_IDENTIFIER = "Card";
        static ItemMethods _nothing;

        class ItemMethods
        {
            public delegate void OnInGame(Item item, ItemRandomizerCore core, object arg);
            public OnInGame onInGame;
            public delegate void OnFileCreation(RealDataSaver saver, ItemRandomizerCore core, object arg);
            public OnFileCreation onFileCreation;

            public object arg;
            public string giveText = "";
            public string giveImage = "";
            public bool useCustomMessage = false;

            public void InGame(Item item, ItemRandomizerCore core)
            {
                onInGame?.Invoke(item, core, arg);
                if (!useCustomMessage) core.PrintInMessageBox(giveImage, giveText);
            }

            public void FileCreation(RealDataSaver saver, ItemRandomizerCore core)
            {
                onFileCreation?.Invoke(saver, core, arg);
            }
        }

        static Dictionary<string, string> _keyTags;
        static Dictionary<string, int> _keyAmounts;
        static readonly Dictionary<string, JSON_DungeonKeyList.JSON_DungeonKey> _keysDictionary = BuildKeysArray();
        static readonly Dictionary<string, ItemMethods> _itemsDictionary = BuildDictionary();
        static readonly string[] _cardsList = new string[]
        {
            "", //Card 0, left empty to match index to card numbers
            "Fishbun",
            "StupidBee",
            "JennySafety",
            "Shellbun",
            "Spikebun",
            "FeralGate",
            "CandySnake",
            "HermitLegs",
            "Ogler",
            "Hyperdusa",
            "EvilEasel",
            "Warnip",
            "Octacle",
            "Rotnip",
            "BeeSwarm",
            "Volcano",
            "JennyShark",
            "SwimmyRoger",
            "Bunboy",
            "Spectre",
            "Brutus",
            "Jelly",
            "Skullnip",
            "JennySlayer",
            "Titan",
            "ChillyRoger",
            "JennyFlower",
            "Hexrot",
            "JennyMole",
            "JennyBun",
            "JennyCat",
            "JennyMermaid",
            "JennyBerry",
            "Mapman",
            "Cyberjenny",
            "LeBiadlo",
            "Lenny",
            "Passel",
            "Tippsie",
            "IttleDew",
            "NappingFly"
        };

        #region Give and Check methods
        //To give the player an item in game, three variables are checked
        //"name" is used to select which item is going to be spawned, this is split into 4 categories, keytags, dungeon keys, cards and others
        //"item" is the dummy item being picked up. This should be used mostly for the global position and the parent
        //"core" is the randomization core, to see if any situation or tag is influencing the item selection
        public static void GiveInGameItem(string name, Item item, ItemRandomizerCore core)
        {
            if(string.IsNullOrEmpty(name))
            {
                _nothing.InGame(item, core);
            }
            else if(_itemsDictionary.TryGetValue(name, out ItemMethods methods))
            {
                methods.InGame(item, core);
            }
            else if(IsKeyTag(name))
            {
                InGame_GiveDungeonKey(name, core);
            }
            else if (IsCard(name, out int index))
            {
                InGame_GiveCard(name, core, index);
            }
            else if(IsDungeonKey(name))
            {
                InGame_GiveKey(name, core);
            }
            else
            {
                _nothing.InGame(item, core);
            }
        }

        //To give the player an item in file start, three variables are checked
        //"name" is used to select which item is going to be spawned, this is split into 4 categories, keytags, dungeon keys, cards and others
        //"saver" is the data saver for the file. It is used to write entries before the game starts
        //"core" is the randomization core, to see if any situation or tag is influencing the item selection
        public static void GiveFileCreationItem(string name, RealDataSaver saver, ItemRandomizerCore core)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            if (_itemsDictionary.TryGetValue(name, out ItemMethods methods))
            {
                methods.FileCreation(saver, core);
            }
            else if (IsKeyTag(name))
            {
                FCreation_GiveDungeonKey(name, saver);
            }
            else if (IsCard(name, out int index))
            {
                FCreation_GiveCard(name, saver, index);
            }
            else if (IsDungeonKey(name))
            {
                FCreation_GiveKey(name, saver);
            }
        }

        public static bool IsDungeonKey(string key)
        {
            return TryGetKey(key, out JSON_DungeonKeyList.JSON_DungeonKey specificKey);
        }

        public static bool IsKeyTag(string tag)
        {
            return _keyTags.TryGetValue(tag, out string keyScene);
        }

        public static bool IsCard(string name, out int index)
        {
            if(name.Contains(CARD_IDENTIFIER))
            {
                int pos = name.IndexOf(CARD_IDENTIFIER);
                if (pos >= 0)
                {
                    string afterTxt = name.Remove(0, CARD_IDENTIFIER.Length);
                    if (int.TryParse(afterTxt, out int numb) && numb > 0 && numb <= _cardsList.Length )
                    {
                        index = numb;
                        return true;
                    }
                }
            }
            index = 0;
            return false;
        }

        public static bool CheckValid(string name)
        {
            return _itemsDictionary.TryGetValue(name, out ItemMethods methods) || TryGetKeyTag(name, out string scene)  || TryGetKey(name, out JSON_DungeonKeyList.JSON_DungeonKey dungeonKey) || IsCard(name, out int index);
        }

        public static int GetKeyAmountInScene(string scene)
        {
            _keyAmounts.TryGetValue(scene, out int amount);
            return amount;
        }

        public static IDataSaver GetDkeysSaver()
        {
            return ModSaver.GetSaver("mod/itemRandomizer/levels");
        }

        public static string GetNickName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "(NOTHING ASSIGNED)";
            if(TryGetKeyTag(name, out string scene))
            {
                return SceneName.GetName(scene) + " dungeon key";
            }
            else if(TryGetKey(name, out JSON_DungeonKeyList.JSON_DungeonKey dungeonKey))
            {
                return (string.IsNullOrEmpty(dungeonKey.nickName)) ? name : dungeonKey.nickName;
            }
            else if (_itemsDictionary.TryGetValue(name, out ItemMethods methods) || IsCard(name, out int index))
            {
                return name;
            }
            else
            {
                return name + "(INVALID)";
            }
        }
        #endregion

        #region Print methods
        public static string GetKeysData()
        {
            List<string> data = new List<string>();
            foreach (KeyValuePair<string, JSON_DungeonKeyList.JSON_DungeonKey> pair in _keysDictionary)
            {
                data.Add(string.Format("Name: {0}.\nScene: {1}.\nRoom: {2}.\nSave Id: {3}", pair.Value.name, pair.Value.scene, pair.Value.room, pair.Value.saveId));
            }
            return string.Join(" ", data.ToArray());
        }

        public static string GetItemsList()
        {
            List<string> output = new List<string>(_itemsDictionary.Keys);
            return string.Join("\n", output.ToArray());
        }
        #endregion

        #region Dictionary builders
        static Dictionary<string, JSON_DungeonKeyList.JSON_DungeonKey> BuildKeysArray()
        {
            _keyTags = new Dictionary<string, string>();
            _keyAmounts = new Dictionary<string, int>();
            string jsonString = ModMaster.RandomizerDataPath + KEYS_PATH;
            Dictionary<string, JSON_DungeonKeyList.JSON_DungeonKey> output = new Dictionary<string, JSON_DungeonKeyList.JSON_DungeonKey>();
            if (System.IO.File.Exists(jsonString))
            {
                try
                {
                    string allText = System.IO.File.ReadAllText(jsonString);
                    JSON_DungeonKeyList list = JsonUtility.FromJson<JSON_DungeonKeyList>(allText);
                    foreach(var item in list.list)
                    {
                        output.Add(item.name, item);
                        if(!string.IsNullOrEmpty(item.scene))
                        {
                            if (!_keyAmounts.ContainsKey(item.scene)) _keyAmounts.Add(item.scene, 0);
                            _keyAmounts[item.scene]++;
                        }
                        if (!string.IsNullOrEmpty(item.keyTag) && !_keyTags.TryGetValue(item.keyTag, out string keyScene)) _keyTags.Add(item.keyTag, item.scene);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return output;
        }

        static Dictionary<string, ItemMethods> BuildDictionary()
        {
            ItemMethods methods;
            Dictionary<string, ItemMethods> output = new Dictionary<string, ItemMethods>();

            //Nothing
            methods = new ItemMethods();
            methods.onInGame += InGame_Nothing;
            methods.useCustomMessage = true;
            output.Add("Nothing", methods);
            _nothing = methods;

            //Stick
            methods = new ItemMethods();
            methods.onInGame += InGame_Stick;
            methods.useCustomMessage = true;
            output.Add("Stick", methods);

            //StickLvl
            methods = new ItemMethods();
            methods.onInGame += InGame_StickLvl;
            methods.useCustomMessage = true;
            output.Add("StickLvl", methods);

            //Roll
            methods = new ItemMethods();
            methods.onInGame += InGame_Roll;
            methods.useCustomMessage = true;
            output.Add("Roll", methods);

            //Amulet
            methods = new ItemMethods();
            methods.onInGame += InGame_Amulet;
            methods.onFileCreation += FCreation_Amulet;
            methods.useCustomMessage = true;
            output.Add("Amulet", methods);

            //DefenseLvl
            methods = new ItemMethods();
            methods.onInGame += InGame_DefenseLvl;
            methods.useCustomMessage = true;
            output.Add("DefenseLvl ", methods);

            //HeadBand
            methods = new ItemMethods();
            methods.giveText = "You got a Blue Headband!\nYou now deal more damage with your melee attacks.";
            methods.giveImage = "Items/ItemIcon_BlueHeadBand";
            methods.onInGame += InGame_HeadBand;
            methods.onFileCreation += FCreation_HeadBand;
            methods.useCustomMessage = true;
            output.Add("Headband", methods);

            //Chain
            methods = new ItemMethods();
            methods.onInGame += InGame_Chain;
            methods.onFileCreation += FCreation_Chain;
            methods.useCustomMessage = true;
            output.Add("Chain", methods);

            //Dynamite
            methods = new ItemMethods();
            methods.giveText = "You got the Dynamite!\nBlows stuff up. Can be frozen with ice attacks.";
            methods.giveImage = "Items/ItemIcon_Dynamite1";
            methods.onInGame += InGame_Dynamite;
            methods.onFileCreation += FCreation_Dynamite;
            output.Add("Dynamite", methods);

            //Dynamite dev
            methods = new ItemMethods();
            methods.giveText = "You got the DEV Dynamite!\nPuzzles tremble in your presence.";
            methods.giveImage = "Items/ItemIcon_Dynamite1";
            methods.onInGame += InGame_DevDynamite;
            methods.onFileCreation += FCreation_DevDynamite;
            output.Add("DynamiteDev", methods);

            //EFCS
            methods = new ItemMethods();
            methods.giveText = "You got the EFCS! Oh, boy.\nCharge the Fire Mace to use it. Destroys most puzzles and doors.";
            methods.giveImage = "Items/ItemIcon_EFCS";
            methods.onInGame += InGame_EFCS;
            methods.onFileCreation += FCreation_EFCS;
            output.Add("EFCS", methods);
            
            //FakeEFCS
            methods = new ItemMethods();
            methods.giveText = "3 gates that were impossible to open have been destroyed!";
            methods.giveImage = "Items/ItemIcon_EFCS";
            methods.onInGame += InGame_FakeEFCS;
            methods.onFileCreation += FCreation_FakeEFCS;
            output.Add("FakeEFCS", methods);

            //Secret key
            methods = new ItemMethods();
            methods.giveText = "You got a Forbidden Key!\nIt's some kind of weird key, I guess?";
            methods.giveImage = "Items/ItemIcon_ForbiddenKey";
            methods.onInGame += InGame_SecretKey;
            methods.onFileCreation += FCreation_SecretKey;
            output.Add("SecretKey", methods);

            //Force wand
            methods = new ItemMethods();
            methods.giveText = "You got the Force Wand!\nShoots magical projectiles that push and damage things.";
            methods.giveImage = "Items/ItemIcon_Forcewand1";
            methods.onInGame += InGame_ForceWand;
            methods.onFileCreation += FCreation_ForceWand;
            output.Add("ForceWand", methods);

            //Gallery
            methods = new ItemMethods();
            methods.giveText = "You got an Art gallery! Check it out on the main menu.";
            methods.giveImage = "Items/ItemIcon_Gallery";
            methods.onInGame += InGame_Gallery;
            output.Add("Gallery", methods);

            //Ice Ring
            methods = new ItemMethods();
            methods.giveText = "You got the Ice Ring!\nChills enemies and creates reflective magical blocks.";
            methods.giveImage = "Items/ItemIcon_Icering1";
            methods.onInGame += InGame_IceRing;
            methods.onFileCreation += FCreation_IceRing;
            output.Add("IceRing", methods);

            //Loot
            methods = new ItemMethods();
            methods.giveText = "You got the Big Old Pile o' Loot!\nThere's enough in here to buy a country!";
            methods.giveImage = "Items/ItemIcon_Loot";
            methods.onInGame += InGame_Loot;
            methods.onFileCreation += FCreation_Loot;
            output.Add("BigOldPileOfLoot", methods);

            //Lockpick
            methods = new ItemMethods();
            methods.giveText = "You got a Lock pick!\nOpens any locked door if you don't have a key.";
            methods.giveImage = "Items/ItemIcon_MasterKey";
            methods.onInGame += InGame_Lockpick;
            methods.onFileCreation += FCreation_Lockpick;
            output.Add("Lockpick", methods);

            //Fire sword
            methods = new ItemMethods();
            methods.giveText = "You got the Fire Sword!\nDeals more damage and is permanently on fire.";
            methods.giveImage = "Items/ItemIcon_Melee1";
            methods.onInGame += InGame_FireSword;
            methods.onFileCreation += FCreation_FireSword;
            output.Add("FireSword", methods);

            //Fire Mace
            methods = new ItemMethods();
            methods.giveText = "You got the Fire Mace!\nSmacks things and shoots one fireball per second.";
            methods.giveImage = "Items/ItemIcon_Melee2";
            methods.onInGame += InGame_FireMace;
            methods.onFileCreation += FCreation_FireMace;
            output.Add("FireMace", methods);

            //Crayons
            methods = new ItemMethods();
            methods.giveText = "You got a Box of Crayons!\nYour life has increased by a quarter heart.";
            methods.giveImage = "Items/ItemIcon_Fullpaper";
            methods.onInGame += InGame_Crayons;
            methods.onFileCreation += FCreation_Crayons;
            output.Add("Crayons", methods);

            //Nega-Crayons
            methods = new ItemMethods();
            methods.giveText = "You got a Box of Nega-Crayons!\nYour life has decreased by a quarter heart... wait what?";
            methods.giveImage = "Items/ItemIcon_Fullpaper";
            methods.onInGame += InGame_NegaCrayons;
            methods.onFileCreation += FCreation_NegaCrayons;
            output.Add("NegaCrayons", methods);

            //Raft piece
            methods = new ItemMethods();
            methods.onInGame += InGame_RaftPiece;
            methods.onFileCreation += FCreation_RaftPiece;
            methods.useCustomMessage = true;
            output.Add("RaftPiece", methods);

            //Bees
            methods = new ItemMethods();
            //methods.onInGame += InGame_GiveBees;
            methods.useCustomMessage = true;
            output.Add("ENEMY_BeeSwarm", methods);

            //Secret gallery
            methods = new ItemMethods();
            methods.giveText = "You got a Secret art gallery! Check it out on the main menu.";
            methods.giveImage = "Items/ItemIcon_SecretGallery";
            methods.onInGame += InGame_SecretGallery;
            output.Add("SecretGallery", methods);

            //Shards
            methods = new ItemMethods();
            methods.useCustomMessage = true;
            methods.onInGame += InGame_Shard;
            methods.onFileCreation += FCreation_Shard;
            output.Add("Shard", methods);

            //2Shards
            methods = new ItemMethods();
            methods.useCustomMessage = true;
            methods.arg = 2;
            methods.onInGame += InGame_MultipleShard;
            methods.onFileCreation += FCreation_MultipleShard;
            output.Add("2Shards", methods);

            //4Shards
            methods = new ItemMethods();
            methods.useCustomMessage = true;
            methods.arg = 4;
            methods.onInGame += InGame_MultipleShard;
            methods.onFileCreation += FCreation_MultipleShard;
            output.Add("4Shards", methods);

            //8Shards
            methods = new ItemMethods();
            methods.useCustomMessage = true;
            methods.arg = 8;
            methods.onInGame += InGame_MultipleShard;
            methods.onFileCreation += FCreation_MultipleShard;
            output.Add("8Shards", methods);
            
            //16Shards
            methods = new ItemMethods();
            methods.useCustomMessage = true;
            methods.arg = 16;
            methods.onInGame += InGame_MultipleShard;
            methods.onFileCreation += FCreation_MultipleShard;
            output.Add("16Shards", methods);

            //24Shards
            methods = new ItemMethods();
            methods.useCustomMessage = true;
            methods.arg = 24;
            methods.onInGame += InGame_MultipleShard;
            methods.onFileCreation += FCreation_MultipleShard;
            output.Add("24Shards", methods);

            //Sound test
            methods = new ItemMethods();
            methods.giveText = "You got a Sound test menu! Check it out on the main menu.";
            methods.giveImage = "Items/ItemIcon_SoundTest";
            methods.onInGame += InGame_SoundTest;
            output.Add("SoundTest", methods);

            //Suit: armor
            methods = new ItemMethods();
            methods.giveText = "You got an outfit!\nFind a changing tent to put it on.";
            methods.giveImage = "Items/ItemIcon_SuitArmor";
            methods.onInGame += InGame_SuitArmor;
            methods.onFileCreation += FCreation_SuitArmor;
            output.Add("SuitArmor", methods);

            //Suit: card
            methods = new ItemMethods();
            methods.giveText = "You got an outfit!\nFind a changing tent to put it on.";
            methods.giveImage = "Items/ItemIcon_SuitCard";
            methods.onInGame += InGame_SuitCard;
            methods.onFileCreation += FCreation_SuitCard;
            output.Add("SuitCard", methods);

            //Suit: Delinquent
            methods = new ItemMethods();
            methods.giveText = "You got an outfit!\nFind a changing tent to put it on.";
            methods.giveImage = "Items/ItemIcon_SuitDelinquent";
            methods.onInGame += InGame_SuitDelinquent;
            methods.onFileCreation += FCreation_SuitDelinquent;
            output.Add("SuitDelinquent", methods);

            //Suit: IttleOriginal
            methods = new ItemMethods();
            methods.giveText = "You got an outfit!\nFind a changing tent to put it on.";
            methods.giveImage = "Items/ItemIcon_SuitIttleOriginal";
            methods.onInGame += InGame_SuitIttleOriginal;
            methods.onFileCreation += FCreation_SuitIttleOriginal;
            output.Add("SuitIttleOriginal", methods);

            //Suit: Jenny
            methods = new ItemMethods();
            methods.giveText = "You got an outfit!\nFind a changing tent to put it on.";
            methods.giveImage = "Items/ItemIcon_SuitJenny";
            methods.onInGame += InGame_SuitJenny;
            methods.onFileCreation += FCreation_SuitJenny;
            output.Add("SuitJenny", methods);

            //Suit: Swim
            methods = new ItemMethods();
            methods.giveText = "You got an outfit!\nFind a changing tent to put it on.";
            methods.giveImage = "Items/ItemIcon_SuitSwim";
            methods.onInGame += InGame_SuitSwim;
            methods.onFileCreation += FCreation_SuitSwim;
            output.Add("SuitSwim", methods);

            //Suit: Tippsie
            methods = new ItemMethods();
            methods.giveText = "You got an outfit!\nFind a changing tent to put it on.";
            methods.giveImage = "Items/ItemIcon_SuitTippsie";
            methods.onInGame += InGame_SuitTippsie;
            methods.onFileCreation += FCreation_SuitTippsie;
            output.Add("SuitTippsie", methods);

            //Suit: Frog
            methods = new ItemMethods();
            methods.onInGame += InGame_SuitFrog;
            methods.onFileCreation += FCreation_SuitFrog;
            methods.useCustomMessage = true;
            output.Add("SuitFrog", methods);

            //Suit: That guy
            methods = new ItemMethods();
            methods.onInGame += InGame_SuitThatGuy;
            methods.onFileCreation += FCreation_SuitThatGuy;
            methods.useCustomMessage = true;
            output.Add("SuitThatGuy", methods);

            //Suit: Jenny Berry
            methods = new ItemMethods();
            methods.onInGame += InGame_Nothing;
            methods.onFileCreation += FCreation_SuitBerry;
            methods.useCustomMessage = true;
            output.Add("SuitBerry", methods);

            //Tome
            methods = new ItemMethods();
            methods.onInGame += InGame_Tome;
            methods.useCustomMessage = true;
            methods.onFileCreation += FCreation_Tome;
            output.Add("Tome", methods);

            //Tracker
            methods = new ItemMethods();
            methods.onInGame += InGame_Tracker;
            methods.useCustomMessage = true;
            methods.onFileCreation += FCreation_Tracker;
            output.Add("Tracker", methods);

            //YellowHeart
            methods = new ItemMethods();
            methods.giveText = "You got a yellow heart! You heal 5 of your hearts!";
            methods.giveImage = "Items/ItemIcon_ChestHeart";
            methods.onInGame += InGame_Heal20;
            output.Add("YellowHeart", methods);

            return output;
        }
        #endregion

        #region Ingame items
        static void AllowStick()
        {
            ModSaver.SaveBoolToFile("mod", "nostick", false);
            GameObject.Find("PlayerController").GetComponent<PlayerController>().AllowStick = true;
        }

        static bool TryGetKey(string name, out JSON_DungeonKeyList.JSON_DungeonKey specificKey)
        {
            specificKey = null;
            if (_keysDictionary == null) return false;
            if (_keysDictionary.TryGetValue(name, out specificKey))
            {
                return true;
            }
            return false;
        }
        
        static bool TryGetKeyTag(string name, out string scene)
        {
            scene = null;
            if (_keyTags.TryGetValue(name, out scene))
            {
                return true;
            }
            return false;
        }

        static void InGame_GiveDungeonKey(string name, ItemRandomizerCore core)
        {
            if (TryGetKeyTag(name, out string scene))
            {
                if (string.IsNullOrEmpty(scene)) return;

                string tagPath = "mod/itemRandomizer/levels/";
                int oldAmount = ModSaver.LoadIntFromFile(tagPath, scene + "_localKeys");
                ModSaver.SaveIntToFile(tagPath, scene + "_localKeys", oldAmount + 1);
                if (SceneManager.GetActiveScene().name == scene)
                {
                    string itemPath = "localKeys";
                    int currentValue = ModSaver.LoadFromEnt(itemPath);
                    ModSaver.SaveToEnt(itemPath, currentValue + 1);
                    core.PrintInMessageBox("items/ItemIcon_Key", "You found a key! It looks like it's for\nthis dungeon!");
                }
                else
                {
                    string path = "levels/" + scene + "/player/vars/";
                    int current = ModSaver.LoadIntFromFile(path, "localKeys");
                    ModSaver.SaveIntToFile(path, "localKeys", current + 1);
                    core.PrintInMessageBox("items/ItemIcon_Key", $"You found a key! It looks like it's for\n{SceneName.GetName(scene)}.");
                }
            }
        }

        static void InGame_GiveKey(string name, ItemRandomizerCore core)
        {
            if(TryGetKey(name, out JSON_DungeonKeyList.JSON_DungeonKey dungeonKey))
            {
                if (string.IsNullOrEmpty(dungeonKey.scene)) return;
                ModSaver.SaveBoolToFile("mod/itemRandomizer/levels/" + dungeonKey.scene + "/", name, true);
                string nickName = string.IsNullOrEmpty(dungeonKey.nickName) ? dungeonKey.name : dungeonKey.nickName;
                string outputText = string.Format("The door {0} in {1} has been opened!", nickName, SceneName.GetName(dungeonKey.scene));
                core.PrintInMessageBoxFromDisk("locked_door", outputText);
                InGameHelper_GiveKey(dungeonKey.scene, dungeonKey.room, dungeonKey.saveId);
            }
        }

        static void InGameHelper_GiveKey(string scene, string keyRoom, string saveId)
        {
            ModSaver.SaveBoolToFile("levels/" + scene + "/" + keyRoom + "/", saveId, true);
            if (SceneManager.GetActiveScene().name == scene)
            {
                GameObject levelRoot = GameObject.Find("LevelRoot");
                if (levelRoot != null)
                {
                    Transform room = levelRoot.transform.Find(keyRoom);
                    if (room != null)
                    {
                        foreach (RoomAction action in room.GetComponentsInChildren<RoomAction>(true))
                        {
                            if (action._saveName == saveId)
                            {
                                action.Fire(true);
                            }
                        }
                    }
                }
            }
        }

        static void InGame_FakeEFCS(Item item, ItemRandomizerCore core, object arg)
        {
            string[] scene = new string[] { "TombOfSimulacrum", "TombOfSimulacrum", "Deep17" };
            string[] room = new string[] { "AC", "S", "B" };
            string[] saveId = new string[] { "PuzzleGate-48--54", "PuzzleDoor_green-64--25", "PuzzleGate-23--5" };

            for (int i = 0; i < 3; i++) InGameHelper_GiveKey(scene[i], room[i], saveId[i]);
        }

        static void InGame_GiveCard(string name, ItemRandomizerCore core, int index)
        {
            ModSaver.SaveIntToFile("cards", _cardsList[index], 1);
            int total = ModSaver.LoadIntFromFile("mod", "cardcount");
            IDataSaver saver = ModSaver.GetSaver("cards");
            string[] cardsInSave = saver.GetAllDataKeys();
            string textMessage;
            if(cardsInSave.Length == total)
            {
                textMessage = string.Format("You got the last card! ALL {0} cards are yours!\n", cardsInSave.Length);
            }
            else
            {
                textMessage = string.Format("You got a card! You have {0} out of {1}.\n", cardsInSave.Length, total);
            }
            core.PrintInMessageBox("items/ItemIcon_Card", textMessage);
        }

        readonly static string[] _nothingString = new string[]
        {
            "You got nothing?\nHow?\n",
            "The item disappeared into thin air!",
            "You found a pair of shoes... you throw them away",
            "This item is a fake! Better not pick it up at all",
            "At the last second you changed your mind, you don't want this item",
            "The item turned into dust!",
            "You found a health potion! Tippsie chugs it...",
            "The item exploded, unsurprisingly\n\n"
        };

        static void InGame_Nothing(Item item, ItemRandomizerCore core, object arg)
        {
            int randomIndex = UnityEngine.Random.Range(0, _nothingString.Length);
            core.PrintInMessageBoxFromDisk("wut", _nothingString[randomIndex]);
        }

        static void InGame_Stick(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "melee";
            Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            bool isTemp = player.TryGetModifiedVar(itemPath, out int currentValue);
            if (!isTemp) currentValue = ModSaver.LoadFromEnt(itemPath);
            core.PrintInMessageBoxFromDisk("stick", "You found your stick! Time to hit stuff!\n");
            if (currentValue == 100)
            {
                if (isTemp) player.UpdateModifiedVar(itemPath, 0);
                else ModSaver.SaveToEnt(itemPath, 0);
            }
            AllowStick();
        }

        static void InGame_Roll(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveBoolToFile("mod", "noroll", false);
            GameObject.Find("PlayerController").GetComponent<PlayerController>().AllowRoll = true;
            core.PrintInMessageBoxFromDisk("roll", "You remembered how to roll!\n");
        }

        static void InGame_StickLvl(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "melee";
            Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            bool isTemp = player.TryGetModifiedVar(itemPath, out int currentValue);
            if(!isTemp) currentValue = ModSaver.LoadFromEnt(itemPath);
            if (currentValue == 100)
            {
                AllowStick();
                core.PrintInMessageBoxFromDisk("stick", "You found your stick! Time to hit stuff!\n");
                if (isTemp) player.UpdateModifiedVar(itemPath, 0);
                else ModSaver.SaveToEnt(itemPath, 0);
            }
            else
            {
                //If the next upgrade is the EFCS
                if(currentValue == 2)
                {
                    if (core.NoEFCSUpgrade)
                    {
                        //Bypasses save, give default item-get message
                        currentValue = 3; 
                    }
                    else if (core.ReplaceEFCSUpgrade)
                    {
                        //Give fake EFCS printing the proper message
                        GiveInGameItem("FakeEFCS", item, core);
                        return;
                    }
                }
                switch (currentValue)
                {
                    case 0:
                        core.PrintInMessageBox("Items/ItemIcon_Melee1", "You got the Fire Sword!\nDeals more damage and is permanently on fire.");
                        break;
                    case 1:
                        core.PrintInMessageBox("Items/ItemIcon_Melee2", "You got the Fire Mace!\nSmacks things and shoots one fireball per second.");
                        break;
                    case 2:
                        core.PrintInMessageBox("Items/ItemIcon_EFCS", "You got the EFCS! Oh, boy.\nCharge the Fire Mace to use it. Destroys most puzzles and doors.");
                        break;
                    default:
                        core.PrintInMessageBoxFromDisk("stick", "You got a melee upgrade!\n");
                        break;
                }
                if (currentValue < 3)
                {
                    if (isTemp) player.UpdateModifiedVar(itemPath, currentValue + 1);
                    else ModSaver.SaveToEnt(itemPath, currentValue + 1);
                }
            }
        }

        static void InGame_Amulet(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "amulet";
            int currentValue = ModSaver.LoadFromEnt(itemPath);
            if (currentValue < 3) ModSaver.SaveToEnt(itemPath, currentValue + 1);
            switch (currentValue)
            {
                case 0:
                    core.PrintInMessageBox("Items/ItemIcon_Amulet", "You got a Protective Amulet!\nYour defense against projectiles is improved.");
                    break;
                case 1:
                    core.PrintInMessageBox("Items/ItemIcon_Amulet", "You got a Protective Amulet Lv 2!\nYour defense against melee attacks is improved now, too.");
                    break;
                case 2:
                    core.PrintInMessageBox("Items/ItemIcon_Amulet", "You got a Protective Amulet Lv 3!\nYour roll invulnerability time is now extended.");
                    break;
                default:
                    core.PrintInMessageBox("Items/ItemIcon_Amulet", "You got a Protective Amulet upgrade!\n");
                    break;
            }
        }

        static void InGame_DefenseLvl(Item item, ItemRandomizerCore core, object arg)
        {
            if(ModSaver.LoadBoolFromFile("mod", "noroll"))
            {
                GiveInGameItem("Roll", item, core);
            }
            else
            {
                GiveInGameItem("Amulet", item, core);
            }
        }

        static void InGame_HeadBand(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "headband";
            int currentValue = ModSaver.LoadFromEnt(itemPath);
            if (currentValue < 3) ModSaver.SaveToEnt(itemPath, currentValue + 1);
            switch (currentValue)
            {
                case 0:
                    core.PrintInMessageBox("Items/ItemIcon_BlueHeadBand", "You got a Blue Headband!\nYou now deal more damage with your melee attacks.");
                    break;
                case 1:
                    core.PrintInMessageBox("Items/ItemIcon_BlueHeadBand", "You got a Blue Headband Lv 2!\nYou now deal more damage with projectile attacks too.");
                    break;
                case 2:
                    core.PrintInMessageBox("Items/ItemIcon_BlueHeadBand", "You got a Blue Headband Lv 3!\nYou now have a chance of instantly defeating weak enemies.");
                    break;
                default:
                    core.PrintInMessageBox("Items/ItemIcon_BlueHeadBand", "You got a Blue Headband upgrade!\n");
                    break;
            }
        }

        static void InGame_Chain(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "chain";
            Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            bool isTemp = player.TryGetModifiedVar(itemPath, out int currentValue);
            if (!isTemp) currentValue = ModSaver.LoadFromEnt(itemPath);
            
            if (currentValue < 3)
            {
                if (isTemp) player.UpdateModifiedVar(itemPath, currentValue + 1);
                else ModSaver.SaveToEnt(itemPath, currentValue + 1);
            }
            switch (currentValue)
            {
                case 0:
                    core.PrintInMessageBox("Items/ItemIcon_chain", "You got a Chain!\nExtends the reach of your melee weapon.");
                    break;
                case 1:
                    core.PrintInMessageBox("Items/ItemIcon_chain", "You got a Chain Lv 2!\nFurther extends the reach of your melee weapon.");
                    break;
                case 2:
                    core.PrintInMessageBox("Items/ItemIcon_chain", "You got a Chain Lv 3!\nAdds a magical reverse attack to your melee weapon.");
                    break;
                default:
                    core.PrintInMessageBox("Items/ItemIcon_chain", "You got a Chain upgrade!\n");
                    break;
            }
        }

        static void InGame_Dynamite(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "dynamite";

            Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            bool isTemp = player.TryGetModifiedVar(itemPath, out int currentValue);
            if (!isTemp) currentValue = ModSaver.LoadFromEnt(itemPath);
            int maxLevel = core.NoDevUpgrade ? 3 : 4;

            if (currentValue < maxLevel)
            {
                if (isTemp) player.UpdateModifiedVar(itemPath, currentValue + 1);
                else ModSaver.SaveToEnt(itemPath, currentValue + 1);
            }
        }

        static void InGame_DevDynamite(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "dynamite";

            Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            bool isTemp = player.TryGetModifiedVar(itemPath, out int currentValue);
            if (!isTemp) currentValue = ModSaver.LoadFromEnt(itemPath);

            if (isTemp) player.UpdateModifiedVar(itemPath, 4);
            else ModSaver.SaveToEnt(itemPath, 4);
        }

        static void InGame_EFCS(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "melee";


            Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            bool isTemp = player.TryGetModifiedVar(itemPath, out int currentValue);
            if (!isTemp) currentValue = ModSaver.LoadFromEnt(itemPath);
            if(currentValue == 100) AllowStick();

            if (isTemp) player.UpdateModifiedVar(itemPath, 3);
            else ModSaver.SaveToEnt(itemPath, 3);
        }

        static void InGame_SecretKey(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "evilKeys";
            int currentValue = ModSaver.LoadFromEnt(itemPath);
            if (currentValue < 4) ModSaver.SaveToEnt(itemPath, currentValue + 1);
        }

        static void InGame_ForceWand(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "forcewand";

            Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            bool isTemp = player.TryGetModifiedVar(itemPath, out int currentValue);
            if (!isTemp) currentValue = ModSaver.LoadFromEnt(itemPath);
            int maxLevel = core.NoDevUpgrade ? 3 : 4;

            if (currentValue < maxLevel)
            {
                if (isTemp) player.UpdateModifiedVar(itemPath, currentValue + 1);
                else ModSaver.SaveToEnt(itemPath, currentValue + 1);
            }
        }

        static void InGame_Gallery(Item item, ItemRandomizerCore core, object arg)
        {

        }

        static void InGame_IceRing(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "icering";

            Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            bool isTemp = player.TryGetModifiedVar(itemPath, out int currentValue);
            if (!isTemp) currentValue = ModSaver.LoadFromEnt(itemPath);
            int maxLevel = core.NoDevUpgrade ? 3 : 4;

            if (currentValue < maxLevel)
            {
                if (isTemp)
                {
                    player.UpdateModifiedVar(itemPath, currentValue + 1);
                }
                else
                {
                    ModSaver.SaveToEnt(itemPath, currentValue + 1);
                }
            }
        }

        static void InGame_Loot(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveToEnt("loot", 1);
        }

        static void InGame_Lockpick(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "keys";
            int currentValue = ModSaver.LoadFromEnt(itemPath);
            ModSaver.SaveToEnt(itemPath, currentValue + 1);
        }

        static void InGame_FireSword(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "melee";

            Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            bool isTemp = player.TryGetModifiedVar(itemPath, out int currentValue);
            if (!isTemp) currentValue = ModSaver.LoadFromEnt(itemPath);
            if (currentValue == 100) AllowStick();

            if (currentValue < 1 || currentValue == 100)
            {
                if (isTemp) player.UpdateModifiedVar(itemPath, 1);
                else ModSaver.SaveToEnt(itemPath, 1);
            }
        }

        static void InGame_FireMace(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "melee";

            Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
            bool isTemp = player.TryGetModifiedVar(itemPath, out int currentValue);
            if (!isTemp) currentValue = ModSaver.LoadFromEnt(itemPath);
            if (currentValue == 100) AllowStick();

            if (currentValue < 2 || currentValue == 100)
            {
                if (isTemp) player.UpdateModifiedVar(itemPath, 2);
                else ModSaver.SaveToEnt(itemPath, 2);
            }
        }
        
        static void InGame_GiveBees(Item item, ItemRandomizerCore core, object arg)
        {
            ModSpawner.SpawnProperties properties = new ModSpawner.SpawnProperties();
            properties.fixedPosition = item.transform.position;
            properties.npcName = "BeeSwarm";
            LevelRoom room = LevelRoom.GetRoomForPosition(item.transform.position);
            properties.parent = room.transform;
            ModSpawner.Instance.SpawnNPC(properties);
        }

        static void InGame_Heal20(Item item, ItemRandomizerCore core, object arg)
        {
            if(core.CanSpawnHeart())
            {
                core.GiveYellowHeart(item.transform.position);
            }
            else
            {
                Killable entityComponent = GameObject.Find("PlayerEnt").GetComponent<Entity>().GetEntityComponent<Killable>();
                float newHealth = entityComponent.CurrentHp + 20f;
                if (newHealth > entityComponent.MaxHp) newHealth = entityComponent.MaxHp;
                entityComponent.CurrentHp = newHealth;
                ModSaver.SaveFloatToFile("player", "hp", entityComponent.CurrentHp);
            }
        }

        static void InGame_Crayons(Item item, ItemRandomizerCore core, object arg)
        {
            Killable entityComponent = GameObject.Find("PlayerEnt").GetComponent<Entity>().GetEntityComponent<Killable>();
            entityComponent.MaxHp += 1;
            entityComponent.CurrentHp = entityComponent.MaxHp;
            ModSaver.SaveFloatToFile("player", "hp", entityComponent.CurrentHp);
            ModSaver.SaveFloatToFile("player", "maxHp", entityComponent.MaxHp);
        }

        static void InGame_NegaCrayons(Item item, ItemRandomizerCore core, object arg)
        {
            Killable entityComponent = GameObject.Find("PlayerEnt").GetComponent<Entity>().GetEntityComponent<Killable>();
            float newHP = entityComponent.MaxHp - 1f;
            if (newHP <= 0f) newHP = 1f;
            entityComponent.MaxHp = newHP;
            if(entityComponent.CurrentHp > newHP) entityComponent.CurrentHp = newHP;

            ModSaver.SaveFloatToFile("player", "hp", entityComponent.CurrentHp);
            ModSaver.SaveFloatToFile("player", "maxHp", entityComponent.MaxHp);
        }

        static void InGame_RaftPiece(Item item, ItemRandomizerCore core, object arg)
        {
            
            string itemPath = "raft";
            int currentValue = ModSaver.LoadFromEnt(itemPath);
            if (currentValue < 8) ModSaver.SaveToEnt(itemPath, currentValue + 1);
            switch (currentValue)
            {
                case 0:
                    core.PrintInMessageBox("Items/ItemIcon_RaftPiece", "You got the first Raft Piece!\nCollect eight in total to make a full raft.");
                    break;
                case 1:
                    core.PrintInMessageBox("Items/ItemIcon_RaftPiece", "You got the second Raft Piece!\nCollect eight in total to make a full raft.");
                    break;
                case 2:
                    core.PrintInMessageBox("Items/ItemIcon_RaftPiece", "You got the third Raft Piece!\nCollect eight in total to make a full raft.");
                    break;
                case 3:
                    core.PrintInMessageBox("Items/ItemIcon_RaftPiece", "You got the fourth Raft Piece!\nCollect eight in total to make a full raft.");
                    break;
                case 4:
                    core.PrintInMessageBox("Items/ItemIcon_RaftPiece", "You got the fifth Raft Piece!\nCollect eight in total to make a full raft.");
                    break;
                case 5:
                    core.PrintInMessageBox("Items/ItemIcon_RaftPiece", "You got the sixth Raft Piece!\nCollect eight in total to make a full raft.");
                    break;
                case 6:
                    core.PrintInMessageBox("Items/ItemIcon_RaftPiece", "You got the seventh Raft Piece!\nOnly one more to go!");
                    break;
                case 7:
                    core.PrintInMessageBox("Items/ItemIcon_RaftPiece", "You got the final Raft Piece!\nThe raft is complete at last!");
                    break;
                default:
                    core.PrintInMessageBox("Items/ItemIcon_RaftPiece", "You got another Raft Piece!");
                    break;
            }
        }

        static void InGame_SecretGallery(Item item, ItemRandomizerCore core, object arg)
        {

        }

        static void InGame_Shard(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "shards";
            int currentValue = ModSaver.LoadFromEnt(itemPath);
            if (currentValue < 24)
            {
                ModSaver.SaveToEnt(itemPath, currentValue + 1);
                core.PrintInMessageBox("Items/ItemIcon_SecretShard", "You got a Secret Shard!\nWhat are these for?");
            }
            else
            {
                GiveInGameItem("YellowHeart", item, core);
            }
        }


        static void InGame_MultipleShard(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "shards";
            int currentValue = ModSaver.LoadFromEnt(itemPath);
            int amount = (int)arg;
            if (currentValue < 24)
            {
                currentValue += amount;
                if (currentValue >= 24) currentValue = 24;
                ModSaver.SaveToEnt(itemPath, currentValue);
                switch(amount)
                {
                    case 2:
                        core.PrintInMessageBox("Items/ItemIcon_SecretShard", "You got 2 Secret Shards!\nTwice as mysterious!");
                        break;
                    case 4:
                        core.PrintInMessageBox("Items/ItemIcon_SecretShard", "You got 4 Secret Shards!\nWhat a great find!");
                        break;
                    case 8:
                        core.PrintInMessageBox("Items/ItemIcon_SecretShard", "You got 8 Secret Shards!\nLets put them to good use!");
                        break;
                    case 16:
                        core.PrintInMessageBox("Items/ItemIcon_SecretShard", "You got 16 Secret Shards!?\nThis is a bit absurd...");
                        break;
                    case 24:
                        core.PrintInMessageBox("Items/ItemIcon_SecretShard", "You found a bag choke-full of shards. There are 24 here, maybe more...");
                        break;
                    default:
                        core.PrintInMessageBox("Items/ItemIcon_SecretShard", "You got a Secret Shard!\nWhat are these for?");
                        break;
                }
            }
            else
            {
                GiveInGameItem("YellowHeart", item, core);
            }
        }

        static void InGame_SoundTest(Item item, ItemRandomizerCore core, object arg)
        {

        }

        static void InGame_SuitArmor(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveBoolToFile("world", "outfit5", true);
        }

        static void InGame_SuitCard(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveBoolToFile("world", "outfit6", true);
        }

        static void InGame_SuitDelinquent(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveBoolToFile("world", "outfit7", true);
        }

        static void InGame_SuitIttleOriginal(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveBoolToFile("world", "outfit2", true);
        }

        static void InGame_SuitJenny(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveBoolToFile("world", "outfit3", true);
        }

        static void InGame_SuitSwim(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveBoolToFile("world", "outfit4", true);
        }

        static void InGame_SuitTippsie(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveBoolToFile("world", "outfit1", true);
        }

        static void InGame_SuitFrog(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "outfit";
            ModSaver.SaveToEnt(itemPath, 8);
            core.PrintInMessageBoxFromDisk("apafrog", "Suddenly you are apathetic to the world around you.");
        }

        static void InGame_SuitThatGuy(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveBoolToFile("world", "outfit9", true);
            core.PrintInMessageBoxFromDisk("thatguy", "You got an outfit!\nFind a changing tent to put it on.");
        }

        static void InGame_SuitBerry(Item item, ItemRandomizerCore core, object arg)
        {
            ModSaver.SaveBoolToFile("world", "outfit10", true);
            core.PrintInMessageBoxFromDisk("jennyberry", "You got an outfit!\nFind a changing tent to put it on.");
        }

        static void InGame_Tome(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "tome";
            int currentValue = ModSaver.LoadFromEnt(itemPath);
            if(currentValue < 3) ModSaver.SaveToEnt(itemPath, currentValue + 1);
            switch(currentValue)
            {
                case 0:
                    core.PrintInMessageBox("Items/ItemIcon_Tome", "You got a Tome!\nHas a one in four chance of blocking negative statuses.");
                    break;
                case 1:
                    core.PrintInMessageBox("Items/ItemIcon_Tome", "You got a Tome Lv 2!\nHas a one in two chance of blocking negative statuses.");
                    break;
                case 2:
                    core.PrintInMessageBox("Items/ItemIcon_Tome", "You got a Tome Lv 3!\nHas a three in four chance of blocking negative statuses.");
                    break;
                default:
                    core.PrintInMessageBox("Items/ItemIcon_Tome", "You got a Tome upgrade!");
                    break;
            }
        }

        static void InGame_Tracker(Item item, ItemRandomizerCore core, object arg)
        {
            string itemPath = "tracker";
            int currentValue = ModSaver.LoadFromEnt(itemPath);
            if (currentValue < 3) ModSaver.SaveToEnt(itemPath, currentValue + 1);
            switch (currentValue)
            {
                case 0:
                    core.PrintInMessageBox("Items/ItemIcon_Tracker", "You got a Tracker!\nReveals the location of dungeon bosses on the map.");
                    break;
                case 1:
                    core.PrintInMessageBox("Items/ItemIcon_Tracker", "You got a Tracker Lv 2!\nReveals dungeon bosses and chests on the map.");
                    break;
                case 2:
                    core.PrintInMessageBox("Items/ItemIcon_Tracker", "You got a Tracker Lv 3!\nReveals dungeon bosses and chests, and fills in the dungeon's outline.");
                    break;
                default:
                    core.PrintInMessageBox("Items/ItemIcon_Tracker", "You got a Tracker upgrade!");
                    break;
            }
        }
        #endregion

        #region File creation items

        static void FCreation_GiveDungeonKey(string name, RealDataSaver saver)
        {
            if (TryGetKeyTag(name, out string scene))
            {
                if (string.IsNullOrEmpty(scene)) return;

                string tagPath = "mod/itemRandomizer/levels/" + scene + "_localKeys";
                IDataSaver tagSaverAndNameByPath = SaverOwner.GetSaverAndNameByPath(tagPath, saver, out string keyTag, false);
                int tagValue = tagSaverAndNameByPath.LoadInt(keyTag);
                ModMaster.SetNewGameData(tagPath, (tagValue + 1).ToString(), saver);

                string path = "levels/" + scene + "/player/vars/localKeys";
                IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(path, saver, out string key, false);
                int currentValue = saverAndNameByPath.LoadInt(key);
                ModMaster.SetNewGameData(path, (currentValue + 1).ToString(), saver);
                ModMaster.SetNewGameData("mod/itemRandomizer/levels/" + scene + "_localKeys", "1", saver);
            }
        }
        
        static void FCreation_GiveKey(string name, RealDataSaver saver)
        {
            if (TryGetKey(name, out JSON_DungeonKeyList.JSON_DungeonKey dungeonKey))
            {
                if (string.IsNullOrEmpty(dungeonKey.scene)) return;
                ModMaster.SetNewGameData("levels/" + dungeonKey.scene + "/" + dungeonKey.room + "/" + dungeonKey.saveId, "1", saver);
                ModMaster.SetNewGameData("mod/itemRandomizer/levels/" + dungeonKey.scene + "/" + name, "1", saver);
            }
        }

        static void FCreation_FakeEFCS(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string[] scene = new string[] { "TombOfSimulacrum", "TombOfSimulacrum", "Deep17" };
            string[] room = new string[] { "AC", "S", "B" };
            string[] saveId = new string[] { "PuzzleGate-48--54", "PuzzleDoor_green-64--25", "PuzzleGate-23--5" };

            for (int i = 0; i < 3; i++) ModMaster.SetNewGameData("levels/" + scene[i] + "/" + room[i] + "/" + saveId[i], "1", saver);
        }

        static void FCreation_GiveCard(string name, RealDataSaver saver, int index)
        {
            ModMaster.SetNewGameData("cards/" + _cardsList[index], "1", saver);
        }

        static void FCreation_Amulet(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/amulet";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 3) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_HeadBand(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/headband";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 3) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_Chain(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/chain";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 3) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_Dynamite(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/dynamite";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            int maxLevel = core.NoDevUpgrade ? 3 : 4;
            if (currentValue < maxLevel) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_DevDynamite(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/dynamite";
             ModMaster.SetNewGameData(itemPath, "4", saver);
        }

        static void FCreation_EFCS(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/melee";
            ModMaster.SetNewGameData(itemPath, "3", saver);
        }

        static void FCreation_SecretKey(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/evilKeys";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 4) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_ForceWand(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/forcewand";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            int maxLevel = core.NoDevUpgrade ? 3 : 4;
            if (currentValue < maxLevel) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_Gallery(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {

        }

        static void FCreation_IceRing(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/icering";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            int maxLevel = core.NoDevUpgrade ? 3 : 4;
            if (currentValue < maxLevel) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_Loot(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/loot";
            ModMaster.SetNewGameData(itemPath, "1", saver);
        }

        static void FCreation_Lockpick(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/keys";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_FireSword(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/melee";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 1) ModMaster.SetNewGameData(itemPath, "1", saver);
        }

        static void FCreation_FireMace(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/melee";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 2) ModMaster.SetNewGameData(itemPath, "2", saver);
        }

        static void FCreation_Crayons(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "mod/updateMaxHP";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_NegaCrayons(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "mod/updateMaxHP";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            ModMaster.SetNewGameData(itemPath, (currentValue - 1).ToString(), saver);
        }

        static void FCreation_RaftPiece(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/raft";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 8) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }
        
        static void FCreation_Shard(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/shards";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 24) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_MultipleShard(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/shards";
            int amount = (int)arg;
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 24)
            {
                currentValue += amount;
                if (currentValue >= 24) currentValue = 24;
                ModMaster.SetNewGameData(itemPath, currentValue.ToString(), saver);
            }
        }

        static void FCreation_SuitArmor(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "world/outfit5";
            ModMaster.SetNewGameData(itemPath, "1", saver);
            itemPath = "player/vars/outfit";
            ModMaster.SetNewGameData(itemPath, "5", saver);
        }

        static void FCreation_SuitCard(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "world/outfit6";
            ModMaster.SetNewGameData(itemPath, "1", saver);
            itemPath = "player/vars/outfit";
            ModMaster.SetNewGameData(itemPath, "6", saver);
        }

        static void FCreation_SuitDelinquent(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "world/outfit7";
            ModMaster.SetNewGameData(itemPath, "1", saver);
            itemPath = "player/vars/outfit";
            ModMaster.SetNewGameData(itemPath, "7", saver);
        }

        static void FCreation_SuitIttleOriginal(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "world/outfit2";
            ModMaster.SetNewGameData(itemPath, "1", saver);
            itemPath = "player/vars/outfit";
            ModMaster.SetNewGameData(itemPath, "2", saver);
        }

        static void FCreation_SuitJenny(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "world/outfit3";
            ModMaster.SetNewGameData(itemPath, "1", saver);
            itemPath = "player/vars/outfit";
            ModMaster.SetNewGameData(itemPath, "3", saver);
        }

        static void FCreation_SuitSwim(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "world/outfit4";
            ModMaster.SetNewGameData(itemPath, "1", saver);
            itemPath = "player/vars/outfit";
            ModMaster.SetNewGameData(itemPath, "4", saver);
        }

        static void FCreation_SuitTippsie(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "world/outfit1";
            ModMaster.SetNewGameData(itemPath, "1", saver);
            itemPath = "player/vars/outfit";
            ModMaster.SetNewGameData(itemPath, "1", saver);
        }

        static void FCreation_SuitFrog(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/outfit";
            ModMaster.SetNewGameData(itemPath, "8", saver);
            itemPath = "player/vars/danger";
            ModMaster.SetNewGameData(itemPath, "1", saver);
        }

        static void FCreation_SuitThatGuy(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "world/outfit9";
            ModMaster.SetNewGameData(itemPath, "1", saver);
            itemPath = "player/vars/outfit";
            ModMaster.SetNewGameData(itemPath, "9", saver);
        }

        static void FCreation_SuitBerry(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "world/outfit10";
            ModMaster.SetNewGameData(itemPath, "1", saver);
            itemPath = "player/vars/outfit";
            ModMaster.SetNewGameData(itemPath, "10", saver);
        }

        static void FCreation_Tome(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/tome";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 3) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }

        static void FCreation_Tracker(RealDataSaver saver, ItemRandomizerCore core, object arg)
        {
            string itemPath = "player/vars/tracker";
            IDataSaver saverAndNameByPath = SaverOwner.GetSaverAndNameByPath(itemPath, saver, out string key, false);
            int currentValue = saverAndNameByPath.LoadInt(key);
            if (currentValue < 3) ModMaster.SetNewGameData(itemPath, (currentValue + 1).ToString(), saver);
        }
        #endregion
    }
}
