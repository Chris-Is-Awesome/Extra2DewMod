using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class ObjFinder : MonoBehaviour
    {
        const string ARMATURE_NAME = "Armature";
        const float DIST_SCALE = 0.001f;

        public enum SelecMode { Entities, EntitiesGfx, Meshes, Armatures, GameObjects }

        public delegate bool ObjFilter(Transform transf);
        ObjFilter _filter;
        ObjFilter[] _modeFilter;

        Transform camContainer;
        Transform sysRoot;
        Camera mainCamera;
        
        void Awake()
        {
            camContainer = GameObject.Find("Cameras").transform.parent;
            sysRoot = GameObject.Find("SystemRoot").transform;
            mainCamera = camContainer.transform.Find("Cameras").Find("OutlineCamera").GetComponent<Camera>();
            _modeFilter = new ObjFilter[]
            {
                new ObjFilter(ModePassEntities),
                new ObjFilter(ModePassEntityGraphics),
                new ObjFilter(ModePass3DModels),
                new ObjFilter(ModePassArmatures),
                new ObjFilter(ModePassAll)
            };
            SetFilter(null);
        }

        //Filters
        bool ModePassEntities(Transform transf)
        {
            return transf.gameObject.GetComponent<Entity>() == null;
        }

        bool ModePassEntityGraphics(Transform transf)
        {
            return transf.gameObject.GetComponent<EntityGraphics>() == null;
        }
        
        bool ModePass3DModels(Transform transf)
        {
            //return transf.gameObject.GetComponent<MeshRenderer>() == null && transf.gameObject.GetComponent<SkinnedMeshRenderer>() == null;
            return transf.gameObject.GetComponent<Renderer>() == null;
        }

        bool ModePassArmatures(Transform transf)
        {
            return TransformUtility.FindInParents<Animation>(transf) == null;
        }

        bool ModePassAll(Transform transf)
        {
            return false;
        }
        
        public void SetFilter(ObjFilter filter)
        {
            if(filter == null)
            {
                _filter = delegate (Transform transf)
                {
                    return false;
                };
            }
            else
            {
                _filter = filter;
            }
        }

        //Search methods
        IEnumerable<Transform> FindSelectableObjects(SelecMode mode)
        {
            Transform[] objectsInScene = FindObjectsOfType<Transform>();
            ObjFilter modeFilter = _modeFilter[(int)mode];

            for (int i = 0; i < objectsInScene.Length; i++)
            {
                Transform transf = objectsInScene[i];
                if (_filter(transf)) continue; //Name filter
                if (modeFilter(transf)) continue; //Selection mode filter
                if (transf.IsChildOf(camContainer) || transf.IsChildOf(sysRoot)) continue; //Child of the camera or sysroot, discard
                if (Vector3.Dot(mainCamera.transform.forward, transf.position - mainCamera.transform.position) <= 0) continue; //Behind the camera, discard
                yield return transf;
            }
        }

        public List<Transform> ObjectsOnScreen(SelecMode mode)
        {
            List<Transform> output = new List<Transform>();

            foreach (Transform transf in FindSelectableObjects(mode))
            {
                Vector3 screenPosition = mainCamera.WorldToViewportPoint(transf.position);
                //If the object is on the screen, add to list
                if (screenPosition.x >= 0f && screenPosition.x <= Screen.width && screenPosition.y >= 0f && screenPosition.y <= Screen.height) output.Add(transf);
            }

            return output;
        }
        
        public List<Transform> MouseScan(SelecMode mode)
        {
            List<Transform> output = new List<Transform>();
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            float minDistance = float.PositiveInfinity;

            foreach (Transform transf in FindSelectableObjects(mode))
            {
                float distance = Vector3.Cross(ray.direction, transf.position - ray.origin).sqrMagnitude;
                float distanceFromCam = (mainCamera.transform.position - transf.position).sqrMagnitude;
                if (distance < distanceFromCam * DIST_SCALE)
                {
                    if (distanceFromCam < minDistance)
                    {
                        output.Insert(0, transf);
                        minDistance = distanceFromCam;
                    }
                    else
                    {
                        output.Add(transf);
                    }
                }
            }

            return output;
        }
    }
}
