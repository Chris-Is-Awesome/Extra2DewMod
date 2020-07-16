using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace ModStuff
{
	public static class UIScreenLibrary
	{
		//Build functions delegate
		public delegate UIScreen UIScreenHandler();

		//UIScreens list
		public static Dictionary<string, UIScreenHandler> GetLibrary()
		{
			Dictionary<string, UIScreenHandler> output = new Dictionary<string, UIScreenHandler>();
			if (SceneManager.GetActiveScene().name == "MainMenu")
			{
				//Main menu dictionary
				output.Add("newgamemodes", new UIScreenHandler(NewGameModes));
				output.Add("newgamemodeselect", new UIScreenHandler(NewGameModeSelect));
			}
			else
			{
				//Pause menu dictionary
				output.Add("mainpause", new UIScreenHandler(MainPause));
				output.Add("spawnmenu", new UIScreenHandler(SpawnMenu));
				output.Add("cameramenu", new UIScreenHandler(CameraMenu));
				output.Add("powerupsmenu", new UIScreenHandler(PowerupsMenu));
				output.Add("itemsmenu", new UIScreenHandler(ItemsMenu));
				output.Add("worldmenu", new UIScreenHandler(WorldMenu));
				output.Add("infomenu", new UIScreenHandler(InfoMenu));
				output.Add("gameoptions", new UIScreenHandler(GameOptions)); 
                output.Add("testchambermenu", new UIScreenHandler(TestChamberMenu));
                output.Add("scriptsmenu", new UIScreenHandler(ScriptsMenu));
                //output.Add("modoptions", new UIScreenHandler(ModSettings));
            }
			return output;
		}

		static float ColDistance { get { return 3.75f; } }
		static float AspectRatio { get { return (float)Screen.width / (float)Screen.height; } }

		static float FirstCol { get { return -AspectRatio * ColDistance; } }
		static float MidCol { get { return 0f; } }
		static float LastCol { get { return AspectRatio * ColDistance; } }

		static UIScreen NewGameModeSelect()
		{
			UIScreen output = UIScreen.CreateBaseScreen("Modes");
			output.name = "modeSelect";

            //Save scrollbar
            UIFactory.Instance.SaveScrollBar();

			//Accomodate back button and place confirm button
			float backConfirmDistance = 3f;
			Vector3 backBtnPos = output.BackButton.transform.localPosition;
			output.BackButton.transform.localPosition += new Vector3(-backConfirmDistance, 0f, 0f);
			UIButton confirmbutton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, backConfirmDistance, backBtnPos.y, output.transform);
			output.SaveElement("confirm", confirmbutton);

			//Dummy checkbox (so randomize is not selected at the start
			UICheckBox dumy = UIFactory.Instance.CreateCheckBox(MidCol - 3f, 15f, output.transform, "Randomize doors");

			//Randomize checkbox
			UICheckBox randomizer = UIFactory.Instance.CreateCheckBox(MidCol - 3f, 5f, output.transform, "Randomize doors");
			randomizer.ScaleBackground(1.6f, Vector3.one);
			output.SaveElement("randomizer", randomizer);

			//Randomize help message
			UIBigFrame randoMessage = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 2, -3.5f, randomizer.transform);
			randoMessage.transform.localPosition += new Vector3(0f, 0f, -1f);
			randoMessage.ScaleBackground(new Vector2(1.7f, 1.65f));
			string randoText = "EXPERT PLAYERS ONLY\n\nRandomize all scene transitions while still making the game beatable. Randomization affects caves, dungeons, overworld connections and some secret places. Hidden entrances to caves will be randomized, such as the grass patch in fancy ruins and jenny berry's home.\n\nConnections left out the randomization: The grand library, all mechanic dungeons, the dream world, the secret remedy and any one way connection. The cheat sheet can be found in the 'extra2dew\\randomizer' folder.";
			randoMessage.UIName = ModText.WrapText(randoText, 30f);
			randoMessage.gameObject.SetActive(false);
			GuiSelectionObject buttonListSelection = randomizer.gameObject.transform.Find("ModUIElement").GetComponent<GuiSelectionObject>();
			bool oldNpcListstate = false;
			randomizer.onUpdate += delegate ()
			{
				if (buttonListSelection.IsSelected == oldNpcListstate) { return; }
				oldNpcListstate = buttonListSelection.IsSelected;
				randoMessage.gameObject.SetActive(oldNpcListstate);
			};

			//Seed
			UITextFrame seed = UIFactory.Instance.CreateTextFrame(MidCol + 3f, 5f, output.transform);
			seed.UIName = DoorsRandomizer.Instance.GetRandomSeed();
			output.SaveElement("seed", seed);

			//Reroll seed
			UIButton rerollSeed = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, MidCol + 1.5f, 4f, output.transform, "New seed");
			rerollSeed.ScaleBackground(new Vector2(0.8f, 1f), Vector2.one * 0.7f);
			rerollSeed.onInteraction += delegate ()
			{
				seed.UIName = DoorsRandomizer.Instance.GetRandomSeed();
			};

			//Paste seed
			UIButton pasteSeed = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, MidCol + 4.5f, 4f, output.transform, "Paste seed");
			pasteSeed.ScaleBackground(new Vector2(0.8f, 1f), Vector2.one * 0.7f);
			pasteSeed.onInteraction += delegate ()
			{
				seed.UIName = DoorsRandomizer.Instance.FixSeed(GUIUtility.systemCopyBuffer);
			};

			//Info box
			UIBigFrame info = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, MidCol - 2.5f, -0.5f, output.transform);
			info.ScaleBackground(new Vector2(1f, 1.4f));

			//Heart rush difficulty
			UIListExplorer hsDifficulty = UIFactory.Instance.CreateListExplorer(MidCol + 4f, -0.5f, output.transform, "Difficulty");
			hsDifficulty.Title.ScaleBackground(new Vector2(0.8f, 0.8f));
			hsDifficulty.ExplorerArray = new string[] { "I cannot see", "Very easy", "Easy", "Default", "Hard", "Very Hard", "ReallyJoel's Dad" };
			hsDifficulty.IndexValue = 3;
			hsDifficulty.gameObject.SetActive(false);
			output.SaveElement("hsdifficulty", hsDifficulty);

			//Mode selector
			UIListExplorer modeSelector = UIFactory.Instance.CreateListExplorer(MidCol, 2.5f, output.transform, "Game Mode");
			modeSelector.Title.ScaleBackground(new Vector2(0.8f, 0.8f));
			modeSelector.AllowLooping = true;
			modeSelector.ExplorerArray = new string[] { "Default", "Heart Rush", "Boss Rush", "Dungeon Rush" };
			modeSelector.onInteraction += delegate (bool arrow, string word, int index)
			{
				string infoText;
				bool showDifficulty = false;
				switch (index)
				{
					case 0:
						infoText = "Vanilla game";
						break;
					case 1:
						infoText = "Instead of damage, you lose max HP. If you run out of HP, the game is over and the profile is deleted.\n\nStarts with a large health pool and crayon boxes give additional heart pieces instead of 1. Can be combined with the randomizer.";
						showDifficulty = true;
						break;
					case 2:
						infoText = "Face a gauntlet of all the bosses in the game.\n\nYour items loadout will be improved after each defeated boss.";
						break;
					case 3:
						infoText = "Take a tour through all the dungeons in the game and finish them as quickly as possible.\n\nAfter defeating a dungeon, you will be teleported to the next one.";
						break;
					default:
						infoText = "";
						break;
				}
				hsDifficulty.gameObject.SetActive(showDifficulty);
				info.UIName = ModText.WrapText(infoText, 16.5f);
			};
			modeSelector.Trigger();
			output.SaveElement("modeselector", modeSelector);

			return output;
		}

        //======================
        //Screen builder functions - PAUSE MENU
        //======================
        //Test Chamber Menu
        static UIScreen TestChamberMenu()
        {
            UIScreen output = UIScreen.CreateBaseScreen("Test Chamber");
            
            return output;
        }

        //Scripts Menu
        static UIScreen ScriptsMenu()
        {
            UIScreen output = UIScreen.CreateBaseScreen("Scripts");

            //Display
            UITextFrame selectedScript = UIFactory.Instance.CreateTextFrame(2f, 4f, output.transform);
            selectedScript.ScaleBackground(Vector2.one * 1.2f);

            //Script List
            UI2DList scriptList = UIFactory.Instance.Create2DList(FirstCol, 2f, new Vector2(1f, 5f), Vector2.one, new Vector2(1.3f, 0.75f), output.transform, "Scripts");
            scriptList.ScrollBar.ResizeLength(6);
            scriptList.ScrollBar.transform.localPosition += new Vector3(-1.2f, 0f, 0f);
            scriptList.onInteraction += delegate (string textValue, int arrayIndex)
            {
                selectedScript.UIName = textValue;
            };

            //Update List
            UIButton updateList = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, FirstCol, -3f, output.transform, "Update List");
            updateList.onInteraction += delegate ()
            {
                scriptList.ExplorerArray = ModScriptHandler.Instance.GetScriptList(); //Set list
                selectedScript.UIName = (scriptList.ExplorerArray.Length > 0) ? scriptList.ExplorerArray[0] : "No script found"; //Set initial value for the display
            };
            updateList.Trigger();

            //Run button
            UIButton runScript = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 5.5f, 4f, output.transform, "Run");
            runScript.ScaleBackground(new Vector2(0.4f, 1f), Vector2.one * 1.2f);
            runScript.onInteraction += delegate () { ModScriptHandler.Instance.ParseTxt(selectedScript.UIName, out string errors); };

            //Set as onload
            UIButton saveOnLoad = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 1.2f, 2.8f, output.transform, "Run OnLoad");
            saveOnLoad.ScaleBackground(new Vector2(0.8f, 1f), Vector2.one * 0.8f);
            saveOnLoad.onInteraction += delegate () { ModScriptHandler.Instance.OnNewSceneTxt = selectedScript.UIName; };
            UITextFrame onLoadHelp = UIFactory.Instance.CreatePopupFrame(1.2f, 1.8f, saveOnLoad, output.transform, "Each time a new scene is\n loaded, run this script");
            onLoadHelp.transform.localPosition += new Vector3(0f, 0f, -0.4f);

            //Clear onload
            UIButton clearOnLoad = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 4.7f, 2.8f, output.transform, "Clear OnLoad");
            clearOnLoad.ScaleBackground(new Vector2(0.8f, 1f), Vector2.one * 0.8f);
            clearOnLoad.onInteraction += delegate () { ModScriptHandler.Instance.OnNewSceneTxt = ""; };

            //Console
            UIBigFrame loadConfigConsole = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 3f, 0f, output.transform);
            loadConfigConsole.ScaleBackground(new Vector2(0.5f, 1.2f));
            loadConfigConsole.name = "LoadConfigConsole";

            //Help button
            UIButton help = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 7f, -3f, output.transform, "?");
            help.ScaleBackground(new Vector2(0.2f, 1f), Vector2.one);
            string helpText = ModText.WrapText("Run scripts located in ID2Data/extra2dew/scripts. Scripts are groups of debug menu commands ran in sequence, complex effects can be achieved by them.\n\nTo run a script, select it on the list and press the 'Run' button. Pressing 'Run OnLoad' will mark one script to be run each time a new scene is loaded.", 25f, false);
            UITextFrame helpHelp = UIFactory.Instance.CreatePopupFrame(0f, 1.5f, help, output.transform, helpText);
            helpHelp.ScaleBackground(new Vector2(2f, 4.2f));
            helpHelp.transform.localPosition += new Vector3(0f, 0f, -0.4f);

            return output;
        }

        //Old scrollmenu function, archive it somewhere
        static UIScreen ScrollFunctionTest()
        {
            UIScreen output = UIScreen.CreateBaseScreen("Test Chamber");

            //Create and configure first menu
            UIScrollMenu firstmenu = UIFactory.Instance.CreateScrollMenu(0f, 3f, output.transform);
            firstmenu.ScrollBar.ResizeLength(9);
            firstmenu.CanvasWindow = 9.5f;
            firstmenu.EmptySpace = 8f;
            firstmenu.ScrollBar.transform.localPosition += new Vector3(4f, 0f, 0f);
            firstmenu.ShowName(false);

            //UIelements for first menu
            UITextFrame text1 = UIFactory.Instance.CreateTextFrame(0f, -5f, output.transform, "Scrollable menus!");
            firstmenu.Assign(text1);
            UITextFrame text2 = UIFactory.Instance.CreateTextFrame(0f, -15f, output.transform, "Look at how much\nspace they have!");
            firstmenu.Assign(text2);
            text2.ScaleBackground(new Vector2(1.8f, 1.5f));
            UICheckBox text3 = UIFactory.Instance.CreateCheckBox(-3f, -25f, output.transform, "And they can");
            text3.ScaleBackground(1.8f, Vector3.one);
            firstmenu.Assign(text3);
            UIListExplorer text4 = UIFactory.Instance.CreateListExplorer(3f, -25f, output.transform, "hold");
            text4.ExplorerArray = new string[] { "anything!", "ANYTHING!" };
            firstmenu.Assign(text4);

            //Create and configure second menu. Assign it to the last one
            UIScrollMenu secondmenu = UIFactory.Instance.CreateScrollMenu(0, -35f, output.transform);
            secondmenu.ScrollBar.ResizeLength(8);
            secondmenu.CanvasWindow = 8.5f;
            secondmenu.ScrollBar.transform.localPosition += new Vector3(3f, 0f, 0f);
            secondmenu.EmptySpace = 8f;
            secondmenu.ShowName(false);
            firstmenu.Assign(secondmenu, -1f, -1f);

            //UIelements for second menu
            UITextFrame text5 = UIFactory.Instance.CreateTextFrame(0f, -5f, output.transform, "Even other menus!");
            secondmenu.Assign(text5);

            //Create and configure third menu. Assign it to the last one
            UIScrollMenu thirdmenu = UIFactory.Instance.CreateScrollMenu(0, -15f, output.transform);
            thirdmenu.ScrollBar.ResizeLength(7);
            thirdmenu.CanvasWindow = 7.5f;
            thirdmenu.ScrollBar.transform.localPosition += new Vector3(2f, 0f, 0f);
            thirdmenu.EmptySpace = 5f;
            thirdmenu.ShowName(false);
            secondmenu.Assign(thirdmenu, -1f, -1f);

            //Create and configure fourth menu. Assign it to the last one
            UIScrollMenu fourthmenu = UIFactory.Instance.CreateScrollMenu(0, -15f, output.transform);
            fourthmenu.ScrollBar.ResizeLength(6);
            fourthmenu.CanvasWindow = 6.5f;
            fourthmenu.ScrollBar.transform.localPosition += new Vector3(1f, 0f, 0f);
            fourthmenu.ShowName(false);
            fourthmenu.EmptySpace = 16f;
            thirdmenu.Assign(fourthmenu, -1f, -1f);

            //Image
            UIImage yodawg = UIFactory.Instance.CreateImage("yodawg", 0f, -20f, output.transform);
            Vector3 imgSize = new Vector3(3.91f, 4.01f, 1f);
            yodawg.transform.localScale = imgSize.normalized * 4f;
            yodawg.transform.localPosition += new Vector3(0f, 0f, -0.5f);
            fourthmenu.Assign(yodawg, 0f, -9f);
            return output;
        }

        //Main Pause menu
        static UIScreen MainPause()
		{
			// Create a base window, which contains a title and a back button. Name it (or not and leave it with default name)
			UIScreen output = UIScreen.CreateBaseScreen("Extra 2 Dew");
			output.Title.transform.localScale = Vector3.one * 1.3f;
			output.Title.transform.localPosition += new Vector3(0f, -1f, 0f);

			UIBigFrame background = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1f, output.transform);
			background.name = "MainMenuBackground";
			background.ScaleBackground(new Vector2(1f, 1.8f));
			background.transform.localPosition += new Vector3(0f, 0f, 2f); //Send to the background

			UIGFXButton powerupMenu = UIFactory.Instance.CreateGFXButton("powerup", -6f, 3f, output.transform, "Powerups");
			output.SaveElement("powerup", powerupMenu);

			UIGFXButton itemsMenu = UIFactory.Instance.CreateGFXButton("itemsmenu", -3f, 3f, output.transform, "Items");
			output.SaveElement("items", itemsMenu);

			UIGFXButton spawnMenu = UIFactory.Instance.CreateGFXButton("npcs", 0f, 3f, output.transform, "Spawn NPCs");
			output.SaveElement("spawn", spawnMenu);

			UIGFXButton cameraMenu = UIFactory.Instance.CreateGFXButton("camera", 3f, 3f, output.transform, "Camera");
			output.SaveElement("camera", cameraMenu);

			UIGFXButton worldMenu = UIFactory.Instance.CreateGFXButton("world", 6f, 3f, output.transform, "World");
			worldMenu.ScaleBackground(new Vector2(1.2f, 1.2f));
			output.SaveElement("world", worldMenu);

            UIGFXButton scripstMenu = UIFactory.Instance.CreateGFXButton("scripts", -6f, 0f, output.transform, "Scripts");
            output.SaveElement("scripts", scripstMenu);

            UIGFXButton infoMenu = UIFactory.Instance.CreateGFXButton("info", 6f, 0f, output.transform, "Info");
			output.SaveElement("info", infoMenu);

			UIGFXButton gameOptionsMenu = UIFactory.Instance.CreateGFXButton("gameoptions", 3f, 0f, output.transform, "Game Options");
			output.SaveElement("gameoptions", gameOptionsMenu);

            /*
			UIGFXButton optionsMenu = UIFactory.Instance.CreateGFXButton("options", 3f, 0f, output.transform, "Options");
			output.SaveElement("options", optionsMenu);
			*/

            return output;
		}

		// Mod settings

		static UIScreen GameOptions()
		{
			UIScreen output = UIScreen.CreateBaseScreen("Game Options");
			output.Title.transform.localScale = Vector3.one * 1.3f;
			output.Title.transform.localPosition += new Vector3(0f, -1f, 0f);

			UIBigFrame background = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1f, output.transform);
			background.name = "MainMenuBackground";
			background.ScaleBackground(new Vector2(1f, 1.8f));
			background.transform.localPosition += new Vector3(0f, 0f, 2f); // Send to the background

			// Button position presets
			float xPosLeftColumn = -10.25f; float xPosRightColumn = -3f;
			float yPosRow1 = 3.5f; float yPosRow2 = 2.25f; float yPosRow3 = 1f; float yPosRow4 = -0.25f;
			float yPosRow5 = -1.5f; float yPosRow6 = -2.5f; float yPosRow7 = -3.75f; float yPosRow8 = -5f;
			float zPos = -0.2f;
			float scale = 1.75f;

			// Popup overlay presets
			string popupText;
			float popupXPos = 0f; float popupYPos = -1.5f;
			Vector2 popupScale = new Vector2(3f, 1.2f);

			// Save options to this file
			string filePath = @"C:\Users\Awesome\Desktop\" + "options.txt";

			// Add UI functionality below...

			// Disable intro & outro cutscenes?
			UICheckBox disableIntroAndOutro = UIFactory.Instance.CreateCheckBox(LastCol, 0f, output.transform, "No intro/outro");
			disableIntroAndOutro.transform.localPosition += new Vector3(xPosLeftColumn, yPosRow1, zPos);
			disableIntroAndOutro.ScaleBackground(1.75f, Vector3.one);
			disableIntroAndOutro.onInteraction += delegate (bool enable)
			{
				GameOptions opts = ModStuff.GameOptions.Instance;
				opts.DisableIntroAndOutro = enable;
				ModSaverNew.SaveToCustomFile<int>(filePath, nameof(opts.DisableIntroAndOutro), Convert.ToInt32(enable));
			};
			output.SaveElement("disableIntroAndOutro", disableIntroAndOutro);
			popupText = "Skip the intro when starting a new file.\nSkip the outro when you beat the game.";
			UITextFrame popupIntroAndOutro = UIFactory.Instance.CreatePopupFrame(popupXPos, popupYPos, disableIntroAndOutro, output.transform, popupText);
			popupIntroAndOutro.ScaleBackground(popupScale);

			// Faster transitions?
			UICheckBox fasterTransitions = UIFactory.Instance.CreateCheckBox(LastCol, 0f, output.transform, "Faster transitions");
			fasterTransitions.transform.localPosition += new Vector3(xPosRightColumn, yPosRow1, zPos);
			fasterTransitions.ScaleBackground(1.75f, Vector3.one);
			fasterTransitions.onInteraction += delegate (bool enable)
			{
				GameOptions opts = ModStuff.GameOptions.Instance;
				opts.FasterTransitions = enable;
				ModSaverNew.SaveToCustomFile<int>(filePath, nameof(opts.FasterTransitions), Convert.ToInt32(enable));
			};
			output.SaveElement("fasterTransitions", fasterTransitions);
			popupText = "Speed up level and room transitions.";
			UITextFrame popupFasterTransitions = UIFactory.Instance.CreatePopupFrame(popupXPos, popupYPos, fasterTransitions, output.transform, popupText);
			popupFasterTransitions.ScaleBackground(popupScale);

			// Add UI functionality above...

			return output;
		}

		/*

		// Mod settings
		static UIScreen ModSettings()
		{
			UIScreen output = UIScreen.CreateBaseScreen("Mod Options");
			output.Title.transform.localScale = Vector3.one * 1.3f;
			output.Title.transform.localPosition += new Vector3(0f, -1f, 0f);

			UIBigFrame background = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1f, output.transform);
			background.name = "MainMenuBackground";
			background.ScaleBackground(new Vector2(1f, 1.8f));
			background.transform.localPosition += new Vector3(0f, 0f, 2f); // Send to the background

			float xPosLeftColumn = -10.25f;
			float xPosRightColumn = -3f;
			float yPosRow1 = 3.5f;
			float yPosRow2 = 2.25f;
			float yPosRow3 = 1f;
			float yPosRow4 = -0.25f;
			float yPosRow5 = -1.5f;
			float zPos = -0.2f;

			float scale = 1.75f;

			string popupText;
			float popupXPos = 0f;
			float popupYPos = -1.5f;
			Vector2 popupScale = new Vector2(3f, 1.2f);

			// Disable intro and outro cutscenes?
			UICheckBox disableIntroAndOutro = UIFactory.Instance.CreateCheckBox(LastCol, 0f, output.transform, "No intro/outro");
			disableIntroAndOutro.transform.localPosition += new Vector3(xPosLeftColumn, yPosRow1, zPos);
			disableIntroAndOutro.ScaleBackground(1.75f, Vector3.one);
			disableIntroAndOutro.onInteraction += delegate (bool enable)
			{
				ModOptions.disableIntroAndOutro = enable;
				ModOptions.WriteOptsToFile("introAndOutro", Convert.ToInt32(enable));
			};
			output.SaveElement("introAndOutro", disableIntroAndOutro);
			popupText = "Skip the intro when starting a new file.\nSkip the outro when you beat the game.";
			UITextFrame popupIntroAndOutro = UIFactory.Instance.CreatePopupFrame(popupXPos, popupYPos, disableIntroAndOutro, output.transform, popupText);
			popupIntroAndOutro.ScaleBackground(popupScale);

			// Disable extra cutscenes (intro dialog, boss spawns, efcs, etc.)?
			UICheckBox disableCutscenes = UIFactory.Instance.CreateCheckBox(LastCol, 0f, output.transform, "No delays");
			disableCutscenes.transform.localPosition += new Vector3(xPosLeftColumn, yPosRow2, zPos);
			disableCutscenes.ScaleBackground(scale, Vector3.one);
			disableCutscenes.onInteraction += delegate (bool enable)
			{
				ModOptions.disableExtraCutscenes = enable;
				ModOptions.WriteOptsToFile("extraCutscenes", Convert.ToInt32(enable));
			};
			output.SaveElement("extraCutscenes", disableCutscenes);
			popupText = "Skip boss/chest spawn delays, raft break delay,\nefcs delay, and more";
			UITextFrame popupCutscenes = UIFactory.Instance.CreatePopupFrame(popupXPos, popupYPos, disableCutscenes, output.transform, popupText);
			popupCutscenes.ScaleBackground(popupScale);

			// Fast transitions?
			UICheckBox fastTransitions = UIFactory.Instance.CreateCheckBox(LastCol, 0f, output.transform, "Fast transitions");
			fastTransitions.transform.localPosition += new Vector3(xPosLeftColumn, yPosRow3, zPos);
			fastTransitions.ScaleBackground(scale, Vector3.one);
			fastTransitions.onInteraction += delegate (bool enable)
			{
				ModOptions.fastTransitions = enable;
				ModOptions.WriteOptsToFile("fastTransitions", Convert.ToInt32(enable));
				ModOptions.FastTransitions(false);
			};
			output.SaveElement("fastTransitions", fastTransitions);
			popupText = "Faster load and room transitions";
			UITextFrame popupFastTransitions = UIFactory.Instance.CreatePopupFrame(popupXPos, popupYPos, fastTransitions, output.transform, popupText);
			popupFastTransitions.ScaleBackground(popupScale);

			// Better health meter?
			UICheckBox betterHealthMeter= UIFactory.Instance.CreateCheckBox(LastCol, 0f, output.transform, "Dynamic HP meter");
			betterHealthMeter.transform.localPosition += new Vector3(xPosRightColumn, yPosRow1, zPos);
			betterHealthMeter.ScaleBackground(scale, Vector3.one);
			betterHealthMeter.onInteraction += delegate (bool enable)
			{
				ModOptions.betterHealthMeter = enable;
				ModOptions.WriteOptsToFile("betterHealthMeter", Convert.ToInt32(enable));
			};
			output.SaveElement("betterHealthMeter", betterHealthMeter);
			popupText = "The health meter removes unused background\npieces and doesn't glitch out when max HP < 0";
			UITextFrame popupHealthMeter = UIFactory.Instance.CreatePopupFrame(popupXPos, popupYPos, betterHealthMeter, output.transform, popupText);
			popupHealthMeter.ScaleBackground(popupScale);

			// Fix Dark Ogler double drop?
			UICheckBox darkOglerDropFix = UIFactory.Instance.CreateCheckBox(LastCol, 0f, output.transform, "Dark Ogler drop fix");
			darkOglerDropFix.transform.localPosition += new Vector3(xPosRightColumn, yPosRow2, zPos);
			darkOglerDropFix.ScaleBackground(scale, Vector3.one);
			darkOglerDropFix.onInteraction += delegate (bool enable)
			{
				ModOptions.darkOglerDropFix = enable;
				ModOptions.WriteOptsToFile("darkOglerDropFix", Convert.ToInt32(enable));
			};
			output.SaveElement("darkOglerDropFix", darkOglerDropFix);
			popupText = "Fixes a vanilla oversight where Dark Oglers\ndouble their droptable presence";
			UITextFrame popupOglerFix = UIFactory.Instance.CreatePopupFrame(popupXPos, popupYPos, darkOglerDropFix, output.transform, popupText);
			popupOglerFix.ScaleBackground(popupScale);

			// Load all rooms at all times?
			UICheckBox loadAllRooms = UIFactory.Instance.CreateCheckBox(LastCol, 0f, output.transform, "Load all rooms");
			loadAllRooms.transform.localPosition += new Vector3(xPosRightColumn, yPosRow3, zPos);
			loadAllRooms.ScaleBackground(scale, Vector3.one);
			loadAllRooms.onInteraction += delegate (bool enable)
			{
				ModOptions.loadAllRooms = enable;
				ModOptions.WriteOptsToFile("loadAllRooms", Convert.ToInt32(enable));
				ModOptions.LoadAllRooms(enable);
			};
			output.SaveElement("loadAllRooms", loadAllRooms);
			popupText = "Loads all rooms for the current scene and any scenes\nyou enter. Will cause lag for large scenes!";
			UITextFrame popupLoadRooms = UIFactory.Instance.CreatePopupFrame(popupXPos, popupYPos, loadAllRooms, output.transform, popupText);
			popupLoadRooms.ScaleBackground(popupScale);

			return output;
		}

		*/

		//World menu
		static UIScreen WorldMenu()
		{
			// Create a base window, which contains a title and a back button. Name it (or not and leave it with default name)
			UIScreen output = UIScreen.CreateBaseScreen("World");

			//Fast travel
			UIListExplorer sceneType = UIFactory.Instance.CreateListExplorer(FirstCol + 0.5f, 4.5f, output.transform, "");
			sceneType.Title.gameObject.SetActive(false);
			sceneType.ScaleBackground(0.8f);
			sceneType.AllowLooping = true;
			sceneType.ExplorerArray = MenuMapNode.MapTypeNames;
			UIListExplorer sceneName = UIFactory.Instance.CreateListExplorer(FirstCol + 0.5f, 3.5f, output.transform, "");
			sceneName.Title.gameObject.SetActive(false);
			sceneName.ScaleBackground(1.2f);
			sceneName.ExplorerArray = MenuMapNode.GatherNodesByType(MenuMapNode.MapType.Overworld);
			sceneName.AllowLooping = true;
			UIListExplorer doorName = UIFactory.Instance.CreateListExplorer(FirstCol + 0.5f, 2.5f, output.transform, "");
			doorName.Title.gameObject.SetActive(false);
			doorName.ScaleBackground(1.2f);
			doorName.ExplorerArray = MenuMapNode.FindByName("Fluffy Fields").GetDoors();
			doorName.AllowLooping = true;
			UIButton goTravel = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, FirstCol + 0.5f, 1.5f, output.transform, "<size=40>Travel</size>");
            goTravel.AutoTextResize = false;
			goTravel.ScaleBackground(new Vector2(0.7f, 1f), Vector2.one);
			//If the type is changed, update the scene explorer and the door explorer
			sceneType.onInteraction += delegate (bool arrow, string stringvalue, int index)
			{
				sceneName.ExplorerArray = MenuMapNode.GatherNodesByType((MenuMapNode.MapType)Enum.ToObject(typeof(MenuMapNode.MapType), index));
				sceneName.Trigger();
			};
			//If the scene is changed, update the door explorer
			sceneName.onInteraction += delegate (bool arrow, string stringvalue, int index)
			{
				MenuMapNode tempNode = MenuMapNode.FindByName(stringvalue);
				if (tempNode != null) { doorName.ExplorerArray = tempNode.GetDoors(); }
			};
			//If travel is clicked, warp to the scene
			goTravel.onInteraction += delegate ()
			{
				MenuMapNode.FindByName(sceneName.StringValue).Warp(doorName.StringValue);
			};

			//Weather
			UIListExplorer weatherList = UIFactory.Instance.CreateListExplorer(FirstCol + 0.5f, -0.5f, output.transform, "Weather");
			weatherList.ScaleBackground(0.8f);
			weatherList.AllowLooping = true;
			weatherList.ExplorerArray = new string[] { "Sunny", "Monochrome", "Rain", "Volcano", "Snow" };
			UIButton triggerWeather = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, FirstCol + 0.5f, -1.5f, output.transform);
			triggerWeather.ScaleBackground(new Vector2(0.7f, 1f), Vector3.one * 0.8f);
			triggerWeather.onInteraction += delegate ()
			{
				DebugCommands.Instance.Weather(new string[] { weatherList.StringValue });
			};
			UITextFrame weatherMessage = UIFactory.Instance.CreateTextFrame(FirstCol + 0.5f, -3f, output.transform, "Weather availability depends on\nvisited map sections");
			weatherMessage.transform.localPosition += new Vector3(0f, 0f, -1.1f);
			weatherMessage.ScaleBackground(new Vector2(1.5f, 1.7f));
			weatherMessage.gameObject.SetActive(false);
			GuiSelectionObject weatherSelection = triggerWeather.gameObject.GetComponentInChildren<GuiSelectionObject>();
			bool oldWeatherState = false;
			triggerWeather.onUpdate += delegate ()
			{
				if (weatherSelection.IsSelected == oldWeatherState) { return; }
				oldWeatherState = weatherSelection.IsSelected;
				weatherMessage.gameObject.SetActive(oldWeatherState);
			};

			//Knockback
			UISlider knockback = UIFactory.Instance.CreateSlider(LastCol, 5.5f, output.transform, "Knockback");
			knockback.SetSlider(-1f, 50f, 0.5f, 1f);
			knockback.onInteraction += delegate (float slider)
			{
				DebugCommands.Instance.Knockback(new string[] { slider.ToString() });
			};
			output.SaveElement("knockback", knockback);

			//NPC Scale
			UIVector3 enemiesScale = UIFactory.Instance.CreateVector3(LastCol, 3f, output.transform, "NPCs size");
			enemiesScale.Explorer.Title.ScaleBackground(new Vector2(0.7f, 0.8f));
			enemiesScale.onInteraction += delegate (Vector3 vector)
			{
				DebugCommands.Instance.SetSize(new string[] { "enemies", vector.x.ToString(), vector.y.ToString(), vector.z.ToString() });
			};
			output.SaveElement("npcscale", enemiesScale);

			//Time of the day
			UISlider tod = UIFactory.Instance.CreateSlider(LastCol, 1.5f, output.transform, "Time of the day");
			tod.SetSlider(0f, 24f, 0.5f, 1f);
			tod.onInteraction += delegate (float slider)
			{
				DebugCommands.Instance.Time(new string[] { "settime", slider.ToString() });
			};
			output.SaveElement("tod", tod);

			//Time flow
			UISlider timeFlow = UIFactory.Instance.CreateSlider(LastCol, 0f, output.transform, "Time flow");
			timeFlow.SetSlider(0f, 100f, 1f, 4f);
			timeFlow.DisplayInteger = true;
			timeFlow.onInteraction += delegate (float slider)
			{
				DebugCommands.Instance.Time(new string[] { "setflow", slider.ToString() });
			};
			output.SaveElement("flow", timeFlow);

			//Time flow
			UICheckBox showhud = UIFactory.Instance.CreateCheckBox(LastCol, -1.25f, output.transform, "Hide HUD");
			showhud.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.ShowHUD(new string[] { box ? "0" : "1" });
			};
			output.SaveElement("showhud", showhud);

			//Reset
			UIButton reset = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, LastCol, -3.5f, output.transform, "Reset");
			reset.ScaleBackground(new Vector2(1f, 2f), Vector2.one);
			reset.onInteraction += delegate ()
			{
				knockback.Trigger(1f);
				timeFlow.Trigger(4f);
				enemiesScale.Trigger(Vector3.one);
				showhud.Trigger(false);
			};

			return output;
		}

		//Items menu
		static UIScreen ItemsMenu()
		{
			// Create a base window, which contains a title and a back button. Name it (or not and leave it with default name)
			UIScreen output = UIScreen.CreateBaseScreen("Items");

			string[] weapons = new string[] { "None", "Level 1", "Level 2", "Level 3", "Dev" };
			string[] equipable = new string[] { "None", "Level 1", "Level 2", "Level 3" };

            UIScrollMenu gear = UIFactory.Instance.CreateScrollMenu(LastCol, 2f, output.transform, "Gear");
            gear.ScrollBar.transform.localPosition += new Vector3(-1.6f, 0f);
            gear.ScrollBar.ResizeLength(7);

			UIListExplorer melee = UIFactory.Instance.CreateListExplorer(0f, -1f, output.transform, "Melee");
			melee.ExplorerArray = new string[] { "Stick", "Fire Sword", "Fire Mace", "EFCS" };
			output.SaveElement("melee", melee);
			melee.onInteraction += delegate (bool arrow, string stringValue, int index)
			{
				DebugCommands.Instance.SetItem(new string[] { "melee", index.ToString() });
			};
            melee.ScaleBackground(0.6f);
            gear.Assign(melee,0.5f,0.5f);

            UIListExplorer forceWand = UIFactory.Instance.CreateListExplorer(0f, -3f, output.transform, "Force Wand");
			//forceWand.Title.ScaleBackground(new Vector2(0.7f, 0.8f));
			forceWand.ExplorerArray = weapons;
			output.SaveElement("forcewand", forceWand);
			forceWand.onInteraction += delegate (bool arrow, string stringValue, int index)
			{
				DebugCommands.Instance.SetItem(new string[] { "forcewand", index.ToString() });
			};
            forceWand.ScaleBackground(0.6f);
            gear.Assign(forceWand, 0.5f, 0.5f);

            UIListExplorer iceRing = UIFactory.Instance.CreateListExplorer(0f, -5f, output.transform, "Ice Ring");
			//iceRing.Title.ScaleBackground(new Vector2(0.7f, 0.8f));
			iceRing.ExplorerArray = weapons;
			output.SaveElement("icering", iceRing);
			iceRing.onInteraction += delegate (bool arrow, string stringValue, int index)
			{
				DebugCommands.Instance.SetItem(new string[] { "icering", index.ToString() });
			};
            iceRing.ScaleBackground(0.6f);
            gear.Assign(iceRing, 0.5f, 0.5f);

            UIListExplorer dynamite = UIFactory.Instance.CreateListExplorer(0f, -7f, output.transform, "Dynamite");
			//dynamite.Title.ScaleBackground(new Vector2(0.7f, 0.8f));
			dynamite.ExplorerArray = weapons;
			output.SaveElement("dynamite", dynamite);
			dynamite.onInteraction += delegate (bool arrow, string stringValue, int index)
			{
				DebugCommands.Instance.SetItem(new string[] { "dynamite", index.ToString() });
			};
            dynamite.ScaleBackground(0.6f);
            gear.Assign(dynamite, 0.5f, 0.5f);

            UIListExplorer chain = UIFactory.Instance.CreateListExplorer(0f, -9f, output.transform, "Chain");
			chain.ExplorerArray = equipable;
			output.SaveElement("chain", chain);
			chain.onInteraction += delegate (bool arrow, string stringValue, int index)
			{
				DebugCommands.Instance.SetItem(new string[] { "chain", index.ToString() });
			};
            chain.ScaleBackground(0.6f);
            gear.Assign(chain, 0.5f, 0.5f);

            UIListExplorer tome = UIFactory.Instance.CreateListExplorer(0f, -11f, output.transform, "Tome");
			tome.ExplorerArray = equipable;
			output.SaveElement("tome", tome);
			tome.onInteraction += delegate (bool arrow, string stringValue, int index)
			{
				DebugCommands.Instance.SetItem(new string[] { "tome", index.ToString() });
			};
            tome.ScaleBackground(0.6f);
            gear.Assign(tome, 0.5f, 0.5f);

            UIListExplorer headBand = UIFactory.Instance.CreateListExplorer(0f, -13f, output.transform, "Headband");
			//headBand.Title.ScaleBackground(new Vector2(0.7f, 0.8f));
			headBand.ExplorerArray = equipable;
			output.SaveElement("headband", headBand);
			headBand.onInteraction += delegate (bool arrow, string stringValue, int index)
			{
				DebugCommands.Instance.SetItem(new string[] { "headband", index.ToString() });
			};
            headBand.ScaleBackground(0.6f);
            gear.Assign(headBand, 0.5f, 0.5f);

            UIListExplorer tracker = UIFactory.Instance.CreateListExplorer(0f, -15f, output.transform, "Tracker");
			tracker.ExplorerArray = equipable;
			output.SaveElement("tracker", tracker);
			tracker.onInteraction += delegate (bool arrow, string stringValue, int index)
			{
				DebugCommands.Instance.SetItem(new string[] { "tracker", index.ToString() });
			};
            tracker.ScaleBackground(0.6f);
            gear.Assign(tracker, 0.5f, 0.5f);

            UIListExplorer amulet = UIFactory.Instance.CreateListExplorer(0f, -17f, output.transform, "Amulet");
			amulet.ExplorerArray = equipable;
			output.SaveElement("amulet", amulet);
			amulet.onInteraction += delegate (bool arrow, string stringValue, int index)
			{
				DebugCommands.Instance.SetItem(new string[] { "amulet", index.ToString() });
			};
            amulet.ScaleBackground(0.6f);
            gear.Assign(amulet, 0.5f, 0.5f);

            UISlider raft = UIFactory.Instance.CreateSlider(FirstCol + 0.4f, 4.5f, output.transform, "Raft pieces");
			raft.SetSlider(0f, 8f, 1f, 0f);
			raft.DisplayInteger = true;
			output.SaveElement("raft", raft);
			raft.onInteraction += delegate (float slider)
			{
				DebugCommands.Instance.SetItem(new string[] { "raft", ((int)slider).ToString() });
			};

			UISlider shards = UIFactory.Instance.CreateSlider(FirstCol + 0.4f, 3f, output.transform, "Shards");
			shards.SetSlider(0f, 24f, 1f, 0f);
			shards.DisplayInteger = true;
			output.SaveElement("shards", shards);
			shards.onInteraction += delegate (float slider)
			{
				DebugCommands.Instance.SetItem(new string[] { "shards", ((int)slider).ToString() });
			};

			UISlider evilKeys = UIFactory.Instance.CreateSlider(FirstCol + 0.4f, 1.5f, output.transform, "Forbidden Keys");
			evilKeys.SetSlider(0f, 4f, 1f, 0f);
			evilKeys.DisplayInteger = true;
			output.SaveElement("evilkeys", evilKeys);
			evilKeys.onInteraction += delegate (float slider)
			{
				DebugCommands.Instance.SetItem(new string[] { "evilkeys", ((int)slider).ToString() });
			};

			UIButton minusKey = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, FirstCol - 1.5f, 0.25f, output.transform, "-1 key");
			minusKey.ScaleBackground(new Vector2(0.7f, 1f), Vector2.one * 0.8f);
			minusKey.onInteraction += delegate ()
			{
				int itemlevel = ModSaver.LoadFromEnt("localKeys");
				itemlevel = itemlevel > 0 ? itemlevel - 1 : 0;
				DebugCommands.Instance.SetItem(new string[] { "localKeys", itemlevel.ToString() });
			};
			UIButton plusKey = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, FirstCol + 1.5f, 0.25f, output.transform, "+1 key");
			plusKey.ScaleBackground(new Vector2(0.7f, 1f), Vector2.one * 0.8f);
			plusKey.onInteraction += delegate ()
			{
				int itemlevel = ModSaver.LoadFromEnt("localKeys");
				itemlevel++;
				DebugCommands.Instance.SetItem(new string[] { "localKeys", itemlevel.ToString() });
			};

			UIButton minusLockpick = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, FirstCol - 1.5f, -1f, output.transform, "-1 lockpick");
			minusLockpick.ScaleBackground(new Vector2(0.7f, 1f), Vector2.one * 0.8f);
			minusLockpick.onInteraction += delegate ()
			{
				int itemlevel = ModSaver.LoadFromEnt("keys");
				itemlevel = itemlevel > 0 ? itemlevel - 1 : 0;
				DebugCommands.Instance.SetItem(new string[] { "keys", itemlevel.ToString() });
			};
			UIButton plusLockpick = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, FirstCol + 1.5f, -1f, output.transform, "+1 lockpick");
			plusLockpick.ScaleBackground(new Vector2(0.7f, 1f), Vector2.one * 0.8f);
			plusLockpick.onInteraction += delegate ()
			{
				int itemlevel = ModSaver.LoadFromEnt("keys");
				itemlevel++;
				DebugCommands.Instance.SetItem(new string[] { "keys", itemlevel.ToString() });
			};

			UIListExplorer itemCreator = UIFactory.Instance.CreateListExplorer(FirstCol, -2.8f, output.transform, "Spawn item");
			itemCreator.Title.ScaleBackground(new Vector2(0.7f, 0.8f));
			itemCreator.ExplorerArray = DebugCommands.Instance.ItemList().Split(new char[] { ' ' });
			itemCreator.AllowLooping = true;

			UIButton spawn = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, FirstCol, -3.8f, output.transform);
			spawn.ScaleBackground(new Vector2(0.7f, 1f), Vector2.one * 0.8f);
			spawn.onInteraction += delegate ()
			{
				DebugCommands.Instance.CreateItem(new string[] { itemCreator.StringValue });
			};


			return output;
		}

		//Powerups menu
		static UIScreen PowerupsMenu()
		{
			// Create a base window, which contains a title and a back button. Name it (or not and leave it with default name)
			UIScreen output = UIScreen.CreateBaseScreen("Powerups");

			//God
			UICheckBox godCheckbox = UIFactory.Instance.CreateCheckBox(FirstCol, 5f, output.transform, "God mode");
			godCheckbox.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.God(new string[] { box ? "1" : "0" });
			};
			output.SaveElement("god", godCheckbox);

			//Like a boss
			UICheckBox bossCheckbox = UIFactory.Instance.CreateCheckBox(FirstCol, 3.75f, output.transform, "Like a boss");
			bossCheckbox.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.LikeABoss(new string[] { box ? "1" : "0" });
			};
			output.SaveElement("likeaboss", bossCheckbox);
			UITextFrame bossMessage = UIFactory.Instance.CreateTextFrame(FirstCol, 2.25f, output.transform, "Destroy enemies\nin one hit");
			bossMessage.transform.localPosition += new Vector3(0f, 0f, -1.1f);
			bossMessage.ScaleBackground(new Vector2(1.5f, 1.7f));
			bossMessage.gameObject.SetActive(false);
			GuiSelectionObject bossSelection = bossCheckbox.gameObject.GetComponentInChildren<GuiSelectionObject>();
			bool oldBossstate = false;
			bossCheckbox.onUpdate += delegate ()
			{
				if (bossSelection.IsSelected == oldBossstate) { return; }
				oldBossstate = bossSelection.IsSelected;
				bossMessage.gameObject.SetActive(oldBossstate);
			};

			//Noclip
			UICheckBox noclipCheckbox = UIFactory.Instance.CreateCheckBox(FirstCol, 2.5f, output.transform, "Noclip");
			noclipCheckbox.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.NoClip(new string[] { box ? "1" : "0" });
			};
			UITextFrame noclipMessage = UIFactory.Instance.CreateTextFrame(FirstCol, 1f, output.transform, "Disable collision, some\nenemies will ignore you");
			noclipMessage.transform.localPosition += new Vector3(0f, 0f, -1.1f);
			noclipMessage.ScaleBackground(new Vector2(1.5f, 1.7f));
			noclipMessage.gameObject.SetActive(false);
			GuiSelectionObject noclipSelection = noclipCheckbox.gameObject.GetComponentInChildren<GuiSelectionObject>();
			bool oldNoclipstate = false;
			noclipCheckbox.onUpdate += delegate ()
			{
				if (noclipSelection.IsSelected == oldNoclipstate) { return; }
				oldNoclipstate = noclipSelection.IsSelected;
				noclipMessage.gameObject.SetActive(oldNoclipstate);
			};
			output.SaveElement("noclip", noclipCheckbox);

			//Super iceblocks
			UICheckBox superIce = UIFactory.Instance.CreateCheckBox(FirstCol, 1.25f, output.transform, "Infinite Ice");
			superIce.ScaleBackground(1f, Vector3.one);
			superIce.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-ice", box ? "1" : "0" });
			};
			output.SaveElement("superice", superIce);

			//Super dynamite
			UICheckBox superDynamite = UIFactory.Instance.CreateCheckBox(FirstCol, 0f, output.transform, "Infinite dynamites");
			superDynamite.ScaleBackground(1.3f, Vector3.one);
			superDynamite.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-dynamite", box ? "1" : "0" });
			};
			output.SaveElement("superdynamite", superDynamite);

			//No icegrid
			UICheckBox freeIce = UIFactory.Instance.CreateCheckBox(FirstCol, -1.25f, output.transform, "Gridless Ice");
			freeIce.ScaleBackground(1.1f, Vector3.one);
			freeIce.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-icegrid", box ? "1" : "0" });
			};
			output.SaveElement("freeice", freeIce);

			//Super attack
			UICheckBox superAttack = UIFactory.Instance.CreateCheckBox(FirstCol, -2.50f, output.transform, "Super Attack");
			superAttack.ScaleBackground(1.2f, Vector3.one);
			superAttack.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-attack", box ? "1" : "0" });
			};
			output.SaveElement("superattack", superAttack);
			UITextFrame attackmessage = UIFactory.Instance.CreateTextFrame(FirstCol, -1.0f, output.transform, "Requires EFCS");
			attackmessage.transform.localPosition += new Vector3(0f, 0f, -1.1f);
			attackmessage.ScaleBackground(new Vector2(1.5f, 1.7f));
			attackmessage.gameObject.SetActive(false);
			GuiSelectionObject attackSelection = superAttack.gameObject.GetComponentInChildren<GuiSelectionObject>();
			bool oldAttackState = false;
			superAttack.onUpdate += delegate ()
			{
				if (attackSelection.IsSelected == oldAttackState) { return; }
				oldAttackState = attackSelection.IsSelected;
				attackmessage.gameObject.SetActive(oldAttackState);
			};

			//Super attack shake
			UICheckBox noShake = UIFactory.Instance.CreateCheckBox(FirstCol, -3.75f, output.transform, "Less shake");
			noShake.ScaleBackground(1.2f, Vector3.one);
			noShake.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-noshake", box ? "1" : "0" });
			};
			superAttack.onInteraction += delegate (bool box)
			{
				noShake.gameObject.SetActive(box);
			};
			noShake.gameObject.SetActive(false);
			output.SaveElement("noshake", noShake);

			//Health
			GameObject hpParent = new GameObject();
			hpParent.transform.SetParent(output.transform, false);
			hpParent.transform.localPosition = new Vector3(0f, 4.7f, 0f);
			UIListExplorer hpExplorer = UIFactory.Instance.CreateListExplorer(MidCol, 0f, hpParent.transform, "Health");
			hpExplorer.ScaleBackground(1.2f);
			hpExplorer.ExplorerArray = new string[] { "Set to 1", "Remove 5 hearts", "Remove 1 heart", "Remove 1/4 hearts", "Add 1/4 hearts", "Add 1 heart", "Add 5 hearts" };
			hpExplorer.IndexValue = 5;
			output.SaveElement("health", hpExplorer);
			UIButton acceptHP = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, MidCol + 1.5f, -0.90f, hpParent.transform);
			acceptHP.ScaleBackground(new Vector2(0.75f, 1f), new Vector2(0.65f, 0.65f));
			int[] hpReference = new int[] { 0, -20, -4, -1, 1, 4, 20 };
			acceptHP.onInteraction += delegate ()
			{
				if (hpExplorer.IndexValue == 0)
				{
					DebugCommands.Instance.SetHP(new string[] { "-setmaxhp", "1", "-full" });
					return;
				}
				DebugCommands.Instance.SetHP(new string[] { "-addmaxhp", hpReference[hpExplorer.IndexValue].ToString(), "-full" });
			};
			UIButton healHP = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, MidCol - 1.5f, -0.90f, hpParent.transform, "Heal");
			healHP.ScaleBackground(new Vector2(0.75f, 1f), new Vector2(0.65f, 0.65f));
			healHP.onInteraction += delegate ()
			{
				DebugCommands.Instance.SetHP(new string[] { "-full" });
			};

			//Speed
			UISlider speedSlider = UIFactory.Instance.CreateSlider(MidCol + 0.2f, 2.5f, output.transform, "Run speed"); //Create the slider
			speedSlider.SliderRange = new Range(0.5f, 25f); //Set the range
			speedSlider.SliderStep = 0.5f; //Set the step size
			speedSlider.Value = 5f; //Set initial value
			speedSlider.onInteraction += delegate (float speed)
			{
				DebugCommands.Instance.SetSpeed(new string[] { speed.ToString() });
			};
			output.SaveElement("speed", speedSlider);

			//Attack range
			UISlider attackRange = UIFactory.Instance.CreateSlider(MidCol + 0.2f, 1f, output.transform, "Melee Range"); //Create the slider
			attackRange.SliderRange = new Range(0f, 50f); //Set the range
			attackRange.SliderStep = 0.5f; //Set the step size
			attackRange.Value = 0f; //Set initial value
			attackRange.onInteraction += delegate (float range)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-range", range.ToString() });
			};
			output.SaveElement("attackrange", attackRange);

			//Dynamite range
			UISlider dynaRange = UIFactory.Instance.CreateSlider(MidCol + 0.2f, -0.5f, output.transform, "Dynamite Range"); //Create the slider
			dynaRange.SliderRange = new Range(0f, 100f); //Set the range
			dynaRange.SliderStep = 0.5f; //Set the step size
			dynaRange.Value = 1.7f; //Set initial value
			dynaRange.onInteraction += delegate (float radius)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-radius", radius.ToString() });
			};
			output.SaveElement("dynarange", dynaRange);

			//Multi-projectile
			UISlider multiProj = UIFactory.Instance.CreateSlider(MidCol + 0.2f, -2f, output.transform, "Multi-Projectile"); //Create the slider
			multiProj.SliderRange = new Range(1f, 20f); //Set the range
			multiProj.SliderStep = 1f; //Set the step size
			multiProj.DisplayInteger = true;
			multiProj.Value = 1f; //Set initial value
			multiProj.onInteraction += delegate (float proj)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-proj", ((int)proj).ToString() });
			};
			output.SaveElement("proj", multiProj);

			//Dynamite fuse
			UIListExplorer dynaFuse = UIFactory.Instance.CreateListExplorer(LastCol, 6f, output.transform, "Dynamite fuse");
			dynaFuse.ExplorerArray = new string[] { "Instant", "0.5 seconds", "Default (1.5)", "5 seconds", "20 seconds", "1 minute", "10 minutes", "5 years" };
			dynaFuse.IndexValue = 2;
			dynaFuse.Title.ScaleBackground(new Vector2(0.9f, 0.8f));
			float[] dynaArray = new float[] { 0f, 0.5f, 1.5f, 5f, 20f, 60f, 600f, 157680000f };
			dynaFuse.onInteraction += delegate (bool rightArrow, string stringValue, int indexValue)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-fuse", dynaArray[indexValue].ToString() });
			};
			output.SaveElement("fuse", dynaFuse);

			//Outfit
			UIListExplorer outfit = UIFactory.Instance.CreateListExplorer(LastCol, 4.25f, output.transform, "Outfit");
			outfit.ExplorerArray = new string[] { "Ittle", "Tippsie", "ID1", "Jenny", "Swimsuit", "Fierce diety", "CCN", "Delinquent", "Frog", "That Guy", "Jenny Berry" };
			outfit.IndexValue = 0;
			outfit.AllowLooping = true;
			outfit.onInteraction += delegate (bool rightArrow, string stringValue, int indexValue)
			{
				DebugCommands.Instance.SetOutfit(new string[] { ((int)indexValue).ToString() });
			};
			output.SaveElement("outfit", outfit);

			//Player Scale
			UIVector3 playerScale = UIFactory.Instance.CreateVector3(LastCol, 1.8f, output.transform, "Player size");
			playerScale.Explorer.Title.ScaleBackground(new Vector2(0.7f, 0.8f));
			playerScale.onInteraction += delegate (Vector3 vector)
			{
				DebugCommands.Instance.SetSize(new string[] { "self", vector.x.ToString(), vector.y.ToString(), vector.z.ToString() });
			};
			output.SaveElement("scale", playerScale);

			//Destroy NPCs and Holding frame
			UIGFXButton destroyNpcs = UIFactory.Instance.CreateGFXButton("destroynpcs", LastCol, -0.25f, output.transform, "Destroy nearby\nenemies");
			destroyNpcs.onInteraction += delegate ()
			{
				DebugCommands.Instance.Kill(new string[] { "enemies" });
			};
            destroyNpcs.AutoTextResize = false;
			output.SaveElement("destroy", destroyNpcs);
			UIBigFrame buttonHolder = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 0f, destroyNpcs.transform.Find("ModButton").Find("ModUIElement"));
			buttonHolder.ScaleBackground(new Vector2(0.5f, 0.5f));
			buttonHolder.transform.localPosition += new Vector3(0f, -0.25f, 1.1f);
			UITextFrame destroyMessage = UIFactory.Instance.CreateTextFrame(0f, -3f, destroyNpcs.transform, "Destroy all enemies\non the screen");
			destroyMessage.ScaleBackground(new Vector2(1.2f, 1.2f));
			destroyMessage.transform.localPosition += new Vector3(0f, 0f, -1.1f);
			destroyMessage.gameObject.SetActive(false);
			GuiSelectionObject destroySelection = destroyNpcs.gameObject.GetComponentInChildren<GuiSelectionObject>();
			bool oldDestroyState = false;
			superAttack.onUpdate += delegate ()
			{
				if (destroySelection.IsSelected == oldDestroyState) { return; }
				oldDestroyState = destroySelection.IsSelected;
				destroyMessage.gameObject.SetActive(oldDestroyState);
			};

			//Reset
			UIButton reset = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, LastCol, -3.5f, output.transform, "Reset");
			reset.ScaleBackground(new Vector2(1f, 2f), Vector2.one);
			reset.onInteraction += delegate ()
			{
				godCheckbox.Trigger(false);
				bossCheckbox.Trigger(false);
				noclipCheckbox.Trigger(false);
				speedSlider.Trigger(5f);
				attackRange.Trigger(0f);
				superDynamite.Trigger(false);
				superIce.Trigger(false);
				noShake.Trigger(false);
				superAttack.Trigger(false);
				dynaRange.Trigger(1.7f);
				dynaFuse.Trigger(false, 2);
				freeIce.Trigger(false);
				multiProj.Trigger(1f);
				playerScale.Trigger(Vector3.one);
			};


			return output;
		}

		//Spawn menu
        //Beware, this is is pure pasta
		static UIScreen SpawnMenu()
		{
			//Create a base window, which contains a title and a back button. Name it.
			UIScreen output = UIScreen.CreateBaseScreen("Spawn Menu");

            //Testframe
            //I used this textframe to print text on screen
            //UITextFrame testme = UIFactory.Instance.CreateTextFrame(0f, -1f, output.transform);
            //testme.UIName = "here I put my test text";

            //Holders for advanced controls
            GameObject advancedControls = new GameObject("AdvancedSpawnControls");
            advancedControls.transform.SetParent(output.transform, false);
            advancedControls.transform.localScale = Vector3.one;

            //Holders for NPC selection
            GameObject npcSelection = new GameObject("NPCSelection");
            npcSelection.transform.SetParent(output.transform, false);
            npcSelection.transform.localScale = Vector3.one;

            //State of the menu
            bool advancedMode = false;
            bool aiSelect = false;

            //NPC type explorer
            UIButton npcExplorer = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, MidCol, 4.5f, advancedControls.transform);
            npcExplorer.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            string[] tempArray = ModSpawner.Instance.EntitiesNamesList; //Gather the list
            Array.Sort(tempArray, StringComparer.InvariantCulture); //Sort the list
            //Initiate on fishbun, if not, on the first value if available
            if (Array.IndexOf(tempArray, "Fishbun") != -1)
            {
                npcExplorer.UIName = "Fishbun";
            }
            else
            {
                npcExplorer.UIName = (tempArray.Length > 0) ? tempArray[0] : "NO NPCS AVAILABLE";
            }
            //Help message
            UITextFrame spawnBtnHelp = UIFactory.Instance.CreatePopupFrame(0f, 3.2f, npcExplorer, advancedControls.transform, "Select NPC to spawn");
            spawnBtnHelp.transform.localPosition += new Vector3(0f, 0f, -0.2f);

            //NPC List message
            string selectionText = ModText.WrapText("NPCs availability depends on the island location. After spawning an NPC it will be available anywhere", 30f, false);
            UITextFrame selectionMessage = UIFactory.Instance.CreateTextFrame(3.0f, -1.5f, npcSelection.transform, selectionText);
            selectionMessage.ScaleBackground(new Vector2(2.3f, 1.5f));

            //Mode switch to advanced
            UIButton openAdvanced = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, -4f, -1.5f, npcSelection.transform, "Advanced Mode");
            openAdvanced.ScaleBackground(new Vector2(0.7f,1f), new Vector2(1.5f, 1.5f));
            openAdvanced.onInteraction += delegate ()
            {
                advancedControls.SetActive(true);
                npcSelection.SetActive(false);
                advancedMode = true;
            };

            //Mode switch to quick
            UIButton openQuick = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, FirstCol, -2f, advancedControls.transform, "Quick Mode");
            openQuick.ScaleBackground(new Vector2(0.7f, 1f), new Vector2(1.2f, 1.2f));
            openQuick.onInteraction += delegate ()
            {
                advancedControls.SetActive(false);
                npcSelection.SetActive(true);
                advancedMode = false;
                openAdvanced.gameObject.SetActive(true);
            };

            //Spawn Button
            UIButton spawnNow = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, MidCol, 3.35f, advancedControls.transform, "Spawn"); //Create the Button
			spawnNow.ScaleBackground(new Vector2(0.6f, 1f), Vector2.one);
            
			//Amount
			UISlider amountToSpawn = UIFactory.Instance.CreateSlider(FirstCol + 0.5f, 2.7f, advancedControls.transform, "Amount"); //Create the slider
			amountToSpawn.SliderRange = new Range(1f, 25f); //Set the range
			amountToSpawn.SliderStep = 1f; //Set the step size
			amountToSpawn.DisplayInteger = true; //Make it display integers
			amountToSpawn.Value = 0f; //Set initial value

			//Distance chooser
			UISlider distanceToSpawn = UIFactory.Instance.CreateSlider(FirstCol + 0.5f, 1.2f, advancedControls.transform, "Distance"); //Create the slider
			distanceToSpawn.SliderRange = new Range(0f, 10f); //Set the range
			distanceToSpawn.SliderStep = 0.5f; //Set the step size
			distanceToSpawn.Value = 2f; //Set initial value

			//Position scatterer
			UISlider randomPos = UIFactory.Instance.CreateSlider(FirstCol + 0.5f, -0.3f, advancedControls.transform, "Scatter"); //Create the slider
			randomPos.SliderRange = new Range(0f, 20f); //Set the range
			randomPos.SliderStep = 0.5f; //Set the step size
			randomPos.Value = 0f; //Set initial value

            //Spawn properties
            UIScrollMenu propMenu = UIFactory.Instance.CreateScrollMenu(LastCol, 0.5f, advancedControls.transform, "Properties");
            propMenu.ScrollBar.transform.localPosition += new Vector3(-1.6f, 0f, 0f);
            propMenu.ScrollBar.ResizeLength(6);
            propMenu.Title.ScaleBackground(new Vector2(0.8f, 1f));
            propMenu.CanvasWindow = 6.5f;
            propMenu.EmptySpace = 1f;

			//Scale
			UIVector3 scaleVector3 = UIFactory.Instance.CreateVector3(0f, -1.5f, advancedControls.transform, "Scale");
            scaleVector3.transform.localScale = Vector3.one * 0.8f;
            propMenu.Assign(scaleVector3, 1.25f, 0.75f);

            //Rotation selector
            //Holder
            GameObject rotHolder = new GameObject("RotHolder");
            rotHolder.transform.SetParent(output.transform, false);
            rotHolder.transform.localScale = Vector3.one;
            rotHolder.transform.localPosition = new Vector3(0f, -3.0f, 0f);
            //List explorer
			UIListExplorer rotSelect = UIFactory.Instance.CreateListExplorer(0f, 0f, rotHolder.transform, "Rotation");
            rotSelect.transform.localScale *= 0.8f;
			rotSelect.AllowLooping = true;
			string constant = "Constant";
			rotSelect.ExplorerArray = new string[] { "Random", "Player", constant };
			rotSelect.ScaleBackground(0.75f);
            //Slider
			UISlider rotSlider = UIFactory.Instance.CreateSlider(0f, -0.75f, rotHolder.transform, ""); //Create the slider
			rotSlider.SetSlider(0f, 360f, 0.01f, 0f); //Set slider can be used instad of the individual variables and it sets non-linear sliders
            rotSlider.transform.localScale *= 0.8f;
            rotSlider.DisplayInteger = true;
			rotSlider.gameObject.SetActive(false);
			rotSelect.onInteraction += delegate (bool rightArrow, string stringValue, int index)
			{
				rotSlider.gameObject.SetActive(stringValue == constant);
			};
            propMenu.Assign(rotHolder, 0.6f, 1f);

            //AI explorer
            //Holder
            GameObject aiHolder = new GameObject("AIHolder");
            aiHolder.transform.SetParent(output.transform, false);
            aiHolder.transform.localScale = Vector3.one;
            aiHolder.transform.localPosition = new Vector3(0f, -5f, 0f);
            //Checkbox
            UICheckBox aiCheckbox = UIFactory.Instance.CreateCheckBox(0f, 0f, aiHolder.transform, "Change AI?");
            aiCheckbox.ScaleBackground(1f, Vector3.one * 0.8f);
			aiCheckbox.transform.localPosition += new Vector3(0f, 0f, -0.2f);
            //Button
			UIButton aiExplorer = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 0f, -0.75f, aiHolder.transform, "Click to select AI");
			aiExplorer.gameObject.SetActive(false);
            aiCheckbox.onInteraction += delegate (bool enable) { aiExplorer.gameObject.SetActive(enable); };
            propMenu.Assign(aiHolder);

            //HP
            UIListExplorer hpExplorer = UIFactory.Instance.CreateListExplorer(0f, -7.3f, advancedControls.transform, "Health"); //Create the UIElement
			hpExplorer.ExplorerArray = new string[] { "1 HP", "0.5x HP", "Regular", "2x HP", "5x HP", "10x HP", "100x HP", "Invulnerable" }; //Create a string[] for the explorer
			hpExplorer.IndexValue = 2; //Set initial point for the list explorer
            hpExplorer.transform.localScale *= 0.8f;
            hpExplorer.ScaleBackground(0.75f);
            propMenu.Assign(hpExplorer);

            //No AI NPC
            UIButton noAI = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -4f, -1.5f, npcSelection.transform, "No AI");
            noAI.ScaleBackground(new Vector2(0.7f, 1f), new Vector2(1.5f, 1.5f));
            noAI.onInteraction += delegate ()
            {
                advancedControls.SetActive(true);
                npcSelection.SetActive(false);
                noAI.gameObject.SetActive(false);
                aiExplorer.UIName = "No AI";
            };
            //AI button function
            aiExplorer.onInteraction += delegate ()
            {
                openAdvanced.gameObject.SetActive(false);
                aiSelect = true;
                advancedControls.SetActive(false);
                npcSelection.SetActive(true);
                noAI.gameObject.SetActive(true);
            };
            noAI.gameObject.SetActive(false);

            //NPC Selection
            UI2DList npcList = UIFactory.Instance.Create2DList(0f, 2f, new Vector2(4f, 3f), Vector2.one, Vector2.one, npcSelection.transform);
            npcList.HighlightSelection = false;
            npcList.ScrollBar.ResizeLength(5);
            npcList.ScrollBar.transform.localPosition += new Vector3(3.3f, 0f, 0f);
            npcList.Title.gameObject.SetActive(false);
            npcList.ExplorerArray = tempArray;
            npcList.onInteraction += delegate (string textValue, int arrayIndex)
            {
                if (advancedMode)
                {
                    if(aiSelect)
                    {
                        aiExplorer.UIName = textValue;
                        aiSelect = false;
                        noAI.gameObject.SetActive(false);
                    }
                    else
                    {
                        npcExplorer.UIName = textValue;
                    }
                    advancedControls.SetActive(true);
                    npcSelection.SetActive(false);
                }
                else
                {
                    ModSpawner.SpawnProperties properties = new ModSpawner.SpawnProperties();
                    properties.npcName = textValue;
                    properties.SpawnInFrontOfPlayer(3.5f);
                    properties.useRandomRotation = false;
                    properties.fixedRotation = 180f;
                    properties.aroundPoint = true;
                    properties.distanceFromPoint = 1.5f;
                    ModSpawner.Instance.HoldNPC(properties.npcName, properties.npcName);
                    ModSpawner.Instance.SpawnNPC(properties);
                }
            };

            //Spawn Function
            spawnNow.onInteraction += delegate () //Set the delegate to the spawn function below, it can also be done using a named function and referencing it
			{
				//Spawn properties is the new class used to spawn NPCs. By default, it initializes
				//spawning a regular fishbun on top of Ittle, but everything can be changed by code
				ModSpawner.SpawnProperties properties = new ModSpawner.SpawnProperties(); //Create new spawn property, initialized
				if (aiCheckbox.Value) { properties.ai = aiExplorer.UIName; }
				properties.npcName = npcExplorer.UIName; //Extract the ArrayValue of the explorer, receiving the string of the NPC
				properties.amount = (int)amountToSpawn.Value; //Extract the Value of the slider, convert it into an integer
				properties.SpawnInFrontOfPlayer(distanceToSpawn.Value); //Set the position to spawn it in the world
				properties.scale = scaleVector3.Value;
				if (randomPos.Value > 0.1f) //If the random position slider is bigger than 0, use random position
				{
					properties.aroundPoint = true;
					properties.distanceFromPoint = randomPos.Value;
				}
				switch (rotSelect.IndexValue)
				{
					case 0:
						break;
					case 1:
						properties.UsePlayerRotation();
						break;
					case 2:
						properties.useRandomRotation = false;
						properties.fixedRotation = rotSlider.Value;
						break;
					default:
						return;
				}
                switch (hpExplorer.IndexValue) //HP selector switch
                {
                    case 0:
                        properties.ConfigureHP(1f, 0f);
                        break;
                    case 1:
                        properties.ConfigureHP(0f, 0.5f);
                        break;
                    case 2:
                        break;
                    case 3:
                        properties.ConfigureHP(0f, 2f);
                        break;
                    case 4:
                        properties.ConfigureHP(0f, 5f);
                        break;
                    case 5:
                        properties.ConfigureHP(0f, 10f);
                        break;
                    case 6:
                        properties.ConfigureHP(0f, 100f);
                        break;
                    case 7:
                        properties.MakeInvulnerable();
                        break;
                    default:
                        break;
                }
                ModSpawner.Instance.HoldNPC(properties.npcName, properties.npcName);
                ModSpawner.Instance.SpawnNPC(properties); //Spawn using spawnproperties
			};
			output.SaveElement("spawnnow", spawnNow); //Save the UIElement

            advancedControls.SetActive(false);
            npcSelection.SetActive(true);

            //NPC explorer function
            npcExplorer.onInteraction += delegate ()
            {
                openAdvanced.gameObject.SetActive(false);
                advancedControls.SetActive(false);
                npcSelection.SetActive(true);
            };

            //If back is pressed, remove NPC and NPC AI selection
            output.BackButton.onInteraction += delegate ()
            {
                if(advancedMode)
                {
                    advancedControls.SetActive(true);
                    npcSelection.SetActive(false);
                    aiSelect = false;
                    noAI.gameObject.SetActive(false);
                }
            };

            return output;
		}

		//Camera Menu
		static UIScreen CameraMenu()
		{
			// Create a base window, which contains a title and a back button. Name it (or not and leave it with default name)
			UIScreen output = UIScreen.CreateBaseScreen("Camera");
			//output.Title.transform.localScale = Vector3.one * 1.3f;
			//output.Title.transform.localPosition += new Vector3(0f, -1f, 0f);


			//Info panel
			UIBigFrame info = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, FirstCol + 2f, 0.75f, output.transform);
			info.ScaleBackground(new Vector2(0.55f, 1.3f));
			float wrapSize = 22f;
			info.NameTextMesh.alignment = TextAlignment.Left;
			output.SaveElement("info", info);

			//Field of view
			UISlider fovSlider = UIFactory.Instance.CreateSlider(LastCol + 0.5f, 4f, output.transform, "FOV"); //Create the slider
			fovSlider.SliderRange = new Range(10f, 120f); //Set the range
			fovSlider.SliderStep = 1f; //Set the step size
			fovSlider.Value = 65f; //Set initial value
			fovSlider.DisplayInteger = true;
			fovSlider.onInteraction += delegate (float fov) { DebugCommands.Instance.SetCam(new string[] { "-fov", fov.ToString() }); };
			output.SaveElement("fov", fovSlider); //Save the UIElement

			//Sensitivity
			UISlider sensSlider = UIFactory.Instance.CreateSlider(LastCol + 0.5f, 2.5f, output.transform, "Sensitivity"); //Create the slider
			sensSlider.SliderRange = new Range(0.2f, 20f); //Set the range
			sensSlider.SliderStep = 0.2f; //Set the step size
			sensSlider.Value = 3f; //Set initial value
			sensSlider.onInteraction += delegate (float fov) { DebugCommands.Instance.SetCam(new string[] { "-sens", fov.ToString() }); };
			output.SaveElement("sens", sensSlider); //Save the UIElement

			//Lock vertical
			UICheckBox lockVertical = UIFactory.Instance.CreateCheckBox(LastCol, 1f, output.transform, "Lock Vertical");
			lockVertical.onInteraction += delegate (bool lockVert) { DebugCommands.Instance.SetCam(new string[] { lockVert ? "-lockvertical" : "-unlockvertical" }); };
			lockVertical.ScaleBackground(1.3f, Vector3.one);
			output.SaveElement("lockvert", lockVertical); //Save the UIElementç

			//Free flight speed
			UISlider flightSlider = UIFactory.Instance.CreateSlider(LastCol + 0.5f, -0.5f, output.transform, "Acceleration"); //Create the slider
			flightSlider.SliderRange = new Range(0.05f, 3f); //Set the range
			flightSlider.SliderStep = 0.05f; //Set the step size
			flightSlider.Value = 5f; //Set initial value
			flightSlider.onInteraction += delegate (float slider) { DebugCommands.Instance.SetCam(new string[] { "-wheel", slider.ToString() }); };
			flightSlider.gameObject.SetActive(false);
			output.SaveElement("wheel", flightSlider); //Save the UIElement

			//Camera type explorer
			UIListExplorer cameraModes = UIFactory.Instance.CreateListExplorer(FirstCol + 2f, 4f, output.transform, "Camera Modes"); //Create the UIElement
			cameraModes.AllowLooping = true;
			cameraModes.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			cameraModes.ExplorerArray = new string[] { "Default", "First Person", "Third Person", "Free Camera" }; //Apply the list
			cameraModes.ScaleBackground(1.3f); //Scale the background to accomodate longer names
			cameraModes.Title.ScaleBackground(new Vector2(1f, 0.8f));
			output.SaveElement("mode", cameraModes);
			cameraModes.onInteraction += delegate (bool rightArrow, string stringValue, int index)
			{
				flightSlider.gameObject.SetActive(index == 3);
				switch (index)
				{
					case 0:
						DebugCommands.Instance.SetCam(new string[] { "-default" });
						info.UIName = ModText.WrapText(ModText.GetString("pausemenu_camera_default"), wrapSize);
						break;
					case 1:
						DebugCommands.Instance.SetCam(new string[] { "-first" });
						info.UIName = ModText.WrapText(ModText.GetString("pausemenu_camera_firstperson"), wrapSize);
						break;
					case 2:
						DebugCommands.Instance.SetCam(new string[] { "-third" });
						info.UIName = ModText.WrapText(ModText.GetString("pausemenu_camera_thirdperson"), wrapSize);
						break;
					case 3:
						DebugCommands.Instance.SetCam(new string[] { "-free" });
						info.UIName = ModText.WrapText(ModText.GetString("pausemenu_camera_free"), wrapSize);
						break;
					default:
						break;
				}
				fovSlider.Value = DebugCommands.Instance.cam_fov;
			};

			//Reset
			UIButton reset = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, LastCol, -3.5f, output.transform, "Reset");
			reset.ScaleBackground(new Vector2(1f, 2f), Vector2.one);
			reset.onInteraction += delegate ()
			{
				DebugCommands.Instance.SetCam(new string[] { "-default", "-fov", "-sens", "-unlockvertical", "-wheel" });
				DebugCommands.Instance.optional_cam_fov = 65f;
				cameraModes.IndexValue = DebugCommands.Instance.fpsmode;
				info.UIName = ModText.WrapText(ModText.GetString("pausemenu_camera_default"), wrapSize);

				fovSlider.Value = DebugCommands.Instance.cam_fov;
				sensSlider.Value = DebugCommands.Instance.cam_sens;
				lockVertical.Value = DebugCommands.Instance.lock_vertical;
				flightSlider.Value = DebugCommands.Instance.cam_accel;
				flightSlider.gameObject.SetActive(false);
			};

			return output;
		}

		//Info Menu
		static UIScreen InfoMenu()
		{
			// Create a base window, which contains a title and a back button. Name it (or not and leave it with default name)
			UIScreen output = UIScreen.CreateBaseScreen("Info");

			//Dummy button
			UIButton dummy = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 0f, 20f, output.transform);

			//Background
			UIBigFrame background = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, MidCol, 1.5f, output.transform);
			background.name = "Background";
			background.ScaleBackground(new Vector2(1f, 2f));
			background.transform.localPosition += new Vector3(0f, 0f, 2f); //Send to the background

			//Splash
			UIImage splash = UIFactory.Instance.CreateImage("e2dsplash", MidCol, 3.25f, output.transform);
			float scaleFactor = 4f;
			Vector3 originalScale = new Vector3(580f, 273f);
			Vector3 newScale = originalScale.normalized * scaleFactor;
			splash.transform.localScale = newScale;

			//Credits info
			UITextFrame credits = UIFactory.Instance.CreateTextFrame(MidCol, 0.75f, output.transform, "Game by: Ludosity\nModded by: Chris is Awesome & Matra");
			//UITextFrame credits = UIFactory.Instance.CreateTextFrame(MidCol + 3f, 3.25f, output.transform, "Game by:Ludosity\n\nModded by:\nChris is Awesome & Matra");
			credits.ScaleBackground(Vector2.zero);
			credits.transform.localScale = Vector3.one * 1.5f;

			//Github button
			UIGFXButton github = UIFactory.Instance.CreateGFXButton("github", MidCol + 6f, 4f, output.transform, "\nMod\nHomepage");
			github.transform.localScale = Vector3.one * 0.7f;
			github.onInteraction += delegate ()
			{
				Application.OpenURL("https://github.com/Chris-Is-Awesome/Extra-2-Dew");
			};

			//Ludosity button
			UIGFXButton ludosity = UIFactory.Instance.CreateGFXButton("ludosity", MidCol - 6f, 4f, output.transform, "\nLudosity\nHomepage");
			ludosity.transform.localScale = Vector3.one * 0.7f;
			ludosity.ScaleBackground(Vector2.one * 1.1f);
			ludosity.onInteraction += delegate ()
			{
				Application.OpenURL("https://ludosity.com/");
			};

			//Gamemode
			UITextFrame gamemode = UIFactory.Instance.CreateTextFrame(MidCol, -1f, output.transform);
			gamemode.ScaleBackground(Vector2.zero);
			gamemode.transform.localScale = Vector3.one * 1.3f;
			string gameModeString = "Vanilla";
			if (ModSaver.LoadStringFromFile("mod", "isheartrush") == "1") { gameModeString = "Heart Rush"; }
			if (ModSaver.LoadStringFromFile("mod", "isbossrush") == "1") { gameModeString = "Boss Rush"; }
			if (ModSaver.LoadStringFromFile("mod", "isdungeonrush") == "1") { gameModeString = "Dungeon Rush"; }
			gameModeString = "Game mode: " + gameModeString;
			if (ModSaver.LoadStringFromFile("mod", "randomizer_doors") == "1")
			{
				gameModeString += "\nDoors randomized. Seed: " + ModSaver.LoadStringFromFile("mod", "randomizer_doorsseed");
			}
			gamemode.UIName = gameModeString;

			return output;
		}

        //======================
        //Screen builder functions - MAIN MENU
        //======================

        //Modes screen
        static UIScreen NewGameModes()
		{
			UIScreen output = UIScreen.CreateBaseScreen("Modes");
			output.name = "E2D_modes";

			//Accomodate back button and place confirm button
			float backConfirmDistance = 3f;
			Vector3 backBtnPos = output.BackButton.transform.localPosition;
			output.BackButton.transform.localPosition += new Vector3(-backConfirmDistance, 0f, 0f);
			UIButton confirmbutton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, backConfirmDistance, backBtnPos.y, output.transform);
			output.SaveElement("confirm", confirmbutton);

			//Dummy checkbox (so randomize is not selected at the start
			UICheckBox dumy = UIFactory.Instance.CreateCheckBox(MidCol - 3f, 15f, output.transform, "Randomize doors");

			//Randomize checkbox
			UICheckBox randomizer = UIFactory.Instance.CreateCheckBox(MidCol - 3f, 5f, output.transform, "Randomize doors");
			randomizer.ScaleBackground(1.6f, Vector3.one);
			output.SaveElement("randomizer", randomizer);

			//Randomize help message
			UIBigFrame randoMessage = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 2, -3.5f, randomizer.transform);
			randoMessage.transform.localPosition += new Vector3(0f, 0f, -1f);
			randoMessage.ScaleBackground(new Vector2(1.7f, 1.65f));
			string randoText = "EXPERT PLAYERS ONLY\n\nRandomize all scene transitions while still making the game beatable. Randomization affects caves, dungeons, overworld connections and some secret places. Hidden entrances to caves will be randomized, such as the grass patch in fancy ruins and jenny berry's home.\n\nConnections left out the randomization: The grand library, all mechanic dungeons, the dream world, the secret remedy and any one way connection. The cheat sheet can be found in the 'extra2dew\\randomizer' folder.";
			randoMessage.UIName = ModText.WrapText(randoText, 30f);
			randoMessage.gameObject.SetActive(false);
			GuiSelectionObject buttonListSelection = randomizer.gameObject.transform.Find("ModUIElement").GetComponent<GuiSelectionObject>();
			bool oldNpcListstate = false;
			randomizer.onUpdate += delegate ()
			{
				if (buttonListSelection.IsSelected == oldNpcListstate) { return; }
				oldNpcListstate = buttonListSelection.IsSelected;
				randoMessage.gameObject.SetActive(oldNpcListstate);
			};

			//Seed
			UITextFrame seed = UIFactory.Instance.CreateTextFrame(MidCol + 3f, 5f, output.transform);
			seed.UIName = DoorsRandomizer.Instance.GetRandomSeed();
			output.SaveElement("seed", seed);

			//Reroll seed
			UIButton rerollSeed = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, MidCol + 1.5f, 4f, output.transform, "New seed");
			rerollSeed.ScaleBackground(new Vector2(0.8f, 1f), Vector2.one * 0.7f);
			rerollSeed.onInteraction += delegate ()
			{
				seed.UIName = DoorsRandomizer.Instance.GetRandomSeed();
			};

			//Paste seed
			UIButton pasteSeed = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, MidCol + 4.5f, 4f, output.transform, "Paste seed");
			pasteSeed.ScaleBackground(new Vector2(0.8f, 1f), Vector2.one * 0.7f);
			pasteSeed.onInteraction += delegate ()
			{
				seed.UIName = DoorsRandomizer.Instance.FixSeed(GUIUtility.systemCopyBuffer);
			};

			//Info box
			UIBigFrame info = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, MidCol - 2.5f, -0.5f, output.transform);
			info.ScaleBackground(new Vector2(1f, 1.4f));

			//Heart rush difficulty
			UIListExplorer hsDifficulty = UIFactory.Instance.CreateListExplorer(MidCol + 4f, -0.5f, output.transform, "Difficulty");
			hsDifficulty.Title.ScaleBackground(new Vector2(0.8f, 0.8f));
			hsDifficulty.ExplorerArray = new string[] { "I cannot see", "Very easy", "Easy", "Default", "Hard", "Very Hard", "ReallyJoel's Dad" };
			hsDifficulty.IndexValue = 3;
			hsDifficulty.gameObject.SetActive(false);
			output.SaveElement("hsdifficulty", hsDifficulty);

			//Mode selector
			UIListExplorer modeSelector = UIFactory.Instance.CreateListExplorer(MidCol, 2.5f, output.transform, "Game Mode");
			modeSelector.Title.ScaleBackground(new Vector2(0.8f, 0.8f));
			modeSelector.AllowLooping = true;
			modeSelector.ExplorerArray = new string[] { "Default", "Heart Rush", "Boss Rush", "Dungeon Rush" };
			modeSelector.onInteraction += delegate (bool arrow, string word, int index)
			{
				string infoText;
				bool showDifficulty = false;
				switch (index)
				{
					case 0:
						infoText = "Vanilla game";
						break;
					case 1:
						infoText = "Instead of damage, you lose max HP. If you run out of HP, the game is over and the profile is deleted.\n\nStarts with a large health pool and crayon boxes give additional heart pieces instead of 1. Can be combined with the randomizer.";
						showDifficulty = true;
						break;
					case 2:
						infoText = "Face a gauntlet of all the bosses in the game.\n\nYour items loadout will be improved after each defeated boss.";
						break;
					case 3:
						infoText = "Take a tour through all the dungeons in the game and finish them as quickly as possible.\n\nAfter defeating a dungeon, you will be teleported to the next one.";
						break;
					default:
						infoText = "";
						break;
				}
				hsDifficulty.gameObject.SetActive(showDifficulty);
				info.UIName = ModText.WrapText(infoText, 16.5f);
			};
			modeSelector.Trigger();
			output.SaveElement("modeselector", modeSelector);

			return output;
		}
	}
}
