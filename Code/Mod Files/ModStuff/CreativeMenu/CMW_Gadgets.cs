using UnityEngine;

namespace ModStuff.CreativeMenu
{
    public class CMW_Gadgets : CMenuWindow
    {
        public override void BuildMenu()
        {
            const float Y_POS = -1.5f;
            const float FIRST_X_POS = -5.5f;
            const float X_SEPARATION = 2.3f;
            const float X_SIZE = 0.5f;
            const float Y_SIZE = 1.2f;

            Transform menuTransf = menuGo.transform;

            float GetXPos(int pos)
            {
                return UIScreenLibrary.LastCol + FIRST_X_POS + pos * X_SEPARATION;
            }

            //Texture swap
            UIButton textSwap = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, GetXPos(0), Y_POS, menuTransf, "Texture\nswap");
            textSwap.ScaleBackground(new Vector2(X_SIZE, Y_SIZE));
            textSwap.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.TextureSwap);
            };

            //Lights
            UIButton lights = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, GetXPos(1), Y_POS, menuTransf, "Lights");
            lights.ScaleBackground(new Vector2(X_SIZE, Y_SIZE));
            lights.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.Lights);
            };

            //Mechanisms
            UIButton mechanisms = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, GetXPos(2), Y_POS, menuTransf, "Mechanisms");
            mechanisms.ScaleBackground(new Vector2(X_SIZE, Y_SIZE));
            mechanisms.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.Mechanisms);
            };

            //Speech bubble
            UIButton speechBubble = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, GetXPos(3), Y_POS, menuTransf, "Speech\nbubbles");
            speechBubble.ScaleBackground(new Vector2(X_SIZE, Y_SIZE));
            speechBubble.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.Bubbles);
            };
        }
    }
}
