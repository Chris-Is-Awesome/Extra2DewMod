using System;
using UnityEngine;

public class EnterNameMenu : MonoBehaviour
{
	[SerializeField]
	bool _allowReturn;

	[SerializeField]
	bool _doneOnReturn = true;

	[SerializeField]
	public int _maxChars;

	[SerializeField]
	float _blinkTime = 0.5f;

	[SerializeField]
	GameObject _layout;

	[SerializeField]
	MappedInput _input;

	GuiWindow menuRoot;

	GuiSelectionObject trackNode;

	string currentText;

	bool currentResult;

	EnterNameMenu.OnDoneFunc onDone;

	MappedInput.ButtonEventListener charListener;

	TextInput.Listener textListener;

	float blinkTimer;

	bool hasCaret;

	int ignoreFirstFramesDestroyUnity;

	void Setup()
	{
		GuiBindInData inData = new GuiBindInData(null, null);
		GuiBindData guiBindData = GuiNode.Connect(this._layout, inData);
		this.menuRoot = guiBindData.GetTracker<GuiWindow>("enterNameRoot");
		guiBindData.GetTrackerEvent<IGuiOnclick>("enterName.done").onclick = new GuiNode.OnVoidFunc(this.ClickedDone);
		guiBindData.GetTrackerEvent<IGuiOnclick>("enterName.back").onclick = new GuiNode.OnVoidFunc(this.ClickedCancel);
		GuiNode tracker = guiBindData.GetTracker("enterName.value");
		if (tracker != null)
		{
			this.trackNode = tracker.GetComponent<GuiSelectionObject>();
		}
	}

	void Start()
	{
		if (this.menuRoot == null)
		{
			this.Setup();
		}
	}

	void OnDestroy()
	{
		this.ClearListeners();
	}

	void ClearListeners()
	{
		if (this.charListener != null)
		{
			this.charListener.Stop();
			this.charListener = null;
		}
	}

	public static string UpdateInputString(char c, string current, out bool doneSignal, bool doneOnReturn, bool allowReturn)
	{
		doneSignal = false;
		if (c != '\b')
		{
			if (c == '\n' || c == '\r')
			{
				if (doneOnReturn)
				{
					doneSignal = true;
					return current;
				}
				if (!allowReturn)
				{
					return current;
				}
			}
			return current + c;
		}
		if (current.Length > 0)
		{
			return current.Substring(0, current.Length - 1);
		}
		return string.Empty;
	}

	void Update()
	{
		if (this.ignoreFirstFramesDestroyUnity > 0)
		{
			this.ignoreFirstFramesDestroyUnity--;
			if (this.ignoreFirstFramesDestroyUnity == 0)
			{
				this.charListener = this._input.RegisterCharEvent(new MappedInput.CharEventFunc(this.GotChar), -1);
			}
			return;
		}
		this.blinkTimer -= Time.deltaTime;
		if (this.blinkTimer <= 0f)
		{
			if (!this.IsInputActive())
			{
				this.UpdateValue(false);
			}
			else
			{
				this.UpdateValue(!this.hasCaret);
			}
		}
	}

	void ClickedDone(object ctx)
	{
		this.currentResult = true;
		this.Hide();
	}

	void ClickedCancel(object ctx)
	{
		this.currentResult = false;
		this.Hide();
	}

	bool IsInputActive()
	{
		return this.trackNode != null && this.trackNode.IsSelected;
	}

	void UpdateValue(bool withCaret)
	{
		bool flag = this._maxChars > 0 && this.currentText.Length >= this._maxChars;
		if (this._maxChars > 0 && this.currentText.Length > this._maxChars)
		{
			this.currentText = this.currentText.Substring(0, this._maxChars);
		}
		GuiContentData guiContentData = new GuiContentData();
		this.hasCaret = false;
		if (!flag && withCaret)
		{
			guiContentData.SetValue("currentValue", this.currentText + "|");
			this.hasCaret = true;
		}
		else
		{
			guiContentData.SetValue("currentValue", this.currentText);
		}
		this.menuRoot.ApplyContent(guiContentData, true);
		this.blinkTimer = this._blinkTime;
	}

	void GotChar(char c)
	{
		if (!this.IsInputActive())
		{
			return;
		}
		bool flag;
		string a = EnterNameMenu.UpdateInputString(c, this.currentText, out flag, this._doneOnReturn, this._allowReturn);
		if (a != this.currentText)
		{
			this.currentText = a;
			this.UpdateValue(true);
		}
		if (flag && this.currentText.Length > 0)
		{
			this.ClickedDone(null);
		}
	}

	public void Show(string startText, GuiWindow prev, EnterNameMenu.OnDoneFunc onDone)
	{
		if (this.menuRoot == null)
		{
			this.Setup();
		}
		this.onDone = onDone;
		this.ignoreFirstFramesDestroyUnity = 2;
		this.currentText = (startText ?? string.Empty);
		GuiContentData guiContentData = new GuiContentData();
		guiContentData.SetValue("cloudWarn", Clouder.QuotaExceeded);
		this.menuRoot.ApplyContent(guiContentData, true);
		this.menuRoot.Show(null, prev);
		this.UpdateValue(true);
		this.blinkTimer = this._blinkTime;
		this.textListener = TextInput.Instance.GetText("entername", startText, new TextInput.OnGotStringFunc(this.OnGetTextInput));
	}

	void OnGetTextInput(bool success, string value)
	{
		if (success && !string.IsNullOrEmpty(value))
		{
			this.currentText = value;
			this.ClickedDone(null);
		}
		else
		{
			this.currentText = string.Empty;
			this.ClickedCancel(null);
		}
	}

	public void Hide()
	{
		EnterNameMenu.OnDoneFunc onDoneFunc = this.onDone;
		this.onDone = null;
		if (this.textListener != null)
		{
			this.textListener.Stop();
		}
		this.textListener = null;
		this.ClearListeners();
		this.menuRoot.Hide(null);
		if (onDoneFunc != null)
		{
			onDoneFunc(this.currentResult, this.currentText);
		}
	}

    //Added to stop double char writing in newgamemode screen
    public void StopListener()
    {
        if (this.textListener != null)
        {
            this.textListener.Stop();
        }
        this.textListener = null;
        this.ClearListeners();
    }
	public delegate void OnDoneFunc(bool success, string result);
}
