using System;
using UnityEngine;
using System.Collections.Generic;

namespace ModStuff
{
    public class UIScrollMenu : UIElement
    {
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

        //Canvas
        GameObject canvasGO;
        public Transform Canvas { get { return canvasGO.transform; } }

        //Children UIElements 
        List<UIAnchor> canvasChildren;

        //Canvas parameters
        public static float initialY = 2.5f;
        float _height;
        public float CanvasHeight
        {
            get
            {
                return _height;
            }
            set
            {
                RefreshCanvas(value, _window, _emptySpace);
            }
        }

        float _window;
        public float CanvasWindow
        {
            get
            {
                return _window;
            }
            set
            {
                RefreshCanvas(_height, value, _emptySpace);
            }
        }

        float _emptySpace;
        public float DefaultEmptySpace { get { return 2f; } }
        public float EmptySpace
        {
            get
            {
                return _emptySpace;
            }
            set
            {
                RefreshCanvas(_height, _window, value);
            }
        }

        bool _dynamicHeight;
        public bool DynamicHeight
        {
            get { return _dynamicHeight; }
            set
            {
                _dynamicHeight = value;
                if (!value) { return; }
                float minY = 0f;
                for (int i = 0; i < canvasChildren.Count; i++)
                {
                    if (canvasChildren[i].YPos < minY) minY = canvasChildren[i].YPos;
                }
                CanvasHeight = -minY;
            }
        }

        //Scroll status
        float _scrollPos;
        public float ScrollPos
        {
            get
            {
                return _scrollPos;
            }
            set
            {
                _scrollPos = Mathf.Clamp(value, 0f, 1f);
                ScrollBar.Value = _scrollPos;
                Scroll();
            }
        }
        public float CurrentHeight { get { return _window >= (_height + _emptySpace) ? 0f : _scrollPos * ( _height + _emptySpace - _window ); } }

        void Scroll()
        {
            Canvas.localPosition = new Vector3(Canvas.localPosition.x, initialY + CurrentHeight, Canvas.localPosition.z);
            for(int i = 0; i < canvasChildren.Count; i++)
            {
                canvasChildren[i].UpdateVisibility();
            }
        }

        void RefreshCanvas(float height, float window, float emptySpace)
        {
            float oldCurrentHeight = CurrentHeight;
            _height = height;
            _window = window;
            _emptySpace = emptySpace;
            _scrollBar.Value = (_window >= (_height + _emptySpace)) ? 0f : oldCurrentHeight / (_height + _emptySpace - _window);
        }

        public void Initialize()
        {
            //Set scrollbar, textframe and canvas
            _scrollBar = gameObject.GetComponentInChildren<UIScrollBar>();
            title = gameObject.GetComponentInChildren<UITextFrame>();
            canvasGO = gameObject.transform.Find("Canvas").gameObject;

            //Initialize
            canvasChildren = new List<UIAnchor>();

            ScrollBar.onInteraction += ScrollBarMoved;
        }

        void ScrollBarMoved(float scrollValue)
        {
            ScrollPos = scrollValue;
        }

        //Functions to parent UIElements to the canvas
        public static float DefaultTolerance = 0.75f;
        public void Assign(UIElement element, float topThreshold, float bottomTreshhold)
        {
            if (DynamicHeight && (element.transform.localPosition.y) < -_height)
            {
                CanvasHeight = -element.transform.localPosition.y;
            }
            UIAnchor anchor = new UIAnchor(element, this, topThreshold, bottomTreshhold);
            canvasChildren.Add(anchor);
            anchor.UpdateVisibility();
        }

        public void Assign(UIElement element)
        {
            Assign(element, DefaultTolerance, DefaultTolerance);
        }

        public void Assign(GameObject element, float topThreshold, float bottomTreshhold)
        {
            if (DynamicHeight && (element.transform.localPosition.y) < -_height)
            {
                CanvasHeight = -element.transform.localPosition.y;
            }
            UIAnchor anchor = new UIAnchor(element, this, topThreshold, bottomTreshhold);
            canvasChildren.Add(anchor);
            anchor.UpdateVisibility();
        }

        public void Assign(GameObject element)
        {
            Assign(element, DefaultTolerance, DefaultTolerance);
        }

        //UIElement anchor class
        public class UIAnchor
        {
            GameObject anchor;
            UIScrollMenu menu;
            float _topThreshold;
            float _bottomTreshhold;

            public float YPos { get { return anchor.transform.localPosition.y; } }
            public float MinY { get { return YPos - _bottomTreshhold; } }
            public float MaxY { get { return YPos + _topThreshold; } }

            public UIAnchor(UIElement element, UIScrollMenu scrollMenu, float topThreshold, float bottomTreshhold)
            {
                _topThreshold = topThreshold;
                _bottomTreshhold = bottomTreshhold;
                menu = scrollMenu;

                anchor = new GameObject("Anchor");
                anchor.transform.SetParent(menu.Canvas, false);
                anchor.transform.localScale = Vector3.one;
                anchor.transform.localPosition = element.transform.localPosition;

                element.transform.localPosition = Vector3.zero;
                element.transform.SetParent(anchor.transform, false);
            }

            public UIAnchor(GameObject element, UIScrollMenu scrollMenu, float topThreshold, float bottomTreshhold)
            {
                _topThreshold = topThreshold;
                _bottomTreshhold = bottomTreshhold;
                menu = scrollMenu;

                anchor = new GameObject("Anchor");
                anchor.transform.SetParent(menu.Canvas, false);
                anchor.transform.localScale = Vector3.one;
                anchor.transform.localPosition = element.transform.localPosition;

                element.transform.localPosition = Vector3.zero;
                element.transform.SetParent(anchor.transform, false);
            }


            public bool UpdateVisibility()
            {
                bool output = (MaxY + menu.CurrentHeight) < 0f && (MinY + menu.CurrentHeight) >= -menu.CanvasWindow;
                anchor.SetActive(output);
                return output;
            }
        }
    }
}