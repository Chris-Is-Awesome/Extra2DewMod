using System.Collections.Generic;
using UnityEngine;
using System;

namespace ModStuff
{
    public class ModSpawner : Singleton<ModSpawner>
    {
        //Spawner Awake
        Dictionary<string, string> controllerexceptions;
        Dictionary<string, string> ContExceptions
        {
            get
            { 
                if(controllerexceptions == null)
                {
                    controllerexceptions = new Dictionary<string, string>
                    {
                        {"lemonjenny","flowerjennycontroller"},
                        {"monochrome","monochrome2controller"},
                        {"fishbuncow","fishbuncontroller"},
                        {"sweatycowbun","fishbuncontroller"},
                        {"sweatyfishbun","fishbuncontroller"},
                        {"hyperdusa_steel","hyperdusacontroller"}
                    };
                }
                return controllerexceptions;
            }
        }

        //Class used to setup NPC spawns
        public class SpawnProperties
        {
            //NPC
            public string npcName;
            public string ai;

            //Quantity
            public int amount;

            //Position
            public Vector3 fixedPosition; //Spawn point
            public bool aroundPoint; //If true, randomize around the spawn point
            public float distanceFromPoint; //Distance to randomize from
            public void SpawnInFrontOfPlayer(float spawnDistance) //Set fixed position to spawnDistance in front of the player
            {
                GameObject playerGO = DebugCommands.Instance.player;
                if (playerGO == null) { return; }
                fixedPosition = playerGO.transform.localPosition + playerGO.transform.localRotation * Vector3.forward * spawnDistance;
            }
            public Vector3 SpawnPosition //Get spawn position for spawning
            {
                get
                {
                    if (!aroundPoint) { return fixedPosition; }
                    float r;
                    float theta;

                    r = distanceFromPoint * Mathf.Sqrt(UnityEngine.Random.value);
                    theta = UnityEngine.Random.value * 2 * Mathf.PI;

                    return new Vector3(fixedPosition.x + r * Mathf.Cos(theta), fixedPosition.y, fixedPosition.z + r * Mathf.Sin(theta));
                }
            }

            //Rotation
            public bool useRandomRotation; //If ture, use random rotation
            public float fixedRotation; //Rotation to use
            Vector3 FixedRotation { get { return new Vector3(0f, fixedRotation, 0f); } }
            Vector3 RandomRotation { get { return new Vector3(0f, UnityEngine.Random.Range(0f, 360f), 0f); } }

            public void UsePlayerRotation()
            {
                useRandomRotation = false;
                GameObject player = DebugCommands.Instance.player;
                if (player == null) { return; }
                fixedRotation = player.transform.localEulerAngles.y;
            }

            public Vector3 FacingDirection //Get spawn rotation for spawning
            {
                get
                {
                    return useRandomRotation ? RandomRotation : FixedRotation;
                }
            }

            //Scale
            public Vector3 scale; //Spawn scale

            //HP
            bool useDifferentHP;
            float fixedHP; //Fixed HP to set to the NPC
            float hpMultiplier; //Multiply regular spawn hp
            public void ConfigureHP(float fixedValue, float multiplierValue)
            {
                useDifferentHP = true;
                fixedHP = fixedValue;
                hpMultiplier = multiplierValue;
            }
            public void MakeInvulnerable() //Sets HP to infinite
            {
                ConfigureHP(float.MaxValue, 0f);
            }
            public void SetHP(Entity spawned) //Called from outside, sets the entity's HP
            {
                if (!useDifferentHP) { return; }

                Killable createdNPCKillable = spawned.GetEntityComponent<Killable>();
                if (createdNPCKillable == null) { return; }

                float newMaxHP = fixedHP + createdNPCKillable.MaxHp * hpMultiplier;
                createdNPCKillable.MaxHp = newMaxHP;
                createdNPCKillable.CurrentHp = newMaxHP;
            }

            //Constructor
            public SpawnProperties()
            {
                amount = 1;

                npcName = "Fishbun";
                ai = "";

                SpawnInFrontOfPlayer(0f);
                aroundPoint = false;
                distanceFromPoint = 0f;

                useRandomRotation = true;
                fixedRotation = 0f;

                scale = Vector3.one;

                useDifferentHP = false;
                fixedHP = 0f;
                hpMultiplier = 1f;
            }
        }

        //Spawn an NPC by SpawnProperties
        public string SpawnNPC(SpawnProperties spawn)
        {
            //if(entitiesList == null || controllersList == null) { FillSpawnerLists(); }

            //Find gameobjects
            GameObject[] target_Gos = FindEntityAndController(spawn.npcName, spawn.ai);

            // If the entity to spawn was not found, exit. If it was found, save it
            if (target_Gos[0] == null) { return "Error: '" + spawn.npcName + "' NPC not found."; }
            Entity spawn_entity = target_Gos[0].GetComponent<Entity>();

            // Save entity and aicontroller
            AIController spawn_controller = null;
            if (target_Gos[1] != null) { spawn_controller = target_Gos[1].GetComponent<AIController>(); }

            // Configure output text
            string output;
            if (spawn.amount > 1)
            {
                output = "Spawned " + spawn.amount + " " + target_Gos[0].name + "s!";
            }
            else
            {
                output = target_Gos[0].name + " spawned!";
            }
            output += (spawn_controller == null) ? " AI controller not found.\n" : "\n";

            // Spawn entities
            for (int j = 0; j < spawn.amount; j++)
            {
                Entity createdNPC = SpawnWithAi(spawn_entity, spawn_controller, spawn.SpawnPosition, spawn.scale, spawn.FacingDirection);
                spawn.SetHP(createdNPC);
            }

            return output;
        }

        //Find NPC entities and controllers
        public GameObject[] FindEntityAndController(string npc, string ai = "")
        {
            //Get lowercase npc name
            string target_npc = npc.ToLower();

            //If no AI was set, use the AI from target_npc
            string target_ai;
            if (string.IsNullOrEmpty(ai)) { target_ai = target_npc; } else { target_ai = ai.ToLower(); }

            //See if the AI has an AIcontroller exception
            if (ContExceptions.TryGetValue(target_ai, out string var))
            {
                target_ai = var;
            }
            else
            {
                target_ai = target_ai + "controller";
            }

            // Search for gameobjects and AIcontrollers with the expected names in the lists
            GameObject found_entity = null;
            GameObject found_controller = null;
            foreach (GameObject go in EntitiesList)
            {
                if (go.name.ToLower() == target_npc) { found_entity = go; }
            }
            foreach (GameObject go2 in ControllersList)
            {
                if (go2.name.ToLower() == target_ai) { found_controller = go2; }
            }

            return new GameObject[] { found_entity, found_controller };
        }

        //Spawn NPC with entity and aicontroller
        Entity SpawnWithAi(Entity target, AIController ai, Vector3 position, Vector3 scale, Vector3 direction)
        {
            Entity myspawn = EntityFactory.Instance.GetEntity(target, null, position);

            if (ai != null) { ControllerFactory.Instance.GetController<AIController>(ai).ControlEntity(myspawn); }
            myspawn.transform.eulerAngles = direction;
            myspawn.transform.localScale = scale;

            return myspawn;
        }

        //List NPCs
        List<GameObject> entitiesList;
        List<GameObject> EntitiesList
        {
            get
            {
                if(entitiesList == null) { FillSpawnerLists(); }
                return entitiesList;
            }
        }

        public string[] EntitiesNamesList
        {
            get
            {
                List<string> output = new List<string>();
                foreach (GameObject go in EntitiesList)
                {
                    output.Add(go.name);
                }
                return output.ToArray();
            }
        }

        List<GameObject> controllersList;
        List<GameObject> ControllersList
        {
            get
            {
                if (controllersList == null) { FillSpawnerLists(); }
                return controllersList;
            }
        }

        public void FillSpawnerLists()
        {
            entitiesList = new List<GameObject>();
            controllersList = new List<GameObject>();
            GameObject[] goList = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
            Array.Reverse(goList);
            
            foreach (GameObject gameObject in goList)
            {
                if(gameObject.name == "PlayerEnt") { continue; }
                if (gameObject.GetComponent<Entity>() != null && !CheckNPCInListByName(gameObject.name, entitiesList))
                {
                    entitiesList.Add(gameObject);
                }
                if (gameObject.GetComponent<AIController>() != null && !CheckNPCInListByName(gameObject.name, controllersList))
                {
                    controllersList.Add(gameObject);
                }
            }
        }

        public void EmptySpawnerLists()
        {
            entitiesList = null;
            controllersList = null;
        }

        bool CheckNPCInListByName(string newName, List<GameObject> list)
        {
            foreach (GameObject go in list)
            {
                if (go.name == newName) { return true; }
            }
            return false;
        }

        public string NPCList()
        {
            string npcOutput = "";
            string[] list = EntitiesNamesList;
            if (list.Length > 0)
            {
                npcOutput += list[0];
                for (int i = 1; i < list.Length; i++)
                {
                    npcOutput += " " + list[i];
                }
            }
            return npcOutput;
        }

        List<string> hold_Npc;
        List<string> hold_Ai;
        public string HoldNPC(string spawn_Type, string spawn_Ai)
        {
            GameObject[] npc_and_controller = FindEntityAndController(spawn_Type, spawn_Ai);
            string output = "";

            //If the entity was not found, cancel
            if (npc_and_controller[0] == null)
            {
                output = "Error: " + spawn_Type + " not found!";
                return output;
            }

            //Initialize list
            if (hold_Npc == null)
            {
                hold_Npc = new List<string>();
                hold_Ai = new List<string>();
            }

            //If the npc is already in memory, cancel
            if (!hold_Npc.Contains(npc_and_controller[0].name))
            {
                hold_Npc.Add(npc_and_controller[0].name);
                GameObject mynpc = Instantiate(npc_and_controller[0]);
                mynpc.SetActive(false);
                mynpc.name = npc_and_controller[0].name;
                DontDestroyOnLoad(mynpc);
                output += mynpc.name + " held in memory.\nNow you can spawn " + mynpc.name + "s anywhere!\n\n";
            }
            else
            {
                output += "Cancelling -hold: " + npc_and_controller[0].name + " is already in memory\n";
            }

            //If the ai is already in memory, cancel
            if (npc_and_controller[1] != null)
            {
                if (!hold_Ai.Contains(npc_and_controller[1].name))
                {
                    GameObject myai = Instantiate(npc_and_controller[1]);
                    hold_Ai.Add(npc_and_controller[1].name);
                    myai.name = npc_and_controller[1].name;
                    DontDestroyOnLoad(myai);
                }
            }
            else if (spawn_Ai != null)
            {
                output += "Cancelling " + spawn_Ai + "'s AI -hold (controller not found)\n";
            }

            return output;
        }
    }
}
