using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ModStuff.ItemRandomizer;

namespace ModStuff
{
	public static class ModeControllerNew
	{
		public enum ModeType
		{
			HeartRush,
			BossRush,
			DungeonRush,
            DoorRandomizer,
            ItemRandomizer
			//Expert,
			//Chaos
		}

        static List<E2DGameMode> allModes;
        public static List<E2DGameMode> AllModes
        {
            get
            {
                if (allModes == null)
                {
                    allModes = new List<E2DGameMode>();
                    allModes.Add(HeartRush.Instance);
                    allModes.Add(BossRush.Instance);
                    allModes.Add(DungeonRush.Instance);
                    AllModes.Add(DoorRandomizerGM.Instance);
                    AllModes.Add(ItemRandomizerGM.Instance);
                }
                return allModes;
            }
        }

		public static List<ModeType> activeModes = new List<ModeType>();

		// Activates the given mode by mode type
		public static void ActivateMode(E2DGameMode mode, RealDataSaver saver)
		{
			ModSaverNew.SaveToNewSaveFile("mod/modes/active/" + mode.Mode.ToString(), "1", saver);
            mode.SetupSaveFile(saver);

			if (!activeModes.Contains(mode.Mode))
			{
				// Subscribe to events
				GameStateNew.OnSceneLoad += OnSceneLoad;

				// Add mode to activeModes list
				activeModes.Add(mode.Mode);
				return;
			}

            //For now, profile names wont deactivate modes, since it would overwrite UI settings

			// If vanilla file loaded, make it vanilla
			//DeactivateModes();
		}

		// Activates the given mode(s) by file name
		public static void ActivateModesByFileName(string fileName, RealDataSaver saver)
		{
            char[] separators = new char[] { ' ', '/', '&' };
			string[] modeNames = fileName.ToLower().Split(separators);
			List<E2DGameMode> modesToActivate = new List<E2DGameMode>();

			// Gets mode(s) to activate from file name
            for (int i = 0; i < modeNames.Length; i++)
            {
                E2DGameMode gameMode = GetMode(modeNames[i]);
                if(gameMode != null && !modesToActivate.Contains(gameMode)) modesToActivate.Add(gameMode);
            }

            // Gets mode(s) to activate from UI
            for (int i = 0; i < readyModes.Count; i++)
            {
                E2DGameMode gameMode = GetMode(readyModes[i]);
                if (gameMode != null && !modesToActivate.Contains(gameMode)) modesToActivate.Add(GetMode(readyModes[i]));
            }

            // Activates each mode
            if (CanActivateMode(modesToActivate))
            {
                for (int i = 0; i < modesToActivate.Count; i++)
                {
                    ActivateMode(modesToActivate[i], saver);
                }
            }
		}

		// Activates the given mode by index
		public static void ActivateMode(int index, RealDataSaver saver)
		{
            if (index > 0 && index < AllModes.Count)
            {
                ActivateMode(AllModes[index], saver);
                return;
            }

			// If vanilla
			//DeactivateModes();
		}

		// Activates the given mode via file load
		public static void ResumeMode(string mode)
		{
			for (int i = 0; i < AllModes.Count; i++)
			{
				if (AllModes[i].Mode.ToString() == mode)
				{
					AllModes[i].Activate();
					activeModes.Add(AllModes[i].Mode);
				}
			}
		}

		// Deactivates all modes to return to vanilla game
		public static void DeactivateModes()
		{
			for (int i = 0; i < activeModes.Count; i++)
			{
                AllModes[(int)activeModes[i]].Deactivate();
			}
            activeModes.Clear();
        }

		// Deactivates specified mode
		static void DeactivateMode(ModeType mode)
		{
			for (int i = 0; i < activeModes.Count; i++)
			{
				if (activeModes[i] == mode)
				{
                    AllModes[(int)activeModes[i]].Deactivate();
                    activeModes.Remove(mode);
                    break;
				}
			}
		}

		// Returns the mode from name
		static E2DGameMode GetMode(string name)
		{
			for (int i = 0; i < AllModes.Count; i++)
			{
                if (AllModes[i].IsMode(name)) { return AllModes[i]; }
            }

			// If vanilla
			return null;
		}

        // Returns the mode from modetype
        static E2DGameMode GetMode(ModeType mode)
        {
            for (int i = 0; i < AllModes.Count; i++)
            {
                if (AllModes[i].Mode == mode) { return AllModes[i]; }
            }

            return null;
        }

        // Returns the mode from index
        static E2DGameMode GetMode(int index)
		{
            if(index > 0 && index < AllModes.Count)	return AllModes[index];

			// If vanilla
			return null;
		}

		// Returns true if the mode doesn't conflict with other active modes. False otherwise
		static bool CanActivateMode(List<E2DGameMode> modesToAllow)
		{
            //Analyze each modeToAllow
			for (int i = 0; i < modesToAllow.Count; i++)
			{
                //For each modeToAllow AFTER the one being analyzed...
                for (int j = i + 1; j < modesToAllow.Count; j++)
			    {
                    if (!modesToAllow[i].CheckCompatibility(modesToAllow[j])) return false;
			    }
			}

			return true;
		}

		// Deactivates all modes when loading to main menu
		static void OnSceneLoad(Scene scene, bool isGameplayScene)
		{
			if (scene.buildIndex < 2)
			{
				// Unsubscribe to events
				DeactivateModes();
				GameStateNew.OnSceneLoad -= OnSceneLoad;
			}
		}

		// Is vanilla?
		public static bool IsVanilla
		{
			get { return activeModes.Count < 1; }
		}

		// Is Heart Rush active?
		public static bool IsHeartRush
		{
			get { return activeModes.Contains(ModeType.HeartRush); }
		}

		// Is Boss Rush active?
		public static bool IsBossRush
		{
			get { return activeModes.Contains(ModeType.BossRush); }
		}

		// Is Dungeon Rush active?
		public static bool IsDungeonRush
		{
			get { return activeModes.Contains(ModeType.DungeonRush); }
		}

		// Is Expert active?
		public static bool IsExpert
		{
			get { return false; /*activeModes.Contains(ModeType.Expert);*/ }
		}

		// Is Chaos active?
		public static bool IsChaos
		{
			get { return false;/* activeModes.Contains(ModeType.Chaos); */}
		}

        //UI Section

        //If a mode is present here, activate it at the start of the game
        static List<ModeType> readyModes = new List<ModeType>();

        //Check if a mode is ready
        public static bool IsModeReady(ModeType mode)
        {
            return readyModes.Contains(mode);
        }

        //Adds and removes modes from the ready list
        static void ChangeReady(ModeType mode, bool add)
        {
            if(add)
            {
                if (!readyModes.Contains(mode)) readyModes.Add(mode);
            }
            else
            {
                if (readyModes.Contains(mode)) readyModes.Remove(mode);
            }
        }

        //Update 2D List
        static void UpdateGameModeList(UI2DList list)
        {
            List<ModeType> modesList = new List<ModeType>();

            for (int i = 0; i < AllModes.Count; i++) modesList.Add(AllModes[i].Mode);

            for (int i = 0; i < readyModes.Count; i++)
            {
                E2DGameMode e2dGameMode = GetMode(readyModes[i]);
                for (int j = 0; j < e2dGameMode.Restrictions.Count; j++)
                {
                    if (modesList.Contains(e2dGameMode.Restrictions[j])) modesList.Remove(e2dGameMode.Restrictions[j]);
                }
            }

            List<string> listFor2DList = new List<string>();

            for (int i = 0; i < modesList.Count; i++) listFor2DList.Add(GetMode(modesList[i]).Title);
            list.ExplorerArray = listFor2DList.ToArray();
        }

        //Active game mode in the UI
        static E2DGameMode activeMenuMode;

        public static UIScreen CreateMenu()
        {
            UIScreen output = UIScreen.CreateBaseScreen("Modes");

            //Ready List
            readyModes = new List<ModeType>();

            //Get new random seed
            RNGSeed.ReRollSeed();

            //GameModes List
            UI2DList modesList = UIFactory.Instance.Create2DList(-5f, 2f, new Vector2(1f, 5f), Vector2.one, new Vector2(1.3f, 0.75f), output.transform, "Game Modes");
            modesList.ScrollBar.ResizeLength(6);
            modesList.Title.ScaleBackground(new Vector2(0.8f, 1f));
            modesList.ScrollBar.transform.localPosition += new Vector3(-1.2f, 0f, 0f);
            modesList.HighlightSelection = false;

            //Reset to vanilla
            UIButton reset = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -5f, -3f, output.transform, "Reset to\nvanilla");
            reset.ScaleBackground(new Vector2(0.7f, 1.3f));

            //Holder
            GameObject controlsHolder = new GameObject("GearSelection");
            controlsHolder.transform.SetParent(output.transform, false);
            controlsHolder.transform.localScale = Vector3.one;
            controlsHolder.transform.localPosition = new Vector3(5f - 2f, 0f, 0f);

            //Background
            UIBigFrame background = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1f, controlsHolder.transform, "");
            background.transform.localPosition += new Vector3(0f, 0f, 0.5f);
            background.ScaleBackground(new Vector2(0.45f, 1.5f));

            //Activate checkbox
            UICheckBox enableTitle = UIFactory.Instance.CreateCheckBox(-1.5f, 5f, controlsHolder.transform);
            enableTitle.ScaleBackground(1.0f, Vector3.one * 1.3f);

            //Info button
            //string randoDes = 
            UIButton gameModeInfo = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 2.5f, 5f, controlsHolder.transform, "Info");
            gameModeInfo.ScaleBackground(new Vector2(0.35f, 1f), Vector2.one * 1.2f);
            UITextFrame gameInfoHelp = UIFactory.Instance.CreatePopupFrame(0f, 1.5f, gameModeInfo, output.transform, "");
            gameInfoHelp.ScaleBackground(new Vector2(3f, 4f));
            gameInfoHelp.ScaleText(0.75f);
            gameInfoHelp.transform.localPosition += new Vector3(0f, 0f, -0.4f);

            //Help button
            UIButton help = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 7f, -3f, output.transform, "?");
            help.ScaleBackground(new Vector2(0.2f, 1f));
            UITextFrame helpHelp = UIFactory.Instance.CreatePopupFrame(0f, 1.5f, help, output.transform, "");
            helpHelp.ScaleBackground(new Vector2(2f, 4.2f));
            helpHelp.ScaleText(0.8f);
            helpHelp.WriteText("Select the mode you want on the left and then click the top checkbox to enable it. Enabled gamemodes will be activated when starting a new game.\n\nGame modes might get removed from the list to avoid conflicts with active ones.");
            helpHelp.transform.localPosition += new Vector3(0f, 0f, -0.4f);

            //Create options and initial 2D List
            GameObject optionsHolder = new GameObject("OptionsHolder");
            optionsHolder.transform.SetParent(controlsHolder.transform, false);
            optionsHolder.transform.localScale = Vector3.one;
            optionsHolder.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            List<string> modesfor2DList = new List<string>();
            for (int i = 0; i < AllModes.Count; i++)
            {
                AllModes[i].CreateOptions();
                AllModes[i].MenuGo.transform.SetParent(optionsHolder.transform, false);
                AllModes[i].MenuGo.SetActive(i == 0);
                modesfor2DList.Add(AllModes[i].Title);
            }
            modesList.ExplorerArray = modesfor2DList.ToArray();

            //2D List function
            modesList.onInteraction += delegate (string textValue, int arrayIndex)
            {
                for (int i = 0; i < AllModes.Count; i++)
                {
                    if(AllModes[i].Title == textValue && activeMenuMode != AllModes[i])
                    {
                        //Change title
                        enableTitle.UIName = AllModes[i].Title;
                        enableTitle.Value = IsModeReady(AllModes[i].Mode);

                        //Change descriptions
                        background.WriteText(AllModes[i].QuickDescription);
                        gameInfoHelp.WriteText(AllModes[i].Description);

                        //Change options
                        if(activeMenuMode != null) activeMenuMode.MenuGo.SetActive(false);
                        activeMenuMode = AllModes[i];
                        activeMenuMode.MenuGo.SetActive(true);
                        activeMenuMode.OnOpenMenu();
                    }
                }
            };
            activeMenuMode = null;
            modesList.Trigger(0);

            //CheckBox function
            enableTitle.onInteraction += delegate (bool checkBox)
            {
                ChangeReady(activeMenuMode.Mode, checkBox);
                if (activeMenuMode.Restrictions.Count > 0) UpdateGameModeList(modesList);
            };

            //Reset button function
            reset.onInteraction += delegate ()
            {
                readyModes.Clear();
                UpdateGameModeList(modesList);
                enableTitle.Value = false;
            };

            return output;
        }
    }
}