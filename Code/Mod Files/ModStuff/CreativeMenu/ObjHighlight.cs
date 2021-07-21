using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace ModStuff.CreativeMenu
{
	public class ObjHighlight : MonoBehaviour
	{
		Transform following;
		Camera camera;
		Texture2D tex;
        float timer;
		float blockSize;

        const string TEXTURE_NAME = "highlighgfx";
        const float HIGHLIGHT_LIFETIME = 2f;
        const float HIGHLIGHT_BLINK = 0.25f;
        const float HIGHLIGHT_GROWTH = 2.0f;
        const float HIGHLIGHT_SIZE = 0.25f;

        static Texture2D _texture;
        static AnimationCurve bumpCurve;

        void Init(Transform goToFollow, Camera cam, float time)
        {
            //Parent to overlaycamera
            transform.SetParent(OverlayCamera.transform);
            transform.localPosition = Vector3.zero;

            //Make plane and texture
            if (_texture == null) SetGfx();
            GameObject go = GameObject.Instantiate(BaseHighlight);
            go.SetActive(true);
            go.transform.SetParent(transform, false);
            go.transform.rotation = OverlayCamera.transform.rotation;

            //Asign variables
            following = goToFollow;
            camera = cam;
            Destroy(gameObject, time);

            //Refresh plane
            Update();
        }

        void ResetTimer()
        {
            timer = 0f;
        }

        public static float factorX = 6f;
        public static float factorY = 6f;
        void Update()
        {
            if (following != null && camera != null)
            {
                Vector3 camPos = camera.WorldToScreenPoint(following.position);
                camPos = new Vector3((camPos.x - camera.pixelWidth / 2) * (factorX / camera.pixelHeight), (camPos.y - camera.pixelHeight / 2) * (factorY / camera.pixelHeight), 0.3f);
                transform.localPosition = camPos;

                timer += Time.deltaTime;
                blockSize = bumpCurve.Evaluate(timer);
                transform.localScale = Vector3.one * blockSize;
            }
        }

        //---------------------------------
        //References
        //---------------------------------
        static void SetGfx()
        {
            //Load image texture and apply as material
            string path = ModMaster.TexturesPath + TEXTURE_NAME + ".png";
            Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            byte[] fileData;
            if (File.Exists(path))
            {
                fileData = File.ReadAllBytes(path);
                tex.LoadImage(fileData, false);
            }
            _texture = tex;

            //Set animation curve
            bumpCurve = new AnimationCurve();
            bumpCurve.preWrapMode = WrapMode.ClampForever;
            bumpCurve.postWrapMode = WrapMode.ClampForever;
            bumpCurve.AddKey(0f, 1f);
            bumpCurve.AddKey(HIGHLIGHT_BLINK / 2f, HIGHLIGHT_GROWTH);
            bumpCurve.AddKey(HIGHLIGHT_BLINK, 1f);
        }

        //Plane maker
        static GameObject MakePlane(float x, float y)
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

        //Base highlight
        static GameObject baseHighlight;
        static GameObject BaseHighlight
        {
            get
            {
                if(baseHighlight == null)
                {
                    baseHighlight = MakePlane(HIGHLIGHT_SIZE, HIGHLIGHT_SIZE);
                    baseHighlight.GetComponent<MeshRenderer>().material.mainTexture = _texture;
                    baseHighlight.SetActive(false);
                }
                return baseHighlight;
            }
        }

        //Used for materials
        static GameObject _textFrameOriginal;
        static GameObject TextFrameOriginal
        {
            get
            {
                if (_textFrameOriginal == null)
                {
                    _textFrameOriginal = SearchGO("Root", "Default");
                }
                return _textFrameOriginal;
            }
        }

        //Search game object by parent. This is a brutish, slow function and it will return the first value found
        static GameObject SearchGO(string target, string targetParent)
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

        //Cameras
        static Camera _mainCamera;
        static Camera MainCamera
        {
            get
            {
                if(_mainCamera == null)
                {
                    GameObject cameraObject = GameObject.Find("Cameras");
                    if (cameraObject == null) return null;
                    _mainCamera = cameraObject.transform.Find("OutlineCamera").GetComponent<Camera>();
                }
                return _mainCamera;
            }
        }

        static Camera _overlayCamera;
        static Camera OverlayCamera
        {
            get
            {
                if (_overlayCamera == null)
                {
                    GameObject cameraObject = GameObject.Find("Cameras");
                    if (cameraObject == null) return null;
                    _overlayCamera = cameraObject.transform.parent.Find("OverlayCamera").GetComponent<Camera>();
                }
                return _overlayCamera;
            }
        }

        //---------------------------------
        //Creation
        //---------------------------------
        public static bool DisableCreation { get; set; }

        static List<ObjHighlight> highlights = new List<ObjHighlight>();
        static Dictionary<Transform, ObjHighlight> pHighlights = new Dictionary<Transform, ObjHighlight>();

        //With default time
        static public void HighlightObjects(Transform obj)
        {
            HighlightObjects(obj, HIGHLIGHT_LIFETIME);
        }

        static public void HighlightObjects(List<Transform> objs)
        {
            HighlightObjects(objs, HIGHLIGHT_LIFETIME);
        }

        static public void HighlightObjects(List<Transform> objs, bool permanent)
        {
            if (permanent) PHighlightObjects(objs);
            else HighlightObjects(objs, HIGHLIGHT_LIFETIME);
        }

        //Configurable time
        static public void HighlightObjects(Transform obj, float time)
        {
            HighlightObjects(new List<Transform>() { obj }, time);
        }

        static public void HighlightObjects(List<Transform> objs, float time)
        {
            ClearHighlights(false);

            if (objs == null || DisableCreation) return;

            Camera mainCamera = MainCamera;
            if (mainCamera == null) return;

            foreach (Transform transfOutput in objs)
            {
                if(pHighlights.TryGetValue(transfOutput, out ObjHighlight oldHighlight) && oldHighlight != null)
                {
                    oldHighlight.ResetTimer();
                }
                else
                {
                    ObjHighlight newHighLight = ObjHighlight.CreateHighlight(transfOutput, mainCamera, time);
                    highlights.Add(newHighLight);
                }
            }
        }

        //Permanent
        static void PHighlightObjects(List<Transform> objs)
        {
            ClearHighlights(true);
            ClearHighlights(false);

            if (objs == null || DisableCreation) return;

            Camera mainCamera = MainCamera;
            if (mainCamera == null) return;

            foreach (Transform transfOutput in objs)
            {
                ObjHighlight newHighLight = ObjHighlight.CreateHighlight(transfOutput, mainCamera, 500000f);
                pHighlights.Add(transfOutput, newHighLight);
            }
        }

        //Clear highlights
        static public void ClearHighlights(bool permanent)
        {
            if (permanent)
            {
                foreach (KeyValuePair<Transform, ObjHighlight> entry in pHighlights)
                {
                    if (entry.Value != null) Destroy(entry.Value.gameObject);
                }
                pHighlights.Clear();
            }
            else
            {
                foreach (ObjHighlight entry in highlights)
                {
                    if (entry != null) Destroy(entry.gameObject);
                }
                highlights.Clear();
            }
        }

        //Main factory
        static ObjHighlight CreateHighlight(Transform goToFollow, Camera cam, float time)
        {
            GameObject go = new GameObject("CModeHighlighUI");
            ObjHighlight uiElement = (ObjHighlight)go.AddComponent<ObjHighlight>();
            uiElement.Init(goToFollow, cam, time);
            return uiElement;
        }
    }
}