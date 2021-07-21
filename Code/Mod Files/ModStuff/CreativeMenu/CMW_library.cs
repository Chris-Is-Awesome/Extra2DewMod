using UnityEngine;

namespace ModStuff.CreativeMenu
{
    public class CMW_Library : CMenuWindow
    {
        UIButton addSelected;
        UI2DList libraryContent;
        GameObject buttonsHolder;
        public override void BuildMenu()
        {
            upperLeftLimit = new Vector2(0.22f, 0.8f);
            lowerRightLimit = new Vector2(0.79f, 0.33f);

            //Create/delete holder
            buttonsHolder = new GameObject();
            buttonsHolder.transform.SetParent(menuGo.transform, false);
            buttonsHolder.transform.localPosition = Vector3.zero;
            buttonsHolder.transform.localScale = Vector3.one;

            //Background
            UIBigFrame background = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1.8f, menuGo.transform, "");
            background.ScaleBackground(new Vector2(0.8f, 1.5f));

            //Title
            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 4.5f, menuGo.transform, "Library");

            //Content
            libraryContent = UIFactory.Instance.Create2DList(-0.5f, 1.5f, new Vector2(3f, 3f), Vector2.one, new Vector2(1f, 0.5f), new Vector2(1f, 1.375f), menuGo.transform, "Library");
            libraryContent.Title.gameObject.SetActive(false);
            libraryContent.onInteraction += delegate (string textValue, int arrayIndex)
            {
                CMenuStorage.Instance.Index = arrayIndex;
            };
            libraryContent.ScrollBar.transform.localPosition += new Vector3(1.9f, -0.5f, -0.2f);
            libraryContent.ScrollBar.ResizeLength(4);

            //Go back
            UIButton escape = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 5f, 4.5f, menuGo.transform, "Close");
            escape.ScaleBackground(new Vector2(0.5f, 1f));
            escape.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };

            //Add selected
            addSelected = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -3f, -1f, menuGo.transform, "Add selected\nto library");
            addSelected.ScaleBackground(new Vector2(0.8f, 1.5f));
            addSelected.gameObject.SetActive(false);

            //Create button
            UIButton create = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 0.4f, -1f, buttonsHolder.transform, "Spawn");
            create.ScaleBackground(new Vector2(0.5f, 1f));
            create.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
                logic.TriggerToolByPass(CMenuLogic.ToolList.InstanceItem);
            };

            //Rename button
            UIButton rename = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 2.7f, -1f, buttonsHolder.transform, "Rename");
            rename.ScaleBackground(new Vector2(0.5f, 1f));
            rename.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.LibraryRename);
            };

            //Delete button
            UIButton delete = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 5f, -1f, buttonsHolder.transform, "Delete");
            delete.ScaleBackground(new Vector2(0.5f, 1f));
            delete.onInteraction += delegate ()
            {
                logic.OpenAndToggleMenu(CMenuLogic.MenuList.LibraryDelete);
            };

            //Name update on selection
            logic.onChangedSelection += delegate (string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    addSelected.gameObject.SetActive(false);;
                }
                else
                {
                    addSelected.gameObject.SetActive(true);
                    addSelected.UIName = "Add " + name + "\nto library";
                }
            };

            //Store selected item
            addSelected.onInteraction += delegate ()
            {
                logic.TriggerToolByPass(CMenuLogic.ToolList.StoreItem);
            };
            
            //On store item library update
            logic.onStoreItem += delegate ()
            {
                UpdateLibraryContent();
            };

            CMenuStorage.Instance.onListUpdate += UpdateLibraryContent;
            CMenuStorage.Instance.onNameChange += OnRename;
            logic.onLogicDestroyed += delegate ()
            {
                CMenuStorage.Instance.onListUpdate -= UpdateLibraryContent;
                CMenuStorage.Instance.onNameChange -= OnRename;
            };
            UpdateLibraryContent();
        }

        void UpdateLibraryContent()
        {
            string[] itemList = CMenuStorage.Instance.ItemList();
            libraryContent.ExplorerArray = itemList;

            buttonsHolder.SetActive(itemList.Length != 0);
        }

        void OnRename()
        {
            int currentIndex = libraryContent.IndexValue;
            UpdateLibraryContent();
            libraryContent.IndexValue = currentIndex;
        }
    }
}
