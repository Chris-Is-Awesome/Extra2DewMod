using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace ModStuff
{
	static public class ModMaster
	{
		// *************** Useful Stuff *************** \\

		// Find nested children from a parent
		static public GameObject FindNestedChild(GameObject parent, string childName, string directParent = "", bool debug = false)
		{
			string output; // Used for debug info outputting

			// If parent has children
			if (parent.transform.childCount > 0)
			{
				List<GameObject> matchingChildren = new List<GameObject>();
				Transform[] foundChildren = parent.GetComponentsInChildren<Transform>();

				// For each child
				foreach (Transform trans in foundChildren)
				{
					GameObject child = trans.gameObject;

					// If child is wanted child
					if (child.name == childName)
					{
						matchingChildren.Add(child);
					}
				}

				// If 1 child is found
				if (matchingChildren.Count == 1)
				{
					GameObject child = matchingChildren[0];

					// If debug
					if (debug)
					{
						PrintInfo("Found child '" + child.name + "' with parent '" + child.transform.parent.name + "'");
					}

					return matchingChildren[0].gameObject;
				}

				// If multiple children with same name found & directParent is given
				if (matchingChildren.Count > 1 && !string.IsNullOrEmpty(directParent))
				{
					// Fore each matching child
					foreach (GameObject child in matchingChildren)
					{
						string parentName = child.transform.parent.name;

						// If parent name is Root (meaning it's a child of a button), get the unique parent name
						if (parentName == "Root" && child.transform.parent.parent.name == directParent)
						{
							PrintInfo("Found child '" + child.name + "' with sequential parents '" + child.transform.parent.parent.name + "' -> '" + parentName + "'");
							return child;
						}

						// If child has directParent
						if (parentName == directParent)
						{
							// If debug
							if (debug)
							{
								PrintInfo("Found child '" + child.name + "' with parent '" + child.transform.parent.name + "'");
							}

							return child;
						}
					}

					// If no child has direct parent
					output = "Error: ModStuff.ModMaster.FindNestedChild()\n";
					output += "Parent '" + parent.name + "' returned " + matchingChildren.Count + " children with the name '" + childName + "'\n";
					output += "None of them have the direct parent '" + directParent + "'\n";
					output += "Returning null";
					PrintInfo(output);
					return null;
				}

				// If multiple children with same name found and directParent not given
				if (matchingChildren.Count > 1 && string.IsNullOrEmpty(directParent))
				{
					output = "Error: ModStuff.ModMaster.FindNestedChild()\nParent '" + parent.name + "' returned " + matchingChildren.Count + " children with the name '" + childName + "'\n";
					output += "directParent was not given, so there is no way to know which of these children to return\n";
					output += "Returning null";
					PrintInfo(output);
					return null;
				}

				// If wanted child not found
				output = "Error: ModStuff.ModMaster.FindNestedChild()\nChild '" + childName + "' not found for parent '" + parent.name + "'\nReturning null";
				PrintInfo(output);
				return null;
			}

			// If parent does not have children
			output = "Warning: ModStuff.ModMaster.FindNestedChild()\nNo children found for parent '" + parent.name + "'\nReturning null";
			PrintInfo(output);
			return null;
		}

		// Return closest object to another object from a list of objects
		static public GameObject FindClosestObject(GameObject closestToThis, List<GameObject> gos)
		{
			GameObject closestObj = null;
			Vector3 pos2 = closestToThis.transform.position;
			float closestDistance = Mathf.Infinity;

			// For each object in list of gos, compare distances
			foreach (GameObject go in gos)
			{
				// Get distance
				Vector3 pos1 = go.transform.position;
				float distance = Vector3.Distance(pos1, pos2);

				// If closestToThis is included in list of gos, skip it since obviously it'll be closest since it's on top of itself!
				if (distance <= 0) { continue; }
				else if (distance < closestDistance)
				{
					closestObj = go;
					closestDistance = distance;
				}
			}

			return closestObj;
		}

		// Change text used by objects (NPCs, signs, UI, etc.)
		// Protip: Use Markdown string format tags (<b>, <i>, <size>, or <color>) to have fancy text! More info: https://docs.unity3d.com/Manual/StyledText.html
		static public void SetText(GameObject textObj, string text)
		{
			ConfigString configString = null;
			Sign sign = null;

			// Find components

			// If object is NICE and has ConfigString, simply assign it. EZPZ
			if (textObj.GetComponent<ConfigString>() != null)
			{
				configString = textObj.GetComponent<ConfigString>();
			}

			// If object is NICE and has Sign, simply assign it. EZPZ
			if (textObj.GetComponent<Sign>() != null)
			{
				sign = textObj.GetComponent<Sign>();
			}

			// If object doesn't have one or either of the comps, it's a piece of shit and this is the exception
			if (configString == null || sign == null)
			{
				Transform[] children = textObj.GetComponentsInChildren<Transform>();

				if (configString == null)
				{
					foreach (Transform child in children)
					{
						if (child.GetComponent<ConfigString>() != null)
						{
							configString = child.GetComponent<ConfigString>();
							break;
						}
					}
				}

				if (sign == null)
				{
					foreach (Transform child in children)
					{
						if (child.GetComponent<Sign>() != null)
						{
							sign = child.GetComponent<Sign>();
							break;
						}
					}
				}
			}

			// If can set text
			if (configString != null && sign != null)
			{
				// Configure text
				string finalText = text.Replace('|', '\n');

				// Set text
				configString.SetString(finalText);

				// Update display
				if (sign.PlayerInRadius(sign._showRadius))
				{
					sign.Hide();
					sign.Show();
				}

				return;
			}

			string output = "";

			if (configString == null)
			{
				output = "Error: GameObject `" + textObj.name + "` does not have component `ConfigString`";
			}

			else if (sign == null)
			{
				output = "Error: GameObject `" + textObj.name + "` does not have component `Sign`";
			}

			output += "[ ModStuff.ModMaster.SetText() ]";
			PrintInfo(output);
		}

		// Create a text UI object with custom text
		static public GameObject MakeTextObj(string objName, Vector3 position, string text, int textSize = 80, TextAlignment textAlignment = TextAlignment.Center, Color? textColor = null, bool dontDestroyOnLoad = true)
		{
			// Creates object if it doesn't already exist
			if (GameObject.Find(objName) == null)
			{
				// Creates text object
				GameObject textObj = new GameObject(objName);
				textObj.transform.parent = GameObject.Find("OverlayCamera").transform;
				textObj.transform.localPosition = position;
				textObj.transform.localEulerAngles = Vector3.zero;
				textObj.transform.localScale = Vector3.one;
				textObj.layer = 9;

				// Creates TextMesh
				TextMesh textObjMesh = textObj.AddComponent<TextMesh>();
				// Gets font
				foreach (Font font in Resources.FindObjectsOfTypeAll(typeof(Font)) as Font[])
				{
					if (font.name == "Cutscene")
					{
						textObjMesh.font = font;
						textObj.GetComponent<UnityEngine.MeshRenderer>().sharedMaterial = FontMaterialMap.LookupFontMaterial(font);
						break;
					}
				}
				textObjMesh.alignment = textAlignment;
				textObjMesh.color = textColor ?? Color.white;
				textObjMesh.characterSize = 0.018f;
				textObjMesh.fontSize = textSize;
				textObjMesh.text = text;
				textObjMesh.richText = true;
				if (dontDestroyOnLoad) { UnityEngine.Object.DontDestroyOnLoad(textObj); }

				return textObj;
			}

			// Return it if it already exists
			return GameObject.Find(objName);
		}

		// Loads the given scene at the given spawn point and applies fade data
		static public void LoadScene(string sceneName, string spawnPoint, bool doSave = true, bool doFade = true, string fadeType = "circle/flash/fullscreen", Color? fadeColor = null, float fadeOutTime = 0.5f, float fadeInTime = 1.25f)
		{
			// If no fade data, load scene instantly
			if (!doFade) { Utility.LoadLevel(sceneName); return; }

			string fadeName;

			// Gets fade type
			switch (fadeType.ToLower())
			{
				case "circle":
					fadeName = "ScreenCircleWipe";
					break;
				case "fullscreen":
					fadeName = "ScreenFade";
					break;
				case "flash":
					fadeName = "AdditiveFade";
					break;
				default:
					fadeName = "ScreenCircleWipe";
					break;
			}

			// Makes fade data
			FadeEffectData fadeData = new FadeEffectData
			{
				_targetColor = fadeColor ?? Color.black,
				_faderName = fadeName,
				_useScreenPos = true,
				_fadeOutTime = fadeOutTime,
				_fadeInTime = fadeInTime,
			};

			if (doSave)
			{
				ModSaver.SaveStringToFile("start", "level", sceneName);
				ModSaver.SaveStringToFile("start", "door", spawnPoint);
			}

			SceneDoor.StartLoad(sceneName, spawnPoint, fadeData, GetSaver());
		}

		// Makes a custom FadeEffectData for use in loads
		static public FadeEffectData MakeFadeEffect(Color color, string fadeType = "circle/flash/fullscreen", float fadeOutTime = 0.5f, float fadeInTime = 1f)
		{
			string fadeName;

			// Gets fade type
			switch (fadeType.ToLower())
			{
				case "circle":
					fadeName = "ScreenCircleWipe";
					break;
				case "fullscreen":
					fadeName = "ScreenFade";
					break;
				case "flash":
					fadeName = "AdditiveFade";
					break;
				default:
					fadeName = "ScreenCircleWipe";
					break;
			}

			FadeEffectData fadeData = new FadeEffectData()
			{
				_targetColor = color,
				_faderName = fadeName,
				_fadeOutTime = fadeOutTime,
				_fadeInTime = fadeInTime
			};

			return fadeData;
		}

		// Returns component for the entity
		static public T GetEntComp<T>(string goName) where T : Component
		{
			GameObject go = GameObject.Find(goName);

			if (go != null)
			{
				T foundComp = go.GetComponent<T>();
				if (foundComp != null) { return foundComp; }

				foreach (Transform trans in go.transform)
				{
					foundComp = trans.GetComponent<T>();
					if (foundComp != null) { return foundComp; }
				}
			}

			return null;
		}

		// Update stats
		static public void UpdateStats(string statName, float valueToAdd = 1)
		{
			float value = ModSaver.LoadFloatFromPrefs(statName);
			ModSaver.SaveFloatToPrefs(statName, value + valueToAdd);
		}

		// Get map name
		static public string GetMapName()
		{
			return SceneManager.GetActiveScene().name;
		}

		// Get map spawn
		static public string GetMapSpawn()
		{
			IDataSaver levelData = ModSaver.GetSaver("start");

			// If level data is found (should always be found, otherwise we got a serious problemo!)
			if (levelData != null)
			{
				if (levelData.LoadData("door") == "StartPoint" || string.IsNullOrEmpty(levelData.LoadData("door")))
				{
					return "Default";
				}
				return levelData.LoadData("door");
			}

			// If level data is not found ?????????????????? This should never happen!
			PrintInfo("Error: IDataSaver not found. Returning null", true);
			return string.Empty;
		}

		// Get map room
		static public string GetMapRoom()
		{
			return ModSaver.LoadStringFromPrefs("CurrRoom");
		}

		// Get type of map for currently loaded scene (cave, dungeon, overworld, etc.)
		static public string GetMapType(string mapName = "")
		{
			List<string> overworldNames = new List<string>()
			{
				"FluffyFields",
				"CandyCoast",
				"FancyRuins",
				"FancyRuins2",
				"StarWoods",
				"StarWoods2",
				"SlipperySlope",
				"VitaminHills",
				"VitaminHills2",
				"VitaminHills3",
				"FrozenCourt",
				"LonelyRoad",
				"LonelyRoad2",
				"DreamWorld"
			};
			List<string> caveNames = new List<string>()
			{
				"FluffyFieldsCaves",
				"CandyCoastCaves",
				"FancyRuinsCaves",
				"StarWoodsCaves",
				"SlipperySlopeCaves",
				"VitaminHillsCaves",
				"FrozenCourtCaves",
				"LonelyRoadCaves"
			};
			List<string> portalWorldNames = new List<string>()
			{
				"Deep1",
				"Deep2",
				"Deep3",
				"Deep4",
				"Deep5",
				"Deep6",
				"Deep7",
				"Deep8",
				"Deep9",
				"Deep10",
				"Deep11",
				"Deep12",
				"Deep13",
				"Deep14",
				"Deep15",
				"Deep16",
				"Deep17",
				"Deep18",
				"Deep19",
				"Deep20",
				"Deep21",
				"Deep22",
				"Deep23",
				"Deep24",
				"Deep25",
				"Deep26"
			};
			List<string> dungeonNames = new List<string>()
			{
				"PillowFort",
				"SandCastle",
				"ArtExhibit",
				"TrashCave",
				"FloodedBasement",
				"PotassiumMine",
				"BoilingGrave",
				"GrandLibrary",
				"GrandLibrary2",
				"SunkenLabyrinth",
				"MachineFortress",
				"DarkHypostyle",
				"TombOfSimulacrum",
				"DreamForce",
				"DreamDynamite",
				"DreamFireChain",
				"DreamIce",
				"DreamAll",
				"Deep19s"
			};
			List<string> specialNames = new List<string>()
			{
				"SplashScreen", // Splash screen
				"MainMenu", // Title screen
				"Intro", // Intro cutscene
				"Outro", // Ending cutscene
				"snobow_title", // Snowboarding minigame title screen
				"snobow_level" // Snowboarding minigame gameplay
			};

			if (!string.IsNullOrEmpty(mapName))
			{
				if (overworldNames.Contains(mapName)) { return "Overworld"; }
				if (caveNames.Contains(mapName)) { return "Cave"; }
				if (portalWorldNames.Contains(mapName)) { return "Portal World"; }
				if (dungeonNames.Contains(mapName)) { return "Dungeon"; }
				if (specialNames.Contains(mapName)) { return "Special"; }
			}

			if (overworldNames.Contains(GetMapName())) { return "Overworld"; }
			if (caveNames.Contains(GetMapName())) { return "Cave"; }
			if (portalWorldNames.Contains(GetMapName())) { return "Portal World"; }
			if (dungeonNames.Contains(GetMapName())) { return "Dungeon"; }
			if (specialNames.Contains(GetMapName())) { return "Special"; }

			// If map is invalid ??????????????????????????????? This should never happen!
			PrintInfo("Error: Map name  '" + GetMapName() + "' is not a valid map??????????\nIs probably a typo in one of the lists under 'ModStuff.ModMaster.GetMapType()'.\nReturning null", true);
			return string.Empty;
		}

		// Get type of item for the given item
		static public List<string> GetItemType(string itemName)
		{
			Dictionary<string, string[]> itemTypes = new Dictionary<string, string[]>()
			{
				{ "amulet", new string[] { "progressive", "limited", "3", "in chest" } },
				{ "card", new string[] { "optional", "limited", "41" } },
				{ "cavescroll", new string[] { "optional", "in chest" } },
				{ "chain", new string[] { "progressive", "limited", "3", "in chest" } },
				{ "chestheart", new string[] { "optional", "in chest" } }, // Yellow heart (chest)
				{ "deepcavescroll", new string[] { "optional", "in chest" } },
				{ "dynamite", new string[] { "weapon", "progressive", "limited", "4", "in chest" } },
				{ "forbiddenkey", new string[] { "progressive", "limited", "4", "in chest" } },
				{ "forcewand", new string[] { "weapon", "progressive", "limited", "4", "in chest" } },
				{ "fruitapple", new string[] { "status", "drop" } },
				{ "fruitbanana", new string[] { "status", "drop" } },
				{ "fruitgrapes", new string[] { "status", "drop" } },
				{ "fruitstrawberry", new string[] { "status", "drop" } },
				{ "gallery", new string[] { "optional", "limited", "1", "global", "in chest" } },
				{ "headband", new string[] { "progressive", "limited", "3", "in chest" } },
				{ "heart", new string[] { "drop" } }, // Red heart
				{ "heart2", new string[] { "drop" } }, // Green heart (unused)
				{ "heart3", new string[] { "drop" } }, // Blue heart
				{ "heart4", new string[] { "drop" } }, // Yellow heart (drop)
				{ "heartalt", new string[] { "optional" } }, // Ice cream cone
				{ "icering", new string[] { "weapon", "progressive", "limited", "3", "in chest" } },
				{ "key", new string[] { "progressive", "limited", "73?" } },
				{ "lightningball", new string[] { "drop" } },
				{ "lockpick", new string[] { "progressive", "limited", "12", "in chest" } },
				{ "loot", new string[] { "progressive", "limited", "1", "in chest" } },
				{ "melee", new string[] { "weapon", "progressive", "limited", "3", "in chest" } },
				{ "pieceofpaper", new string[] { "optional", "limited", "20", "in chest" } },
				{ "raftpiece", new string[] { "progressive", "limited", "8", "in chest" } },
				{ "secretgallery", new string[] { "optional", "limited", "1", "global", "in chest" } },
				{ "secretshard", new string[] { "progressive", "limited", "24", "in chest" } },
				{ "soundtest", new string[] { "optional", "limited", "1", "global", "in chest" } },
				{ "suit", new string[] { "optional", "limited", "7", "in chest" } },
				{ "tome", new string[] { "progressive", "limited", "3", "in chest" } },
				{ "tracker", new string[] { "progressive", "limited", "3", "in chest" } },
			};

			// Remove any digit
			if (char.IsDigit(itemName[itemName.Length - 1])) { itemName = itemName.Remove(itemName.Length - 1); }

			// If key exists, return its types
			if (itemTypes.TryGetValue(itemName.ToLower(), out string[] types)) { return types.ToList<string>(); }

			// If key does not exist, return null
			return null;
		}

		// Get current time of day
		static public string GetCurrentTime(string timerName = "currTime", float timeStyle = 24, string timeFormat = "H:M")
		{
			LevelTime timer = LevelTime.Instance;
			float time = Mathf.Repeat(timer.GetTime(timerName) + 12, timeStyle);
			string formattedTime = StringUtility.ConvertToTime(time * 3600, timeFormat);

			return formattedTime;
		}

		// Get a texture by name of its parent object
		static public Material GetTextureByName(string parentName, int texIndex = 0)
		{
			foreach (Transform child in Resources.FindObjectsOfTypeAll<Transform>())
			{
				// If texture is found and is owned by specified parent, return it
				if (child.GetComponent<SkinnedMeshRenderer>() != null && child.parent != null && child.parent.name.ToLower().Contains(parentName.ToLower()))
				{
					return child.GetComponent<SkinnedMeshRenderer>().materials[texIndex];
				}
			}

			// If no texture found, return null
			return null;
		}

		static public void SetNewGameData(string path, string value, IDataSaver saver)
		{
			DataSaverData.DebugAddData[] newValue = new DataSaverData.DebugAddData[]
			{
				new DataSaverData.DebugAddData(),
			};

			newValue[0].path = path;
			newValue[0].value = value;
			DataSaverData.AddDebugData(saver, newValue);
		}

		static public string ScriptsPath
		{
			get
			{
				string path = Application.dataPath + "\\extra2dew\\scripts\\";
				if (!System.IO.Directory.Exists(path)) { System.IO.Directory.CreateDirectory(path); }
				return path;
			}
		}
		static public string RandomizerPath
		{
			get
			{
				string path = Application.dataPath + "\\extra2dew\\randomizer\\";
				if (!System.IO.Directory.Exists(path)) { System.IO.Directory.CreateDirectory(path); }
				return path;
			}
		}
		static public string TexturesPath
		{
			get
			{
				string path = Application.dataPath + "\\extra2dew\\textures\\";
				if (!System.IO.Directory.Exists(path)) { System.IO.Directory.CreateDirectory(path); }
				return path;
			}
		}

		// Returns list of scene names by type
		static public List<string> GetSceneNames(string sceneType)
		{
			List<string> overworld = new List<string>()
			{
				"FluffyFields",
				"CandyCoast",
				"FancyRuins",
				"FancyRuins2",
				"StarWoods",
				"StarWoods2",
				"SlipperySlope",
				"VitaminHills",
				"VitaminHills2",
				"VitaminHills3",
				"FrozenCourt",
				"LonelyRoad",
				"LonelyRoad2",
				"DreamWorld",
			};
			List<string> caves = new List<string>()
			{
				"FluffyFieldsCaves",
				"CandyCoastCaves",
				"FancyRuinsCaves",
				"StarWoodsCaves",
				"SlipperySlopeCaves",
				"VitaminHillsCaves",
				"FrozenCourtCaves",
				"LonelyRoadCaves",
			};
			List<string> portalWorlds = new List<string>()
			{
				"Deep1",
				"Deep2",
				"Deep3",
				"Deep4",
				"Deep5",
				"Deep6",
				"Deep7",
				"Deep8",
				"Deep9",
				"Deep10",
				"Deep11",
				"Deep12",
				"Deep13",
				"Deep14",
				"Deep15",
				"Deep16",
				"Deep17",
				"Deep18",
				"Deep19",
				"Deep19s",
				"Deep20",
				"Deep21",
				"Deep22",
				"Deep23",
				"Deep24",
				"Deep25",
				"Deep26",
			};
			List<string> dungeons = new List<string>()
			{
				"PillowFort",
				"SandCastle",
				"ArtExhibit",
				"TrashCave",
				"FloodedBasement",
				"PotassiumMine",
				"BoilingGrave",
				"GrandLibrary",
				"GrandLibrary2",
				"SunkenLabyrinth",
				"MachineFortress",
				"DarkHypostyle",
				"TombOfSimulacrum",
				"DreamForce",
				"DreamDynamite",
				"DreamFireChain",
				"DreamIce",
				"DreamAll"
			};
			List<string> special = new List<string>()
			{
				"SplashScreen", // Splash screen
				"MainMenu", // Title screen
				"Intro", // Intro cutscene
				"Outro", // Ending cutscene
				"snobow_title", // Snowboarding minigame title screen
				"snobow_level", // Snowboarding minigame gameplay
			};

			if (sceneType == "all")
			{
				List<string> all = new List<string>();

				// Adds overworld
				for (int i = 0; i < overworld.Count; i++)
				{
					all.Add(overworld[i]);
				}

				// Adds caves
				for (int i = 0; i < caves.Count; i++)
				{
					all.Add(caves[i]);
				}

				// Adds portal worlds
				for (int i = 0; i < portalWorlds.Count; i++)
				{
					all.Add(portalWorlds[i]);
				}

				// Adds dungeons
				for (int i = 0; i < dungeons.Count; i++)
				{
					all.Add(dungeons[i]);
				}

				// Adds special
				for (int i = 0; i < special.Count; i++)
				{
					all.Add(special[i]);
				}

				return all;
			}
			else if (sceneType == "overworld") { return overworld; }
			else if (sceneType == "caves") { return caves; }
			else if (sceneType == "portalWorlds") { return portalWorlds; }
			else if (sceneType == "dungeons") { return dungeons; }
			else if (sceneType == "special") { return special; }

			PrintInfo("Error: 'ModMaster.GetSceneNames()' was given '" + sceneType + "' which is an invalid argument.\nValid types are: 'overworld', 'caves', 'portalWorlds', 'dungeons', or 'special'.\nReturning null");
			return null;
		}

		// Returns list of room names
		static public List<string> GetRoomNames()
		{
			List<string> rooms = new List<string>()
			{
				"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
			};

			return rooms;
		}

		// Get debugmenu info message
		static public string GetDebugMenuHelp()
		{
			return "[DEBUG MENU] Enter: Activate command - Home/End: Move the cursor - \n" +
					"PgUp/PgDown: Browse the commands history - Ctrl+C/Ctrl+V: Copy/Paste from\n" +
					"clipboard - Ctrl+H: Clear command history\n\n";
		}

		// Write info messages to console
		static public string GetCommandHelp(string description, string arguments = "", string usage = "", string values = "")
		{
			string output = description;

			if (!string.IsNullOrEmpty(arguments))
			{
				output += "\n\nArguments:\n" + arguments;
			}

			if (!string.IsNullOrEmpty(usage))
			{
				output += "\n\nUsage:\n" + usage;
			}

			if (!string.IsNullOrEmpty(values))
			{
				output += "\n\nValues:\n" + values;
			}

			return output;
		}

		// Shortcut for handling debug messages to PlayerPrefs.SetString("test") or to output to console
		static public void PrintInfo(string info, bool doShow = false)
		{
			// If errorMessage given and do not show in console
			if (!doShow && !string.IsNullOrEmpty(info))
			{
				// Save
				PlayerPrefs.SetString("test", info);
			}

			// If error message given
			else if (doShow && !string.IsNullOrEmpty(info))
			{
				DebugCommands debugger = GameObject.Find("Debugger").GetComponent<DebugCommands>();
				debugger.OutputText(info);
			}
		}

		// Returns SaverOwner
		public static SaverOwner GetSaver()
		{
			if (ModMaster.GetMapName() == "MainMenu")
			{
				return GameObject.Find("GuiFuncs").GetComponent<MainMenu>()._saver;
			}

			if (GameObject.Find("BaseLevelData") != null)
			{
				return GameObject.Find("BaseLevelData").GetComponent<LevelTime>()._saver;
			}

			return null;
		}

		// Returns the distance between two points
		public static float GetDistanceFromObj(GameObject go1, GameObject go2)
		{
			return Vector3.Distance(go1.transform.position, go2.transform.position);
		}
	}
}