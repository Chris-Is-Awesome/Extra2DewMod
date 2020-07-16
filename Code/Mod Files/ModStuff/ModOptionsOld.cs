using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff
{
	public static class ModOptionsOld
	{
		public static bool disableExtraCutscenes = false;
		public static bool disableIntroAndOutro = false;
		public static bool betterHealthMeter = false;
		public static bool darkOglerDropFix = false;
		public static bool fastTransitions = false;
		public static bool loadAllRooms = true;

		static Dictionary<string, string> bossRooms = new Dictionary<string, string>()
		{
			{ "PillowFort", "C" },
			{ "SandCastle", "A" },
			{ "ArtExhibit", "D" },
			{ "TrashCave", "B" },
			{ "FloodedBasement", "C" },
			{ "PotassiumMine", "A" },
			{ "BoilingGrave", "E" },
			{ "GrandLibrary2", "BA" },
			{ "SunkenLabyrinth", "G" },
			{ "MachineFortress", "G" },
			{ "DarkHypostyle", "D" },
			{ "TombOfSimulacrum", "G" },
			{ "DreamAll", "AD" }
		};
		static Dictionary<string, List<string>> darkOglerRooms = new Dictionary<string, List<string>>()
		{
			{ "SunkenLabyrinth", new List<string> { "C", "O", "R" }  },
			{ "MachineFortress", new List<string> { "Q" } },
			{ "DarkHypostyle", new List<string> { "M" } },
			{ "TombOfSimulacrum", new List<string> { "E", "AM", "AJ", "AI", "AA", "V", "S", "L" } }
		};
		static Entity currBoss;
		static EntityDropHandler dropHandler;

		static string path = Application.dataPath + @"\extra2dew\modstuff\" + "options.txt";
		//static string path= @"C:\Users\Awesome\Desktop\" + "options.txt";

		public static void Initialization()
		{
			disableExtraCutscenes = ReadOptsFromFile("extraCutscenes");
			disableIntroAndOutro = ReadOptsFromFile("introAndOutro");
			betterHealthMeter = ReadOptsFromFile("betterHealthMeter");
			darkOglerDropFix = ReadOptsFromFile("darkOglerDropFix");
			fastTransitions = ReadOptsFromFile("fastTransitions");
			loadAllRooms = ReadOptsFromFile("loadAllRooms");

			// Event subscriptions
			GameStateNew.OnRoomChange += SkipDelays;
			GameStateNew.OnSceneLoad += SkipOutro;
			GameStateNew.OnEnemyKill += SkipDelays;
			GameStateNew.OnSceneLoad += BetterHealthMeter;
			GameStateNew.OnRoomChange += DarkOglerDropFix;
			GameStateNew.OnEnemyKill += DarkOglerDropFix;
			GameStateNew.OnPlayerSpawn += FastTransitions;
			GameStateNew.OnSceneLoad += LoadAllRooms;
		}

		public static void WriteOptsToFile(string key, int value)
		{
			try
			{
				// If key exists, replace it
				if (File.Exists(path))
				{
					string allText = File.ReadAllText(path);

					if (allText.Contains(key))
					{
						int oldValue = Convert.ToInt32(ReadOptsFromFile(key));
						allText = allText.Replace(key + ": " + oldValue, key + ": " + value);
						File.WriteAllText(path, allText);
						return;
					}
				}

				// Write key
				using (StreamWriter sw = new StreamWriter(path, true))
				{
					sw.WriteLine(key + ": " + value);
					sw.Close();
				}
			}
			catch (Exception ex)
			{
				PlayerPrefs.SetString("test", "ERROR: Failed to write key '" + key + "' to options file.\n\n" + ex.ToString());
			}
		}

		static bool ReadOptsFromFile(string key)
		{
			try
			{
				// If file doesn't exist, error
				if (!File.Exists(path)) { PlayerPrefs.SetString("test", "ERROR: Mod options file could not be read because it does not exist."); return false; }

				// If file exists, read from it
				using (StreamReader sr = new StreamReader(path))
				{
					for (int i = 0; i < File.ReadAllLines(path).Length; i++)
					{
						string line = sr.ReadLine();

						// If found key, get bool from it
						if (line.Contains(key))
						{
							if (line.Contains("1") || line.Contains("true")) { return true; }
							return false;
						}
					}
				}

				return false;
			}
			catch (Exception ex)
			{
				PlayerPrefs.SetString("test", "ERROR: Failed to read key '" + key + "' from options file.\n\n" + ex.ToString());
				return false;
			}
		}

		static void SkipDelays(LevelRoom room)
		{
			// If ModOptions disables extra cutscenes, disable intro dialogue, boss spawn delays, and efcs get delays
			if (disableExtraCutscenes)
			{
				string sceneName = ModMaster.GetMapName();

				// Disable intro dialogue delay (saves 1s)
				if (sceneName == "FluffyFields" && room.RoomName == "A" && ModSaver.LoadIntFromFile("levels/FluffyFields/A", "IntroDialog-39--26") < 1)
				{
					GameObject dialog = GameObject.Find("IntroDialog");
					if (dialog != null)
					{
						GameObject.Destroy(dialog.GetComponent<TouchTrigger>());
						ModSaver.SaveIntToFile("levels/FluffyFields/A", "IntroDialog-39--26", 1);
					}
					return;
				}

				// Disable boss spawn delay (saves 5s)
				if (bossRooms.TryGetValue(SceneManager.GetActiveScene().name, out string bossRoom))
				{
					if (room.RoomName == bossRoom)
					{
						if (sceneName == "GrandLibrary2")
						{
							if (currBoss != null) { SkipPostFightDelays(SceneManager.GetActiveScene()); return; }

							Transform pauser = GameObject.Find("Fight").transform.Find("Pause");
							pauser.GetComponent<TemporaryPauseEventObserver>().FireNext(false);
							GameObject.Destroy(pauser.GetComponent<FollowerChangeEventObserver>());
							currBoss = GameObject.Find("PlayerEnt").GetComponent<Entity>(); // Abritarily setting it to Ittle. We just need this value to not be null or any other boss
							return;
						}

						// Skip delays
						TemporaryPauseEventObserver[] pausers = room.GetComponentsInChildren<TemporaryPauseEventObserver>();
						for (int i = 0; i < pausers.Length; i++)
						{
							pausers[i].FireNext(false);
							GameObject.Destroy(pausers[i]);
						}

						// Skip camera pan
						GameObject.Destroy(room.GetComponentInChildren<FollowerChangeEventObserver>());

						GameObject boss = null;
						string state = "dummy";

						// Start fight immediately
						switch (sceneName)
						{
							case "TombOfSimulacrum":
								TemporaryPauseEventObserver pauser = GameObject.Find("DialogStuff").transform.Find("Dialog").GetComponent<TemporaryPauseEventObserver>();
								pauser.FireNext(false);
								GameObject.Destroy(pauser);
								break;
							case "DreamAll":
								boss = GameObject.Find("DreamMothCocoon");
								state = "scan";
								break;
						}
						// Normal bosses
						if (boss == null)
						{
							switch (ModSaver.LoadIntFromFile("world/bosses", "nextBoss"))
							{
								case 1:
									boss = GameObject.Find("LeBiadloA");
									break;
								case 2:
									boss = GameObject.Find("LennyA");
									break;
								case 3:
									boss = GameObject.Find("CyberjennyB");
									break;
								case 4:
									boss = GameObject.Find("LeBiadloB");
									break;
								case 5:
									boss = GameObject.Find("LennyB");
									break;
								case 6:
									boss = GameObject.Find("CyberjennyC");
									break;
								default:
									boss = GameObject.Find("CyberjennyA");
									break;
							}
						}
						if (boss == null)
						{
							switch (ModSaver.LoadIntFromFile("world/bosses/mechabun", "counter"))
							{
								case 1:
									boss = GameObject.Find("MechabunB");
									break;
								case 2:
									boss = GameObject.Find("MechabunC");
									break;
								default:
									boss = GameObject.Find("MechabunA");
									break;
							}
						}
						
						if (boss != null)
						{
							currBoss = boss.GetComponent<Entity>();
							EntityStateManager.Instance.SetStateForEnt(boss, state);
						}

						return;
					}
				}

				// Skip efcs delay (saves 1s)
				if (SceneManager.GetActiveScene().name == "TombOfSimulacrum" && room.RoomName == "I")
				{
					GameObject dialog = GameObject.Find("Dialog");
					if (dialog != null) { GameObject.Destroy(dialog); }
					return;
				}
			}
		}

		static void SkipPostFightDelays(Scene scene)
		{
			if (bossRooms.TryGetValue(scene.name, out string roomName))
			{
				List<GameObject> chests = new List<GameObject>();
				GameObject portal = GameObject.Find("SecretPortal");
				GameObject bossDoor = GameObject.Find("PuzzleDoor_bosstrapdoor");

				if (scene.name == "GrandLibrary2" && ModMaster.GetMapRoom() == roomName)
				{
					Transform d8Stuff = GameObject.Find("ChestsAndPortal").transform;
					chests.Add(d8Stuff.Find("SecretChestRaft").gameObject);
					chests.Add(d8Stuff.Find("SecretChestKey").gameObject);
				}
				else { chests.Add(GameObject.Find("SecretChest")); }

				// For chests
				for (int i = 0; i < chests.Count; i++)
				{
					GameObject chest = chests[i];
					GameObject.Destroy(chest.GetComponent<DelayedEventObserver>()); // Skip appear delay
					chest.GetComponent<DisableEventObserver>().SetCompsActive(true); // Activate chest
					chest.GetComponent<EffectEventObserver>().PlayIn(); // Play appear animation
				}

				// For portal
				if (scene.name != "GrandLibrary2")
				{
					GameObject.Destroy(portal.GetComponent<DelayedEventObserver>()); // Skip appear delay
					portal.GetComponent<DisableEventObserver>().SetCompsActive(true); // Activate portal
					portal.GetComponent<EffectEventObserver>().PlayIn(); // Play appear animation
				}

				// For boss door
				GameObject.Destroy(bossDoor.GetComponent<DelayedEventObserver>()); // Skip appear delay
				bossDoor.GetComponent<DisableEventObserver>().SetCompsActive(true); // Activate boss door
				bossDoor.GetComponent<AnimationEventObserver>().PlayIn(); // Play appear animation
			}
		}

		static void SkipDelays(Entity deadEnt)
		{
			if (disableExtraCutscenes && currBoss != null && deadEnt == currBoss) { SkipPostFightDelays(SceneManager.GetActiveScene()); }
		}

		static void SkipOutro(Scene scene, bool isGameplayScene)
		{
			if (scene.name != "FluffyFields") { return; }

			// Skip outro scene
			Transform outroWarp = GameObject.Find("WinGameTrigger").transform.Find("LevelDoor");
			if (outroWarp != null) { outroWarp.GetComponent<SceneDoor>()._scene = "MainMenu"; }
		}

		static void BetterHealthMeter(Scene scene, bool isGameplayScene)
		{
			if (betterHealthMeter && isGameplayScene)
			{
				HealthMeter.BackData healthMeter = GameObject.Find("HealthMeter").GetComponent<HealthMeter>()._backData[0];
				healthMeter.maxHpMoreThan = Mathf.NegativeInfinity;
				healthMeter.texture = FindManager.Instance.FindTextureByName("HeartPaper1");
			}
		}

		static void DarkOglerDropFix(LevelRoom room)
		{
			if (darkOglerDropFix && darkOglerRooms.TryGetValue(ModMaster.GetMapName(), out List<string> roomNames))
			{
				if (roomNames.Contains(room.RoomName))
				{
					foreach (EntityDroppable oglerDropper in GameObject.FindObjectsOfType<EntityDroppable>())
					{
						if (oglerDropper.name == "OglerDeath")
						{
							oglerDropper._dropTable = null;
							oglerDropper._superTable = null;
						}
					}
				}
			}
		}

		static void DarkOglerDropFix(Entity deadEnt)
		{
			if (darkOglerDropFix && deadEnt.name == "OglerDeath")
			{
				if (dropHandler == null) { dropHandler = GameObject.Find("DropHandler").GetComponent<EntityDropHandler>(); }
				dropHandler.state.dropCounter -= 1;
			}
		}

		public static void FastTransitions(bool isRespawn)
		{
			if (fastTransitions && !isRespawn)
			{
				float roomSpeed = 0.5f;
				float levelSpeed = 0.5f;

				//RoomSwitchable roomSwitcher = GameObject.Find("PlayerEnt").GetComponent<RoomSwitchable>();
				RoomSwitchable roomSwitcher = ModMaster.GetEntComp<RoomSwitchable>("PlayerEnt");
				roomSwitcher._transitionSpeed = roomSpeed;
				roomSwitcher._levelTransitionSpeed = levelSpeed;

				foreach (SceneDoor door in Resources.FindObjectsOfTypeAll<SceneDoor>())
				{
					FadeEffectData fadeOut = door._fadeData;
					FadeEffectData fadeIn = door._fadeInData;

					if (fadeOut != null)
					{
						fadeOut._fadeOutTime = levelSpeed;
						fadeOut._fadeInTime = levelSpeed;
					}

					if (fadeIn != null)
					{
						fadeIn._fadeOutTime = levelSpeed;
						fadeIn._fadeInTime = levelSpeed;
					}
				}
			}
		}

		static void LoadAllRooms(Scene scene, bool isGameplayScene)
		{
			if (isGameplayScene)
			{
				LoadAllRooms(loadAllRooms);
			}
		}

		public static void LoadAllRooms(bool enable)
		{
			List<LevelRoom> rooms = GameObject.Find("LevelRoot").GetComponent<LevelRoot>().AllRooms();

			foreach (LevelRoom room in rooms)
			{
				if (room.RoomName != ModMaster.GetMapRoom())
				{
					room.SetRoomActive(enable, false);
				}
			}
		}
	}
}