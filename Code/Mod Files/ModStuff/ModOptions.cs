using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff
{
	public class ModOptions : Singleton<ModOptions>
	{
		// Universal settings
		public bool showNextDungeonHint = true;
		public bool showCutscenes = true;
		public bool showTimer = false;
		public bool easierCombat = false;
		// Asset loading
		public bool prefetchAssets = true;
		public bool dynamicRefLoading = true;

		public void Initialize()
		{
			// Event subscriptions
			GameStateNew.OnSceneLoad += OnSceneLoad;
			GameStateNew.OnFileLoad += OnFileLoad;
		}

		void OnSceneLoad(Scene scene, bool isGameplayScene)
		{
			//
		}

		void OnFileLoad(bool isNew, string fileName = "", string filePath = "", RealDataSaver saver = null)
		{
			string _showNextDungeonHint = !showNextDungeonHint ? "1" : "0";
			string _showCutscenes = !showCutscenes ? "1" : "0";
			string _showTimer = showTimer ? "1" : "0";
			string _easierCombat = easierCombat ? "1" : "0";

			// Save universal settings to file
			ModMaster.SetNewGameData("settings/hideMapHint", _showNextDungeonHint, saver);
			ModMaster.SetNewGameData("settings/hideCutscenes", _showCutscenes, saver);
			ModMaster.SetNewGameData("settings/showTime", _showTimer, saver);
			ModMaster.SetNewGameData("settings/easyMode", _easierCombat, saver);
			if (easierCombat) { ModMaster.SetNewGameData("player/vars/easyMode", _easierCombat, saver); }
		}
	}
}