using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class CMW_Mechanism : CMenuWindow
    {
        const string DOWN_KEY_STRING = "Down key: ";
        const string UP_KEY_STRING = "Up key: ";
        const string ONOFF_KEY_STRING = "On/Off key: ";
        const string HOLD_KEY_STRING = "Hold key: ";
        const string CLICK_TO_SWITCH = "\nClick to switch";

        const float Y_SEPARATION = 1.3f;

        UICheckBox _mechaVisi;

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
            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 4.5f, menuGo.transform, "Mechanisms");

            //Escape button
            UIButton escape = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 4.0f, 4.5f, menuGo.transform, "Close");
            escape.ScaleBackground(new Vector2(0.5f, 1f));
            escape.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };

            //Type selector
            UIListExplorer typeOfMecha = UIFactory.Instance.CreateListExplorer(0f, 2.90f, menuGo.transform, "Type");
            typeOfMecha.AllowLooping = true;
            typeOfMecha.ScaleBackground(1.5f);
            typeOfMecha.ExplorerArray = new string[] { "Position", "Rotation", "Scale", "One axis Scale", "Position (toggle)", "Rotation (toggle)", "Scale (toggle)", "One axis scale (toggle)", "Permanent rotation" };
            typeOfMecha.transform.localPosition += new Vector3(0f, 0f, -0.3f);

            //Choose key buttons
            UIKeyInput keyButton0 = UIFactory.Instance.CreateKeyInput(-2f, 2.90f - Y_SEPARATION, menuGo.transform);
            keyButton0.Button.ScaleBackground(new Vector2(0.8f, 1.3f));
            keyButton0.Key = KeyCode.Q;
            UIKeyInput keyButton1 = UIFactory.Instance.CreateKeyInput(2.0f, 2.90f - Y_SEPARATION, menuGo.transform);
            keyButton1.Button.ScaleBackground(new Vector2(0.8f, 1.3f));
            keyButton1.Key = KeyCode.W;

            //Interpolation type
            UIListExplorer interpoType = UIFactory.Instance.CreateListExplorer(2.0f, 2.90f - Y_SEPARATION, menuGo.transform, "Interpolation");
            interpoType.ScaleBackground(0.8f);
            interpoType.ExplorerArray = new string[] { "Easy In/Out", "Easy In", "Easy Out", "Linear" };
            interpoType.AllowLooping = true;

            //Sliders
            UISlider leftSlider = UIFactory.Instance.CreateSlider(-1.65f, 2.90f - Y_SEPARATION * 2f, menuGo.transform, "LeftSlider");
            leftSlider.transform.localScale *= 0.75f;
            UISlider rightSlider = UIFactory.Instance.CreateSlider(2.65f, 2.90f - Y_SEPARATION * 2f, menuGo.transform, "RightSlider");
            rightSlider.transform.localScale *= 0.75f;

            //Control with mouse
            UICheckBox mouseCheckbox = UIFactory.Instance.CreateCheckBox(2.0f, 2.95f - Y_SEPARATION * 2f, menuGo.transform, "Mouse Controlled");
            //mouseCheckbox.ScaleBackground(0.8f, Vector3.one);
            mouseCheckbox.transform.localScale *= 0.8f;

            //Mech visibility
            _mechaVisi = UIFactory.Instance.CreateCheckBox(1.8f, 2.90f - Y_SEPARATION * 3f, menuGo.transform, "Visible mechanisms");
            _mechaVisi.ScaleBackground(1.2f, Vector3.one);
            _mechaVisi.Value = true;
            _mechaVisi.onInteraction += delegate (bool box)
            {
                MechanismManager.MeshVisibility = box;
            };

            //Spawn button
            UIButton spawnButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, -2.4f, 2.90f - Y_SEPARATION * 3f, menuGo.transform, "Create\nmechanism");
            spawnButton.ScaleBackground(new Vector2(0.8f, 1.3f));
            spawnButton.UIName = "Create\nMechanism";
            spawnButton.onInteraction += delegate ()
            {
                switch (typeOfMecha.IndexValue)
                {
                    //Position
                    case 0:
                        MakeMechanism(RemoteTransformControl.TransformType.POS, leftSlider.Value, mouseCheckbox.Value, keyButton0.Key, keyButton1.Key);
                        break;
                    //Rotation
                    case 1:
                        MakeMechanism(RemoteTransformControl.TransformType.ROT, leftSlider.Value, mouseCheckbox.Value, keyButton0.Key, keyButton1.Key);
                        break;
                    //Scale
                    case 2:
                        MakeMechanism(RemoteTransformControl.TransformType.SCALE, leftSlider.Value, mouseCheckbox.Value, keyButton0.Key, keyButton1.Key);
                        break;
                    //Scale one axis
                    case 3:
                        MakeMechanism(RemoteTransformControl.TransformType.ONE_AXIS_SCALE, leftSlider.Value, mouseCheckbox.Value, keyButton0.Key, keyButton1.Key);
                        break;
                    //Position (toggle)
                    case 4:
                        MakeToggleMechanism(RemoteTransformControlCyclic.TransformType.POS, interpoType.IndexValue, leftSlider.Value, rightSlider.Value, keyButton0.Key);
                        break;
                    //Rotation (toggle)
                    case 5:
                        MakeToggleMechanism(RemoteTransformControlCyclic.TransformType.ROT, interpoType.IndexValue, leftSlider.Value, rightSlider.Value, keyButton0.Key);
                        break;
                    //Scale (toggle)
                    case 6:
                        MakeToggleMechanism(RemoteTransformControlCyclic.TransformType.SCALE, interpoType.IndexValue, leftSlider.Value, rightSlider.Value, keyButton0.Key);
                        break;
                    //Scale one axis (toggle)
                    case 7:
                        MakeToggleMechanism(RemoteTransformControlCyclic.TransformType.ONE_AXIS_SCALE, interpoType.IndexValue, leftSlider.Value, rightSlider.Value, keyButton0.Key);
                        break;
                    //Permanent rotation
                    case 8:
                        MakePermanentRotationMechanism(leftSlider.Value, keyButton0.Key);
                        break;
                }
            };

            mouseCheckbox.onInteraction += delegate (bool checkbox)
            {
                SetKeyButtonsNames(typeOfMecha.IndexValue, mouseCheckbox.Value, keyButton0, keyButton1);
            };

            //UI change per type
            typeOfMecha.onInteraction += delegate (bool arrow, string text, int type)
            {
                switch(type)
                {
                    //Position
                    case 0:
                        leftSlider.gameObject.SetActive(true);
                        leftSlider.SetSlider(0.01f, 20f, 0.01f, 1f, true);
                        leftSlider.UIName = "Speed";
                        rightSlider.gameObject.SetActive(false);
                        rightSlider.SetSlider(0.01f, 20f, 0.01f, 1f, true);
                        rightSlider.UIName = "";
                        mouseCheckbox.gameObject.SetActive(true);
                        interpoType.gameObject.SetActive(false);
                        break;
                    //Rotation
                    case 1:
                        leftSlider.gameObject.SetActive(true);
                        leftSlider.SetSlider(0.01f, 1800f, 0.01f, 180f, true, 180f);
                        leftSlider.UIName = "Speed";
                        rightSlider.gameObject.SetActive(false);
                        rightSlider.SetSlider(0.01f, 20f, 0.01f, 1f, true);
                        rightSlider.UIName = "";
                        mouseCheckbox.gameObject.SetActive(true);
                        interpoType.gameObject.SetActive(false);
                        break;
                    //Scale
                    case 2:
                        leftSlider.gameObject.SetActive(true);
                        leftSlider.SetSlider(0.01f, 20f, 0.01f, 1f, true);
                        leftSlider.UIName = "Speed";
                        rightSlider.gameObject.SetActive(false);
                        rightSlider.SetSlider(0.01f, 20f, 0.01f, 1f, true);
                        rightSlider.UIName = "";
                        mouseCheckbox.gameObject.SetActive(true);
                        interpoType.gameObject.SetActive(false);
                        break;
                    //Scale one axis
                    case 3:
                        leftSlider.gameObject.SetActive(true);
                        leftSlider.SetSlider(0.01f, 20f, 0.01f, 1f, true);
                        leftSlider.UIName = "Speed";
                        rightSlider.gameObject.SetActive(false);
                        rightSlider.SetSlider(0.01f, 20f, 0.01f, 1f, true);
                        rightSlider.UIName = "";
                        mouseCheckbox.gameObject.SetActive(true);
                        interpoType.gameObject.SetActive(false);
                        break;
                    //Position (toggle)
                    case 4:
                        leftSlider.gameObject.SetActive(true);
                        leftSlider.SetSlider(0.10f, 60f, 0.01f, 1f, true);
                        leftSlider.UIName = "Time";
                        rightSlider.gameObject.SetActive(true);
                        rightSlider.SetSlider(0.01f, 10f, 0.01f, 1f, true);
                        rightSlider.UIName = "Final position";
                        mouseCheckbox.gameObject.SetActive(false);
                        interpoType.gameObject.SetActive(true);
                        break;
                    //Rotation (toggle)
                    case 5:
                        leftSlider.gameObject.SetActive(true);
                        leftSlider.SetSlider(0.10f, 60f, 0.01f, 1f, true);
                        leftSlider.UIName = "Time";
                        rightSlider.gameObject.SetActive(true);
                        rightSlider.SetSlider(0.01f, 720f, 0.01f, 180f, true, 180f);
                        rightSlider.UIName = "Final rotation";
                        mouseCheckbox.gameObject.SetActive(false);
                        interpoType.gameObject.SetActive(true);
                        break;
                    //Scale (toggle)
                    case 6:
                        leftSlider.gameObject.SetActive(true);
                        leftSlider.SetSlider(0.10f, 60f, 0.01f, 1f, true);
                        leftSlider.UIName = "Time";
                        rightSlider.gameObject.SetActive(true);
                        rightSlider.SetSlider(0.01f, 20f, 0.01f, 1f, true);
                        rightSlider.UIName = "Final scale";
                        mouseCheckbox.gameObject.SetActive(false);
                        interpoType.gameObject.SetActive(true);
                        break;
                    //Scale one axis (toggle)
                    case 7:
                        leftSlider.gameObject.SetActive(true);
                        leftSlider.SetSlider(0.10f, 60f, 0.01f, 1f, true);
                        leftSlider.UIName = "Time";
                        rightSlider.gameObject.SetActive(true);
                        rightSlider.SetSlider(0.01f, 20f, 0.01f, 1f, true);
                        rightSlider.UIName = "Final scale";
                        mouseCheckbox.gameObject.SetActive(false);
                        interpoType.gameObject.SetActive(true);
                        break;
                    //Permanent rotation
                    case 8:
                        leftSlider.gameObject.SetActive(true);
                        leftSlider.SetSlider(0.10f, 60f, 0.01f, 1f, true);
                        leftSlider.UIName = "Spin time";
                        rightSlider.gameObject.SetActive(false);
                        rightSlider.SetSlider(0.01f, 1800f, 0.01f, 180f, true, 180f);
                        rightSlider.UIName = "Rotation";
                        mouseCheckbox.gameObject.SetActive(false);
                        interpoType.gameObject.SetActive(false);
                        break;
                }
                SetKeyButtonsNames(type, mouseCheckbox.Value, keyButton0, keyButton1);
            };
            keyButton0.onKeyPressed += delegate (KeyCode key)
            {
                SetKeyButtonsNames(typeOfMecha.IndexValue, mouseCheckbox.Value, keyButton0, keyButton1);
            };
            keyButton1.onKeyPressed += delegate (KeyCode key)
            {
                SetKeyButtonsNames(typeOfMecha.IndexValue, mouseCheckbox.Value, keyButton0, keyButton1);
            };
            typeOfMecha.Trigger();
        }

        void SetKeyButtonsNames(int type, bool useMouse, UIKeyInput keyButton0, UIKeyInput keyButton1)
        {
            switch (type)
            {
                //Position
                case 0:
                    keyButton0.gameObject.SetActive(true);
                    keyButton0.UIName = useMouse ? HOLD_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH : DOWN_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH;
                    keyButton1.gameObject.SetActive(!useMouse);
                    keyButton1.UIName = UP_KEY_STRING + keyButton1.Key.ToString() + CLICK_TO_SWITCH;
                    break;
                //Rotation
                case 1:
                    keyButton0.gameObject.SetActive(true);
                    keyButton0.UIName = useMouse ? HOLD_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH : DOWN_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH;
                    keyButton1.gameObject.SetActive(!useMouse);
                    keyButton1.UIName = UP_KEY_STRING + keyButton1.Key.ToString() + CLICK_TO_SWITCH;
                    break;
                //Scale
                case 2:
                    keyButton0.gameObject.SetActive(true);
                    keyButton0.UIName = useMouse ? HOLD_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH : DOWN_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH;
                    keyButton1.gameObject.SetActive(!useMouse);
                    keyButton1.UIName = UP_KEY_STRING + keyButton1.Key.ToString() + CLICK_TO_SWITCH;
                    break;
                //Scale one axis
                case 3:
                    keyButton0.gameObject.SetActive(true);
                    keyButton0.UIName = useMouse ? HOLD_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH : DOWN_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH;
                    keyButton1.gameObject.SetActive(!useMouse);
                    keyButton1.UIName = UP_KEY_STRING + keyButton1.Key.ToString() + CLICK_TO_SWITCH;
                    break;
                //Position (toggle)
                case 4:
                    keyButton0.gameObject.SetActive(true);
                    keyButton0.UIName = ONOFF_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH;
                    keyButton1.gameObject.SetActive(false);
                    keyButton1.UIName = UP_KEY_STRING + keyButton1.Key.ToString() + CLICK_TO_SWITCH;
                    break;
                //Rotation (toggle)
                case 5:
                    keyButton0.gameObject.SetActive(true);
                    keyButton0.UIName = ONOFF_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH;
                    keyButton1.gameObject.SetActive(false);
                    keyButton1.UIName = UP_KEY_STRING + keyButton1.Key.ToString() + CLICK_TO_SWITCH;
                    break;
                //Scale (toggle)
                case 6:
                    keyButton0.gameObject.SetActive(true);
                    keyButton0.UIName = ONOFF_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH;
                    keyButton1.gameObject.SetActive(false);
                    keyButton1.UIName = UP_KEY_STRING + keyButton1.Key.ToString() + CLICK_TO_SWITCH;
                    break;
                //Scale one axis (toggle)
                case 7:
                    keyButton0.gameObject.SetActive(true);
                    keyButton0.UIName = ONOFF_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH;
                    keyButton1.gameObject.SetActive(false);
                    keyButton1.UIName = UP_KEY_STRING + keyButton1.Key.ToString() + CLICK_TO_SWITCH;
                    break;
                //Permanent rotation
                case 8:
                    keyButton0.gameObject.SetActive(true);
                    keyButton0.UIName = ONOFF_KEY_STRING + keyButton0.Key.ToString() + CLICK_TO_SWITCH;
                    keyButton1.gameObject.SetActive(false);
                    keyButton1.UIName = UP_KEY_STRING + keyButton1.Key.ToString() + CLICK_TO_SWITCH;
                    break;
            }
        }

        void MakeMechanism(RemoteTransformControl.TransformType type, float speed, bool useMouse, KeyCode key0, KeyCode key1)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = type.ToString() + "Mechanism";
            obj.transform.position = logic.player.transform.position + new Vector3(0f, 1f, 0f);
            if (type == RemoteTransformControl.TransformType.ROT)
            {
                obj.transform.Rotate(new Vector3(-90f, 0f, 0f));
            }
            else
            {
                obj.transform.Rotate(new Vector3(0f, 0f, -90f));
            }
            obj.GetComponent<MeshRenderer>().material.color = Color.green;
            RemoteTransformControl newComp = GameObjectUtility.GetOrAddComponent<RemoteTransformControl>(obj);
            newComp.ControlType = type;

            newComp.SetupControlMode(useMouse, false, speed);
            newComp.SetupKeys(key0, key1);
            logic.ForceSelect(obj.transform);
            logic.CloseCurrentMenu();
            logic.TriggerToolByPass(CMenuLogic.ToolList.MoveSpawned);
        }

        void MakeToggleMechanism(RemoteTransformControlCyclic.TransformType type, int interpTypeInt, float time, float target, KeyCode key0)
        {
            RemoteTransformControlCyclic.InterpType interpType = RemoteTransformControlCyclic.InterpType.EASYINOUT;
            if (interpTypeInt == 1) interpType = RemoteTransformControlCyclic.InterpType.EASYIN;
            else if (interpTypeInt == 2) interpType = RemoteTransformControlCyclic.InterpType.EASYOUT;
            else if (interpTypeInt == 3) interpType = RemoteTransformControlCyclic.InterpType.LINEAR;
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = type.ToString() + "TOGMechanism";
            obj.transform.position = logic.player.transform.position + new Vector3(0f, 1f, 0f);
            if (type == RemoteTransformControlCyclic.TransformType.ROT || type == RemoteTransformControlCyclic.TransformType.PERMA_ROT)
            {
                obj.transform.Rotate(new Vector3(-90f, 0f, 0f));
            }
            else
            {
                obj.transform.Rotate(new Vector3(0f, 0f, -90f));
            }
            obj.GetComponent<MeshRenderer>().material.color = Color.green;
            RemoteTransformControlCyclic newComp = GameObjectUtility.GetOrAddComponent<RemoteTransformControlCyclic>(obj);
            newComp.ControlType = type;

            newComp.SetupCurve(target, time, interpType);
            newComp.SetupKeys(key0);
            logic.ForceSelect(obj.transform);
            logic.CloseCurrentMenu();
            logic.TriggerToolByPass(CMenuLogic.ToolList.MoveSpawned);
        }

        void MakePermanentRotationMechanism(float time, KeyCode key0)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = "PermaRotTOGMechanism";
            obj.transform.position = logic.player.transform.position + new Vector3(0f, 1f, 0f);
            obj.transform.Rotate(new Vector3(-90f, 0f, 0f));
            obj.GetComponent<MeshRenderer>().material.color = Color.green;
            RemoteTransformPermaRot newComp = GameObjectUtility.GetOrAddComponent<RemoteTransformPermaRot>(obj);
            newComp.SetupTime(time);
            newComp.SetupKeys(key0);

            logic.ForceSelect(obj.transform);
            logic.CloseCurrentMenu();
            logic.TriggerToolByPass(CMenuLogic.ToolList.MoveSpawned);
        }

        public override void OnOpen()
        {
            _mechaVisi.Value = MechanismManager.MeshVisibility;
        }
    }
}
