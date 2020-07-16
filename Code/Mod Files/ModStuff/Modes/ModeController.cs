using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModStuff
{
	public class ModeController : MonoBehaviour
	{
		public bool isHeartRush;
		public bool deleteHeartRushFile;
		public bool isBossRush;
		public bool bossRushRestore;
		public bool isDungeonRush;
		public bool isExpert;

		// File names
		List<string> heartRushFileNames = new List<string>() { "hearts", "heartrush" };
		List<string> bossRushFileNames = new List<string>() { "bosses", "bossrush" };
		List<string> dungeonRushFileNames = new List<string>() { "dungeons", "dungeonrush" };
		List<string> expertFileNames = new List<string>() { "expert", "master" };

		// Mode functions
		public HeartRush heartRushManager;
		public BossRush bossRushManager;
		public DungeonRush dungeonRushManager;

		void Awake()
		{
			heartRushManager = new HeartRush();
			bossRushManager = new BossRush();
			dungeonRushManager = new DungeonRush();
		}

		public void SetupGameModes(string fileName)
		{
			// Reset modes on main menu
			isHeartRush = false;
			isBossRush = false;
			isDungeonRush = false;
			isExpert = false;

			// Enable modes
			if (heartRushFileNames.Contains(fileName)) { isHeartRush = true; }
			else if (bossRushFileNames.Contains(fileName) && !fileName.Contains("restore")) { isBossRush = true; }
			else if (bossRushFileNames.Contains(fileName) && fileName.Contains("restore")) { isBossRush = true; bossRushRestore = true; }
			else if (dungeonRushFileNames.Contains(fileName)) { isDungeonRush = true; }
			else if (expertFileNames.Contains(fileName)) { isExpert = true; }
		}

		public void SetupGameModes(int mode)
		{
			// Reset modes on main menu
			isHeartRush = false;
			isBossRush = false;
			isDungeonRush = false;
			isExpert = false;

			// Enable modes
			switch (mode)
			{
				case 1:
					isHeartRush = true;
					break;
				case 2:
					isBossRush = true;
					break;
				case 3:
					isDungeonRush = true;
					break;
				case 4:
					isExpert = true;
					break;
				default:
					break;
			}
		}

		public void SetupGameModes()
		{
			//Get strings
			string HeartRushSave = ModSaver.LoadStringFromFile("mod", "isheartrush");
			string BossRushSave = ModSaver.LoadStringFromFile("mod", "isbossrush");
			string DungeonRushSave = ModSaver.LoadStringFromFile("mod", "isdungeonrush");
			string ExpertModeSave = ModSaver.LoadStringFromFile("mod", "isexpert");

			//Evaluate strings
			isHeartRush = !string.IsNullOrEmpty(HeartRushSave) && HeartRushSave == "1";
			isBossRush = !string.IsNullOrEmpty(BossRushSave) && BossRushSave == "1";
			isDungeonRush = !string.IsNullOrEmpty(DungeonRushSave) && DungeonRushSave == "1";
			isExpert = !string.IsNullOrEmpty(ExpertModeSave) && ExpertModeSave == "1";
		}

		public void SetupGameModes(IDataSaver saver)
		{
			//Get strings
			string HeartRushSave = saver.LoadData("isheartrush");
			string BossRushSave = saver.LoadData("isbossrush");
			string DungeonRushSave = saver.LoadData("isdungeonrush");
			string ExpertModeSave = saver.LoadData("isexpert");

			//Evaluate strings
			isHeartRush = !string.IsNullOrEmpty(HeartRushSave) && HeartRushSave == "1";
			isBossRush = !string.IsNullOrEmpty(BossRushSave) && BossRushSave == "1";
			isDungeonRush = !string.IsNullOrEmpty(DungeonRushSave) && DungeonRushSave == "1";
			isExpert = !string.IsNullOrEmpty(ExpertModeSave) && ExpertModeSave == "1";
		}

		public class HeartRush
		{
			// Set player's current & max HP
			public void StartOfGame(Killable playerHp)
			{
				return;
				// Sets player's max HP if not already set
				float maxHp = 120f;
				if (playerHp.MaxHp != maxHp)
				{
					playerHp.MaxHp = maxHp;
					playerHp.CurrentHp = maxHp;
				}
			}

			public static int[] InitialHP()
			{
				return new int[] { 6 * 120, 3 * 120, 2 * 120, 120, 60, 30, 1 };
			}

			// Add to player's max HP and set current HP to equal that
			public void OnCrayonPickup(Killable playerHp)
			{
				float addThisMuchHp = 16f;
				playerHp.MaxHp += addThisMuchHp;
				playerHp.CurrentHp = playerHp.MaxHp;
			}

			// When damage is taken, reduce max HP rather than just current HP
			public void DamageTaken(Killable playerHp, float dmg)
			{
				// If player is dead
				if (playerHp.MaxHp - dmg <= 0)
				{
					playerHp.CurrentHp = 0;
					OnPlayerDeath(playerHp);
				}
				// Else reduce max HP
				else
				{
					playerHp.MaxHp -= dmg;
				}
			}

			// Send to main menu
			public string defeatedPlayerPath;
			void OnPlayerDeath(Killable playerHp)
			{
				playerHp.SignalDeath();
				GameObject.Find("ModeController").GetComponent<ModeController>().deleteHeartRushFile = true;
				defeatedPlayerPath = ModSaver.LoadStringFromFile("mod", "savepath");
			}

			// Send to main menu
			public void ToMainMenu()
			{
				ModMaster.LoadScene("MainMenu", "", false, false);
			}

			// Delete file upon returning to main menu after death
			public void DeleteFile(SaverOwner _saver)
			{
				DataIOBase currentIO = DataFileIO.GetCurrentIO();
				string saveFilePath = defeatedPlayerPath;
				string thumbFilePath = saveFilePath.Remove(saveFilePath.Length - 3, 3) + "png";
				if (File.Exists(currentIO.FinalizePath(saveFilePath)))
				{
					currentIO.DeleteFile(saveFilePath);
				}
				if (File.Exists(currentIO.FinalizePath(thumbFilePath)))
				{
					currentIO.DeleteFile(thumbFilePath);
				}
				_saver.ResetLocalSaver();
				GameObject.Find("ModeController").GetComponent<ModeController>().deleteHeartRushFile = false;
			}
		}

		public class BossRush
		{
			// Enable timer, disables cutscenes, and makes boss counter text UI
			public void StartOfGame()
			{
				// Sets player's max HP if not already set
				Killable playerHp = GameObject.Find("PlayerEnt").transform.Find("Hittable").GetComponent<Killable>();
				float maxHp = 40f;
				if (playerHp.MaxHp != maxHp)
				{
					playerHp.MaxHp = maxHp;
					playerHp.CurrentHp = maxHp;
				}

				// Makes boss count UI
				Vector3 textPos = new Vector3(-0.25f, -2.7f, 0f);
				int bossCount = ModSaver.LoadIntFromFile("mod/BossRush", "bossCount") + 1;
				string text = bossCount + " / 14";
				ModMaster.MakeTextObj("BossCounter", textPos, text, 100, TextAlignment.Right);
			}

			// Used by PlayerSpawner to set starting spawn position and activate boss room
			public Vector3 GetPlayerSpawnPoint()
			{
				string sceneName = ModMaster.GetMapName();
				string spawnName = "RestorePt1";

				if (sceneName == "GrandLibrary2" && ModMaster.GetMapRoom() != "BA") { return Vector3.zero; }

				Dictionary<string, Vector3> spawnPositions = new Dictionary<string, Vector3>()
				{
					{ "PillowFort", new Vector3(33f, 0f, -5.5f) },
					{ "SandCastle", new Vector3(11f, 0f, -5.5f) },
					{ "ArtExhibit", new Vector3(48f, 0f, -5.5f) },
					{ "TrashCave", new Vector3(33f, 0f, -5.5f) },
					{ "FloodedBasement", new Vector3(71f, 0f, -5.5f) },
					{ "PotassiumMine", new Vector3(41f, 0f, -5.5f) },
					{ "BoilingGrave",  new Vector3(71f, 0f, -5.5f) },
					{ "GrandLibrary2", new Vector3(7.5f, 0f, -9f) },
					{ "SunkenLabyrinth", new Vector3(63f, 0f, -17.5f) },
					{ "MachineFortress", new Vector3(82.5f, 0f, -45f) },
					{ "DarkHypostyle", new Vector3(63f, 0f, -5.5f) },
					{ "TombOfSimulacrum", new Vector3(97.5f, 0f, -9f) },
					{ "DreamAll", new Vector3(48f, 0f, -60.5f) },
					{ "Deep19s", new Vector3(97.5f, 0f, -1f) },
				};

				// Saves data to file
				ModSaver.SaveStringToFile("start", "level", sceneName);
				ModSaver.SaveStringToFile("start", "door", spawnName, true);

				return spawnPositions[sceneName];
			}

			// Used by RoomSwitchable and PlayerRespawner to set player's facing direction on spawn/respawn
			public Vector3 GetPlayerFacingDirection()
			{
				Dictionary<string, Vector3> directions = new Dictionary<string, Vector3>()
				{
					{ "PillowFort", new Vector3(90, 0, 0) },
					{ "SandCastle", new Vector3(-90, 0, 0) },
					{ "ArtExhibit", new Vector3(90, 0, 0) },
					{ "TrashCave", new Vector3(90, 0, 0) },
					{ "FloodedBasement", new Vector3(-90, 0, 0) },
					{ "PotassiumMine", new Vector3(-90, 0, 0) },
					{ "BoilingGrave", new Vector3(-90, 0, 0) },
					{ "GrandLibrary2", new Vector3(0, 0, 180) },
					{ "SunkenLabyrinth", new Vector3(90, 0, 0) },
					{ "MachineFortress", new Vector3(0, 0, 180) },
					{ "DarkHypostyle", new Vector3(90, 0, 0) },
					{ "TombOfSimulacrum", new Vector3(0, 0, 180) },
					{ "DreamAll", new Vector3(90, 0, 0) },
					{ "Deep19s", new Vector3(0, 0, 180) },
				};

				string sceneName = ModMaster.GetMapName();
				return directions[sceneName];
			}

			// Used by RoomSwitchable to determine start position for walking animation (0.5 units away from GetPlayerSpawnPoint())
			public Vector3 GetPlayerStartWalkPosition()
			{
				string sceneName = ModMaster.GetMapName();
				Vector3 direction = GetPlayerFacingDirection();
				Vector3 endPos = GetPlayerEndWalkPosition();
				float posOffset = 1f;

				// Determine direction
				if (direction.x == 90) { return new Vector3(endPos.x - posOffset, endPos.y, endPos.z); }
				if (direction.x == -90) { return new Vector3(endPos.x + posOffset, endPos.y, endPos.z); }
				if (sceneName != "Deep19s" && direction.z == 180) { return new Vector3(endPos.x, endPos.y, endPos.z - posOffset); }
				if (sceneName != "Deep19s" && direction.z == -180) { return new Vector3(endPos.x, endPos.y, endPos.z + posOffset); }

				// If in Remedy, do not have walk animation
				return endPos;
			}

			// Used by RoomSwitchable to determine end position for walking animation (same as GetPlayerSpawnPoint()
			public Vector3 GetPlayerEndWalkPosition()
			{
				if (ModMaster.GetMapName() == "GrandLibrary2" && ModMaster.GetMapRoom() != "BA") { return Vector3.zero; }
				return GetPlayerSpawnPoint();
			}

			// Sets the item you would get in that dungeon for use against its boss
			public void SetItemsForBoss()
			{
				Dictionary<string, string> items = new Dictionary<string, string>()
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
					{ "Deep19s", "loot" },
				};

				string sceneName = ModMaster.GetMapName();

				// Write data
				if (items.TryGetValue(sceneName, out string item))
				{
					// Allow max upgrade of lv 3
					if (sceneName == "TombOfSimulacrum" && ModSaver.LoadFromEnt(item) < 3)
					{
						int level = ModSaver.LoadFromEnt(item) + 1;
						ModSaver.SaveToEnt(item, level);
					}

					// Allow max upgrade of lv 2
					else if ((sceneName == "GrandLibrary2" || sceneName == "SunkenLabyrinth" || sceneName == "MachineFortress" || sceneName == "DarkHypostyle") && ModSaver.LoadFromEnt(item) < 2)
					{
						int level = ModSaver.LoadFromEnt(item) + 1;
						ModSaver.SaveToEnt(item, level);
					}

					// Allow max upgrade of lv 1
					else if (ModSaver.LoadFromEnt(item) < 1)
					{
						int level = ModSaver.LoadFromEnt(item) + 1;
						ModSaver.SaveToEnt(item, level);
					}
				}
			}

			// Restores player's health to max in between each boss fight if it's enabled
			public void RestorePlayerHealth()
			{
				Killable playerHp = GameObject.Find("PlayerEnt").transform.Find("Hittable").GetComponent<Killable>();
				playerHp.CurrentHp = playerHp.MaxHp;
			}

			// For each boss that is defeated, load the next dungeon and save boss as defeated so it loads the next boss in next dungeon
			public void OnBossKilled(string bossName)
			{
				// Do not trigger on Passel's phases
				if (bossName == "passela") { return; }

				Dictionary<string, string> dungeons = new Dictionary<string, string>()
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
					{ "Deep19s", "Outro" },
				};

				// Save data to set what the next boss will be (only needed for first 7)
				string savePath = "world/bosses";
				string saveKey = "nextBoss";
				int nextBoss = ModSaver.LoadIntFromFile(savePath, saveKey) + 1;
				ModSaver.SaveIntToFile(savePath, saveKey, nextBoss);
				if (bossName.Contains("cyberjenny"))
				{
					savePath = "world/bosses/" + "cyberjenny"; saveKey = "counter";
					nextBoss = ModSaver.LoadIntFromFile(savePath, saveKey) + 1;
					ModSaver.SaveIntToFile(savePath, saveKey, nextBoss);
				}
				else if (bossName.Contains("lebiadlo"))
				{
					savePath = "world/bosses/" + "lebiadlo"; saveKey = "counter";
					nextBoss = ModSaver.LoadIntFromFile(savePath, saveKey) + 1;
					ModSaver.SaveIntToFile(savePath, saveKey, nextBoss);
				}
				else if (bossName.Contains("lenny"))
				{
					savePath = "world/bosses/" + "lenny"; saveKey = "counter";
					nextBoss = ModSaver.LoadIntFromFile(savePath, saveKey) + 1;
					ModSaver.SaveIntToFile(savePath, saveKey, nextBoss);
				}
				ModSaver.SaveIntToFile("mod/BossRush", "bossCount", ModSaver.LoadIntFromFile("mod/BossRush", "bossCount") + 1);

				string sceneName = ModMaster.GetMapName();
				string spawnName = "RestorePt1";

				// Loads scene
				ModMaster.LoadScene(dungeons[sceneName], spawnName);
			}
		}

		public class DungeonRush
		{
			// Enable timer and disable cutscenes
			public void StartOfGame()
			{
				// Save scene data
				string scene = ModMaster.GetMapName();
				string spawn = scene + "Inside";

				if (ModMaster.GetMapName() == "Deep19s") { spawn = scene; }

				ModSaver.SaveStringToFile("start", "level", scene);
				ModSaver.SaveStringToFile("start", "door", spawn);
			}

			// Checks if in boss room to allow loading next dungeon
			public bool CanLoadNextDungeon()
			{
				Dictionary<string, string> firstRooms = new Dictionary<string, string>()
				{
					// Format: sceneName, roomName for first room
					{ "PillowFort", "H" },
					{ "SandCastle", "G" },
					{ "ArtExhibit", "Q" },
					{ "TrashCave", "R" },
					{ "FloodedBasement", "S" },
					{ "PotassiumMine", "Q" },
					{ "BoilingGrave", "Y" },
					{ "GrandLibrary", "Y" },
					{ "GrandLibrary2", "BB" },
					{ "SunkenLabyrinth", "M" },
					{ "MachineFortress", "P" },
					{ "DarkHypostyle", "S" },
					{ "TombOfSimulacrum", "AN" },
					{ "DreamForce", "AD" },
					{ "DreamDynamite", "AU" },
					{ "DreamFireChain", "Q" },
					{ "DreamIce", "H" },
					{ "DreamAll", "B" },
					{ "Deep19s", "Q" }, // Or N?
				};

				string sceneName = ModMaster.GetMapName();
				string roomName = ModMaster.GetMapRoom();

				if (roomName == firstRooms[sceneName]) { return true; }

				return false;
			}

			// Send to next dungeon
			public void OnDungeonComplete()
			{
				Dictionary<string, string> nextDungeon = new Dictionary<string, string>()
				{
					{ "PillowFort", "SandCastle" },
					{ "SandCastle", "ArtExhibit" },
					{ "ArtExhibit", "TrashCave" },
					{ "TrashCave", "FloodedBasement" },
					{ "FloodedBasement", "PotassiumMine" },
					{ "PotassiumMine", "BoilingGrave" },
					{ "BoilingGrave", "GrandLibrary" },
					{ "GrandLibrary2", "SunkenLabyrinth" },
					{ "SunkenLabyrinth", "MachineFortress" },
					{ "MachineFortress", "DarkHypostyle" },
					{ "DarkHypostyle", "TombOfSimulacrum" },
					{ "TombOfSimulacrum", "DreamForce" },
					{ "DreamForce", "DreamDynamite" },
					{ "DreamDynamite", "DreamFireChain" },
					{ "DreamFireChain", "DreamIce" },
					{ "DreamIce", "DreamAll" },
					{ "DreamAll", "Deep19s" },
					{ "Deep19s", "Outro" },
				};

				string dungeon = nextDungeon[ModMaster.GetMapName()];
				string spawn = dungeon + "Inside";

				// Change spawn if going to Remedy to prevent softlock with invalid spawn point
				if (ModMaster.GetMapName() == "DreamAll") { spawn = dungeon; }

				ModMaster.LoadScene(dungeon, spawn);
			}
		}
	}
}