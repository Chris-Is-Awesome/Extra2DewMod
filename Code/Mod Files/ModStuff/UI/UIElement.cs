using System;
using UnityEngine;

namespace ModStuff
{
    public abstract class UIElement : MonoBehaviour
    {
        //TextMesh for the title
        protected TextMesh nameTextMesh;
        public TextMesh NameTextMesh { get { return nameTextMesh; } }
        protected virtual void OnEnable()
        {
            if (nameTextMesh != null) { nameTextMesh.text = _name; }
        }

        //Show the TextMesh name
        public virtual void ShowName(bool enable) 
        {
            if (nameTextMesh == null) { return; }
            nameTextMesh.gameObject.SetActive(enable);
        }

        //Title of the UIElement
        string _name;
        public virtual string UIName
        {
            get { return _name; }
            set
            {
                _name = value;
                nameTextMesh.text = _name;
                if(AutoTextResize) ResizeTextMesh();
            }
        }

        //Update delegate (mainly used by text help)
        public delegate void UpdateHandler();
        public event UpdateHandler onUpdate;

        void Update()
        {
            if (onUpdate != null) { onUpdate(); }
        }

        //ID tracker counter for unique names
        static int uiElementCounter;
        public static int GetNewTrackerID()
        {
            uiElementCounter++;
            return uiElementCounter;
        }

        //Text resizing
        bool _resizeText;
        public virtual bool AutoTextResize
        {
            get { return _resizeText; }
            set
            {
                _resizeText = value;
                if (value)
                {
                    ResizeTextMesh();
                }
                else
                {
                    if (nameTextMesh != null) nameTextMesh.transform.localScale = OriginalTextSize;
                }
            }
        }

        Vector3 _originalTextMeshSize;
        protected Vector3 OriginalTextSize
        {
            get { return _originalTextMeshSize; }
            set
            {
                _originalTextMeshSize = value;
            }
        }

        float _defLineSize;
        protected float DefLineSize
        {
            get { return _defLineSize; }
            set
            {
                _defLineSize = value;
                LineSize = value;
            }
        }

        float _lineSize;
        public float LineSize
        {
            get { return _lineSize; }
            set
            {
                _lineSize = value;
            }
        }

        void ResizeTextMesh()
        {
            if (NameTextMesh == null || UIName == null || string.IsNullOrEmpty(UIName)) return;
            float textLength = ModText.Instance.GetMaxLineSize(UIName);
            if (textLength == 0f) return;
            NameTextMesh.transform.localScale = OriginalTextSize * ((textLength > LineSize) ? (LineSize / textLength) : 1f);
        }
    }
}
