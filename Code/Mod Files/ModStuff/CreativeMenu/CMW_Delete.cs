using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class CMW_Delete : CMenuWindow
    {
        UIBigFrame backgroundConfirm;
        public override void BuildMenu()
        {
            //Background
            backgroundConfirm = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 2.2f, menuGo.transform, "");
            backgroundConfirm.ScaleBackground(new Vector2(0.5f, 1.1f));

            //Dont ask again checkbox
            UICheckBox dontAsk = UIFactory.Instance.CreateCheckBox(-1f, 1.4f, menuGo.transform, "Don't ask again");
            dontAsk.onInteraction += delegate (bool box)
            {
                logic.recklessDelete = box;
            };

            //Confirm
            UIButton confirmErase = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 2f, 0.2f, menuGo.transform);
            confirmErase.ScaleBackground(new Vector2(0.5f, 1f));
            confirmErase.onInteraction += delegate ()
            {
                logic.DeleteSelected();
                logic.CloseCurrentMenu();
            };

            //Cancel
            UIButton cancelErase = UIFactory.Instance.CreateButton(UIButton.ButtonType.Back, -2f, 0.2f, menuGo.transform);
            cancelErase.ScaleBackground(new Vector2(0.5f, 1f));
            cancelErase.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };
        }

        public override void OnOpen()
        {
            if(logic.selObj != null)
            {
                if(logic.recklessDelete)
                {
                    logic.DeleteSelected();
                    logic.CloseCurrentMenu();
                    return;
                }
                backgroundConfirm.WriteText("\nDelete " + logic.selObj.name + "?\n\nWARNING: Deleting objects may lead to unforeseen consequences.");
            }
            else
            {
                logic.CloseCurrentMenu();
            }
        }
    }
}
