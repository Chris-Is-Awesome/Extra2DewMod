using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff
{
	public class BossRush : E2DGameModeSingleton<BossRush>
	{
        void Awake()
        {
            Mode = ModeControllerNew.ModeType.BossRush;
            Title = "Boss Rush";
            QuickDescription = "Fight against all bosses in the game, one by one.";
            Description = "Face a gauntlet of all bosses in the game one after another! You will get each dungeon's item as you progress, so you get progressively stronger with each boss. But be careful, there's no healing!";
			FileNames = new List<string> { "bosses", "bossrush", "boss rush" };
            Restrictions = new List<ModeControllerNew.ModeType> { ModeControllerNew.ModeType.DungeonRush, ModeControllerNew.ModeType.DoorRandomizer, ModeControllerNew.ModeType.ItemRandomizer };
        }

        class BossData
        {
            public enum StartDir { RIGHT, LEFT, UP, DOWN}

            public string scene;
            public Vector3 position;
            public StartDir direction;
            public string item;

            public BossData(string sceneName, Vector3 spawnPos, string spawnItem, StartDir facingDir)
            {
                scene = sceneName;
                position = spawnPos;
                direction = facingDir;
                item = spawnItem;
            }
        }

        List<BossData> _bossData = new List<BossData>()
        {
            new BossData("PillowFort", new Vector3(33f, 0f, -5.5f), null, BossData.StartDir.RIGHT),
            //new BossData("Deep19s", new Vector3(97.5f, 0f, -1f), "loot", BossData.StartDir.UP),
            //new BossData("DreamAll", new Vector3(48f, 0f, -60.5f), null, BossData.StartDir.RIGHT),
            new BossData("SandCastle", new Vector3(11f, 0f, -5.5f), "forcewand", BossData.StartDir.LEFT),
            new BossData("ArtExhibit", new Vector3(48f, 0f, -5.5f),  "dynamite", BossData.StartDir.RIGHT),
            new BossData("TrashCave", new Vector3(33f, 0f, -5.5f), "melee", BossData.StartDir.RIGHT),
            new BossData("FloodedBasement", new Vector3(71f, 0f, -5.5f), "headband", BossData.StartDir.LEFT),
            new BossData("PotassiumMine", new Vector3(41f, 0f, -5.5f), "icering", BossData.StartDir.LEFT),
            new BossData("BoilingGrave", new Vector3(71f, 0f, -5.5f), "chain", BossData.StartDir.LEFT),
            new BossData("GrandLibrary2", new Vector3(7.5f, 0f, -9f), "melee", BossData.StartDir.UP),
            new BossData("SunkenLabyrinth", new Vector3(63f, 0f, -17.5f), "forcewand", BossData.StartDir.RIGHT),
            new BossData("MachineFortress", new Vector3(82.5f, 0f, -45f), "dynamite", BossData.StartDir.UP),
            new BossData("DarkHypostyle", new Vector3(63f, 0f, -5.5f), "icering", BossData.StartDir.RIGHT),
            new BossData("TombOfSimulacrum", new Vector3(97.5f, 0f, -9f), "melee", BossData.StartDir.UP),
            new BossData("DreamAll", new Vector3(48f, 0f, -60.5f), null, BossData.StartDir.RIGHT),
            new BossData("Deep19s", new Vector3(97.5f, 0f, -1f), "loot", BossData.StartDir.UP)
        };

        BossData GetDataByScene(string scene)
        {
            for (int i = 0; i < _bossData.Count; i++)
            {
                if (_bossData[i].scene == scene) return _bossData[i];
            }
            return null;
        }

        int GetIndexofScene(string scene)
        {
            for (int i = 0; i < _bossData.Count; i++)
            {
                if (_bossData[i].scene == scene) return i;
            }
            return -1;
        }


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

		override public void SetupSaveFile(RealDataSaver saver)
		{
            if (saver == null) return;

			string hp = startingHp.ToString();

			// Save data to file
			if (HeartRush.Instance.IsActive) { hp = (HeartRush.Instance.InitialHP + startingHp).ToString(); }
            
			ModMaster.SetNewGameData("start/level", "PillowFort", saver);
			ModMaster.SetNewGameData("start/door", "RestorePt1", saver);
			ModMaster.SetNewGameData("player/maxHp", hp, saver);
			ModMaster.SetNewGameData("player/hp", hp, saver);
			ModMaster.SetNewGameData("player/vars/easyMode", "0", saver);
			ModMaster.SetNewGameData("settings/easyMode", "0", saver);
			ModMaster.SetNewGameData("settings/hideCutscenes", "1", saver);
			ModMaster.SetNewGameData("settings/showTime", "1", saver);
			ModMaster.SetNewGameData("settings/hideMapHints", "1", saver);

			Activate();
        }

		override public void Activate()
		{
			// Subscribe to events
			GameStateNew.OnSceneLoad += OnSceneLoad;
			GameStateNew.OnEnemyKill += OnEnemyKilled;
            IsActive = true;
        }

        override public void Deactivate()
		{
			// Unsubscribe to events
			GameStateNew.OnSceneLoad -= OnSceneLoad;
			GameStateNew.OnEnemyKill -= OnEnemyKilled;
            IsActive = false;
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
			string sceneName = ModMaster.GetMapName();

			// Return end position if Remedy since Remedy doesn't have a walk animation entering the boss room
			if (sceneName == "Deep19s") { return GetEndPosition(); }

            BossData bossData = GetDataByScene(sceneName);
            if (bossData == null) return Vector3.zero;
			Vector3 endPos = GetEndPosition();
			float posOffset = 1;


			switch (bossData.direction)
			{
				case BossData.StartDir.LEFT:
                    return new Vector3((endPos.x + posOffset), endPos.y, endPos.z);
				case BossData.StartDir.RIGHT:
					return new Vector3((endPos.x - posOffset), endPos.y, endPos.z);
				case BossData.StartDir.UP:
                    return new Vector3(endPos.x, endPos.y, (endPos.z - posOffset));
                default:
					return new Vector3(endPos.x, endPos.y, (endPos.z + posOffset));
			}
		}

		// Returns end position (end walk)
		public Vector3 GetEndPosition()
		{
            string sceneName = ModMaster.GetMapName();

            BossData bossData = GetDataByScene(sceneName);
            if (bossData == null) return Vector3.zero;
            else return bossData.position;
		}

		// Returns start facing direction
		public Vector3 GetFacingDirection()
		{
            string sceneName = ModMaster.GetMapName();

            BossData bossData = GetDataByScene(sceneName);
            BossData.StartDir dir;

            if (bossData == null) dir = BossData.StartDir.RIGHT;
            else dir = bossData.direction;
            
            switch (dir)
            {
                case BossData.StartDir.LEFT:
                    return new Vector3(-90f, 0f, 0f);
                case BossData.StartDir.RIGHT:
                    return new Vector3(90f, 0f, 0f);
                case BossData.StartDir.UP:
                    return new Vector3(0f, 0f, 180f);
                default:
                    return new Vector3(0f, 0f, -180f);
            }
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
            string sceneName = ModMaster.GetMapName();

            BossData bossData = GetDataByScene(sceneName);

            if (bossData != null && !string.IsNullOrEmpty(bossData.item))
			{
                string item = bossData.item;

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
			bool isRecurringBoss = bossName.Contains("cyberjenny") || bossName.Contains("lebiadlo") || bossName.Contains("lenny") || bossName.Contains("mechabun");
            string savePath = "world/bosses";
			string saveKey = "nextBoss";
			int nextBoss = ModSaver.LoadIntFromFile(savePath, saveKey) + 1;
			ModSaver.SaveIntToFile(savePath, saveKey, nextBoss);
			
			if (isRecurringBoss)
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
            string sceneName = ModMaster.GetMapName();
            int currentIndex = GetIndexofScene(sceneName);

            if (currentIndex + 1 == _bossData.Count) ModMaster.LoadScene("outro", "RestorePt1", true);
            else ModMaster.LoadScene(_bossData[currentIndex + 1].scene, "RestorePt1", true);
            //else MenuMapNode.FindByName(_bossData[currentIndex + 1].scene).Warp("RestorePt1");//SceneDoor.StartLoad(_bossData[currentIndex + 1].scene, "RestorePt1", null, ModMaster.GetSaver());
        }
	}
}