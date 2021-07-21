using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ModStuff
{
	public class DungeonRush : E2DGameModeSingleton<DungeonRush>
    {
        void Awake()
        {
            Mode = ModeControllerNew.ModeType.DungeonRush;
            Title = "Dungeon Rush";
            QuickDescription = "Play through all the dungeons in the game, one by one.";
            Description = "Traverse all the dungeons in the game, including Dream World and The Promised Remedy! Rush through them and see if you can get the best time!"; 
			FileNames = new List<string> { "dungeons", "dungeonrush", "dungeon rush" };
            Restrictions = new List<ModeControllerNew.ModeType> { ModeControllerNew.ModeType.BossRush , ModeControllerNew.ModeType.DoorRandomizer, ModeControllerNew.ModeType.ItemRandomizer };
        }

        Dictionary<string, string> entranceRooms = new Dictionary<string, string>()
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
			{ "Deep19s", "Q" }
		};
		Dictionary<string, string> nextDungeon= new Dictionary<string, string>()
		{
			// Format: sceneName, next dungeon
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
			{ "DreamAll", "Deep19s" }
		};

		public float startingHp = 80f;

		override public void SetupSaveFile(RealDataSaver saver)
		{
            if (saver == null) return;
			string hp = startingHp.ToString();

			// Save data to file
			if (HeartRush.Instance.IsActive) { hp = (HeartRush.Instance.InitialHP + startingHp).ToString(); }

			ModMaster.SetNewGameData("start/level", "PillowFort", saver);
			ModMaster.SetNewGameData("start/door", "PillowFortInside", saver);
			ModMaster.SetNewGameData("mod/modes/DungeonRush/enabled", "1", saver);
			ModMaster.SetNewGameData("player/maxHp", hp, saver);
			ModMaster.SetNewGameData("player/hp", hp, saver);
			ModMaster.SetNewGameData("player/vars/easyMode", "0", saver);
			ModMaster.SetNewGameData("settings/easyMode", "0", saver);
			ModMaster.SetNewGameData("settings/hideCutscenes", "1", saver);
			ModMaster.SetNewGameData("settings/showTIme", "1", saver);
			ModMaster.SetNewGameData("settings/hideMapHints", "1", saver);


			Activate();
		}

		override public void Activate()
		{
			// Subscribe to events
			GameStateNew.OnDungeonComplete += OnDungeonComplete;
			GameStateNew.OnSceneLoad += OnSceneLoad;
            IsActive = true;
		}

        override public void Deactivate()
		{
			// Unsubscribe to events
			GameStateNew.OnDungeonComplete -= OnDungeonComplete;
			GameStateNew.OnSceneLoad -= OnSceneLoad;
            IsActive = false;
        }

		// Returns true if you're not in the first room and reloads the scene if you try to leave. Returns false otherwise. Takes in to account Heart Rush death if Heart Rush + Dungeon Rush
		public bool ReloadDungeon()
		{
			if (HeartRush.Instance.IsActive && HeartRush.Instance.isDead) { return false; }

			string sceneName = ModMaster.GetMapName();
			string roomName = ModMaster.GetMapRoom();
            roomName = "";
            if (roomName == entranceRooms[sceneName]) { return true; }
			return false;
		}

		// Continue to next dungeon when you beat a dungeon
		void OnDungeonComplete(Scene scene)
		{
			string dungeon = nextDungeon[scene.name];
			string spawn = dungeon + "Inside";

			// Change spawn if going to Remedy to prevent softlock with invalid spawn (doesn't include "Inside")
			if (dungeon == "Deep19s") { spawn = dungeon; }

			ModMaster.LoadScene(dungeon, spawn);
		}

		// Go to Outro when beating Remedy
		void OnSceneLoad(Scene scene, bool isGameplayScene)
		{
			if (isGameplayScene && scene.name == "FrozenCourtCaves") { ModMaster.LoadScene("Outro", "Outro"); }
		}
	}
}