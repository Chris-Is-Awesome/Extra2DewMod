using System;
using UnityEngine;
using System.Collections.Generic;

namespace ModStuff
{
    public class UI2DList : UIElement
    {
        //Delegate functions
        public delegate void OnInteraction(string textValue, int arrayIndex);
        public event OnInteraction onInteraction;

        //The title frame of the menu
        UITextFrame title;
        public UITextFrame Title { get { return title; } }
        public override string UIName
        {
            get => title.UIName; set => title.UIName = value;
        }
        public override void ShowName(bool enable)
        {
            if (title == null) { return; }
            title.gameObject.SetActive(enable);
        }

        //Scrollbar
        UIScrollBar _scrollBar;
        public UIScrollBar ScrollBar { get { return _scrollBar; } }

        //Buttons
        UIButton[] buttonsArray;
        int btnsX;
        bool arraySet;
        public void AssignButtonsArray(UIButton[] btnArray, int x)
        {
            if (arraySet) return;
            arraySet = true;
            if (x < 1) x = 1;
            btnsX = x;
            buttonsArray = btnArray;

            for (int i = 0; i < btnArray.Length; i++)
            {
                int j = i;
                btnArray[i].onInteraction += delegate () { ButtonPressed(btnArray[j].UIName, j); };
            }
        }

        //Highlight selection
        bool _highlightSelected;
        public bool HighlightSelection
        {
            get { return _highlightSelected; }
            set
            {
                _highlightSelected = value;
                UpdateArray();
            }
        }

        //Text resizing
        bool resizingArray;
        public override bool AutoTextResize
        {
            get
            {
                return resizingArray;
            }
            set
            {
                resizingArray = value;
                if (buttonsArray != null)
                {
                    for (int i = 0; i < buttonsArray.Length; i++)
                    {
                        buttonsArray[i].AutoTextResize = value;
                    }
                }
                UpdateArray();
            }
        }

        //Button highlight
        Color _highlightColor;
        public Color HighLightColor
        {
            get { return _highlightColor; }
            set
            {
                _highlightColor = value;
                UpdateArray();
            }
        }

        //Data handling
        string[] _explorerArray;
        float scrollDiv;
        public string[] ExplorerArray
        {
            get { return _explorerArray; }
            set
            {
                _explorerArray = value;
                offset = 0;
                _arrayIndex = 0;
                ScrollBar.Value = 0f;
                if (_explorerArray == null) { _explorerArray = new string[] { }; }
                float extraRows = Mathf.Ceil(((float)_explorerArray.Length - (float)buttonsArray.Length) / (float)btnsX);
                if (extraRows < 0f) extraRows = 1f;
                scrollDiv = 1f / (extraRows + 1f);
                ScrollBar.SliderStep = 1f / extraRows;
                UpdateArray();
            }
        }

        int _arrayIndex;
        public int IndexValue
        {
            get { return _arrayIndex; }
            set
            {
                _arrayIndex = Mathf.Clamp(value, 0, _explorerArray.Length - 1);
                UpdateArray();
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
                for (int i = 0; i < _explorerArray.Length; i++)
                {
                    if (_explorerArray[i] == value)
                    {
                        _arrayIndex = i;
                        UpdateArray();
                        return;
                    }
                }
                _arrayIndex = 0;
                UpdateArray();
            }
        }

        //Scroll status
        int offset;
        void UpdateArray()
        {
            if(buttonsArray == null) { return; }
            for (int i = 0; i < buttonsArray.Length; i++)
            {
                int index = i + offset;
                if (index < _explorerArray.Length)
                {
                    buttonsArray[i].gameObject.SetActive(true);
                    buttonsArray[i].UIName = _explorerArray[index];
                    buttonsArray[i].NameTextMesh.color = (IndexValue == index && _highlightSelected) ? _highlightColor : Color.black;
                }
                else
                {
                    buttonsArray[i].gameObject.SetActive(false);
                }
            }
        }

        public void Initialize()
        {
            //Set scrollbar, textframe and canvas
            _scrollBar = gameObject.GetComponentInChildren<UIScrollBar>();
            title = gameObject.GetComponentInChildren<UITextFrame>();
            _explorerArray = new string[] { };
            ScrollBar.SliderStep = 1f;
            scrollDiv = 1f;
            _highlightColor = Color.red;
            _highlightSelected = true;
            AutoTextResize = true;

            ScrollBar.onInteraction += ScrollBarMoved;
        }

        void ScrollBarMoved(float scrollValue)
        {
            scrollValue = Mathf.Clamp(scrollValue, 0f, 0.99f);
            offset = btnsX * (int)Mathf.Floor(scrollValue / scrollDiv);
            UpdateArray();
        }

        public void Trigger()
        {
            if (onInteraction != null) { onInteraction(StringValue, _arrayIndex); }
        }

        public void Trigger(string stringValue)
        {
            StringValue = stringValue;
            if (onInteraction != null) { onInteraction(StringValue, _arrayIndex); }
        }

        public void Trigger(int indexValue)
        {
            IndexValue = indexValue + offset;
            if (onInteraction != null) { onInteraction(StringValue, _arrayIndex); }
        }

        void ButtonPressed(string textValue, int arrayIndex)
        {
            IndexValue = arrayIndex + offset;
            if (onInteraction != null) { onInteraction(textValue, _arrayIndex); }
        }
    }
}