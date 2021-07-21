using System;
using System.Collections.Generic;
using ModStuff;
using ModStuff.ItemRandomizer;
using UnityEngine;

public class PauseMenu : EntityOverlayWindow
{
	[SerializeField]
	GameObject _layout;

	[SerializeField]
	bool _layoutIsPrefab;

	[SerializeField]
	OptionsMenu _options;

	[SerializeField]
	CardsMenu _cards;

	[SerializeField]
	bool _optionsIsPrefab = true;

	[SerializeField]
	MapWindow _mapWindow;

	[SerializeField]
	DebugMenu _debugMenu;

	[SerializeField]
	SaverOwner _saver;

	[SerializeField]
	string _quitScene;

	[SerializeField]
	string _defaultHintStr;

	[SerializeField]
	VariableInfoData _itemInfoData;

	[SerializeField]
	FadeEffectData _fadeEffect;

	[SerializeField]
	KeyCode _debugStartCode = KeyCode.LeftControl;

	[SerializeField]
	KeyCode[] _debugStartSequence;

	MenuImpl<PauseMenu> menuImpl;

	OptionsMenu realOpts;

	Entity currEnt;

	ObjectUpdater.PauseTag pauseTag;

	List<KeyCode> debugSequence;

	List<KeyCode> uniqueDebugCodeSet;

	MapWindow mapWindow;

	OptionsMenu GetOptions()
	{
		if (this.realOpts == null)
		{
			if (this._optionsIsPrefab)
			{
				this.realOpts = GameObjectUtility.TransformInstantiate<OptionsMenu>(this._options, this._layout.transform.parent);
			}
			else
			{
				this.realOpts = this._options;
			}
		}
		return this.realOpts;
	}

	void Setup()
	{
		//Build all mod screens
		UIScreen.BuildAllScreens();

		GuiBindInData inData = new GuiBindInData(null, null);
		PrefabReplacer[] componentsInChildren = this._layout.GetComponentsInChildren<PrefabReplacer>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Apply();
		}
		GuiBindData data;
		if (this._layoutIsPrefab)
		{
			data = GuiNode.CreateAndConnect(this._layout, inData);
		}
		else
		{
			data = GuiNode.Connect(this._layout, inData);
		}
		this.menuImpl = new MenuImpl<PauseMenu>(this);
		this.menuImpl.AddScreen(new PauseMenu.MainScreen(this, "pauseRoot", data));
		this.menuImpl.AddScreen(new PauseMenu.OptionsScreen(this, "optionsRoot", data));
		this.menuImpl.AddScreen(new PauseMenu.MapScreen(this, "mapRoot", data));
		this.menuImpl.AddScreen(new PauseMenu.InfoScreen(this, "infoRoot", data));
		this.menuImpl.AddScreen(new PauseMenu.ItemScreen(this, "itemRoot", data));
		this.menuImpl.AddScreen(new PauseMenu.CardsScreen(this, "cardsRoot", data));

		//Mod Screens
		this.menuImpl.AddScreen(new PauseMenu.MainModScreen(this, "mainpause", data));
		this.menuImpl.AddScreen(new PauseMenu.CamScreen(this, "cameramenu", data));
		this.menuImpl.AddScreen(new PauseMenu.SpawnScreen(this, "spawnmenu", data));
		this.menuImpl.AddScreen(new PauseMenu.PowerupsScreen(this, "powerupsmenu", data));
		this.menuImpl.AddScreen(new PauseMenu.ModItemsScreen(this, "itemsmenu", data));
		this.menuImpl.AddScreen(new PauseMenu.WorldScreen(this, "worldmenu", data));
		this.menuImpl.AddScreen(new PauseMenu.ModInfoScreen(this, "infomenu", data));
        this.menuImpl.AddScreen(new PauseMenu.TestChamberScreen(this, "testchambermenu", data));
        this.menuImpl.AddScreen(new PauseMenu.ScriptsScreen(this, "scriptsmenu", data));
        this.menuImpl.AddScreen(new PauseMenu.CreativeMenuScreen(this, "creativemenu", data));
        this.menuImpl.AddScreen(new PauseMenu.GameOptions(this, "gameoptions", data));
        this.menuImpl.AddScreen(new PauseMenu.RandomizedKeysScreen(this, "randomizedkeysmenu", data));
        //this.menuImpl.AddScreen(new PauseMenu.ModSettings(this, "modoptions", data));

        if (this._debugMenu != null)
		{
			this.menuImpl.AddScreen(new PauseMenu.DebugScreen(this, "debugRoot", data));
		}
		PerPlatformData.DebugCodeData debugCode = PlatformInfo.Current.DebugCode;
		if (debugCode != null && debugCode.useOverride)
		{
			this._debugStartCode = debugCode.startCode;
			this._debugStartSequence = debugCode.sequence;
		}


		//-----------------
		//Add UIelements to the main pause menu screen
		//-----------------
		//Setup container
		GameObject pauseMenuContainer = new GameObject("ModContainer");
		pauseMenuContainer.transform.SetParent(GameObject.Find("PauseOverlay").transform.Find("Pause").Find("Main").Find("Layout"), false);
		pauseMenuContainer.transform.localPosition = new Vector3(-5f, -1.5f, 0f);
		//Setup main mod menu button
		UIGFXButton mainMenuButton = UIFactory.Instance.CreateGFXButton("aha", 0f, 0f, pauseMenuContainer.transform, "Extra 2 Dew");
		mainMenuButton.onInteraction += OpenMainMenu;
		mainMenuButton.name = "OpenModScreen";
		//Setup debug menu button
		UIButton debugMenuButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 0f, -2.2f, pauseMenuContainer.transform, "Debug Menu");
		debugMenuButton.onInteraction += OpenDebug;
		debugMenuButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
		debugMenuButton.name = "OpenDebugScreen";
		//Setup background
		UIBigFrame buttonsHolder = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, -0.9f, pauseMenuContainer.transform);
		buttonsHolder.ScaleBackground(new Vector2(0.28f, 1.1f));
		buttonsHolder.gameObject.name = "ModBackground";

        //Add randomized keys button (if randomizing)
        if(ItemRandomizerGM.Instance.Core.Randomizing)
        {
            Transform randoParent = GameObject.Find("PauseOverlay").transform.Find("Pause").Find("ItemScreen");
            UIButton randoButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 0f, 0.35f, randoParent, "Randomizer");
            randoButton.onInteraction += OpenRandomDungeonKeys;
        }

        if (ModVersion.IsDevBuild())
        {
            //Setup container
            GameObject devMenuContainer = new GameObject("ModContainer");
            devMenuContainer.transform.SetParent(GameObject.Find("PauseOverlay").transform.Find("Pause").Find("Main").Find("Layout"), false);
            devMenuContainer.transform.localPosition = new Vector3(5f, -1.5f, 0f);
            //Setup main mod menu button
            UIGFXButton devMenuButton = UIFactory.Instance.CreateGFXButton("testchamber", 0f, 0f, devMenuContainer.transform, "Test Chamber");
            devMenuButton.onInteraction += OpenTestChamber;
            devMenuButton.name = "OpenTestChamber";
            //Setup debug menu button
            UIButton fireTestButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 0f, -2.2f, devMenuContainer.transform, "Run TestButton");
            fireTestButton.onInteraction += DebugCommands.Instance.TestButton;
            fireTestButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            fireTestButton.name = "OpenDebugScreen";
            //Setup background
            UIBigFrame devButtonsHolder = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, -0.9f, devMenuContainer.transform);
            devButtonsHolder.ScaleBackground(new Vector2(0.28f, 1.1f));
            devButtonsHolder.gameObject.name = "TestChamberBackground";
        }
	}

    public void OpenDebug()
	{
		menuImpl.SwitchToScreen("debugRoot", null);
	}

	public void OpenMainMenu()
	{
		menuImpl.SwitchToScreen("mainpause", null);
	}

    public void OpenTestChamber()
    {
        menuImpl.SwitchToScreen("testchambermenu", null);
    }

    public void OpenRandomDungeonKeys()
    {
        menuImpl.SwitchToScreen("randomizedkeysmenu", null);
    }

    protected override void Start()
	{
		base.Start();
		if (this.menuImpl == null)
		{
			this.Setup();
		}
	}

	protected override void DoShow(Entity ent, string arg, string arg2)
	{
		if (this.menuImpl == null)
		{
			this.Setup();
		}
		this.currEnt = ent;
		if (arg == "info")
		{
			this.menuImpl.SwitchToScreen("infoRoot", null);
		}
		else if (arg == "map")
		{
			this.menuImpl.SwitchToScreen("mapRoot", null);
		}
		else if (arg == "cards")
		{
			this.menuImpl.SwitchToScreen("cardsRoot", arg2);
        }
        else if (arg == "debug")
        {
            this.menuImpl.SwitchToScreen("debugRoot", null);
        }
        else if (arg == "creative")
        {
            this.menuImpl.SwitchToScreen("creativemenu", null);
        }
        else
		{
			this.menuImpl.ShowFirst();
		}
		this.pauseTag = ObjectUpdater.Instance.RequestPause(null);
	}

	protected override void DoHide()
	{
		if (this.mapWindow != null && this.mapWindow.IsActive)
		{
			this.mapWindow.Hide();
			this.mapWindow = null;
		}
		this.menuImpl.Hide();
		this.menuImpl.Reset();
		if (this.pauseTag != null)
		{
			this.pauseTag.Release();
			this.pauseTag = null;
		}
	}

	protected override void OnDestroy()
	{
		if (this.pauseTag != null)
		{
			this.pauseTag.Release();
		}
		base.OnDestroy();
	}

	bool enableDebugSwitch;
	//KeyCode debugKey;
	void Update()
	{
		//if (debugKey == null) { debugKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("debugKey", "F1")); }
		if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("debugKey", "F1"))) && enableDebugSwitch)
		{
			menuImpl.SwitchToScreen("debugRoot", null);
		}
	}

	void UpdateDebug()
	{
		if (!Input.GetKey(this._debugStartCode))
		{
			this.debugSequence = null;
			return;
		}
		if (this.debugSequence == null)
		{
			this.debugSequence = new List<KeyCode>();
		}
		if (this.uniqueDebugCodeSet == null)
		{
			this.uniqueDebugCodeSet = new List<KeyCode>();
			for (int i = this._debugStartSequence.Length - 1; i >= 0; i--)
			{
				if (!this.uniqueDebugCodeSet.Contains(this._debugStartSequence[i]))
				{
					this.uniqueDebugCodeSet.Add(this._debugStartSequence[i]);
				}
			}
		}
		for (int j = this.uniqueDebugCodeSet.Count - 1; j >= 0; j--)
		{
			if (Input.GetKeyDown(this.uniqueDebugCodeSet[j]))
			{
				this.debugSequence.Add(this.uniqueDebugCodeSet[j]);
			}
		}
		while (this.debugSequence.Count > this._debugStartSequence.Length)
		{
			this.debugSequence.RemoveAt(0);
		}
		if (this.debugSequence.Count != this._debugStartSequence.Length)
		{
			return;
		}
		for (int k = 0; k < this.debugSequence.Count; k++)
		{
			if (this.debugSequence[k] != this._debugStartSequence[k])
			{
				return;
			}
		}
		this.debugSequence = null;
		this.menuImpl.SwitchToScreen("debugRoot", null);


	}

	class MainScreen : MenuScreen<PauseMenu>
	{
		public MainScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			MenuScreen<PauseMenu>.BindClickEvent(data, "pause.back", new GuiNode.OnVoidFunc(this.ClickedBack));
			MenuScreen<PauseMenu>.BindClickEvent(data, "pause.quit", new GuiNode.OnVoidFunc(this.ClickedQuit));
			base.BindSwitchClickEvent(data, "pause.hint", "infoRoot");
			base.BindSwitchClickEvent(data, "pause.item", "itemRoot");
			MenuScreen<PauseMenu>.BindClickEvent(data, "pause.warp", new GuiNode.OnVoidFunc(this.ClickedWarp));
			base.BindSwitchClickEvent(data, "pause.options", "optionsRoot");
			base.BindSwitchClickEvent(data, "pause.map", "mapRoot");
		}

		void ClickedBack(object ctx)
		{
			base.Owner.Hide();
		}

		void DoQuit()
		{
			base.Owner.Hide();
			Utility.LoadLevel(base.Owner._quitScene);
		}

		void ClickedQuit(object ctx)
		{
			// Invoke OnGameQuit events
			GameStateNew.OnGameQuitted();

			base.MenuImpl.Hide();
			bool saveDone = false;
			bool fadeDone = base.Owner._fadeEffect == null;
			OverlayFader.OnDoneFunc onDone = delegate ()
			{
				if (saveDone & fadeDone)
				{
					this.DoQuit();
				}
			};
			if (base.Owner._fadeEffect != null)
			{
				OverlayFader.StartFade(base.Owner._fadeEffect, true, delegate ()
				{
					fadeDone = true;
					onDone();
				}, null);
			}
			base.Owner._saver.SaveAll(true, delegate (bool success, string error)
			{
				saveDone = true;
				onDone();
			});
		}

		void ClickedWarp(object ctx)
		{
			PlayerRespawner activeInstance = PlayerRespawner.GetActiveInstance();
			if (activeInstance != null)
			{
				base.Owner.Hide();
				activeInstance.ForceRespawn();
			}
		}

		//Disable hotkey switching outside of mainmenu
		protected override bool DoHide(MenuScreen<PauseMenu> next)
		{
			base.Owner.enableDebugSwitch = false;
			return base.DoHide(next);
		}

		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			//Enable hotkey switching in mainmenu
			base.Owner.enableDebugSwitch = true;

			GuiContentData guiContentData = new GuiContentData();
			guiContentData.SetValue("showMap", base.Owner._mapWindow.CanShow(base.Owner.currEnt));
			bool flag = !PlayerRespawner.RespawnInhibited() && PlayerRespawner.GetActiveInstance() != null;
			guiContentData.SetValue("canWarp", flag);
			if (flag)
			{
				IDataSaver saver = base.Owner._saver.GetSaver("/local/start", true);
				if (saver != null)
				{
					guiContentData.SetValue("hasWarpTgt", true);
					guiContentData.SetValue("warpTgt", saver.LoadData("level"));
				}
				else
				{
					guiContentData.SetValue("hasWarpTgt", false);
				}
			}
			base.Root.ApplyContent(guiContentData, true);
			return true;
		}
	}

	class OptionsScreen : MenuScreen<PauseMenu>
	{
		public OptionsScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
		}

		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			base.Owner.GetOptions().Show(true, false, new GuiNode.OnVoidFunc(base.StandardBackClick));
			return false;
		}
	}

	class DebugScreen : MenuScreen<PauseMenu>
	{
		public DebugScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
		}

		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			base.Owner._debugMenu.Show(MenuScreen<PauseMenu>.GetRoot(previous), new DebugMenu.OnDoneFunc(ClickedBack));
			return false;
        }

        protected override bool DoHide(MenuScreen<PauseMenu> next)
        {
            base.Owner._debugMenu.Hide();
            return base.DoHide(next);
        }

        void ClickedBack()
        {
            if (base.Previous == null)
            {
                base.Owner.Hide();
                return;
            }
            base.SwitchToBack();
        }
    }

	class MapScreen : MenuScreen<PauseMenu>
	{
		public MapScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
		}

		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			if (base.Owner._mapWindow.CanShow(base.Owner.currEnt))
			{
				MapWindow pooledWindow = OverlayWindow.GetPooledWindow<MapWindow>(base.Owner._mapWindow);
				pooledWindow.Show(base.Owner.currEnt, null, new EntityOverlayWindow.OnDoneFunc(this.ClickedBack), null);
				GuiSelectionHandler component = base.Owner.GetComponent<GuiSelectionHandler>();
				if (component != null)
				{
					component.enabled = false;
				}
				base.Owner.mapWindow = pooledWindow;
			}
			else
			{
				Debug.LogWarning("No map for " + Utility.GetCurrentSceneName());
				base.SwitchToBack();
			}
			return false;
		}

		void ClickedBack()
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}

		protected override bool DoHide(MenuScreen<PauseMenu> next)
		{
			GuiSelectionHandler component = base.Owner.GetComponent<GuiSelectionHandler>();
			if (component != null)
			{
				component.enabled = true;
			}
			base.Owner.mapWindow = null;
			return true;
		}
	}

	class InfoScreen : MenuScreen<PauseMenu>
	{
		public InfoScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			MenuScreen<PauseMenu>.BindClickEvent(data, "info.back", new GuiNode.OnVoidFunc(this.ClickedBack));
		}

		string GetHintString()
		{
			HintSystem weakInstance = HintSystem.WeakInstance;
			if (weakInstance != null)
			{
				HintSystem.HintData currentHint = weakInstance.GetCurrentHint();
				if (currentHint != null)
				{
					return currentHint.String;
				}
			}
			return base.Owner._defaultHintStr;
		}

		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			GuiContentData guiContentData = new GuiContentData();
			guiContentData.SetValue("infoStr", this.GetHintString());
			int stateVariable = base.Owner.currEnt.GetStateVariable("outfit");
			guiContentData.SetValue("playerSuit", stateVariable);
			base.Root.ApplyContent(guiContentData, true);
			return true;
		}

		void ClickedBack(object ctx)
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}
	}

	class ItemScreen : MenuScreen<PauseMenu>
	{
		public ItemScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			base.BindBackButton(data, "item.back");
			base.BindSwitchClickEvent(data, "item.cards", "cardsRoot");
		}

		void ClickedItem(object ctx)
		{
			GuiContentData guiContentData = new GuiContentData();
			VariableInfoData.RealValueData realValueData = ctx as VariableInfoData.RealValueData;
			if (realValueData != null)
			{
				guiContentData.SetValue("currItemPic", realValueData.icon);
				guiContentData.SetValue("currItemName", realValueData.name);
				guiContentData.SetValue("currItemDesc", realValueData.desc);
				guiContentData.SetValue("hasItem", true);
				guiContentData.SetValue("currItemHasCount", realValueData.hasCount);
				guiContentData.SetValue("currItemCount", realValueData.count);
			}
			else
			{
				guiContentData.SetValue("hasItem", false);
			}
			base.Root.ApplyContent(guiContentData, true);
		}

		List<GuiContentData> MakeItemList(IExprContext ctx, VariableInfoData info)
		{
			List<GuiContentData> list = new List<GuiContentData>();
			List<VariableInfoData.RealValueData> allData = info.GetAllData(ctx);
			GuiNode.OnVoidFunc f = new GuiNode.OnVoidFunc(this.ClickedItem);
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
					realValueData = null;
				}
				guiContentData.SetValue("itemTag", realValueData);
				guiContentData.SetValue("itemEvent", new GuiNode.VoidBinding(f, realValueData));
				guiContentData.SetValue("itemEnabled", realValueData != null && realValueData.icon != null);
				list.Add(guiContentData);
			}
			return list;
		}

		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			GuiContentData guiContentData = new GuiContentData();
			guiContentData.SetValue("itemList", this.MakeItemList(base.Owner.currEnt, base.Owner._itemInfoData));
			guiContentData.SetValue("hasItem", false);
			guiContentData.SetValue("playerSuit", base.Owner.currEnt.GetStateVariable("outfit"));
			guiContentData.SetValue("hasCards", base.Owner._cards.ShouldShow());
			base.Root.ApplyContent(guiContentData, true);
			return true;
		}

		protected override bool StorePrevious(MenuScreen<PauseMenu> previous)
		{
			return !(previous is PauseMenu.CardsScreen) && !(previous is PauseMenu.RandomizedKeysScreen);
		}
	}

	class CardsScreen : MenuScreen<PauseMenu>
	{
		public CardsScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
		}

		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			base.Owner._cards.Show(delegate
			{
				this.ClickedBack(null);
			}, base.ShowParams as string);
			return true;
		}

		void ClickedBack(object ctx)
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}
	}

	//MOD SCREEN
	class MainModScreen : MenuScreen<PauseMenu>
	{
		UIScreen modScreen;
		public MainModScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			modScreen = UIScreen.GetUIScreenComponent(Root);
			modScreen.BackButton.onInteraction += ClickedBack;
			modScreen.GetElement<UIGFXButton>("spawn").onInteraction += delegate ()
			{
				base.StandardSwitch(null, "spawnmenu");
			};
			modScreen.GetElement<UIGFXButton>("camera").onInteraction += delegate ()
			{
				base.StandardSwitch(null, "cameramenu");
			};
			modScreen.GetElement<UIGFXButton>("powerup").onInteraction += delegate ()
			{
				base.StandardSwitch(null, "powerupsmenu");
			};
			modScreen.GetElement<UIGFXButton>("items").onInteraction += delegate ()
			{
				base.StandardSwitch(null, "itemsmenu");
			};
			modScreen.GetElement<UIGFXButton>("world").onInteraction += delegate ()
			{
				base.StandardSwitch(null, "worldmenu");
			};
            modScreen.GetElement<UIGFXButton>("scripts").onInteraction += delegate ()
            {
                base.StandardSwitch(null, "scriptsmenu");
            };
            modScreen.GetElement<UIGFXButton>("creative").onInteraction += delegate ()
            {
                base.StandardSwitch(null, "creativemenu");
            };
            modScreen.GetElement<UIGFXButton>("info").onInteraction += delegate ()
			{
				base.StandardSwitch(null, "infomenu");
			};
			//modScreen.GetElement<UIGFXButton>("gameoptions").onInteraction += delegate ()
			//{
			//	base.StandardSwitch(null, "gameoptions");
			//};

            /*
			modScreen.GetElement<UIGFXButton>("options").onInteraction += delegate ()
			{
				base.StandardSwitch(null, "modoptions");
			};
			*/
        }

		//Runs each time the window appears on screen
		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			return true;
		}

		void ClickedBack()
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.StandardSwitch(null, "pauseRoot");
		}
	}

    //MOD SCREEN
    class TestChamberScreen : MenuScreen<PauseMenu>
    {
        UIScreen modScreen;
        public TestChamberScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
        {
            modScreen = UIScreen.GetUIScreenComponent(Root);
            modScreen.BackButton.onInteraction += ClickedBack;
            /*modScreen.GetElement<UIButton>("backtomain").onInteraction += delegate ()
			{
			    base.StandardSwitch(null, "pauseRoot");
			};*/
        }

        //Runs each time the window appears on screen
        protected override bool DoShow(MenuScreen<PauseMenu> previous)
        {
            return true;
        }

        //Runs each time the window is hidden
        protected override bool DoHide(MenuScreen<PauseMenu> next)
        {
            return true;
        }

        void ClickedBack()
        {
            if (base.Previous == null)
            {
                base.Owner.Hide();
                return;
            }
            base.SwitchToBack();
        }
    }

    //MOD SCREEN
    class ScriptsScreen : MenuScreen<PauseMenu>
    {
        UIScreen modScreen;
        public ScriptsScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
        {
            modScreen = UIScreen.GetUIScreenComponent(Root);
            modScreen.BackButton.onInteraction += ClickedBack;
            /*modScreen.GetElement<UIButton>("backtomain").onInteraction += delegate ()
			{
			    base.StandardSwitch(null, "pauseRoot");
			};*/
        }

        //Runs each time the window appears on screen
        protected override bool DoShow(MenuScreen<PauseMenu> previous)
        {
            return true;
        }

        void ClickedBack()
        {
            if (base.Previous == null)
            {
                base.Owner.Hide();
                return;
            }
            base.SwitchToBack();
        }
    }

    //MOD SCREEN
    class SpawnScreen : MenuScreen<PauseMenu>
	{
        UIScreen modScreen;
		public SpawnScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			modScreen = UIScreen.GetUIScreenComponent(Root);
			modScreen.BackButton.onInteraction += ClickedBack;

            /*modScreen.GetElement<UIButton>("backtomain").onInteraction += delegate ()
			{
			    base.StandardSwitch(null, "pauseRoot");
			};*/
        }

		//Runs each time the window appears on screen
		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
            modScreen.GetElement<UIGFXButton>("destroy").UIName = ModSpawner.Instance.DestroyMessage();
            modScreen.GetElement<UIVector3>("npcscale").Value = DebugCommands.Instance.resizeEnemiesScale;
            return true;
		}

		void ClickedBack()
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}
	}

	class CamScreen : MenuScreen<PauseMenu>
	{
		UIScreen modScreen;
		public CamScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			modScreen = UIScreen.GetUIScreenComponent(Root);
			modScreen.BackButton.onInteraction += ClickedBack;
			/*modScreen.GetElement<UIButton>("backtomain").onInteraction += delegate ()
			{
			    base.StandardSwitch(null, "pauseRoot");
			};*/
		}

		//Runs each time the window appears on screen
		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			modScreen.GetElement<UIListExplorer>("mode").IndexValue = ModCamera.Instance.fpsmode;
			modScreen.GetElement<UIButton>("updatetext").Trigger();
			modScreen.GetElement<UISlider>("fov").Value = ModCamera.Instance.cam_fov;
			modScreen.GetElement<UISlider>("sens").Value = ModCamera.Instance.cam_sens;
			modScreen.GetElement<UICheckBox>("lockvert").Value = ModCamera.Instance.lock_vertical;
            modScreen.GetElement<UIListExplorer>("unlockplayer").gameObject.SetActive(ModCamera.Instance.fpsmode == 3);
            modScreen.GetElement<UIListExplorer>("unlockplayer").IndexValue = ModCamera.Instance.cam_fc_unlock_player;

            return true;
		}

		void ClickedBack()
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}
	}

	class PowerupsScreen : MenuScreen<PauseMenu>
	{
		UIScreen modScreen;
		public PowerupsScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			modScreen = UIScreen.GetUIScreenComponent(Root);
			modScreen.BackButton.onInteraction += ClickedBack;
		}

		//Runs each time the window appears on screen
		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			modScreen.GetElement<UICheckBox>("god").Value = DebugCommands.Instance.godmode;
			modScreen.GetElement<UICheckBox>("likeaboss").Value = DebugCommands.Instance.likeABoss;
			modScreen.GetElement<UICheckBox>("noclip").Value = DebugCommands.Instance.noclip;
			modScreen.GetElement<UISlider>("speed").Value = DebugCommands.Instance.move_speed;
			modScreen.GetElement<UISlider>("attackrange").Value = DebugCommands.Instance.extra_bulge;
			modScreen.GetElement<UICheckBox>("superdynamite").Value = DebugCommands.Instance.superDynamite;
			modScreen.GetElement<UISlider>("dynarange").Value = DebugCommands.Instance.dynamite_radius;
			float[] dynaArray = new float[] { 0f, 0.5f, 1.5f, 5f, 20f, 60f, 600f, 157680000f };
			for (int i = 0; i < dynaArray.Length; i++)
			{
				if (DebugCommands.Instance.dynamite_fuse == dynaArray[i])
				{
					modScreen.GetElement<UIListExplorer>("fuse").IndexValue = i;
					break;
				}
			}
			modScreen.GetElement<UICheckBox>("superice").Value = DebugCommands.Instance.superIce;
			modScreen.GetElement<UICheckBox>("freeice").Value = DebugCommands.Instance.iceOffgrid;
			modScreen.GetElement<UICheckBox>("superattack").Value = DebugCommands.Instance.superAttack;
			modScreen.GetElement<UICheckBox>("noshake").Value = DebugCommands.Instance.noShake;
			modScreen.GetElement<UICheckBox>("noshake").gameObject.SetActive(DebugCommands.Instance.superAttack);
			modScreen.GetElement<UISlider>("proj").Value = DebugCommands.Instance.projectile_count;
			modScreen.GetElement<UIListExplorer>("outfit").IndexValue = ModSaver.LoadFromEnt("outfit");
			modScreen.GetElement<UIVector3>("scale").Value = DebugCommands.Instance.resizeSelfScale;
            modScreen.GetElement<UICheckBox>("showhud").Value = !DebugCommands.Instance.showHUD;
            modScreen.GetElement<UISlider>("knockback").Value = DebugCommands.Instance.knockback_multiplier;

            return true;
		}

		void ClickedBack()
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}
	}

	class ModItemsScreen : MenuScreen<PauseMenu>
	{
		UIScreen modScreen;
		public ModItemsScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			modScreen = UIScreen.GetUIScreenComponent(Root);
			modScreen.BackButton.onInteraction += ClickedBack;
		}

		//Runs each time the window appears on screen
		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			modScreen.GetElement<UIListExplorer>("melee").IndexValue = ModSaver.LoadFromEnt("melee");
			modScreen.GetElement<UIListExplorer>("icering").IndexValue = ModSaver.LoadFromEnt("icering");
			modScreen.GetElement<UIListExplorer>("forcewand").IndexValue = ModSaver.LoadFromEnt("forcewand");
			modScreen.GetElement<UIListExplorer>("dynamite").IndexValue = ModSaver.LoadFromEnt("dynamite");
			modScreen.GetElement<UIListExplorer>("amulet").IndexValue = ModSaver.LoadFromEnt("amulet");
			modScreen.GetElement<UIListExplorer>("tracker").IndexValue = ModSaver.LoadFromEnt("tracker");
			modScreen.GetElement<UIListExplorer>("headband").IndexValue = ModSaver.LoadFromEnt("headband");
			modScreen.GetElement<UIListExplorer>("chain").IndexValue = ModSaver.LoadFromEnt("chain");
			modScreen.GetElement<UIListExplorer>("tome").IndexValue = ModSaver.LoadFromEnt("tome");
			modScreen.GetElement<UISlider>("raft").Value = (float)ModSaver.LoadFromEnt("raft");
			modScreen.GetElement<UISlider>("shards").Value = (float)ModSaver.LoadFromEnt("shards");
			modScreen.GetElement<UISlider>("evilkeys").Value = (float)ModSaver.LoadFromEnt("evilKeys");

			return true;
		}

		void ClickedBack()
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}
	}

	class WorldScreen : MenuScreen<PauseMenu>
	{
		UIScreen modScreen;
		public WorldScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			modScreen = UIScreen.GetUIScreenComponent(Root);
			modScreen.BackButton.onInteraction += ClickedBack;
		}

		//Runs each time the window appears on screen
		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			LevelTime timer = LevelTime.Instance;
			float time = Mathf.Repeat(timer.GetTime("currTime") + 12, 24);
			modScreen.GetElement<UISlider>("tod").Value = time;
			modScreen.GetElement<UISlider>("flow").Value = DebugCommands.Instance.timeflow;

            float colorSlider;
            int colorIndex = modScreen.GetElement<UIListExplorer>("skyexplorer").IndexValue;
            if (colorIndex == 0) colorSlider = CameraSkyColor.Instance.red;
            else if (colorIndex == 1) colorSlider = CameraSkyColor.Instance.green;
            else colorSlider = CameraSkyColor.Instance.blue;
            modScreen.GetElement<UICheckBox>("skybool").Trigger(CameraSkyColor.Instance.UseForceColor);
            modScreen.GetElement<UISlider>("skyslider").Value = colorSlider;

            return true;
		}

		void ClickedBack()
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}
	}

    //Creative menu
    class CreativeMenuScreen : MenuScreen<PauseMenu>
    {
        UIScreen modScreen;
        public CreativeMenuScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
        {
            modScreen = UIScreen.GetUIScreenComponent(Root);
            modScreen.BackButton.onInteraction += ClickedBack;
        }

        //Runs each time the window appears on screen
        protected override bool DoShow(MenuScreen<PauseMenu> previous)
        {
            GuiSelectionHandler selectionHandler = TransformUtility.FindInParents<GuiSelectionHandler>(base.Owner.transform);
            selectionHandler.ListenerToggles(false);
            selectionHandler.ChangeSelection(null, false);
            modScreen.GetElement<UIButton>("updatemarkers").Trigger();
            return true;
        }

        //Runs each time the window is hidden
        protected override bool DoHide(MenuScreen<PauseMenu> next)
        {
            TransformUtility.FindInParents<GuiSelectionHandler>(base.Owner.transform).ListenerToggles(true);
            modScreen.GetElement<UIButton>("forceclose").Trigger();
            return true;
        }

        void ClickedBack()
        {
            if (base.Previous == null)
            {
                base.Owner.Hide();
                return;
            }
            base.SwitchToBack();
        }
    }

    //Info menu
    class ModInfoScreen : MenuScreen<PauseMenu>
	{
		UIScreen modScreen;
		public ModInfoScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			modScreen = UIScreen.GetUIScreenComponent(Root);
			modScreen.BackButton.onInteraction += ClickedBack;
		}

		void ClickedBack()
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}
	}

    //Randomized keys menu
    class RandomizedKeysScreen : MenuScreen<PauseMenu>
    {
        UIScreen modScreen;
        public RandomizedKeysScreen(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
        {
            modScreen = UIScreen.GetUIScreenComponent(Root);
            modScreen.BackButton.onInteraction += ClickedBack;
        }

        protected override bool DoShow(MenuScreen<PauseMenu> previous)
        {
            // Add UI start values here!
            ItemRandomizerGM.Instance.UpdateDungeonKeyScreen();
            return true;
        }

        void ClickedBack()
        {
            if (base.Previous == null)
            {
                base.Owner.Hide();
                return;
            }
            base.SwitchToBack();
        }
    }

    // Added by mod
    class GameOptions : MenuScreen<PauseMenu>
	{
		UIScreen modScreen;
		public GameOptions(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			modScreen = UIScreen.GetUIScreenComponent(Root);
			modScreen.BackButton.onInteraction += ClickedBack;
		}

		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			// Add UI start values here!
			modScreen.GetElement<UICheckBox>("disableIntroAndOutro").Value = ModStuff.GameOptions.Instance.DisableIntroAndOutro;
			modScreen.GetElement<UICheckBox>("fasterTransitions").Value = ModStuff.GameOptions.Instance.FasterTransitions;
			return true;
		}

		void ClickedBack()
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}
	}

	/*
	//Mod options menu
	class ModSettings : MenuScreen<PauseMenu>
	{
		UIScreen modScreen;
		public ModSettings(PauseMenu owner, string root, GuiBindData data) : base(owner, root, data)
		{
			modScreen = UIScreen.GetUIScreenComponent(Root);
			modScreen.BackButton.onInteraction += ClickedBack;
		}

		//Runs each time the window appears on screen
		protected override bool DoShow(MenuScreen<PauseMenu> previous)
		{
			modScreen.GetElement<UICheckBox>("extraCutscenes").Value = ModOptions.disableExtraCutscenes;
			modScreen.GetElement<UICheckBox>("introAndOutro").Value = ModOptions.disableIntroAndOutro;
			modScreen.GetElement<UICheckBox>("betterHealthMeter").Value = ModOptions.betterHealthMeter;
			modScreen.GetElement<UICheckBox>("darkOglerDropFix").Value = ModOptions.darkOglerDropFix;
			modScreen.GetElement<UICheckBox>("fastTransitions").Value = ModOptions.fastTransitions;
			modScreen.GetElement<UICheckBox>("loadAllRooms").Value = ModOptions.loadAllRooms;
			return true;
		}

		void ClickedBack()
		{
			if (base.Previous == null)
			{
				base.Owner.Hide();
				return;
			}
			base.SwitchToBack();
		}
	}
	*/
}
