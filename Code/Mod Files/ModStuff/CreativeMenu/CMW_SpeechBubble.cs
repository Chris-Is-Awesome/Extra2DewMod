using ModStuff;
using ModStuff.CreativeMenu;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class CMW_SpeechBubble : CMenuWindow
    {
        const string LINE_JUMP = "<>";
        public static string currentText;

        UIBigFrame _textPreview;
        public override void BuildMenu()
        {
            upperLeftLimit = new Vector2(0.29f, 0.76f);
            lowerRightLimit = new Vector2(0.71f, 0.31f);

            currentText = "Hello!";

            //Background
            UIBigFrame background = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1.5f, menuGo.transform);
            background.ScaleText(0.9f);
            background.ScaleBackground(new Vector2(0.60f, 1.5f));
            background.transform.localPosition += new Vector3(0f, 0f, 0.2f);

            //Text preview
            _textPreview = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 2f, menuGo.transform);
            _textPreview.ScaleText(0.9f);
            _textPreview.ScaleBackground(new Vector2(0.40f, 0.75f));
            _textPreview.transform.localPosition += new Vector3(0f, 0f, -0.2f);

            //Go back
            UIButton escape = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 4f, 4.5f, menuGo.transform, "Close");
            escape.ScaleBackground(new Vector2(0.5f, 1f));
            escape.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };

            //Text Preview title
            UITextFrame preview = UIFactory.Instance.CreateTextFrame(0f, 3.5f, menuGo.transform, "Text Preview");
            preview.transform.localPosition += new Vector3(0f, 0f, -0.4f);
            preview.transform.localScale *= 0.8f;

            //Title
            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 4.5f, menuGo.transform, "Speech bubble");

            //Edit
            UIButton editText = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -2f, -0.5f, menuGo.transform, "Edit");
            editText.ScaleBackground(new Vector2(0.75f, 1f));
            editText.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.BubblesEdit);
            };

            //Spawn
            UIButton spawn = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 2f, -0.5f, menuGo.transform, "Spawn");
            spawn.ScaleBackground(new Vector2(0.75f, 1f));
            spawn.onInteraction += delegate ()
            {
                if (string.IsNullOrEmpty(currentText)) return;
                GameObject bubble = DebugCommands.Instance.GetSavedSpeechBubble();
                if(bubble == null)
                {
                    currentText = "ERROR: No speech bubble saved.\n\nGo to Fluffy fields and try again";
                    SetPreviewText();
                    return;
                }
                
                //Bubble instantiated in a place where the player might not be, otherwise its behaviour gets weird
                Vector3 initialPosition = new Vector3(876f, -753f, 169f);

                //Bubble instantiation
                GameObject instantiatedGo = GameObject.Instantiate(bubble, initialPosition, new Quaternion(0f, 0f, 0f, 0f));
                instantiatedGo.name = "ModSpeechBubble";

                //Bubble configuration
                Sign sign = instantiatedGo.GetComponent<Sign>();
                string textToUse = GetConvertedText(currentText);
                sign.ChangeText(textToUse);
                sign.SetArrow(true);
                sign.GivePlayerTransform(logic.player.transform);
                sign.SetOffset(Vector3.up + Vector3.down);
                sign.Radius(1.4f);
                instantiatedGo.SetActive(true);

                //Set radius and move to the proper spot
                instantiatedGo.transform.localPosition = logic.player.transform.position;
                sign.Hide();
                sign.SetCustomMrSpeaker(instantiatedGo.transform);
                sign.ForceAlwaysShow(true);
               
                logic.ForceSelect(sign.transform);
                logic.CloseCurrentMenu();
                logic.TriggerToolByPass(CMenuLogic.ToolList.MoveSpawned);
            };
        }

        //Delay drag
        IEnumerator SelectWithDelay(GameObject instantiatedGo, float time)
        {
            yield return new WaitForSeconds(time);
            if (instantiatedGo != null)
            {
                logic.ForceSelect(instantiatedGo.transform);
                logic.CloseCurrentMenu();
                logic.TriggerToolByPass(CMenuLogic.ToolList.MoveSpawned);
            }
        }

        string GetConvertedText(string text)
        {
            if (text == null) text = "";
            return text.Replace(LINE_JUMP, "\n");
        }

        void SetPreviewText()
        {
            string textToUse = GetConvertedText(currentText);
            _textPreview.WriteText("\n\n" + textToUse);
        }

        public override void OnOpen()
        {
            SetPreviewText();
        }
    }
}
