using System;
using System.Collections.Generic;
using UnityEngine;
using ModStuff; //Added by injection

public class EntityHUD : MonoBehaviour
{
	[SerializeField]
	Entity _entity;

	[SerializeField]
	EntityHUDData _data;

	[SerializeField]
	MappedInput _input;

	[SerializeField]
	SaverOwner _saver;

	[SerializeField]
	string _overlayCameraName = "OverlayCamera";

	HealthMeterBase realMeter;

	Killable killable;

	PlayerController controller;

	float oldMaxHp;

	List<GameObject> ownedObjects = new List<GameObject>();

	Dictionary<string, ICounterOverlay> counters;

	List<EntityHUD.Button> buttons;

	List<MappedInput.ButtonEventListener> buttonListeners;

	PrioMouseHandler.Tag mouseTag;

	List<EntityHUD.SaveToggleOverlay> saveToggleWnds;

	List<EntityHUD.ConnectedHealthMeter> healthMeters = new List<EntityHUD.ConnectedHealthMeter>();

	HashSet<ItemId> localShownItems = new HashSet<ItemId>();

	List<GuiClickRect> clickRects = new List<GuiClickRect>();

	Camera overlayCam;

	List<EntityHUD.ClickTrack> currClickers = new List<EntityHUD.ClickTrack>();

	ItemMessageBox currentMsgBox;

	bool isSetup;

	static EntityHUD currentHud;

	void Setup()
	{
		if (!this.isSetup)
		{
			this.isSetup = true;
			EntityHUDData standardHUDOverride = PlatformInfo.Current.StandardHUDOverride;
			if (standardHUDOverride != null)
			{
				this._data = standardHUDOverride;
			}
		}
	}

	public void Observe(Entity ent, PlayerController controller)
	{
		this.Setup();
		this._entity = ent;
		this.controller = controller;
		base.enabled = true;
		this.Connect();
	}

	bool ShouldHideStuff()
	{
		if (!string.IsNullOrEmpty(this._data.HideHUDVar))
		{
			string key;
			IDataSaver saverAndName = this._saver.GetSaverAndName(this._data.HideHUDVar, out key, true);
			if (saverAndName != null && saverAndName.LoadBool(key))
			{
				return true;
			}
		}
		return false;
	}

	public EntityHUD.HealthMeterTag ConnectHealthMeter(Entity ent, string meterId)
	{
		this.Setup();
		HealthMeterBase healthMeterForId = this._data.GetHealthMeterForId(meterId);
		Killable entityComponent = ent.GetEntityComponent<Killable>();
		if (healthMeterForId != null && entityComponent != null)
		{
			HealthMeterBase pooledWindow = OverlayWindow.GetPooledWindow<HealthMeterBase>(healthMeterForId);
			EntityHUD.ConnectedHealthMeter connectedHealthMeter = new EntityHUD.ConnectedHealthMeter(ent, entityComponent, pooledWindow);
			this.healthMeters.Add(connectedHealthMeter);
			if (this.ShouldHideStuff())
			{
				connectedHealthMeter.Hide();
			}
			return new EntityHUD.HealthMeterTag(this, connectedHealthMeter);
		}
		return null;
	}

	void DisconnectHealthMeter(EntityHUD.ConnectedHealthMeter meter)
	{
		int num = this.healthMeters.IndexOf(meter);
		if (num != -1)
		{
			this.healthMeters.RemoveAt(num);
			meter.Stop(false);
		}
	}

	bool ShouldShow(ItemId item)
	{
		if (item == null || item.ItemGetMode == ItemId.ShowMode.Never)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(item.ShowCheckVar))
		{
			string key;
			IDataSaver saverAndName = this._saver.GetSaverAndName(item.ShowCheckVar, out key, true);
			if (saverAndName == null || !saverAndName.LoadBool(key))
			{
				return false;
			}
		}
		if (item.ItemGetMode == ItemId.ShowMode.OncePerLevel)
		{
			if (this.localShownItems == null)
			{
				this.localShownItems = new HashSet<ItemId>();
			}
			if (this.localShownItems.Contains(item))
			{
				return false;
			}
			this.localShownItems.Add(item);
		}
		else if (item.ItemGetMode == ItemId.ShowMode.Once)
		{
			string path = this._data.ShownItemsPath + "/" + item.ItemGetTag;
			string key2;
			IDataSaver saverAndName2 = this._saver.GetSaverAndName(path, out key2, false);
			if (saverAndName2 != null && saverAndName2.LoadBool(key2))
			{
				return false;
			}
			saverAndName2.SaveBool(key2, true);
		}
		return true;
	}

    //Force get item window creation (created by code injection)
    public void ForceGotItem(string image, string text)
    {
        this.currentMsgBox = OverlayWindow.GetPooledWindow<ItemMessageBox>(this._data.GetItemBox);
        this.currentMsgBox.Show(image, new StringHolder.OutString(text));
    }

    //Force get item window creation with texture (created by code injection)
    public void ForceGotItem(Texture2D texture, string text)
    {
        this.currentMsgBox = OverlayWindow.GetPooledWindow<ItemMessageBox>(this._data.GetItemBox);
        this.currentMsgBox.Show(texture, new StringHolder.OutString(text));
    }

    void GotItem(Entity ent, Item item)
	{
		if (this.ShouldShow(item.ItemId))
		{
			if (this.currentMsgBox != null && this.currentMsgBox.IsActive)
			{
				this.currentMsgBox.Hide(true);
			}
			this.currentMsgBox = OverlayWindow.GetPooledWindow<ItemMessageBox>(this._data.GetItemBox);
			this.currentMsgBox.Show(item.ItemId);
		}
	}

	void VariableUpdated(Entity ent, string var, int value)
	{
		ICounterOverlay counterOverlay;
		if (this.counters != null && this.counters.TryGetValue(var, out counterOverlay))
		{
			counterOverlay.SetValue(value);
		}
	}

	void Died(Entity ent)
	{
		if (ent == this._entity)
		{
			if (this.realMeter != null)
			{
				Killable entityComponent = ent.GetEntityComponent<Killable>();
				if (entityComponent == null)
				{
					this.realMeter.SetHealth(0f, false);
				}
				else
				{
					this.realMeter.SetHealth(Mathf.Max(0f, entityComponent.CurrentHp), false);
				}
			}
			this.Disconnect();
		}
	}

	void SetupVariables(Entity ent)
	{
		if (this.counters == null)
		{
			this.counters = new Dictionary<string, ICounterOverlay>();
			EntityHUDData.VarConnection[] varConnects = this._data.GetVarConnects();
			for (int i = 0; i < varConnects.Length; i++)
			{
				string varName = varConnects[i].varName;
				CounterOverlay pooledWindow = OverlayWindow.GetPooledWindow<CounterOverlay>(varConnects[i].hudPrefab);
				pooledWindow.SetValue(ent.GetStateVariable(varName));
				this.counters.Add(varName, pooledWindow);
				this.ownedObjects.Add(pooledWindow.gameObject);
			}
			EntityHUDData.WeaponConnection[] weaponConnects = this._data.GetWeaponConnects();
			for (int j = 0; j < weaponConnects.Length; j++)
			{
				string varName2 = weaponConnects[j].varName;
				WeaponIconOverlay pooledWindow2 = OverlayWindow.GetPooledWindow<WeaponIconOverlay>(weaponConnects[j].hudPrefab);
				pooledWindow2.SetValue(ent.GetStateVariable(varName2));
				this.counters.Add(varName2, pooledWindow2);
				if (this.controller != null)
				{
					InputButton key;
					if (this.controller.GetKeyForAction(weaponConnects[j].actionName, out key))
					{
						pooledWindow2.SetKey(key);
						GuiClickable componentInChildren = pooledWindow2.GetComponentInChildren<GuiClickable>();
						if (componentInChildren != null)
						{
							componentInChildren.onclick = delegate(object b)
							{
								this.controller.SignalPress(key);
							};
							componentInChildren.onrelease = delegate(object b)
							{
								this.controller.SignalRelease(key);
							};
						}
					}
				}
				this.ownedObjects.Add(pooledWindow2.gameObject);
			}
		}
		else
		{
			foreach (KeyValuePair<string, ICounterOverlay> keyValuePair in this.counters)
			{
				keyValuePair.Value.SetValue(ent.GetStateVariable(keyValuePair.Key));
			}
		}
	}

    //CHANGED THROUGH METHOD EDIT
	void SetupButtons()
	{
		if (this.buttons == null)
		{
			EntityHUDData.Button[] array = this._data.GetButtons();
			this.buttons = new List<EntityHUD.Button>(array.Length);
			this.buttonListeners = new List<MappedInput.ButtonEventListener>(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				EntityHUDData.Button button = array[i];
				EntityHUD.Button nBtn = new EntityHUD.Button(button);
				this.buttons.Add(nBtn);
				if (nBtn.button != null)
				{
					this.ownedObjects.Add(nBtn.button.gameObject);
					GuiClickable componentInChildren = nBtn.button.gameObject.GetComponentInChildren<GuiClickable>();
					if (componentInChildren != null)
					{
						GuiClickableWheel guiClickableWheel = componentInChildren as GuiClickableWheel;
						if (guiClickableWheel != null)
						{
							guiClickableWheel.onclick = delegate(object b)
							{
								this.PressedWheel();
							};
							guiClickableWheel.ondir = delegate(Vector2 d)
							{
								this.UpdateWheel(d);
							};
							guiClickableWheel.onrelease = delegate(object b)
							{
								this.ReleasedWheel();
							};
						}
						else
						{
							componentInChildren.onclick = delegate(object b)
							{
								this.PressedButton(nBtn, null);
							};
						}
					}
				}
				if (button.hotkey != null)
				{
					this.buttonListeners.Add(this._input.RegisterButtonDown(button.hotkey, delegate(InputButton b)
					{
						this.PressedButton(nBtn, null);
					}, -1));
				}
			}

            //Debug menu hotkey
            EntityHUDData.Button debugMenuButton = new EntityHUDData.Button();
            InputButton inputButton = new InputButton();
            inputButton.AddKey(KeyCode.F1);
            debugMenuButton.hotkey = inputButton;
            EntityHUDAction action = ScriptableObject.Instantiate<EntityHUDAction>(array[0].buttonAction);
            action.SetWnd("debug");
            debugMenuButton.buttonAction = action;
            EntityHUD.Button modBtn = new EntityHUD.Button(debugMenuButton);
            this.buttons.Add(modBtn);
            this.buttonListeners.Add(this._input.RegisterButtonDown(inputButton, delegate (InputButton b)
            {
                this.PressedButton(modBtn, null);
            }, -1));

            //Creative menu hotkey
            EntityHUDData.Button creativeMenuButton = new EntityHUDData.Button();
            InputButton inputButton0 = new InputButton();
            inputButton0.AddKey(KeyCode.F2);
            creativeMenuButton.hotkey = inputButton0;
            EntityHUDAction action0 = ScriptableObject.Instantiate<EntityHUDAction>(array[0].buttonAction);
            action0.SetWnd("creative");
            creativeMenuButton.buttonAction = action0;
            EntityHUD.Button mod0Btn = new EntityHUD.Button(creativeMenuButton);
            this.buttons.Add(mod0Btn);
            this.buttonListeners.Add(this._input.RegisterButtonDown(inputButton0, delegate (InputButton b)
            {
                this.PressedButton(mod0Btn, null);
            }, -1));
        }
		if (this.mouseTag == null && PlatformInfo.Current.AllowMouseInput)
		{
			this.mouseTag = PrioMouseHandler.GetHandler(this._input).GetListener(2, new PrioMouseHandler.MouseDownFunc(this.MouseDown), new PrioMouseHandler.MouseMoveFunc(this.MouseMove), new PrioMouseHandler.MouseUpFunc(this.MouseUp));
		}
	}

	void ClearButtons()
	{
		if (this.buttons != null)
		{
			this.buttons.Clear();
			this.buttons = null;
		}
		if (this.buttonListeners != null)
		{
			for (int i = this.buttonListeners.Count - 1; i >= 0; i--)
			{
				this.buttonListeners[i].Stop();
			}
			this.buttonListeners.Clear();
			this.buttonListeners = null;
		}
		if (this.mouseTag != null)
		{
			this.mouseTag.Stop();
			this.mouseTag = null;
		}
	}

	void UpdateSaveVarWindows()
	{
		this._saver = (this._saver ?? this._data.StandardSaver);
		if (this._saver == null)
		{
			return;
		}
		if (this.saveToggleWnds == null)
		{
			EntityHUDData.SaveVarConnection[] saveVarToggables = this._data.GetSaveVarToggables();
			if (saveVarToggables != null)
			{
				this.saveToggleWnds = new List<EntityHUD.SaveToggleOverlay>(saveVarToggables.Length);
				for (int i = 0; i < saveVarToggables.Length; i++)
				{
					this.saveToggleWnds.Add(new EntityHUD.SaveToggleOverlay(saveVarToggables[i]));
				}
			}
		}
		for (int j = this.saveToggleWnds.Count - 1; j >= 0; j--)
		{
			EntityHUD.SaveToggleOverlay saveToggleOverlay = this.saveToggleWnds[j];
			string value;
			IDataSaver saverAndName = this._saver.GetSaverAndName(saveToggleOverlay.conn.varPath, out value, true);
			bool flag = SaverOwner.LoadSaveData(value, saverAndName, false);
			if (!flag && saveToggleOverlay.wnd != null && saveToggleOverlay.wnd.IsActive)
			{
				saveToggleOverlay.wnd.gameObject.SetActive(false);
			}
			else if (flag)
			{
				if (saveToggleOverlay.wnd == null)
				{
					saveToggleOverlay.wnd = OverlayWindow.GetPooledWindow<OverlayWindow>(saveToggleOverlay.conn.hudPrefab);
					this.ownedObjects.Add(saveToggleOverlay.wnd.gameObject);
				}
				saveToggleOverlay.wnd.gameObject.SetActive(true);
			}
		}
		if (!string.IsNullOrEmpty(this._data.HideHUDVar))
		{
			string key;
			IDataSaver saverAndName2 = this._saver.GetSaverAndName(this._data.HideHUDVar, out key, true);
			if (saverAndName2 != null && saverAndName2.LoadBool(key))
			{
				for (int k = this.ownedObjects.Count - 1; k >= 0; k--)
				{
					GameObject gameObject = this.ownedObjects[k];
					if (gameObject != null)
					{
						gameObject.SetActive(false);
					}
				}
				for (int l = this.healthMeters.Count - 1; l >= 0; l--)
				{
					this.healthMeters[l].Hide();
				}
				if (this.realMeter != null)
				{
					this.realMeter.gameObject.SetActive(false);
				}
			}
		}
	}

	void Disconnect()
	{
		Entity entity = this._entity;
		this.killable = null;
		this._entity = null;
		if (entity != null)
		{
			entity.LocalEvents.DeathListener -= this.Died;
			entity.LocalEvents.ItemGetListener -= this.GotItem;
			entity.LocalEvents.VarSetListener -= this.VariableUpdated;
		}
	}

	void Connect()
	{
		Entity entity = this._entity;
		if (entity != null)
		{
			entity.LocalEvents.DeathListener += this.Died;
			entity.LocalEvents.ItemGetListener += this.GotItem;
			this.killable = entity.GetEntityComponent<Killable>();
			if (this.killable != null && this._data.HealthMeter != null)
			{
				if (this.realMeter == null)
				{
					this.realMeter = OverlayWindow.GetPooledWindow<HealthMeterBase>(this._data.HealthMeter);
				}
				this.realMeter.SetMaxHealth(this.killable.MaxHp, true);
				this.realMeter.SetHealth(this.killable.CurrentHp, true);
				this.oldMaxHp = this.killable.MaxHp;
			}
			this.SetupVariables(entity);
			this.SetupButtons();
			entity.LocalEvents.VarSetListener += this.VariableUpdated;
		}
		this.UpdateSaveVarWindows();
	}

	void OnDestroy()
	{
		if (this.realMeter != null)
		{
			Object.Destroy(this.realMeter.gameObject);
		}
		if (this.counters != null)
		{
			this.counters.Clear();
		}
		for (int i = this.healthMeters.Count - 1; i >= 0; i--)
		{
			this.healthMeters[i].Stop(true);
		}
		this.healthMeters.Clear();
		this.ClearButtons();
		for (int j = 0; j < this.ownedObjects.Count; j++)
		{
			Object.Destroy(this.ownedObjects[j]);
		}
	}

	void WindowDone(EntityHUD.Button btn)
	{
		btn.currentWindow = null;
		List<GameObject> hiddenObjects = btn.hiddenObjects;
		btn.hiddenObjects = null;
		if (hiddenObjects != null)
		{
			for (int i = hiddenObjects.Count - 1; i >= 0; i--)
			{
				hiddenObjects[i].SetActive(true);
			}
		}
		this.UpdateSaveVarWindows();
	}

	void PressedButton(EntityHUD.Button btn, string arg = null)
	{
		if (btn.currentWindow != null && btn.currentWindow.IsActive && btn.toggle)
		{
			EntityOverlayWindow currentWindow = btn.currentWindow;
			btn.currentWindow = null;
			currentWindow.Hide();
		}
		else if (btn.currentWindow == null && btn.action.CanShow(this._entity))
		{
			if (btn.action.Exclusive)
			{
				List<GameObject> list = new List<GameObject>();
				for (int i = this.ownedObjects.Count - 1; i >= 0; i--)
				{
					GameObject gameObject = this.ownedObjects[i];
					if (gameObject.activeInHierarchy)
					{
						gameObject.SetActive(false);
						list.Add(gameObject);
					}
				}
				btn.hiddenObjects = list;
			}
			btn.currentWindow = btn.action.DoShow(this._entity, delegate
			{
				this.WindowDone(btn);
			}, arg);
		}
	}

	public void PressedAction(EntityHUDAction action, string arg = null)
	{
		EntityHUD.Button btn = new EntityHUD.Button(new EntityHUDData.Button
		{
			buttonAction = action
		});
		this.PressedButton(btn, arg);
	}

	public Entity CurrentEntity
	{
		get
		{
			return this._entity;
		}
	}

	void PressedWheel()
	{
	}

	void UpdateWheel(Vector2 dir)
	{
		if (this.controller != null)
		{
			this.controller.SignalMouseDir(dir);
		}
	}

	void ReleasedWheel()
	{
		if (this.controller != null)
		{
			this.controller.SignalMouseUp();
		}
	}

	List<GuiClickRect> GetClickRects()
	{
		if (this.clickRects == null)
		{
			this.clickRects = new List<GuiClickRect>();
		}
		this.clickRects.Clear();
		base.GetComponentsInChildren<GuiClickRect>(false, this.clickRects);
		for (int i = this.clickRects.Count - 1; i >= 0; i--)
		{
			GuiClickRect guiClickRect = this.clickRects[i];
			if (guiClickRect.GetComponentInParent<GuiSelectionHandler>() != null)
			{
				this.clickRects.RemoveAt(i);
			}
		}
		return this.clickRects;
	}

	Camera GetOverlayCam()
	{
		if (this.overlayCam == null)
		{
			Camera[] allCameras = Camera.allCameras;
			for (int i = 0; i < allCameras.Length; i++)
			{
				if (allCameras[i].name == this._overlayCameraName)
				{
					this.overlayCam = allCameras[i];
					return this.overlayCam;
				}
			}
			Debug.LogWarning("Overlay Camera not found: " + this.overlayCam);
			this.overlayCam = Camera.current;
		}
		return this.overlayCam;
	}

	void ClickObject(GuiClickRect rect, int btn)
	{
		GuiClickable component = rect.GetComponent<GuiClickable>();
		if (component != null)
		{
			for (int i = this.currClickers.Count - 1; i >= 0; i--)
			{
				if (this.currClickers[i].click == component)
				{
					return;
				}
			}
			this.currClickers.Add(new EntityHUD.ClickTrack(component, btn));
			component.SendClick();
			if (component as GuiClickableWheel)
			{
				this.MouseMove(btn, this.mouseTag.Pos);
			}
		}
		else
		{
			Debug.LogWarning("No function bound to clicker");
		}
	}

	bool MouseDown(int btn, Vector2 P)
	{
		List<GuiClickRect> list = this.GetClickRects();
		Camera cam = this.GetOverlayCam();
		for (int i = 0; i < list.Count; i++)
		{
			GuiClickRect guiClickRect = list[i];
			if (guiClickRect.GetActiveScreenRect(cam).Contains(P))
			{
				this.ClickObject(guiClickRect, btn);
				return true;
			}
		}
		return false;
	}

	static float GetQuadrant(float ang, out int q)
	{
		ang = ang / 3.14159274f * 0.5f + 0.5f;
		q = Mathf.FloorToInt(ang * 8f);
		float num = (float)q / 8f;
		return (ang - num) * 8f;
	}

	static float ToQuadrant(float x, int q)
	{
		x = (x + (float)q) / 8f;
		return (x * 2f - 1f) * 3.14159274f;
	}

	static float Sigmoid(float x, float s)
	{
		return Mathf.Clamp01(x * s + 0.5f);
	}

	static float BiasWheelAngle(float ang)
	{
		int q;
		float quadrant = EntityHUD.GetQuadrant(ang, out q);
		float x = EntityHUD.Sigmoid(quadrant * 2f - 1f, 1.5f);
		return EntityHUD.ToQuadrant(x, q);
	}

	void MouseMove(int btn, Vector2 P)
	{
		for (int i = this.currClickers.Count - 1; i >= 0; i--)
		{
			EntityHUD.ClickTrack clickTrack = this.currClickers[i];
			if (clickTrack.btn == btn)
			{
				GuiClickableWheel guiClickableWheel = clickTrack.click as GuiClickableWheel;
				if (guiClickableWheel != null)
				{
					Camera cam = this.GetOverlayCam();
					Rect activeScreenRect = guiClickableWheel.GetComponent<GuiClickRect>().GetActiveScreenRect(cam);
					Vector2 vector = (P - activeScreenRect.center) * 2f;
					float num = vector.sqrMagnitude / (activeScreenRect.width * activeScreenRect.width + activeScreenRect.height * activeScreenRect.height);
					if (num > 0.005f)
					{
						float num2 = EntityHUD.BiasWheelAngle(Mathf.Atan2(vector.y, vector.x));
						vector..ctor(Mathf.Cos(num2), Mathf.Sin(num2));
						Vector2 d = vector * num;
						guiClickableWheel.SendDir(d);
					}
				}
			}
		}
	}

	void MouseUp(int btn)
	{
		for (int i = this.currClickers.Count - 1; i >= 0; i--)
		{
			EntityHUD.ClickTrack clickTrack = this.currClickers[i];
			if (clickTrack.btn == btn)
			{
				this.currClickers.RemoveAt(i);
				clickTrack.click.SendRelease();
			}
		}
	}

	void Update()
	{
		if (this._entity == null || !this._entity.gameObject.activeInHierarchy)
		{
			this.Disconnect();
			base.enabled = false;
		}
		if (this.killable != null && this.realMeter != null)
		{
			float maxHp = this.killable.MaxHp;
			if (this.oldMaxHp != maxHp)
			{
				this.oldMaxHp = maxHp;
				this.realMeter.SetMaxHealth(maxHp, false);
			}
			this.realMeter.SetHealth(this.killable.CurrentHp, false);
		}
		for (int i = this.healthMeters.Count - 1; i >= 0; i--)
		{
			EntityHUD.ConnectedHealthMeter connectedHealthMeter = this.healthMeters[i];
			if (connectedHealthMeter.Update())
			{
				connectedHealthMeter.Stop(false);
				this.healthMeters.RemoveAt(i);
			}
		}
	}

	void Awake()
	{
		EntityHUD.currentHud = this;
	}

	public static EntityHUD GetCurrentHUD()
	{
		return EntityHUD.currentHud;
	}

	public class Button
	{
		public InputButton hotkey;

		public OverlayWindow button;

		public EntityHUDAction action;

		public bool toggle;

		public EntityOverlayWindow currentWindow;

		public List<GameObject> hiddenObjects;

		public Button(EntityHUDData.Button button)
		{
			this.hotkey = button.hotkey;
			if (button.buttonPrefab != null)
			{
				this.button = OverlayWindow.GetPooledWindow<OverlayWindow>(button.buttonPrefab);
			}
			this.action = button.buttonAction;
			this.toggle = button.toggle;
		}
	}

	public class SaveToggleOverlay
	{
		public EntityHUDData.SaveVarConnection conn;

		public OverlayWindow wnd;

		public SaveToggleOverlay(EntityHUDData.SaveVarConnection conn)
		{
			this.conn = conn;
		}
	}

	public class ConnectedHealthMeter
	{
		Entity entity;

		Killable target;

		HealthMeterBase meter;

		float oldMaxHp;

		public ConnectedHealthMeter(Entity ent, Killable target, HealthMeterBase meter)
		{
			this.entity = ent;
			this.target = target;
			this.meter = meter;
			this.oldMaxHp = target.MaxHp;
			meter.SetMaxHealth(this.oldMaxHp, true);
			ent.LocalEvents.DeactivateListener += this.Deactivated;
			ent.LocalEvents.DeathListener += this.Died;
		}

		public void Stop(bool clear)
		{
			if (this.meter != null)
			{
				if (clear)
				{
					Object.Destroy(this.meter.gameObject);
				}
				else
				{
					this.meter.gameObject.SetActive(false);
				}
			}
			if (this.entity != null)
			{
				this.entity.LocalEvents.DeactivateListener -= this.Deactivated;
				this.entity.LocalEvents.DeathListener -= this.Died;
			}
		}

		public void Hide()
		{
			this.meter.gameObject.SetActive(false);
		}

		void Deactivated(Entity ent)
		{
		}

		void Died(Entity ent)
		{
			this.meter.SetHealth(0f, false);
		}

		public bool Update()
		{
			if (this.target != null && this.meter != null)
			{
				float maxHp = this.target.MaxHp;
				if (this.oldMaxHp != maxHp)
				{
					this.oldMaxHp = maxHp;
					this.meter.SetMaxHealth(maxHp, false);
				}
				this.meter.SetHealth(this.target.CurrentHp, false);
			}
			return this.entity == null || this.entity.InactiveOrDead;
		}
	}

	public class HealthMeterTag
	{
		EntityHUD owner;

		EntityHUD.ConnectedHealthMeter meter;

		public HealthMeterTag(EntityHUD owner, EntityHUD.ConnectedHealthMeter meter)
		{
			this.owner = owner;
			this.meter = meter;
		}

		public void Stop()
		{
			EntityHUD entityHUD = this.owner;
			EntityHUD.ConnectedHealthMeter connectedHealthMeter = this.meter;
			this.owner = null;
			this.meter = null;
			if (entityHUD != null && connectedHealthMeter != null)
			{
				entityHUD.DisconnectHealthMeter(connectedHealthMeter);
			}
		}
	}

	struct ClickTrack
	{
		public GuiClickable click;

		public int btn;

		public ClickTrack(GuiClickable click, int btn)
		{
			this.click = click;
			this.btn = btn;
		}
	}
}
