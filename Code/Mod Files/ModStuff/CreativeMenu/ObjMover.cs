using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class ObjMover : MonoBehaviour
    {
        Transform targetObj;
        Camera mainCamera;
        AxisCtrl mainAxis;

        //Scale
        Vector3 initialScale;

        //Movement
        Vector3 initialPos;
        bool moveObj;
        Vector4 movePlaneData;
        Ray moveRay;

        //Rotation
        bool rotObj;
        Vector3 initialRot;
        Vector3 initialRotGlobal;
        Quaternion initRotQuaternion;
        Vector3 rotPlaneNormal;

        //Scale
        bool scaleObj;

        void Awake()
        {
            mainCamera = GameObject.Find("OutlineCamera").GetComponent<Camera>();
        }

        public void StartManipulation(Transform obj)
        {
            targetObj = obj;

            //Default x/z movement
            initialPos = obj.position;
            Vector3 planeNormal = Vector3.up;
            float num = Vector3.Dot(planeNormal, initialPos);
            movePlaneData = new Vector4(planeNormal.x, -planeNormal.y, planeNormal.z, num);

            //Default rot
            initialRot = obj.localEulerAngles;
            initialRotGlobal = obj.eulerAngles;
            initRotQuaternion = targetObj.localRotation;
            ChangeAxis(AxisCtrl.None);

            //Default scale
            initialScale = obj.localScale;

            //Set initial state
            moveObj = true;
            rotObj = false;
            scaleObj = false;
        }

        public void UseMove(bool reset)
        {
            if (targetObj == null) return;
            if(reset)
            {
                targetObj.localEulerAngles = initialRot;
                targetObj.localScale = initialScale;
            }
            ChangeAxis(AxisCtrl.None);
            moveObj = true;
            rotObj = false;
            scaleObj = false;
        }

        public void UseRot(bool reset)
        {
            if (targetObj == null) return;
            if (reset)
            {
                targetObj.position = initialPos;
                targetObj.localScale = initialScale;
            }
            ChangeAxis(AxisCtrl.Y);
            moveObj = false;
            rotObj = true;
            scaleObj = false;
        }

        public void UseScale(bool reset)
        {
            if (targetObj == null) return;
            if (reset)
            {
                targetObj.position = initialPos;
                targetObj.localEulerAngles = initialRot;
            }
            ChangeAxis(AxisCtrl.None);
            moveObj = false;
            rotObj = false;
            scaleObj = true;
        }

        public void ChangeAxis(AxisCtrl axis)
        {
            if (targetObj == null) return;
            if (mainAxis == axis) mainAxis = AxisCtrl.None;
            else mainAxis = axis;
            switch(axis)
            {
                case AxisCtrl.X:
                    rotPlaneNormal = Vector3.left;
                    break;
                case AxisCtrl.Y:
                    rotPlaneNormal = Vector3.up;
                    break;
                case AxisCtrl.Z:
                    rotPlaneNormal = Vector3.forward;
                    break;
                case AxisCtrl.None:
                default:
                    rotPlaneNormal = Vector3.forward;
                    break;
            }
            moveRay = new Ray(initialPos, rotPlaneNormal);
        }

        public void CycleAxis()
        {
            if (mainAxis == AxisCtrl.Z) mainAxis = AxisCtrl.None;
            else mainAxis = mainAxis + 1;
        }

        public AxisCtrl CurrentAxis { get { return mainAxis; } }

        public void StopManipulation()
        {
            targetObj = null;
            moveObj = false;
            rotObj = false;
            scaleObj = false;
        }

        public void CancelManipulation()
        {
            if (targetObj == null) return;
            mainAxis = AxisCtrl.None;
            targetObj.position = initialPos;
            targetObj.localEulerAngles = initialRot;
            targetObj.localScale = initialScale;
            StopManipulation();
        }

        void OnDisable()
        {
            CancelManipulation();
        }
 
        void Update()
        {
            if (targetObj == null) return;
            Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (moveObj)
            {
                Vector3 newPos = initialPos;
                if (mainAxis == AxisCtrl.None)
                {
                    if (PlaneRayIntersection(cameraRay, movePlaneData, out Vector3 intersection))
                    {
                        newPos = intersection;
                    }
                }
                else
                {
                    if(ClosestPointToAxis(out Vector3 closestPoint, moveRay, cameraRay))
                    {
                        switch (mainAxis)
                        {
                            case AxisCtrl.X:
                                newPos = new Vector3(closestPoint.x, initialPos.y, initialPos.z);
                                break;
                            case AxisCtrl.Y:
                                newPos = new Vector3(initialPos.x, closestPoint.y, initialPos.z);
                                break;
                            case AxisCtrl.Z:
                                newPos = new Vector3(initialPos.x, initialPos.y, closestPoint.z);
                                break;
                            default:
                                newPos = new Vector3(initialPos.x, initialPos.y, initialPos.z);
                                break;
                        }
                    }
                }
                targetObj.position = newPos;
            }
            if(rotObj)
            {
                float angle = SignedVectorAngle(GetInitPosToMouseDirVector(), GetInitPosDirVector());
                targetObj.localRotation = initRotQuaternion;
                switch (mainAxis)
                {
                    case AxisCtrl.X:
                        targetObj.Rotate(angle, 0f, 0f, Space.Self);
                        break;
                    case AxisCtrl.None:
                    case AxisCtrl.Y:
                    default:
                        targetObj.Rotate(0f, angle, 0f, Space.Self);
                        break;
                    case AxisCtrl.Z:
                        targetObj.Rotate(0f, 0f, angle, Space.Self);
                        break;
                }
            }
            if(scaleObj)
            {
                
                float xPos = (Input.mousePosition - mainCamera.WorldToScreenPoint(targetObj.position)).x / (Screen.width/2f);
                float scale;
                if (xPos > 0) scale = (SCALE_FACTOR - 1f) * xPos + 1f;
                else scale = 1f / (1 + ((SCALE_FACTOR - 1f) * (-xPos)));
                switch (mainAxis)
                {
                    case AxisCtrl.None:
                    default:
                        targetObj.localScale = new Vector3(initialScale.x * scale, initialScale.y * scale, initialScale.z * scale);
                        break;
                    case AxisCtrl.X:
                        targetObj.localScale = new Vector3(initialScale.x * scale, initialScale.y, initialScale.z);
                        break;
                    case AxisCtrl.Y:
                        targetObj.localScale = new Vector3(initialScale.x, initialScale.y * scale, initialScale.z);
                        break;
                    case AxisCtrl.Z:
                        targetObj.localScale = new Vector3(initialScale.x, initialScale.y, initialScale.z * scale);
                        break;
                }
            }

        }

        //Constants and definitions
        //----------------------------------------------
        public enum AxisCtrl { None, X, Y, Z }
        const float SCALE_FACTOR = 8f;

        //Math functions
        //----------------------------------------------
        Vector3 GetInitPosDirVector()
        {
            Vector3 camPoint = Quaternion.Euler(initialRotGlobal) * Vector3.forward;
            camPoint = camPoint + initialPos;
            camPoint = mainCamera.WorldToScreenPoint(camPoint);

            Vector3 output = mainCamera.WorldToScreenPoint(initialPos) - camPoint;
            if (output.sqrMagnitude < 0.001f) output = new Vector3(0f, 1f, 0f);

            return output;
        }

        Vector3 GetInitPosToMouseDirVector()
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 output = mainCamera.WorldToScreenPoint(initialPos) - mousePos;
            if (output.sqrMagnitude < 0.001f) output = new Vector3(0f, 1f, 0f);

            return output;
        }

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
    }
}
