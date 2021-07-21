using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class CMW_LibraryDelete : CMenuWindow
    {
        UIBigFrame _backgroundConfirm;
        public override void BuildMenu()
        {
            upperLeftLimit = new Vector2(0.33f, 0.73f);
            lowerRightLimit = new Vector2(0.68f, 0.43f);
            
            //Title
            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 4.5f, menuGo.transform, "Library");

            //Background
            _backgroundConfirm = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 2f, menuGo.transform, "");
            _backgroundConfirm.ScaleBackground(new Vector2(0.5f, 1f));

            //Confirm
            UIButton confirmErase = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 2f, 0.4f, menuGo.transform);
            confirmErase.ScaleBackground(new Vector2(0.5f, 1f));
            confirmErase.onInteraction += delegate ()
            {
                CMenuStorage.Instance.DeleteItem(CMenuStorage.Instance.Index);
                logic.CloseCurrentMenu();
            };

            //Cancel
            UIButton cancelErase = UIFactory.Instance.CreateButton(UIButton.ButtonType.Back, -2f, 0.4f, menuGo.transform);
            cancelErase.ScaleBackground(new Vector2(0.5f, 1f));
            cancelErase.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };
        }

        public override void OnClose()
        {
            logic.OpenAndToggleMenu(CMenuLogic.MenuList.Library);
        }

        public override void OnOpen()
        {
            string[] itemList = CMenuStorage.Instance.ItemList();

            _backgroundConfirm.WriteText("\n\n\n\nRemove " + itemList[CMenuStorage.Instance.Index] + " from the library?");
        }
    }
}
