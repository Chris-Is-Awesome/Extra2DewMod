using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class CMW_Lights : CMenuWindow
    {
        const float Y_SEPARATION = 1.1f;
        Color _lightColor = Color.white;
        UIImage _currentColor;
        UISlider _intensitySlider;
        UISlider _coneAngle;

        public override void BuildMenu()
        {
            upperLeftLimit = new Vector2(0.25f, 0.77f);
            lowerRightLimit = new Vector2(0.75f, 0.30f);

            UIBigFrame mainText = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1.5f, menuGo.transform);
            mainText.ScaleText(0.9f);
            mainText.ScaleBackground(new Vector2(0.70f, 1.5f));
            mainText.transform.localPosition += new Vector3(0f, 0f, 0.2f);

            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 4.5f, menuGo.transform, "Lights");
            UIButton escape = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 4.5f, 4.5f, menuGo.transform, "Close");
            escape.ScaleBackground(new Vector2(0.5f, 1f));
            escape.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };

            //Color example and background
            _currentColor = UIFactory.Instance.CreateImage("white", -5f, 4.3f, menuGo.transform);
            _currentColor.ImageScale = Vector3.one * 0.4f;
            _currentColor.transform.localPosition += new Vector3(0f, 0f, -0.2f);
            UIBigFrame currentColorBack = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, -5f, 4.3f, menuGo.transform);
            currentColorBack.ScaleBackground(new Vector2(0.17f * 0.5f, 0.65f * 0.5f));
            currentColorBack.transform.localPosition += new Vector3(0f, 0f, -0.1f);

            //Sliders title
            UITextFrame colorTitle = UIFactory.Instance.CreateTextFrame(-2.5f, 2.45f + Y_SEPARATION * 0.8f + 0.3f, menuGo.transform, "Light property");
            colorTitle.ScaleBackground(new Vector3(1.25f, 1f));
            colorTitle.transform.localScale *= 0.5f;

            //Sliders
            UISlider redSlider = UIFactory.Instance.CreateSlider(-2.5f, 2.70f, menuGo.transform, "Red");
            redSlider.transform.localScale = redSlider.transform.localScale * 0.75f;
            redSlider.SetSlider(0f, 1f, 0.01f, 1f);
            redSlider.onInteraction += delegate (float slider)
            {
                _lightColor.r = slider;
                MakeFlatColorTexture(_lightColor);
            };
            UISlider greenSlider = UIFactory.Instance.CreateSlider(-2.5f, 2.70f - Y_SEPARATION, menuGo.transform, "Green");
            greenSlider.transform.localScale = greenSlider.transform.localScale * 0.75f;
            greenSlider.SetSlider(0f, 1f, 0.01f, 1f);
            greenSlider.onInteraction += delegate (float slider)
            {
                _lightColor.g = slider;
                MakeFlatColorTexture(_lightColor);
            };
            UISlider blueSlider = UIFactory.Instance.CreateSlider(-2.5f, 2.70f - Y_SEPARATION * 2f, menuGo.transform, "Blue");
            blueSlider.transform.localScale = blueSlider.transform.localScale * 0.75f;
            blueSlider.SetSlider(0f, 1f, 0.01f, 1f);
            blueSlider.onInteraction += delegate (float slider)
            {
                _lightColor.b = slider;
                MakeFlatColorTexture(_lightColor);
            };

            //Intensity
            _intensitySlider = UIFactory.Instance.CreateSlider(-2.5f, 2.70f - Y_SEPARATION * 3f, menuGo.transform, "Intensity");
            _intensitySlider.transform.localScale = _intensitySlider.transform.localScale * 0.75f;
            _intensitySlider.SetSlider(0f, 10f, 0.01f, 1f, true, 1);

            //Global light
            UICheckBox toggleAmbience = UIFactory.Instance.CreateCheckBox(2.25f, 0.75f, menuGo.transform, "Global Light");
            toggleAmbience.Value = true;
            toggleAmbience.onInteraction += delegate (bool box)
            {
                GameObject directionalLight = GameObject.Find("Directional light");
                if (directionalLight != null)
                {
                    Light globalLight = directionalLight.GetComponent<Light>();
                    globalLight.enabled = box;
                }
            };

            //Change global light color
            UIButton changeGlobal = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 2.25f, 0.75f - Y_SEPARATION * 1.2f, menuGo.transform, "Apply properties\nto global light");
            changeGlobal.onInteraction += delegate ()
            {
                GameObject directionalLight = GameObject.Find("Directional light");
                if (directionalLight != null)
                {
                    Light globalLight = directionalLight.GetComponent<Light>();
                    globalLight.color = _lightColor;
                    globalLight.intensity = _intensitySlider.Value;
                }
            };
            changeGlobal.ScaleBackground(new Vector2(1f, 1.2f));
            UITextFrame popup = UIFactory.Instance.CreatePopupFrame(2.25f, 1.25f, changeGlobal, menuGo.transform, "Note: Global light could be reset\n after unpausing. Set 'Time flow' in the\nWorld Menu to 0 to keep the changes.");
            popup.ScaleBackground(Vector2.one * 1.5f);
            popup.transform.localPosition += new Vector3(0f, 0f, -0.4f);

            //Light type
            UIListExplorer lightType = UIFactory.Instance.CreateListExplorer(2.25f, 2.75f, menuGo.transform, "Light type");
            lightType.transform.localPosition += new Vector3(0f, 0f, -0.3f);
            lightType.ExplorerArray = new string[] { "Point", "Spot", "Directional" };
            lightType.AllowLooping = true;

            //Cone for spot light
            _coneAngle = UIFactory.Instance.CreateSlider(1.6f, 1.75f, menuGo.transform, "Angle");
            _coneAngle.SetSlider(1f, 90f, 0.01f, 45f);
            _coneAngle.transform.localScale *= 0.6f;
            _coneAngle.gameObject.SetActive(false);
            lightType.onInteraction += delegate (bool rightArrow, string textValue, int arrayIndex)
            {
                _coneAngle.gameObject.SetActive(arrayIndex == 1);
            };

            //Spawn light
            UIButton spawnButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 3.8f, 1.75f, menuGo.transform, "Spawn\nLight");
            spawnButton.ScaleBackground(new Vector2(0.5f, 1.2f), Vector2.one * 0.7f);
            spawnButton.onInteraction += delegate () { MakeLight(_lightColor, _intensitySlider.Value, lightType.IndexValue, _coneAngle.Value); };

            redSlider.Trigger();
            MakeFlatColorTexture(_lightColor);
        }

        void MakeFlatColorTexture(Color color)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Color[] colors = new Color[] { color, color, color, color };
            tex.SetPixels(colors);
            _currentColor.ApplyTexture(tex);
        }

        void MakeLight(Color color, float intensity, int type, float angle)
        {
            GameObject lightGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lightGameObject.name = "Light";
            lightGameObject.transform.localScale *= 0.4f;
            lightGameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
            Light lightComp = lightGameObject.AddComponent<Light>();
            lightGameObject.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            if (type == 0) lightComp.type = LightType.Point;
            else if(type == 1)
            {
                lightComp.type = LightType.Spot;
                lightComp.spotAngle = angle;
            }
            else
            {
                lightComp.type = LightType.Directional;
            }
            lightComp.color = color;
            lightComp.intensity = intensity;
            lightGameObject.transform.position = logic.player.position + new Vector3(0f, 2f, 0f);
            lightGameObject.AddComponent<LightMeshVisibility>();
            logic.ForceSelect(lightComp.transform);
            logic.CloseCurrentMenu();
            logic.TriggerToolByPass(CMenuLogic.ToolList.MoveSpawned);

        }
    }
}
