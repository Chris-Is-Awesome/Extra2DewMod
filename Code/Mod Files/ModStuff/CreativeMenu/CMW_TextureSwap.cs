using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class CMW_TextureSwap : CMenuWindow
    {
        const string TRANSPARENT_TEX_NAME = "transparent";
        const int UILIST_SIZE = 5;

        TextureList objList;
        TextureList _diskList;
        List<Material> _objMaterial;
        int _objMaterialOffset;
        UITextFrame _errorMessage;
        UIButton _saveText;
        UIButton _swapText;
        public override void BuildMenu()
        {
            upperLeftLimit = new Vector2(0.14f, 0.79f);
            lowerRightLimit = new Vector2(0.86f, 0.28f);

            _objMaterial = new List<Material>();

            logic.onChangedSelection += delegate (string name)
            {
                UpdateObjList();
            };

            //Background
            UIBigFrame background = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 1.5f, menuGo.transform);
            background.ScaleBackground(new Vector2(1f, 1.65f));
            background.transform.localPosition += new Vector3(0f, 0f, 0.2f);

            //Title
            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 4.7f, menuGo.transform, "Texture swap");

            /*
            //Quit button
            UIButton escape = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 6f, 4.5f, menuGo.transform, "Close");
            escape.ScaleBackground(new Vector2(0.5f, 1f));
            escape.onInteraction += delegate ()
            {
                logic.CloseCurrentMenu();
            };*/

            //Back and forward buttons
            UIButton goBack = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -5.8f, -1.1f, menuGo.transform, "<--");
            goBack.ScaleBackground(new Vector2(0.4f, 1f), new Vector2(0.7f, 0.7f));
            UIButton goForward = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -4.2f, -1.1f, menuGo.transform, "-->");
            goForward.ScaleBackground(new Vector2(0.4f, 1f), new Vector2(0.7f, 0.7f));

            //Selected object texture explorer
            UI2DList objMaterialsList = UIFactory.Instance.Create2DList(-5f, 1.25f, new Vector2(1f, (float)UILIST_SIZE), Vector2.one * 0.65f, new Vector2(1.3f, 0.75f), menuGo.transform, "Textures");
            objMaterialsList.Title.transform.localPosition += new Vector3(0f, -0.5f, 0f);
            objMaterialsList.transform.localPosition += new Vector3(0f, 0f, -0.2f);
            objMaterialsList.ScrollBar.ResizeLength(5);
            objMaterialsList.ScrollBar.transform.localPosition += new Vector3(-1.2f, 0f, 0f);

            //Error message
            _errorMessage = UIFactory.Instance.CreateTextFrame(-5f, 2f, menuGo.transform);
            _errorMessage.gameObject.SetActive(false);
            _errorMessage.ScaleBackground(new Vector2(1f, 3f));

            //Object texture preview and its background
            UIImage objTexturePreview = UIFactory.Instance.CreateImage(TRANSPARENT_TEX_NAME, -1.5f, 2.7f, menuGo.transform);
            UIBigFrame objTextPrevBack = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, -1.5f, 2.7f, menuGo.transform);
            objTextPrevBack.ScaleBackground(new Vector2(0.17f, 0.65f));
            objTextPrevBack.transform.localPosition += new Vector3(0f, 0f, 0.1f);

            //Object select list
            objList = new TextureList();
            objList.Initialize(objMaterialsList, objTexturePreview, goBack, goForward, logic);
            objList.onUpdateTexture += delegate (int index)
            {
                UpdateObjTexturePreview();
            };

            //Save texture button
            _saveText = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, -1.5f, 0.45f, menuGo.transform);
            _saveText.ScaleBackground(new Vector2(0.6f, 1.6f));
            _saveText.UIName = "Save to\nDisk";
            _saveText.onInteraction += SaveToDisk;
            objList.onUpdateTexture += delegate (int index)
            {
                _saveText.UIName = "Save\n" + objList.CurrentText + "\nto Disk";
            };

            //Materials in disk explorer
            UI2DList diskList = UIFactory.Instance.Create2DList(5f, 1.25f, new Vector2(1f, (float)UILIST_SIZE), Vector2.one * 0.65f, new Vector2(1.3f, 0.75f), menuGo.transform, "On Disk Textures");
            diskList.Title.transform.localPosition += new Vector3(0f, -0.5f, 0f);
            diskList.transform.localPosition += new Vector3(0f, 0f, -0.2f);
            diskList.ScrollBar.ResizeLength(5);
            diskList.ScrollBar.transform.localPosition += new Vector3(-1.2f, 0f, 0f);

            //Disk back and forward buttons
            UIButton goBackDisk = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 4.2f, -1.1f, menuGo.transform, "<--");
            goBackDisk.ScaleBackground(new Vector2(0.4f, 1f), new Vector2(0.7f, 0.7f));
            UIButton goForwardDisk = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 5.8f, -1.1f, menuGo.transform, "-->");
            goForwardDisk.ScaleBackground(new Vector2(0.4f, 1f), new Vector2(0.7f, 0.7f));

            //Disk texture preview and its background
            UIImage diskTexturePreview = UIFactory.Instance.CreateImage(TRANSPARENT_TEX_NAME, 1.5f, 0.0f, menuGo.transform);
            UIBigFrame diskTextPrevBack = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 1.5f, 0.0f, menuGo.transform);
            diskTextPrevBack.ScaleBackground(new Vector2(0.17f, 0.65f));
            diskTextPrevBack.transform.localPosition += new Vector3(0f, 0f, 0.1f);

            //Object select list
            _diskList = new TextureList();
            _diskList.Initialize(diskList, diskTexturePreview, goBackDisk, goForwardDisk, logic);
            _diskList.onUpdateTexture += delegate (int index)
            {
                UpdateDiskImage();
            };

            //Texture swap button
            _swapText = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 1.5f, 3.3f, menuGo.transform);
            _swapText.ScaleBackground(new Vector2(0.6f, 1.2f));
            _swapText.UIName = "Swap\ntexture";
            _swapText.onInteraction += SwapTexture;

            //Reload list button
            UIButton reloadList = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 1.5f, 2.0f, menuGo.transform);
            reloadList.ScaleBackground(new Vector2(0.6f, 1.2f));
            reloadList.UIName = "Refresh\ndisk list";
            reloadList.onInteraction += RefreshDiskList;
            RefreshDiskList();
            UpdateObjList();
        }

        void UpdateObjTexturePreview(bool transparent = false)
        {
            if(transparent)
            {
                SetImageToTransparent(objList.Image);
            }
            else
            {
                int index = objList.CurrentIndex;
                if (index < _objMaterial.Count && index >= 0 && _objMaterial[index].mainTexture != null)
                {
                    objList.Image.ApplyTexture((Texture2D)_objMaterial[index].mainTexture);
                }
                else
                {
                    SetImageToTransparent(objList.Image);
                }
            }
        }

        void SetImageToTransparent(UIImage image)
        {
            image.ChangeTexture(ModMaster.TexturesPath + TRANSPARENT_TEX_NAME + ".png");
        }

        public override void OnOpen()
        {

        }

        void UpdateObjList()
        {
            if (logic.selObj != null)
            {
                objList.SetListName(logic.selObj.name);
                Renderer renderer = logic.selObj.GetComponent<Renderer>();
                _objMaterial.Clear();
                List<string> texNames = new List<string>();

                if (renderer != null)
                {
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        if (renderer.materials[i].mainTexture != null)
                        {
                            texNames.Add(renderer.materials[i].mainTexture.name);
                            _objMaterial.Add(renderer.materials[i]);
                        }
                    }
                    if (_objMaterial.Count == 0)
                    {
                        _errorMessage.UIName = "No textures\navailable";
                        _errorMessage.gameObject.SetActive(true);
                        _saveText.gameObject.SetActive(false);
                    }
                    else
                    {
                        _saveText.gameObject.SetActive(true);
                        _errorMessage.gameObject.SetActive(false);
                    }
                }
                else
                {
                    _errorMessage.UIName = "No renderer found\n\nUse '3D Graphics'\nselection mode";
                    _saveText.gameObject.SetActive(false);
                    _errorMessage.gameObject.SetActive(true);
                }
                objList.SetList(texNames.ToArray());
                UpdateObjTexturePreview();
            }
            else
            {
                _errorMessage.UIName = "No object\nselected";
                _errorMessage.gameObject.SetActive(true);
                _saveText.gameObject.SetActive(false);
                objList.SetListName("----");
                objList.EmptyList();
                UpdateObjTexturePreview(true);
            }
            UpdateSwapButton();
        }

        void UpdateSwapButton()
        {
            _swapText.gameObject.SetActive(_objMaterial.Count > 0 && _diskList.CurrentText != "NOLIST!");
        }

        void RefreshDiskList()
        {
            List<string> output = new List<string>();
            string path = ModMaster.MaterialsPath;
            foreach (string file in System.IO.Directory.GetFiles(path))
            {
                int pos = file.IndexOf(".png");
                if (pos >= 0)
                {
                    string beforeTxt = file.Remove(pos);
                    beforeTxt = beforeTxt.Remove(0, path.Length);
                    if (!output.Contains(beforeTxt)) output.Add(beforeTxt);
                }
            }

            _diskList.SetList(output.ToArray());
        }
        

        void UpdateDiskImage()
        {
            string toLoad = _diskList.CurrentText;
            if (toLoad == "NOLIST!") SetImageToTransparent(_diskList.Image);

            string path = ModMaster.MaterialsPath + toLoad + ".png";

            if (File.Exists(path))
            {
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                byte[] fileData;
                fileData = File.ReadAllBytes(path);
                tex.LoadImage(fileData, false);
                _diskList.Image.ApplyTexture(tex);
            }
            else
            {
                SetImageToTransparent(_diskList.Image);
            }
        }

        void SaveToDisk()
        {
            if (objList.CurrentIndex < 0 || objList.CurrentIndex > _objMaterial.Count) return;
            Texture texture = _objMaterial[objList.CurrentIndex].mainTexture;
            if (texture == null) return;

            //Mumbo jumbo
            Texture2D img = (Texture2D)texture;
            Color32[] pixelBlock = null;
            img.filterMode = FilterMode.Point;
            RenderTexture rt = RenderTexture.GetTemporary(img.width, img.height);
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            Graphics.Blit(img, rt);
            Texture2D img2 = new Texture2D(img.width, img.height);
            img2.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
            img2.Apply();
            RenderTexture.active = null;
            img = img2;
            pixelBlock = img.GetPixels32(0);

            //Create new texture2D
            Texture2D newText = new Texture2D(texture.width, texture.height);
            newText.SetPixels32(pixelBlock, 0);

            //Write to disk
            byte[] itemBGBytes = newText.EncodeToPNG();
            File.WriteAllBytes(ModMaster.MaterialsPath + texture.name + ".png", itemBGBytes);
            RefreshDiskList();
        }

        void SwapTexture()
        {
            if (objList.CurrentIndex < 0 || objList.CurrentIndex > _objMaterial.Count) return;
            Texture texture = _objMaterial[objList.CurrentIndex].mainTexture;
            if (texture == null) return;

            string toLoad = _diskList.CurrentText;
            if (toLoad == "NOLIST!") return;

            string path = ModMaster.MaterialsPath + toLoad + ".png";

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (File.Exists(path))
            {
                byte[] fileData;
                fileData = File.ReadAllBytes(path);
                tex.LoadImage(fileData, false);
                tex.name = _diskList.CurrentText;
                _objMaterial[objList.CurrentIndex].mainTexture = tex;
                objList.ChangeCurrentText(_diskList.CurrentText);
                UpdateObjTexturePreview();
            }
        }

        void OnButtonHidden()
        {

        }

        class TextureList
        {
            public delegate void OnUpdateTexture(int index);
            public event OnUpdateTexture onUpdateTexture;

            string[] _entries;
            UI2DList _list;
            UIImage _image;
            int _index;
            UIButton _back;
            UIButton _next;
            CMenuLogic _logic;

            public void Initialize(UI2DList list, UIImage image, UIButton back, UIButton next, CMenuLogic logic)
            {
                _entries = new string[] { };
                _logic = logic;
                _image = image;
                _back = back;
                _back.onInteraction += delegate ()
                {
                    AdvanceList(-UILIST_SIZE, false);
                    if (!_back.gameObject.activeSelf) logic.DeselectGUIElement(true);
                };
                _next = next;
                _next.onInteraction += delegate ()
                {
                    AdvanceList(UILIST_SIZE, false);
                    if (!_next.gameObject.activeSelf) logic.DeselectGUIElement(true);
                };
                _list = list;
                _list.onInteraction += delegate (string text, int index)
                {
                    RequestImageUpdate();
                };
                AdvanceList(0, false);
            }

            public void AdvanceList(int advance, bool forceUpdate)
            {
                int nextValue = _index + advance;
                if (forceUpdate || (nextValue >= 0 && nextValue < _entries.Length))
                {
                    _index = nextValue;
                    int newSize = Mathf.Clamp(_entries.Length - nextValue, 0, UILIST_SIZE);
                    string[] explorerArray = new string[newSize];
                    for (int i = 0; i < newSize; i++)
                    {
                        explorerArray[i] = _entries[_index + i];
                    }
                    _list.ExplorerArray = explorerArray;
                    _list.IndexValue = 0;
                }
                _back.gameObject.SetActive(_index != 0);
                _next.gameObject.SetActive(_index < (_entries.Length - UILIST_SIZE));
                RequestImageUpdate();
            }

            public void RequestImageUpdate()
            {
                onUpdateTexture?.Invoke(CurrentIndex);

            }

            public void SetList(string[] stringList)
            {
                _entries = stringList;
                _index = 0;
                _list.IndexValue = 0;
                AdvanceList(0, true);
            }

            public void SetListName(string listName)
            {
                _list.UIName = listName;
            }

            public void EmptyList()
            {
                _entries = new string[] { };
                _index = 0;
                _list.IndexValue = 0;
                AdvanceList(0, true);
            }

            public void ChangeCurrentText(string text)
            {
                string[] currentArray = _list.ExplorerArray;
                int currentIndex = _list.IndexValue;
                currentArray[currentIndex] = text;
                _list.ExplorerArray = currentArray;
                _list.IndexValue = currentIndex;
            }

            public int CurrentIndex { get { return _index + _list.IndexValue; } }
            public string CurrentText { get { return _list.StringValue; } }
            public UIImage Image { get { return _image; } }
        }

    }
}
