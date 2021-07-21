using System;
using UnityEngine;

namespace ModStuff
{
    public class UIButton : UIElement
    {
        public enum ButtonType { Default, Confirm, Back }
        public delegate void OnInteraction();
        public event OnInteraction onInteraction;

        static Vector2 originalVector = new Vector2(2f, 0.5f);

        public void ScaleBackground(Vector2 backgroundScale, Vector2 uielementScale)
        {
            gameObject.GetComponentInChildren<GuiClickRect>().SetSizeAndCenter(new Vector2(4f * backgroundScale.x, backgroundScale.y), Vector2.zero);
            gameObject.GetComponentInChildren<NineSlice>().Size = new Vector2(originalVector.x * backgroundScale.x, originalVector.y * backgroundScale.y);

            gameObject.transform.localScale = new Vector3(uielementScale.x, uielementScale.y, gameObject.transform.localScale.z);

            LineSize = DefLineSize * backgroundScale.x;
            AutoTextResize = AutoTextResize;
        }

        public void ScaleBackground(Vector2 backgroundScale)
        {
            gameObject.GetComponentInChildren<GuiClickRect>().SetSizeAndCenter(new Vector2(4f * backgroundScale.x, backgroundScale.y), Vector2.zero);
            gameObject.GetComponentInChildren<NineSlice>().Size = new Vector2(originalVector.x * backgroundScale.x, originalVector.y * backgroundScale.y);

            LineSize = DefLineSize * backgroundScale.x;
            AutoTextResize = AutoTextResize;
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
