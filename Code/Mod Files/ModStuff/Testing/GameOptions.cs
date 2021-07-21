using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Purpose:
 *		Logic that handles the options in the Game Options menu
 *	Authors:
 *		Chris
 *	     Faster transitions
 *	     No delays
 *	     Dark Ogler drop fix
 *	     Fluffy warp fix
 *	     Dynamic health meter
 *	     Automatically change into obtained outfit when you get outfit from chest
*/

namespace ModStuff
{
     class GameOptions : Singleton<GameOptions>
     {
          // Logic
          public string filePath = @"C:\Users\Awesome\Desktop\" + "options.txt";
          //public bool DisableIntroAndOutro { get { return ModSaverNew.LoadFromCustomFile(filePath, nameof(DisableIntroAndOutro)) == "1"; } set { } }
          //public bool FasterTransitions { get { return ModSaverNew.LoadFromCustomFile(filePath, nameof(FasterTransitions)) == "1"; } set { } }
          public bool DisableIntroAndOutro { get; set; }
          public bool FasterTransitions { get; set; }

          // Faster transitions vars
          float roomTransitionTime = 0.5f; // How fast should you change rooms?
          float levelTransitionTime = 0.5f; // How fast should you enter/exit levels?
          float levelFadeOutTime = 0.5f; // Duration of load fade outs (before load)
          float levelFadeInTime = 0.5f; // Duration of load fade ins (after load)

          public void Initialize()
          {
               /*
               if (string.IsNullOrEmpty(ModSaverNew.LoadFromCustomFile(filePath, "// Game Options \\")))
               {
                    ModSaverNew.SaveToCustomFile<string>(filePath, "", "// Game Options \\");
               }*/

               // Subscribe to events
               GameStateNew.OnSceneLoad += OnSceneLoad;
               GameStateNew.OnPlayerSpawn += OnPlayerSpawn;
               GameStateNew.OnCollision += OnCollision;
          }

          void OnSceneLoad(Scene scene, bool isGameplayScene)
          {
               // Skip intro & outro
               if (DisableIntroAndOutro)
               {
                    // Skip intro
                    if (scene.name == "MainMenu")
                    {
                         GameObject mainMenu = GameObject.Find("GuiFuncs");

                         if (mainMenu != null) mainMenu.GetComponent<MainMenu>()._defaultStartScene = "FluffyFields";
                    }

                    // Skip outro
                    else if (scene.name == "FluffyFields")
                    {
                         GameObject outroWarp = GameObject.Find("WinGameTrigger");

                         if (outroWarp != null) outroWarp.transform.Find("LevelDoor").GetComponent<SceneDoor>()._scene = "MainMenu";
                    }
               }
          }

          void OnPlayerSpawn(bool isRespawn)
          {
               // Faster transitions
               if (FasterTransitions)
               {
                    if (!isRespawn)
                    {
                         RoomSwitchable roomSwitcher = ModMaster.GetEntComp<RoomSwitchable>("PlayerEnt");
                         roomSwitcher._transitionSpeed = roomTransitionTime;
                         roomSwitcher._levelTransitionSpeed = levelTransitionTime;
                    }
               }
          }

          void OnCollision(BC_Collider outgoingCollider, BC_Collider incomingCollider, BC_Collider.EventMode collisionType = BC_Collider.EventMode.Enter, bool isTrigger = false)
          {
               SceneDoor collider1Door = outgoingCollider.GetComponent<SceneDoor>();
               SceneDoor collider2Door = incomingCollider.GetComponent<SceneDoor>();
               FadeEffectData fadeOut = null;
               FadeEffectData fadeIn = null;

               if (collider1Door != null)
               {
                    fadeOut = collider1Door._fadeData;
                    fadeIn = collider1Door._fadeInData;
               }
               if (collider2Door != null)
               {
                    fadeOut = collider2Door._fadeData;
                    fadeIn = collider2Door._fadeInData;
               }

               if (fadeOut != null)
               {
                    fadeOut._fadeOutTime = levelFadeOutTime;
                    fadeOut._fadeInTime = levelFadeInTime;
               }
               if (fadeIn != null)
               {
                    fadeOut._fadeOutTime = levelFadeOutTime;
                    fadeOut._fadeInTime = levelFadeInTime;
               }
          }
     }
}