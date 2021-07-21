using System;
using UnityEngine;
using ModStuff;
using System.Collections.Generic;

namespace ModStuff
{
    public class UITextInput : UIElement
    {
        float _blinkTime = 0.5f;
        MappedInput _input;
        UITextFrame _textFrame;

        GuiSelectionObject trackNode;
        public delegate void OnKeyPressed();
        public event OnKeyPressed onEnterPressed;

        public delegate void OnValueChanged(string text);
        public event OnValueChanged onValueChanged;

        public bool OnlyActiveOnFocus { get; set; }

        //UIName is not used, its functionality is disabled to avoid confusing behavior
        public override string UIName
        {
            set
            {
            }
        }

        string currentText = "";

        MappedInput.ButtonEventListener charListener;

        MappedInput.ButtonEventListener dirListener;

        TextInput.Listener textListener;

        float blinkTimer;

        bool hasCaret;

        int ignoreFirstFramesDestroyUnity;

        int currentCursorPos = 0;

        public void Initialize(MappedInput input, UITextFrame textFrame)
        {
            _input = input;
            _textFrame = textFrame;

            //Find existing GUI node
            GuiNode node = gameObject.GetComponentInChildren<GuiNode>(true);

            //Link this component to the node
            GuiBindInData inData = new GuiBindInData(null, null);
            GuiBindData guiBindData = GuiNode.Connect(this.gameObject, inData);
            string trackerID = "Button" + UIElement.GetNewTrackerID();
            guiBindData.AddTracker(trackerID, node);

            trackNode = node.GetComponent<GuiSelectionObject>();
        }

        public string StringValue
        {
            get
            {
                return currentText;
            }
            set
            {
                if (value == null) value = "";
                currentText = value;
                currentCursorPos = currentText.Length;
                UpdateValue(true);
            }
        }

        void OnGetTextInput(bool success, string value)
        {
            if (success && !string.IsNullOrEmpty(value))
            {
                currentText = value;
                return;
            }
            ModText.QuickText("OnGetTextInput called with false success");
            currentText = string.Empty;
        }

        void ShowTextInput()
        {
            if (textListener != null && textListener.IsActive)
            {
                textListener.Stop();
            }
            textListener = TextInput.Instance.GetText("uiinputtext", string.Empty, new TextInput.OnGotStringFunc(OnGetTextInput));
        }

        void UpdateValue(bool withCaret)
        {
            hasCaret = false;
            ScrollText();
            string textToShow = "";
            int caretPosition = currentCursorPos - textScrollPos;

            if (currentText == null)
            {
                currentText = "";
            }
            int maxPos = textScrollPos + MAX_PROMPT_LENGTH;
            if (maxPos < currentText.Length)
            {
                textToShow = currentText.Substring(textScrollPos, MAX_PROMPT_LENGTH);
            }
            else
            {
                textToShow = currentText.Substring(textScrollPos);
            }
            if (withCaret)
            {
                _textFrame.UIName = textToShow.Insert(caretPosition, "|");
                hasCaret = true;
            }
            else
            {
                _textFrame.UIName = textToShow;
            }
            blinkTimer = _blinkTime;
        }

        void GotChar(char c)
        {
            if (!IsInputActive())
            {
                return;
            }
            if (currentText == null) currentText = "";
            bool flag;
            string a = UpdateInputString(c, currentText, out flag, true, false);
            if (a != currentText)
            {
                currentText = a;
                onValueChanged?.Invoke(currentText);
                UpdateValue(true);
            }
            if (flag)
            {
                onEnterPressed?.Invoke();
            }
        }

        string UpdateInputString(char c, string current, out bool doneSignal, bool doneOnReturn, bool allowReturn)
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

        void Update()
        {
            KeyEvents();
            if (ignoreFirstFramesDestroyUnity > 0)
            {
                ignoreFirstFramesDestroyUnity--;
                if (ignoreFirstFramesDestroyUnity == 0)
                {
                    charListener = _input.RegisterCharEvent(new MappedInput.CharEventFunc(GotChar), -1);
                }
                return;
            }
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0f)
            {
                if (!IsInputActive())
                {
                    UpdateValue(false);
                }
                else
                {
                    UpdateValue(!hasCaret);
                }
            }
        }

        protected override void OnEnable(/*GuiWindow prev, DebugMenu.OnDoneFunc onDone*/)
        {
            base.OnEnable();
            /*if (this.menuRoot == null)
            {
                this.Setup();
            }
            this.onDone = onDone;*/
            this.ignoreFirstFramesDestroyUnity = 2;
            //this.currentText = string.Empty;
            currentCursorPos = 0;
            //this.menuRoot.Show(null, prev);
            this.UpdateValue(true);
            this.blinkTimer = this._blinkTime;
            this.ShowTextInput();
        }

        public void ScaleBackground(Vector2 backgroundScale)
        {
            gameObject.GetComponentInChildren<GuiClickRect>().SetSizeAndCenter(new Vector2(4.75f * backgroundScale.x, backgroundScale.y), Vector2.zero);
            _textFrame.ScaleBackground(backgroundScale);
        }

        protected void OnDisable()
        {
            //DebugMenu.OnDoneFunc onDoneFunc = this.onDone;
            //this.onDone = null;
            ClearListeners();
            /*this.menuRoot.Hide(null);
            if (onDoneFunc != null)
            {
                onDoneFunc();
            }*/
        }

        void ClearListeners()
        {
            if (charListener != null)
            {
                charListener.Stop();
                charListener = null;
            }
            if (dirListener != null)
            {
                dirListener.Stop();
                dirListener = null;
            }
        }

        void OnDestroy()
        {
            ClearListeners();
        }

        float keyDownTimer;
        float keyDowntimerReset = 0.5f;
        bool everyOtherFrame;
        void KeyEvents()
        {
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
                onValueChanged?.Invoke(currentText);
                UpdateValue(true);
            }
            if (Input.GetKeyDown(KeyCode.C) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                GUIUtility.systemCopyBuffer = currentText;
            }
            if (Input.GetKeyDown(KeyCode.H) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                currentText = "";
                currentCursorPos = 0;
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

        bool IsInputActive()
        {
            if (!OnlyActiveOnFocus) return true;
            return this.trackNode != null && this.trackNode.IsSelected;
        }

        void DeleteKeyPress()
        {
            if (currentCursorPos != currentText.Length)
            {
                currentText = currentText.Remove(currentCursorPos, 1);
            }
            UpdateValue(true);
        }

        int textScrollPos;
        const int MAX_PROMPT_LENGTH = 38;
        void UpdateCursorPos(int posChange)
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

        void ScrollText()
        {
            if (currentCursorPos == 0) textScrollPos = 0;
            else if (currentCursorPos < textScrollPos) textScrollPos = currentCursorPos;
            else if (currentCursorPos > (textScrollPos + MAX_PROMPT_LENGTH)) textScrollPos = currentCursorPos - MAX_PROMPT_LENGTH;
        }
    }
}
