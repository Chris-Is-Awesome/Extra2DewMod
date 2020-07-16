using UnityEngine;
using System.Collections;

namespace ModStuff
{
    public class UIGFXButton : UIElement
    {
        public delegate void OnInteraction();
        public event OnInteraction onInteraction;

        //Button graphics functions
        GameObject buttonGfx;
        public GameObject ButtonGfx { get { return buttonGfx; } }

        public void ScaleBackground(Vector2 backgroundScale)
        {
            buttonGfx.transform.localScale = new Vector3(backgroundScale.x / 0.5f, backgroundScale.y / 1.6f, 1f);
        }

        public void Initialize()
        {
            //Set TextMesh
            nameTextMesh = gameObject.GetComponentInChildren<TextMesh>();

            //Set button gfx
            buttonGfx = gameObject.transform.Find("ModButton").Find("ModUIElement").Find("Root").Find("ImgTexture").gameObject;

            //Find existing GUI node
            GuiNode node = gameObject.GetComponentInChildren<GuiNode>(true);
            if (node == null) { return; }

            //Link this component to the node
            GuiBindInData inData = new GuiBindInData(null, null);
            GuiBindData guiBindData = GuiNode.Connect(this.gameObject, inData);
            string trackerID = "ButtonGFX" + UIElement.GetNewTrackerID();
            guiBindData.AddTracker(trackerID, node);

            //Set delegate function
            IGuiOnclick tryfind = guiBindData.GetTrackerEvent<IGuiOnclick>(trackerID);
            if (tryfind == null) { return; }
            tryfind.onclick = new GuiNode.OnVoidFunc(ButtonClicked);

            //Text resizing
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
