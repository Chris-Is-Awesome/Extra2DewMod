using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

//Modified Scripts:
//PlayerSpawner - Handles graphics change
//Attack - Hides player weapon
//Composite attack? Chargeing attack? - Hides EFCS ////  INVESTIGATE WEAPON GRAPHICS, MIGHT DO ALL IN ONE SCRIPT

namespace ModStuff
{
    public class ModelSwapper : Singleton<ModelSwapper>
    {
        private GameObject swapModelGfx;
        private Animation originalAnims;
        public bool hideWeapons;

        //Model functions dictionary
        Dictionary<string, ModelFunc> allModels;

        //Delegate functions
        private delegate bool ModelFunc();
        private delegate void AnimSpeedUpdate(EntityAnimator player);
        AnimSpeedUpdate ModelSpeedChange;

        void Awake()
        {
            //Commands dictionary
            allModels = new Dictionary<string, ModelFunc>
            {
                {"safetyjenny", new ModelFunc(SafetyJenny)},
                {"cyberjenny", new ModelFunc(CyberJenny)},
                //{"fishbun", new ModelFunc(Fishbun)},
                //{"jennycat", new ModelFunc(JennyCat)},
                {"reset", new ModelFunc(Reset)}
            };
        }

        public bool GetModel(GameObject playerent, out GameObject model)
        {
            model = swapModelGfx;
            if (ModelSpeedChange != null)
            {
                ModelSpeedChange(playerent.GetComponent<EntityAnimator>());
            }
            return swapModelGfx != null;
        }

        public bool SetModel(string model)
        {
            ModelFunc modelFunc;
            if (originalAnims == null)
            {
                GameObject originalModel = Instantiate(GameObject.Find("Ittle").transform.Find("ittle").gameObject);
                originalModel.name = "SwapModelOriginal";
                DontDestroyOnLoad(originalModel);
                originalAnims = originalModel.GetComponent<Animation>();
                //originalModel.SetActive(false);
            }
            if (allModels.TryGetValue(model, out modelFunc))
            {
                return modelFunc();
            }
            return false;
        }

        private bool SafetyJenny()
        {
            GameObject safety1 = FindEntity("safetyjenny");
            GameObject safety2 = FindEntity("safetyjenny2");
            if (safety1 == null || safety2 == null)
            {
                return false;
            }

            GameObject gfx;
            if (safety1.GetComponent<EntityGraphics>() != null)
            { gfx = safety1.GetComponent<EntityGraphics>().gameObject; }
            else
            { gfx = safety1.GetComponentInChildren<EntityGraphics>().gameObject; }

            if (swapModelGfx != null) { Destroy(swapModelGfx); }
            swapModelGfx = Instantiate(gfx);

            Animation safety2anims = safety2.GetComponentInChildren<EntityGraphics>().gameObject.GetComponent<Animation>();
            Animation newAnims = swapModelGfx.GetComponent<Animation>();
            newAnims.AddClip(newAnims.GetClip("scaredattack"), "sword");
            newAnims.AddClip(newAnims.GetClip("scaredattack"), "forcewand");
            newAnims.AddClip(safety2anims.GetClip("spinattack"), "roll");
            newAnims.AddClip(safety2anims.GetClip("spinattack"), "rollstart");
            newAnims.AddClip(safety2anims.GetClip("spinattack"), "rollloop");
            newAnims.AddClip(safety2anims.GetClip("spinattack"), "rollend");
            newAnims.AddClip(newAnims.GetClip("scaredattack"), "chargeefcs");
            newAnims.AddClip(newAnims.GetClip("scaredattack"), "efcs");
            newAnims.AddClip(newAnims.GetClip("walk"), "pushing");
            newAnims.AddClip(newAnims.GetClip("explode"), "warp");
            newAnims.AddClip(newAnims.GetClip("idle"), "strongidle");
            swapModelGfx.name = "Ittle";
            //swapModelGfx.name = "Itol";

            hideWeapons = true;
            //ModelSpeedChange = SafetyJennyAnimationSpeed;

            DontDestroyOnLoad(swapModelGfx);
            //swapModelGfx.SetActive(false);

            return true;
        }

        /*private void SafetyJennyAnimationSpeed(EntityAnimator player)
        {
            SetAnimationSpeed(player, "roll", 0.0010f);
            SetAnimationSpeed(player, "sword", 0.000021f);
        }
        
        void SetAnimationSpeed(EntityAnimator animator, string animation_name, float speed)
        {
            for (int i = 0; i < animator._anims.Length; i++)
            {
                if (animator._anims[i].AnimName == animation_name) { animator._anims[i].SetSpeed(speed); }
            }
        }*/

        private bool CyberJenny()
        {
            GameObject targetNPC = FindEntity("cyberjennya_ent");
            if (targetNPC == null)
            {
                return false;
            }

            GameObject gfx;
            if (targetNPC.GetComponent<EntityGraphics>() != null)
            { gfx = targetNPC.GetComponent<EntityGraphics>().gameObject; }
            else
            { gfx = targetNPC.GetComponentInChildren<EntityGraphics>().gameObject; }

            if (swapModelGfx != null) { Destroy(swapModelGfx); }
            swapModelGfx = Instantiate(gfx);

            Animation newAnims = swapModelGfx.GetComponent<Animation>();
            newAnims.AddClip(originalAnims.GetClip("idle"), "idle");
            newAnims.AddClip(originalAnims.GetClip("run"), "run");
            newAnims.AddClip(originalAnims.GetClip("sword"), "sword");
            newAnims.AddClip(originalAnims.GetClip("forcewand"), "forcewand");
            newAnims.AddClip(originalAnims.GetClip("roll"), "roll");
            newAnims.AddClip(originalAnims.GetClip("rollstart"), "rollstart");
            newAnims.AddClip(originalAnims.GetClip("rollloop"), "rollloop");
            newAnims.AddClip(originalAnims.GetClip("rollend"), "rollend");
            newAnims.AddClip(originalAnims.GetClip("chargeefcs"), "chargeefcs");
            newAnims.AddClip(originalAnims.GetClip("efcs"), "efcs");
            newAnims.AddClip(originalAnims.GetClip("death"), "death");
            newAnims.AddClip(originalAnims.GetClip("pushing"), "pushing");
            newAnims.AddClip(originalAnims.GetClip("holefall"), "holefall");
            newAnims.AddClip(originalAnims.GetClip("warp"), "warp");
            newAnims.AddClip(originalAnims.GetClip("strongidle"), "strongidle");
            swapModelGfx.name = "Ittle";

            hideWeapons = false;
            //ModelSpeedChange = SafetyJennyAnimationSpeed;

            DontDestroyOnLoad(swapModelGfx);
            //swapModelGfx.SetActive(false);

            return true;
        }

        public bool Reset()
        {
            if (swapModelGfx != null) { Destroy(swapModelGfx); }
            hideWeapons = false;
            ModelSpeedChange = null;
            return true;
        }

        //Find NPC entities and controllers
        private GameObject FindEntity(string npc)
        {
            //Get lowercase npc name
            string target_npc = npc.ToLower();

            // Search for gameobject with the expected name
            GameObject found_entity = null;
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go.name.ToLower() == target_npc) { if (go.GetComponent<Entity>() != null) { found_entity = go; } }
            }

            return found_entity;
        }


    }
}