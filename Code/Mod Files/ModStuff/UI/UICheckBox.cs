using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModStuff
{
    public class UICheckBox : UIElement
    {
        public delegate void OnInteraction(bool output);
        public event OnInteraction onInteraction;

        GuiSwitchable switchable;

        //Flag that dissallows delegate event while changing the value of the slider by code
        bool disallowDelegate;

        bool _value;
        public bool Value
        {
            get { return _value; }
            set
            {
                disallowDelegate = true;
                switchable.SetValue(value, true);
                disallowDelegate = false;
            }
        }

        public void ScaleBackground(float backgroundScale, Vector3 uielementScale)
        {
            if(backgroundScale == 0f) { return; }
            OriginalTextSize = new Vector3(0.3333333f / backgroundScale, 0.2f, 0.1f);
            foreach (Transform child in gameObject.transform.Find("ModUIElement"))
            {
                switch(child.name)
                {
                    case "Text":
                        child.localScale = OriginalTextSize;
                        break;
                    case "CheckBox":
                        child.localScale = new Vector3(0.8333333f / backgroundScale, 0.5f, 1f);
                        break;
                    case "CheckBoxX":
                        child.localScale = new Vector3(1f / backgroundScale, 0.6f, 1f);
                        break;
                    default:
                        break;
                }
            }
            LineSize = DefLineSize * backgroundScale;
            gameObject.transform.localScale = new Vector3(uielementScale.x * 0.6f * backgroundScale, uielementScale.y, 1f);
            UIName = UIName;
        }

        public void Initialize()
        {
            //Set TextMesh and switchable
            nameTextMesh = gameObject.GetComponentInChildren<TextMesh>();
            switchable = gameObject.GetComponentInChildren<GuiSwitchable>();

            //Find existing GUI node
            GuiNode node = gameObject.GetComponentInChildren<GuiNode>(true);
            if (node == null) { return; }

            //Link this component to the node
            GuiBindInData inData = new GuiBindInData(null, null);
            GuiBindData guiBindData = GuiNode.Connect(this.gameObject, inData);
            string trackerID = "CheckBox" + UIElement.GetNewTrackerID();
            guiBindData.AddTracker(trackerID, node);

            //Set delegate function
            IGuiOnchangeBool tryfind = guiBindData.GetTrackerEvent<IGuiOnchangeBool>(trackerID);
            if (tryfind == null) { return; }
            tryfind.onchange = new GuiNode.OnBoolFunc(CheckBoxClicked);

            DefLineSize = 185f;
            OriginalTextSize = nameTextMesh.transform.localScale;
            AutoTextResize = true;
        }

        public void Trigger()
        {
            if (onInteraction != null && !disallowDelegate) { onInteraction(_value); }
        }

        public void Trigger(bool checkBoxValue)
        {
            Value = checkBoxValue;
            if (onInteraction != null && !disallowDelegate) { onInteraction(_value); }
        }

        void CheckBoxClicked(bool output, object ctx)
        {
            _value = output;
            if (onInteraction != null && !disallowDelegate) { onInteraction(output); }
        }
    }
}
