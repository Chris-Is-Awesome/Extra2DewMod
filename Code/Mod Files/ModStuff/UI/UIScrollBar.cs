using System;
using UnityEngine;

namespace ModStuff
{
    public class UIScrollBar : UIElement
    {
        public delegate void OnInteraction(float output);
        public event OnInteraction onInteraction;
        GuiSlidable slidable;

        //Slider step
        float _step;
        public float SliderStep
        {
            get { return _step; }
            set
            {
                _step = value;
                slidable.Step = _step;
            }
        }

        //Saves the rope pieces array into the class
        GameObject[] ropePieces;
        public void SaveRopePieces(GameObject[] pieces)
        {
            if (ropePieces == null) ropePieces = pieces;
        }

        //Rope Resizing. Can be done only once per ScrollBar
        bool resized;
        public void ResizeLength(int length)
        {
            if (resized || length == 8 || length < 2) return;
            resized = true;
            GameObject elementCreated = gameObject.transform.Find("ModUIElement").gameObject;
            float yShift = 1.08f * (8f - length);

            //Save all rope pieces in an array
            Transform sliderEnd = elementCreated.transform.Find("SliderEnd");
            
            if (length < 8)
            {
                //Delete unused rope pieces
                for (int i = 7; i > (length - 1); i--)
                {
                    if (ropePieces[i - 1] != null)
                    {
                        Destroy(ropePieces[i - 1]);
                    }
                }

                //Move end of the rope to the proper position
                Vector3 endPosChange = new Vector3(0f, yShift, 0f);
                ropePieces[7].transform.localPosition += endPosChange;
                sliderEnd.localPosition += endPosChange;
            }
            else
            {
                //Add new rope pieces
                for (int i = 8; i < length; i++)
                {
                    GameObject newPiece = Instantiate(ropePieces[2], elementCreated.transform);
                    newPiece.transform.localPosition = new Vector3(0f, -5.13f - (i - 7) * 1.08f + 1.1f, 0.35f);
                }

                //Move end of the rope to the proper position
                Vector3 endPosChange = new Vector3(0f, yShift, 0f);
                ropePieces[7].transform.localPosition += endPosChange;
                sliderEnd.localPosition += endPosChange;
            }

            //Fix Y axis offset
            yShift = 1.05f * (8f - length);
            elementCreated.GetComponentInChildren<GuiClickRect>().SetSizeAndCenter(new Vector2(1f, 7.3f - yShift * 1.02f), new Vector2(0f, -2.6f + 0.915f * yShift));
        }

        //Flag that dissallows delegate event while changing the value of the slider by code
        bool disallowDelegate;

        //Output Value
        float _value;
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                disallowDelegate = true;
                _value = Mathf.Clamp(value, 0f, 1f);
                slidable.SetValue(_value, true);
                disallowDelegate = false;
            }
        }

        public void Initialize()
        {
            //Set slidable
            //nameTextMesh = gameObject.GetComponentInChildren<TextMesh>();
            slidable = gameObject.GetComponentInChildren<GuiSlidable>();

            //Find existing GUI node
            GuiNode node = gameObject.GetComponentInChildren<GuiNode>(true);
            if (node == null) { return; }

            //Link this component to the node
            GuiBindInData inData = new GuiBindInData(null, null);
            GuiBindData guiBindData = GuiNode.Connect(this.gameObject, inData);
            string trackerID = "ScrollBar" + UIElement.GetNewTrackerID();
            guiBindData.AddTracker(trackerID, node);

            //Set delegate function
            IGuiOnchangeFloat tryfind = guiBindData.GetTrackerEvent<IGuiOnchangeFloat>(trackerID);
            if (tryfind == null) { return; }
            tryfind.onchange = new GuiNode.OnFloatFunc(SliderMoved);

            //Set default step
            SliderStep = 0.01f;
        }

        public void Trigger()
        {
            if (onInteraction != null && !disallowDelegate) { onInteraction(_value); }
        }

        public void Trigger(float sliderValue)
        {
            Value = sliderValue;
            if (onInteraction != null && !disallowDelegate) { onInteraction(_value); }
        }

        void SliderMoved(float sliderValue, object ctx)
        {
            Value = sliderValue;
            if (onInteraction != null && !disallowDelegate) { onInteraction(_value); }
        }
    }
}
