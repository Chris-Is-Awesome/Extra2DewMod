using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using ModStuff.CreativeMenu;

namespace ModStuff.CreativeMenu
{
    public static class CMenuUI
    {
        public static UIScreen CMenuBuilder()
        {
            UIScreen output = UIScreen.CreateBaseScreen("Creative Menu");
            output.BackButton.gameObject.SetActive(false);

            //All UI should be parented to this, for easy mass hiding
            GameObject uiHolder = new GameObject();
            uiHolder.transform.SetParent(output.transform, false);
            uiHolder.transform.localPosition = Vector3.zero;
            uiHolder.transform.localScale = Vector3.one;

            //Move title and parent it
            output.Title.transform.SetParent(uiHolder.transform, false);
            output.Title.transform.localPosition += new Vector3(UIScreenLibrary.FirstCol, -0.5f, 0f);

            //Instantiate CMenuLogic
            GameObject go = new GameObject("Logic", typeof(CMenuLogic));
            go.transform.SetParent(output.transform);
            CMenuLogic logic = go.GetComponent<CMenuLogic>();

            //Show markers button
            UIButton showMarkers = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 16f, 0f, output.transform, "Iminvisible!");
            showMarkers.gameObject.SetActive(false);
            showMarkers.onInteraction += delegate () { logic.UpdateMarkers(true); logic.DeselectGUIElement(true); logic.DeselectWithDelay(); };
            output.SaveElement("updatemarkers", showMarkers);

            //Hide everything button
            UIButton hideEverything = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 16f, 0f, output.transform, "Iminvisible!");
            hideEverything.gameObject.SetActive(false);
            hideEverything.onInteraction += delegate () { logic.ForceClose(); };
            output.SaveElement("forceclose", hideEverything);

            //Selection mode
            UIListExplorer selecMode = UIFactory.Instance.CreateListExplorer(0f, 5.7f, uiHolder.transform, "");
            selecMode.ScaleTitleBackground(1.3f);
            selecMode.UIName = "Selection mode";
            selecMode.ExplorerArray = new string[] { "Entities", "Entities graphics", "3D Graphics", "Animated", "Game Objects" };
            selecMode.IndexValue = 1;
            selecMode.onInteraction += delegate (bool arrow, string stringValue, int intValue)
            {
                int valueChange = arrow ? 1 : -1;
                bool tryChange = logic.ChangeSelectionMode(valueChange);
                if (!tryChange) selecMode.IndexValue -= valueChange;
            };

            //Selected frame
            UITextFrame objNameFrameTitle = UIFactory.Instance.CreateTextFrame(UIScreenLibrary.FirstCol, -2.7f, output.transform, "Selected");
            objNameFrameTitle.ScaleBackground(new Vector2(0.6f, 0.75f));
            objNameFrameTitle.transform.localScale = Vector3.one * 0.6666f;
            objNameFrameTitle.transform.localPosition += new Vector3(0f, 0.5f, -0.2f);
            UITextFrame objNameFrame = UIFactory.Instance.CreateTextFrame(UIScreenLibrary.FirstCol, -2.7f, output.transform, "----");

            //Held frame
            UITextFrame objSaveFrameTitle = UIFactory.Instance.CreateTextFrame(UIScreenLibrary.FirstCol, -4.1f, output.transform, "Held");
            objSaveFrameTitle.ScaleBackground(new Vector2(0.6f, 0.75f));
            objSaveFrameTitle.transform.localScale = Vector3.one * 0.6666f;
            objSaveFrameTitle.transform.localPosition += new Vector3(0f, 0.5f, -0.2f);
            UITextFrame objSaveFrame = UIFactory.Instance.CreateTextFrame(UIScreenLibrary.FirstCol, -4.1f, output.transform, "----");

            //Windows
            //-----------------------------------------------
            CMW_ToolSelect toolSelect = CMenuWindow.CreateMenu<CMW_ToolSelect>(CMenuLogic.MenuList.ToolSelect, logic, output.transform);
            CMW_Help help = CMenuWindow.CreateMenu<CMW_Help>(CMenuLogic.MenuList.Help, logic, output.transform);
            CMW_Library library = CMenuWindow.CreateMenu<CMW_Library>(CMenuLogic.MenuList.Library, logic, output.transform);
            CMW_LibraryRename libraryRename = CMenuWindow.CreateMenu<CMW_LibraryRename>(CMenuLogic.MenuList.LibraryRename, logic, output.transform);
            CMW_LibraryDelete libraryDelete = CMenuWindow.CreateMenu<CMW_LibraryDelete>(CMenuLogic.MenuList.LibraryDelete, logic, output.transform);
            CMW_Filter filter = CMenuWindow.CreateMenu<CMW_Filter>(CMenuLogic.MenuList.Filter, logic, output.transform);
            CMW_Delete deleteMenu = CMenuWindow.CreateMenu<CMW_Delete>(CMenuLogic.MenuList.Delete, logic, output.transform);
            CMW_TextureSwap texSwapMenu = CMenuWindow.CreateMenu<CMW_TextureSwap>(CMenuLogic.MenuList.TextureSwap, logic, output.transform);
            CMW_Gadgets gadgetsMenu = CMenuWindow.CreateMenu<CMW_Gadgets>(CMenuLogic.MenuList.Gadgets, logic, output.transform);
            CMW_Lights lightsMenu = CMenuWindow.CreateMenu<CMW_Lights>(CMenuLogic.MenuList.Lights, logic, output.transform);
            CMW_Mechanism mechaMenu = CMenuWindow.CreateMenu<CMW_Mechanism>(CMenuLogic.MenuList.Mechanisms, logic, output.transform);
            CMW_SpeechBubble speechBubbleMenu = CMenuWindow.CreateMenu<CMW_SpeechBubble>(CMenuLogic.MenuList.Bubbles, logic, output.transform);
            CMW_SpeechBubbleEdit speechBubbleEditMenu = CMenuWindow.CreateMenu<CMW_SpeechBubbleEdit>(CMenuLogic.MenuList.BubblesEdit, logic, output.transform);
            CMW_Options optionsMenu = CMenuWindow.CreateMenu<CMW_Options>(CMenuLogic.MenuList.Options, logic, output.transform);

            //Tools
            //-----------------------------------------------
            float xSeparation = 1.8f;

            //Filter
            UIButton searchFilter = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.LastCol, 5.7f, uiHolder.transform, "Object filter: NONE");
            filter.onUpdateFilterButton += delegate (string name)
            {
                searchFilter.UIName = name;
            };
            searchFilter.onInteraction += delegate ()
            {
                filter.ResetFilterName();
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.Filter);
            };

            //Tool Select
            const string SELECT_TOOL_TEXT_0 = "Tool:\n";//"Switch\nTool\n\nCurrent:\n";
            const string SELECT_TOOL_TEXT_1 = "\n\nClick to\nswitch";//"Switch\nTool\n\nCurrent:\n";
            UIButton selectButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.FirstCol + 3.3f, -3.4f, uiHolder.transform);
            selectButton.ScaleBackground(new Vector2(0.5f, 2.35f));
            selectButton.LineSize = 100f;
            selectButton.UIName = SELECT_TOOL_TEXT_0 + "Select" + SELECT_TOOL_TEXT_1;
            selectButton.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.ToolSelect);
            };
            logic.onToolChange += delegate (string tool)
            {
                selectButton.UIName = SELECT_TOOL_TEXT_0 + tool + SELECT_TOOL_TEXT_1;
            };

            //Hold
            UIButton holdButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.LastCol - 4f, -2.7f, uiHolder.transform, "Hold");
            holdButton.ScaleBackground(new Vector2(0.4f, 1f));
            holdButton.onInteraction += delegate ()
            {
                logic.TriggerTool(CMenuLogic.ToolList.Hold);
            };

            //Delete
            UIButton deleteButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.LastCol - 4f + xSeparation, -4.1f, uiHolder.transform, "Delete");
            deleteButton.ScaleBackground(new Vector2(0.4f, 1f));
            deleteButton.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.Delete);
            };
            deleteButton.gameObject.SetActive(false);

            //Attach
            UIButton attachButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.LastCol - 4f + xSeparation, -2.7f, uiHolder.transform, "Attach");
            attachButton.ScaleBackground(new Vector2(0.4f, 1f));
            attachButton.onInteraction += delegate ()
            {
                logic.TriggerTool(CMenuLogic.ToolList.Attach);
            };

            //Disable animations
            UIButton disableAnimsButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.LastCol - 4f, -4.1f, uiHolder.transform, "Pose");
            disableAnimsButton.ScaleBackground(new Vector2(0.4f, 1f));
            disableAnimsButton.onInteraction += delegate ()
            {
                logic.TriggerTool(CMenuLogic.ToolList.DisableAnimations);
            };
            logic.onChangedMode += delegate (ObjFinder.SelecMode mode)
            {
                disableAnimsButton.gameObject.SetActive(mode == ObjFinder.SelecMode.Armatures);
            };
            disableAnimsButton.gameObject.SetActive(false);

            //Gadgets
            UIButton gadgetsButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.LastCol + 0.2f, -2.7f, uiHolder.transform, "Gadgets");
            gadgetsButton.ScaleBackground(new Vector2(0.4f, 1f));
            gadgetsButton.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.Gadgets);
            };

            //Library
            UIButton libraryButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.LastCol + 0.2f + xSeparation, -2.7f, uiHolder.transform, "Library");
            libraryButton.ScaleBackground(new Vector2(0.4f, 1f));
            libraryButton.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.Library);
            };

            //Options
            UIButton optionsButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.LastCol + 0.2f, -4.1f, uiHolder.transform, "Options");
            optionsButton.ScaleBackground(new Vector2(0.4f, 1f));
            optionsButton.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.Options);
            };

            //Help
            UIButton helpButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, UIScreenLibrary.LastCol + 0.2f + xSeparation, -4.1f, uiHolder.transform, "HELP");
            helpButton.ScaleBackground(new Vector2(0.4f, 1f));
            helpButton.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.Help);
            };

            /*
            logic.onDebug += delegate (string text)
            {
                libraryButton.UIName = text;
            };*/

            logic.onChangedSelection += delegate (string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = "----";
                }
                objNameFrame.UIName = name;
            };

            logic.onChangedSaved += delegate (string name)
            {
                if (string.IsNullOrEmpty(name)) name = "----";
                objSaveFrame.UIName = name;
            };

            logic.onExit += delegate ()
            {
                output.BackButton.Trigger();
            };

            logic.onChangedMode += delegate (ObjFinder.SelecMode mode)
            {
                selecMode.IndexValue = (int)mode;
            };

            logic.onFullScreen += delegate (bool active)
            {
                uiHolder.SetActive(!active);
            };

            logic.onChangedSelection += delegate (string name)
            {
                bool active = !string.IsNullOrEmpty(name);
                deleteButton.gameObject.SetActive(active);
            };

            return output;
        }
    }
}
