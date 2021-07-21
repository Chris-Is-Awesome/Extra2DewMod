using UnityEngine;

namespace ModStuff.CreativeMenu
{
    public class CMW_SpeechBubbleEdit : CMenuWindow
    {
        UITextInput _textInput;
        public override void BuildMenu()
        {
            upperLeftLimit = new Vector2(0.29f, 0.76f);
            lowerRightLimit = new Vector2(0.71f, 0.31f);

            //Background
            UIBigFrame background = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1.5f, menuGo.transform);
            background.ScaleText(0.9f);
            background.ScaleBackground(new Vector2(0.60f, 1.5f));
            background.transform.localPosition += new Vector3(0f, 0f, 0.2f);

            //Help button
            //UIButton help = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 4.0f, 4.5f, menuGo.transform, "?");
            //help.ScaleBackground(new Vector2(0.15f, 1f));
            UITextFrame helpText = UIFactory.Instance.CreateTextFrame(0f, 1.75f, menuGo.transform, "Write '<>' to add\nline jumps");
            helpText.ScaleBackground(new Vector2(1f, 1.2f));

            //Text preview
            _textInput = UIFactory.Instance.CreateTextInput(0f, 3f, menuGo.transform);
            _textInput.ScaleBackground(new Vector2(1.75f, 1f));
            _textInput.onEnterPressed += delegate ()
            {
                FinishEdit();
            };
            
            //Confirm
            UIButton confirmErase = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 2f, 0.4f, menuGo.transform);
            confirmErase.ScaleBackground(new Vector2(0.5f, 1f));
            confirmErase.onInteraction += delegate ()
            {
                FinishEdit();
            };

            //Cancel
            UIButton cancelErase = UIFactory.Instance.CreateButton(UIButton.ButtonType.Back, -2f, 0.4f, menuGo.transform);
            cancelErase.ScaleBackground(new Vector2(0.5f, 1f));
            cancelErase.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };
        }

        void FinishEdit()
        {
            CMW_SpeechBubble.currentText = _textInput.StringValue;
            logic.CloseCurrentMenu();
        }

        public override void OnClose()
        {
            logic.OpenAndToggleMenu(CMenuLogic.MenuList.Bubbles);
        }

        public override void OnOpen()
        {
            _textInput.StringValue = CMW_SpeechBubble.currentText;
        }
    }
}
