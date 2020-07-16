using UnityEngine;
using System.Collections.Generic;

namespace ModStuff
{
	public class SceneStuff
	{
		public class SceneData
		{
			public string sceneName;
			public List<string> altSceneNames = new List<string>();

			public SceneData(string name, List<string> altNames)
			{
				sceneName = name;
				altSceneNames = altNames;
			}
		}
		public class SpawnData
		{
			public string scene;
			public List<string> spawnNames = new List<string>();
			public List<string> altSpawnNames = new List<string>();

			public SpawnData(string sceneName, List<string> names, List<string> altNames)
			{
				scene = sceneName;
				spawnNames = names;
				altSpawnNames = altNames;
			}
		}

		public List<SceneData> validScenes = new List<SceneData>();
		public List<SpawnData> validSpawns = new List<SpawnData>();
		public string realSceneName = "";
		public string realSpawnName = "";

		void Awake()
		{
			GenerateSceneData();
		}

		// Returns true if scene and spawn exist, false if either don't
		public bool DoSceneAndSpawnExist(string scene, string spawn)
		{
			bool sceneExists = false;
			bool spawnExists = false;

			// Checks if scene exists
			for (int i = 0; i < validScenes.Count; i++)
			{
				SceneData data = validScenes[i];

				if (data.sceneName == scene || data.altSceneNames.Contains(scene))
				{
					sceneExists = true;
					realSceneName = data.sceneName;
					break;
				}
			}

			// If scene does not exist, error
			if (!sceneExists)
			{
				PlayerPrefs.SetString("test", "Error: Scene " + scene + " does not exist.\nThis could be a typo!");
				return false;
			}

			// Checks if spawn exists
			for (int i = 0; i < validSpawns.Count; i++)
			{
				SpawnData data = validSpawns[i];

				if (data.scene == realSceneName && (data.spawnNames.Contains(spawn) || data.altSpawnNames.Contains(spawn)))
				{
					spawnExists = true;
					
					// Gets real spawn name
					for (int j = 0; j < data.spawnNames.Count; j++)
					{
						if (data.spawnNames[j] == spawn)
						{
							realSpawnName = data.spawnNames[j];
							break;
						}
					}
				}
			}

			// If spawn does not exist, error
			if (!spawnExists)
			{
				PlayerPrefs.SetString("test", "Error: Spawn " + spawn + " does not exist.\nThis could be a typo!");
				return false;
			}

			// If both scene and spawn exist, return true
			return true;
		}

		// Makes list of scenes
		void GenerateSceneData()
		{
			validScenes = new List<SceneData>()
			{
				// Overworld
				//
				// Dungeons
				{	new SceneData("PillowFort", // Scene name
					new List<string> { "d1", "pf", "pillow" } // Alt names
				)},
				{	new SceneData("SandCastle", // Scene name
					new List<string> { "d2", "sc", "sand" } // Alt names
				)},
				{    new SceneData("ArtExhibit", // Scene name
					 new List<string> { "d3", "ae", "art" } // Alt names
				)},
				// Portal worlds
				//
				// Caves
				//
			};
		}

		// Makes list of spawn points
		void GenerateSpawnData()
		{
			// Overworld
			//
			// Dungeons
			{ new SpawnData("PillowFort", // Scene name
			  new List<string> { "PillowFortInside", "RestorePt1" }, // Spawn names
			  new List<string> { "inside", "restore" } // Alt spawn names
			 )},
			// Portal worlds
			//
			// Caves
			//
		};
	}
}