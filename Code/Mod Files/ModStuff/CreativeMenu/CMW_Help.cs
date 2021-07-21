using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class CMW_Help : CMenuWindow
    {
        static string[] titleText = new string[]
        {
                "Welcome!",
                "Hotkeys",
                "Controls",
                "Controls (cont.)",
                "Selection modes",
                "Camera",
                "Object filter",
                "Tool: Select and Hold",
                "Tool: Move",
                "Tool: Rotate",
                "Tool: Scale",
                "Tool: Clone",
                "Option: Attach",
                "Option: Pose",
                "Option: Delete",
                "The Library",
                "Gadgets",
                "Gadgets: Texture Swap",
                "Gadgets: Mechanisms",
                "Gadgets: Lights",
                "Gadgets: Speech bubbles"
                

        };
        static string[] helpText = new string[]
        {
                "This menu allows you to manipulate objects in the game. Move them around, attach them to other objects, change their properties or spawn useful gadgets!\n\nTo open this menu more quickly, use 'F2' while the game is unpaused",

                "Main controls:\n\nLeft click: Activate tool - Right click (hold): Camera mode - Escape: Cancel/Exit - Object filter: Spacebar\n\n" +
                "Tools:\n\nSelect: F - Move: G - Rotate: R - Scale: S - Clone: C - Hold: Q - Select Held: E - Always show markers: X - Attach: T - Enable/Disable posing: K" +
                " - Open library: V - Store object: O - Retrieve object: P - Toggle full screen: B - Open last gadget: J - Help: H",

                "Left click on 'Tool' to select which tool you want to use, then click and drag on an object to manipulate it. Hold the right click and use WASD to move the camera around. Press 'Esc' to exit the menu.",

                "If you have an object selected, you can use hotkeys instead. This is much quicker than using the buttons and much more precise! You can find a list" +
                " of them in the last pages\n\nIf the UI gets in the way, try out full screen (hotkey: B). It will hide most of the UI, you can exit full screen by pressing 'B' again or 'Esc'.",

                "Not all objects are shown at the same time. Use the selection mode (or mouse wheel) to switch between them:\n\n" +
                "Entities: NPCs and the player.\n" +
                "Entities Graphics: Entities graphics. This is an object which has both the 3d models and the animation controllers.\n" +
                "3D graphics: All objects which have a 3D model. You can swap their textures using the 'Texture swap' menu.\n" +
                "Animated: All objects that are used for animation and posing. Enable posing first before trying to manipulate them.\n" +
                "Game Objects: All game objects on the scene. Game objects are unity containers for logic, meshes, particles, almost everything. Handle with care.\n\n",

                "Enter camera mode holding right click. There are two camera modes, free camera and orbit camera:\n\n" +
                "Free camera: Move around freely. WASD moves you around, mouse wheel adjusts the speed.\n" +
                "Orbit camera: Focus on an object and orbit it, toggle it by pressing 'X'. Use the mouse wheel to adjust the distance to the object.",

                "Object filter (Hotkey: Space bar).\n\nIs an object you want to select buried underneath other objects? Filter the selectable objects to click only the ones you want. The filter can be made case sensitive or force it to use the exact name of the object." +
                " To remove the filter, leave the space blank. The filter will be reset every time you enter the filter selection menu." +
                "\n\nIf you want to keep all the markers visible, press 'Z' outside the filter menu.",

                "Select objects (Hotkey: F). This will only select an object, without manipulating it. Very useful for hotkey controls.\n\n" +
                "Hold an object (Hotkey: Q). Held objects are used by the 'Attach' option, attaching the selected object to the held object. You " +
                "can also select the held object (Hotkey: E).",

                "Move an object around (Hotkey: G).\n\nPress X/Y/Z to change the movement axis. Press G to move parallel to the camera. Press one of those keys again to go back to the regular mode.",

                "Rotate objects from the camera perspective (Hotkey: R).\n\nPress X/Y/Z to change the rotation axis. Press R to enable trackball rotation. Press one of those keys again to go back to the regular mode.",

                "Scale objects, make them bigger or smaller (Hotkey: S).\n\nPress X/Y/Z to change the scale axis, press one of those again to go back to regular mode. Press S to toggle single-object scaling (useful for posing)",

                "Clone objects, make copies of the selected object (Hotkey: C).\n\nAfter cloning, the menu uses 'Move' controls.\n\nNOTE: Some objects do nothing when cloned or have unpredictable behavior, specially armatures and entities.",

                "Attach the selected object to the held object (Hotkey: T).\n\nThis will make the selected object move, scale and rotate with the held object.",

                "Enable/disable posing (Hotkey: K).\n\nAnimated objects cannot be manipulated unless posing is activated. Posing will disable all animations on that object. Activating this option again will turn the animations back on.",

                "Delete the selected object (Hotkey: Delete).\n\nIf you so require, objects can be deleted. Keep in mind deleting certain objects might cause weird behavior. Changing the map should solve most issues, but if things go horribly wrong, restarting the game will always turn everything back to normal.",

                "Open the library (Hotkey: V).\n\nThe library saves object to be later used anywhere else, useful for gathering objects from different parts of the game or saving your creations. Objects will be saved with anything " +
                "attached to them, but game logic (like NPCs AI) might not work correctly.\n\nStore object hotkey: O - Retrieve object hotkey: P.\n\nThe library will be cleared every time you close Ittle Dew 2.",

                "Gadgets are tools to modify or spawn new objects into the game. There are many gadgets windows, press 'J' to open the last one used.",

                "Texture swap allows you to save or change textures of 3D objects in the game, best used with the '3D model' selection mode. Select a 3D object and open the menu to see all the textures it has. Pressing the 'Save to disk' button will save them in your hard drive in" +
                " the 'extra2dew/materials/texture/' folder. You can also exchange textures with ones inside that folder. Only png files can be used for this.\n\nNote: Not all 3D objects have textures and changing textures might not change the appearance of an object at all.",

                "Mechanisms are objects that move and rotate on their own. Pressing keys can control how they move depending on their type. Regular mechanisms have two keys and will move only when one of them is pressed. Toggle and permanent " +
                "mechanisms will always move and the key will turn them on/off. Mouse controlled mechanisms will move with the mouse when you hold a specific key.",

                "Lights illuminate the game. With this menu you can change the global light or create new lights. Keep in mind that adding too many lights might cause visual artifacts. Created light boxes can be made invisible from the options menu.",

                "Speech bubbles lets you make people or things talk. Write '<>' to insert line jumps in your text. The bubbles accept unity HTML format text to change their color and size. For more information, search 'unity rich text' online."
        };

        public override void BuildMenu()
        {
            upperLeftLimit = new Vector2(0.25f, 0.77f);
            lowerRightLimit = new Vector2(0.75f, -0.30f);

            UIBigFrame mainText = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1.5f, menuGo.transform);
            mainText.ScaleText(0.9f);
            mainText.ScaleBackground(new Vector2(0.7f, 1.5f));

            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 4.5f, menuGo.transform);
            UIButton escape = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 4.5f, 4.5f, menuGo.transform, "Close");
            escape.ScaleBackground(new Vector2(0.5f, 1f));
            escape.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };

            UIButton goBack = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -2f, -1.4f, menuGo.transform, "<--");
            goBack.ScaleBackground(new Vector2(0.4f, 1f));
            goBack.gameObject.SetActive(false);
            UIButton goForward = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 2f, -1.4f, menuGo.transform, "-->");
            goForward.ScaleBackground(new Vector2(0.4f, 1f));

            int index = 0;

            goBack.onInteraction += delegate ()
            {
                index--;
                mainText.WriteText(helpText[index]);
                title.UIName = titleText[index];
                goForward.gameObject.SetActive(true);
                if (index == 0) goBack.gameObject.SetActive(false);
            };

            goForward.onInteraction += delegate ()
            {
                index++;
                mainText.WriteText(helpText[index]);
                title.UIName = titleText[index];
                goBack.gameObject.SetActive(true);
                if (index == (helpText.Length - 1)) goForward.gameObject.SetActive(false);
            };

            mainText.WriteText(helpText[0]);
            title.UIName = titleText[0];
        }
    }
}
