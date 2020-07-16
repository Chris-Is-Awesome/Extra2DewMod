using System;
using UnityEngine;
using ModStuff;
using System.Collections.Generic;

public class DebugMenu : MonoBehaviour
{
	[SerializeField]
	float _blinkTime = 0.5f;

	[SerializeField]
	GameObject _layout;

	[SerializeField]
	MappedInput _input;

	[SerializeField]
	public FadeEffectData _fadeData;

	public SaverOwner _saver;

	[SerializeField]
	GameVersion _version;

	GuiWindow menuRoot;

	string currentText;

	DebugMenu.OnDoneFunc onDone;

	MappedInput.ButtonEventListener charListener;

	MappedInput.ButtonEventListener dirListener;

	TextInput.Listener textListener;

	float blinkTimer;

	bool hasCaret;

	int ignoreFirstFramesDestroyUnity;

	DebugCommands commands;

	GameObject pauseOverlay;

	public string errorArgs = "\nError: Not enough arguments given";

	public string errorType = "\nError: Type given is not a valid type";

	public string errorNull = "\nError: Reference not found";

	public string errorOutOfRange = "\nError: Number given is out of range";

	string versionCredit = "Modded by: Chris is Awesome & Matra";

	int currentCursorPos = 0;

	GameObject backBtn;

	GameObject confirmBtn;

	int currPrevPos = 0;

	void Setup ()
	{
		this.pauseOverlay = GameObject.Find("PauseOverlay");
		this.commands = GameObject.Find("Debugger").GetComponent<DebugCommands>();
		this.PrettyUI();
		GuiBindInData inData = new GuiBindInData(null, null);
		GuiBindData guiBindData = GuiNode.Connect(this._layout, inData);
		this.menuRoot = guiBindData.GetTracker<GuiWindow>("debugRoot");
		guiBindData.GetTrackerEvent<IGuiOnclick>("debug.done").onclick = new GuiNode.OnVoidFunc(this.ClickedDone);
		guiBindData.GetTrackerEvent<IGuiOnclick>("debug.back").onclick = new GuiNode.OnVoidFunc(this.ClickedCancel);
		GuiContentData guiContentData = new GuiContentData();
		guiContentData.SetValue("version", this.versionCredit);
		this.menuRoot.ApplyContent(guiContentData, true);
		TextMesh component = this.pauseOverlay.transform.Find("Pause").Find("Debug").Find("Title").Find("Text").GetComponent<TextMesh>();
		component.text = "Extra 2 Dew (" + ModVersion.GetModVersion() + ")";
		TextMesh component2 = this.pauseOverlay.transform.Find("Pause").Find("Debug").Find("Version").GetComponent<TextMesh>();
		component2.text = "\n\n\n" + this.versionCredit;
		this.PrettyTextMesh(component, Color.black, TextAlignment.Center, 35, FontStyle.Normal, 0f);
		this.PrettyTextMesh(component2, Color.cyan, TextAlignment.Center, 25, FontStyle.Italic, 0f);
		UpdateOutput("                     Welcome to the Extra 2 Dew Debug menu!\n\n" +
				    ModStuff.ModMaster.GetDebugMenuHelp() +
				   "Enter 'help' to see the command list.");
	}

	void Start ()
	{
		if (this.menuRoot == null)
		{
			this.Setup();
		}
	}

	void OnGetTextInput (bool success, string value)
	{
		if (success && !string.IsNullOrEmpty(value))
		{
			this.currentText = value;
			this.ClickedDone(null);
			return;
		}
		this.currentText = string.Empty;
		currentCursorPos = 0;
		this.ClickedCancel(null);
	}

	void ClickedDone (object ctx)
	{
		if (currentText.Length == 0)
		{
			return;
		}
		commands.ParseResultString(currentText, true);
		if (currentText.ToLower() != "clearhistory")
		{
			if (commands.prevComs.Count == 0)
			{
				commands.prevComs.Add(currentText);
			}
			if (commands.prevComs[commands.prevComs.Count - 1] != currentText)
			{
				commands.prevComs.Add(currentText);
			}
		}
		currPrevPos = commands.prevComs.Count;
		currentText = string.Empty;
		currentCursorPos = 0;
		UpdateValue(true);
		ShowTextInput();
	}

	void ClickedCancel (object ctx)
	{
		this.Hide();
	}

	void ShowTextInput ()
	{
		if (this.textListener != null && this.textListener.IsActive)
		{
			this.textListener.Stop();
		}
		this.textListener = TextInput.Instance.GetText("command", string.Empty, new TextInput.OnGotStringFunc(this.OnGetTextInput));
	}

	void MoveCurrCom (int dir)
	{
		currPrevPos = Mathf.Clamp(currPrevPos + dir, 0, commands.prevComs.Count);
		if (currPrevPos == commands.prevComs.Count)
		{
			currentText = "";
		}
		else
		{
			currentText = commands.prevComs[currPrevPos];
		}
		currentCursorPos = currentText.Length;
		UpdateValue(true);
	}

	void GotKeyDir (Vector2 dir, bool repeat)
	{
		if (!repeat)
		{
			if (dir.y > 0.5f)
			{
				this.MoveCurrCom(-1);
				return;
			}
			if (dir.y < -0.5f)
			{
				this.MoveCurrCom(1);
			}
		}
	}

	void UpdateValue (bool withCaret)
	{
		GuiContentData guiContentData = new GuiContentData();
		this.hasCaret = false;
		if (withCaret)
		{
			guiContentData.SetValue("currentValue", this.currentText.Insert(currentCursorPos, "|"));
			this.hasCaret = true;
		}
		else
		{
			guiContentData.SetValue("currentValue", this.currentText);
		}
		this.menuRoot.ApplyContent(guiContentData, true);
		this.blinkTimer = this._blinkTime;
	}

	void GotChar (char c)
	{
		bool flag;
		string a = UpdateInputString(c, this.currentText, out flag, true, false);
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

	string UpdateInputString (char c, string current, out bool doneSignal, bool doneOnReturn, bool allowReturn)
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
			currentCursorPos++;
			return current.Insert(currentCursorPos - 1, c.ToString());
		}
		if (current.Length > 0)
		{
			if (currentCursorPos == 0) { return current; }
			string textoutput = current.Remove(currentCursorPos - 1, 1);
			UpdateCursorPos(-1);
			return textoutput;
		}
		return string.Empty;
	}

	void Update ()
	{
		this.KeyEvents();
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
			this.UpdateValue(!this.hasCaret);
		}
	}

	public void Show (GuiWindow prev, DebugMenu.OnDoneFunc onDone)
	{
		if (this.menuRoot == null)
		{
			this.Setup();
		}
		this.onDone = onDone;
		this.ignoreFirstFramesDestroyUnity = 2;
		this.currentText = string.Empty;
		currentCursorPos = 0;
		currPrevPos = commands.prevComs.Count;
		this.menuRoot.Show(null, prev);
		this.UpdateValue(true);
		this.blinkTimer = this._blinkTime;
		this.ShowTextInput();
	}

	public void Hide ()
	{
		DebugMenu.OnDoneFunc onDoneFunc = this.onDone;
		this.onDone = null;
		this.ClearListeners();
		this.menuRoot.Hide(null);
		if (onDoneFunc != null)
		{
			onDoneFunc();
		}
	}

	void ClearListeners ()
	{
		if (this.charListener != null)
		{
			this.charListener.Stop();
			this.charListener = null;
		}
		if (this.dirListener != null)
		{
			this.dirListener.Stop();
			this.dirListener = null;
		}
	}

	void OnDestroy ()
	{
		this.ClearListeners();
	}

	public void UpdateOutput (string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (text.Contains("\n"))
		{
			string str = "";
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\n')
				{
					str += "\n";
				}
			}
			text = str + text;
		}
		TextMesh component = this.pauseOverlay.transform.Find("Pause").Find("Debug").Find("InfoValue").Find("Text").GetComponent<TextMesh>();
		if (text.ToLower().Contains("error") || text.ToLower().Contains("unknown command"))
		{
			if (text.Length >= 200)
			{
				Color color = new Color(0.8f, 0f, 0f);
				this.PrettyTextMesh(component, color, TextAlignment.Left, 15, FontStyle.Normal, 0f);
			}
			else
			{
				this.PrettyTextMesh(component, Color.red, TextAlignment.Left, 25, FontStyle.Normal, 0f);
			}
		}
		else if (text.ToLower().Contains("warning") || text.ToLower().Contains("caution"))
		{
			Color color = new Color(1f, 0.75f, 0.1f);
			this.PrettyTextMesh(component, color, TextAlignment.Left, 25, FontStyle.Normal, 0f);
		}
		else if (!text.ToLower().Contains("error") && !text.ToLower().Contains("unknown command") && !text.ToLower().Contains("warning") && !text.ToLower().Contains("caution"))
		{
			this.PrettyTextMesh(component, Color.black, TextAlignment.Left, 25, FontStyle.Normal, 0f);
		}
		this.OutputText(text);
	}

	void OutputText (string info)
	{
		GuiContentData guiContentData = new GuiContentData();
		guiContentData.SetValue("currentInfo", info);
		this.menuRoot.ApplyContent(guiContentData, true);
	}

	void PrettyTextMesh (TextMesh mesh, Color color, TextAlignment align = TextAlignment.Center, int size = 25, FontStyle style = FontStyle.Normal, float offset = 0f)
	{
		mesh.color = color;
		mesh.alignment = align;
		mesh.fontSize = size;
		mesh.fontStyle = style;
		mesh.offsetZ = offset;
	}

	float keyDownTimer;
	float keyDowntimerReset = 0.5f;
	bool everyOtherFrame;
	void KeyEvents ()
	{
		if (Input.GetKeyDown(KeyCode.PageUp) && commands.prevComs.Count > 0)
		{
			MoveCurrCom(-1);
			return;
		}
		if (Input.GetKeyDown(KeyCode.PageDown))
		{
			MoveCurrCom(1);
			return;
		}
		if (Input.GetKeyDown(KeyCode.Home))
		{
			UpdateCursorPos(-1);
			keyDownTimer = keyDowntimerReset;
		}
		if (Input.GetKeyDown(KeyCode.End))
		{
			UpdateCursorPos(1);
			keyDownTimer = keyDowntimerReset;
		}
		if (Input.GetKeyDown(KeyCode.Delete))
		{
			DeleteKeyPress();
			keyDownTimer = keyDowntimerReset;
		}
		if (Input.GetKeyDown(KeyCode.V) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
		{
			string textToPaste = GUIUtility.systemCopyBuffer;
			textToPaste = textToPaste.Replace('\n', '|');
			currentText = currentText.Insert(currentCursorPos, textToPaste);
			currentCursorPos += textToPaste.Length;
			UpdateValue(true);
		}
		if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
		{
			GUIUtility.systemCopyBuffer = currentText;
		}
		if (Input.GetKeyDown(KeyCode.H) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
		{
			currentText = "";
			commands.prevComs = new List<string>();
			currentCursorPos = 0;
			currPrevPos = 0;
			UpdateValue(true);
		}
		if (Input.GetKey(KeyCode.Home) || Input.GetKey(KeyCode.End) || Input.GetKey(KeyCode.Delete))
		{
			keyDownTimer -= Time.deltaTime;
			everyOtherFrame = !everyOtherFrame;
			if (keyDownTimer < 0 && everyOtherFrame)
			{
				if (Input.GetKey(KeyCode.Delete)) { DeleteKeyPress(); }
				else { UpdateCursorPos(Input.GetKey(KeyCode.Home) ? -1 : 1); }
			}
		}
	}

	void DeleteKeyPress ()
	{
		if (currentCursorPos != currentText.Length)
		{
			currentText = currentText.Remove(currentCursorPos, 1);
		}
		UpdateValue(true);
	}

	void UpdateCursorPos (int posChange)
	{
		if (posChange > 0)
		{
			currentCursorPos++;
		}
		else
		{
			currentCursorPos--;
		}
		currentCursorPos = Mathf.Clamp(currentCursorPos, 0, currentText.Length);
		UpdateValue(true);
	}

	void PrettyUI ()
	{
		GameObject gameObject = this.pauseOverlay.transform.Find("Pause").transform.Find("Debug").gameObject;
		GameObject gameObject2 = gameObject.transform.Find("Version").gameObject;
		GameObject gameObject3 = gameObject.transform.Find("Title").gameObject;
		GameObject gameObject4 = gameObject.transform.Find("Back").gameObject;
		GameObject gameObject5 = gameObject.transform.Find("Confirm").gameObject;
		GameObject gameObject6 = gameObject.transform.Find("StringValue").gameObject;
		GameObject gameObject7 = gameObject.transform.Find("InfoValue").gameObject;
		GameObject gameObject8 = gameObject7.transform.Find("Text").gameObject;
		GameObject gameObject9 = UnityEngine.Object.Instantiate<GameObject>(this.pauseOverlay.transform.Find("Pause").transform.Find("Main").transform.Find("Layout").transform.Find("Background").gameObject, gameObject7.transform);
		gameObject4.transform.localPosition = new Vector3(-2.5f, -4f, 0f);
		gameObject5.transform.localPosition = new Vector3(2.5f, -4f, 0f);
		gameObject7.transform.localPosition = new Vector3(-0.02f, 1.5f, 0f);
		gameObject9.transform.localPosition = new Vector3(0f, -1.75f, 0f);
		gameObject9.transform.localScale = new Vector3(1.8f, 2.5f, 1f);
		gameObject2.transform.localPosition = new Vector3(3f, 4.75f, -0.1791036f);
		gameObject6.transform.localPosition = new Vector3(-0.02f, 3f, 0f);
		gameObject8.transform.localPosition = new Vector3(-6.32f, 0.1375f, -0.18f);
	}

	public delegate void OnDoneFunc ();

	delegate void CommandFunc (string com, string[] args);
}
