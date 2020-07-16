using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

//Modified ludo scripts:
//GUINode.cs: Made _trackId public. I'm sure there should be a way to implement UI without 
//this change, but it would require a lot of time to make it work, this is much simpler
//PauseMenu.cs: Calls the UIwindows build function, handles part of the UIwindows logic
//MainMenu.cs: Calls the UIwindows build function, handles part of the UIwindows logic

namespace ModStuff
{
    public class UIFactory : Singleton<UIFactory>
    {
        //======================
        //Buttons
        //======================
        GameObject defaultButtonOriginal;
        GameObject confirmButtonOriginal;
        GameObject backButtonOriginal;

        //Base Button
        GameObject CreateButtonPrimitive(UIButton.ButtonType type, float posX, float posY, Transform parent)
        {
            //Select button type to spawn
            GameObject objectToCopy;
            switch (type)
            {
                case UIButton.ButtonType.Default:
                    if (defaultButtonOriginal == null)
                    {
                        defaultButtonOriginal = SearchGO("Default", "KeyConfig");
                        if (defaultButtonOriginal == null) { return null; }
                    }
                    objectToCopy = defaultButtonOriginal;
                    break;
                case UIButton.ButtonType.Confirm:
                    if (confirmButtonOriginal == null)
                    {
                        confirmButtonOriginal = SearchGO("Confirm", "KeyConfig");
                        if (confirmButtonOriginal == null) { return null; }
                    }
                    objectToCopy = confirmButtonOriginal;
                    break;
                case UIButton.ButtonType.Back:
                    if (backButtonOriginal == null)
                    {
                        backButtonOriginal = SearchGO("Cancel", "Gamesettings");
                        if (backButtonOriginal == null) { return null; }
                    }
                    objectToCopy = backButtonOriginal;
                    break;
                default:
                    return null;
            }

            //Create parent GO and set it up
            GameObject uiParent = new GameObject("ModButton");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localScale = Vector3.one;
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);

            //Instantiate GO
            GameObject elementCreated = Instantiate(objectToCopy, uiParent.transform);
            elementCreated.name = "ModUIElement";
            elementCreated.transform.localPosition = ZFixVector; //Shifts the UIelement to the front, to avoid z level clashing

            //For Default button, disable selection change on click 
            if (type == UIButton.ButtonType.Default)
            {
                Destroy(elementCreated.GetComponent<GuiClickChangeSelection>());
            }
            //For Confirm button, disable warning sign 
            if (type == UIButton.ButtonType.Confirm)
            {
                Destroy(elementCreated.transform.Find("CloudWarn").gameObject);
            }

            return uiParent;
        }

        //Regular Button
        public UIButton CreateButton(UIButton.ButtonType type, float posX, float posY, Transform parent, string name = "!LEFTBLANK!")
        {
            GameObject uiParent = CreateButtonPrimitive(type, posX, posY, parent);
            switch (type)
            {
                case UIButton.ButtonType.Default:
                    if (name == "!LEFTBLANK!") { name = "Default"; }
                    break;
                case UIButton.ButtonType.Confirm:
                    if (name == "!LEFTBLANK!") { name = "Confirm"; }
                    break;
                case UIButton.ButtonType.Back:
                    if (name == "!LEFTBLANK!") { name = "Back"; }
                    break;
                default:
                    return null;
            }

            //Assign component
            UIButton output = uiParent.AddComponent<UIButton>();
            output.Initialize();
            output.UIName = name;

            return output;
        }

        //Button with graphics
        public UIGFXButton CreateGFXButton(string pngName, float posX, float posY, Transform parent, string name = "Default")
        {
            //Create parent GO and set it up
            GameObject uiParent = new GameObject("ModGFXButton");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localScale = Vector3.one;
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);

            //Create basic button without logic component
            GameObject uiIntermediateParent = CreateButtonPrimitive(UIButton.ButtonType.Default, 0f, 0f, parent);
            uiIntermediateParent.transform.SetParent(uiParent.transform, false);
            uiIntermediateParent.transform.localScale = Vector3.one;
            uiIntermediateParent.transform.localPosition = Vector3.zero;

            //Destroy original background
            Destroy(uiIntermediateParent.transform.Find("ModUIElement").Find("Root").Find("Background").gameObject);

            //Create plane and texture it
            GameObject newPlane = MakePlane(2f, 2f);
            ChangeTexture(pngName, newPlane.GetComponent<MeshRenderer>());
            newPlane.name = "ImgTexture";
            newPlane.transform.SetParent(uiIntermediateParent.transform.Find("ModUIElement").Find("Root"), false);

            //Resize every element inside the button by the inverse of (0.5f, 1.6f, 1f), to counteract the button rescaling
            uiIntermediateParent.transform.localScale = new Vector3(0.5f, 1.6f, 1f);
            foreach (Transform child in uiIntermediateParent.transform.Find("ModUIElement").Find("Root"))
            {
                child.localScale = new Vector3(child.localScale.x / 0.5f, child.localScale.y / 1.6f, child.localScale.z);
            }
   
            //Change text parent so it doesnt pulsate like the GFX, move it a bit farther down
            Transform textTransform = uiIntermediateParent.transform.Find("ModUIElement").Find("Root").Find("Text");
            textTransform.SetParent(textTransform.parent.parent);
            textTransform.localPosition = new Vector3(0f, -0.75f, 0f);

            //Assign component
            UIGFXButton output = uiParent.AddComponent<UIGFXButton>();
            output.Initialize();
            output.UIName = name;

            return output;
        }

        //======================
        //Image
        //======================
        public UIImage CreateImage(string pngName, float posX, float posY, Transform parent, string name = "")
        {
            //Instantiate GOs
            GameObject elementCreated = Instantiate(TextFrameOriginal, parent);
            elementCreated.name = "ModUIImage";
            elementCreated.transform.localScale = Vector3.one;
            elementCreated.transform.localPosition = new Vector3(posX, posY, 0f) + ZFixVector;

            //Destroy original background
            Destroy(elementCreated.transform.Find("Background").gameObject);

            //Create plane and texture it
            GameObject newPlane = MakePlane(2f, 2f);
            ChangeTexture(pngName, newPlane.GetComponent<MeshRenderer>());
            newPlane.name = "ImgTexture";
            newPlane.transform.SetParent(elementCreated.transform, false);

            //Assign component
            UIImage output = elementCreated.AddComponent<UIImage>();
            output.Initialize();
            output.UIName = name;

            return output;
        }

        //Make plane
        GameObject MakePlane(float x, float y)
        {
            GameObject go = new GameObject("Plane");
            go.layer = 9;
            float halfWidth = x / 2;
            float halfLength = y / 2;
            Mesh m = new Mesh();
            m.Clear();
            m.vertices = new Vector3[]
            {
                new Vector3(-halfWidth, -halfLength, 0f),
                new Vector3(-halfWidth, halfLength, 0f),
                new Vector3(halfWidth, halfLength, 0f),
                new Vector3(halfWidth, -halfLength, 0f)
            };
            m.uv = new Vector2[]
            {
                new Vector2(0,0),
                new Vector2(0,1),
                new Vector2(1,1),
                new Vector2(1,0)
            };
            m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            m.RecalculateNormals();

            MeshFilter meshFilter = (MeshFilter)go.AddComponent(typeof(MeshFilter));
            meshFilter.mesh = m;

            MeshRenderer renderer = (MeshRenderer)go.AddComponent(typeof(MeshRenderer));
            renderer.material = TextFrameOriginal.transform.Find("Background").GetComponent<MeshRenderer>().material;

            return go;
        }
        //Texture plane
        public void ChangeTexture(string pngName, MeshRenderer renderer)
        {
            //Load image texture and apply as material
            string path = ModMaster.TexturesPath + pngName + ".png";
            Texture2D tex = new Texture2D(512, 512);
            byte[] fileData;
            if (File.Exists(path))
            {
                fileData = File.ReadAllBytes(path);
                tex.LoadImage(fileData, false);
            }
            renderer.material.mainTexture = tex;
        }

        //======================
        //Checkbox
        //======================
        GameObject checkBoxOriginal;
        public UICheckBox CreateCheckBox(float posX, float posY, Transform parent, string name = "Default")
        {
            //Search for checkbox to spawn
            GameObject objectToCopy;
            if (checkBoxOriginal == null)
            {
                checkBoxOriginal = SearchGO("OnScreenTime", "Gamesettings");
                if (checkBoxOriginal == null) { return null; }
            }
            objectToCopy = checkBoxOriginal;

            //Create parent GO and set it up
            GameObject uiParent = new GameObject("ModCheckBox");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localScale = Vector3.one;
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);

            //Instantiate GO
            GameObject elementCreated = Instantiate(objectToCopy, uiParent.transform);
            elementCreated.name = "ModUIElement";
            elementCreated.transform.localPosition = ZFixVector;

            //Resize the whole checkbox to a more manageable size (60%)
            foreach (Transform child in elementCreated.transform)
            {
                //All gfx children are resized to the inverse of 0.6f in the x axis ( for example, 0.2f / 0.6f = 0.33333f)
                switch (child.name)
                {
                    case "Text":
                        child.localScale = new Vector3(0.3333333f, 0.2f, 0.1f);
                        break;
                    case "CheckBox":
                        child.localScale = new Vector3(0.8333333f, 0.5f, 1f);
                        break;
                    case "CheckBoxX":
                        child.localScale = new Vector3(1f, 0.6f, 1f);
                        break;
                    default:
                        break;
                }
            }
            uiParent.transform.localScale = new Vector3(0.6f, 1f, 1f); //Parent object resized to 0.6f in the x axis (1f * 0.6f = 0.6f

            //Assign component
            UICheckBox output = uiParent.AddComponent<UICheckBox>();
            output.Initialize();
            output.UIName = name;

            return output;
        }

        //======================
        //Slider
        //======================
        GameObject sliderOriginal;
        public UISlider CreateSlider(float posX, float posY, Transform parent, string name = "")
        {
            //Search for slider to spawn
            GameObject objectToCopy;
            if (sliderOriginal == null)
            {
                sliderOriginal = SearchGO("ListLevel", "List");
                if (sliderOriginal == null) { return null; }
            }
            objectToCopy = sliderOriginal;

            //Create parent GO and set it up
            GameObject uiParent = new GameObject("ModSlider");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localScale = Vector3.one;
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);

            //Instantiate GO
            GameObject elementCreated = Instantiate(objectToCopy, uiParent.transform);
            elementCreated.name = "ModUIElement";
            elementCreated.transform.localPosition = ZFixVector;

            //Set TextFrame
            UITextFrame display = UIFactory.Instance.CreateTextFrame(0f, 0f, elementCreated.transform);
            display.transform.localPosition = new Vector3(-2.35f, 0f, -0.2f);
            display.ScaleBackground(new Vector2(0.3f, 0.8f));

            //Remove slider icon, hider icon/component and arrows. Reposition title name
            Destroy(elementCreated.GetComponent<GuiFloatHiderApplier>());
            Destroy(elementCreated.transform.Find("Sound").gameObject);
            elementCreated.transform.Find("Arrows").localScale = Vector3.zero;
            elementCreated.transform.Find("Name").localPosition = new Vector3(0f, 0.32f, -0.2f);
            Destroy(elementCreated.transform.Find("OffIcon").gameObject);

            //Assign component
            UISlider output = uiParent.AddComponent<UISlider>();
            output.Initialize();
            output.UIName = name;

            return output;
        }

        //======================
        //ScrollBar
        //======================
        GameObject scrollBarOriginal;
        public void SaveScrollBar()
        {
            if (scrollBarOriginal != null) { return; }
            GameObject sliderFound = SearchGO("Slider", "FileSelect");
            if (sliderFound == null) { return; }
            scrollBarOriginal = Instantiate(sliderFound);
            scrollBarOriginal.SetActive(false);
            scrollBarOriginal.name = "ScrollBarOriginal";
            DontDestroyOnLoad(scrollBarOriginal);
        }

        public UIScrollBar CreateScrollBar(float posX, float posY, Transform parent)
        {
            //Search for slider to spawn
            if (scrollBarOriginal == null)
            {
                SaveScrollBar();
                if (scrollBarOriginal == null) { return null; }
            }

            //Create parent GO and set it up
            GameObject uiParent = new GameObject("ModScrollBar");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localScale = Vector3.one;
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);

            //Instantiate GO
            GameObject elementCreated = Instantiate(scrollBarOriginal, uiParent.transform);
            elementCreated.SetActive(true); //The saved GO is initially inactive
            elementCreated.name = "ModUIElement";
            elementCreated.transform.localPosition = ZFixVector;

            //Save all rope pieces in an array for changing the size later
            GameObject[] ropePieces = new GameObject[8];
            foreach (Transform child in elementCreated.transform)
            {
                switch (child.localPosition.y)
                {
                    case 1.08f:
                        ropePieces[0] = child.gameObject;
                        break;
                    case 0.07f:
                        ropePieces[1] = child.gameObject;
                        break;
                    case -0.97f:
                        ropePieces[2] = child.gameObject;
                        break;
                    case -2.01f:
                        ropePieces[3] = child.gameObject;
                        break;
                    case -3.04f:
                        ropePieces[4] = child.gameObject;
                        break;
                    case -4.1f:
                        ropePieces[5] = child.gameObject;
                        break;
                    case -5.13f:
                        ropePieces[6] = child.gameObject;
                        break;
                    case -6.21f:
                        ropePieces[7] = child.gameObject;
                        break;
                    default:
                        break;
                }
            }

            //Fix Y axis offset
            elementCreated.GetComponentInChildren<GuiClickRect>().SetSizeAndCenter(new Vector2(1f, 7.3f), new Vector2(0f, -2.6f));
            Vector3 fixVector = new Vector3(0f, 1.1f, 0f);
            foreach (Transform child in elementCreated.transform)
            {
                child.localPosition += fixVector;
            }

            //Assign component
            UIScrollBar output = uiParent.AddComponent<UIScrollBar>();
            output.Initialize();
            output.SaveRopePieces(ropePieces);

            return output;
        }

        //======================
        //TextFrame
        //======================
        GameObject _textFrameOriginal;
        GameObject TextFrameOriginal
        {
            get
            {
                if(_textFrameOriginal == null)
                {
                    _textFrameOriginal = SearchGO("Root", "Default");
                }
                return _textFrameOriginal;
            }
        }
        public UITextFrame CreateTextFrame(float posX, float posY, Transform parent, string name = "")
        {
            //Create parent GO and set it up
            GameObject uiParent = new GameObject("ModTextFrame");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localScale = Vector3.one;
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);

            //Instantiate GOs
            GameObject elementCreated = Instantiate(TextFrameOriginal, uiParent.transform);
            elementCreated.name = "ModUIElement";
            elementCreated.transform.localPosition = ZFixVector;

            //Assign component
            UITextFrame output = uiParent.AddComponent<UITextFrame>();
            output.Initialize();
            output.UIName = name;

            return output;
        }

        //======================
        //BigFrame
        //======================
        GameObject bigFrameOriginal;
        GameObject bigScrollOriginal;
        public UIBigFrame CreateBigFrame(UIBigFrame.FrameType type, float posX, float posY, Transform parent, string content = "")
        {
            //Search for text frame to spawn
            GameObject objectToCopy;
            if(type == UIBigFrame.FrameType.Default)
            { 
                if (bigFrameOriginal == null)
                {
                    if(SceneManager.GetActiveScene().name == "MainMenu")
                    {
                        bigFrameOriginal = SearchGO("Background", "Records");
                    }
                    else
                    {
                        bigFrameOriginal = GameObject.Find("PauseOverlay").transform.Find("Pause").Find("Main").Find("Layout").Find("Background").gameObject; //tempGO.transform.Find("Background").gameObject;
                    }
                }
                objectToCopy = bigFrameOriginal;
            }
            else
            {
                if (bigScrollOriginal == null)
                {
                    bigScrollOriginal = SearchGO("Background", "KeyConfig");
                }
                objectToCopy = bigScrollOriginal;
            }

            //Create parent GO and set it up
            GameObject uiParent = new GameObject("ModBigFrame");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localScale = Vector3.one;
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);

            //Instantiate GOs
            GameObject elementCreated = Instantiate(objectToCopy, uiParent.transform);
            elementCreated.name = "Background";
            if (type == UIBigFrame.FrameType.Default)
            {
                //elementCreated.transform.localScale = new Vector3(0.9f, 4f, 1f);
                elementCreated.transform.localPosition = Vector3.zero;
            }
            else
            {
                elementCreated.transform.localPosition = new Vector3(-2.4f, -2.2f, 0f);
            }

            //Create content textmesh
            GameObject contentTextMesh = Instantiate(SearchTitleText(), uiParent.transform);
            contentTextMesh.transform.localScale = Vector3.one * 0.15f;
            contentTextMesh.transform.localPosition = ZFixVector + ((SceneManager.GetActiveScene().name == "MainMenu") ? new Vector3(0f, 1.25f, -0.2f) : new Vector3(0f, 1.5f, -0.2f));
            contentTextMesh.name = "Content";

            //Assign component
            UIBigFrame output = uiParent.AddComponent<UIBigFrame>();
            output.Initialize();
            output.UIName = content;

            return output;
        }

        //======================
        //ScrollMenu
        //======================
        public UIScrollMenu CreateScrollMenu(float posX, float posY, Transform parent, string name = "ScrollMenu")
        {
            //Create parent GO and set it up
            GameObject uiParent = new GameObject("ModScrollMenu");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localScale = Vector3.one;
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);

            //Create Canvas
            GameObject canvas = new GameObject("Canvas");
            canvas.transform.SetParent(uiParent.transform, false);
            canvas.transform.localScale = Vector3.one;
            canvas.transform.localPosition = new Vector3(0f, UIScrollMenu.initialY, 0f);

            //Create ScrollBar
            UIScrollBar scrollBar = CreateScrollBar(4f, -0.5f, uiParent.transform);

            //Create title
            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 3f, uiParent.transform, name);

            //Assign component
            UIScrollMenu output = uiParent.AddComponent<UIScrollMenu>();
            output.Initialize();
            output.UIName = name;
            output.CanvasWindow = 8.5f;
            output.EmptySpace = 2f;
            output.DynamicHeight = true;

            return output;
        }

        //======================
        //2D List
        //======================
        public UI2DList Create2DList(float posX, float posY, Vector2 arraySize, Vector2 btnScale, Vector2 btnBackground, Transform parent, string name = "2D List")
        {
            //Create parent GO and set it up
            GameObject uiParent = new GameObject("Mod2DList");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localScale = Vector3.one;
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);

            //Create ScrollBar
            UIScrollBar scrollBar = CreateScrollBar(4f, 0f, uiParent.transform);

            //Create title
            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 3.2f, uiParent.transform, name);

            //Buttons array
            //--------------
            //Initialize array variables
            UIButton[] buttonsArray = new UIButton[(int)(arraySize.x * arraySize.y)];
            float xSeparation = 3.4f * btnScale.x * btnBackground.x;
            float ySeparation = -1.6f * btnScale.y * btnBackground.y;
            float initialX = xSeparation * (-0.5f) * (arraySize.x - 1);
            float initialY = 1.6f;
            Vector2 backgroundScale = new Vector2(0.75f * btnBackground.x, 1.5f * btnBackground.y);
            //Instantiate array
            UIButton createdButton;
            for (int i = 0; i < (int)(arraySize.x * arraySize.y); i++)
            {
                float buttonXPos = initialX + ((float)(i % (int)arraySize.x)) * xSeparation;
                float buttonYPos = initialY + ((float)(i / ((int)arraySize.x))) * ySeparation;
                createdButton = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, buttonXPos, buttonYPos, uiParent.transform, i.ToString() + "\nTest");
                createdButton.ScaleBackground(backgroundScale, btnScale);
                buttonsArray[i] = createdButton;
            }

            //Assign component
            UI2DList output = uiParent.AddComponent<UI2DList>();
            output.Initialize();
            output.AssignButtonsArray(buttonsArray, (int)arraySize.x);
            output.UIName = name;

            return output;
        }

        //======================
        //ListExplorer
        //======================
        public UIListExplorer CreateListExplorer(float posX, float posY, Transform parent, string name = "")
        {
            //Create parent GO and set it up
            GameObject uiParent = new GameObject("ModListExplorer");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localScale = Vector3.one;
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);

            //Create display
            UITextFrame display = CreateTextFrame(0, 0, uiParent.transform);
            display.name = "Display";
            display.transform.localPosition = ZFixVector;

            //Left  arrow
            //--------------
            //Create arrow button, position and name it
            UIButton leftArrow = CreateButton(UIButton.ButtonType.Default, 0, 0, uiParent.transform, "");
            leftArrow.name = "LeftArrow";
            leftArrow.transform.localScale = new Vector3(0.4f, 0.8f, 1f);
            leftArrow.transform.localPosition = new Vector3(-1.8f, 0f, -0.2f);
            //Replace gfx and position it
            SearchArrows(); //Search for arrows gfx
            Destroy(leftArrow.transform.Find("ModUIElement").Find("Root").Find("Background").gameObject);
            GameObject lArrow = Instantiate(leftArrowOriginal, leftArrow.transform.Find("ModUIElement").Find("Root"));
            lArrow.transform.localScale = new Vector3(2f, 1f, 1f);
            lArrow.transform.localPosition = new Vector3(-0.5f, 0f, 0f);
            //Remove logic components from the gfx
            Destroy(lArrow.GetComponent<GuiClickable>());
            Destroy(lArrow.GetComponent<GuiClickRect>());
            Destroy(lArrow.GetComponent<GuiChangeEffect_ClickUpdSlider>());

            //Right arrow
            //--------------
            //Create arrow button, position and name it
            UIButton rightArrow = CreateButton(UIButton.ButtonType.Default, 0, 0, uiParent.transform, "");
            rightArrow.name = "RightArrow";
            rightArrow.transform.localScale = new Vector3(0.4f, 0.8f, 1f);
            rightArrow.transform.localPosition = new Vector3(1.8f, 0f, -0.2f);
            //Replace gfx and position it
            Destroy(rightArrow.transform.Find("ModUIElement").Find("Root").Find("Background").gameObject);
            GameObject rArrow = Instantiate(rightArrowOriginal, rightArrow.transform.Find("ModUIElement").Find("Root"));
            rArrow.transform.localScale = new Vector3(-2f, 1f, 1f); //The -2 flips the gfx horizontally
            rArrow.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            rArrow.transform.localEulerAngles = Vector3.zero;
            //Remove logic components from the gfx
            Destroy(rArrow.GetComponent<GuiClickable>());
            Destroy(rArrow.GetComponent<GuiClickRect>());
            Destroy(rArrow.GetComponent<GuiChangeEffect_ClickUpdSlider>());

            //Create title
            UITextFrame title = CreateTextFrame(0f, 0f, uiParent.transform);
            title.name = "UIName";
            title.transform.localPosition = new Vector3(0f, 0.65f, 0.6f);
            title.ScaleBackground(new Vector2(0.5f, 0.8f));

            //Assign component
            UIListExplorer output = uiParent.AddComponent<UIListExplorer>();
            output.Initialize();
            output.UIName = name;

            return output;
        }

        //======================
        //Vector3
        //======================
        public UIVector3 CreateVector3(float posX, float posY, Transform parent, string name = "")
        {
            GameObject uiParent = new GameObject("ModVector3");
            uiParent.transform.SetParent(parent, false);
            uiParent.transform.localPosition = new Vector3(posX, posY, 0f);
            UIListExplorer explorer = UIFactory.Instance.CreateListExplorer(0f, 0.75f, uiParent.transform, name); //Create the UIElement
            explorer.ExplorerArray = new string[] { "All directions", "X", "Y", "Z" }; //Create a string[] for the explorer
            explorer.ScaleBackground(0.8f);
            explorer.name = "Vector3Explorer";
            explorer.AllowLooping = true;

            UISlider slider = UIFactory.Instance.CreateSlider(0f, 0f, uiParent.transform, ""); //Create the slider
            slider.name = "Vector3Slider";
            slider.SetSlider(0.01f, 10f, 0.01f, 1f, true, 1f);

            //Assign component
            UIVector3 output = uiParent.AddComponent<UIVector3>();
            output.Initialize();

            return output;
        }

        //======================
        //Utility functions
        //======================
        //Popup frame
        //Only works with UIButton, UIGfxButton, UICheckbox and UISlider
        public UITextFrame CreatePopupFrame(float x, float y, UIElement hover, Transform parent, string text)
        {
            UITextFrame textFrame = UIFactory.Instance.CreateTextFrame(x, y, parent.transform, text);
            textFrame.gameObject.SetActive(false);
            GuiSelectionObject hoverSelection = hover.gameObject.GetComponentInChildren<GuiSelectionObject>();
            bool oldState = false;
            hover.onUpdate += delegate ()
            {
                if (hoverSelection.IsSelected == oldState) { return; }
                oldState = hoverSelection.IsSelected;
                textFrame.gameObject.SetActive(oldState);
            };

            return textFrame;
        }

        //Zfix vector, assigned to UIelements to shift them in the Z axis and avoid z clashing.
        //NOTE: if the accumulated z shift becomes too big (for example, too many nested UIelements), the accumulated z shift
        //might make the element not visible to the camera anymore
        static Vector3 ZFixVector { get { return new Vector3(0f, 0f, -0.2f); } }

        //Search game object by parent. This is a brutish, slow function and it will return the first value found
        public static GameObject SearchGO(string target, string targetParent)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go.name == target)
                {
                    if (go.transform.parent != null)
                    {
                        if (go.transform.parent.name == targetParent)
                        {
                            return go;
                        }
                    }
                }
            }
            return null;
        }

        GameObject originalTextTitle;
        GameObject SearchTitleText()
        {
            if (originalTextTitle == null)
            {
                originalTextTitle = SearchGO("Text", "Root");
            }
            return originalTextTitle;
        }

        GameObject leftArrowOriginal;
        GameObject rightArrowOriginal;
        void SearchArrows()
        {
            if (leftArrowOriginal == null)
            {
                leftArrowOriginal = SearchGO("Left", "Arrows");
                rightArrowOriginal = SearchGO("Right", "Arrows");
            }
        }
    }
}