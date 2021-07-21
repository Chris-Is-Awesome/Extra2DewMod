using UnityEngine;

namespace ModStuff.CreativeMenu
{
    public class CMW_ToolSelect : CMenuWindow
    {
        public override void BuildMenu()
        {
            Transform menuTransf = menuGo.transform;
            float xSeparation = 1.8f;

            //Select
            UIButton selectButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.FirstCol + 3.3f, -1.1f, menuTransf, "Select");
            selectButton.ScaleBackground(new Vector2(0.4f, 1f));
            selectButton.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
                logic.SetDragTool(CMenuLogic.ToolList.Select);
            };

            //Move
            UIButton moveButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.FirstCol + 3.3f + xSeparation * 1.2f, -0.5f, menuTransf, "Move");
            moveButton.ScaleBackground(new Vector2(0.4f, 1f));
            moveButton.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
                logic.SetDragTool(CMenuLogic.ToolList.Move);
            };

            //Rotate
            UIButton rotateButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.FirstCol + 3.3f + xSeparation * 2.2f, -0.5f, menuTransf, "Rotate");
            rotateButton.ScaleBackground(new Vector2(0.4f, 1f));
            rotateButton.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
                logic.SetDragTool(CMenuLogic.ToolList.Rotate);
            };

            //Scale
            UIButton scaleButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.FirstCol + 3.3f + xSeparation * 1.2f, -1.7f, menuTransf, "Scale");
            scaleButton.ScaleBackground(new Vector2(0.4f, 1f));
            scaleButton.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
                logic.SetDragTool(CMenuLogic.ToolList.Scale);
            };

            //Clone
            UIButton cloneButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.FirstCol + 3.3f + xSeparation * 2.2f, -1.7f, menuTransf, "Clone");
            cloneButton.ScaleBackground(new Vector2(0.4f, 1f));
            cloneButton.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
                logic.SetDragTool(CMenuLogic.ToolList.Clone);
            };
        }
    }
}
