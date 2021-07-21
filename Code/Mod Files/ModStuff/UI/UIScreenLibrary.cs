using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using ModStuff.CreativeMenu;
using ModStuff.ItemRandomizer;
using ModStuff.Options;

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
				//output.Add("newgamemodeselect", new UIScreenHandler(NewGameModeSelect));
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
                output.Add("creativemenu", new UIScreenHandler(CreativeMenu));
                output.Add("randomizedkeysmenu", new UIScreenHandler(RandomizedKeysMenu));
                //output.Add("modoptions", new UIScreenHandler(ModSettings));
            }
			return output;
		}

		static float ColDistance { get { return 3.75f; } }
		static float AspectRatio { get { return (float)Screen.width / (float)Screen.height; } }

		public static float FirstCol { get { return -AspectRatio * ColDistance; } }
        public static float MidCol { get { return 0f; } }
        public static float LastCol { get { return AspectRatio * ColDistance; } }

        //======================
        //Screen builder functions - PAUSE MENU
        //======================
        //Test Chamber Menu
        static UIScreen TestChamberMenu()
        {
           UIScreen output = UIScreen.CreateBaseScreen("Test Chamber");

            //UIListExplorer list = UIFactory.Instance.CreateListExplorer(0f, 0f, output.transform, "chanchan");
            //UITextFrame frame = UIFactory.Instance.CreatePopupFrame(0f, 3f, list, output.transform, "testtest");


            return output;
            
            //UIScreen output = ItemRandomizerGM.Instance.GetDungeonKeyScreen();

            //return output;
        }

        //Scripts Menu
        static UIScreen ScriptsMenu()
        {
            UIScreen output = UIScreen.CreateBaseScreen("Scripts");

            //Display
            UITextFrame selectedScript = UIFactory.Instance.CreateTextFrame(2f, 4f, output.transform);
            selectedScript.transform.localScale *= 1.2f;

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
            help.ScaleBackground(new Vector2(0.2f, 1f));
            UITextFrame helpHelp = UIFactory.Instance.CreatePopupFrame(0f, 1.5f, help, output.transform, "");
            helpHelp.ScaleBackground(new Vector2(2f, 4.2f));
            helpHelp.ScaleText(0.8f);
            helpHelp.WriteText("Run scripts located in ID2Data/extra2dew/scripts. Scripts are groups of debug menu commands ran in sequence, complex effects can be achieved by them.\n\nTo run a script, select it on the list and press the 'Run' button. Pressing 'Run OnLoad' will mark one script to be run each time a new scene is loaded.");
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

			UIGFXButton powerupMenu = UIFactory.Instance.CreateGFXButton("powerup", -6f, 3f, output.transform, "Player");
			output.SaveElement("powerup", powerupMenu);

			UIGFXButton itemsMenu = UIFactory.Instance.CreateGFXButton("itemsmenu", -3f, 3f, output.transform, "Items");
			output.SaveElement("items", itemsMenu);

			UIGFXButton spawnMenu = UIFactory.Instance.CreateGFXButton("npcs", 0f, 3f, output.transform, "NPCs");
			output.SaveElement("spawn", spawnMenu);

			UIGFXButton cameraMenu = UIFactory.Instance.CreateGFXButton("camera", 3f, 3f, output.transform, "Camera");
			output.SaveElement("camera", cameraMenu);

			UIGFXButton worldMenu = UIFactory.Instance.CreateGFXButton("world", 6f, 3f, output.transform, "World");
			worldMenu.ScaleBackground(new Vector2(1.2f, 1.2f));
			output.SaveElement("world", worldMenu);

            UIGFXButton scripstMenu = UIFactory.Instance.CreateGFXButton("scripts", -6f, 0f, output.transform, "Scripts");
            output.SaveElement("scripts", scripstMenu);

            UIGFXButton creativeMenu = UIFactory.Instance.CreateGFXButton("cmenu", -3f, 0f, output.transform, "Creative Menu");
            creativeMenu.AutoTextResize = false;
            output.SaveElement("creative", creativeMenu);

            UIGFXButton infoMenu = UIFactory.Instance.CreateGFXButton("info", 6f, 0f, output.transform, "Info");
			output.SaveElement("info", infoMenu);

			UIGFXButton gameOptionsMenu = UIFactory.Instance.CreateGFXButton("gameoptions", 3f, 0f, output.transform, "Game Options");
			output.SaveElement("gameoptions", gameOptionsMenu);
            gameOptionsMenu.gameObject.SetActive(false);

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

			/*

			// Skip Splash screen?
			UICheckBox skipSplash = UIFactory.Instance.CreateCheckBox(LastCol, 0f, output.transform, "Skip splash screen");
			skipSplash.transform.localPosition += new Vector3(xPosLeftColumn, yPosRow1, zPos);
			skipSplash.ScaleBackground(1.75f, Vector3.one);
			skipSplash.onInteraction += delegate (bool enable)
			{
				ModStuff.Options.GameOptions opts = ModStuff.Options.GameOptions.Instance;
				opts.SkipSplash = enable;
				ModSaverNew.SaveToCustomFile<int>(filePath, nameof(opts.SkipSplash), Convert.ToInt32(enable));
			};
			output.SaveElement("skipSplash", skipSplash);
			popupText = "Skip the Ludosity splash scren on startup of the game.";
			UITextFrame popupSkipSplash = UIFactory.Instance.CreatePopupFrame(popupXPos, popupYPos, skipSplash, output.transform, popupText);
			popupSkipSplash.ScaleBackground(popupScale);
			
			*/

			/*

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

			// Skip all delays
			UICheckBox noDelays = UIFactory.Instance.CreateCheckBox(LastCol, 0f, output.transform, "No Delays");
			fasterTransitions.transform.localPosition += new Vector3(xPosLeftColumn, yPosRow2, zPos);
			fasterTransitions.ScaleBackground(1.75f, Vector3.one);
			fasterTransitions.onInteraction += delegate (bool enable)
			{
				GameOptions opts = ModStuff.GameOptions.Instance;
				opts.NoDelays = enable;
				ModSaverNew.SaveToCustomFile<int>(filePath, nameof(opts.FasterTransitions), Convert.ToInt32(enable));
			};
			output.SaveElement("fasterTransitions", fasterTransitions);
			popupText = "Speed up level and room transitions.";
			UITextFrame popupFasterTransitions = UIFactory.Instance.CreatePopupFrame(popupXPos, popupYPos, fasterTransitions, output.transform, popupText);
			popupFasterTransitions.ScaleBackground(popupScale);

			*/

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
			goTravel.ScaleBackground(new Vector2(0.7f, 1f));
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

            //Sky color
            //Holder
            GameObject skyColorHolder = new GameObject("SkyColorHolder");
            skyColorHolder.transform.SetParent(output.transform, false);
            skyColorHolder.transform.localScale = Vector3.one;
            skyColorHolder.transform.localPosition = new Vector3(LastCol, 4.5f, 0f);
            //Checkbox
            UICheckBox boolColor = UIFactory.Instance.CreateCheckBox(0f, 0.3f, skyColorHolder.transform, "Colorize the sky?");
            boolColor.transform.localPosition += new Vector3(0f, 0f, -0.2f);
            output.SaveElement("skybool", boolColor);
            //List explorer
            UIListExplorer colorExplorer = UIFactory.Instance.CreateListExplorer(0f, -0.75f, skyColorHolder.transform);
            colorExplorer.ScaleBackground(0.7f);
            colorExplorer.Title.gameObject.SetActive(false);
            colorExplorer.AllowLooping = true;
            colorExplorer.ExplorerArray = new string[] { "Red", "Green", "Blue" };
            colorExplorer.ScaleBackground(0.75f);
            colorExplorer.gameObject.SetActive(false);
            output.SaveElement("skyexplorer", colorExplorer);
            //Slider
            UISlider colorSlider = UIFactory.Instance.CreateSlider(0f, -1.5f, skyColorHolder.transform, "");
            colorSlider.SetSlider(0f, 1f, 0.01f, 1f);
            colorSlider.gameObject.SetActive(false);
            output.SaveElement("skyslider", colorSlider);
            colorExplorer.onInteraction += delegate (bool rightArrow, string stringValue, int index)
            {
                float sliderValue;

                if (index == 0) sliderValue = CameraSkyColor.Instance.red;
                else if (index == 1) sliderValue = CameraSkyColor.Instance.green;
                else sliderValue = CameraSkyColor.Instance.blue;

                colorSlider.Value = sliderValue;
            };
            boolColor.onInteraction += delegate (bool useColor)
            {
                colorSlider.gameObject.SetActive(useColor);
                colorExplorer.gameObject.SetActive(useColor);
                CameraSkyColor.Instance.UseForceColor = useColor;
            };
            colorSlider.onInteraction += delegate (float sliderValue)
            {
                if (colorExplorer.IndexValue == 0) CameraSkyColor.Instance.red = sliderValue;
                else if (colorExplorer.IndexValue == 1) CameraSkyColor.Instance.green = sliderValue;
                else CameraSkyColor.Instance.blue = sliderValue;
            };

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

			//Reset
			UIButton reset = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, LastCol, -3.5f, output.transform, "Reset");
			reset.ScaleBackground(new Vector2(1f, 2f));
			reset.onInteraction += delegate ()
			{
				timeFlow.Trigger(4f);

                CameraSkyColor.Instance.red = 1f;
                CameraSkyColor.Instance.green = 1f;
                CameraSkyColor.Instance.blue = 1f;
                boolColor.Trigger(false);
                colorExplorer.IndexValue = 0;
                colorSlider.Value = CameraSkyColor.Instance.red;
            };

			return output;
		}

        //Creative mode menu
        static UIScreen CreativeMenu()
        {
            return CMenuUI.CMenuBuilder();
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
			UIScreen output = UIScreen.CreateBaseScreen("Player");

            //Holders for powerups controls
            GameObject powerupsControls = new GameObject("PowerupsControls");
            powerupsControls.transform.SetParent(output.transform, false);
            powerupsControls.transform.localScale = Vector3.one;

            //Holders for quick selection
            GameObject gearControl = new GameObject("GearSelection");
            gearControl.transform.SetParent(output.transform, false);
            gearControl.transform.localScale = Vector3.one;

            //Quick menu controls
            GameObject miscControl = new GameObject("MiscConstrols");
            miscControl.transform.SetParent(output.transform, false);
            miscControl.transform.localScale = Vector3.one;

            //Menu Switch
            UIListExplorer menuSwitch = UIFactory.Instance.CreateListExplorer(0f, 5.0f, output.transform);
            menuSwitch.transform.localScale *= 1.5f;
            menuSwitch.Title.gameObject.SetActive(false);
            menuSwitch.AllowLooping = true;
            menuSwitch.ExplorerArray = new string[] { "Powerups", "Gear", "Misc" };
            menuSwitch.onInteraction += delegate (bool rightArrow, string stringValue, int indexValue)
            {
                powerupsControls.SetActive(indexValue == 0);
                gearControl.SetActive(indexValue == 1);
                miscControl.SetActive(indexValue == 2);
            };

            //POWERUPS
            //--------------------------------------
            //Health
            GameObject hpParent = new GameObject();
            hpParent.transform.SetParent(powerupsControls.transform, false);
            hpParent.transform.localPosition = new Vector3(0f, 3f, 0f);
            UIListExplorer hpExplorer = UIFactory.Instance.CreateListExplorer(MidCol, 0f, hpParent.transform, "Health");
            hpExplorer.ScaleBackground(1.2f);
            hpExplorer.ExplorerArray = new string[] { "Set to 1", "Remove 5 hearts", "Remove 1 heart", "Remove 1/4 hearts", "Add 1/4 hearts", "Add 1 heart", "Add 5 hearts" };
            hpExplorer.IndexValue = 5;
            output.SaveElement("health", hpExplorer);
            UIButton acceptHP = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, MidCol + 1.5f, -0.90f, hpParent.transform);
            acceptHP.ScaleBackground(new Vector2(0.75f, 1f), Vector2.one * 0.65f);
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
            healHP.ScaleBackground(new Vector2(0.75f, 1f), Vector2.one * 0.65f);
            healHP.onInteraction += delegate ()
            {
                DebugCommands.Instance.SetHP(new string[] { "-full" });
            };

            //God
            UICheckBox godCheckbox = UIFactory.Instance.CreateCheckBox(FirstCol, 1.25f, powerupsControls.transform, "God mode");
			godCheckbox.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.God(new string[] { box ? "1" : "0" });
			};
			output.SaveElement("god", godCheckbox);

			//Like a boss
			UICheckBox bossCheckbox = UIFactory.Instance.CreateCheckBox(FirstCol, 0f, powerupsControls.transform, "Like a boss");
			bossCheckbox.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.LikeABoss(new string[] { box ? "1" : "0" });
			};
			output.SaveElement("likeaboss", bossCheckbox);
            UITextFrame bossMessage = UIFactory.Instance.CreatePopupFrame(FirstCol, -1.5f, bossCheckbox, powerupsControls.transform, "Destroy enemies\nin one hit");
            bossMessage.transform.localPosition += new Vector3(0f, 0f, -1.1f);
			bossMessage.ScaleBackground(new Vector2(1.5f, 1.7f));

			//Noclip
			UICheckBox noclipCheckbox = UIFactory.Instance.CreateCheckBox(FirstCol, -1.25f, powerupsControls.transform, "Noclip");
			noclipCheckbox.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.NoClip(new string[] { box ? "1" : "0" });
			};
            UITextFrame noclipMessage = UIFactory.Instance.CreatePopupFrame(FirstCol, -2.75f, noclipCheckbox, powerupsControls.transform, "Disable collision, some\nenemies will ignore you");
            noclipMessage.transform.localPosition += new Vector3(0f, 0f, -1.1f);
			noclipMessage.ScaleBackground(new Vector2(1.5f, 1.7f));
			output.SaveElement("noclip", noclipCheckbox);

            //Speed
            UISlider speedSlider = UIFactory.Instance.CreateSlider(LastCol + 0.2f, 1f, powerupsControls.transform, "Run speed"); //Create the slider
            speedSlider.SliderRange = new Range(0.5f, 25f); //Set the range
            speedSlider.SliderStep = 0.5f; //Set the step size
            speedSlider.Value = 5f; //Set initial value
            speedSlider.onInteraction += delegate (float speed)
            {
                DebugCommands.Instance.SetSpeed(new string[] { speed.ToString() });
            };
            output.SaveElement("speed", speedSlider);

            //Knockback
            UISlider knockback = UIFactory.Instance.CreateSlider(LastCol + 0.2f, -0.5f, powerupsControls.transform, "Knockback");
            knockback.SetSlider(-1f, 50f, 0.5f, 1f);
            knockback.onInteraction += delegate (float slider)
            {
                DebugCommands.Instance.Knockback(new string[] { slider.ToString() });
            };
            output.SaveElement("knockback", knockback);

            //Gear
            //--------------------------------------
            //Super iceblocks
            UICheckBox superIce = UIFactory.Instance.CreateCheckBox(FirstCol, 2.5f, gearControl.transform, "Infinite Ice");
			superIce.ScaleBackground(1f, Vector3.one);
			superIce.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-ice", box ? "1" : "0" });
			};
			output.SaveElement("superice", superIce);

			//Super dynamite
			UICheckBox superDynamite = UIFactory.Instance.CreateCheckBox(FirstCol, 1.25f, gearControl.transform, "Infinite dynamites");
			superDynamite.ScaleBackground(1.3f, Vector3.one);
			superDynamite.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-dynamite", box ? "1" : "0" });
			};
			output.SaveElement("superdynamite", superDynamite);

			//No icegrid
			UICheckBox freeIce = UIFactory.Instance.CreateCheckBox(FirstCol, 0f, gearControl.transform, "Gridless Ice");
			freeIce.ScaleBackground(1.1f, Vector3.one);
			freeIce.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-icegrid", box ? "1" : "0" });
			};
			output.SaveElement("freeice", freeIce);

			//Super attack
			UICheckBox superAttack = UIFactory.Instance.CreateCheckBox(FirstCol, -1.25f, gearControl.transform, "Super Attack");
			superAttack.ScaleBackground(1.2f, Vector3.one);
			superAttack.onInteraction += delegate (bool box)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-attack", box ? "1" : "0" });
			};
			output.SaveElement("superattack", superAttack);
            UITextFrame attackmessage = UIFactory.Instance.CreatePopupFrame(FirstCol, 0.25f, superAttack, gearControl.transform, "Requires EFCS");
            attackmessage.transform.localPosition += new Vector3(0f, 0f, -1.1f);
			attackmessage.ScaleBackground(new Vector2(1.5f, 1.7f));

			//Super attack shake
			UICheckBox noShake = UIFactory.Instance.CreateCheckBox(FirstCol, -2.50f, gearControl.transform, "Less shake");
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

			//Attack range
			UISlider attackRange = UIFactory.Instance.CreateSlider(LastCol + 0.2f, 3.5f, gearControl.transform, "Melee Range"); //Create the slider
			attackRange.SliderRange = new Range(0f, 50f); //Set the range
			attackRange.SliderStep = 0.5f; //Set the step size
			attackRange.Value = 0f; //Set initial value
			attackRange.onInteraction += delegate (float range)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-range", range.ToString() });
			};
			output.SaveElement("attackrange", attackRange);

			//Dynamite range
			UISlider dynaRange = UIFactory.Instance.CreateSlider(LastCol + 0.2f, 2f, gearControl.transform, "Dynamite Range"); //Create the slider
			dynaRange.SliderRange = new Range(0f, 100f); //Set the range
			dynaRange.SliderStep = 0.5f; //Set the step size
			dynaRange.Value = 1.7f; //Set initial value
			dynaRange.onInteraction += delegate (float radius)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-radius", radius.ToString() });
			};
			output.SaveElement("dynarange", dynaRange);

			//Multi-projectile
			UISlider multiProj = UIFactory.Instance.CreateSlider(LastCol + 0.2f, 0.5f, gearControl.transform, "Multi-Projectile"); //Create the slider
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
			UIListExplorer dynaFuse = UIFactory.Instance.CreateListExplorer(LastCol, -1.75f, gearControl.transform, "Dynamite fuse");
			dynaFuse.ExplorerArray = new string[] { "Instant", "0.5 seconds", "Default (1.5)", "5 seconds", "20 seconds", "1 minute", "10 minutes", "5 years" };
			dynaFuse.IndexValue = 2;
            dynaFuse.ScaleTitleBackground(1.8f);// Title.ScaleBackground(new Vector2(0.9f, 0.8f));
			float[] dynaArray = new float[] { 0f, 0.5f, 1.5f, 5f, 20f, 60f, 600f, 157680000f };
			dynaFuse.onInteraction += delegate (bool rightArrow, string stringValue, int indexValue)
			{
				DebugCommands.Instance.AtkMod(new string[] { "-fuse", dynaArray[indexValue].ToString() });
			};
			output.SaveElement("fuse", dynaFuse);

            //MISC
            //--------------------------------------
            //Outfit
            UIListExplorer outfit = UIFactory.Instance.CreateListExplorer(LastCol, 3.05f, miscControl.transform, "Outfit");
			outfit.ExplorerArray = new string[] { "Ittle", "Tippsie", "ID1", "Jenny", "Swimsuit", "Fierce diety", "CCN", "Delinquent", "Frog", "That Guy", "Jenny Berry" };
			outfit.IndexValue = 0;
			outfit.AllowLooping = true;
			outfit.onInteraction += delegate (bool rightArrow, string stringValue, int indexValue)
			{
				DebugCommands.Instance.SetOutfit(new string[] { ((int)indexValue).ToString() });
			};
			output.SaveElement("outfit", outfit);

			//Player Scale
			UIVector3 playerScale = UIFactory.Instance.CreateVector3(LastCol, 0.6f, miscControl.transform, "Player size");
            playerScale.Explorer.ScaleTitleBackground(1.4f); //Title.ScaleBackground(new Vector2(0.7f, 0.8f));
			playerScale.onInteraction += delegate (Vector3 vector)
			{
				DebugCommands.Instance.SetSize(new string[] { "self", vector.x.ToString(), vector.y.ToString(), vector.z.ToString() });
			};
			output.SaveElement("scale", playerScale);

            //ShowHud
            UICheckBox showhud = UIFactory.Instance.CreateCheckBox(LastCol, -0.6f, miscControl.transform, "Hide HUD");
            showhud.onInteraction += delegate (bool box)
            {
                DebugCommands.Instance.ShowHUD(new string[] { box ? "0" : "1" });
            };
            output.SaveElement("showhud", showhud);

            //Reset
            UIButton reset = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, LastCol, -3.5f, output.transform, "Reset");
			reset.ScaleBackground(new Vector2(1f, 2f));
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
                knockback.Trigger(1f);
                showhud.Trigger(false);
            };

            menuSwitch.Trigger();

            return output;
		}

		//Spawn menu
		static UIScreen SpawnMenu()
		{
            return ModSpawner.SpawnMenu();
		}

		//Camera Menu
		static UIScreen CameraMenu()
		{
			// Create a base window, which contains a title and a back button. Name it (or not and leave it with default name)
			UIScreen output = UIScreen.CreateBaseScreen("Camera");

			//Info panel
			UIBigFrame info = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, FirstCol + 2f, 0.75f, output.transform);
			info.ScaleBackground(new Vector2(0.55f, 1.3f));
			output.SaveElement("info", info);

			//Field of view
			UISlider fovSlider = UIFactory.Instance.CreateSlider(LastCol + 0.2f, 4.5f, output.transform, "FOV"); //Create the slider
			fovSlider.SliderRange = new Range(10f, 120f); //Set the range
			fovSlider.SliderStep = 1f; //Set the step size
			fovSlider.Value = 65f; //Set initial value
			fovSlider.DisplayInteger = true;
			fovSlider.onInteraction += delegate (float fov) { DebugCommands.Instance.SetCam(new string[] { "-fov", fov.ToString() }); };
			output.SaveElement("fov", fovSlider); //Save the UIElement

			//Sensitivity
			UISlider sensSlider = UIFactory.Instance.CreateSlider(LastCol + 0.2f, 3f, output.transform, "Sensitivity"); //Create the slider
			sensSlider.SliderRange = new Range(0.2f, 20f); //Set the range
			sensSlider.SliderStep = 0.2f; //Set the step size
			sensSlider.Value = 5f; //Set initial value
			sensSlider.onInteraction += delegate (float fov) { DebugCommands.Instance.SetCam(new string[] { "-sens", fov.ToString() }); };
			output.SaveElement("sens", sensSlider); //Save the UIElement

			//Lock vertical
			UICheckBox lockVertical = UIFactory.Instance.CreateCheckBox(LastCol, 1.5f, output.transform, "Lock Vertical");
			lockVertical.onInteraction += delegate (bool lockVert) { DebugCommands.Instance.SetCam(new string[] { lockVert ? "-lockvertical" : "-unlockvertical" }); };
			lockVertical.ScaleBackground(1.3f, Vector3.one);
			output.SaveElement("lockvert", lockVertical); //Save the UIElement

            //Unlock player
            UIListExplorer unlockPlayer = UIFactory.Instance.CreateListExplorer(LastCol,-1.75f,output.transform, "Player controls");
            unlockPlayer.ScaleTitleBackground(1.6f); //Title.ScaleBackground(new Vector2(0.8f, 0.8f));
            unlockPlayer.AllowLooping = true;
            unlockPlayer.ExplorerArray = new string[] { "Locked", "FPS controls", "Default controls", "Everything locked" };
            unlockPlayer.onInteraction += delegate (bool rightArrow, string stringValue, int index) { DebugCommands.Instance.SetCam(new string[] { "-fcmovement", index.ToString() }); };
            unlockPlayer.gameObject.SetActive(false);
            output.SaveElement("unlockplayer", unlockPlayer); //Save the UIElement

			//Camera type explorer
			UIListExplorer cameraModes = UIFactory.Instance.CreateListExplorer(FirstCol + 2f, 4f, output.transform, "Camera Modes"); //Create the UIElement
			cameraModes.AllowLooping = true;
			cameraModes.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
			cameraModes.ExplorerArray = new string[] { "Default", "First Person", "Third Person", "Free Camera" }; //Apply the list
			cameraModes.ScaleBackground(1.3f); //Scale the background to accomodate longer names
            cameraModes.ScaleTitleBackground(2f); //Title.ScaleBackground(new Vector2(1f, 0.8f));
			output.SaveElement("mode", cameraModes);

			//Reset
			UIButton reset = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, LastCol, -3.5f, output.transform, "Reset");
			reset.ScaleBackground(new Vector2(1f, 2f));
			reset.onInteraction += delegate ()
			{
				DebugCommands.Instance.SetCam(new string[] { "-default", "-fov", "-sens", "-unlockvertical", "-wheel", "-reset" });
				ModCamera.Instance.optional_cam_fov = 65f;
                cameraModes.Trigger(false, ModCamera.Instance.fpsmode);

				fovSlider.Value = ModCamera.Instance.cam_fov;
				sensSlider.Value = ModCamera.Instance.cam_sens;
				lockVertical.Value = ModCamera.Instance.lock_vertical;
                unlockPlayer.IndexValue = ModCamera.Instance.cam_fc_unlock_player;
			};

            //RefreshTextButton
            //This is a trick to refresh the text window without needing extra stuff
            UIButton camTextButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, LastCol, -500.5f, output.transform, "This doesnt exist");
            camTextButton.onInteraction += delegate ()
            {
                string cameraInfo;
                switch (cameraModes.IndexValue)
                {
                    case 0:
                        cameraInfo = "Default camera\n\n";
                        break;
                    case 1:
                        cameraInfo = "First person camera \n\nPlay from Ittle's perspective. Look around using the mouse, move with the WASD keys and click to attack. Change your weapons with 1, 2, 3 and 4 or using the scroll wheel\n\nThe game needs the mouse to stay still for a moment to calibrate the camera";
                        break;
                    case 2:
                        cameraInfo = "Third person camera \n\nOrbit around Ittle. Look around using the mouse, move with the WASD keys and click to attack. Change your weapons with 1, 2, 3 and 4 or using the scroll wheel. Hold right click and scroll to zoom in/out\n\nThe game needs the mouse to stay still for a moment to calibrate the camera";
                        break;
                    case 3:
                        cameraInfo = "Free flight camera \n\nMove around the world, take the camera anywhere. Look around using the mouse and move with the WASD keys. Use the scroll wheel to speed down/up while flying\n\nThe game needs the mouse to stay still for a moment to calibrate the camera";
                        break;
                    default:
                        cameraInfo = "";
                        break;
                }
                info.WriteText(cameraInfo);
            };
            output.SaveElement("updatetext", camTextButton);
            output.gameObject.SetActive(false);
            cameraModes.onInteraction += delegate (bool rightArrow, string stringValue, int index)
            {
                bool freeCamera = index == 3;

                unlockPlayer.gameObject.SetActive(freeCamera);
                camTextButton.Trigger();

                string cameraMode;
                switch (index)
                {
                    case 0:
                        cameraMode = "-default";
                        break;
                    case 1:
                        cameraMode = "-first";
                        break;
                    case 2:
                        cameraMode = "-third";
                        break;
                    case 3:
                        cameraMode = "-free";
                        break;
                    default:
                        cameraMode = "";
                        break;
                }
                DebugCommands.Instance.SetCam(new string[] { cameraMode });

                fovSlider.Value = ModCamera.Instance.cam_fov;
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
			UITextFrame credits = UIFactory.Instance.CreateTextFrame(MidCol, 0.35f, output.transform, "Game by: Ludosity\nModded by: Chris is Awesome & Matra\nRandomizer logic: Linkshot");
			//UITextFrame credits = UIFactory.Instance.CreateTextFrame(MidCol + 3f, 3.25f, output.transform, "Game by:Ludosity\n\nModded by:\nChris is Awesome & Matra");
			credits.ScaleBackground(new Vector2(1.5f, 1.3f));
			credits.transform.localScale *= 1.5f;

			//Github button
			UIGFXButton github = UIFactory.Instance.CreateGFXButton("github", MidCol + 6f, 4f, output.transform, "\nMod\nHomepage");
			github.transform.localScale = Vector3.one * 0.7f;
			github.onInteraction += delegate ()
			{
				Application.OpenURL("https://github.com/Chris-Is-Awesome/Extra2DewMod");
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
			UITextFrame gamemode = UIFactory.Instance.CreateTextFrame(MidCol, -1.4f, output.transform);
			gamemode.ScaleBackground(new Vector2(1.3f, 1f));
			gamemode.transform.localScale *= 1.3f;
            if(DoorRandomizerGM.Instance.IsActive || ItemRandomizerGM.Instance.IsActive)
            {
                gamemode.UIName = "Randomization seed: " + ModSaver.LoadStringFromFile("mod", "randomizer_seed");
            }
            else
            {
                gamemode.UIName = "No randomization active";
            }

			return output;
		}

        static UIScreen RandomizedKeysMenu()
        {
            return ItemRandomizerGM.Instance.GetDungeonKeyScreen();
        }

        //======================
        //Screen builder functions - MAIN MENU
        //======================

        //Modes screen
        static UIScreen NewGameModes()
		{
            return ModeControllerNew.CreateMenu();
		}
	}
}
