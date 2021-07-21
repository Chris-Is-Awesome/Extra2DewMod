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
                        {"hyperdusa_steel","hyperdusacontroller"},
                        {"hyperdusa_steelcold","hyperdusacontroller"}
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

            //Parent
            public Transform parent;

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
                Entity createdNPC = SpawnWithAi(spawn_entity, spawn_controller, spawn.SpawnPosition, spawn.scale, spawn.FacingDirection, spawn.parent);
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
            string target_ai = ((string.IsNullOrEmpty(ai)) ? target_npc : ai).ToLower();

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
        Entity SpawnWithAi(Entity target, AIController ai, Vector3 position, Vector3 scale, Vector3 direction, Transform parent)
        {
            Entity myspawn = EntityFactory.Instance.GetEntity(target, parent, position);

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

        //Destroy all enemies message for the UI
        string[] destroyText;
        public string DestroyMessage()
        {
            if(destroyText == null)
            {
                destroyText = new string[]
                {
                    "Destroy enemies", "Remove enemies", "Erase enemies",
                    "DESTROY", "Send enemies to\nthe shadow realm", "<i>*snap fingers*</i>",
                    "Obliterate", "Explodantinate", "<color=red>CONFETTI FOR THE\nCONFETTI GOD</color>",
                    "Slap 'em", "DYNAMITE!", "Ask the NPCs\nto leave",
                    "Tell a <i>really</i>\nbad joke", "Outsmart enemies", "<i>*poof*</i>",
                    "Highlander", "Call the Goddess\nof Explosions", "Find diplomatic\nsolution",
                    "<size=28>Shawadin-shawadon!\nEnemies are gone!</size>", "Adventurer's glee",
                    "<size=28>Galactic\nAdventurer Buster</size>", "Confettification",
                    "what does this\nbutton do?", "Light the fuse", "JUSTICE", "You are all FIRED!",
                    "<i>Passive</i> agression", "Wish for more\nexplosions",
                    "<size=28>GETOUTGETOUTGETOUT\nGETOUTGETOUTGETOUT</size>"
                };
            }

            return destroyText[UnityEngine.Random.Range(0, destroyText.Length)];
        }

        //Spawn menu
        //Beware, this is is pure pasta
        public static UIScreen SpawnMenu()
        {
            //Create a base window, which contains a title and a back button. Name it.
            UIScreen output = UIScreen.CreateBaseScreen("NPCs");
            output.BackButton.transform.localPosition = new Vector3(0f, -4f, 0f);

            //Holders for advanced controls
            GameObject advancedControls = new GameObject("AdvancedSpawnControls");
            advancedControls.transform.SetParent(output.transform, false);
            advancedControls.transform.localScale = Vector3.one;

            //Holders for quick selection
            GameObject npcSelection = new GameObject("NPCSelection");
            npcSelection.transform.SetParent(output.transform, false);
            npcSelection.transform.localScale = Vector3.one;

            //Quick menu controls
            GameObject quickControls = new GameObject("QuickConstrols");
            quickControls.transform.SetParent(output.transform, false);
            quickControls.transform.localScale = Vector3.one;

            //Destroy NPCs 
            //Button
            UIGFXButton destroyNpcs = UIFactory.Instance.CreateGFXButton("destroynpcs", 0f, -0.75f, quickControls.transform, "Destroy nearby\nenemies");
            destroyNpcs.onInteraction += delegate ()
            {
                DebugCommands.Instance.Kill(new string[] { "enemies" });
            };
            destroyNpcs.AutoTextResize = false;
            output.SaveElement("destroy", destroyNpcs);
            //Holding frame
            UIBigFrame buttonHolder = UIFactory.Instance.CreateBigFrame(UIBigFrame.FrameType.Default, 0f, 0f, destroyNpcs.transform.Find("ModButton").Find("ModUIElement"));
            buttonHolder.name = "killbackground";
            buttonHolder.ScaleBackground(new Vector2(0.26f, 0.85f));
            buttonHolder.transform.localScale = new Vector3(2f, 0.625f, 1f);
            buttonHolder.transform.localPosition += new Vector3(0f, -0.25f, 1.1f);
            //Help frame
            UITextFrame destroyHelp = UIFactory.Instance.CreatePopupFrame(0f, -2.25f, destroyNpcs, quickControls.transform, "Destroy all enemies\non the screen");
            destroyHelp.ScaleBackground(new Vector2(1.25f, 1.2f));
            destroyHelp.transform.localPosition += new Vector3(0f, 0f, -1.1f);

            //NPC Scale
            UIVector3 enemiesScale = UIFactory.Instance.CreateVector3(5.5f, -1f, quickControls.transform, "NPCs size");
            enemiesScale.Explorer.Title.ScaleBackground(new Vector2(0.7f, 0.8f));
            enemiesScale.onInteraction += delegate (Vector3 vector)
            {
                DebugCommands.Instance.SetSize(new string[] { "enemies", vector.x.ToString(), vector.y.ToString(), vector.z.ToString() });
            };
            output.SaveElement("npcscale", enemiesScale);
            enemiesScale.transform.localScale *= 0.8f;
            UIButton resetScale = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 5.5f, -2f, quickControls.transform, "Reset");
            resetScale.ScaleBackground(Vector2.one, Vector2.one * 0.75f);
            resetScale.onInteraction += delegate () { enemiesScale.Trigger(Vector3.one); };

            //Help button
            UIButton help = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 6f, -4f, output.transform, "?");
            help.ScaleBackground(new Vector2(0.2f, 1f));
            UITextFrame helpHelp = UIFactory.Instance.CreatePopupFrame(0f, 2f, help, output.transform, "");
            helpHelp.ScaleBackground(new Vector2(2.5f, 2.6f));
            helpHelp.ScaleText(0.75f);
            helpHelp.WriteText("Click on a NPC name in the list to spawn it. Use advanced mode for more spawning options.\n\nNOTE: Initially not all NPCs are available everywhere, bosses for example can only be found in dungeons. Once an NPC is spawned from the menu it will unlock for everywhere else.");
            helpHelp.transform.localPosition += new Vector3(0f, 0f, -0.4f);

            //State of the menu
            bool advancedMode = false;
            bool aiSelect = false;

            //NPC selection
            UITextFrame helpSelection = UIFactory.Instance.CreateTextFrame(0f, 5.3f, advancedControls.transform, "Choose NPC");
            helpSelection.transform.localScale = Vector3.one * 0.7f;
            helpSelection.transform.localPosition += new Vector3(0f, 0f, 0.3f);
            UIButton npcExplorer = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 0f, 4.5f, advancedControls.transform);
            npcExplorer.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            string[] tempArray = ModSpawner.Instance.EntitiesNamesList; //Gather the list
            Array.Sort(tempArray, StringComparer.InvariantCulture); //Sort the list
            //Initiate on fishbun, if not, on the first value if available
            if (Array.IndexOf(tempArray, "Fishbun") != -1)
            {
                npcExplorer.UIName = "Fishbun";
            }
            else
            {
                npcExplorer.UIName = (tempArray.Length > 0) ? tempArray[0] : "NO NPCS AVAILABLE";
            }

            //Mode switch to advanced
            UIButton openAdvanced = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, -5f, -1.75f, quickControls.transform, "Advanced Mode");
            openAdvanced.ScaleBackground(new Vector2(0.7f, 1f), new Vector2(1.5f, 1.5f));
            openAdvanced.onInteraction += delegate ()
            {
                advancedControls.SetActive(true);
                quickControls.SetActive(false);
                npcSelection.SetActive(false);
                destroyNpcs.gameObject.SetActive(false);
                advancedMode = true;
            };

            //Mode switch to quick
            UIButton openQuick = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, UIScreenLibrary.FirstCol, -2f, advancedControls.transform, "Quick Mode");
            openQuick.ScaleBackground(new Vector2(0.7f, 1f), new Vector2(1.2f, 1.2f));
            openQuick.onInteraction += delegate ()
            {
                destroyNpcs.UIName = ModSpawner.Instance.DestroyMessage();
                advancedControls.SetActive(false);
                quickControls.SetActive(true);
                npcSelection.SetActive(true);
                advancedMode = false;
                openAdvanced.gameObject.SetActive(true);
                destroyNpcs.gameObject.SetActive(true);
            };

            //Spawn Button
            UIButton spawnNow = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 0f, 3.35f, advancedControls.transform, "Spawn"); //Create the Button
            spawnNow.ScaleBackground(new Vector2(0.6f, 1f));

            //Amount
            UISlider amountToSpawn = UIFactory.Instance.CreateSlider(UIScreenLibrary.FirstCol + 0.5f, 2.7f, advancedControls.transform, "Amount"); //Create the slider
            amountToSpawn.SliderRange = new Range(1f, 25f); //Set the range
            amountToSpawn.SliderStep = 1f; //Set the step size
            amountToSpawn.DisplayInteger = true; //Make it display integers
            amountToSpawn.Value = 0f; //Set initial value

            //Distance chooser
            UISlider distanceToSpawn = UIFactory.Instance.CreateSlider(UIScreenLibrary.FirstCol + 0.5f, 1.2f, advancedControls.transform, "Distance"); //Create the slider
            distanceToSpawn.SliderRange = new Range(0f, 10f); //Set the range
            distanceToSpawn.SliderStep = 0.5f; //Set the step size
            distanceToSpawn.Value = 2f; //Set initial value

            //Position scatterer
            UISlider randomPos = UIFactory.Instance.CreateSlider(UIScreenLibrary.FirstCol + 0.5f, -0.3f, advancedControls.transform, "Scatter"); //Create the slider
            randomPos.SliderRange = new Range(0f, 20f); //Set the range
            randomPos.SliderStep = 0.5f; //Set the step size
            randomPos.Value = 0f; //Set initial value

            //Spawn properties
            UIScrollMenu propMenu = UIFactory.Instance.CreateScrollMenu(UIScreenLibrary.LastCol, 0.5f, advancedControls.transform, "Properties");
            propMenu.ScrollBar.transform.localPosition += new Vector3(-1.6f, -1f, 0f);
            propMenu.ScrollBar.ResizeLength(4);
            propMenu.Title.ScaleBackground(new Vector2(0.8f, 1f));
            propMenu.CanvasWindow = 6.5f;
            propMenu.EmptySpace = 1f;

            //Scale
            UIVector3 scaleVector3 = UIFactory.Instance.CreateVector3(0f, -1.5f, advancedControls.transform, "Scale");
            scaleVector3.transform.localScale = Vector3.one * 0.8f;
            propMenu.Assign(scaleVector3, 1.25f, 0.75f);

            //Rotation selector
            //Holder
            GameObject rotHolder = new GameObject("RotHolder");
            rotHolder.transform.SetParent(output.transform, false);
            rotHolder.transform.localScale *= 0.8f;
            rotHolder.transform.localPosition = new Vector3(0f, -3.0f, 0f);
            //List explorer
            UIListExplorer rotSelect = UIFactory.Instance.CreateListExplorer(0f, 0f, rotHolder.transform, "Rotation");
            rotSelect.AllowLooping = true;
            string constant = "Constant";
            rotSelect.ExplorerArray = new string[] { "Random", "Player", constant };
            rotSelect.ScaleBackground(0.75f);
            //Slider
            UISlider rotSlider = UIFactory.Instance.CreateSlider(0f, -0.75f, rotHolder.transform, ""); //Create the slider
            rotSlider.SetSlider(0f, 360f, 1f, 0f); //Set slider can be used instad of the individual variables and it sets non-linear sliders
            rotSlider.DisplayInteger = true;
            rotSlider.gameObject.SetActive(false);
            rotSelect.onInteraction += delegate (bool rightArrow, string stringValue, int index)
            {
                rotSlider.gameObject.SetActive(stringValue == constant);
            };
            propMenu.Assign(rotHolder, 0.6f, 1f);

            //AI explorer
            //Holder
            GameObject aiHolder = new GameObject("AIHolder");
            aiHolder.transform.SetParent(output.transform, false);
            aiHolder.transform.localScale = Vector3.one;
            aiHolder.transform.localPosition = new Vector3(0f, -5f, 0f);
            //Checkbox
            UICheckBox aiCheckbox = UIFactory.Instance.CreateCheckBox(0f, 0f, aiHolder.transform, "Change AI?");
            aiCheckbox.ScaleBackground(1f, Vector3.one * 0.8f);
            aiCheckbox.transform.localPosition += new Vector3(0f, 0f, -0.2f);
            //Button
            UIButton aiExplorer = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 0f, -0.75f, aiHolder.transform, "Click to select AI");
            aiExplorer.gameObject.SetActive(false);
            aiCheckbox.onInteraction += delegate (bool enable) { aiExplorer.gameObject.SetActive(enable); };
            propMenu.Assign(aiHolder);

            //HP
            UIListExplorer hpExplorer = UIFactory.Instance.CreateListExplorer(0f, -7.3f, advancedControls.transform, "Health"); //Create the UIElement
            hpExplorer.ExplorerArray = new string[] { "1 HP", "0.5x HP", "Regular", "2x HP", "5x HP", "10x HP", "100x HP", "Invulnerable" }; //Create a string[] for the explorer
            hpExplorer.IndexValue = 2; //Set initial point for the list explorer
            hpExplorer.transform.localScale *= 0.8f;
            hpExplorer.ScaleBackground(0.75f);
            propMenu.Assign(hpExplorer);

            //No AI NPC
            UIButton noAI = UIFactory.Instance.CreateButton(UIButton.ButtonType.Default, 0f, -1.5f, npcSelection.transform, "No AI");
            noAI.ScaleBackground(new Vector2(0.7f, 1f), new Vector2(1.5f, 1.5f));
            noAI.onInteraction += delegate ()
            {
                advancedControls.SetActive(true);
                npcSelection.SetActive(false);
                noAI.gameObject.SetActive(false);
                aiExplorer.UIName = "No AI";
                aiSelect = false;
            };
            //AI button function
            aiExplorer.onInteraction += delegate ()
            {
                openAdvanced.gameObject.SetActive(false);
                aiSelect = true;
                advancedControls.SetActive(false);
                npcSelection.SetActive(true);
                noAI.gameObject.SetActive(true);
            };
            noAI.gameObject.SetActive(false);

            //NPC Selection
            UI2DList npcList = UIFactory.Instance.Create2DList(0f, 3.15f, new Vector2(4f, 3f), Vector2.one, Vector2.one, npcSelection.transform);
            npcList.HighlightSelection = false;
            npcList.ScrollBar.ResizeLength(5);
            npcList.ScrollBar.transform.localPosition += new Vector3(3.3f, 0f, 0f);
            npcList.Title.gameObject.SetActive(false);
            npcList.ExplorerArray = tempArray;
            npcList.onInteraction += delegate (string textValue, int arrayIndex)
            {
                if (advancedMode)
                {
                    if (aiSelect)
                    {
                        aiExplorer.UIName = textValue;
                        aiSelect = false;
                        noAI.gameObject.SetActive(false);
                    }
                    else
                    {
                        npcExplorer.UIName = textValue;
                    }
                    advancedControls.SetActive(true);
                    npcSelection.SetActive(false);
                }
                else
                {
                    ModSpawner.SpawnProperties properties = new ModSpawner.SpawnProperties();
                    properties.npcName = textValue;
                    properties.SpawnInFrontOfPlayer(3.5f);
                    properties.useRandomRotation = false;
                    properties.fixedRotation = 180f;
                    properties.aroundPoint = true;
                    properties.distanceFromPoint = 1.2f;
                    ModSpawner.Instance.HoldNPC(properties.npcName, properties.npcName);
                    ModSpawner.Instance.SpawnNPC(properties);
                }
            };

            //Spawn Function
            spawnNow.onInteraction += delegate () //Set the delegate to the spawn function below, it can also be done using a named function and referencing it
            {
                //Spawn properties is the new class used to spawn NPCs. By default, it initializes
                //spawning a regular fishbun on top of Ittle, but everything can be changed by code
                ModSpawner.SpawnProperties properties = new ModSpawner.SpawnProperties(); //Create new spawn property, initialized
                if (aiCheckbox.Value) { properties.ai = aiExplorer.UIName; }
                properties.npcName = npcExplorer.UIName; //Extract the ArrayValue of the explorer, receiving the string of the NPC
                properties.amount = (int)amountToSpawn.Value; //Extract the Value of the slider, convert it into an integer
                properties.SpawnInFrontOfPlayer(distanceToSpawn.Value); //Set the position to spawn it in the world
                properties.scale = scaleVector3.Value;
                if (randomPos.Value > 0.1f) //If the random position slider is bigger than 0, use random position
                {
                    properties.aroundPoint = true;
                    properties.distanceFromPoint = randomPos.Value;
                }
                switch (rotSelect.IndexValue)
                {
                    case 0:
                        break;
                    case 1:
                        properties.UsePlayerRotation();
                        break;
                    case 2:
                        properties.useRandomRotation = false;
                        properties.fixedRotation = rotSlider.Value;
                        break;
                    default:
                        return;
                }
                switch (hpExplorer.IndexValue) //HP selector switch
                {
                    case 0:
                        properties.ConfigureHP(1f, 0f);
                        break;
                    case 1:
                        properties.ConfigureHP(0f, 0.5f);
                        break;
                    case 2:
                        break;
                    case 3:
                        properties.ConfigureHP(0f, 2f);
                        break;
                    case 4:
                        properties.ConfigureHP(0f, 5f);
                        break;
                    case 5:
                        properties.ConfigureHP(0f, 10f);
                        break;
                    case 6:
                        properties.ConfigureHP(0f, 100f);
                        break;
                    case 7:
                        properties.MakeInvulnerable();
                        break;
                    default:
                        break;
                }
                ModSpawner.Instance.HoldNPC(properties.npcName, properties.npcName);
                ModSpawner.Instance.SpawnNPC(properties); //Spawn using spawnproperties
            };
            output.SaveElement("spawnnow", spawnNow); //Save the UIElement

            advancedControls.SetActive(false);
            quickControls.SetActive(true);
            npcSelection.SetActive(true);

            //NPC explorer function
            npcExplorer.onInteraction += delegate ()
            {
                openAdvanced.gameObject.SetActive(false);
                advancedControls.SetActive(false);
                npcSelection.SetActive(true);
            };

            //If back is pressed, remove NPC and NPC AI selection
            output.BackButton.onInteraction += delegate ()
            {
                if (advancedMode)
                {
                    advancedControls.SetActive(true);
                    npcSelection.SetActive(false);
                    aiSelect = false;
                    noAI.gameObject.SetActive(false);
                }
            };

            return output;
        }

    }
}
