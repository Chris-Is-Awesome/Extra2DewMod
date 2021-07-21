using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    class CM_Mover : CMenuTool 
    {
        public enum AxisCtrl { None, CameraPlane, X, Y, Z }
        AxisCtrl mainAxis;

        Vector4 movePlaneData;
        Vector4 cameraPlaneData;
        Ray moveRay;

        protected override void OnUpdate()
        {
            if (TargetObj == null) return;

            if (Input.GetKeyDown(KeyCode.X)) ChangeAxis(AxisCtrl.X);
            else if (Input.GetKeyDown(KeyCode.Y)) ChangeAxis(AxisCtrl.Y);
            else if (Input.GetKeyDown(KeyCode.Z)) ChangeAxis(AxisCtrl.Z);
            else if (Input.GetKeyDown(KeyCode.G)) ChangeAxis(AxisCtrl.CameraPlane);
            else if (Input.mouseScrollDelta.y < 0) CycleAxis(-1);
            else if (Input.mouseScrollDelta.y > 0) CycleAxis(1);

            Ray cameraRay = MainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 newPos = TargetObj.initialGlobalPos;

            if (mainAxis == AxisCtrl.None)
            {
                if (PlaneRayIntersection(cameraRay, cameraPlaneData, out Vector3 intersection))
                {
                    newPos = intersection;
                }
            }
            else if (mainAxis == AxisCtrl.CameraPlane)
            {
                if (PlaneRayIntersection(cameraRay, movePlaneData, out Vector3 intersection))
                {
                    newPos = intersection;
                }
            }
            else
            {
                if (ClosestPointToAxis(out Vector3 closestPoint, moveRay, cameraRay))
                {
                    switch (mainAxis)
                    {
                        default:
                        case AxisCtrl.X:
                            newPos = new Vector3(closestPoint.x, TargetObj.initialGlobalPos.y, TargetObj.initialGlobalPos.z);
                            break;
                        case AxisCtrl.Y:
                            newPos = new Vector3(TargetObj.initialGlobalPos.x, closestPoint.y, TargetObj.initialGlobalPos.z);
                            break;
                        case AxisCtrl.Z:
                            newPos = new Vector3(TargetObj.initialGlobalPos.x, TargetObj.initialGlobalPos.y, closestPoint.z);
                            break;
                    }
                }
            }

            TargetObj.mainTransf.position = newPos;
        }

        protected override void Start()
        {
            if (TargetObj == null)
            {
                active = false;
                return;
            }

            Vector3 planeNormal = MainCamera.transform.forward;
            float num = Vector3.Dot(planeNormal, TargetObj.initialGlobalPos);
            movePlaneData = new Vector4(-planeNormal.x, -planeNormal.y, -planeNormal.z, num);

            planeNormal = Vector3.up;
            num = Vector3.Dot(planeNormal, TargetObj.initialGlobalPos);
            cameraPlaneData = new Vector4(planeNormal.x, -planeNormal.y, planeNormal.z, num);

            ObjHighlight.HighlightObjects(TargetObj.mainTransf, 270f);
            mainAxis = AxisCtrl.None;
        }

        protected override void Stop()
        {
            ObjHighlight.ClearHighlights(false);
        }

        protected override void Cancel()
        {
            if (TargetObj == null) return;
            TargetObj.mainTransf.position = TargetObj.initialGlobalPos;
        }

        public void ChangeAxis(AxisCtrl axis)
        {
            if (mainAxis == axis)
            {
                mainAxis = AxisCtrl.None;
                return;
            }

            mainAxis = axis;
            switch (axis)
            {
                case AxisCtrl.X:
                    moveRay = new Ray(TargetObj.initialGlobalPos, Vector3.left);
                    break;
                case AxisCtrl.Y:
                    moveRay = new Ray(TargetObj.initialGlobalPos, Vector3.up);
                    break;
                case AxisCtrl.Z:
                    moveRay = new Ray(TargetObj.initialGlobalPos, Vector3.forward);
                    break;
                default:
                    break;
            }
        }

        void CycleAxis(int num)
        {
            Array a = Enum.GetValues(typeof(AxisCtrl));
            int newIndex = mod(((int)mainAxis + num), a.Length);
            ChangeAxis((AxisCtrl)a.GetValue((int)newIndex));
        }

        //Math functions
        //---------------------------------------
        static bool ClosestPointToAxis(out Vector3 closestPoint, Ray axis, Ray camera)
        {
            return ClosestPointsOnTwoLines(out closestPoint, out Vector3 closestPointLine2, axis.origin, axis.direction, camera.origin, camera.direction);
        }

        //by http://wiki.unity3d.com/index.php/3d_Math_functions
        //Two non-parallel lines which may or may not touch each other have a point on each line which are closest
        //to each other. This function finds those two points. If the lines are not parallel, the function 
        //outputs true, otherwise false.
        static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {

            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;

            float a = Vector3.Dot(lineVec1, lineVec1);
            float b = Vector3.Dot(lineVec1, lineVec2);
            float e = Vector3.Dot(lineVec2, lineVec2);

            float d = a * e - b * b;

            //lines are not parallel
            if (d != 0.0f)
            {

                Vector3 r = linePoint1 - linePoint2;
                float c = Vector3.Dot(lineVec1, r);
                float f = Vector3.Dot(lineVec2, r);

                float s = (b * f - c * e) / d;
                float t = (a * f - c * b) / d;

                closestPointLine1 = linePoint1 + lineVec1 * s;
                closestPointLine2 = linePoint2 + lineVec2 * t;

                return true;
            }

            else
            {
                return false;
            }
        }

        static bool PlaneRayIntersection(Ray ray, Vector4 planeData, out Vector3 intersection)
        {
            intersection = Vector3.zero;
            float dotDir = PlaneDotDir(planeData, ray.direction);
            if (dotDir == 0) return false;
            float dotPos = PlaneDotPos(planeData, ray.origin);

            float num = dotPos / dotDir;
            if (num >= 0) return false;
            intersection = ray.origin - num * ray.direction;
            return true;
        }
        
        static float PlaneDotPos(Vector4 plane, Vector3 P)
        {
            return plane.x * P.x + plane.y * P.y + plane.z * P.z + plane.w;
        }

        static float PlaneDotDir(Vector4 plane, Vector3 N)
        {
            return plane.x * N.x + plane.y * N.y + plane.z * N.z;
        }

        int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
    }
}
