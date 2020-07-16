using System;
using UnityEngine;

namespace ModStuff
{
    public class UIListExplorer : UIElement
    {
        //Delegate functions
        public delegate void OnInteraction(bool rightArrow, string textValue, int arrayIndex);
        public event OnInteraction onInteraction;

        //UI components
        UIButton leftArrow;
        UIButton rightArrow;
        UITextFrame valueDisplay;
        public UITextFrame ValueDisplay { get { return valueDisplay; } }
        UITextFrame title;
        public UITextFrame Title { get { return title; } }

        public override void ShowName(bool enable)
        {
            title.gameObject.SetActive(enable);
        }

        public override string UIName
        {
            get => title.UIName; set => title.UIName = value;
        }

        //Data handling
        string[] _explorerArray;
        public string[] ExplorerArray
        {
            get { return _explorerArray; }
            set
            {
                _explorerArray = value;
                if(_explorerArray == null) { _explorerArray = new string[] { }; }
                _arrayIndex = 0;
                UpdateText();
            }
        }

        int _arrayIndex;
        public int IndexValue
        {
            get { return _arrayIndex; }
            set
            {
                _arrayIndex = Mathf.Clamp(value, 0, _explorerArray.Length - 1);
                UpdateText();
            }
        }

        public string StringValue
        {
            get
            {
                if (_arrayIndex >= 0 && _arrayIndex < _explorerArray.Length) { return _explorerArray[_arrayIndex]; }
                return "NOLIST!";
            }
            set
            {
                for(int i = 0; i < _explorerArray.Length; i++)
                {
                    if (_explorerArray[i] == value)
                    {
                        _arrayIndex = i;
                        UpdateText();
                        return;
                    }
                }
                _arrayIndex = 0;
                UpdateText();
            }
        }

        void UpdateText()
        {
            valueDisplay.UIName = StringValue;
        }

        public void ScaleBackground(float scaleDistance)
        {
            if (scaleDistance <= 0f) { return; }
            valueDisplay.ScaleBackground(new Vector2(scaleDistance, 1f));
            leftArrow.transform.localPosition = new Vector3(-2f * scaleDistance, 0f, -0.2f);
            rightArrow.transform.localPosition = new Vector3(2f * scaleDistance, 0f, -0.2f);
        }

        public void Initialize()
        {
            //Set TextMeshes and buttons
            title = gameObject.transform.Find("UIName").GetComponent<UITextFrame>();
            //nameTextMesh = title.NameTextMesh;
            leftArrow = gameObject.transform.Find("LeftArrow").GetComponent<UIButton>();
            rightArrow = gameObject.transform.Find("RightArrow").GetComponent<UIButton>();
            valueDisplay = gameObject.transform.Find("Display").GetComponent<UITextFrame>();

            //Set arrows delegates
            leftArrow.onInteraction += LeftPressed;
            rightArrow.onInteraction += RightPressed;
        }

        void LeftPressed()
        {
            ButtonPressed(false);
        }

        void RightPressed()
        {
            ButtonPressed(true);
        }

        public void Trigger()
        {
            if (onInteraction != null) { onInteraction(false, StringValue, _arrayIndex); }
        }

        public void Trigger(bool rightArrow, string stringValue)
        {
            StringValue = stringValue;
            if (onInteraction != null) { onInteraction(rightArrow, StringValue, _arrayIndex); }
        }

        public void Trigger(bool rightArrow, int indexValue)
        {
            IndexValue = indexValue;
            if (onInteraction != null) { onInteraction(rightArrow, StringValue, _arrayIndex); }
        }

        public bool AllowLooping { get; set; }
        void ButtonPressed(bool rightArrow)
        {
            _arrayIndex += rightArrow ? 1 : -1;
            if (_arrayIndex < 0) { _arrayIndex = AllowLooping ? _explorerArray.Length - 1 : 0; }
            if (_arrayIndex >= _explorerArray.Length) { _arrayIndex = AllowLooping ? 0 : _explorerArray.Length - 1; }
            UpdateText();
            if (onInteraction != null) { onInteraction(rightArrow, StringValue, _arrayIndex); }
        }
    }
}