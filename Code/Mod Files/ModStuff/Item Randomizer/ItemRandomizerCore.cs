using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

namespace ModStuff.ItemRandomizer
{
    //Handles the randomization itself
    public class ItemRandomizerCore
    {
        #region Class constants, variables and constructors 
        const string CHEST_LIST_DATABASE = "chest_data";
        const string CHEST_LIST = CHEST_LIST_DATABASE + ".json";
        const string SEPARATOR = "------------------------------------------";
        const string POOLS_SEPARATOR = "*******\n";
        const string SEPARATOR_WITH_JUMPS = "\n" + SEPARATOR + "\n";
        const string USE_MOD_SPAWNED = "USE_MOD_SPAWNED";
        const string NO_ITEM = "Nothing";
        const string DEFAULT_ITEM = NO_ITEM;

        public delegate bool TagFilterFunc(ChestItem chest);

        ItemRandomizationState _state;

        string _itemDatabaseFile = "";
        public string FileInfo { get; private set; } = "";
        List<ChestItem> _randomizedChests;

        List<RandomizationPool> _allPools;
        ChestItem[] _allChests;
        string[] _initialItemsJSON;

        List<ItemToRandomize> _initialItems;
        Dictionary<string, ItemGroup> _itemGroups;

        string currentSeed = "00000000";
        bool _configLoaded = false;
        string[] _globalTags;
        string _configFile = "";

        static Dictionary<string, TagFilterFunc> _tagFilterDictionary;

        public bool Randomizing { get; set; }
        public string Outcome { get; private set; }
        public string Header { get; private set; }
        public string ConfigInfo { get; private set; }
        public string Error { get; private set; }

        public bool OverrideNoStick { get; set; }
        public bool OverrideNoRoll { get; set; }
        bool _snatchLvlStick = false;
        delegate bool OverrideItemTest(string name);
        OverrideItemTest _overrideItemTest;

        public bool GiveTracker3 { get; set; }
        public bool StickDisallowed { get; private set; }
        public bool RollDisallowed { get; private set; }
        public bool NoDevUpgrade { get; private set; }
        public bool NoEFCSUpgrade { get; private set; }
        public bool ReplaceEFCSUpgrade { get; private set; }
        public bool OpenDreams { get; private set; }
        bool _useModSpawned = false;
        bool _noLogic = false;
        bool _ignoreDefaultError = false;
        bool _ignoreNoChests = false;

        public bool SpawnBees { get; set; }
        public bool SpawnYellowHearts { get; set; }

        EntityHUD _entityHUD;

        //Constructor
        public ItemRandomizerCore()
        {
            _allPools = new List<RandomizationPool>();
            _randomizedChests = new List<ChestItem>();
            _initialItems = new List<ItemToRandomize>();
            _state = new ItemRandomizationState();
            _itemGroups = new Dictionary<string, ItemGroup>();

            if (_tagFilterDictionary == null)
            {
                _tagFilterDictionary = new Dictionary<string, TagFilterFunc>()
                {
                    {"ALL", TagAll },
                    {"DREAM_WORLD", TagDreamWorld },
                    {"CARDS", TagCards },
                    {"RAFT_PIECES", TagRaftPieces },
                    {"CAVES", TagCaves },
                    {"DUNGEONS", TagDungeon },
                    {"DUNGEON_KEYS", TagDungeonKeys },
                    {"SECRET_DUNGEONS", TagSecretDungeon },
                    {"PORTAL_WORLDS", TagPortalWorlds },
                    {"DREAM_WORLDS", TagDreamWorlds },
                    {"BOSS", TagBosses }
                };
            }
        }
        #endregion

        #region Fire randomization methods
        //Regular randomize
        public bool RandomizeWithSeed(string name, string seed)
        {
            LoadChestsData(out string errors);
            LoadConfigFile(name);
            RNGSeed.SetSeed(seed);
            currentSeed = RNGSeed.CurrentSeed;
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed.GetHashCode());
            bool success = RandomizeItems();
            UnityEngine.Random.state = oldState;
            return success;
        }

        //Randomize without loading the config file or chest database
        public bool RandomizeNoLoads(string seed)
        {
            RNGSeed.SetSeed(seed);
            currentSeed = RNGSeed.CurrentSeed;
            UnityEngine.Random.State oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(seed.GetHashCode());
            bool success = RandomizeItems();
            UnityEngine.Random.state = oldState;
            return success;
        }

        //Randomizes 100 times re-rolling seeds, returns the amount of succesful randomizations and their seeds
        public int TestMassRandomize(string name, out string[] goodSeeds)
        {
            List<string> seedsLists = new List<string>();
            LoadChestsData(out string errors);
            LoadConfigFile(name);
            int output = 0;
            for (int i = 0; i < 100; i++)
            {
                RNGSeed.ReRollSeed();
                string newSeed = RNGSeed.CurrentSeed;
                UnityEngine.Random.State oldState = UnityEngine.Random.state;
                UnityEngine.Random.InitState(newSeed.GetHashCode());
                bool success = RandomizeItems();
                UnityEngine.Random.state = oldState;
                if (success)
                {
                    seedsLists.Add(newSeed);
                    output++;
                }
            }
            goodSeeds = seedsLists.ToArray();
            return output;
        }
        #endregion

        #region File loading
        class ItemGroup
        {
            string[] _items;
            List<string> _availableItems;

            public ItemGroup (string[] items)
            {
                if (items == null) _items = new string[] { };
                else _items = items;
                Reset();
            }

            public void Reset()
            {
                _availableItems = _items.ToList<string>();
            }

            public string GetItem(ItemRandomizerCore core)
            {
                if (_availableItems.Count == 0) return NO_ITEM;
                int randomIndex = UnityEngine.Random.Range(0, _availableItems.Count);
                string output = _availableItems[randomIndex];
                _availableItems.RemoveAt(randomIndex);
                output = core.GetItemName(output);
                return output;
            }
        }

        //Load the chest database
        public void LoadChestsData(out string errors)
        {
            errors = "";
            string jsonString = ModMaster.RandomizerDataPath + CHEST_LIST;
            string allText = "";
            if (System.IO.File.Exists(jsonString))
            {
                try
                {
                    allText = System.IO.File.ReadAllText(jsonString);
                    JSON_ChestItemList items = JsonUtility.FromJson<JSON_ChestItemList>(allText);
                    _allChests = new ChestItem[items.list.Length];
                    for(int i = 0; i < items.list.Length; i++)
                    {
                        _allChests[i] = ChestItem.GetChestItem(items.list[i], _state, out string chestErrors);
                        _allChests[i].chestID = i;
                        errors += chestErrors;
                    }
                    _itemDatabaseFile = CHEST_LIST_DATABASE;
                }
                catch (Exception ex)
                {
                    Error = "Randomization failed: Invalid chest database";
                    _allChests = null;
                }
            }
        }

        //Loads a config file. If it fails, "Error" will be different from null. This doesnt reset "Error", that is done by the randomizer method 
        public void LoadConfigFile(string fileName)
        {
            ResetLoadConfig();

            string jsonString = ModMaster.RandomizerConfigFilesPath + fileName + ".json";
            if (System.IO.File.Exists(jsonString))
            {
                try
                {
                    string allText = System.IO.File.ReadAllText(jsonString);
                    JSON_ItemConfig config = JsonUtility.FromJson<JSON_ItemConfig>(allText);
                    _configFile = fileName;

                    //Get info
                    FileInfo = config.info;

                    //Build pools
                    if(config.pools != null)
                    {
                        for (int i = 0; i < config.pools.Length; i++)
                        {
                            _allPools.Add(new RandomizationPool(config.pools[i], this));
                        }
                    }

                    //Tags
                    _globalTags = config.globalTags;

                    //Item groups
                    if (config.itemGroups != null)
                    {
                        for (int i = 0; i < config.itemGroups.Length; i++)
                        {
                            _itemGroups.Add(config.itemGroups[i].groupName, new ItemGroup(config.itemGroups[i].items));
                        }
                    }

                    //Initial items
                    if (config.initialItems != null)
                    {
                        _initialItemsJSON = config.initialItems;
                    }

                    //Success!
                    _configLoaded = true;
                }
                catch (Exception ex)
                {
                    Error = "Randomization failed: Error loading config file";
                    ResetLoadConfig();
                }
            }
        }

        void ResetLoadConfig()
        {
            FileInfo = "";
            _configFile = "";
            _configLoaded = false;
            _initialItemsJSON = new string[] { };
            _allPools.Clear();
            _itemGroups.Clear();
        }
        #endregion

        #region Randomization pool class
        class RandomizationPool
        {
            public enum PoolState { ITEM_PLACED, NO_CHEST, PRE_ASSIGNED_LEFT, FINISHED }
            const string ITEMS_TAG_IDENTIFIER = "ITEM_";
            const string SCENE_TAG_IDENTIFIER = "SCENE_";
            const string CHEST_ID_IDENTIFIER = "CHESTID_";

            List<ChestItem> _availableChests;
            List<ChestItem> _inaccesibleChests;

            public string name;
            ItemRandomizerCore _core;
            Range[] _shuffleRanges;
            string[] _tags;
            string[] _itemList;
            string[] _defaultItems = new string[] { DEFAULT_ITEM };
            int _totalChestsUsed;

            bool _itemListFinished;
            bool _noLogic;
            bool _isVanilla;
            PoolState _state = PoolState.NO_CHEST;
            int _itemIndex = 0;

            string shuffles;
            string[] _orderedItemsList;
            List<string> _itemListStrings;
            string[] _tagNames;

            public RandomizationPool(JSON_ItemConfig.JSON_RandomizationPool pool, ItemRandomizerCore core)
            {
                _availableChests = new List<ChestItem>();
                _inaccesibleChests = new List<ChestItem>();
                _itemListStrings = new List<string>();
                _itemList = pool.itemsList != null ? pool.itemsList : new string[] { };
                _tags = pool.tags != null ? pool.tags : new string[] { };
                _core = core;
                name = string.IsNullOrEmpty(pool.name) ? "Pool" : pool.name;

                //Create the shuffle ranges
                if (pool.allShuffles != null)
                {
                    List<Range> ranges = new List<Range>();
                    for (int i = 0; i < pool.allShuffles.Length; i++)
                    {
                        if (pool.allShuffles[i].end < pool.allShuffles[i].start)
                        {
                            ranges.Add(new Range(pool.allShuffles[i].end, pool.allShuffles[i].start));
                        }
                        else
                        {
                            ranges.Add(new Range(pool.allShuffles[i].start, pool.allShuffles[i].end));
                        }
                    }
                    _shuffleRanges = ranges.ToArray();
                }

                //Get the default items
                if (pool.defaultItems != null && pool.defaultItems.Length > 0)
                {
                    _defaultItems = pool.defaultItems;
                }
            }

            public PoolState GetState()
            {
                return _state;
            }

            public void Reset()
            {
                _itemIndex = 0;
                shuffles = "";
                _state = PoolState.NO_CHEST;
                _itemListFinished = false;
                _noLogic = false;
                _isVanilla = false;
                _itemListStrings.Clear();
                _availableChests.Clear();
                _inaccesibleChests.Clear();
            }

            //Assign chests to the pool
            public void GetChests(List<ChestItem> unassignedChests)
            {
                Reset();

                //Set tag function
                List<TagFilterFunc> filters = new List<TagFilterFunc>();
                bool useModSpawned = _core._useModSpawned;
                _noLogic = _core._noLogic;

                List<string> tagsStrings = new List<string>();
                if(_tags != null)
                {
                    for (int i = 0; i < _tags.Length; i++)
                    {
                        bool invalid = false;
                        if (!string.IsNullOrEmpty(_tags[i]))
                        {
                            //Check if it filter defined by the dictionary
                            if (_tagFilterDictionary.TryGetValue(_tags[i], out TagFilterFunc filterFunc))
                            {
                                filters.Add(filterFunc);
                            }
                            //Check if it is an item tag
                            else if (_tags[i].Contains(ITEMS_TAG_IDENTIFIER))
                            {
                                int pos = _tags[i].IndexOf(ITEMS_TAG_IDENTIFIER);
                                string textParam = _tags[i].Remove(0, ITEMS_TAG_IDENTIFIER.Length);
                                filters.Add(new TagFilterFunc(
                                        delegate (ChestItem item)
                                        {
                                            return item.originalItem == textParam;
                                        }
                                ));
                            }
                            //Check if it is a scene tag
                            else if (_tags[i].Contains(SCENE_TAG_IDENTIFIER))
                            {
                                int pos = _tags[i].IndexOf(SCENE_TAG_IDENTIFIER);
                                string textParam = _tags[i].Remove(0, SCENE_TAG_IDENTIFIER.Length);
                                filters.Add(new TagFilterFunc(
                                        delegate (ChestItem item)
                                        {
                                            return item.scene == textParam;
                                        }
                                ));
                            }
                            //Check if it is a chest id tag
                            else if (_tags[i].Contains(CHEST_ID_IDENTIFIER))
                            {
                                int pos = _tags[i].IndexOf(CHEST_ID_IDENTIFIER);
                                string textParam = _tags[i].Remove(0, CHEST_ID_IDENTIFIER.Length);
                                int chestId = int.TryParse(textParam, out int number) ? number : -1;
                                filters.Add(new TagFilterFunc(
                                        delegate (ChestItem item)
                                        {
                                            return item.chestID == chestId;
                                        }
                                ));
                            }
                            //Vanilla
                            else if (_tags[i] == "VANILLA")
                            {
                                _isVanilla = true;
                            }
                            //No logic (for this pool only)
                            else if (_tags[i] == "NO_LOGIC")
                            {
                                _noLogic = true;
                            }
                            //If the tag is plain wrong, mark it as invalid
                            else invalid = true;

                            //Add name to the print list
                            if (invalid) tagsStrings.Add(_tags[i] + "(INVALID)");
                            else tagsStrings.Add(_tags[i]);
                        }
                    }
                }
                _tagNames = tagsStrings.ToArray();

                //Check if the chest is going to be in the pool
                for (int i = unassignedChests.Count - 1; i >= 0; i--)
                {
                    ChestItem tempChest = unassignedChests[i];

                    //Check if the chest is user spawned. If it is and they are not allowed, skip it
                    if (_core.TagModSpawned(tempChest) && !useModSpawned) continue;

                    //If the chest matches with a tag, add it to the inaccesible chests list 
                    if (CheckTags(tempChest, filters))
                    {
                        if (_isVanilla)
                        {
                            tempChest.assignedItem = tempChest.originalItem;
                            tempChest.isVanilla = true;
                        }
                        else
                        {
                            tempChest.assignedItem = null;
                        }
                        _inaccesibleChests.Add(tempChest);
                        unassignedChests.RemoveAt(i);
                    }
                }

                //If there is no logic, move all the chests to the available pool and clear the inaccesible pool
                if (_noLogic)
                {
                    foreach (var chest in _inaccesibleChests)
                    {
                        _availableChests.Add(chest);
                    }
                    _inaccesibleChests.Clear();
                }

                if(_isVanilla)
                {
                    //If the pool is vanilla, set the ordered list to nothing
                    _orderedItemsList = new string[] { };
                }
                else
                {
                    //Duplicate item array for reordering
                    _orderedItemsList = new string[_itemList.Length];
                    for (int i = 0; i < _orderedItemsList.Length; i++)
                    {
                        string currentItem = _itemList[i];
                        _orderedItemsList[i] = currentItem;
                    }

                    //Shuffle the list by the shuffle ranges
                    if (_shuffleRanges != null)
                    {
                        shuffles = "Item pool shuffles:\n";
                        for (int i = 0; i < _shuffleRanges.Length; i++)
                        {
                            int start = (int)_shuffleRanges[i].min < 0 ? 0 : (int)_shuffleRanges[i].min;
                            int end = (int)_shuffleRanges[i].max < _orderedItemsList.Length ? (int)_shuffleRanges[i].max : _orderedItemsList.Length;
                            if (start == end) continue;
                            shuffles += string.Format("Shuffling from index {0} to {1}\n", start, end, name);
                            for (int j = start; j < end; j++)
                            {
                                int randomIndex = UnityEngine.Random.Range(start, end);
                                string temp = _orderedItemsList[randomIndex];
                                _orderedItemsList[randomIndex] = _orderedItemsList[j];
                                _orderedItemsList[j] = temp;
                            }
                        }
                        shuffles += "\n";
                    }
                }

                _totalChestsUsed = _availableChests.Count + _inaccesibleChests.Count;
            }

            public string GetPoolInfo()
            {
                return string.Format("{0}{1}\n{0}\nTags: {2}\nChests: {3}\n\n{8}Default items ({4}):\n{5}\n\nRandomized items ({6} from a total of {9}):\n{7}\n",
                    POOLS_SEPARATOR, name, string.Join(" / ", _tagNames), _totalChestsUsed,_defaultItems.Length, string.Join("\n", _defaultItems), 
                    _itemListStrings.Count, string.Join("\n", _itemListStrings.ToArray()), shuffles, _orderedItemsList.Length);
            }

            public string PrintInaccesibleChests()
            {
                string[] chests = new string[_inaccesibleChests.Count];
                for (int i = 0; i < _inaccesibleChests.Count; i++) chests[i] = _inaccesibleChests[i].name;
                return string.Join(" / ", chests);
            }

            public int InaccesibleChestsCount()
            {
                return _inaccesibleChests.Count;
            }
            
            //Check tags. If any of the conditions is met, return true
            bool CheckTags(ChestItem chest, List<TagFilterFunc> tagFuncs)
            {
                for (int i = 0; i < tagFuncs.Count; i++)
                {
                    if (tagFuncs[i].Invoke(chest))
                    {
                        return true;
                    }
                }
                return false;
            }

            //Attempts to place an item and returns the pool state
            public PoolState PlaceItem()
            {
                //Update availability
                for (int i = _inaccesibleChests.Count - 1; i >= 0; i--)
                {
                    ChestItem currentChest = _inaccesibleChests[i];
                    if (currentChest.CheckAllConditions())
                    {
                        _inaccesibleChests.RemoveAt(i);
                        _availableChests.Add(currentChest);
                    }
                }
                
                int totalChestCount = _availableChests.Count + _inaccesibleChests.Count;

                //If the pool is vanilla and there are no more chests to assign, finish the pool
                if (_isVanilla && totalChestCount == 0)
                {
                    _state = PoolState.FINISHED;
                    return _state;
                }
                //If the item index reached the end, fill the remaining chests with default items
                else if (_itemIndex == _itemList.Length && !_itemListFinished && !_isVanilla)
                {
                    _itemListFinished = true;

                    _core.Outcome += string.Format("{0} (EVENT): No more items to randomize\n", name);
                    if (totalChestCount > 0)
                    {
                        _core.Outcome += string.Format("{0} (EVENT): Empty chests detected ({1}), filling them with default items\n", name, totalChestCount);
                    }
                }

                //Check if there are available chests. If not, update state and return
                if (_availableChests.Count == 0)
                {
                    //If it hasnt finished, mark this pool with no chests available
                    if (!_itemListFinished) _state = PoolState.NO_CHEST;
                    //If there are inaccesible chests, it failed to set a default item
                    else if (_inaccesibleChests.Count > 0) _state = PoolState.PRE_ASSIGNED_LEFT;
                    //If none of the above, the pool is considered finished
                    else _state = PoolState.FINISHED;

                    return _state;
                }

                //Get random chest
                int randomIndex = UnityEngine.Random.Range(0, _availableChests.Count);
                ChestItem selectedChest = _availableChests[randomIndex];
                _availableChests.RemoveAt(randomIndex);
                string chestType;
                ItemToRandomize tempItem;

                //Place vanilla item
                if (_isVanilla)
                {
                    tempItem = _core.GetItemToRandomize(selectedChest.originalItem);
                    chestType = " (VANILLA)";
                }
                //Place default item
                else if (_itemListFinished)
                {
                    tempItem = _core.GetItemToRandomize(GetDefaultItem());
                    selectedChest.assignedItem = tempItem.name;
                    chestType = " (DEFAULT ITEM)";
                }
                //Place item list item
                else
                {
                    tempItem = _core.GetItemToRandomize(_orderedItemsList[_itemIndex]);

                    //Check if the item needs to be overrided
                    if (_core.CheckIfOverrided(tempItem.name)) tempItem = _core.GetItemToRandomize(GetDefaultItem());
                    _itemListStrings.Add(tempItem.name);

                    selectedChest.assignedItem = tempItem.name;
                    chestType = "";
                }

                //Activate item
                tempItem.ActivateEffect();

                //Check if the chest has a setflag item
                if (!string.IsNullOrEmpty(selectedChest.setFlag))
                {
                    tempItem = _core.GetItemToRandomize(selectedChest.setFlag);
                    tempItem.ActivateEffect();
                }

                //Add to randomized list and increment counter
                _core.AddChestToRandomizedList(selectedChest);
                _itemIndex++;

                //Log outcome
                _core.Outcome += string.Format("{0}:{1} {2} assigned to {3} / Chests remaining: {4}\n", name, chestType, selectedChest.assignedItem, selectedChest.name, _availableChests.Count.ToString());

                //Set state as ITEM_PLACED to restart the untested pools. Return.
                _state = PoolState.ITEM_PLACED;
                return _state;
            }

            string GetDefaultItem()
            {
                int randomIndex = UnityEngine.Random.Range(0, _defaultItems.Length);
                return _defaultItems[randomIndex];
            }
        }
        #endregion

        #region Randomization methods
        //The randomizer itself. Returns true on a successful randomization. "Error" will point out where the randomization failed, otherwise it will be NULL
        public bool RandomizeItems()
        {
            Outcome = "";
            Header = "";
            ConfigInfo = "";
            if (_allChests == null)
            {
                Outcome = "Randomization failed: no chest database loaded.\n";
                Error = Outcome;
                return false;
            }

            if (!_configLoaded)
            {
                Outcome = "Randomization failed: Couldn't load" + _configFile + ".json config file.\n";
                Error = Outcome;
                return false;
            }

            //Reset simulation state, lists and item groups
            _state.Reset();
            _randomizedChests.Clear();
            _initialItems.Clear();
            foreach (KeyValuePair<string, ItemGroup> pair in _itemGroups)
            {
                pair.Value.Reset();
            }

            //Reset global tags
            StickDisallowed = false;
            RollDisallowed = false;
            NoDevUpgrade = false;
            NoEFCSUpgrade = false;
            ReplaceEFCSUpgrade = false;
            OpenDreams = false;
            _useModSpawned = false;
            _noLogic = false;
            _ignoreDefaultError = false;
            _ignoreNoChests = false;
            _snatchLvlStick = false;

            //Reset reload flags
            SpawnBees = false;
            SpawnYellowHearts = false;
            
            //Check global tags
            List<string> tagsStrings = new List<string>();
            if (_globalTags != null)
            {
                for (int i = 0; i < _globalTags.Length; i++)
                {
                    bool invalid = false;
                    if (!string.IsNullOrEmpty(_globalTags[i]))
                    {
                        if (_globalTags[i] == "GLITCHES_ALLOWED")
                        {
                            _state.AllowGlitches();
                        }
                        else if (_globalTags[i] == "NO_LOGIC")
                        {
                            _noLogic = true;
                        }
                        else if (_globalTags[i] == "NO_STICK")
                        {
                            if(OverrideNoStick)
                            {
                                _snatchLvlStick = true;
                            }
                            else
                            {
                                _state.RemoveStick();
                                StickDisallowed = true;
                            }
                        }
                        else if (_globalTags[i] == "NO_ROLL")
                        {
                            if (!OverrideNoRoll)
                            {
                                _state.RemoveRoll();
                                RollDisallowed = true;
                            }
                        }
                        else if (_globalTags[i] == "NO_DEV_UPGRADES")
                        {
                            NoDevUpgrade = true;
                            _state.DisallowDevWeapons();
                        }
                        else if (_globalTags[i] == "NO_EFCS_UPGRADE")
                        {
                            NoEFCSUpgrade = true;
                            _state.DisallowEFCSUpgrade();
                        }
                        else if (_globalTags[i] == "REPLACE_EFCS_UPGRADE")
                        {
                            ReplaceEFCSUpgrade = true;
                        }
                        else if (_globalTags[i] == "NO_DKEYS_LOGIC")
                        {
                            _state.IgnoreDKeys();
                        }
                        else if (_globalTags[i] == "BYPASS_DEFAULT_ITEM_ERROR")
                        {
                            _ignoreDefaultError = true;
                        }
                        else if (_globalTags[i] == "BYPASS_NO_CHESTS_ERROR")
                        {
                            _ignoreNoChests = true;
                        }
                        else if (_globalTags[i] == "OPEN_DREAM_WORLD")
                        {
                            OpenDreams = true;
                            _state.OpenDreams();
                        }
                        /*else if (_globalTags[i] == "GATHER_LOAD_ITEMS")
                        {
                            SpawnBees = true;
                        }*/
                        else if (_globalTags[i] == "USE_MOD_SPAWNED")
                        {
                            _useModSpawned = true;
                        }

                        //If the tag is plain wrong, mark it as invalid
                        else invalid = true;

                        //Add name to the log list
                        if (invalid) tagsStrings.Add(_globalTags[i] + "(INVALID)");
                        else tagsStrings.Add(_globalTags[i]);
                    }
                }
            }

            //Check game menu overrides
            int overrideState = (OverrideNoStick ? 1 : 0) + (OverrideNoRoll ? 2 : 0);
            switch(overrideState)
            {
                case 1:
                    _overrideItemTest = delegate (string name)
                    {
                        if(_snatchLvlStick && name == "StickLvl")
                        {
                            _snatchLvlStick = false;
                            return true;
                        }
                        return false;
                    };
                    break;
                case 2:
                    _overrideItemTest = delegate (string name)
                    {
                        return name == "Roll";
                    };
                    break;
                case 3:
                    _overrideItemTest = delegate (string name)
                    {
                        if (name == "Roll")
                        {
                            return true;
                        }
                        else if (_snatchLvlStick && name == "StickLvl")
                        {
                            _snatchLvlStick = false;
                            return true;
                        }
                        return false;
                    };
                    break;
                default:
                    _overrideItemTest = delegate (string name)
                    {
                        return false;
                    };
                    break;
            }
            
            //Log headers
            Header += string.Format("Item randomization:\n\nConfig file: {1}\nSeed: {0}\n", currentSeed, _configFile, SEPARATOR);
            if (OverrideNoRoll) Header += "No roll overrided\n";
            if (OverrideNoStick) Header += "No stick overrided\n";
            ConfigInfo += "Global tags: " + string.Join(" / ", tagsStrings.ToArray()) + "\n\n";

            //Get starting item list and activate their effects in the simulation
            for (int i = 0; i < _initialItemsJSON.Length; i++)
            {
                if (!string.IsNullOrEmpty(_initialItemsJSON[i]))
                {
                    _initialItems.Add(GetItemToRandomize(_initialItemsJSON[i]));
                    _initialItems[i].ActivateEffect();
                }
            }

            //Log initial items lists
            string itemListInitial = "";
            for (int i = 0; i < _initialItems.Count; i++)
            {
                itemListInitial += _initialItems[i].GetNameForPrint() + "\n";
            }
            ConfigInfo += string.Format("Initial items ({0}):\n{1}{2}\n", _initialItems.Count, itemListInitial, SEPARATOR);

            //Initialize pools
            List<RandomizationPool> _availablePools = new List<RandomizationPool>();
            List<RandomizationPool> _untestedPools = new List<RandomizationPool>();
            List<ChestItem> unassignedChests = new List<ChestItem>();

            for (int i = 0; i < _allChests.Length; i++)
            {
                _allChests[i].isVanilla = false;
                unassignedChests.Add(_allChests[i]);
            }
            for (int i = 0; i < _allPools.Count; i++)
            {
                _availablePools.Add(_allPools[i]);
                _untestedPools.Add(_allPools[i]);
                _allPools[i].GetChests(unassignedChests);
            }
            
            Outcome += "Randomization progress:\n\n";

            //While there are untested pools...
            while (_untestedPools.Count > 0)
            {
                //Get a random pool
                int randomIndex = UnityEngine.Random.Range(0, _untestedPools.Count);
                RandomizationPool selected = _untestedPools[randomIndex];

                //Attempt to place an item and get the pool's state
                RandomizationPool.PoolState state = selected.PlaceItem();

                //Check the pool state
                switch (state)
                {
                    //If an item was placed, reset the untested pools list with all the available pools
                    case RandomizationPool.PoolState.ITEM_PLACED:
                        _untestedPools.Clear();
                        for (int i = 0; i < _availablePools.Count; i++)
                        {
                            _untestedPools.Add(_availablePools[i]);
                        }
                        break;
                    //If the pool finished, remove it from both pools lists
                    case RandomizationPool.PoolState.FINISHED:
                        Outcome += string.Format("{0} (EVENT): Finished\n", selected.name);
                        _untestedPools.Remove(selected);
                        _availablePools.Remove(selected);
                        break;
                    //In all other cases, remove it from the untested pools list
                    default:
                        _untestedPools.Remove(selected);
                        break;
                }
            }
            
            //If there are no more untested pools, consider the randomization over
            Outcome += string.Format("\n--- Randomization finished ---\n\n");

            //Check and log the end state of each pool
            bool error = false;
            for (int i = 0; i < _allPools.Count; i++)
            {
                RandomizationPool selected = _allPools[i];
                RandomizationPool.PoolState state = selected.GetState();
                string errorType;
                switch (state)
                {
                    case RandomizationPool.PoolState.PRE_ASSIGNED_LEFT:
                        //Check if the error should be bypassed
                        if (!_ignoreDefaultError)
                        {
                            error = true;
                            errorType = "ERROR";
                        }
                        else
                        {
                            errorType = "Error bypassed";
                        }
                        Outcome += string.Format("{0}: {1}. Chests with default items left inaccesible ({2} chests). List: {3}\n", selected.name, errorType, selected.InaccesibleChestsCount(), selected.PrintInaccesibleChests());
                        break;
                    case RandomizationPool.PoolState.NO_CHEST:
                        //Check if the error should be bypassed
                        if (!_ignoreNoChests)
                        {
                            error = true;
                            errorType = "ERROR";
                        }
                        else
                        {
                            errorType = "Error bypassed";
                        }
                        Outcome += string.Format("{0}: {1}. No more chests available ({2} inaccesible chests). List: {3}\n", selected.name, errorType, selected.InaccesibleChestsCount(), selected.PrintInaccesibleChests());
                        break;
                    default:
                        Outcome += string.Format("{0}: Pool randomized succesfully\n", selected.name);
                        break;
                }
            }
            
            //Log pools info in order
            for (int i = 0; i < _allPools.Count; i++)
            {
                ConfigInfo += _allPools[i].GetPoolInfo() + SEPARATOR_WITH_JUMPS;
            }

            //If the simulation failed, mark the error and return false
            if (error)
            {
                string errorString = "Randomization failed\n";
                Error = errorString;
                Outcome += errorString;
                return false;
            }

            //If this point was reached, congratulations! The randomization succeded
            Error = null;
            Outcome += "\nRandomization SUCCEDED!\n";
            return true;
        }

        //Check if item is overrided
        public bool CheckIfOverrided(string name)
        {
            return _overrideItemTest(name);
        }

        //Get item to randomize
        ItemToRandomize GetItemToRandomize(string name)
        {
            string itemString = GetItemName(name);
            return ItemToRandomize.CreateItem(itemString, _state);
        }

        //Recursive GetItem method
        string GetItemName(string name)
        {
            if (_itemGroups.TryGetValue(name, out ItemGroup group))
            {
                return group.GetItem(this);
            }
            else
            {
                return name;
            }
        }

        void AddChestToRandomizedList(ChestItem chest)
        {
            /*if(!string.IsNullOrEmpty(chest.assignedItem) && chest.assignedItem != NO_ITEM)*/ _randomizedChests.Add(chest);
        }
        #endregion

        #region Game interaction
        //Takes the list of initial items and saves them into the file
        public void SaveInitialItems(RealDataSaver saver)
        {
            //Setup initial state to save file
            for (int i = 0; i < _initialItems.Count; i++)
            {
                ModItemHandler.GiveFileCreationItem(_initialItems[i].name, saver, this);
            }
        }

        public void SaveCardCount(RealDataSaver saver)
        {
            int[] cardList = _state.GetCardList();
            ModMaster.SetNewGameData("mod/cardcount", cardList.Length.ToString(), saver);
        }

        //Shows the "You got an item" UI with a custom image and text. Image is loaded from resources
        public void PrintInMessageBox(string image, string text)
        {
            if(_entityHUD == null)
            {
                _entityHUD = GameObject.Find("OverlayCamera").transform.parent.GetComponent<EntityHUD>();
            }
            _entityHUD.ForceGotItem(image, text);

        }

        //Shows the "You got an item" UI with a custom image and text. Image is loaded from disk
        public void PrintInMessageBoxFromDisk(string pngName, string text)
        {
            if (_entityHUD == null)
            {
                _entityHUD = GameObject.Find("OverlayCamera").transform.parent.GetComponent<EntityHUD>();
            }

            //Load image texture
            string path = ModMaster.TexturesPath + pngName + ".png";
            Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            byte[] fileData;
            if (File.Exists(path))
            {
                fileData = File.ReadAllBytes(path);
                tex.LoadImage(fileData, false);
            }
            tex.name = pngName;

            _entityHUD.ForceGotItem(tex, text);
        }

        //Called for every item spawned in chests when "Randomizing" is true. On false, the chest returns the original item, on true
        //it returns the item specific to that spot. If the chest is not found in the randomized chests list, it returns true and gives the "Nothing" item.
        public bool TryGiveRandomizedItem(Item item)
        {
            string scene = SceneManager.GetActiveScene().name;
            string room = ModMaster.GetMapRoom();

            for (int i = 0; i < _randomizedChests.Count; i++)
            {
                ChestItem chestItem = _randomizedChests[i];
                if (chestItem.scene == scene && chestItem.room == room && chestItem.saveId == item.saveId)
                {
                    if(TagBosses(chestItem) && (TagDungeon(chestItem) || TagDreamWorld(chestItem) || TagSecretDungeon(chestItem)))
                    {
                        ModSaver.SaveBoolToFile("mod/itemRandomizer/bosses", scene, true);
                    }
                    //If the assigned and original items are the same, this item is vanilla
                    if (chestItem.isVanilla) return false;

                    //Give randomized item
                    ModItemHandler.GiveInGameItem(chestItem.assignedItem, item, this);
                    return true;
                }
            }
            
            //Item not found in the randomized chests list, give empty
            ModItemHandler.GiveInGameItem(NO_ITEM, item, this);
            return true;
        }
        #endregion

        #region Print functions
        //Writes to file the cheat sheet
        public void PrintSheetTxt()
        {
            if (string.IsNullOrEmpty(currentSeed)) { currentSeed = "InvalidSeed"; }
            string text = string.Format("{0}{1}{2}", Header, SEPARATOR_WITH_JUMPS, PrintRandomizedChestsData());
            File.WriteAllText(ModMaster.RandomizerPath + _configFile + "_" + currentSeed + ".txt", text);
        }

        //Writes to file extended cheat sheet
        public void PrintExtensiveData()
        {
            if (string.IsNullOrEmpty(currentSeed)) { currentSeed = "InvalidSeed"; }
            string text = string.Format("{0}{1}{2}{4}\n{3}", Header, ConfigInfo, Outcome, PrintChestsData(), SEPARATOR);
            File.WriteAllText(ModMaster.RandomizerPath + _configFile + "_" + currentSeed + "_EXTENDED.txt", text);
        }

        //Prints the data of ALL chests
        public string PrintChestsData()
        {
            if (_allChests == null) return null;
            ChestItem[] chestData = _allChests.OrderBy(o => o.scene).ThenBy(o => o.room).ToArray();
            string[] output = new string[_allChests.Length];
            for (int i = 0; i < chestData.Length; i++)
            {
                output[i] = chestData[i].ToString();
            }
            return "Database chests:\n" + SEPARATOR + "\n" + string.Join(SEPARATOR_WITH_JUMPS, output);
        }

        //Prints the short data of randomized chests
        public string PrintRandomizedChestsData()
        {
            ChestItem[] chestData = _randomizedChests.OrderBy(o => o.scene).ThenBy(o => o.room).ToArray();
            string[] output = new string[_randomizedChests.Count];
            for (int i = 0; i < chestData.Length; i++)
            {
                output[i] = chestData[i].GetShortInfo();
            }
            return string.Join(SEPARATOR_WITH_JUMPS, output);
        }
        #endregion

        #region Yellow Heart
        private Item yellowHeart;
        /*void CheckIfLoadRequired(string itemName)
        {
            if (itemName.Contains("ENEMY_BeeSwarm") || itemName.Contains("YellowHeart")) SpawnBees = true;
        }*/

        public bool CanSpawnHeart()
        {
            return yellowHeart != null;
        }

        public void FetchHeart()
        {
            ModText.QuickText("Trying to get heart");
            if (FindItem("Item_Heart4", out Item itemOutput))
            {
                ModText.QuickText("Found it");
                GameObject yellow = GameObject.Instantiate(itemOutput.gameObject);
                yellow.SetActive(false);
                yellow.name = itemOutput.name;
                GameObject.DontDestroyOnLoad(yellow);
                yellowHeart = yellow.GetComponent<Item>();
            }
        }
        
        //Spawns an item given an item component. Fast!
        public bool GiveYellowHeart(Vector3 position)
        {
            if (yellowHeart != null)
            {
                Item spawneditem = ItemFactory.Instance.GetItem(yellowHeart, null, position, true);
                spawneditem.gameObject.name = yellowHeart.gameObject.name;
                Entity player = GameObject.Find("PlayerEnt").GetComponent<Entity>();
                spawneditem.Pickup(player);
                return true;
            }
            return false;
        }

        //Finds an itemcomponent using a string. Does a resources.findall, SLOW
        public bool FindItem(string itemname, out Item itemcomponent)
        {
            itemcomponent = null;

            foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (gameObject.name.ToLower() == itemname.ToLower())
                {
                    itemcomponent = gameObject.GetComponent<Item>();
                    return true;
                }
            }
            return false;
        }

        public bool CheckIfNoStickConfig()
        {
            if (_globalTags != null)
            {
                for (int i = 0; i < _globalTags.Length; i++)
                {
                    if (!string.IsNullOrEmpty(_globalTags[i]) && _globalTags[i] == "NO_STICK")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckIfNoRollConfig()
        {
            if (_globalTags != null)
            {
                for (int i = 0; i < _globalTags.Length; i++)
                {
                    if (!string.IsNullOrEmpty(_globalTags[i]) && _globalTags[i] == "NO_ROLL")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckIfNoLogicConfig()
        {
            if (_globalTags != null)
            {
                for (int i = 0; i < _globalTags.Length; i++)
                {
                    if (!string.IsNullOrEmpty(_globalTags[i]) && _globalTags[i] == "NO_LOGIC")
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Tag filters
        bool TagIgnore(ChestItem item)
        {
            return false;
        }

        bool TagAll(ChestItem item)
        {
            return true;
        }

        bool TagModSpawned(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.ModSpawned) != ChestItem.Tags.None;
        }

        bool TagDreamWorld(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.DreamWorld) != ChestItem.Tags.None;
        }

        bool TagRaftPieces(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.RaftPiece) != ChestItem.Tags.None;
        }

        bool TagCards(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.Card) != ChestItem.Tags.None;
        }
        
        bool TagDungeonKeys(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.DungeonKey) != ChestItem.Tags.None;
        }

        bool TagDungeon(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.Dungeon) != ChestItem.Tags.None;
        }

        bool TagSecretDungeon(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.SecretDungeon) != ChestItem.Tags.None;
        }

        bool TagCaves(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.Cave) != ChestItem.Tags.None;
        }

        bool TagBosses(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.Boss) != ChestItem.Tags.None;
        }

        bool TagPortalWorlds(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.PortalWorld) != ChestItem.Tags.None;
        }

        bool TagDreamWorlds(ChestItem item)
        {
            return (item.tags & ChestItem.Tags.DreamWorld) != ChestItem.Tags.None;
        }
        #endregion
    }
}
