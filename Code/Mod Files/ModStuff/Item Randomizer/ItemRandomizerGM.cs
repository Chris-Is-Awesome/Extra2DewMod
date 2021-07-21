using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.ItemRandomizer
{
    //The game mode singleton. Chests will look for this class when giving items
    public class ItemRandomizerGM : E2DGameModeSingleton<ItemRandomizerGM>
    {
        void Awake()
        {
            Mode = ModeControllerNew.ModeType.ItemRandomizer;
            Title = "Item Randomizer";
            QuickDescription = "EXPERT PLAYERS ONLY\nRandomize important items.";
            Description = "Randomizes overworld, dungeon and cave items not dropped by enemies. Randomization is fully customizable and can wildly change the flow of a run, changing which items are added to the pool or which chests are randomized. Randomization files can be found inside `extra2dew/randomizer/config` and cannot have spaces in their names.";
            FileNames = new List<string> { "itemrandomizer", "crazyitems", "iteemms" };
            Restrictions = new List<ModeControllerNew.ModeType> { ModeControllerNew.ModeType.DungeonRush, ModeControllerNew.ModeType.BossRush };
        }

        ItemRandomizerCore _core;
        public ItemRandomizerCore Core
        {
            get
            {
                if (_core == null) _core = new ItemRandomizerCore();
                return _core;
            }
        }

        bool fileCreated;
        bool randomizationDone;
        override public void SetupSaveFile(RealDataSaver saver)
        {
            if (saver == null) return;
            
            ModMaster.SetNewGameData("mod/randomizer_seed", RNGSeed.CurrentSeed, saver);
            ModMaster.SetNewGameData("mod/item_randomizer_file", configFileName, saver);
            if(Core.SpawnBees) ModMaster.SetNewGameData("settings/hideCutscenes", "1", saver);
            fileCreated = true;
            Activate();
            if (randomizationDone)
            {
                Core.SaveInitialItems(saver);
                Core.SaveCardCount(saver);
                if(_core.OverrideNoStick)
                {
                    ModMaster.SetNewGameData("mod/override_nostick", "1", saver);
                }
                else if(Core.StickDisallowed)
                {
                    ModMaster.SetNewGameData("player/vars/melee", "100", saver);
                    ModMaster.SetNewGameData("mod/nostick", "1", saver);
                }
                if (_core.OverrideNoRoll)
                {
                    ModMaster.SetNewGameData("mod/override_noroll", "1", saver);
                }
                else if (Core.RollDisallowed)
                {
                    ModMaster.SetNewGameData("mod/noroll", "1", saver);
                }
                if(_startWithTracker)
                {
                    ModMaster.SetNewGameData("player/vars/tracker", "3", saver);
                }
                if(_warpGardenOpen)
                {
                    ModMaster.SetNewGameData("world/WorldWarpA", "1", saver);
                    ModMaster.SetNewGameData("world/WorldWarpB", "1", saver);
                    ModMaster.SetNewGameData("world/WorldWarpC", "1", saver);
                    ModMaster.SetNewGameData("world/WorldWarpD", "1", saver);
                    ModMaster.SetNewGameData("world/WorldWarpE", "1", saver);
                    ModMaster.SetNewGameData("world/WorldWarpF", "1", saver);
                    ModMaster.SetNewGameData("world/WorldWarpG", "1", saver);
                    ModMaster.SetNewGameData("world/WorldWarpH", "1", saver);
                    ModMaster.SetNewGameData("world/WorldWarpI", "1", saver);
                    ModMaster.SetNewGameData("world/WorldWarpJ", "1", saver);
                    ModMaster.SetNewGameData("markers/WorldWarpA", "WorldWarpA", saver);
                    ModMaster.SetNewGameData("markers/WorldWarpB", "WorldWarpB", saver);
                    ModMaster.SetNewGameData("markers/WorldWarpC", "WorldWarpC", saver);
                    ModMaster.SetNewGameData("markers/WorldWarpD", "WorldWarpD", saver);
                    ModMaster.SetNewGameData("markers/WorldWarpE", "WorldWarpE", saver);
                    ModMaster.SetNewGameData("markers/WorldWarpF", "WorldWarpF", saver);
                    ModMaster.SetNewGameData("markers/WorldWarpG", "WorldWarpG", saver);
                    ModMaster.SetNewGameData("markers/WorldWarpH", "WorldWarpH", saver);
                    ModMaster.SetNewGameData("markers/WorldWarpJ", "WorldWarpI", saver);
                    ModMaster.SetNewGameData("markers/WorldWarpI", "WorldWarpJ", saver);
                }
            }
            Core.PrintSheetTxt();
        }

        override public void Activate()
        {
            string seedString = fileCreated ? RNGSeed.CurrentSeed : ModSaver.LoadStringFromFile("mod", "randomizer_seed");
            string fileString = fileCreated ? configFileName : ModSaver.LoadStringFromFile("mod", "item_randomizer_file");
            if(!fileCreated)
            {
                _core.OverrideNoStick = ModSaver.LoadBoolFromFile("mod", "override_nostick");
                _core.OverrideNoRoll = ModSaver.LoadBoolFromFile("mod", "override_noroll");
            }
            randomizationDone = Core.RandomizeWithSeed(fileString, seedString);
            Core.Randomizing = true;
            GameStateNew.OnPlayerSpawn += OnPlayerSpawn;
            fileCreated = false;
            IsActive = true;
        }

        override public void Deactivate()
        {
            Core.Randomizing = false;
            fileCreated = false;
            IsActive = false;
            _timer = 0f;
            _getPrefabs = false;
            GameStateNew.OnPlayerSpawn -= OnPlayerSpawn;
        }

        #region Event hooks

        void OnPlayerSpawn(bool isRespawn)
        {
            //If the stick is disabled in the save file, disable it in the playercontroller
            if (ModSaver.LoadBoolFromFile("mod", "nostick"))
            {
                GameObject.Find("PlayerController").GetComponent<PlayerController>().AllowStick = false;
            }

            //If roll is disabled in the save file, disable it in the playercontroller
            if (ModSaver.LoadBoolFromFile("mod", "noroll"))
            {
                GameObject.Find("PlayerController").GetComponent<PlayerController>().AllowRoll = false;
            }

            //If an HP change has been queued from file creation, apply it
            int hpUpdate = ModSaver.LoadIntFromFile("mod", "updateMaxHP");
            if (hpUpdate != 0)
            {
                Killable entityComponent = GameObject.Find("PlayerEnt").GetComponent<Entity>().GetEntityComponent<Killable>();
                float direction = 1f;
                if (hpUpdate < 0)
                {
                    direction = -1f;
                    hpUpdate = -hpUpdate;
                }
                for (int i = 0; i < hpUpdate; i++) UpdatePlayerMaxHP(direction, entityComponent);

                //Reset HP change queue to 0
                ModSaver.SaveIntToFile("mod", "updateMaxHP", 0);
            }/*
            if(_getPrefabs)
            {
                _getPrefabs = false;
                ModSpawner.Instance.HoldNPC("BeeSwarm", "BeeSwarm");
                _core.FetchHeart();
                _timer = 0.5f;
            }
            else if(_core.SpawnYellowHearts || _core.SpawnBees)
            {
                GameObject[] npc = ModSpawner.Instance.FindEntityAndController("BeeSwarm");
                if (npc[0] == null)
                {
                    _getPrefabs = true;
                    _departingFromScene = ModMaster.GetMapName();
                    _departingFromDoor = ModMaster.GetMapSpawn();
                    _timer = 0.5f;
                }
            }*/
        }
        string _departingFromScene = "";
        string _departingFromDoor = "";
        bool _hideCutscenes = true;
        bool _getPrefabs = false;
        float _timer;

        void UpdatePlayerMaxHP(float amount, Killable entityComponent)
        {
            float newHP = entityComponent.MaxHp + amount;
            if (newHP <= 0) newHP = 1;
            entityComponent.MaxHp = newHP;
            entityComponent.CurrentHp = newHP;
            ModSaver.SaveFloatToFile("player", "hp", entityComponent.CurrentHp);
            ModSaver.SaveFloatToFile("player", "maxHp", entityComponent.MaxHp);
        }
        /*
        void Update()
        {
            if(_timer > 0f)
            {
                _timer -= Time.deltaTime;
                if(_timer <= 0f)
                {
                    if(_getPrefabs)
                    {
                        ModMaster.LoadScene("MachineFortress", "MachineFortressInside", false);
                    }
                    else
                    {
                        ModMaster.LoadScene(_departingFromScene, _departingFromDoor, false);
                    }
                }
            }
        }*/

        #endregion

        #region GameMode menu
        public void TestRunRandomize()
        {
            randomizationDone = Core.RandomizeWithSeed(configFileName, RNGSeed.CurrentSeed);
        }

        UITextFrame seed;
        GameObject holder0;
        GameObject holder1;
        GameObject holder2;
        JSON_ItemConfig configJson;

        JSON_ItemConfig LoadConfigFile(string name)
        {
            string jsonString = ModMaster.RandomizerConfigFilesPath + name + ".json";
            if (System.IO.File.Exists(jsonString))
            {
                try
                {
                    string allText = System.IO.File.ReadAllText(jsonString);
                    configJson = JsonUtility.FromJson<JSON_ItemConfig>(allText);
                    return configJson;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;
        }

        string configFileName;
        string[] defaultConfigsOrder = new string[]
        {
            "Simple", "Dungeon_Diver", "Scrambled_Vanilla", "Queen_of_Adventure", "Dungeon_Master", "Keybearer", "Chaotic_Neutral",
            "Chaotic_Evil", "Frog's_Quest"
        };
        List<string> properConfigFiles;
        void FillFilesList(UI2DList fileList, UIButton selectFile, bool updateScreen)
        {
            properConfigFiles = new List<string>();
            string path = ModMaster.RandomizerConfigFilesPath;
            
            foreach (string file in System.IO.Directory.GetFiles(path))
            {
                int pos = file.IndexOf(".json");
                if (pos >= 0)
                {
                    string beforeTxt = file.Remove(pos);
                    beforeTxt = beforeTxt.Remove(0, path.Length);
                    if (beforeTxt.Contains(" ")) continue;
                    properConfigFiles.Add(beforeTxt);
                }
            }
            if (properConfigFiles.Count > 0)
            {
                //Sort lists using predetermined, then add the rest
                List<string> tempConfigList = new List<string>();
                for(int i = 0; i < defaultConfigsOrder.Length; i++)
                {
                    string nameToFind = defaultConfigsOrder[i];
                    for (int j = 0; j < properConfigFiles.Count; j++)
                    {
                        if(nameToFind == properConfigFiles[j])
                        {
                            tempConfigList.Add(properConfigFiles[j]);
                            properConfigFiles.RemoveAt(j);
                            break;
                        }
                    }
                }

                properConfigFiles.Sort();

                for (int j = 0; j < tempConfigList.Count; j++)
                {
                    properConfigFiles.Add(tempConfigList[j]);
                }

                List<string> configFiles = new List<string>();
                foreach (string oneName in properConfigFiles)
                {
                    configFiles.Add(oneName.Replace("_", " "));
                }
                fileList.ExplorerArray = configFiles.ToArray();
                fileList.StringValue = selectFile.UIName;
                if(updateScreen)
                {
                    holder0.SetActive(false);
                    holder1.SetActive(true);
                }
            }
            else
            {
                selectFile.UIName = "No files available";
            }
        }

        void GetValidSeed()
        {
            Core.LoadChestsData(out string errors);
            Core.LoadConfigFile(configFileName);
            for (int i = 0; i < 100; i++)
            {
                RNGSeed.ReRollSeed();
                Core.RandomizeNoLoads(RNGSeed.CurrentSeed);
                if (string.IsNullOrEmpty(Core.Error))
                {
                    seed.UIName = RNGSeed.CurrentSeed;
                    return;
                }
            }
            seed.UIName = "NO VALID SEED FOUND";
        }

        void ValidateSeed()
        {
            Core.RandomizeWithSeed(configFileName, RNGSeed.CurrentSeed);
            bool success = string.IsNullOrEmpty(Core.Error);
            seed.UIName = success ? RNGSeed.CurrentSeed : "INVALID SEED";
        }

        bool showedOnce = false;
        UI2DList fileList;
        UIButton selectFile;
        const float SEPARATION_Y_OPTIONS = -1.25f;
        bool _startWithTracker;
        bool _warpGardenOpen;
        public override void CreateOptions()
        {
            GameObject output = new GameObject();

            //Reset options

            Core.OverrideNoStick = false;
            _core.OverrideNoRoll = false;
            _startWithTracker = false;
            _warpGardenOpen = false;
            
            //Holders for the first screen with file name and seed; the second one is for sile select; third is for options
            holder0 = new GameObject();
            holder0.transform.SetParent(output.transform, false);
            holder1 = new GameObject();
            holder1.transform.SetParent(output.transform, false);
            holder2 = new GameObject();
            holder2.transform.SetParent(output.transform, false);
            showedOnce = false;

            //File select
            selectFile = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -1f, 1.6f, holder0.transform, "Click to select file");

            //Info popup
            UIButton info = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 2f, 1.6f, holder0.transform, "Info");
            info.ScaleBackground(new Vector2(0.30f, 1f));
            UITextFrame infoText = UIFactory.Instance.CreatePopupFrame(0f, -0.6f, info, holder0.transform, "Hover over this button to\nread the confilg file info");
            infoText.transform.localPosition += new Vector3(0f, 0f, -0.2f);
            infoText.ScaleBackground(new Vector2(1.7f, 3.2f));

            //File list
            fileList = UIFactory.Instance.Create2DList(-0.5f, 0f, new Vector2(2f, 4f), Vector2.one * 0.8f, new Vector2(1f, 0.5f), new Vector2(1f, 1.4f), holder1.transform);
            fileList.Title.gameObject.SetActive(false);
            fileList.ScrollBar.ResizeLength(4);

            //Seed
            seed = UIFactory.Instance.CreateTextFrame(0f, 0.4f, holder0.transform);

            //File selection logic
            selectFile.onInteraction += delegate ()
            {
                FillFilesList(fileList, selectFile, true);
            };

            fileList.onInteraction += delegate (string text, int index)
            {
                holder0.SetActive(true);
                holder1.SetActive(false);
                JSON_ItemConfig tempConfig = null;
                if (properConfigFiles != null && index < properConfigFiles.Count) tempConfig = LoadConfigFile(properConfigFiles[index]);
                if (tempConfig != null)
                {
                    configJson = tempConfig;
                    selectFile.UIName = text;
                    configFileName = properConfigFiles[index];
                    ValidateSeed();
                    string infoMessage = configJson.info;
                    List<string> modifiers = new List<string>();
                    if (Core.CheckIfNoStickConfig()) modifiers.Add("NO STICK");
                    if (Core.CheckIfNoRollConfig()) modifiers.Add("NO ROLL");
                    if (Core.CheckIfNoLogicConfig()) modifiers.Add("NO LOGIC");
                    if (modifiers.Count > 0) infoMessage += "\n\n" + string.Join(" / ", modifiers.ToArray());
                    infoText.UIName = infoMessage;
                }
                else
                {
                    selectFile.UIName = "Error loading file";
                }
            };

            //FillFilesList(fileList, selectFile, false);

            //Gamemode options
            UIButton options = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 0f, -1.6f, holder0.transform, "Options");
            options.onInteraction += delegate ()
            {
                holder0.SetActive(false);
                holder2.SetActive(true);
            };

            //No stick override
            UICheckBox noStickOverride = UIFactory.Instance.CreateCheckBox(-2f, 1.25f, holder2.transform, "Override No-Stick");
            noStickOverride.onInteraction += delegate (bool box)
            {
                _core.OverrideNoStick = box;
                ValidateSeed();
            };

            //No roll override
            UICheckBox noRollOverride = UIFactory.Instance.CreateCheckBox(2f, 1.25f, holder2.transform, "Override No-Roll");
            noRollOverride.onInteraction += delegate (bool box)
            {
                _core.OverrideNoRoll = box;
                ValidateSeed();
            };

            //Start with tracker 3
            UICheckBox startWithTracker = UIFactory.Instance.CreateCheckBox(-2f, 1.25f + SEPARATION_Y_OPTIONS, holder2.transform, "Start with Tracker 3");
            startWithTracker.onInteraction += delegate (bool box)
            {
                _startWithTracker = box;
            };

            //Warp garden
            UICheckBox warpGarden = UIFactory.Instance.CreateCheckBox(2f, 1.25f + SEPARATION_Y_OPTIONS, holder2.transform, "Warp Garden unlocked");
            warpGarden.onInteraction += delegate (bool box)
            {
                _warpGardenOpen = box;
            };

            //Close menu
            UIButton optionsBack = UIFactory.Instance.CreateButton(UIButton.ButtonType.Back, 0f, 1f + SEPARATION_Y_OPTIONS * 2f, holder2.transform);
            optionsBack.onInteraction += delegate ()
            {
                holder2.gameObject.SetActive(false);
                holder0.gameObject.SetActive(true);
            };

            //Reroll seed
            UIButton rerollSeed = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -1.5f, -0.6f, holder0.transform, "New seed");
            rerollSeed.ScaleBackground(new Vector2(0.8f, 1f), Vector2.one * 0.7f);
            rerollSeed.onInteraction += GetValidSeed;

            //Paste seed
            UIButton pasteSeed = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 1.5f, -0.6f, holder0.transform, "Paste seed");
            pasteSeed.ScaleBackground(new Vector2(0.8f, 1f), Vector2.one * 0.7f);
            pasteSeed.onInteraction += delegate ()
            {
                RNGSeed.SetSeed(GUIUtility.systemCopyBuffer);
                ValidateSeed();
            };

            holder1.SetActive(false);
            holder2.SetActive(false);
            MenuGo = output;
        }

        public override void OnOpenMenu()
        {
            if(!showedOnce)
            {
                showedOnce = true;
                FillFilesList(fileList, selectFile, false);
                if (fileList.ExplorerArray != null && fileList.ExplorerArray.Length > 0) fileList.Trigger("Simple");
            }
            else
            {
                ValidateSeed();
            }
            holder0.SetActive(true);
            holder1.SetActive(false);
            holder2.SetActive(false);
        }
        #endregion

        #region Dungeon keys menu
        DungeonKeysInfo[] keysInfo;
        DungeonKeysInfo _currentKeyInfo;
        UIButton _cardCount;
        const float X_INITIAL = -6f;
        const float X_INITIAL_DREAMS = X_INITIAL - X_SEPARATION * 0.5f;
        const float X_MID_POINT = X_INITIAL_DREAMS + X_SEPARATION * 2f;
        const float Y_INITIAL_MAIN = 3.5f;
        const float Y_INITIAL_SECRET = Y_INITIAL_MAIN - Y_SEPARATION * 3.2f;
        const float Y_INITIAL_DREAMS = Y_INITIAL_SECRET - Y_SEPARATION * 2.2f;
        const float X_SEPARATION = 1.2f;
        const float Y_SEPARATION = 1.2f;

        public void UpdateDungeonKeyScreen()
        {
            if(Core.Randomizing)
            {
                foreach (DungeonKeysInfo key in keysInfo)
                {
                    key.UpdateButton();
                }
            }
            DungeonKeysInfo.ForceUpdate();
        }

        public UIScreen GetDungeonKeyScreen()
        {
            UIScreen output = UIScreen.CreateBaseScreen("Randomizer");
            if (!Core.Randomizing) return output;

            Transform parent = output.transform;

            //Back button
            output.BackButton.transform.localPosition = new Vector3(4f, output.BackButton.transform.localPosition.y, output.BackButton.transform.localPosition.z);

            //Info
            UIBigFrame info = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 4f, 1.8f, parent);
            info.ScaleBackground(new Vector2(0.45f, 1.5f));

            //Title
            UITextFrame infoTitle = UIFactory.Instance.CreateTextFrame(4f, 4.7f, parent, "No keys found");
            infoTitle.ScaleBackground(new Vector2(1.2f, 1f));

            //Buttons background
            UIBigFrame buttonsBackground = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, X_MID_POINT, 0.3f, parent);
            buttonsBackground.ScaleBackground(new Vector2(0.40f, 2.30f));

            //Dungeons titles
            UITextFrame mainDungeons = UIFactory.Instance.CreateTextFrame(X_MID_POINT, Y_INITIAL_MAIN + Y_SEPARATION, parent, "Main Dungeons");
            UITextFrame secretDungeons = UIFactory.Instance.CreateTextFrame(X_MID_POINT, Y_INITIAL_SECRET + Y_SEPARATION, parent, "Secret Dungeons");
            UITextFrame dreamDungeons = UIFactory.Instance.CreateTextFrame(X_MID_POINT, Y_INITIAL_DREAMS + Y_SEPARATION, parent, "Dream Dungeons");

            DungeonKeysInfo.ClearCurrentSetInfo();
            IDataSaver saver = ModItemHandler.GetDkeysSaver();
            IDataSaver bossesSaver = ModSaver.GetSaver("mod/itemRandomizer/bosses");
            keysInfo = new DungeonKeysInfo[]
            {
                //Main dungeons
                new DungeonKeysInfo("PillowFort", X_INITIAL, Y_INITIAL_MAIN, "DKEYS_PillowFort", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("SandCastle", X_INITIAL + X_SEPARATION, Y_INITIAL_MAIN, "DKEYS_SandCastle", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("ArtExhibit", X_INITIAL + 2 * X_SEPARATION, Y_INITIAL_MAIN, "DKEYS_ArtExhibit", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("TrashCave", X_INITIAL + 3 * X_SEPARATION, Y_INITIAL_MAIN, "DKEYS_TrashCave", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("FloodedBasement", X_INITIAL, Y_INITIAL_MAIN - Y_SEPARATION, "DKEYS_FloodedBasement", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("PotassiumMine", X_INITIAL + X_SEPARATION, Y_INITIAL_MAIN - Y_SEPARATION, "DKEYS_PotassiumMine", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("BoilingGrave", X_INITIAL + 2 * X_SEPARATION, Y_INITIAL_MAIN - Y_SEPARATION, "DKEYS_BoilingGrave", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("GrandLibrary", X_INITIAL + 3 * X_SEPARATION, Y_INITIAL_MAIN - Y_SEPARATION, "DKEYS_GrandLibrary", saver, bossesSaver, infoTitle, info, parent, "GrandLibrary2"),
                //Secret dungeons
                new DungeonKeysInfo("SunkenLabyrinth", X_INITIAL, Y_INITIAL_SECRET, "DKEYS_SunkenLabyrinth", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("MachineFortress", X_INITIAL + X_SEPARATION, Y_INITIAL_SECRET, "DKEYS_MachineFortress", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("DarkHypostyle", X_INITIAL + 2 * X_SEPARATION, Y_INITIAL_SECRET, "DKEYS_DarkHypostyle", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("TombOfSimulacrum", X_INITIAL + 3 * X_SEPARATION, Y_INITIAL_SECRET, "DKEYS_TombOfSimulacrum", saver, bossesSaver, infoTitle, info, parent),
                //Dream dungeons
                //new DungeonKeysInfo("DreamForce", X_INITIAL, Y_INITIAL_DREAMS, "DKEYS_DreamForce", saver, infoTitle, info, parent),
                new DungeonKeysInfo("DreamDynamite", X_INITIAL, Y_INITIAL_DREAMS, "DKEYS_DreamDynamite", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("DreamIce", X_INITIAL + 1 * X_SEPARATION, Y_INITIAL_DREAMS, "DKEYS_DreamIce", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("DreamFireChain", X_INITIAL + 2 * X_SEPARATION, Y_INITIAL_DREAMS, "DKEYS_DreamFireChain", saver, bossesSaver, infoTitle, info, parent),
                new DungeonKeysInfo("DreamAll", X_INITIAL + 3 * X_SEPARATION, Y_INITIAL_DREAMS, "DKEYS_DreamAll", saver, bossesSaver, infoTitle, info, parent),
            };
            foreach(DungeonKeysInfo key in keysInfo)
            {
                key.UpdateButton();
            }

            _cardCount = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 4f, output.BackButton.transform.localPosition.y + 1.5f, parent, "Extra");
            GuiSelectionObject hoverSelection = _cardCount.gameObject.GetComponentInChildren<GuiSelectionObject>();
            bool oldState = false;
            _cardCount.onUpdate += delegate ()
            {
                if (hoverSelection.IsSelected == oldState) { return; }
                oldState = hoverSelection.IsSelected;
                if (oldState)
                {
                    string extraText = "";
                    int total = ModSaver.LoadIntFromFile("mod", "cardcount");
                    IDataSaver cardsSaver = ModSaver.GetSaver("cards");
                    string[] cardsInSave = cardsSaver.GetAllDataKeys();
                    extraText += "\n";
                    extraText += "Config: " + ModSaver.LoadStringFromFile("mod", "item_randomizer_file") + "\n";
                    extraText += "Seed: " + ModSaver.LoadStringFromFile("mod", "randomizer_seed") + "\n";
                    extraText += "\n";
                    extraText += string.Format("Cards: {0}/{1}\n", cardsInSave.Length, total);
                    info.WriteText(extraText);
                    infoTitle.UIName = "Extra";
                }
            };
            return output;
        }

        class DungeonKeysInfo
        {
            string _scene;
            string _bossScene;
            UIGFXButton _button;
            UITextFrame _title;
            UIBigFrame _info;
            IDataSaver _saver;
            IDataSaver _bossesSaver;
            int _amount;
            static DungeonKeysInfo _currentInfo;

            public DungeonKeysInfo(string keysScene, float x, float y, string image, IDataSaver saver, IDataSaver bossesSaver, UITextFrame title, UIBigFrame info, Transform parent, string bossScene = null)
            {
                _scene = keysScene;
                _amount = ModItemHandler.GetKeyAmountInScene(_scene);
                _saver = saver;
                _bossesSaver = bossesSaver;
                _title = title;
                _info = info;
                _bossScene = bossScene == null ? _scene : bossScene; 
                
                _button = UIFactory.Instance.CreateGFXButton(image, x, y, parent, "");
                _button.ScaleBackground(Vector2.one * 0.50f);
                
                GuiSelectionObject hoverSelection = _button.gameObject.GetComponentInChildren<GuiSelectionObject>();
                bool oldState = false;
                _button.onUpdate += delegate ()
                {
                    if (hoverSelection.IsSelected == oldState) { return; }
                    oldState = hoverSelection.IsSelected;
                    if (oldState)
                    {
                        _currentInfo = this;
                        SetInfo();
                    }
                };
                UpdateButton();
            }

            public static void ClearCurrentSetInfo()
            {
                _currentInfo = null;
            }

            public static void ForceUpdate()
            {
                if(_currentInfo != null) _currentInfo.SetInfo();
            }

            void SetInfo()
            {
                List<string> output = new List<string>();

                //Key tags
                int genericKeysObtained = _saver.LoadInt(_scene + "_localKeys");
                if (genericKeysObtained > 0)
                {
                    int currentKeys = 0;
                    if (SceneManager.GetActiveScene().name == _scene)
                    {
                        currentKeys = ModSaver.LoadFromEnt("localKeys");
                    }
                    else
                    {
                        currentKeys = ModSaver.LoadIntFromFile("levels/" + _scene + "/player/vars/", "localKeys");
                    }
                    output.Add($"{genericKeysObtained} key{(genericKeysObtained > 1 ? "s" : "")} obtained ({genericKeysObtained - currentKeys} already used)");
                }

                //Unlocked doors
                IDataSaver local = _saver.GetLocalSaver(_scene);
                string[] keys = local.GetAllDataKeys();
                int keyAmount = keys.Length;
                for (int i = 0; i < keyAmount; i++)
                {
                    keys[i] = ModItemHandler.GetNickName(keys[i]);
                }
                Array.Sort(keys);
                for (int i = 0; i < keyAmount; i++)
                {
                    output.Add(keys[i]);
                }

                //Boss defeated
                if (_bossesSaver.LoadBool(_bossScene))
                {
                    output.Add("---- Boss defeated ----");
                }
                int keysToShow = keyAmount + genericKeysObtained;
                _title.UIName = string.Format("{0} ({1}/{2})", SceneName.GetName(_scene), keysToShow, _amount);
                _info.WriteText("\n" + string.Join("\n", output.ToArray()));
            }

            public void UpdateButton()
            {
                IDataSaver local = _saver.GetLocalSaver(_scene);
                string[] keys = local.GetAllDataKeys();
                int genericKeyAmount = _saver.LoadInt(_scene + "_localKeys");
                _button.gameObject.SetActive(keys.Length > 0 || _bossesSaver.LoadBool(_scene) || genericKeyAmount > 0);
            }
        }
        #endregion
    }
}