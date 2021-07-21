using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    class CM_MoveSpawned : CMenuTool 
    {
        CM_Mover mover = new CM_Mover();
        Transform instantiatedObject;

        protected override void OnUpdate()
        {
            if (TargetObj == null) return;
            mover.TryRunUpdate();
        }

        protected override void Start()
        {
            mover.Activate(instantiatedObject);
        }

        protected override void Stop()
        {
            mover.Deactivate(false);
        }

        protected override void Cancel()
        {
            mover.Deactivate(true);
            GameObject.Destroy(instantiatedObject.gameObject);
            instantiatedObject = null;
        }

        public Transform MoveItem(Vector3 initialPos, Transform selectedObject)
        {
            instantiatedObject = selectedObject;
            if (instantiatedObject == null) return null;
            instantiatedObject.position = initialPos;
            return instantiatedObject;
        }
    }
}