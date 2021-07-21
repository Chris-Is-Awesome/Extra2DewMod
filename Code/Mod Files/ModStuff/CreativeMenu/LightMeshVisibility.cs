using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    public class LightMeshVisibility : MonoBehaviour
    {
        public static bool MeshVisibility = true;
        MeshRenderer _renderer;
        
        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        void Update()
        {
            if (MeshVisibility != _renderer.enabled)
            {
                _renderer.enabled = MeshVisibility;
            }
        }
    }
}
