using UnityEngine;
using System;
using System.Collections.Generic;

namespace ModStuff
{
	public class CommonFunctions : MonoBehaviour
	{
		public List<MusicSong> loadedSongs = new List<MusicSong>();
		public List<LevelEvent> loadedWeathers = new List<LevelEvent>();
		public MusicSong playThisSongOnLoad;
		bool randomizeOutfit;
		List<int> outfitsToRandomize = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		
		// Progress vars
		public float caveTotal = 171;
		public float chestTotal = 132;
		public float dungeonTotal = 17;
		public float cardTotal = 41;

		void Start()
		{
			AddSongs();
		}

		public void OnPlayerSpawn()
		{
			AddSongs();
			AddWeather();
			if (randomizeOutfit) { ChangeOutfit(0, true); }
		}

		// Updates list of loaded songs
		void AddSongs()
		{
			foreach (MusicSong song in Resources.FindObjectsOfTypeAll<MusicSong>())
			{
				// Don't add duplicates
				if (!loadedSongs.Contains(song))
				{
					loadedSongs.Add(song);
				}
			}
		}

		// Updates list of loaded weather effects
		void AddWeather()
		{
			foreach (LevelEvent weather in Resources.FindObjectsOfTypeAll<LevelEvent>())
			{
				// Don't add duplicates
				if (!loadedWeathers.Contains(weather))
				{
					loadedWeathers.Add(weather);
				}
			}
		}

		// Plays music
		public bool ChangeSong(string songName, float fadeTime = 0)
		{
			// Changes currently playing song
			if (loadedSongs.Count > 0)
			{
				MusicPlayer mp = Resources.FindObjectsOfTypeAll<MusicPlayer>()[0];

				// If random
				if (songName.ToLower() == "random")
				{
					int randNum = UnityEngine.Random.Range(0, loadedSongs.Count);
					mp.PlaySong(loadedSongs[randNum], 2);
					return true;
				}

				for (int i = 0; i < loadedSongs.Count; i++)
				{
					// If song is found, play it
					if (loadedSongs[i]._songClip._clip.name.ToLower() == songName.ToLower())
					{
						playThisSongOnLoad = null;
						mp.PlaySong(loadedSongs[i], fadeTime);
						playThisSongOnLoad = loadedSongs[i];
						return true;
					}
				}
			}

			return false;
		}

		// Changes Ittle's outfit
		public bool ChangeOutfit(int outfitNum, bool random = false)
		{
			// If randomize
			if (random)
			{
				if (outfitsToRandomize.Count > 0)
				{
					outfitNum = outfitsToRandomize[UnityEngine.Random.Range(0, outfitsToRandomize.Count - 1)];
					outfitsToRandomize.Remove(outfitNum);
					randomizeOutfit = true;
				}
				
				if (outfitsToRandomize.Count < 1)
				{
					outfitsToRandomize = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				}
			}
			else
			{
				outfitsToRandomize = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				randomizeOutfit = false;
			}
			
			// If valid outfit
			if (outfitNum > -1 && outfitNum < 11)
			{
				ModSaver.SaveToEnt("outfit", outfitNum);
				return true;
			}

			return false;
		}

		// Changes weather
		public bool ChangeWeather(string weatherName)
		{
			LevelEvent weatherEvent = null;
			string wantedWeather = "";

			// Determines which weather event to look for
			switch (weatherName)
			{
				case "volcano":
				case "eruption":
				case "meteor":
					wantedWeather = "VolcanoEvent";
					break;
				case "monochrome":
				case "nightcave":
				case "spider":
					wantedWeather = "CaveSpiderEvent";
					break;
				case "sun":
				case "sunny":
				case "boogaloo":
					wantedWeather = "BoogalooEvent";
					break;
				case "rain":
					wantedWeather = "ConstantRainEvent";
					break;
				case "snow":
					wantedWeather = "ConstantSnowEvent";
					break;
			}

			// Gets the weather event, if it exists
			foreach (LevelEvent weather in loadedWeathers)
			{
				if (weather.name == wantedWeather)
				{
					weatherEvent = weather;
					break;
				}
			}

			// Activates the weather event, if it exists
			if (weatherEvent != null)
			{
				LevelEventMotivator.MotivateEvent(weatherEvent);
				return true;
			}

			return false;
		}

		/*
		// Updates progress
		public void UpdateProgress(string progressType)
		{
			string path = "mod/progress";
			string name = progressType;

			if (ModSaver.HasData(path, name))
			{
				// Load data
				int caveProgress = ModSaver.LoadIntFromFile(path, "caveProgress");
				int chestProgress = ModSaver.LoadIntFromFile(path, "chestProgress");
				int dungeonProgress = ModSaver.LoadIntFromFile(path, "dungeonProgress");
				int cardProgress = ModSaver.LoadIntFromFile(path, "cardProgress");

				// Update data
				int count = ModSaver.LoadIntFromFile(path, name) + 1;
				ModSaver.SaveIntToFile(path, name, count);

				double currCount = caveProgress + chestProgress + dungeonProgress + cardProgress;
				double totalCount = caveTotal + chestTotal + dungeonTotal + cardTotal;
				double totalProgress = Math.Round((currCount / totalCount) * 100, 2);

				ModSaver.SaveFloatToFile(path, "totalProgress", (float)totalProgress);
			}
		} */

		// Update progress
		public string UpdateProgress (bool returnAll, string fileName = "")
		{
			float totalChecks = 361;

			// Read data from file
			string path = ModSaver.GetFilePath(fileName);

			// Defaults

			// Caves
			float caveProgress = ModSaver.GetDataOccurrences(path, "CaveClear");
			string cavePercent = (caveProgress / caveTotal * 100).ToString("F2") + "%";
			// Chests
			float chestProgress = ModSaver.GetDataOccurrences(path, "Dungeon_Chest") + ModSaver.GetDataOccurrences(path, "Dungeon_EnemyChest") + ModSaver.GetDataOccurrences(path, "Dungeon_PuzzleChest");
			string chestPercent = (chestProgress / chestTotal * 100).ToString("F2") + "%";
			// Dungeons
			float dungeonProgress = ModSaver.GetDataOccurrences(path, "clearedBoss");
			string dungeonPercent = (dungeonProgress / dungeonTotal * 100).ToString("F2") + "%";
			// Cards
			float cardProgress = ModSaver.GetDataOccurrences(path, "CardChest");
			string cardPercent = (cardProgress / cardTotal * 100).ToString("F2") + "%";
			// Total
			float totalProgress = caveProgress + chestProgress + dungeonProgress + cardProgress;
			string totalPercent = (totalProgress / totalChecks * 100).ToString("F2") + "%";

			// Strings to return
			string totalProgressTxt = "Total progress: " + totalPercent + "\n\n";
			string caveProgressTxt = "Cave progress: " + caveProgress + " / " + caveTotal + " (" + cavePercent + ")\n";
			string chestProgressTxt = "Chest progress: " + chestProgress + " / " + chestTotal + " (" + chestPercent + ")\n";
			string dungeonProgressTxt = "Dungeon progress: " + dungeonProgress + " / " + dungeonTotal + " (" + dungeonPercent + ")\n";
			string cardProgressTxt = "Card progress: " + cardProgress + " / " + cardTotal + " (" + cardPercent + ")";

			// Return strings
			string output = totalProgressTxt;

			if (returnAll) { output += caveProgressTxt += chestProgressTxt += dungeonProgressTxt += cardProgressTxt; }

			return output;
		}
	}
}