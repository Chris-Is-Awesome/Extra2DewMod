using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff
{
    public class E2DGameMode : MonoBehaviour
    {
        public ModeControllerNew.ModeType Mode { get; protected set; }
        public bool IsActive { get; protected set; }

        public string Title { get; protected set; }
        public string QuickDescription { get; protected set; }
        public string Description { get; protected set; }

        public List<string> FileNames { get; protected set; }
        public List<ModeControllerNew.ModeType> Restrictions { get; protected set; }

        public GameObject MenuGo { get; protected set; }

        //Checks if the name string belongs to the mode
        public bool IsMode(string name)
        {
            for (int i = 0; i < FileNames.Count; i++)
            {
                if (FileNames[i] == name) return true;
            }
            return false;
        }

        //Check compatibility between modes
        public bool CheckCompatibility(E2DGameMode mode)
        {
            for (int i = 0; i < Restrictions.Count; i++)
            {
                if (Restrictions[i] == mode.Mode) { return false; }
            }

            return true;
        }

        //Setup a new game file
        virtual public void SetupSaveFile(RealDataSaver saver)
        {
        }

        //Activates the game mode 
        virtual public void Activate()
        {
        }

        //Deactivates the game mode
        virtual public void Deactivate()
        {
        }

        //UI game mode selection
        virtual public void CreateOptions()
        {
            MenuGo = new GameObject();
        }

        //On open menu
        virtual public void OnOpenMenu()
        {

        }
    }
}
