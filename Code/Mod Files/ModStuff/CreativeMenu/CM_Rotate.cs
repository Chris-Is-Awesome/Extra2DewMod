using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    class CM_Rotate : CMenuTool 
    {
        public enum AxisCtrl { None, TrackBall, X, Y, Z }
        AxisCtrl mainAxis;
        const float ROT_FACTOR = 0.3f;
        Vector3 initialMousePos;

        Vector3 camUpVector;
        Vector3 camRightVector;
        Vector3 camForwardVector;

        protected override void OnUpdate()
        {
            if (TargetObj == null) return;

            if (Input.GetKeyDown(KeyCode.X)) ChangeAxis(AxisCtrl.X);
            else if (Input.GetKeyDown(KeyCode.Y)) ChangeAxis(AxisCtrl.Y);
            else if (Input.GetKeyDown(KeyCode.Z)) ChangeAxis(AxisCtrl.Z);
            else if (Input.GetKeyDown(KeyCode.R)) ChangeAxis(AxisCtrl.TrackBall);
            else if (Input.mouseScrollDelta.y < 0) CycleAxis(-1);
            else if (Input.mouseScrollDelta.y > 0) CycleAxis(1);

            Ray cameraRay = MainCamera.ScreenPointToRay(Input.mousePosition);

            Vector3 mousePosition = GetInitPosToMouseDirVector();
            Vector3 initialPos = GetInitPosDirVector();
            float angle = SignedVectorAngle(mousePosition, initialPos);
            TargetObj.mainTransf.localRotation = TargetObj.initialLocalQuatRot;

            switch (mainAxis)
            {
                case AxisCtrl.None:
                default:
                        TargetObj.mainTransf.Rotate(camForwardVector, -angle, Space.World);
                    break;
                case AxisCtrl.TrackBall:
                    mousePosition -= initialPos;
                    TargetObj.mainTransf.Rotate(camUpVector, mousePosition.x * ROT_FACTOR, Space.World);
                    TargetObj.mainTransf.Rotate(camRightVector, -mousePosition.y * ROT_FACTOR, Space.World);
                    break;
                case AxisCtrl.X:
                    if (Vector3.Dot(camForwardVector, TargetObj.mainTransf.right) > 0) angle = -angle;
                    TargetObj.mainTransf.Rotate(angle, 0f, 0f, Space.Self);
                    break;
                case AxisCtrl.Y:
                    if (Vector3.Dot(camForwardVector, TargetObj.mainTransf.up) > 0) angle = -angle;
                    TargetObj.mainTransf.Rotate(0f, angle, 0f, Space.Self);
                    break;
                case AxisCtrl.Z:
                    if (Vector3.Dot(camForwardVector, TargetObj.mainTransf.forward) > 0) angle = -angle;
                    TargetObj.mainTransf.Rotate(0f, 0f, angle, Space.Self);
                    break;
            }
        }

        protected override void Start()
        {
            if (TargetObj == null)
            {
                active = false;
                return;
            }
            dragMode = false;
            initialMousePos = Input.mousePosition;
            ChangeAxis(AxisCtrl.None);
            camUpVector = MainCamera.transform.up;
            camRightVector = MainCamera.transform.right;
            camForwardVector = MainCamera.transform.forward;

            ObjHighlight.HighlightObjects(TargetObj.mainTransf, 270f);
        }

        protected override void Stop()
        {
            ObjHighlight.ClearHighlights(false);
        }

        protected override void Cancel()
        {
            if (TargetObj == null) return;
            TargetObj.mainTransf.localRotation = TargetObj.initialLocalQuatRot;
        }

        public void ChangeAxis(AxisCtrl axis)
        {
            if (mainAxis == axis) mainAxis = AxisCtrl.None;
            else mainAxis = axis;
        }

        void CycleAxis(int num)
        {
            Array a = Enum.GetValues(typeof(AxisCtrl));
            int newIndex = mod(((int)mainAxis + num), a.Length);
            ChangeAxis((AxisCtrl)a.GetValue((int)newIndex));
        }

        //Math functions
        //---------------------------------------
        Vector3 GetInitPosDirVector()
        {
            Vector3 camPoint;
            if(!dragMode)
            {
                camPoint = initialMousePos;
            }
            else
            {
                Vector3 direction;
                if (mainAxis == AxisCtrl.Y) direction = Vector3.forward;
                else direction = Vector3.up;

                camPoint = Quaternion.Euler(TargetObj.initialGlobalRot) * direction + TargetObj.initialGlobalPos;
                camPoint = MainCamera.WorldToScreenPoint(camPoint);
            }

            Vector3 output = MainCamera.WorldToScreenPoint(TargetObj.initialGlobalPos) - camPoint;
            if (output.sqrMagnitude < 0.001f) output = new Vector3(0f, 1f, 0f);

            return output;
        }

        Vector3 GetInitPosToMouseDirVector()
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 output = MainCamera.WorldToScreenPoint(TargetObj.initialGlobalPos) - mousePos;
            if (output.sqrMagnitude < 0.001f) output = new Vector3(0f, 1f, 0f);

            return output;
        }

        static float SignedVectorAngle(Vector3 referenceVector, Vector3 otherVector)
        {
            Vector2 reference = new Vector2(referenceVector.x, referenceVector.y);
            Vector2 other = new Vector2(otherVector.x, otherVector.y);

            reference.Normalize();
            other.Normalize();

            return SignedAngle(reference, other);
        }

        public static float Angle(Vector2 from, Vector2 to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = Mathf.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
            if (denominator < 1E-15F)
                return 0F;

            float dot = Mathf.Clamp(Vector2.Dot(from, to) / denominator, -1F, 1F);
            return Mathf.Acos(dot) * Mathf.Rad2Deg;
        }

        // Returns the signed angle in degrees between /from/ and /to/. Always returns the smallest possible angle
        public static float SignedAngle(Vector2 from, Vector2 to)
        {
            float unsigned_angle = Angle(from, to);
            float sign = Mathf.Sign(from.x * to.y - from.y * to.x);
            return unsigned_angle * sign;
        }

        int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}
