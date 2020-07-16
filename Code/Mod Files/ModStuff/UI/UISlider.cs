using System;
using UnityEngine;

namespace ModStuff
{
    public class UISlider : UIElement
    {
        public delegate void OnInteraction(float output);
        public event OnInteraction onInteraction;
        GuiSlidable slidable;

        UITextFrame _display;
        public UITextFrame Display { get { return _display; } }

        //Display integers instead of float
        bool roundDisplayToInteger;
        public bool DisplayInteger
        {
            get { return roundDisplayToInteger; }
            set
            {
                roundDisplayToInteger = value;
                UpdateDisplayValue();
            }
        }

        //Enable/disable display
        public void EnableDisplay(bool activate)
        {
            _display.gameObject.SetActive(activate);
        }

        void UpdateDisplayValue()
        {
            string displayedValue = roundDisplayToInteger ? Mathf.Floor(_value).ToString("F0") : _value.ToString("F2");
            _display.UIName = displayedValue;
        }

        //Output range
        Range _range;
        public Range SliderRange
        {
            get { return _range; }
            set
            {
                _range = value;
                slidable.Range = _range;
                _value = Mathf.Clamp(_value, _range.min, _range.max);
                UpdateDisplayValue();
            }
        }

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

        //Calculate non-linear function
        //DisplayValue = A + B * Math.Exp(C * SliderValue);
        //SliderValue = Math.Log((DisplayValue - A) / B) / C;
        //x: min, y: mid, z: max
        //A = (xz - y²) / (x - 2y + z)
        //B = (y - x)² / (x - 2y + z)
        //C = 2 * log((z-y) / (y-x))

        float CalcNonLinear(float floatValue)
        {
            return Mathf.Clamp(A + B * Mathf.Exp(C * floatValue), _range.min, _range.max);
        }

        float CalcLinear(float floatValue)
        {
            return Mathf.Clamp(_range.min + floatValue * (_range.max - _range.min), _range.min, _range.max);
        }

        float A;
        float B;
        float C;
        bool nonLinear;
        public void SetSlider(float min, float max, float step, float initial, bool useNonLinear = false, float mid = 1f)
        {
            if(min == max) { return; }
            //If min is bigger than max, switch
            if(min > max)
            {
                float temp = max;
                max = min;
                min = temp;
            }
            if(useNonLinear)
            {
                if (mid == min || mid == max) { return; }
                useNonLinear = !((min - 2 * mid + max) == 0); //If the condition applies, use a linear function
            }
            nonLinear = useNonLinear;
            if(nonLinear)
            {
                SliderRange = new Range(0f, 1f);
                A = (min * max - mid * mid) / (min - 2 * mid + max);
                B = Mathf.Pow(mid - min, 2) / (min - 2 * mid + max);
                C = 2 * Mathf.Log((max - mid) / (mid - min));
                _range = new Range(min, max);
            }
            else
            {
                SliderRange = new Range(min, max);
            }
            SliderStep = step;
            Value = initial;
        }

        //Flag that dissallows delegate event while changing the value of the slider by code
        bool disallowDelegate;

        //Output Value
        float _value;
        public float Value
        {
            get
            {
                return roundDisplayToInteger ? (float)Math.Round(_value): _value;
            }
            set
            {
                float updateValue;
                if (nonLinear)
                {
                    updateValue = Mathf.Log((value - A) / B) / C;
                }
                else
                {
                    updateValue = (value - _range.min) / (_range.max - _range.min);
                }
                disallowDelegate = true;
                slidable.SetValue(updateValue, true);
                disallowDelegate = false;
                _value = Mathf.Clamp(value, _range.min, _range.max);
                UpdateDisplayValue();
            }
        }

        public void Initialize()
        {
            //Set TextMesh, slidable and textframe
            nameTextMesh = gameObject.GetComponentInChildren<TextMesh>();
            slidable = gameObject.GetComponentInChildren<GuiSlidable>();
            _display = gameObject.GetComponentInChildren<UITextFrame>();
            UpdateDisplayValue();

            //Find existing GUI node
            GuiNode node = gameObject.GetComponentInChildren<GuiNode>(true);
            if (node == null) { return; }

            //Link this component to the node
            GuiBindInData inData = new GuiBindInData(null, null);
            GuiBindData guiBindData = GuiNode.Connect(this.gameObject, inData);
            string trackerID = "Slider" + UIElement.GetNewTrackerID();
            guiBindData.AddTracker(trackerID, node);

            //Set delegate function
            IGuiOnchangeFloat tryfind = guiBindData.GetTrackerEvent<IGuiOnchangeFloat>(trackerID);
            if (tryfind == null) { return; }
            tryfind.onchange = new GuiNode.OnFloatFunc(SliderMoved);

            //Text resizing
            DefLineSize = 200f;
            OriginalTextSize = nameTextMesh.transform.localScale;
            AutoTextResize = true;
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
            _value = nonLinear ? CalcNonLinear(sliderValue) : CalcLinear(sliderValue);
            UpdateDisplayValue();
            if (onInteraction != null && !disallowDelegate) { onInteraction(_value); }
        }
    }
}
