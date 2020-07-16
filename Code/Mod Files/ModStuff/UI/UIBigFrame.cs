using System;
using UnityEngine;

namespace ModStuff
{
    public class UIBigFrame : UIElement
    {
        public enum FrameType { Default, Scroll }

        public void Initialize()
        {
            //Set content text
            nameTextMesh = gameObject.transform.Find("Content").GetComponent<TextMesh>();
            nameTextMesh.alignment = TextAlignment.Left;
        }

        public void ScaleBackground(Vector2 backgroundScale)
        {
            if(backgroundScale.x == 0f || backgroundScale.y == 0f) { return; }
            gameObject.transform.localScale = new Vector3(backgroundScale.x, backgroundScale.y, 1f);
            nameTextMesh.transform.localScale = new Vector3(0.15f / backgroundScale.x, 0.15f / backgroundScale.y, 1f);
        }
    }
}
