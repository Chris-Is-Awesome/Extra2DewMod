using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class CMenuKeyListener : MonoBehaviour
    {
        //Definitions
        const float LONG_PRESS_TIME = 0.2f;
        public delegate void OnKeyPress();
        public delegate void OnTimedKeyPress(bool longTime);

        //Axis
        public event OnKeyPress onX;
        public event OnKeyPress onY;
        public event OnKeyPress onZ;

        //Main tools
        public event OnKeyPress onG; //Move
        public event OnKeyPress onR; //Rotate
        public event OnKeyPress onS; //Scale
        public event OnKeyPress onQ; //Save
        public event OnKeyPress onE; //Load
        public event OnKeyPress onH; //ChangeP
        public event OnKeyPress onC; //Clone
        public event OnKeyPress onT; //Testing
        public event OnKeyPress onU; //Testing
        public event OnKeyPress onJ; //Testing
        public event OnKeyPress onSpaceBar; //Move?

        //Clicks
        public event OnKeyPress onLeftClick;
        public event OnKeyPress onLeftClickRelease;
        public event OnTimedKeyPress onTLeftClickRelease;
        float leftClickTime;
        bool leftClickPressed;
        public event OnKeyPress onRightClick;
        public event OnKeyPress onRightClickRelease;
        public event OnTimedKeyPress onTRightClickRelease;
        float rightClickTime;
        bool rightClickPressed;

        //Scroll
        public event OnKeyPress onScrollWheelUp;
        public event OnKeyPress onScrollWheelDown;

        //Cancel
        public event OnKeyPress onEscape;

        void Update()
        {
            if (leftClickPressed) leftClickTime += Time.deltaTime;
            if (rightClickPressed) rightClickTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.U)) onU?.Invoke();
            if (Input.GetKeyDown(KeyCode.J)) onJ?.Invoke();

            if (Input.GetKeyDown(KeyCode.X)) onX?.Invoke();
            if (Input.GetKeyDown(KeyCode.Y)) onY?.Invoke();
            if (Input.GetKeyDown(KeyCode.Z)) onZ?.Invoke();

            if (Input.GetKeyDown(KeyCode.G)) onG?.Invoke();
            if (Input.GetKeyDown(KeyCode.R)) onR?.Invoke();
            if (Input.GetKeyDown(KeyCode.S)) onS?.Invoke();

            if (Input.GetKeyDown(KeyCode.Q)) onQ?.Invoke();
            if (Input.GetKeyDown(KeyCode.E)) onE?.Invoke();
            if (Input.GetKeyDown(KeyCode.H)) onH?.Invoke();
            if (Input.GetKeyDown(KeyCode.C)) onC?.Invoke();
            if (Input.GetKeyDown(KeyCode.Space)) onSpaceBar?.Invoke();
            if (Input.GetKeyDown(KeyCode.T)) onT?.Invoke();

            if (Input.GetKeyDown(KeyCode.Mouse0)) onLeftClick?.Invoke();
            if (!Input.GetKey(KeyCode.Mouse0))
            {
                if (leftClickPressed)
                {
                    onLeftClickRelease?.Invoke();
                    onTLeftClickRelease?.Invoke(leftClickTime >= LONG_PRESS_TIME);
                    leftClickTime = 0f;
                }
            }
            leftClickPressed = Input.GetKey(KeyCode.Mouse0);

            if (Input.GetKeyDown(KeyCode.Mouse1)) onRightClick?.Invoke();
            if (!Input.GetKey(KeyCode.Mouse1))
            {
                if (rightClickPressed)
                {
                    onRightClickRelease?.Invoke();
                    onTRightClickRelease?.Invoke(rightClickTime >= LONG_PRESS_TIME);
                    rightClickTime = 0f;
                }
            }
            rightClickPressed = Input.GetKey(KeyCode.Mouse1);

            if (Input.mouseScrollDelta.y < 0) onScrollWheelDown?.Invoke();
            if (Input.mouseScrollDelta.y > 0) onScrollWheelUp?.Invoke();

            if (Input.GetKeyDown(KeyCode.Escape)) onEscape?.Invoke();
        }

        void OnDisable()
        {
            leftClickPressed = false;
            rightClickPressed = false;
        }
    }
}
