using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff
{
    public class UITextFrame : UIElement
    {
        GameObject frameGfx;

        static Vector2 originalVector = new Vector2(2f, 0.5f);
        float textScale;

        public override string UIName
        {
            set
            {
                base.UIName = value;
                nameTextMesh.transform.localScale *= textScale;
            }
        }

        public void Initialize()
        {
            //Set TextMesh
            nameTextMesh = gameObject.GetComponentInChildren<TextMesh>();

            //Set frame
            frameGfx = gameObject.transform.Find("ModUIElement").Find("Background").gameObject;

            //Set text resizing variables
            DefLineSize = 295f;
            textScale = 1f;
            OriginalTextSize = nameTextMesh.transform.localScale;
            AutoTextResize = true;
        }

        public void ScaleBackground(Vector2 backgroundScale)
        {
            gameObject.GetComponentInChildren<NineSlice>().Size = new Vector2(originalVector.x * backgroundScale.x, originalVector.y * backgroundScale.y);
            //float mainMenuSize = (SceneManager.GetActiveScene().name == "MainMenu") ? 1.2f : 1f;
            LineSize = DefLineSize * backgroundScale.x / textScale; //* mainMenuSize
            UIName = UIName;
        }

        public void ScaleText(float scale)
        {
            if (scale == 0f) scale = 1f;
            LineSize = LineSize * textScale;
            LineSize = LineSize / scale;
            textScale = scale;
            UIName = UIName;
        }

        public void WriteText(string text)
        {
            UIName = ModText.WrapText(text, LineSize, false);
        }
    }
}