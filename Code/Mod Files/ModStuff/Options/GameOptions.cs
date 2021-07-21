using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff.Options
{
	public class GameOptions : OptionsSingleton<GameOptions>
	{
		// Option properties
		public bool SkipSplash = false;
		public bool SkipIntroOutro;
		public bool RunInBackground = true;
		public bool FastTransitions;
		public bool DynamicHealthMeter = true; // Replaced with pro hud
		public bool FluffyWarpFix; //  Combine?
		public bool DarkOglerDropFix; // Combine?
		public bool ShowFPS;
		public bool LoadAllRooms; // Remove this, become debug command
		public bool NoDelays;
		// Ittle health regen
		// Enemy health regen
		// Pro HUD

		// Fields

		// FastTransitions fields
		float roomTransitionTime = 0.5f; // Speed of room transitions
		public float sceneTransitionTime = 0.5f; // Speed of scene transitions (public to set cooldown in SceneDoor.DoLoad())
		float sceneFadeOutTime = 0.5f; // Speed of scene fade out
		float sceneFadeInTime = 0.5f; // Speed of scene fade in

		// ShowFPS fields
		int frameDelay;
		float deltaTime;
		float levelFrameCount;
		Vector3 fpsPosition = new Vector3(3.25f, -0.65f, 0f);
		TextMesh fpsText;

		// Runs on LudositySplash.Awake()
		// Use for start of game initialization
		public void Startup()
		{
			// Skip splash screen
			if (SceneManager.GetActiveScene().name == "SplashScreen")
			{
				// Run in background
				if (RunInBackground) Application.runInBackground = true;

				// Skip splash screen by loading straight to MainMenu
				if (SkipSplash) Utility.LoadLevel("MainMenu");
			}
		}

		void Update()
		{
			if (ShowFPS && fpsText != null)
			{
				deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
				float fps = 1.0f / deltaTime;
				levelFrameCount++;
				float totalFrameCount = Time.frameCount;
				frameDelay++;

				if (frameDelay % 10 == 0)
				{
					fpsText.text = "FPS: " + fps.ToString("0.00") + "\n";
					fpsText.text += "Frames (total): " + totalFrameCount + "\n";
					fpsText.text += "Frames (level): " + levelFrameCount + "\n";
				}
			}
		}

		protected override void OnSceneLoad(Scene scene, bool isGameplayScene)
		{
			// Skip outro
			// Skips the outro cutscene by loading directly to MainMenu when Outro is loaded from FluffyFields
			if (SkipIntroOutro && scene.name == "FluffyFields")
			{
				GameObject outroWarp = GameObject.Find("WinGameTrigger");

				if (outroWarp != null)
				{
					outroWarp.transform.Find("LevelDoor").GetComponent<SceneDoor>()._scene = "MainMenu";
					ModSaverNew.SaveToSaveFile("start/level", "FluffyFields");
				}
			}

			// Show FPS
			// Shows FPS and other in-game performance statistics in an overlay
			if (ShowFPS)
			{
				GameObject fpsObj = ModMaster.MakeTextObj("FPSView", fpsPosition, "Initializing...", 80, TextAlignment.Left);
				fpsText = fpsObj.GetComponent<TextMesh>();

				// Reset level frame count
				levelFrameCount = 0;
			}

			// Load all rooms
			// Sets each room to active
			if (LoadAllRooms && isGameplayScene)
			{
				List<LevelRoom> roomsInScene = GameObject.Find("LevelRoot").GetComponent<LevelRoot>().AllRooms();

				foreach (LevelRoom room in roomsInScene)
				{
					if (room.RoomName != ModMaster.GetMapName())
						room.SetRoomActive(true, false);
				}
			}
		}

		protected override void OnFileLoad(bool isNew, string fileName = "", string filePath = "", RealDataSaver saver = null)
		{
			// Skip intro
			// Skips the intro by loading from MainMenu into FluffyFields at default start point
			// and updates save file to prevent softlock if warping before loading another scene
			if (SkipIntroOutro && isNew) ModSaverNew.SaveToNewSaveFile("start/level", "FluffyFields", saver);
		}

		protected override void OnPlayerSpawn(bool isRespawn)
		{
			// Faster transitions
			// Speeds up room transitions
			if (FastTransitions && !isRespawn)
			{
				RoomSwitchable roomSwitcher = ModMaster.GetEntComp<RoomSwitchable>("PlayerEnt");
				roomSwitcher._transitionSpeed = roomTransitionTime;
				roomSwitcher._levelTransitionSpeed = sceneTransitionTime;
			}

			// Dynamic health meter
			if (DynamicHealthMeter)
			{
				HealthMeter.BackData healthMeter = GameObject.Find("HealthMeter").GetComponent<HealthMeter>()._backData[0];
				healthMeter.maxHpMoreThan = Mathf.NegativeInfinity;
				healthMeter.texture = FindManager.Instance.FindTextureByName("HeartPaper1");
			}
		}

		protected override void OnCollision(BC_Collider outgoingCollider, BC_Collider incomingCollider, BC_Collider.EventMode collisionType = BC_Collider.EventMode.Enter, bool isTrigger = false)
		{
			// Faster transitions
			// Speeds up scene transitions
			if (FastTransitions)
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
					fadeOut._fadeOutTime = sceneFadeOutTime;
					fadeOut._fadeInTime = sceneFadeInTime;
				}
				if (fadeIn != null)
				{
					fadeOut._fadeOutTime = sceneFadeOutTime;
					fadeOut._fadeInTime = sceneFadeInTime;
				}
			}
		}

		protected override void OnRoomLoad(LevelRoom loadedRoom)
		{
			// Load all rooms
			// Sets each room to active
			if (LoadAllRooms)
			{
				List<LevelRoom> roomsInScene = GameObject.Find("LevelRoot").GetComponent<LevelRoot>().AllRooms();

				foreach (LevelRoom room in roomsInScene)
					room._interpolator = null;
			}

			// No delays - Disable delay before Passel fight
			if (NoDelays)
			{
				if (ModMaster.GetMapName() == "GrandLibrary2" && loadedRoom.RoomName == "BA" && GameObject.Find("RestorePt1") == null)
				{
					GameObject pauser = GameObject.Find("Fight").transform.Find("Pause").gameObject;
					
					if (GameObject.Find("PrePassel") != null && pauser != null)
					{
						pauser.GetComponent<TemporaryPauseEventObserver>().FireNext(false);
						Destroy(pauser.GetComponent<FollowerChangeEventObserver>());
					}

					if (GameObject.Find("PrePassel") == null)
					{
						List<GameObject> chestsAndPortal = new List<GameObject>
						{
							GameObject.Find("SecretChestRaft"),
							GameObject.Find("SecretChestKey"),
							GameObject.Find("SecretPortal")
						};

						for (int i = 0; i < chestsAndPortal.Count; i++)
						{
							GameObject obj = chestsAndPortal[i];
							Destroy(obj.GetComponent<DelayedEventObserver>()); // Skip object appear delay
							obj.GetComponent<DisableEventObserver>().SetCompsActive(true); // Activate object
							obj.GetComponent<EffectEventObserver>().PlayIn(); // Play appear animation
						}

						// Move player away from portal
						GameObject.Find("PlayerEnt").transform.position = new Vector3(7, 0, -6);
					}
				}
				else if (ModMaster.GetMapName() == "TombOfSimulacrum" && loadedRoom.RoomName == "I")
				{
					TemporaryPauseEventObserver pauser = GameObject.Find("Dialog").GetComponent<TemporaryPauseEventObserver>();
					pauser.FireNext(false);
					Destroy(pauser);
				}
				else if (ModMaster.GetMapName() == "FluffyFields" && loadedRoom.RoomName == "A")
				{
					GameObject introDialog = GameObject.Find("IntroDialog");
					if (introDialog != null)
					{
						introDialog.GetComponent<TemporaryPauseEventObserver>()._pauseTime = 0.1f;
					}
				}
			}
		}

		protected override void OnEnemySpawn(Entity ent)
		{
			// No delays - Disable delay before boss fights
			if (NoDelays && ModMaster.GetMapName() != "GrandLibrary2" && ModMaster.GetMapName() != "Deep19s")
			{
				// If boss
				if (ent.GetComponent<EntityTag>().TagName.ToLower() == "boss")
				{
					LevelRoom bossRoom = LevelRoom.GetRoomForPosition(ent.transform.position);

					// If simula
					if (ent.name == "Simulacrum")
					{
						// Skip delays (BAD CODE WARNING)
						GameObject hpMeter = GameObject.Find("SimulaBossDialog").transform.Find("BossMeter").gameObject;
						hpMeter.SetActive(true);
						hpMeter.transform.parent = bossRoom.transform.Find("Logic");
						GameObject.Destroy(GameObject.Find("SimulaBossDialog"));

						// Set boss state to start fight
						LudoFunctions.AIManager.Instance.SetState(ent.gameObject, "do canel");

						// Disable boss death explosion
						Killable bossKillable = ent.transform.Find("Killable").GetComponent<Killable>();
						bossKillable._deathEffect = null;
						bossKillable._startDeathEffect = null;
					}
					// For GOOD bosses
					else
					{
						// Skip delays
						TemporaryPauseEventObserver[] pausers = bossRoom.GetComponentsInChildren<TemporaryPauseEventObserver>();
						for (int i = 0; i < pausers.Length; i++)
						{
							pausers[i].FireNext(false);
							GameObject.Destroy(pausers[i]);
						}

						// Skip camera pan
						GameObject.Destroy(bossRoom.GetComponentInChildren<FollowerChangeEventObserver>());

						// Set boss state to start fight
						LudoFunctions.AIManager.Instance.SetState(ent.gameObject, "dummy");

						// Disable boss death explosion
						Killable bossKillable = ent.transform.Find("Killable").GetComponent<Killable>();
						bossKillable._deathEffect = null;
						bossKillable._startDeathEffect = null;
					}
				}
			}
		}

		protected override void OnEnemyDeath(Entity deadEnt)
		{
			// No cutscenes - Disable post fight delay (spawns chest and portal, opens door instantly)
			if (NoDelays && ModMaster.GetMapName() != "GrandLibrary2" && !deadEnt.name.Contains("Cocoon"))
			{
				// If boss
				if (deadEnt.GetComponent<EntityTag>() != null && deadEnt.GetComponent<EntityTag>().TagName.ToLower() == "boss")
				{
					List<GameObject> postFightStuff = new List<GameObject>();

					// ANOTHER exception for Simula since she is a horrible robot, also exception for Flooded Basement
					if (deadEnt.name == "Simulacrum" || ModMaster.GetMapName() == "FloodedBasement")
					{
						Transform bossRoom = LevelRoom.GetRoomForPosition(deadEnt.transform.position).transform.Find("Logic");

						postFightStuff.Add(bossRoom.Find("SecretChest").gameObject); // Get chest
						postFightStuff.Add(bossRoom.Find("SecretPortal").gameObject); // Get portal
						// Get boss door
						if (ModMaster.GetMapName() == "FloodedBasement") postFightStuff.Add(bossRoom.Find("PuzzleDoor_bosstrapdoor").gameObject);
						else postFightStuff.Add(ModMaster.FindNestedChild(bossRoom.parent.gameObject, "PuzzleDoor_bosstrapdoor", "Doodads").gameObject);
					}
					else if (deadEnt.name == "DreamMoth")
					{
						// Activate portal
						GameObject.Find("Boss").transform.Find("MiniWarp4").gameObject.SetActive(true);

						GameObject bossDoor = GameObject.Find("AD").transform.Find("Doodads").Find("PuzzleDoor_bosstrapdoor").gameObject;
						GameObject.Destroy(bossDoor.GetComponent<DelayedEventObserver>()); // Skip delay
						bossDoor.GetComponent<DisableEventObserver>().SetCompsActive(true); // Activate object

						// Play animation of boss door opening
						if (bossDoor.name == "PuzzleDoor_bosstrapdoor") bossDoor.GetComponent<AnimationEventObserver>().PlayIn();
					}
					else if (deadEnt.name == "ThatGuy2")
					{
						GameObject.Find("InstantWarper").GetComponent<DisableEventObserver>().SetCompsActive(true);
					}
					// Good bosses
					else
					{
						Transform bossRoom;

						// Exception for Mechabuns
						if (deadEnt.name.Contains("Mechabun"))
						{
							bossRoom = GameObject.Find("Logic").transform;
						}
						else
						{
							bossRoom = LevelRoom.GetRoomForPosition(deadEnt.transform.position).transform.Find("Doodads");
						}

						// Get chests
						foreach (DummyAction chest in bossRoom.GetComponentsInChildren<DummyAction>())
						{
							if (chest._saveName.Contains("SecretChest")) postFightStuff.Add(chest.gameObject);
						}

						PlayerPrefs.SetString("test", "Found boss room obj as: " + bossRoom.name);

						postFightStuff.Add(bossRoom.Find("SecretPortal").gameObject); // Get portal

						// Exception for Mechabuns
						if (deadEnt.name.Contains("Mechabun"))
						{
							bossRoom = LevelRoom.GetRoomForPosition(deadEnt.transform.position).transform.Find("Doodads");
						}

						postFightStuff.Add(bossRoom.Find("PuzzleDoor_bosstrapdoor").gameObject); // Get boss door
					}

					// Spawn chest(s) & portal, open boss door
					for (int i = 0; i < postFightStuff.Count; i++)
					{
						GameObject go = postFightStuff[i];
						GameObject.Destroy(go.GetComponent<DelayedEventObserver>()); // Skip delay
						go.GetComponent<DisableEventObserver>().SetCompsActive(true); // Activate object

						// Play animation of boss door opening
						if (go.name == "PuzzleDoor_bosstrapdoor") go.GetComponent<AnimationEventObserver>().PlayIn();
						// Play animation of chest/portal spawn
						else go.GetComponent<EffectEventObserver>().PlayIn();

						// Destroy boss
						GameObject.Destroy(deadEnt.gameObject);
					}

					if (!ModeControllerNew.activeModes.Contains(ModeControllerNew.ModeType.BossRush))
					{
						// Update boss save flags
						string bossName = deadEnt.name.ToLower();
						bool isMainThree = bossName.Contains("cyberjenny") || bossName.Contains("lebiadlo") || bossName.Contains("lenny");
						string savePath = "world/bosses";
						string saveKey = "nextBoss";
						int nextBoss = ModSaver.LoadIntFromFile(savePath, saveKey) + 1;
						ModSaver.SaveIntToFile(savePath, saveKey, nextBoss);

						if (isMainThree)
						{
							savePath = savePath + "/" + bossName.Remove(bossName.Length - 1);
							saveKey = "counter";
							nextBoss = ModSaver.LoadIntFromFile(savePath, saveKey) + 1;
							ModSaver.SaveIntToFile(savePath, saveKey, nextBoss);
						}
					}
				}
			}
		}
	}
}