using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    class CM_Scale : CMenuTool 
    {
        public enum AxisCtrl { None, X, Y, Z }
        AxisCtrl mainAxis;
        public bool poseMode;
        float hotkeyFactor;

        const float SCALE_FACTOR = 8f;

        protected override void OnUpdate()
        {
            if (TargetObj == null) return;

            if (Input.GetKeyDown(KeyCode.X)) ChangeAxis(AxisCtrl.X);
            else if (Input.GetKeyDown(KeyCode.Y)) ChangeAxis(AxisCtrl.Y);
            else if (Input.GetKeyDown(KeyCode.Z)) ChangeAxis(AxisCtrl.Z);
            else if (Input.mouseScrollDelta.y < 0) CycleAxis(-1);
            else if (Input.mouseScrollDelta.y > 0) CycleAxis(1);

            if (Input.GetKeyDown(KeyCode.S)) poseMode = !poseMode;

            Vector3 distToObj = Input.mousePosition - MainCamera.WorldToScreenPoint(TargetObj.mainTransf.position);
            float scale;

            if (dragMode)
            {
                float xPos = distToObj.x / (Screen.width / 2f);
                if (xPos > 0) scale = (SCALE_FACTOR - 1f) * xPos + 1f;
                else scale = 1f / (1 + ((SCALE_FACTOR - 1f) * (-xPos)));
            }
            else
            {
                scale = 0.01f + distToObj.sqrMagnitude * hotkeyFactor;
            }

            Vector3 scaleVector;

            if (scale == 0f) { scale = 1f; }

            switch (mainAxis)
            {
                case AxisCtrl.None:
                default:
                    scaleVector = Vector3.one * scale;
                    break;
                case AxisCtrl.X:
                    scaleVector = new Vector3(scale, 1f, 1f);
                    break;
                case AxisCtrl.Y:
                    scaleVector = new Vector3(1f, scale, 1f);
                    break;
                case AxisCtrl.Z:
                    scaleVector = new Vector3(1f, 1f, scale);
                    break;
            }
            TargetObj.mainTransf.localScale = new Vector3(TargetObj.initialLocalscale.x * scaleVector.x, TargetObj.initialLocalscale.y * scaleVector.y, TargetObj.initialLocalscale.z * scaleVector.z);

            if (poseMode)
            {
                foreach (KeyValuePair<Transform, Vector3> entry in TargetObj.initialChildrenScale)
                {
                    entry.Key.localScale = new Vector3(entry.Value.x / scaleVector.x, entry.Value.y / scaleVector.y, entry.Value.z / scaleVector.z);
                }
            }
        }

        void CycleAxis(int num)
        {
            Array a = Enum.GetValues(typeof(AxisCtrl));
            int newIndex = mod(((int)mainAxis + num), a.Length);
            ChangeAxis((AxisCtrl)a.GetValue((int)newIndex));
        }

        protected override void Start()
        {
            if (TargetObj == null)
            {
                active = false;
                return;
            }
            float distToObj = (Input.mousePosition - MainCamera.WorldToScreenPoint(TargetObj.mainTransf.position)).sqrMagnitude;
            if(distToObj == 0f)
            {
                active = false;
                return;
            }
            dragMode = false;
            poseMode = false;
            hotkeyFactor = 0.99f / distToObj;
            ChangeAxis(AxisCtrl.None);

            ObjHighlight.HighlightObjects(TargetObj.mainTransf, 270f);
        }

        protected override void Stop()
        {
            ObjHighlight.ClearHighlights(false);
        }

        protected override void Cancel()
        {
            if (TargetObj == null) return;
            TargetObj.mainTransf.localScale = TargetObj.initialLocalscale;
            foreach (KeyValuePair<Transform, Vector3> entry in TargetObj.initialChildrenScale)
            {
                entry.Key.localScale = entry.Value;
            }
        }

        public void ChangeAxis(AxisCtrl axis)
        {
            if (mainAxis == axis) mainAxis = AxisCtrl.None;
            else mainAxis = axis;
        }

        int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}
