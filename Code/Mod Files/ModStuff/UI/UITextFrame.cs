using System;
using UnityEngine;

namespace ModStuff
{
    public class UITextFrame : UIElement
    {
        GameObject frameGfx;

        public override string UIName
        {
            set
            {
                base.UIName = value;
            }
        }

        public void Initialize()
        {
            //Set TextMesh
            nameTextMesh = gameObject.GetComponentInChildren<TextMesh>();

            //Set frame
            frameGfx = gameObject.transform.Find("ModUIElement").Find("Background").gameObject;

            //Set text resizing variables
            DefLineSize = 275f;
            OriginalTextSize = nameTextMesh.transform.localScale;
            AutoTextResize = true;
        }

        public void ScaleBackground(Vector2 backgroundScale)
        {
            frameGfx.transform.localScale = new Vector3(2f * backgroundScale.x, 2f * backgroundScale.y, 1f);
            LineSize = DefLineSize * backgroundScale.x;
            UIName = UIName;
        }
    }
}