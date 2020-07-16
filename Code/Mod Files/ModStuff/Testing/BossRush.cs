using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff
{
	public class BossRush : Singleton<BossRush>, IModeFuncs
	{	
		class SpawnData
		{
			public string scene;
			public Vector3 position;
			public string direction;

			public SpawnData(string sceneName, Vector3 spawnPos, string facingDir)
			{
				scene = sceneName;
				position = spawnPos;
				direction = facingDir;
			}
		}

		List<SpawnData> spawnData = new List<SpawnData>()
		{
			new SpawnData("PillowFort", new Vector3(33f, 0f, -5.5f), "right"),
			new SpawnData("SandCastle", new Vector3(11f, 0f, -5.5f), "left"),
			new SpawnData("ArtExhibit", new Vector3(48f, 0f, -5.5f), "right"),
			new SpawnData("TrashCave", new Vector3(33f, 0f, -5.5f), "right"),
			new SpawnData("FloodedBasement", new Vector3(71f, 0f, -5.5f), "left"),
			new SpawnData("PotassiumMine", new Vector3(41f, 0f, -5.5f), "left"),
			new SpawnData("BoilingGrave", new Vector3(71f, 0f, -5.5f), "left"),
			new SpawnData("GrandLibrary2", new Vector3(7.5f, 0f, -9f), "up"),
			new SpawnData("SunkenLabyrinth", new Vector3(63f, 0f, -17.5f), "right"),
			new SpawnData("MachineFortress", new Vector3(82.5f, 0f, -45f), "up"),
			new SpawnData("DarkHypostyle", new Vector3(63f, 0f, -5.5f), "right"),
			new SpawnData("TombOfSimulacrum", new Vector3(97.5f, 0f, -9f), "up"),
			new SpawnData("DreamAll", new Vector3(48f, 0f, -60.5f), "right"),
			new SpawnData("Deep19s", new Vector3(97.5f, 0f, -1f), "up")
		};
		Dictionary<string, string> itemOrder = new Dictionary<string, string>()
		{
			{ "SandCastle", "forcewand" },
			{ "ArtExhibit", "dynamite" },
			{ "TrashCave", "melee" },
			{ "FloodedBasement", "headband" },
			{ "PotassiumMine", "icering" },
			{ "BoilingGrave", "chain" },
			{ "GrandLibrary2", "melee" },
			{ "SunkenLabyrinth", "forcewand" },
			{ "MachineFortress", "dynamite" },
			{ "DarkHypostyle", "icering" },
			{ "TombOfSimulacrum", "melee" },
			{ "Deep19s", "loot" }
		};
		Dictionary<string, string> dungeonOrder = new Dictionary<string, string>()
		{
			{ "PillowFort", "SandCastle" },
			{ "SandCastle", "ArtExhibit" },
			{ "ArtExhibit", "TrashCave" },
			{ "TrashCave", "FloodedBasement" },
			{ "FloodedBasement", "PotassiumMine" },
			{ "PotassiumMine", "BoilingGrave" },
			{ "BoilingGrave", "GrandLibrary2" },
			{ "GrandLibrary2", "SunkenLabyrinth" },
			{ "SunkenLabyrinth", "MachineFortress" },
			{ "MachineFortress", "DarkHypostyle" },
			{ "DarkHypostyle", "TombOfSimulacrum" },
			{ "TombOfSimulacrum", "DreamAll" },
			{ "DreamAll", "Deep19s" },
			{ "Deep19s", "Outro" }
		};
		string sceneName;

		public float startingHp = 40;
		int difficulty = -1;

		public void Initialize(bool activate, RealDataSaver saver = null, int _difficulty = -1)
		{
			if (activate && saver != null)
			{
				string hp = startingHp.ToString();

				// Save data to file
				if (ModeControllerNew.IsHeartRush) { hp = (HeartRush.Instance.startingHp + startingHp).ToString(); }

				ModMaster.SetNewGameData("start/level", "PillowFort", saver);
				ModMaster.SetNewGameData("start/door", "RestorePt1", saver);
				ModMaster.SetNewGameData("player/maxHp", hp, saver);
				ModMaster.SetNewGameData("player/hp", hp, saver);
				ModMaster.SetNewGameData("player/vars/easyMode", "0", saver);
				ModMaster.SetNewGameData("settings/easyMode", "0", saver);
				ModMaster.SetNewGameData("settings/hideCutscenes", "1", saver);
				ModMaster.SetNewGameData("settings/showTime", "1", saver);
				ModMaster.SetNewGameData("settings/hideMapHints", "1", saver);

				difficulty = _difficulty;

				Activate();
			}
		}

		public void Activate()
		{
			// Set difficulty
			if (difficulty < 0)
			{
				string className = GetType().ToString().Split('.')[1];
				string value = ModSaverNew.LoadFromSaveFile("mod/modes/active/" + className + "/difficulty");

				if (!string.IsNullOrEmpty(value)) difficulty = int.Parse(value);
			}

			// Subscribe to events
			GameStateNew.OnSceneLoad += OnSceneLoad;
			GameStateNew.OnEnemyKill += OnEnemyKilled;
		}

		public void Deactivate()
		{
			// Unsubscribe to events
			GameStateNew.OnSceneLoad -= OnSceneLoad;
			GameStateNew.OnEnemyKill -= OnEnemyKilled;
		}

		void OnSceneLoad (Scene scene, bool isGameplayScene)
		{
			if (isGameplayScene)
			{
				// Heal player
				RestorePlayerHP();

				// Give dungeon item
				GiveDungeonItem();

				// Update boss count UI
				GameObject bossCountObj = GameObject.Find("BossCounter");
				int bossCount = ModSaver.LoadIntFromFile("mod/modes/BossRush", "bossCount");
				string bossCountText = bossCount + " / 14";

				// Create boss count UI
				if (bossCountObj == null)
				{
					Vector3 textPos = new Vector3(-0.25f, -2.7f, 0f);
					bossCountObj = ModMaster.MakeTextObj("BossCounter", textPos, bossCountText, 100, TextAlignment.Right);
				}

				bossCountObj.GetComponent<TextMesh>().text = bossCountText;
			}
		}

		void OnEnemyKilled(Entity ent)
		{
			// If boss, update boss counter
			PresenceActivator presence = ent.GetComponent<PresenceActivator>();
			if ((presence != null && presence._tag._tagId == "pres_Boss") || ent.name == "DreamMoth" || ent.name == "ThatGuy2")
			{
				// Proceed to next boss only if boss killed wasn't Passel's phases
				if (ent.name.ToLower() != "passela") { OnBossKilled(ent); }
			}
		}

		// Returns start position (start walk)
		public Vector3 GetStartPosition()
		{
			sceneName = ModMaster.GetMapName();

			// Return end position if Remedy since Remedy doesn't have a walk animation entering the boss room
			if (sceneName == "Deep19s") { return GetEndPosition(); }

			Vector3 endPos = GetEndPosition();
			float posOffset = 1;

			for (int i = 0; i < spawnData.Count; i++)
			{
				if (spawnData[i].scene == sceneName)
				{
					switch (spawnData[i].direction)
					{
						case "left":
							return new Vector3((endPos.x + posOffset), endPos.y, endPos.z);
						case "right":
							return new Vector3((endPos.x - posOffset), endPos.y, endPos.z);
						case "up":
							return new Vector3(endPos.x, endPos.y, (endPos.z - posOffset));
						case "down":
							return new Vector3(endPos.x, endPos.y, (endPos.z + posOffset));
					}
				}
			}

			return Vector3.zero;
		}

		// Returns end position (end walk)
		public Vector3 GetEndPosition()
		{
			sceneName = ModMaster.GetMapName();

			for (int i = 0; i < spawnData.Count; i++)
			{
				if (spawnData[i].scene == sceneName) { return spawnData[i].position; }
			}
			return Vector3.zero;
		}

		// Returns start facing direction
		public Vector3 GetFacingDirection()
		{
			sceneName = ModMaster.GetMapName();

			for (int i = 0; i < spawnData.Count; i++)
			{
				if (spawnData[i].scene == sceneName)
				{
					switch (spawnData[i].direction)
					{
						case "left":
							return new Vector3(-90f, 0f, 0f);
						case "right":
							return new Vector3(90f, 0f, 0f);
						case "up":
							return new Vector3(0f, 0f, 180f);
						case "down":
							return new Vector3(0f, 0f, -180f);
					}
				}
			}
			return new Vector3(90f, 0f, 0f);
		}

		// Restore player health to max in between each boss fight (if enabled)
		public void RestorePlayerHP()
		{
			Killable playerHp = ModMaster.GetEntComp<Killable>("PlayerEnt");
			playerHp.CurrentHp = playerHp.MaxHp;
		}

		// Give dungeon item for use against the boss
		public void GiveDungeonItem()
		{
			if (itemOrder.TryGetValue(sceneName, out string item))
			{
				bool canUpgrade = false;

				switch (sceneName)
				{
					case "TombOfSimulacrum":
						if (ModSaver.LoadFromEnt(item) < 3) { canUpgrade = true; }
						break;
					case "GrandLibrary2":
					case "SunkenLabyrinth":
					case "MachineFortress":
					case "DarkHypostyle":
						if (ModSaver.LoadFromEnt(item) < 2) { canUpgrade = true; }
						break;
					default:
						if (ModSaver.LoadFromEnt(item) < 1) { canUpgrade = true; }
						break;
				}

				if (canUpgrade)
				{
					int level = ModSaver.LoadFromEnt(item) + 1;
					ModSaver.SaveToEnt(item, level);
				}
			}
		}

		// For each boss that is defated, proceed to the next boss
		void OnBossKilled(Entity ent)
		{
			// Save data to set what next boss will be
			string bossName = ent.name.ToLower();
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

			// Save data to update boss count
			int bossCount = ModSaver.LoadIntFromFile("mod/modes/BossRush", "bossCount") + 1;
			ModSaver.SaveIntToFile("mod/modes/BossRush", "bossCount", bossCount);

			// Load to next boss
			ModMaster.LoadScene(dungeonOrder[sceneName], "RestorePt1", true);
		}
	}
}