using UnityEngine;
using System.Collections;

namespace ModStuff
{
	public class GameState : Singleton<GameState>
	{
		// Publics
		public bool hasPlayerSpawned;
		public bool hasPlayerDied;

		// Privates
		ModeController mc;

		void Start()
		{
			if (mc == null) { mc = GameObject.Find("ModeController").GetComponent<ModeController>(); }
		}

		public void OnSceneChange(string sceneName = "", string door = "", bool debug = false)
		{
			if (debug)
			{
				bool sceneExists = !string.IsNullOrEmpty(sceneName);
				bool doorExists = !string.IsNullOrEmpty(door);
				string output = "Scene change happened!\n";

				if (sceneExists || doorExists)
				{
					output += ("Came from " + ModMaster.GetMapName() + " from spawn " + ModMaster.GetMapSpawn() + "\nGoing to " + sceneName + " at spawn " + door + "!");

					// Save level data for use by our mod
					ModSaver.SaveStringToPrefs("CurrMap", sceneName);
					ModSaver.SaveStringToPrefs("CurrSpawn", door);
				}

				PlayerPrefs.SetString("test", output);
			}

			// Check if dungeon was beaten
			if (ModSaver.LoadBoolFromFile("mod/GameState", "HasCompletedDungeon"))
			{
				OnDungeonComplete(ModSaver.LoadStringFromFile("mod/GameState", "CompletedDungeonName"), true);
			}
		}

		public void OnMapChange(string fromMap = "", string toMap = "", bool debug = false)
		{
			if (debug)
			{
				bool fromMapExists = !string.IsNullOrEmpty(fromMap);
				bool toMapExists = !string.IsNullOrEmpty(toMap);
				string output = "Map change happened in " + ModMaster.GetMapName() + "!\n";

				if (fromMapExists && toMapExists)
				{
					output += (fromMap + " -> " + toMap);
				}

				PlayerPrefs.SetString("test", output);
			}
		}

		public void OnRoomChange(string fromRoom = "", string toRoom = "", bool debug = false)
		{
			if (debug)
			{
				bool fromRoomExists = !string.IsNullOrEmpty(fromRoom);
				bool toRoomExists = !string.IsNullOrEmpty(toRoom);
				string output = "Room change happened in " + ModMaster.GetMapName() + "!\n";

				if (fromRoomExists && toRoomExists)
				{
					output += (fromRoom + " -> " + toRoom);
				}

				PlayerPrefs.SetString("test", output);
			}
		}

		public void OnDungeonComplete(string dungeon = "", bool debug = false)
		{
			if (debug)
			{
				PlayerPrefs.SetString("test", "Completed " + dungeon + "!");
			}

			// Dungeon Rush
			if (mc.isDungeonRush)
			{
				ModeController.DungeonRush drm = mc.dungeonRushManager;
				drm.OnDungeonComplete();
			}

			// Update dungeon progress
			int dungeonProgress = ModSaver.LoadIntFromFile("mod/progress", "DungeonProgress");
			ModSaver.SaveIntToFile("mod/progress", "DungeonProgress", dungeonProgress + 1);
			ModSaver.SaveBoolToFile("mod/GameState", "HasCompletedDungeon", false);
		}

		public void OnPlayerSpawn(bool isRespawn = false)
		{
			// Update CommonFunctions
			GameObject.Find("CommonFunctions").GetComponent<CommonFunctions>().OnPlayerSpawn();

			if (isRespawn)
			{
				//
			}

			DebugCommands.Instance.OnLoadNewScene();

			if (mc == null) { Start(); }

			/*
			// Universal settings
			if (!(mc.isBossRush || mc.isDungeonRush || mc.isHeartRush || mc.isExpert) || ModeControllerNew.IsVanilla)
			{
				ModSaver.SaveIntToFile("settings", "hideCutscenes", ModSaver.LoadIntFromPrefs("hideCutscenes"));
				ModSaver.SaveIntToFile("settings", "hideMapHint", ModSaver.LoadIntFromPrefs("hideMapHint"));
				ModSaver.SaveIntToFile("settings", "showTime", ModSaver.LoadIntFromPrefs("showTime"));
				ModSaver.SaveIntToFile("settings", "easyMode", ModSaver.LoadIntFromPrefs("easyMode"));

                if (ModSaver.LoadIntFromPrefs("easyMode") == 1) { ModSaver.SaveToEnt("easyMode", 1); }
			 */
		}

		public void OnPlayerDeath()
		{
			// Restart Heart Rush
			if (mc.isHeartRush)
			{
				mc.heartRushManager.ToMainMenu();
			}
		}

		public void OnGameQuit()
		{
			//ModSaver.SaveIntToFile("mod/progress", ")
		}

		public void OnItemGet(Item item)
		{
			// Get item name (eg. of current input: "Item_Tracker1(clone)", we just want "Tracker1")
			string itemName = item.gameObject.name;

			if (itemName.ToLower().Contains("(clone)")) { itemName = itemName.Remove(itemName.IndexOf('(')); }
			if (itemName.Contains("Item_")) { itemName = itemName.Substring(5); }
			else if (itemName.Contains("Dungeon_")) { itemName = itemName.Substring(8); }

			if (ModMaster.GetItemType(itemName).Contains("in chest"))
			{
				// Update chest stats
				ModMaster.UpdateStats("ChestsOpened");
			}
		}
	}
}