using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public abstract class CMenuWindow 
    {
        protected GameObject menuGo;
        protected CMenuLogic logic;
        protected Vector2 upperLeftLimit = Vector2.zero;
        protected Vector2 lowerRightLimit = Vector2.zero;

        static public T CreateMenu<T>(CMenuLogic.MenuList menuIndex, CMenuLogic cMenuLogic, Transform parent) where T : CMenuWindow, new()
        {
            //Create and setup GameObject
            GameObject menuGo = new GameObject();
            menuGo.transform.localPosition = Vector3.zero;
            menuGo.transform.localScale = Vector3.one;
            menuGo.transform.SetParent(parent, false);
            menuGo.name = menuIndex.ToString();
            menuGo.SetActive(false);

            //Create and setup CMenuWindow component
            T menu = new T();
            menu.logic = cMenuLogic;
            menu.menuGo = menuGo;
            menu.BuildMenu();

            //Add to CMenuArray
            cMenuLogic.AddMenu(menuIndex, menu);

            return menu;
        }

        public void Open()
        {
            menuGo.SetActive(true);
            logic.SetMenuLimits(upperLeftLimit, lowerRightLimit);
            OnOpen();
        }

        virtual public void OnOpen()
        {

        }

        public void Close()
        {
            menuGo.SetActive(false);
            OnClose();
        }

        virtual public void OnClose()
        {

        }

        virtual public void BuildMenu()
        {

        }
    }
}
