using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff
{
    public class UIBigFrame : UIElement
    {
        public enum FrameType { Default, Scroll }

        static Vector2 originalVector = new Vector2(7.32f, 1.85f);
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
            //Set content text
            nameTextMesh = gameObject.transform.Find("Content").GetComponent<TextMesh>();
            nameTextMesh.alignment = TextAlignment.Left;

            DefLineSize = 1400f;
            textScale = 1f;
            OriginalTextSize = nameTextMesh.transform.localScale;
            AutoTextResize = true;
        }

        public void ScaleText(float scale)
        {
            if (scale == 0f) scale = 1f;
            LineSize = LineSize * textScale;
            LineSize = LineSize / scale;
            textScale = scale;
        }

        public void ScaleBackground(Vector2 backgroundScale)
        {
            gameObject.GetComponentInChildren<NineSlice>().Size = new Vector2(originalVector.x * backgroundScale.x, originalVector.y * backgroundScale.y);
            nameTextMesh.transform.localPosition = new Vector3(0f, 1.5f * backgroundScale.y, -0.2f);
            float mainMenuSize = (SceneManager.GetActiveScene().name == "MainMenu") ? 1.2f : 1f;
            LineSize = DefLineSize * mainMenuSize * backgroundScale.x / textScale;
        }

        public void WriteText(string text)
        {
            UIName = ModText.WrapText(text, LineSize, true);
        }
    }
}
