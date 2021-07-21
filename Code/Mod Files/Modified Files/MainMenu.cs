using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using ModStuff;
using ModStuff.ItemRandomizer;

public class MainMenu : MonoBehaviour
{
	[SerializeField]
	GameObject _layout;

	[SerializeField]
	bool _layoutIsPrefab;

	[SerializeField]
	GameVersion _gameVersion;

	[SerializeField]
	public SaverOwner _saver;

	[SerializeField]
	TextLoader _texts;

	[SerializeField]
	MappedInput _input;

	[SerializeField]
	public string _defaultStartScene;

	public string _defaultStartSpawn;

	[SerializeField]
	FadeEffectData _fadeData;

	[SerializeField]
	float _songFadeoutTime = 1f;

	[SerializeField]
	MusicLayer _songLayer;

	[SerializeField]
	VariableInfoData _saveFileValues;

	[SerializeField]
	ExprVarHolderBase _saveFileValueContext;

	[SerializeField]
	string _saveFileDungeonPath;

	[SerializeField]
	string _saveFileTimePath;

	[SerializeField]
	string _bestTimeRecordPath;

	[SerializeField]
	LeaderboardId _bestTimeBoard;

	[SerializeField]
	string _defaultName = "Ittle";

	[SerializeField]
	EnterNameMenu _enterNameMenu;

	[SerializeField]
	OptionsMenu _options;

	[SerializeField]
	SoundTestMenu _soundTestMenu;

	[SerializeField]
	GalleryMenu _galleryMenu;

	[SerializeField]
	CardsMenu _cardsMenu;

	[SerializeField]
	RoomEventObserver _onStart;

	[SerializeField]
	MainMenu.Codez[] _codez;

	MenuImpl<MainMenu> menuImpl;

	static bool printedSysInfo;

	bool waitForSync;

	static bool didInitStuff;

	public static List<string> GetSortedSaveKeys(SaverOwner saverOwner, string path)
	{
		IDataSaver saver = saverOwner.GetSaver(path, true);
		if (saver != null)
		{
			List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
			string[] allDataKeys = saver.GetAllDataKeys();
			for (int i = 0; i < allDataKeys.Length; i++)
			{
				list.Add(new KeyValuePair<string, int>(allDataKeys[i], saver.LoadInt(allDataKeys[i])));
			}
			list.Sort((KeyValuePair<string, int> a, KeyValuePair<string, int> b) => a.Value - b.Value);
			List<string> list2 = new List<string>(list.Count);
			for (int j = 0; j < list.Count; j++)
			{
				list2.Add(list[j].Key);
			}
			return list2;
		}
		return new List<string>();
	}

	public static void SetupStartGame(SaverOwner saver, TextLoader texts)
	{
		if (saver != null && texts != null)
		{
			texts.SetGlobalValue("playerName", saver.LocalFileName);
		}
	}

	bool SaveFileWarning()
	{
		return !DataFileIO.GetCurrentIO().CanCreateFiles;
	}

	DataSaverData.DebugAddData[] GetCode(string name)
	{
		if (this._codez != null)
		{
			for (int i = this._codez.Length - 1; i >= 0; i--)
			{
				if (this._codez[i].savename == name)
				{
					return this._codez[i].data;
				}
			}
		}
		return null;
	}

	bool HasBoolVar(string varPath)
	{
		if (this._saver == null)
		{
			return false;
		}
		string key;
		IDataSaver saverAndName = this._saver.GetSaverAndName(varPath, out key, true);
		return saverAndName != null && saverAndName.LoadBool(key);
	}

	bool HasAnyVar(string varPath)
	{
		if (this._saver == null)
		{
			return false;
		}
		string key;
		IDataSaver saverAndName = this._saver.GetSaverAndName(varPath, out key, true);
		return saverAndName != null && saverAndName.HasData(key);
	}

	void StartGame()
	{
		this.menuImpl.Hide();
		MainMenu.SetupStartGame(this._saver, this._texts);

		//Check for game modes
		//mc.SetupGameModes();

		//Check for door randomization and apply it if thats the case
		//DoorsRandomizer.Instance.CheckForRandomization();

		IDataSaver saver = this._saver.GetSaver("/local/start", true);
		if (saver != null)
		{
			string targetScene = saver.LoadData("level");
			string targetDoor = saver.LoadData("door");
			SceneDoor.StartLoad(targetScene, targetDoor, this._fadeData, null, null);
		}
		else
		{
			// If Boss Rush, load into first boss
			if (mc.isBossRush || mc.isDungeonRush) { SceneDoor.StartLoad("PillowFort", "PillowFortInside", _fadeData); }
			else { SceneDoor.StartLoad(this._defaultStartScene, _defaultStartSpawn, this._fadeData, null, null); }
		}
		MusicSelector.Instance.StopLayer(this._songLayer, this._songFadeoutTime);
		GameObject.Find("CommonFunctions").GetComponent<CommonFunctions>().playThisSongOnLoad = null;
	}

	public static void PrintSystemInfo(GameVersion version)
	{
		Debug.Log("SYSTEM INFO");
		Debug.Log("Game version: " + version.GetVersion());
		Debug.Log("NPOT Texture: " + SystemInfo.npotSupport);
		Debug.Log("RenderTexture: " + SystemInfo.supportedRenderTargetCount);
		Debug.Log("Shader level: " + SystemInfo.graphicsShaderLevel);
		Debug.Log("END SYSTEM INFO");
	}

	static void ApplySingleUIData(PerPlatformData.UIData data, GuiContentData inData)
	{
		switch (data.mode)
		{
			case PerPlatformData.UIDataValueMode.Int:
				{
					int num = 0;
					int.TryParse(data.value, out num);
					inData.SetValue(data.key, num);
					return;
				}
			case PerPlatformData.UIDataValueMode.Float:
				{
					float num2 = 0f;
					float.TryParse(data.value, NumberStyles.Any, CultureInfo.InvariantCulture, out num2);
					inData.SetValue(data.key, num2);
					return;
				}
			case PerPlatformData.UIDataValueMode.Bool:
				{
					bool flag = data.value == "1" || data.value.Equals("true", StringComparison.InvariantCultureIgnoreCase);
					inData.SetValue(data.key, flag);
					return;
				}
			default:
				inData.SetValue(data.key, data.value);
				return;
		}
	}

	public static void ApplyUIData(PerPlatformData.UIData[] data, GuiContentData cont)
	{
		if (data != null)
		{
			for (int i = data.Length - 1; i >= 0; i--)
			{
				MainMenu.ApplySingleUIData(data[i], cont);
			}
		}
	}

	void DoStartMenu()
	{
        //Save mapped input
        UIFactory.Instance.SetMappetInput(_input);

		StartMenu.InitGame(this._saver, this._input, this._texts);
		GuiBindInData inData = new GuiBindInData(null, null);
		GuiBindData guiBindData;

		//Build all mod screens
		UIScreen.BuildAllScreens();

		if (this._layoutIsPrefab)
		{
			guiBindData = GuiNode.CreateAndConnect(this._layout, inData);
		}
		else
		{
			guiBindData = GuiNode.Connect(this._layout, inData);
		}
		this.menuImpl = new MenuImpl<MainMenu>(this);
		this.menuImpl.AddScreen(new MainMenu.MainScreen(this, "startRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.OptionsScreen(this, "optionRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.FileSelectScreen(this, "fileSelectRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.FileStartScreen(this, "fileStartRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.NewGameScreen(this, "enterNameRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.DeleteConfirmScreen(this, "deleteRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.ExtrasScreen(this, "extrasRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.LangScreen(this, "langRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.SoundTestScreen(this, "soundTestRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.GalleryScreen(this, "galleryRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.SaveWarnScreen(this, "savewarnRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.RecordsScreen(this, "recordsRoot", guiBindData));
		this.menuImpl.AddScreen(new MainMenu.CardsScreen(this, "cardsRoot", guiBindData));

		//Mod screen
		this.menuImpl.AddScreen(new MainMenu.GameModeScreen(this, "newgamemodes", guiBindData));

		this.menuImpl.ShowFirst();
		if (this._onStart != null)
		{
			this._onStart.FireOnActivate(true);
		}
	}

	static ModeController mc;
	void Start()
	{
		GameStateNew.OnSceneLoaded(SceneManager.GetActiveScene(), false);

		_enterNameMenu._maxChars = 100; // Sets max chars for file name to allow for longer names

		mc = GameObject.Find("ModeController").GetComponent<ModeController>();

		if (!MainMenu.printedSysInfo)
		{
			MainMenu.printedSysInfo = true;
			MainMenu.PrintSystemInfo(this._gameVersion);
		}
		if (MainMenu.didInitStuff)
		{
			this.DoStartMenu();
			return;
		}
		MainMenu.didInitStuff = true;
		DataFileIO.Init();
		Clouder.Init();
		Clouder.SyncAllFiles();
		if (Clouder.IsReady)
		{
			this.DoStartMenu();
			return;
		}
		this.waitForSync = true;
	}

	void Update()
	{
		// Load straight to file if pressed
		if (ModStuff.ModVersion.IsDevBuild() && Input.GetKeyDown(KeyCode.Tab))
		{
			Utility.LoadLevel("deep19");
			return;
		}

		if (this.waitForSync && Clouder.IsReady)
		{
			this.DoStartMenu();
			this.waitForSync = false;
		}
	}

	[Serializable]
	public class Codez
	{
		public string savename;

		public DataSaverData.DebugAddData[] data;
	}

	class MainScreen : MenuScreen<MainMenu>
	{
		public MainScreen(MainMenu owner, string root, GuiBindData bindData) : base(owner, root, bindData)
		{
			MenuScreen<MainMenu>.BindClickEvent(bindData, "main.btnStart", new GuiNode.OnVoidFunc(this.ClickedNewGame));
			base.BindSwitchClickEvent(bindData, "main.btnLoad", "fileSelectRoot");
			MenuScreen<MainMenu>.BindClickEvent(bindData, "main.btnQuit", new GuiNode.OnVoidFunc(this.ClickedQuit));
			base.BindSwitchClickEvent(bindData, "main.btnOption", "optionRoot");
			base.BindSwitchClickEvent(bindData, "main.btnExtra", "extrasRoot");
			base.BindSwitchClickEvent(bindData, "main.btnLang", "langRoot");

			//Mod splash
			Transform splashParent = GameObject.Find("GuiLayout").transform.Find("Main").Find("Start").Find("Layout");
			UIImage gameModesButton = UIFactory.Instance.CreateImage("titlesplash", 1.5f, 5f, splashParent);
			float scaleFactor = 2.5f;
			Vector3 originalScale = new Vector3(400f, 83f);
			Vector3 newScale = originalScale.normalized * scaleFactor;
			gameModesButton.transform.localScale = newScale;
		}

		protected override void StandardSwitch(object ctx, string switchTo)
		{
			if (base.Root.IsIntroAnimating)
			{
				base.Root.StopAnimation();
				return;
			}
			base.StandardSwitch(ctx, switchTo);
		}

		void ClickedQuit(object ctx)
		{
			if (base.Root.IsIntroAnimating)
			{
				base.Root.StopAnimation();
				return;
			}
			Application.Quit();
		}

		void ClickedNewGame(object ctx)
		{
			if (!base.Owner.SaveFileWarning())
			{
				this.StandardSwitch(ctx, "enterNameRoot");
				return;
			}
			this.StandardSwitch(ctx, "savewarnRoot");
		}

		void GotUpdate(bool available)
		{
			this.ApplyData();
		}

		bool ShouldShowExtras()
		{
			return MainMenu.ExtrasScreen.ShouldShowExtras(base.Owner);
		}

		bool ShouldShowLanguage()
		{
			return base.Owner._texts.GetNumLanguages() > 1;
		}

		void ApplyData()
		{
			GuiContentData guiContentData = new GuiContentData();
			string value = "ID2+: " + base.Owner._gameVersion.GetSmallVersion() + "\nE2D Mod: " + ModVersion.GetModVersion();
			guiContentData.SetValue("showExtras", this.ShouldShowExtras());
			guiContentData.SetValue("showLanguage", this.ShouldShowLanguage());
			guiContentData.SetValue("hasSaveFiles", base.Owner._saver.GetAvailableLocalSavers().Count > 0);
			guiContentData.SetValue("saveFileWarn", base.Owner.SaveFileWarning());
			guiContentData.SetValue("version", value);
			MainMenu.ApplyUIData(PlatformInfo.Current.GetDataForUI("main.main"), guiContentData);
			base.Root.ApplyContent(guiContentData, true);
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			this.ApplyData();
			LeaderboardListener.OnAvailableChange += this.GotUpdate;
			return true;
		}

		protected override bool DoHide(MenuScreen<MainMenu> next)
		{
			LeaderboardListener.OnAvailableChange -= this.GotUpdate;
			return true;
		}
	}

	class OptionsScreen : MenuScreen<MainMenu>
	{
		public OptionsScreen(MainMenu owner, string name, GuiBindData data) : base(owner, name, data)
		{
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			base.Owner._options.Show(false, true, new GuiNode.OnVoidFunc(base.StandardBackClick));
			return false;
		}
	}

	class FileSelectScreen : MenuScreen<MainMenu>
	{
		public FileSelectScreen(MainMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			base.BindSwitchClickEvent(data, "file.back", "startRoot");
		}

		void ClickedSaveFile(object ctx)
		{
			base.MenuImpl.SwitchToScreen("fileStartRoot", (DataIOBase.SaveFileData)ctx);
		}

		bool UpdateFileList()
		{
			List<DataIOBase.SaveFileData> availableLocalSavers = base.Owner._saver.GetAvailableLocalSavers();
			if (availableLocalSavers == null || availableLocalSavers.Count == 0)
			{
				return false;
			}
			availableLocalSavers.Sort((DataIOBase.SaveFileData a, DataIOBase.SaveFileData b) => DateTime.Compare(b.timestamp, a.timestamp));
			List<GuiContentData> list = new List<GuiContentData>();
			GuiNode.OnVoidFunc f = new GuiNode.OnVoidFunc(this.ClickedSaveFile);
			for (int i = 0; i < availableLocalSavers.Count; i++)
			{
				GuiContentData guiContentData = new GuiContentData();
				guiContentData.SetValue("saveName", availableLocalSavers[i].name);
				guiContentData.SetValue("fileTime", availableLocalSavers[i].timestamp.ToString());
				guiContentData.SetValue("btnEvent", new GuiNode.VoidBinding(f, availableLocalSavers[i]));
				guiContentData.SetValue("btnTag", availableLocalSavers[i]);
				if (DataFileIO.GetCurrentIO().FileExists(availableLocalSavers[i].thumbPath))
				{
					guiContentData.SetValue("fileThumb", availableLocalSavers[i].thumbPath);
					guiContentData.SetValue("showThumb", true);
				}
				else
				{
					guiContentData.SetValue("showThumb", false);
				}
				list.Add(guiContentData);
			}
			GuiContentData guiContentData2 = new GuiContentData();
			guiContentData2.SetValue("files", list);
			guiContentData2.SetValue("hasFileData", false);
			guiContentData2.SetValue("hasFiles", availableLocalSavers.Count > 0);
			guiContentData2.SetValue("cloudWarn", Clouder.QuotaExceeded);
			guiContentData2.SetValue("saveFileWarn", base.Owner.SaveFileWarning());

			base.Root.ApplyContent(guiContentData2, true);
			return true;
		}

		protected override bool StorePrevious(MenuScreen<MainMenu> previous)
		{
			return !(previous is MainMenu.NewGameScreen) && !(previous is MainMenu.DeleteConfirmScreen) && !(previous is MainMenu.FileStartScreen);
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			if (!this.UpdateFileList())
			{
				base.SwitchToBack();
				return false;
			}
			return true;
		}
	}

	class FileStartScreen : MenuScreen<MainMenu>
	{
		DataIOBase.SaveFileData currentSaveFile;

		public FileStartScreen(MainMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			base.BindSwitchClickEvent(data, "filestart.back", "fileSelectRoot");
			MenuScreen<MainMenu>.BindClickEvent(data, "filestart.duplicate", new GuiNode.OnVoidFunc(this.ClickedDuplicate));
			MenuScreen<MainMenu>.BindClickEvent(data, "filestart.erase", new GuiNode.OnVoidFunc(this.ClickedDelete));
			MenuScreen<MainMenu>.BindClickEvent(data, "filestart.start", new GuiNode.OnVoidFunc(this.ClickedStart));
		}

		List<GuiContentData> MakeItemInfoList()
		{
			List<VariableInfoData.RealValueData> allData = base.Owner._saveFileValues.GetAllData(base.Owner._saveFileValueContext);
			List<GuiContentData> list = new List<GuiContentData>(allData.Count);
			for (int i = 0; i < allData.Count; i++)
			{
				GuiContentData guiContentData = new GuiContentData();
				VariableInfoData.RealValueData realValueData = allData[i];
				if (realValueData != null && realValueData.icon != null)
				{
					guiContentData.SetValue("itemPic", realValueData.icon);
					guiContentData.SetValue("hasCount", realValueData.hasCount);
					guiContentData.SetValue("itemCount", realValueData.count);
				}
				else
				{
					guiContentData.SetValue("itemPic", false);
				}
				list.Add(guiContentData);
			}
			return list;
		}

		List<GuiContentData> MakeDungeonIndexList()
		{
			List<string> sortedSaveKeys = MainMenu.GetSortedSaveKeys(base.Owner._saver, base.Owner._saveFileDungeonPath);
			List<GuiContentData> list = new List<GuiContentData>(sortedSaveKeys.Count);
			for (int i = 0; i < sortedSaveKeys.Count; i++)
			{
				if (sortedSaveKeys[i].Contains("dungeon"))
				{
					GuiContentData guiContentData = new GuiContentData();
					guiContentData.SetValue("dungeon", sortedSaveKeys[i]);
					list.Add(guiContentData);
				}
			}
			return list;
		}

		void ShowSaveData(SaverOwner saver, DataIOBase.SaveFileData fileData)
		{
			saver.LoadLocalFromFile(fileData.path);
			GuiContentData guiContentData = new GuiContentData();
			string localThumbnailPath = saver.GetLocalThumbnailPath();
			if (DataFileIO.GetCurrentIO().FileExists(localThumbnailPath))
			{
				guiContentData.SetValue("fileThumb", localThumbnailPath);
				guiContentData.SetValue("showThumb", true);
			}
			else
			{
				guiContentData.SetValue("showThumb", false);
			}
			guiContentData.SetValue("itemList", this.MakeItemInfoList());
			guiContentData.SetValue("dungeonList", this.MakeDungeonIndexList());
			string value;
			IDataSaver saverAndName = saver.GetSaverAndName(base.Owner._saveFileTimePath, out value, true);
			guiContentData.SetValue("fileTime", SaverOwner.LoadSaveData(value, saverAndName, 0f));
			guiContentData.SetValue("fileName", fileData.name);
			guiContentData.SetValue("hasFileData", true);
			guiContentData.SetValue("cloudWarn", Clouder.QuotaExceeded);
			guiContentData.SetValue("saveFileWarn", base.Owner.SaveFileWarning());

			// Show progress %
			CommonFunctions comFuncs = GameObject.Find("CommonFunctions").GetComponent<CommonFunctions>();
			string progressText = "Progress:\n" + comFuncs.UpdateProgress(false, fileData.path).Substring(15);
			progressTextObj = ModMaster.MakeTextObj("progress", new Vector3(-2.475f, 0.6f, -0.18f), progressText, 125, TextAlignment.Center, new Color?(Color.black), false);
			progressTextObj.transform.parent = GameObject.Find("GuiLayout").transform.Find("Main").Find("FileStart").Find("FileData").Find("Data").transform;

			base.Root.ApplyContent(guiContentData, true);
		}

		GameObject progressTextObj;
		// Deletes progress text
		void DestroyProgressText()
		{
			Destroy(progressTextObj);
		}

		void ClickedStart(object ctx)
		{
			// Invoke OnFileLoaded events
			GameStateNew.OnFileLoaded(false);

            //If the item randomizer is active and there is an error, halt the start and deactivate modes
            if(!ItemRandomizerGM.Instance.IsActive || ItemRandomizerGM.Instance.Core.Error == null)
            {
                base.Owner.StartGame();
                DestroyProgressText();
            }
            else
            {
                ModeControllerNew.DeactivateModes();
                UITextFrame errorFrame = UIFactory.Instance.CreateTextFrame(0f, 1f, GameObject.Find("GuiLayout").transform.Find("Main").Find("FileStart").transform, ItemRandomizerGM.Instance.Core.Error);
                errorFrame.ScaleBackground(Vector2.one * 2f);
                errorFrame.StartCoroutine(DelayDestroy(errorFrame));
            }
        }
        
        IEnumerator DelayDestroy(UITextFrame frame)
        {
            yield return new WaitForSeconds(2.5f);
            if (frame != null) GameObject.Destroy(frame.gameObject);
        }

        void ClickedDuplicate(object ctx)
		{
			DestroyProgressText();

			if (base.Owner.SaveFileWarning())
			{
				this.StandardSwitch(ctx, "savewarnRoot");
				return;
			}
			if (this.currentSaveFile != null)
			{
				string path = this.currentSaveFile.path;
				string uniqueLocalSavePath = base.Owner._saver.GetUniqueLocalSavePath();
				DataFileIO.GetCurrentIO().CopySaver(path, uniqueLocalSavePath);
				base.MenuImpl.SwitchToScreen("fileSelectRoot", null);
			}
		}

		void ClickedDelete(object ctx)
		{
			DestroyProgressText();

			if (this.currentSaveFile != null)
			{
				MainMenu.DeleteConfirmScreen.ShowCtx args = new MainMenu.DeleteConfirmScreen.ShowCtx(this.currentSaveFile.path, this.currentSaveFile.thumbPath, this.currentSaveFile.name);
				base.Owner.menuImpl.SwitchToScreen("deleteRoot", args);
			}
		}

		protected override bool StorePrevious(MenuScreen<MainMenu> previous)
		{
			DestroyProgressText();

			return !(previous is MainMenu.NewGameScreen) && !(previous is MainMenu.DeleteConfirmScreen) && !(previous is MainMenu.SaveWarnScreen);
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			if (base.ShowParams is DataIOBase.SaveFileData)
			{
				this.currentSaveFile = (base.ShowParams as DataIOBase.SaveFileData);
			}
			this.ShowSaveData(base.Owner._saver, this.currentSaveFile);
			return true;
		}
	}

	class NewGameScreen : MenuScreen<MainMenu>
	{
		public IDataSaver saver;

		public NewGameScreen(MainMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			//Mod code for main window
			Transform gameModesParent = GameObject.Find("GuiLayout").transform.Find("Main").Find("FileCreate");
			UIButton gameModesButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 0f, 0f, gameModesParent, "Game Modes");
			gameModesButton.onInteraction += OpenGameModes;

			// Is speedrun button
            
			/*UICheckBox isSpeedrun = UIFactory.Instance.CreateCheckBox(0f, 0.25f, gameModesParent, "Speedrun?");
			isSpeedrun.name = "isSpeedrun";
            isSpeedrun.gameObject.SetActive(false);*/
		}

		void OpenGameModes()
		{
			base.MenuImpl.SwitchToScreen("newgamemodes", null);
			base.Owner._enterNameMenu.StopListener();
		}
        
        IEnumerator DelayDestroy(UITextFrame frame)
        {
            yield return new WaitForSeconds(2.5f);
            if (frame != null) GameObject.Destroy(frame.gameObject);
        }

        IEnumerator DelatedSwitch()
        {
            yield return new WaitForSeconds(1.0f);
            base.MenuImpl.SwitchToScreen("enterNameRoot", null);
        }

        void EnterNameDone(bool success, string value)
		{
			if (success && !string.IsNullOrEmpty(value))
			{
                //Before doing anything, check if item randomizer is on and do a test run to see if everything is set up properly
                if (ModeControllerNew.IsModeReady(ModeControllerNew.ModeType.ItemRandomizer))
                {
                    ItemRandomizerGM.Instance.TestRunRandomize();
                    if(ItemRandomizerGM.Instance.Core.Error != null)
                    {
                        UITextFrame errorFrame = UIFactory.Instance.CreateTextFrame(0f, 1f, GameObject.Find("GuiLayout").transform.Find("Main").transform, ItemRandomizerGM.Instance.Core.Error);
                        errorFrame.ScaleBackground(Vector2.one * 2f);
                        ItemRandomizerGM.Instance.StartCoroutine(DelayDestroy(errorFrame));
                        ItemRandomizerGM.Instance.Core.Randomizing = false;
                        base.SwitchToBack();
                        //ItemRandomizerGM.Instance.StartCoroutine(DelatedSwitch());
                        return;
                    }
                }

                DataIOBase currentIO = DataFileIO.GetCurrentIO();
				RealDataSaver realDataSaver = new RealDataSaver(value);
				saver = realDataSaver;
				DataSaverData.DebugAddData[] code = base.Owner.GetCode(value);
				if (code != null)
				{
					DataSaverData.AddDebugData(realDataSaver, code);
				}
				string uniqueLocalSavePath = base.Owner._saver.GetUniqueLocalSavePath();

				// Invoke OnFileLoaded (new) events
				GameStateNew.OnFileLoaded(true, value, uniqueLocalSavePath, realDataSaver);

				// Gather anticheat data
				/*UICheckBox isSpeedrun = GameObject.Find("isSpeedrun").GetComponent<UICheckBox>();
				if (isSpeedrun.Value)
                {
                    ModMaster.SetNewGameData("settings/showTime", "1", realDataSaver);
                    ModMaster.SetNewGameData("settings/hideCutscenes", "1", realDataSaver);
                    ModMaster.SetNewGameData("start/level", "FluffyFields", realDataSaver);

                }*/

				// Skip intro
				/*
				if (ModOptionsOld.disableIntroAndOutro)
				{
					GameObject.Find("GuiFuncs").GetComponent<MainMenu>()._defaultStartScene = "FluffyFields";
					ModMaster.SetNewGameData("start/level", "FluffyFields", realDataSaver);
					ModMaster.SetNewGameData("start/door", "", realDataSaver);
				}*/

				//If the doors have to be randomized, randomize them and print the cheat sheet


				//if (value == "expert" || value == "master") { ExpertMode.Instance.StartOfGame(realDataSaver); }

				// Universal settings, enable timer
				if (ModSaver.LoadIntFromPrefs("showTime") == 1)
				{
					ModMaster.SetNewGameData("settings/showTime", "1", realDataSaver);
				}

				currentIO.WriteFile(uniqueLocalSavePath, realDataSaver.GetSaveData());
				Debug.Log("Created file " + uniqueLocalSavePath);
				base.Owner._saver.LoadLocalFromFile(uniqueLocalSavePath);
				base.Owner.StartGame();
				return;
			}
			base.SwitchToBack();
		}


		protected override bool StorePrevious(MenuScreen<MainMenu> previous)
		{
			return !(previous is MainMenu.GameModeScreen);
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			base.Owner._enterNameMenu.Show(base.Owner._defaultName, MenuScreen<MainMenu>.GetRoot(previous), new EnterNameMenu.OnDoneFunc(this.EnterNameDone));
			return false;
		}

	}

	class DeleteConfirmScreen : MenuScreen<MainMenu>
	{
		public DeleteConfirmScreen(MainMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			base.BindBackButton(data, "delete.back");
			MenuScreen<MainMenu>.BindClickEvent(data, "delete.confirm", new GuiNode.OnVoidFunc(this.ClickedConfirm));
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			MainMenu.DeleteConfirmScreen.ShowCtx showCtx = base.ShowParams as MainMenu.DeleteConfirmScreen.ShowCtx;
			if (showCtx != null)
			{
				GuiContentData guiContentData = new GuiContentData();
				guiContentData.SetValue("fileName", showCtx.savename);
				bool flag = DataFileIO.GetCurrentIO().FileExists(showCtx.thumbPath);
				guiContentData.SetValue("showThumb", flag);
				guiContentData.SetValue("fileThumb", (!flag) ? string.Empty : showCtx.thumbPath);
				base.Root.ApplyContent(guiContentData, true);
			}
			return true;
		}

		void ClickedConfirm(object clickCtx)
		{
			MainMenu.DeleteConfirmScreen.ShowCtx showCtx = base.ShowParams as MainMenu.DeleteConfirmScreen.ShowCtx;
			if (showCtx != null)
			{
				DataIOBase currentIO = DataFileIO.GetCurrentIO();
				currentIO.DeleteFile(showCtx.filePath);
				if (!string.IsNullOrEmpty(showCtx.thumbPath))
				{
					currentIO.DeleteFile(showCtx.thumbPath);
				}
				base.Owner._saver.ResetLocalSaver();
				Debug.Log("Deleted file " + showCtx.filePath);
			}
			base.MenuImpl.SwitchToScreen("fileSelectRoot", null);
		}

		public class ShowCtx
		{
			public string filePath;

			public string thumbPath;

			public string savename;

			public ShowCtx(string filePath, string thumbPath, string savename)
			{
				this.filePath = filePath;
				this.thumbPath = thumbPath;
				this.savename = savename;
			}
		}
	}

	class ExtrasScreen : MenuScreen<MainMenu>
	{
		public ExtrasScreen(MainMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			base.BindBackButton(data, "extra.back");
			base.BindSwitchClickEvent(data, "extra.soundTest", "soundTestRoot");
			base.BindSwitchClickEvent(data, "extra.gallery", "galleryRoot");
			base.BindSwitchClickEvent(data, "extra.records", "recordsRoot");
			base.BindSwitchClickEvent(data, "extra.cards", "cardsRoot");
		}

		static bool ShouldShowRecords(MainMenu owner)
		{
			return owner.HasAnyVar(owner._bestTimeRecordPath) || LeaderboardListener.Instance.IsActive;
		}

		public static bool ShouldShowExtras(MainMenu owner)
		{
			return owner.HasBoolVar("/global/extras/soundtest") || owner.HasBoolVar("/global/extras/gallery") || MainMenu.ExtrasScreen.ShouldShowRecords(owner) || owner._cardsMenu.ShouldShow();
		}

		void ApplyData()
		{
			GuiContentData guiContentData = new GuiContentData();
			guiContentData.SetValue("showSoundTest", base.Owner.HasBoolVar("/global/extras/soundtest"));
			guiContentData.SetValue("showGallery", base.Owner.HasBoolVar("/global/extras/gallery"));
			guiContentData.SetValue("showRecords", MainMenu.ExtrasScreen.ShouldShowRecords(base.Owner));
			guiContentData.SetValue("showCards", base.Owner._cardsMenu.ShouldShow());
			base.Root.ApplyContent(guiContentData, true);
		}

		void GotChange(bool available)
		{
			this.ApplyData();
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			this.ApplyData();
			LeaderboardListener.OnAvailableChange += this.GotChange;
			return true;
		}

		protected override bool DoHide(MenuScreen<MainMenu> next)
		{
			LeaderboardListener.OnAvailableChange -= this.GotChange;
			return true;
		}

		protected override bool StorePrevious(MenuScreen<MainMenu> previous)
		{
			return !(previous is MainMenu.SoundTestScreen) && !(previous is MainMenu.GalleryScreen) && !(previous is MainMenu.RecordsScreen) && !(previous is MainMenu.CardsScreen);
		}
	}

	class LangScreen : MenuScreen<MainMenu>
	{
		bool changed;

		public LangScreen(MainMenu owner, string name, GuiBindData data) : base(owner, name, data)
		{
			base.BindBackButton(data, "lang.back");
		}

		void ClickedLang(object ctx)
		{
			TextLoader.Language language = (TextLoader.Language)ctx;
			string currentLangCode = base.Owner._texts.CurrentLangCode;
			if (language.code == currentLangCode)
			{
				return;
			}
			base.Owner._texts.SetLanguage(language.code);
			base.Owner._saver.GetSaver("/global/lang", false).SaveData("current", language.code);
			this.changed = true;
			GuiNode nodeByTag = base.Root.GetNodeByTag(language.code);
			GuiNode nodeByTag2 = base.Root.GetNodeByTag(currentLangCode);
			GuiContentData guiContentData = new GuiContentData();
			if (nodeByTag != null)
			{
				guiContentData.SetValue("lang.isCurrent", true);
				nodeByTag.ApplyContent(guiContentData, true);
			}
			if (nodeByTag2 != null)
			{
				guiContentData.SetValue("lang.isCurrent", false);
				nodeByTag2.ApplyContent(guiContentData, true);
			}
		}

		void UpdateLangList()
		{
			List<TextLoader.Language> allLanguages = base.Owner._texts.GetAllLanguages();
			string currentLangCode = base.Owner._texts.CurrentLangCode;
			for (int i = 0; i < allLanguages.Count; i++)
			{
				if (allLanguages[i].code == currentLangCode)
				{
					TextLoader.Language value = allLanguages[0];
					allLanguages[0] = allLanguages[i];
					allLanguages[i] = value;
					break;
				}
			}
			GuiContentData guiContentData = new GuiContentData();
			GuiNode.OnVoidFunc f = new GuiNode.OnVoidFunc(this.ClickedLang);
			List<GuiContentData> list = new List<GuiContentData>(allLanguages.Count);
			for (int j = 0; j < allLanguages.Count; j++)
			{
				TextLoader.Language language = allLanguages[j];
				GuiContentData guiContentData2 = new GuiContentData();
				guiContentData2.SetValue("lang.name", language.name);
				if (!string.IsNullOrEmpty(language.iconres))
				{
					guiContentData2.SetValue("lang.icon", Resources.Load<Texture2D>(language.iconres));
				}
				else if (!string.IsNullOrEmpty(language.iconfile))
				{
					guiContentData2.SetValue("lang.iconfile", language.iconfile);
				}
				guiContentData2.SetValue("lang.isCurrent", language.code == currentLangCode);
				guiContentData2.SetValue("lang.clickTag", new GuiNode.VoidBinding(f, language));
				guiContentData2.SetValue("lang.tag", language.code);
				list.Add(guiContentData2);
			}
			guiContentData.SetValue("languages", list);
			base.Root.ApplyContent(guiContentData, true);
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			this.changed = false;
			this.UpdateLangList();
			return true;
		}

		protected override bool DoHide(MenuScreen<MainMenu> next)
		{
			if (this.changed)
			{
				base.Owner._saver.SaveGlobal();
			}
			return true;
		}
	}

	class SoundTestScreen : MenuScreen<MainMenu>
	{
		public SoundTestScreen(MainMenu owner, string name, GuiBindData data) : base(owner, name, data)
		{
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			base.Owner._soundTestMenu.Show(delegate
			{
				base.StandardBackClick(null);
			});
			return false;
		}
	}

	class GalleryScreen : MenuScreen<MainMenu>
	{
		public GalleryScreen(MainMenu owner, string name, GuiBindData data) : base(owner, name, data)
		{
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			base.Owner._galleryMenu.Show(delegate
			{
				base.StandardBackClick(null);
			});
			return false;
		}
	}

	class SaveWarnScreen : MenuScreen<MainMenu>
	{
		public SaveWarnScreen(MainMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			base.BindBackButton(data, "savewarn.back");
		}
	}

	class RecordsScreen : MenuScreen<MainMenu>
	{
		LeaderboardListener.IEventTag loadTag;

		public RecordsScreen(MainMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			base.BindBackButton(data, "records.back");
		}

		bool GetBestTime(out float t)
		{
			string key;
			IDataSaver saverAndName = base.Owner._saver.GetSaverAndName(base.Owner._bestTimeRecordPath, out key, true);
			if (saverAndName != null && saverAndName.HasData(key))
			{
				t = (float)saverAndName.LoadInt(key) / 1000f;
				return true;
			}
			t = 0f;
			return false;
		}

		void GotResults(bool success, LeaderboardListener.RecordsResult results)
		{
			GuiContentData guiContentData = new GuiContentData();
			guiContentData.SetValue("loadWorldTime", false);
			if (success && results.data.Length != 0)
			{
				float num = (float)results.data[0].data / 1000f;
				guiContentData.SetValue("worldTime", num);
			}
			base.Root.ApplyContent(guiContentData, true);
		}

		protected override bool DoHide(MenuScreen<MainMenu> next)
		{
			if (this.loadTag != null)
			{
				this.loadTag.Cancel();
			}
			return base.DoHide(next);
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			GuiContentData guiContentData = new GuiContentData();
			float num;
			if (this.GetBestTime(out num))
			{
				guiContentData.SetValue("hasBestTime", true);
				guiContentData.SetValue("bestTime", num);
			}
			else
			{
				guiContentData.SetValue("hasBestTime", false);
			}
			bool flag = false;
			if (LeaderboardListener.Instance.IsActive)
			{
				guiContentData.SetValue("hasWorldTime", true);
				guiContentData.SetValue("loadWorldTime", true);
				flag = true;
			}
			else
			{
				guiContentData.SetValue("hasWorldTime", false);
			}
			base.Root.ApplyContent(guiContentData, true);

			if (flag)
			{
				this.loadTag = LeaderboardListener.Instance.GetRecords(base.Owner._bestTimeBoard, 0, 1, new LeaderboardListener.GetRecordsFunc(this.GotResults));
			}
			return base.DoShow(previous);
		}
	}

	class CardsScreen : MenuScreen<MainMenu>
	{
		public CardsScreen(MainMenu owner, string name, GuiBindData data) : base(owner, name, data)
		{
		}

		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			base.Owner._cardsMenu.Show(delegate
			{
				base.StandardBackClick(null);
			}, null);
			return false;
		}
	}

	//Game mode screen
	class GameModeScreen : MenuScreen<MainMenu>
	{
		UIScreen modScreen;

		public GameModeScreen(MainMenu owner, string name, GuiBindData data) : base(owner, name, data)
		{
			modScreen = UIScreen.GetUIScreenComponent(Root);
			modScreen.BackButton.onInteraction += delegate () { base.StandardBackClick(null); };
		}

		//This function runs every time the screen appears
		protected override bool DoShow(MenuScreen<MainMenu> previous)
		{
			//base.DoShow(previous);
			return true;
		}

		void ClickedBack()
		{
			base.StandardBackClick(null);
		}
	}
}
