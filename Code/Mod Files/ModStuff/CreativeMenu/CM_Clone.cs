using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.CreativeMenu
{
    class CM_Clone : CMenuTool 
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
            if (TargetObj == null)
            {
                active = false;
                return;
            }
            if(TargetObj.mainTransf.parent != null)
            {
                instantiatedObject = GameObject.Instantiate(TargetObj.mainTransf.gameObject, TargetObj.mainTransf.parent, true).transform;
                instantiatedObject.SetParent(null, true);
            }
            else
            {
                instantiatedObject = GameObject.Instantiate(TargetObj.mainTransf.gameObject, null, true).transform;
            }
            //instantiatedObject = GameObject.Instantiate(TargetObj.mainTransf.gameObject, TargetObj.initialGlobalPos, TargetObj.mainTransf.rotation).transform;
            instantiatedObject.name = TargetObj.mainTransf.name;
            mover.Activate(instantiatedObject);
        }

        protected override void Stop()
        {
            mover.Deactivate(false);
        }

        protected override void Cancel()
        {
            if (TargetObj == null) return;
            mover.Deactivate(true);
            GameObject.Destroy(instantiatedObject.gameObject);
        }

        public Transform GetClone()
        {
            return instantiatedObject;
        }
    }
}
