using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public abstract class CMenuTool
    {
        protected bool active;
        protected bool dragMode;
        public bool Active { get { return active; } }

        static Camera _mainCamera;
        static protected Camera MainCamera
        {
            get
            {
                if(_mainCamera == null)
                {
                    GameObject cameraGo = GameObject.Find("OutlineCamera");
                    if (cameraGo == null) return null;
                    _mainCamera = cameraGo.GetComponent<Camera>();
                }
                return _mainCamera;
            }
        }

        protected ObjProperties TargetObj { get; private set; }

        public void Activate(Transform transf)
        {
            active = true;
            if (transf == null) TargetObj = null;
            else TargetObj = new ObjProperties(transf);
            Start();
        }

        public void Deactivate(bool cancel)
        {
            active = false;

            if (cancel) Cancel();
            Stop();

            TargetObj = null;
        }

        public void TryRunUpdate()
        {
            if (active) OnUpdate();
        }

        protected virtual void OnUpdate()
        {

        }

        protected virtual void Start()
        {

        }

        protected virtual void Stop()
        {

        }

        protected virtual void Cancel()
        {

        }

        public virtual void ActivateDragMode()
        {
            dragMode = true;
        }

        public class ObjProperties
        {
            public Transform mainTransf;

            public Vector3 initialGlobalPos;
            public Vector3 initialLocalPos;

            public Vector3 initialLocalRot;
            public Vector3 initialGlobalRot;
            public Quaternion initialLocalQuatRot;

            public Vector3 initialLocalscale;

            public Dictionary<Transform, Vector3> initialChildrenScale;

            public ObjProperties(Transform transf)
            {
                if (transf == null) return;
                mainTransf = transf;

                initialGlobalPos = transf.position;
                initialLocalPos = transf.localPosition;

                initialLocalRot = transf.localEulerAngles;
                initialGlobalRot = transf.eulerAngles;
                initialLocalQuatRot = transf.localRotation;

                initialLocalscale = transf.localScale;

                initialChildrenScale = new Dictionary<Transform, Vector3>();
                for(int i = 0; i < transf.childCount; i++)
                {
                    Transform child = transf.GetChild(i);
                    initialChildrenScale.Add(child, child.localScale);
                }
            }
        }
    }
}
