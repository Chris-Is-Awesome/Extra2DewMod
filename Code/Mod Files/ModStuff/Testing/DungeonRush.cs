using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ModStuff
{
	public class DungeonRush : Singleton<DungeonRush>, IModeFuncs
	{
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
		int difficulty = -1;

		public void Initialize(bool activate, RealDataSaver saver = null, int _difficulty = -1)
		{
			if (activate && saver != null)
			{
				string hp = startingHp.ToString();

				// Save data to file
				if (ModeControllerNew.IsHeartRush) { hp = (HeartRush.Instance.startingHp + startingHp).ToString(); }

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
			GameStateNew.OnDungeonComplete += OnDungeonComplete;
			GameStateNew.OnSceneLoad += OnSceneLoad;
		}

		public void Deactivate()
		{
			// Unsubscribe to events
			GameStateNew.OnDungeonComplete -= OnDungeonComplete;
			GameStateNew.OnSceneLoad -= OnSceneLoad;
		}

		// Returns true if you're not in the first room and reloads the scene if you try to leave. Returns false otherwise. Takes in to account Heart Rush death if Heart Rush + Dungeon Rush
		public bool ReloadDungeon()
		{
			if (ModeControllerNew.IsHeartRush && HeartRush.Instance.isDead) { return false; }

			string sceneName = ModMaster.GetMapName();
			string roomName = ModMaster.GetMapRoom();

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