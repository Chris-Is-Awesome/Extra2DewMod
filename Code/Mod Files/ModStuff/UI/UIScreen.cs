using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ModStuff
{
    public class UIScreen : UIElement
    {
        //The title frame of the screen
        UITextFrame title;
        public UITextFrame Title { get { return title; } }
        public override string UIName
        {
            get => title.UIName; set => title.UIName = value;
        }
        public override void ShowName(bool enable)
        {
            if(title == null) { return; }
            title.gameObject.SetActive(enable);
        }

        //The back button. This is not included in uiElements
        UIButton backButton;
        public UIButton BackButton { get { return backButton; } }

        //UIElement inside the class dictionary
        Dictionary<string, UIElement> uiElements;

        //The internal gui component
        public GuiWindow GUIWindow { get { return gameObject.GetComponent<GuiWindow>(); } }

        //Saved References, saved to avoid multiple searches
        //Uses Debug window for pause menu, Filestart window for main menu
        static GameObject mainMenuWindowOriginal;
        static GameObject pauseMenuWindowOriginal;
        static Transform mainMenuParent;
        static Transform pauseMenuParent;

        //These automatically return the correct menu and transform for instantiating the menus
        static GameObject BaseScreen
        {
            get
            {
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    if (mainMenuWindowOriginal == null)
                    {
                        mainMenuWindowOriginal = GameObject.Find("GuiLayout").transform.Find("Main").Find("FileStart").gameObject;
                    }
                    return mainMenuWindowOriginal;
                }
                if (pauseMenuWindowOriginal == null)
                {
                    pauseMenuWindowOriginal = SearchGO("Debug", "Pause");
                }
                return pauseMenuWindowOriginal;
            }
        }
        static Transform BaseParent
        {
            get
            {
                if (SceneManager.GetActiveScene().name == "MainMenu")
                {
                    if (mainMenuParent == null)
                    {
                        mainMenuParent = GameObject.Find("GuiLayout").transform.Find("Main");
                    }
                    return mainMenuParent;
                }
                if (pauseMenuParent == null)
                {
                    pauseMenuParent = SearchGO("Pause", "PauseOverlay").transform;
                }
                return pauseMenuParent;
            }
        }

        //Create base screen
        public static UIScreen CreateBaseScreen(string title = "Mod Screen")
        {
            //Instantiate, name and position the menu
            GameObject elementCreated = GameObject.Instantiate(BaseScreen, BaseParent);
            elementCreated.transform.localPosition = SceneManager.GetActiveScene().name != "MainMenu" ? Vector3.zero : new Vector3(0f, 0.5f, 0f);
            elementCreated.transform.localScale = Vector3.one;
            elementCreated.name = "ModUIScreen";

            //Destroy every child but the animation GOs
            foreach (Transform child in elementCreated.transform)
            {
                switch (child.name)
                {
                    case "InAnim":
                    case "OutAnime":
                        break;
                    default:
                        GameObject.Destroy(child.gameObject);
                        break;
                }
            }

            //Assign component
            UIScreen uIScreen = elementCreated.AddComponent<UIScreen>();

            //Move screen more to the front
            elementCreated.transform.localPosition += new Vector3(0f, 0f, -2f);

            //Create back button
            uIScreen.backButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Back, 0f, -3.5f, elementCreated.transform);
            uIScreen.backButton.name = "Back";

            //Create title and hide it
            uIScreen.title = UIFactory.Instance.CreateTextFrame(0f, 6.5f, elementCreated.transform, title);
            uIScreen.title.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            //Initialize uielements dictionary
            uIScreen.uiElements = new Dictionary<string, UIElement>();

            return uIScreen;
        }

        //Get UIScreen from a GuiWindow
        static public UIScreen GetUIScreenComponent(GuiWindow root)
        {
            return root.gameObject.GetComponent<UIScreen>();
        }

        //Get screen element
        public T GetElement<T>(string name) where T : UIElement
        {
            uiElements.TryGetValue(name, out UIElement element);
            return element as T;
        }

        //Save element to screen
        public bool SaveElement(string name, UIElement element)
        {
            if (!uiElements.TryGetValue(name, out UIElement foundElement))
            {
                uiElements.Add(name, element);
                return true;
            }
            return false;
        }

        //Build UI windows
        public static void BuildAllScreens()
        {
            UIScreen tempWindow;

            //Get the UIScreens library and build each of them
            foreach (KeyValuePair<string, UIScreenLibrary.UIScreenHandler> entry in UIScreenLibrary.GetLibrary())
            {
                tempWindow = entry.Value(); //Run build function
                tempWindow.GUIWindow._trackId = entry.Key; //Set tracker ID
            }
        }

        //Brutish search function
        static GameObject SearchGO(string target, string targetParent)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go.name == target && go.transform.parent != null && go.transform.parent.name == targetParent)
                {
                    return go;
                }
            }
            return null;
        }
    }
}
