using UnityEngine;
using System.Collections;
using System.IO;

namespace ModStuff
{
    public class UIImage : UIElement
    {
        public void Initialize()
        {
            //Set TextMesh
            nameTextMesh = gameObject.GetComponentInChildren<TextMesh>();
        }
    }
}
