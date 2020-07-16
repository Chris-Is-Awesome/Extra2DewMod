using System;
using UnityEngine;

namespace ModStuff
{
     public class UIVector3 : UIElement
     {
          public delegate void OnInteraction(Vector3 output);
          public event OnInteraction onInteraction;

          UISlider _slider;
          public UISlider Slider { get { return _slider; } }

          UIListExplorer _explorer;
          public UIListExplorer Explorer { get { return _explorer; } }

          public override void ShowName(bool state)
          {
               _explorer.ShowName(state);
          }

          //Output Value
          Vector3 _value;
          public Vector3 Value
          {
               get
               {
                    return _value;
               }
               set
               {
                    _value = value;
                    ExplorerActivated(false, _explorer.StringValue, 0);
                    if (_explorer.StringValue == "All directions") { _slider.Value = _value.x; }
               }
          }

          public void Initialize()
          {
               _value = Vector3.one;
               //Set TextMesh, slidable and textframe
               _slider = gameObject.GetComponentInChildren<UISlider>();
               _explorer = gameObject.GetComponentInChildren<UIListExplorer>();
               nameTextMesh = _slider.NameTextMesh;
               _slider.onInteraction += SliderActivated;
               _explorer.onInteraction += ExplorerActivated;
          }

          bool dontRunSliderFunction;
          void ExplorerActivated(bool rightArrow, string textValue, int arrayIndex)
          {
               dontRunSliderFunction = true; //We dont want the slider delegate to be activated during this function
               switch (textValue)
               {
                    case "X":
                         _slider.Value = _value.x;
                         break;
                    case "Y":
                         _slider.Value = _value.y;
                         break;
                    case "Z":
                         _slider.Value = _value.z;
                         break;
                    default:
                         break;
               }
               dontRunSliderFunction = false; //We reactivate the delegate
          }

          void SliderActivated(float value)
          {
               if (dontRunSliderFunction) { return; }
               switch (_explorer.StringValue)
               {
                    case "All directions":
                         _value = new Vector3(value, value, value);
                         break;
                    case "X":
                         _value.x = value;
                         break;
                    case "Y":
                         _value.y = value;
                         break;
                    case "Z":
                         _value.z = value;
                         break;
                    default:
                         break;
               }
               OutputVector();
          }

          public void Trigger()
          {
               if (onInteraction != null) { onInteraction(_value); }
          }

          public void Trigger(Vector3 vector)
          {
               Value = vector;
               if (onInteraction != null) { onInteraction(_value); }
          }

          void OutputVector()
          {
               if (onInteraction != null) { onInteraction(_value); }
          }
     }
}
