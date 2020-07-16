using System;
using UnityEngine;

namespace ModStuff
{
    public class UIButton : UIElement
    {
        public enum ButtonType { Default, Confirm, Back }
        public delegate void OnInteraction();
        public event OnInteraction onInteraction;
        public override string UIName
        {
            set
            {
                base.UIName = value;
            }
        }

        public void ScaleBackground(Vector2 backgroundScale, Vector2 uielementScale)
        {
            if (backgroundScale.x == 0f || backgroundScale.y == 0) { return; }
            OriginalTextSize = new Vector3(0.175f / backgroundScale.x, 0.175f / backgroundScale.y, 0.1f);
            NameTextMesh.transform.localScale = OriginalTextSize;
            
            gameObject.transform.localScale = new Vector3(uielementScale.x * backgroundScale.x, uielementScale.y * backgroundScale.y, 1f);

            LineSize = DefLineSize * backgroundScale.x;
            UIName = UIName;
        }

        public void Initialize()
        {
            //Set TextMesh
            nameTextMesh = gameObject.GetComponentInChildren<TextMesh>();

            //Find existing GUI node
            GuiNode node = gameObject.GetComponentInChildren<GuiNode>(true);
            if (node == null) { return; }

            //Link this component to the node
            GuiBindInData inData = new GuiBindInData(null, null);
            GuiBindData guiBindData = GuiNode.Connect(this.gameObject, inData);
            string trackerID = "Button" + UIElement.GetNewTrackerID();
            guiBindData.AddTracker(trackerID, node);

            //Set delegate function
            IGuiOnclick tryfind = guiBindData.GetTrackerEvent<IGuiOnclick>(trackerID);
            if (tryfind == null) { return; }
            tryfind.onclick = new GuiNode.OnVoidFunc(ButtonClicked);

            //Set text resizing variables
            DefLineSize = 275f;
            OriginalTextSize = nameTextMesh.transform.localScale;
            AutoTextResize = true;
        }

        public void Trigger()
        {
            ButtonClicked(null);
        }

        void ButtonClicked(object ctx)
        {
            if (onInteraction != null) { onInteraction(); }
        }
    }
}
