using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class CMW_LibraryRename : CMenuWindow
    {
        UITextInput _textInput;
        public override void BuildMenu()
        {
            upperLeftLimit = new Vector2(0.33f, 0.73f);
            lowerRightLimit = new Vector2(0.68f, 0.43f);
            
            //Title
            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 4.5f, menuGo.transform, "Library");

            //Background
            UIBigFrame backgroundConfirm = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 2f, menuGo.transform, "");
            backgroundConfirm.ScaleBackground(new Vector2(0.5f, 1f));

            //Text Input
            _textInput = UIFactory.Instance.CreateTextInput(0f, 2f, menuGo.transform);
            _textInput.transform.localPosition += new Vector3(0f, 0f, -0.2f);
            _textInput.onEnterPressed += delegate ()
            {
                Rename();
            };

            //Confirm
            UIButton confirmErase = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 2f, 0.4f, menuGo.transform);
            confirmErase.ScaleBackground(new Vector2(0.5f, 1f));
            confirmErase.onInteraction += delegate ()
            {
                Rename();
            };

            //Cancel
            UIButton cancelErase = UIFactory.Instance.CreateButton(UIButton.ButtonType.Back, -2f, 0.4f, menuGo.transform);
            cancelErase.ScaleBackground(new Vector2(0.5f, 1f));
            cancelErase.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };
        }

        void Rename()
        {
            CMenuStorage.Instance.RenameCurrent(_textInput.StringValue);
            _textInput.StringValue = null;
            logic.CloseCurrentMenu();
        }

        public override void OnClose()
        {
            logic.OpenAndToggleMenu(CMenuLogic.MenuList.Library);
        }
    }
}
