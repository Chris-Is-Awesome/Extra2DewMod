using System;
using System.Collections.Generic;
using UnityEngine;

public class GuiSelectionHandler : MonoBehaviour
{
	[SerializeField]
	GuiSelectionHandlerData _sharedData;

	[SerializeField]
	MappedInput _input;

	[SerializeField]
	InputButton[] _confirmBtns;

	[SerializeField]
	InputButton[] _cancelButtons;

	[SerializeField]
	InputButton[] _hotkeys;

	[SerializeField]
	GuiSelectionObject _currentSelection;

	[SerializeField]
	Camera _camera;

	[SerializeField]
	string _cameraName = "OverlayCamera";

	[SerializeField]
	int _inputPrio = -1;

	MappedInput.ButtonEventListener dirListener;

	MappedInput.ButtonEventListener confirmListener;

	MappedInput.ButtonEventListener cancelListener;

	MappedInput.ButtonEventListener hotkeyListener;

	MappedInput.MouseListener mouseListener;

	Vector3 lastMouseDragPos;

	Vector3 lastMousePos;

	GuiClickRect currDragObject;

	GuiClickRect currMouseOver;

	bool gotWeakRects;

	bool updateSelection;

	List<GuiSelectionObject> savedSelectList = new List<GuiSelectionObject>();

	List<GuiClickRect> savedClickRects = new List<GuiClickRect>();

	List<GuiClickRect> savedRectObjects = new List<GuiClickRect>();

	public void Reselect()
	{
		if (this._currentSelection != null)
		{
			this._currentSelection.Deselect();
			this._currentSelection.Select(true);
		}
	}

	List<GuiSelectionObject> GetActiveSelectables(GuiSelectionObject except)
	{
		this.gotWeakRects = false;
		GuiSelectionObject[] componentsInChildren = base.GetComponentsInChildren<GuiSelectionObject>(false);
		this.savedSelectList.Clear();
		for (int i = componentsInChildren.Length - 1; i >= 0; i--)
		{
			GuiSelectionObject guiSelectionObject = componentsInChildren[i];
			if (guiSelectionObject != except && guiSelectionObject.Active)
			{
				this.savedSelectList.Add(guiSelectionObject);
			}
		}
		return this.savedSelectList;
	}

	static List<T> GetActiveLayerTagObjects<T>(GameObject root, T except) where T : GuiLayerTagObject
	{
		List<T> list = new List<T>();
		T[] componentsInChildren = root.GetComponentsInChildren<T>(false);
		for (int i = componentsInChildren.Length - 1; i >= 0; i--)
		{
			T t = componentsInChildren[i];
			if (t != except && t.IsActive)
			{
				list.Add(t);
			}
		}
		return list;
	}

	List<GuiCancelObjectTag> GetActiveCancelTags(GuiCancelObjectTag except)
	{
		return GuiSelectionHandler.GetActiveLayerTagObjects<GuiCancelObjectTag>(base.gameObject, except);
	}

	List<GuiHotkeyTag> GetActiveHotkeyTags(GuiHotkeyTag except)
	{
		return GuiSelectionHandler.GetActiveLayerTagObjects<GuiHotkeyTag>(base.gameObject, except);
	}

	List<GuiClickRect> GetActiveRects(List<GuiClickRect> objs, Vector2 atPos)
	{
		this.savedClickRects.Clear();
		for (int i = objs.Count - 1; i >= 0; i--)
		{
			GuiClickRect guiClickRect = objs[i];
			if (guiClickRect.IsActive && guiClickRect.GetActiveScreenRect(this._camera).Contains(atPos))
			{
				this.savedClickRects.Add(guiClickRect);
			}
		}
		return this.savedClickRects;
	}

	List<GuiClickRect> GetActiveClickRects(Vector2 atPos)
	{
		this.savedRectObjects.Clear();
		base.GetComponentsInChildren<GuiClickRect>(false, this.savedRectObjects);
		this.savedRectObjects.Sort((GuiClickRect a, GuiClickRect b) => (int)(a.ZIndex - b.ZIndex));
		return this.GetActiveRects(this.savedRectObjects, atPos);
	}

	List<GuiClickRect> GetActiveClickRectsWeak(Vector2 pos)
	{
		if (!this.gotWeakRects)
		{
			this.gotWeakRects = true;
			this.savedRectObjects.Clear();
			base.GetComponentsInChildren<GuiClickRect>(false, this.savedRectObjects);
			this.savedRectObjects.Sort((GuiClickRect a, GuiClickRect b) => (int)(a.ZIndex - b.ZIndex));
		}
		return this.GetActiveRects(this.savedRectObjects, pos);
	}

	public void InvalidateSelectionLists()
	{
		this.gotWeakRects = false;
	}

	static GuiSelectionObject GetTopObject(List<GuiSelectionObject> objs)
	{
		if (objs.Count == 0)
		{
			return null;
		}
		for (int i = objs.Count - 1; i >= 0; i--)
		{
			GuiPrioSelectTag component = objs[i].GetComponent<GuiPrioSelectTag>();
			if (component != null)
			{
				return objs[i];
			}
		}
		Vector3 vector = objs[objs.Count - 1].transform.position;
		GuiSelectionObject guiSelectionObject = objs[objs.Count - 1];
		for (int j = objs.Count - 2; j >= 0; j--)
		{
			if (guiSelectionObject.PrioSelection)
			{
				return guiSelectionObject;
			}
			Vector3 position = objs[j].transform.position;
			if (position.y > vector.y)
			{
				guiSelectionObject = objs[j];
				vector = position;
			}
		}
		return guiSelectionObject;
	}

    public GuiSelectionObject CurrentSelected { get { return _currentSelection; } }

    GuiSelectionObject CurrentSelection(List<GuiSelectionObject> objs)
	{
		if ((this._currentSelection == null || !this._currentSelection.Active) && objs.Count > 0)
		{
			this.ChangeSelection(GuiSelectionHandler.GetTopObject(objs), true);
		}
		return this._currentSelection;
	}

	static Vector2 SnapDir(Vector2 dir)
	{
		float num = Mathf.Atan2(dir.y, dir.x);
		num = Mathf.Round(num / 1.57079637f) * 3.14159274f * 0.5f;
		return new Vector2(Mathf.Cos(num), Mathf.Sin(num));
	}

	GuiSelectionObject GetNextObject(Vector2 dir)
	{
		List<GuiSelectionObject> activeSelectables = this.GetActiveSelectables(this._currentSelection);
		GuiSelectionObject guiSelectionObject = this.CurrentSelection(activeSelectables);
		if (guiSelectionObject != null)
		{
			Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
			dir = GuiSelectionHandler.SnapDir(dir);
			Vector3 vector = worldToLocalMatrix.MultiplyPoint3x4(guiSelectionObject.transform.position);
			float num = -1f;
			GuiSelectionObject result = null;
			for (int i = activeSelectables.Count - 1; i >= 0; i--)
			{
				Vector3 vector2 = worldToLocalMatrix.MultiplyPoint3x4(activeSelectables[i].transform.position) - vector;
				float num2 = Vector2.Dot(dir, new Vector2(vector2.x, vector2.y));
				float num3 = Mathf.Abs(vector2.x) + Mathf.Abs(vector2.y);
				if (num2 > 0.0001f && (num < 0f || num3 < num))
				{
					num = num3;
					result = activeSelectables[i];
				}
			}
			return result;
		}
		return null;
	}

	public void ChangeSelection(GuiSelectionObject next, bool quick)
	{
		if (next == this._currentSelection)
		{
			return;
		}
		if (this._currentSelection != null)
		{
			this._currentSelection.Deselect();
		}
		this._currentSelection = next;
		if (this._currentSelection != null)
		{
			this._currentSelection.Select(quick);
		}
	}

	bool ObjectConsumeDir(GuiSelectionObject sel, Vector2 dir)
	{
		if (sel == null)
		{
			return false;
		}
		GuiSlidable component = this._currentSelection.GetComponent<GuiSlidable>();
		if (component != null && component.UpdateValue(dir))
		{
			return true;
		}
		GuiSliderPusherObject component2 = this._currentSelection.GetComponent<GuiSliderPusherObject>();
		if (component2 != null && component2.UpdateValue(dir))
		{
			return true;
		}
		GuiDirConsumerBase component3 = this._currentSelection.GetComponent<GuiDirConsumerBase>();
		return component3 != null && component3.ConsumeDir(dir);
	}

	void MoveDirChanged(Vector2 dir, bool repeat)
	{
		if (dir.sqrMagnitude > 0.25f)
		{
			if (this.ObjectConsumeDir(this._currentSelection, dir))
			{
				return;
			}
			GuiSelectionObject nextObject = this.GetNextObject(dir);
			if (nextObject != null)
			{
				this.ChangeSelection(nextObject, false);
			}
		}
	}

	static void ClickObject(Component obj)
	{
		GuiClickable component = obj.GetComponent<GuiClickable>();
		GuiSwitchable component2 = obj.GetComponent<GuiSwitchable>();
		if (component != null)
		{
			component.SendClick();
		}
		if (component2 != null)
		{
			component2.SendClick();
		}
	}

	void ConfirmPressed(InputButton btn)
	{
		if (this._currentSelection != null)
		{
			GuiSelectionHandler.ClickObject(this._currentSelection);
		}
	}

	void CancelPressed(InputButton btn)
	{
		List<GuiCancelObjectTag> activeCancelTags = this.GetActiveCancelTags(null);
		for (int i = activeCancelTags.Count - 1; i >= 0; i--)
		{
			GuiCancelObjectTag guiCancelObjectTag = activeCancelTags[i];
			if (guiCancelObjectTag != null)
			{
				GuiSelectionHandler.ClickObject(guiCancelObjectTag);
				return;
			}
		}
	}

	void HotkeyPressed(InputButton btn)
	{
		List<GuiHotkeyTag> activeHotkeyTags = this.GetActiveHotkeyTags(null);
		for (int i = activeHotkeyTags.Count - 1; i >= 0; i--)
		{
			GuiHotkeyTag guiHotkeyTag = activeHotkeyTags[i];
			if (guiHotkeyTag != null && guiHotkeyTag.Hotkey == btn)
			{
				GuiSelectionHandler.ClickObject(guiHotkeyTag);
				return;
			}
		}
	}

	void SelectionDisabled(GuiSelectionObject sel)
	{
		if (sel == this._currentSelection)
		{
			this.SelectAny();
		}
	}

	void SelectAny()
	{
		List<GuiSelectionObject> activeSelectables = this.GetActiveSelectables(this._currentSelection);
		this.ChangeSelection(GuiSelectionHandler.GetTopObject(activeSelectables), true);
	}

	public void SelectTag(string tag, bool quick)
	{
		if (string.IsNullOrEmpty(tag))
		{
			return;
		}
		List<GuiSelectionObject> activeSelectables = this.GetActiveSelectables(this._currentSelection);
		foreach (GuiSelectionObject guiSelectionObject in activeSelectables)
		{
			GuiNode component = guiSelectionObject.GetComponent<GuiNode>();
			if (component != null && (string)component.ObjectTag == tag)
			{
				GuiSelectionObject component2 = guiSelectionObject.GetComponent<GuiSelectionObject>();
				if (component2 != null)
				{
					this.ChangeSelection(component2, quick);
					break;
				}
			}
		}
	}

	void Awake()
	{
		if (this._sharedData != null)
		{
			this._input = this._sharedData.Input;
			this._confirmBtns = this._sharedData.GetConfirmBtns();
			this._cancelButtons = this._sharedData.GetCancelButtons();
			this._hotkeys = this._sharedData.GetHotkeyButtons();
			if (string.IsNullOrEmpty(this._cameraName))
			{
				this._cameraName = this._sharedData.CameraName;
			}
		}
		if (this._camera == null)
		{
			Camera[] allCameras = Camera.allCameras;
			for (int i = allCameras.Length - 1; i >= 0; i--)
			{
				if (allCameras[i].name == this._cameraName)
				{
					this._camera = allCameras[i];
				}
			}
			if (this._camera == null)
			{
				this._camera = Camera.main;
			}
		}
	}

	void Start()
	{
		if (this._currentSelection == null)
		{
			this.SelectAny();
		}
		else
		{
			this.gotWeakRects = false;
			this.ChangeSelection(this._currentSelection, true);
		}
	}

	void OnEnable()
	{
		this.dirListener = this._input.RegisterMoveDir(new MappedInput.MoveDirEventFunc(this.MoveDirChanged), this._inputPrio);
		this.confirmListener = this._input.RegisterButtonDown(this._confirmBtns, new MappedInput.ButtonEventFunc(this.ConfirmPressed), this._inputPrio);
		this.cancelListener = this._input.RegisterButtonDown(this._cancelButtons, new MappedInput.ButtonEventFunc(this.CancelPressed), this._inputPrio);
		this.hotkeyListener = this._input.RegisterButtonDown(this._hotkeys, new MappedInput.ButtonEventFunc(this.HotkeyPressed), this._inputPrio);
		this.mouseListener = this._input.GetMouseListener(this._inputPrio);
	}

	void OnDisable()
	{
		GuiSelectionHandler.FreeListener(ref this.dirListener);
		GuiSelectionHandler.FreeListener(ref this.confirmListener);
		GuiSelectionHandler.FreeListener(ref this.cancelListener);
		GuiSelectionHandler.FreeListener(ref this.hotkeyListener);
		if (this.mouseListener != null)
		{
			this.mouseListener.Stop();
			this.mouseListener = null;
		}
		this.updateSelection = true;
	}

    //Added for the mod
    public void ListenerToggles(bool activate)
    {
        if(activate)
        {
            this.dirListener = this._input.RegisterMoveDir(new MappedInput.MoveDirEventFunc(this.MoveDirChanged), this._inputPrio);
            this.confirmListener = this._input.RegisterButtonDown(this._confirmBtns, new MappedInput.ButtonEventFunc(this.ConfirmPressed), this._inputPrio);
            this.cancelListener = this._input.RegisterButtonDown(this._cancelButtons, new MappedInput.ButtonEventFunc(this.CancelPressed), this._inputPrio);
            this.hotkeyListener = this._input.RegisterButtonDown(this._hotkeys, new MappedInput.ButtonEventFunc(this.HotkeyPressed), this._inputPrio);
        }
        else
        {
            GuiSelectionHandler.FreeListener(ref this.dirListener);
            GuiSelectionHandler.FreeListener(ref this.confirmListener);
            GuiSelectionHandler.FreeListener(ref this.cancelListener);
            GuiSelectionHandler.FreeListener(ref this.hotkeyListener);
        }
    }

	static void FreeListener(ref MappedInput.ButtonEventListener listener)
	{
		if (listener != null)
		{
			listener.Stop();
			listener = null;
		}
	}

	void UpdateMouse()
	{
		if (this._camera != null)
		{
			if (this.mouseListener.ButtonDown(0))
			{
				this.currDragObject = null;
				Vector3 mousePos = this.mouseListener.MousePos;
				List<GuiClickRect> activeClickRects = this.GetActiveClickRects(mousePos);
				if (activeClickRects.Count > 0)
				{
					GuiClickRect guiClickRect = activeClickRects[activeClickRects.Count - 1];
					GuiSlidable component = guiClickRect.GetComponent<GuiSlidable>();
					if (component != null)
					{
						GuiSelectionObject component2 = guiClickRect.GetComponent<GuiSelectionObject>();
						if (component2 != null)
						{
							this.ChangeSelection(component2, false);
						}
						Rect worldScreenRect = guiClickRect.GetWorldScreenRect(this._camera);
						component.UpdateAbs(new Vector2((mousePos.x - worldScreenRect.xMin) / worldScreenRect.width, (mousePos.y - worldScreenRect.yMin) / worldScreenRect.height));
						this.currDragObject = guiClickRect;
						this.lastMouseDragPos = mousePos;
					}
					else
					{
						GuiSelectionHandler.ClickObject(guiClickRect);
					}
				}
			}
			if (this.mouseListener == null)
			{
				return;
			}
			if (this.mouseListener.ButtonPressed(0) && this.currDragObject != null)
			{
				Vector3 mousePos2 = this.mouseListener.MousePos;
				if ((mousePos2 - this.lastMouseDragPos).sqrMagnitude > 4f)
				{
					this.lastMouseDragPos = mousePos2;
					GuiSlidable component3 = this.currDragObject.GetComponent<GuiSlidable>();
					if (component3 != null)
					{
						Rect worldScreenRect2 = this.currDragObject.GetWorldScreenRect(this._camera);
						component3.UpdateAbs(new Vector2((mousePos2.x - worldScreenRect2.xMin) / worldScreenRect2.width, (mousePos2.y - worldScreenRect2.yMin) / worldScreenRect2.height));
					}
				}
			}
			if (this.mouseListener == null)
			{
				return;
			}
			if (this.mouseListener.ButtonUp(0))
			{
				this.currDragObject = null;
			}
			if (this.mouseListener.IsActive && (this.lastMousePos - this.mouseListener.MousePos).sqrMagnitude > 4f)
			{
				this.lastMousePos = this.mouseListener.MousePos;
				List<GuiClickRect> activeClickRectsWeak = this.GetActiveClickRectsWeak(this.lastMousePos);
				if (activeClickRectsWeak.Count > 0)
				{
					GuiClickRect guiClickRect2 = activeClickRectsWeak[activeClickRectsWeak.Count - 1];
					if (this.currMouseOver != guiClickRect2)
					{
						GuiSelectionObject component4 = guiClickRect2.GetComponent<GuiSelectionObject>();
						if (component4 != null)
						{
							this.currMouseOver = guiClickRect2;
							if (this._currentSelection == component4)
							{
								this._currentSelection = null;
							}
							this.ChangeSelection(component4, false);
						}
					}
				}
				else
				{
					if (this.currMouseOver != null)
					{
						GuiSelectionObject component5 = this.currMouseOver.GetComponent<GuiSelectionObject>();
						if (component5 != null && component5 == this._currentSelection)
						{
							component5.Deselect();
						}
					}
					this.currMouseOver = null;
				}
			}
		}
	}

	void Update()
	{
		if (this.updateSelection || (this._currentSelection != null && !this._currentSelection.Active))
		{
			if (!this.updateSelection)
			{
				this._currentSelection.PrioSelection = true;
			}
			this.ChangeSelection(null, true);
			this.updateSelection = false;
			this.SelectAny();
		}
		this.UpdateMouse();
	}
}
