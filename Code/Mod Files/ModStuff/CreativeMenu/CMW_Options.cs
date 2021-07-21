using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class CMW_Options : CMenuWindow
    {
        const string DOWN_KEY_STRING = "Down key: ";
        const string UP_KEY_STRING = "Up key: ";
        const string ONOFF_KEY_STRING = "On/Off key: ";
        const string HOLD_KEY_STRING = "Hold key: ";
        const string CLICK_TO_SWITCH = "\nClick to switch";

        const float Y_SEPARATION = 1.5f;
        UIListExplorer _typeOfMarkers;
        UICheckBox _mechaVisi;
        UICheckBox _lightVisi;

        public override void BuildMenu()
        {
            upperLeftLimit = new Vector2(0.29f, 0.76f);
            lowerRightLimit = new Vector2(0.71f, 0.31f);

            //Background
            UIBigFrame mainText = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1.5f, menuGo.transform);
            mainText.ScaleText(0.9f);
            mainText.ScaleBackground(new Vector2(0.60f, 1.5f));
            mainText.transform.localPosition += new Vector3(0f, 0f, 0.2f);

            //Title
            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 4.5f, menuGo.transform, "Options");

            //Escape button
            UIButton escape = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 4.0f, 4.5f, menuGo.transform, "Close");
            escape.ScaleBackground(new Vector2(0.5f, 1f));
            escape.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };

            //Markers
            _typeOfMarkers = UIFactory.Instance.CreateListExplorer(0f, 2.90f, menuGo.transform, "Markers");
            _typeOfMarkers.AllowLooping = true;
            _typeOfMarkers.ExplorerArray = new string[] { "Default", "Always visible", "None"};
            _typeOfMarkers.transform.localPosition += new Vector3(0f, 0f, -0.3f);
            _typeOfMarkers.onInteraction += delegate (bool arrow, string name, int index)
            {
                logic.ToggleMarkers((CMenuLogic.MarkersState)index);
            };

            //Mech visibility
            _mechaVisi = UIFactory.Instance.CreateCheckBox(0f, 2.9f - Y_SEPARATION, menuGo.transform, "Visible mechanisms");
            _mechaVisi.ScaleBackground(1.2f, Vector3.one);
            _mechaVisi.Value = true;
            _mechaVisi.onInteraction += delegate (bool box)
            {
                MechanismManager.MeshVisibility = box;
            };

            //Lights visibility
            _lightVisi = UIFactory.Instance.CreateCheckBox(0f, 2.9f - Y_SEPARATION * 2f, menuGo.transform, "Visible light boxes");
            _lightVisi.ScaleBackground(1.2f, Vector3.one);
            _lightVisi.Value = true;
            _lightVisi.onInteraction += delegate (bool box)
            {
                LightMeshVisibility.MeshVisibility = box;
            };
        }

        public override void OnOpen()
        {
            _mechaVisi.Value = MechanismManager.MeshVisibility;
            _lightVisi.Value = LightMeshVisibility.MeshVisibility;
            _typeOfMarkers.IndexValue = (int)logic.markersState;
        }
    }
}
