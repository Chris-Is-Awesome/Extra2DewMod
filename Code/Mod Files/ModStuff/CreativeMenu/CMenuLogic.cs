using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using ModStuff.CreativeMenu;

namespace ModStuff.CreativeMenu
{
    public class CMenuLogic : MonoBehaviour
    {
        //The order of this list must match the how tools are added to the list
        public enum ToolList { Select, Move, Rotate, Scale, Clone, Attach, Hold, Load, DisableAnimations, ToggleMarkers, FullScreen,
            Library, Help, Filter, StoreItem, InstanceItem, Delete, MoveSpawned, OpenLastMenu}
        public enum MenuList { ToolSelect, Library, Help, Filter, Delete, Gadgets, Lights, Mechanisms, TextureSwap, Bubbles, Options,
            LibraryDelete, LibraryRename, BubblesEdit }
        public enum MarkersState { Default, AlwaysOn, AlwaysOff}

        public delegate void OnCMenuEvent();
        public OnCMenuEvent onExit;
        //public OnCMenuEvent onHideMenu;
        public OnCMenuEvent onLogicDestroyed;
        public OnCMenuEvent onOpenLibrary;
        public OnCMenuEvent onOpenHelp;
        public OnCMenuEvent onOpenFilter;
        public OnCMenuEvent onStoreItem;

        public delegate void OnCMenuNameEvent(string name);
        public OnCMenuNameEvent onChangedSelection;
        public OnCMenuNameEvent onChangedSaved;
        public event OnCMenuNameEvent onDebug;
        public event OnCMenuNameEvent onToolChange;

        public delegate void OnCMenuBoolEvent(bool active);
        public OnCMenuBoolEvent onFullScreen;

        public delegate void OnCMenuModeEvent(ObjFinder.SelecMode mode);
        public OnCMenuModeEvent onChangedMode;

        List<Tool> availableTools;
        CMenuWindow[] availableMenus;
        CMenuWindow currentMenu;
        Tool currentTool;
        Tool dragTool;

        bool cameraMoving;
        bool alwaysDrawMarkers;
        int originalCamMode;
        bool cameraChanged;
        bool focusedCamera;
        bool fullScreenMode;
        bool menuOpen;
        bool menuOpenDelay;
        bool menuCloseDelay;
        bool onCanvasDelay;
        int deselectDelay;
        bool dontForceClose;
        Transform focusedObject;

        bool mouse0Pressed;

        public MarkersState markersState;

        public Transform player;
        public Transform selObj;
        Transform pastSelObj;
        Transform savedObj;

        ObjFinder.SelecMode currentMode = ObjFinder.SelecMode.EntitiesGfx;
        ObjFinder finder;

        Vector2 _upperLeftMenuLimit = Vector2.zero;
        Vector2 _lowerRightMenuLimit = Vector2.zero;

        MenuList[] trackedMenus = new MenuList[] { MenuList.Lights, MenuList.Mechanisms, MenuList.Bubbles, MenuList.TextureSwap };
        MenuList lastMenu = MenuList.TextureSwap;

        public bool recklessDelete;

        class Tool
        {
            public delegate void OnKeyPress(CMenuTool handler);

            public KeyCode hotKey;
            public CMenuTool handler;
            public OnKeyPress onKeyPress;
            public OnKeyPress onCancel;
            public OnKeyPress onStop;
        }

        public void TriggerTool(ToolList tool)
        {
            if (menuOpen || ToolActive() || cameraMoving) return;
            Tool toUse = availableTools[(int)tool];
            TryFireTool(toUse);
        }

        public void TriggerToolByPass(ToolList tool)
        {
            if (ToolActive() || cameraMoving) return;
            Tool toUse = availableTools[(int)tool];
            TryFireTool(toUse);
        }

        public void SetDragTool(ToolList tool)
        {
            if (menuOpen || ToolActive() || cameraMoving) return;
            dragTool = (tool == ToolList.Select) ? null : availableTools[(int)tool];
            onToolChange?.Invoke(tool.ToString());
        }

        GuiSelectionHandler _selectionHandler;
        void OnEnable()
        {
            if (_selectionHandler == null)
            {
                GameObject overlay = GameObject.Find("PauseOverlay");
                if (overlay != null && overlay.GetComponent<GuiSelectionHandler>() != null)
                    _selectionHandler = GameObject.Find("PauseOverlay").GetComponent<GuiSelectionHandler>();
            }
        }

        void Awake()
        {
            MechanismManager.MeshVisibility = true;
            LightMeshVisibility.MeshVisibility = true;
            ObjHighlight.DisableCreation = false;

            availableTools = new List<Tool>();
            availableMenus = new CMenuWindow[Enum.GetNames(typeof(MenuList)).Length];
            player = GameObject.Find("PlayerEnt").transform;

            CM_Utility utility = new CM_Utility();

            GameObject go = new GameObject("Finder", typeof(ObjFinder));
            go.transform.SetParent(this.transform);
            finder = go.GetComponent<ObjFinder>();

            Tool tool;

            //Select
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.F;
            tool.handler = utility;
            tool.onKeyPress = SelectSelect;

            //Mover
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.G;
            tool.handler = new CM_Mover();

            //Rotate
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.R;
            tool.handler = new CM_Rotate();

            //Scale
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.S;
            tool.handler = new CM_Scale();

            //Clone
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.C;
            tool.handler = new CM_Clone();
            tool.onKeyPress = CloneObj;
            tool.onCancel = CancelCloneObj;

            //Utilities
            //-----------
            //ChangeP
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.T;
            tool.handler = utility;
            tool.onKeyPress = ChangeParent;
            //Save
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.Q;
            tool.handler = utility;
            tool.onKeyPress = SaveTransform;
            //Load
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.E;
            tool.handler = utility;
            tool.onKeyPress = LoadTransform;
            //Disable animations
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.K;
            tool.handler = utility;
            tool.onKeyPress = ToggleAnimation;
            //Toggle markers
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.Z;
            tool.handler = utility;
            tool.onKeyPress = ToggleMarkers;
            //Full screen
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.B;
            tool.handler = utility;
            tool.onKeyPress = ToolFullScreen;
            //Library
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.V;
            tool.handler = utility;
            tool.onKeyPress = OpenLibrary;
            //Help
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.H;
            tool.handler = utility;
            tool.onKeyPress = OpenHelp;
            //Search
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.Space;
            tool.handler = utility;
            tool.onKeyPress = OpenSearch;

            CM_Storage storage = new CM_Storage();
            //Store item
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.O;
            tool.handler = storage;
            tool.onKeyPress = StoreItem;
            //Instance item
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.P;
            tool.handler = storage;
            tool.onKeyPress = RetrieveItem;

            //Delete
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.Delete;
            tool.handler = utility;
            tool.onKeyPress = OpenDelete;

            //Move spawned
            CM_MoveSpawned moveSpawned = new CM_MoveSpawned();
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.DownArrow;
            tool.handler = moveSpawned;
            tool.onKeyPress = MoveSpawned;

            //Open last menu
            tool = new Tool();
            availableTools.Add(tool);
            tool.hotKey = KeyCode.J;
            tool.handler = utility;
            tool.onKeyPress = OpenLastMenu;
        }

        void Update()
        {
            //Set toolActive flag
            bool toolActive = ToolActive();

            if(deselectDelay > 0)
            {
                deselectDelay--;
                if (deselectDelay == 0) DeselectGUIElement(true);
            }

            if (menuOpenDelay)
            {
                menuOpenDelay = false;
                return;
            }

            //Escape key pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (toolActive)
                {
                    currentTool.handler.Deactivate(true);
                    currentTool.onCancel?.Invoke(currentTool.handler);
                }
                else if (cameraMoving) FreeCamera(false);
                else if (fullScreenMode) ActivateFullScreen(false);
                else if (menuOpen)
                {
                    CloseCurrentMenu();
                }
                else
                {
                    dontForceClose = true;
                    UpdateMarkers(false);
                    ResetCamera();
                    onExit?.Invoke();
                }

                return;
            }
            
            //If a menu is open, check if we are clicking outside a button. If we do, close the menu. Afterwards, exit
            if (menuOpen)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && MouseOnCanvas()) CloseCurrentMenu();
                /*
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    Vector2 mousePos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
                    string pos = mousePos.x.ToString("f2") + " " + mousePos.y.ToString("f2");
                    onDebug?.Invoke(pos);
                }*/
                onCanvasDelay = false;
                return;
            }
            else
            {
                onCanvasDelay = false;
            }

            //Run updates
            if (currentTool != null) currentTool.handler.TryRunUpdate();
            if (cameraMoving) ModCamera.Instance.ModCamUpdate();

            //If not moving or a tool is active, allow select, key detection and mode switching
            if (!toolActive && !cameraMoving)
            {
                for (int i = 0; i < availableTools.Count; i++)
                {
                    if (availableTools[i].hotKey != KeyCode.DownArrow && Input.GetKeyDown(availableTools[i].hotKey))
                    {
                        TryFireTool(availableTools[i]);
                        return;
                    }
                }

                if (Input.mouseScrollDelta.y < 0) ChangeSelectionMode(-1);
                if (Input.mouseScrollDelta.y > 0) ChangeSelectionMode(1);
            }

            //Right click (hold)
            if(!toolActive)
            {
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    if (!cameraMoving) FreeCamera(true);
                }
                else if (cameraMoving && !Input.GetKey(KeyCode.Mouse1))
                {
                    FreeCamera(false);
                }
            }

            //Right click
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (toolActive) currentTool.handler.Deactivate(true);
                currentTool.onCancel?.Invoke(currentTool.handler);
            }

            //If not tool is active, check camera controls
            if (Input.GetKeyDown(KeyCode.X) && !toolActive)
            {
                FocusCamera();
            }

            //Left click
            if (Input.GetKeyDown(KeyCode.Mouse0) && MouseOnCanvas() && !menuCloseDelay)
            {
                mouse0Pressed = true;

                //If the camera is moving, stop it
                if (cameraMoving) FreeCamera(false);
                else
                {
                    //If a tool is active, stop it
                    if (toolActive)
                    {
                        currentTool.handler.Deactivate(false);
                    }
                    else
                    {
                        //Select an object
                        TrySelect();

                        //If a dragtool is selected, try to use it
                        if (dragTool != null) TryFireTool(dragTool);
                        if (dragTool.handler.Active) dragTool.handler.ActivateDragMode();
                    }
                }
            }

            //Tool release (drag tools)
            if (!Input.GetKey(KeyCode.Mouse0))
            {
                if (mouse0Pressed)
                {
                    mouse0Pressed = false;
                    if (dragTool != null && toolActive)
                    {
                        currentTool.handler.Deactivate(false);
                    }
                }
            }

            menuCloseDelay = false;
        }

        void TryFireTool(Tool tool)
        {
            if (tool.onKeyPress != null) tool.onKeyPress?.Invoke(tool.handler);
            else tool.handler.Activate(selObj);

            if (tool.handler.Active) currentTool = tool;
        }

        void TrySelect()
        {
            List<Transform> found = finder.MouseScan(currentMode);
            if (found.Count < 1)
            {
                ClearSelection();
            }
            else
            {
                selObj = found[0];
                ObjHighlight.HighlightObjects(selObj);
                if (currentMode == ObjFinder.SelecMode.Armatures)
                {
                    SkinnedMeshRenderer meshObj = FindSMR(selObj);
                    if (meshObj != null) meshObj.updateWhenOffscreen = true;
                }
                onChangedSelection?.Invoke(selObj.name);
            }
        }

        bool ToolActive()
        {
            if (currentTool == null) return false;
            return currentTool.handler.Active;
        }

        public void FocusCamera()
        {
            if (focusedCamera)
            {
                focusedCamera = false;
                ModCamera.Instance.FPSToggle(3, false);
            }
            else if (selObj != null)
            {
                if(!cameraChanged) originalCamMode = ModCamera.Instance.fpsmode;
                cameraChanged = true;
                focusedCamera = true;
                focusedObject = selObj;
                ModCamera.Instance.FPSToggle(2, false);
                ModCamera.Instance.ChangeFocus(focusedObject);
            }
        }

        public void FreeCamera(bool activate)
        {
            cameraMoving = activate;

            UpdateMarkers(!cameraMoving);

            if (!cameraChanged && cameraMoving && ModCamera.Instance.fpsmode != 3)
            {
                cameraChanged = true;
                originalCamMode = ModCamera.Instance.fpsmode;
                ModCamera.Instance.FPSToggle(3, false);
                if (originalCamMode == 0) ModCamera.Instance.CMenuZoomIn();
            }
        }

        public void SetMenuLimits(Vector2 upperLeft, Vector2 lowerRight)
        {
            _upperLeftMenuLimit = upperLeft;
            _lowerRightMenuLimit = lowerRight;
        }

        public void ResetMenuLimits()
        {
            _upperLeftMenuLimit = Vector2.zero;
            _lowerRightMenuLimit = Vector2.zero;
        }

        public bool ChangeSelectionMode(int num)
        {
            if (ToolActive()) return false;
            Array a = Enum.GetValues(typeof(ObjFinder.SelecMode));
            int newIndex = (int)currentMode + num;
            if (newIndex < 0) newIndex = 0;
            else if (newIndex >= a.Length) newIndex = a.Length - 1;
            currentMode = (ObjFinder.SelecMode)a.GetValue((int)newIndex);
            UpdateMarkers(true);
            onChangedMode?.Invoke(currentMode);
            DeselectGUIElement(true);
            return true;
        }

        public void UpdateMarkers(bool enable)
        {
            if(enable)
            {
                List<Transform> objsOnScreen = finder.ObjectsOnScreen(currentMode);
                ObjHighlight.HighlightObjects(objsOnScreen, alwaysDrawMarkers);
            }
            else
            {
                ObjHighlight.ClearHighlights(true);
                ObjHighlight.ClearHighlights(false);
            }
        }

        SkinnedMeshRenderer FindSMR(Transform obj)
        {
            Transform root = obj;

            while (root != null)
            {
                if (root.name.Contains("Armature"))
                {
                    root = root.parent;
                    if (root == null) return null;
                    return root.GetComponentInChildren<SkinnedMeshRenderer>();
                }
                root = root.parent;
            }
            return null;
        }

        public void ResetCamera()
        {
            if(cameraChanged)
            {
                ModCamera.Instance.FPSToggle(originalCamMode, true);
                cameraChanged = false;
                focusedCamera = false;
                focusedObject = null;
            }
        }

        public static float topTreshold = 0.84f;
        public static float bottomTreshold = 0.23f;

        public void ActivateFullScreen(bool activate)
        {
            fullScreenMode = activate;
            onFullScreen?.Invoke(activate);
        }

        bool MouseOnCanvas()
        {
            bool output = true;

            //If the delay is on, return false
            if (onCanvasDelay) return false;

            //If the mouse is in a menu rect, return false
            Vector2 mousePos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            if (mousePos.x > _upperLeftMenuLimit.x &&
                mousePos.x < _lowerRightMenuLimit.x &&
                mousePos.y < _upperLeftMenuLimit.y &&
                mousePos.y > _lowerRightMenuLimit.y)
                output = false;

            //If a GUI element is curretnly selected and in selected state, return false
            if (_selectionHandler != null && 
                _selectionHandler.CurrentSelected != null &&
                _selectionHandler.CurrentSelected.IsSelected)
                output = false;

            return output;
        }
        
        //--------------------------------------------------
        //OnKeyPress functions
        //--------------------------------------------------
        void CloneObj(CMenuTool handler)
        {
            CM_Clone cloneHandler = handler as CM_Clone;

            cloneHandler.Activate(selObj);
            if (!handler.Active) return;
            pastSelObj = selObj;
            selObj = cloneHandler.GetClone();
        }

        void CancelCloneObj(CMenuTool handler)
        {
            selObj = pastSelObj;
        }

        //--------------------------------------------------
        //Utility
        //--------------------------------------------------
        void SelectSelect(CMenuTool handler)
        {
            dragTool = null;
        }

        void ChangeParent(CMenuTool handler)
        {
            if (savedObj == null || selObj == null) return;
            selObj.SetParent(savedObj, true);
            List<Transform> highlights = new List<Transform>() { selObj, savedObj };
            ObjHighlight.HighlightObjects(highlights);
        }

        void SaveTransform(CMenuTool handler)
        {
            if (selObj == null) return;
            bool fireEvent = false;
            if (selObj != savedObj) fireEvent = true;
            savedObj = selObj;
            bool objectIsNull = savedObj == null;
            if (!objectIsNull)
            {
                ObjHighlight.HighlightObjects(selObj);
            }
            if (fireEvent)
            {
                onChangedSaved?.Invoke(objectIsNull ? null : savedObj.name);

            }
        }

        void LoadTransform(CMenuTool handler)
        {
            if (savedObj == null) return;
            ForceSelect(savedObj);
            ObjHighlight.HighlightObjects(selObj);
        }

        void ToggleAnimation(CMenuTool handler)
        {
            if (selObj == null) return;
            if (GetanimationState(selObj, out Animation anim)) anim.enabled = !anim.enabled;
            ObjHighlight.HighlightObjects(selObj);
        }

        void ToolFullScreen(CMenuTool handler)
        {
            ActivateFullScreen(!fullScreenMode);
        }

        public bool GetanimationState(Transform selObj, out Animation anim)
        {
            anim = null;
            if (selObj == null) return false;
            anim = TransformUtility.FindInParents<Animation>(selObj.transform);
            if (anim != null) return true;
            return false;
        }

        public void ToggleMarkers(MarkersState state)
        {
            markersState = state;
            switch (state)
            {
                case MarkersState.Default:
                    alwaysDrawMarkers = false;
                    ObjHighlight.DisableCreation = false;
                    UpdateMarkers(alwaysDrawMarkers);
                    break;
                case MarkersState.AlwaysOn:
                    ObjHighlight.DisableCreation = false;
                    alwaysDrawMarkers = true;
                    UpdateMarkers(alwaysDrawMarkers);
                    break;
                case MarkersState.AlwaysOff:
                    ObjHighlight.DisableCreation = true;
                    UpdateMarkers(false);
                    break;
            }
        }

        void ToggleMarkers(CMenuTool handler)
        {
            alwaysDrawMarkers = !alwaysDrawMarkers;
            ObjHighlight.DisableCreation = false;
            markersState = alwaysDrawMarkers ? MarkersState.AlwaysOn : MarkersState.Default;
            UpdateMarkers(alwaysDrawMarkers);
        }

        void EmptyFunc(CMenuTool handler)
        {

        }
        
        void StoreItem(CMenuTool handler)
        {
            CM_Storage libraryHandler = handler as CM_Storage;
            if (selObj == null) return;
            libraryHandler.StoreItem(selObj);
            ObjHighlight.HighlightObjects(selObj);
            onStoreItem?.Invoke();
        }

        void RetrieveItem(CMenuTool handler)
        {
            CM_Storage libraryHandler = handler as CM_Storage;
            Vector3 spawnPoint;
            if (selObj != null) spawnPoint = selObj.position;
            else spawnPoint = player.position;
            selObj = libraryHandler.SpawnItem(spawnPoint);
            onChangedSelection?.Invoke(selObj.name);
            libraryHandler.Activate(selObj);
        }
        
        void MoveSpawned(CMenuTool handler)
        {
            CM_MoveSpawned libraryHandler = handler as CM_MoveSpawned;
            Vector3 spawnPoint;
            if (selObj != null) spawnPoint = selObj.position;
            else spawnPoint = player.position;
            spawnPoint += new Vector3(0f, 0f, 2f);
            libraryHandler.MoveItem(spawnPoint, selObj);
            onChangedSelection?.Invoke(selObj.name);
            libraryHandler.Activate(selObj);
        }

        void CancelRetrieve(CMenuTool handler)
        {
            ClearSelection();
        }

        void OpenLastMenu(CMenuTool handler)
        {
            OpenAndToggleMenu(lastMenu);
        }

        void ClearSelection()
        {
            selObj = null;
            onChangedSelection?.Invoke(null);
        }

        public void SetFilter(string text, bool caseSensitive, bool exactName)
        {
            if (string.IsNullOrEmpty(text)) finder.SetFilter(null);
            else if (caseSensitive)
            {
                if(exactName) finder.SetFilter(transform => transform.name != text);
                else finder.SetFilter(transform => !transform.name.Contains(text));
            }
            else
            {
                text = text.ToLower();
                if (exactName) finder.SetFilter(transform => transform.name.ToLower() != text);
                else finder.SetFilter(transform => !transform.name.ToLower().Contains(text));
            }
        }

        public void DeleteSelected()
        {
            if (selObj != null)
            {
                GameObject.Destroy(selObj.gameObject);
                ClearSelection();
            }
        }

        public void ForceSelect(Transform transform)
        {
            selObj = transform;
            onChangedSelection?.Invoke(selObj == null ? null : selObj.name);
        }

        public void ForceClose()
        {
            if(dontForceClose)
            {
                dontForceClose = false;
                return;
            }
            if (ToolActive())
            {
                currentTool.handler.Deactivate(true);
                currentTool.onCancel?.Invoke(currentTool.handler);
            }
            if (cameraMoving) FreeCamera(false);
            if (fullScreenMode) ActivateFullScreen(false);
            if (menuOpen)
            {
                menuOpenDelay = true;
                CloseCurrentMenu();
            }
            UpdateMarkers(false);
            ResetCamera();
        }

        //--------------------------------------------------
        //Menu windows handling
        //--------------------------------------------------
        public void AddMenu(MenuList index, CMenuWindow window)
        {
            availableMenus[(int)index] = window;
        }

        public void DeselectGUIElement(bool delayOnCanvasDetect)
        {
            _selectionHandler.ChangeSelection(null, false);
            if (delayOnCanvasDetect) onCanvasDelay = true;
        }

        public void DeselectWithDelay()
        {
            deselectDelay = 10;
        }

        public void CloseCurrentMenu()
        {
            menuOpen = false;
            ResetMenuLimits();
            if (currentMenu == null) return;
            CMenuWindow lastMenu = currentMenu; 
            currentMenu = null;
            menuCloseDelay = true;
            lastMenu.Close();
            DeselectGUIElement(true);
        }

        public void OpenAndToggleMenu(MenuList index)
        {
            if (ToolActive() || cameraMoving) return;
            CMenuWindow oldCurrent = currentMenu;
            CMenuWindow nextMenu = availableMenus[(int)index];
            if (nextMenu == null) return;
            CloseCurrentMenu();
            if (oldCurrent != nextMenu && !menuOpenDelay)
            {
                menuOpen = true;
                menuOpenDelay = true;
                currentMenu = nextMenu;
                currentMenu.Open();
                for(int i = 0; i < trackedMenus.Length; i++)
                {
                    if (trackedMenus[i] == index) lastMenu = index;
                }
            }
        }

        void OpenLibrary(CMenuTool handler)
        {
            OpenAndToggleMenu(MenuList.Library);
        }

        void OpenHelp(CMenuTool handler)
        {
            OpenAndToggleMenu(MenuList.Help);
        }

        void OpenSearch(CMenuTool handler)
        {
            SetFilter("", false, false);
            UpdateMarkers(false);
            UpdateMarkers(true);
            OpenAndToggleMenu(MenuList.Filter);
        }

        void OpenDelete(CMenuTool handler)
        {
            OpenAndToggleMenu(MenuList.Delete);
        }

        void OnDestroy()
        {
            onLogicDestroyed?.Invoke();
        }
    }
}