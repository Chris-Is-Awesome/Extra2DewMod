using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModStuff
{
	public static class ModeControllerNew
	{
		public enum ModeType
		{
			Vanilla,
			HeartRush,
			BossRush,
			DungeonRush,
			Expert,
			Chaos
		}

		public class ModeData
		{
			public ModeType mode;
			public ModeDataUI uiData;
			public IModeFuncs modeActivationFuncs;
			public List<string> fileNames = new List<string>();
			public int modeIndex;
			public List<ModeType> doNotAllowWithTheseModes = new List<ModeType>();

			public ModeData(ModeType modeType, ModeDataUI _uiData, IModeFuncs modeActivationfunctions = null, List<string> fileNamesForMode = null, int modeNumber = 0, List<ModeType> _doNotAllowWithTheseModes = null)
			{
				mode = modeType;
				uiData = _uiData;
				modeActivationFuncs = modeActivationfunctions;
				fileNames = fileNamesForMode;
				modeIndex = modeNumber;
				doNotAllowWithTheseModes = _doNotAllowWithTheseModes;
			}
		}

		public class ModeDataUI
		{
			public string name;
			public string description;
			public string[] difficultyNames;
			public int initialDifficulty;

			public ModeDataUI(string _name, string _description, string[] _difficultyNames = null, int _initialDifficulty = 0)
			{
				name = _name;
				description = _description;
				difficultyNames = _difficultyNames;
				initialDifficulty = _initialDifficulty;
			}
		}

		public static List<ModeData> allModes = new List<ModeData>()
		{
			// Heart Rush
			new ModeData(ModeType.HeartRush, // Mode Type
				new ModeDataUI("Heart Rush", // Mode name (for UI)
					"You start with a lot of HP, but every hit you take reduces your maximum HP. Getting a crayon adds some to your max HP. Run out of HP and it's game over!", // Mode description (for UI)
					new string[] { "I Cannot See", "Very Easy", "Easy", "Normal", "Hard", "Very Hard", "ReallyJoel's Dad" }, // Mode difficulty names (for UI)
					3), // Initial difficulty level (for UI)
				(IModeFuncs)HeartRush.Instance, // Mode start functions
				new List<string> { "hearts", "heartrush", "heart rush" }, // Mode file names (for activating via file name)
				0), // Mode selection index #

			// Boss Rush
			new ModeData(ModeType.BossRush, // Mode Type
				new ModeDataUI("Boss Rush", // Mode name (for UI)
					"Face a gauntlet of all bosses in the game one after another! You will get each dungeon's item as you progress, so you get progressively stronger with each boss. But be careful, there's no healing!", // Mode description (for UI)
					new string[] { "I Cannot See", "Very Easy", "Easy", "Normal", "Hard", "Very Hard", "ReallyJoel's Dad" }, // Mode difficulty names (for UI)
					3), // Initial difficulty level (for UI)
				(IModeFuncs)BossRush.Instance, // Mode start functions
				new List<string> { "bosses", "bossrush", "boss rush" }, // Mode file names (for activating via file name)
				1, // Mode selection index #
				new List<ModeType> { ModeType.DungeonRush }), // Modes to not allow with this mode

			// Dungeon Rush
			new ModeData(ModeType.DungeonRush, // Mode Type
				new ModeDataUI("Dungeon Rush", // Mode name (for UI)
					"Traverse all the dungeons in the game, including Dream World and The Promised Remedy! Rush through them and see if you can get the best time!", // Mode description (for UI)
					new string[] { "I Cannot See", "Very Easy", "Easy", "Normal", "Hard", "Very Hard", "ReallyJoel's Dad" }, // Mode difficulty names (for UI)
					3), // Initial difficulty level (for UI)
				(IModeFuncs)DungeonRush.Instance, // Mode start functions
				new List<string> { "dungeons", "dungeonrush", "dungeon rush" }, // Mode file names (for activating via file name)
				2, // Mode selection index #
				new List<ModeType> { ModeType.BossRush }), // Modes to not allow with this mode
		};
		public static List<ModeType> activeModes = new List<ModeType>();

		// Activates the given mode by mode type
		public static void ActivateMode(ModeType mode, bool activate, RealDataSaver saver, int difficulty = -1)
		{
			for (int i = 0; i < allModes.Count; i++)
			{
				if (allModes[i].mode == mode)
				{
					ModSaverNew.SaveToNewSaveFile("mod/modes/active/" + mode.ToString(), Convert.ToInt32(activate).ToString(), saver);
					if (difficulty >= 0) { ModSaverNew.SaveToNewSaveFile("mod/modes/active/" + mode.ToString() + "/difficulty", difficulty.ToString(), saver); }
					allModes[i].modeActivationFuncs.Initialize(activate, saver, difficulty);

					if (activate && !activeModes.Contains(mode))
					{
						// Subscribe to events
						GameStateNew.OnSceneLoad += OnSceneLoad;

						// Add mode to activeModes list
						activeModes.Add(mode);
						return;
					}

					if (!activate && activeModes.Contains(mode))
					{
						// Deactive mode
						DeactivateMode(mode);
						return;
					}
				}
			}

			// If vanilla file loaded, make it vanilla
			DeactivateModes();
		}

		// Activates the given mode(s) by file name
		public static void ActivateMode(string fileName, bool activate, RealDataSaver saver)
		{
			char[] separators = new char[] { ' ', '/', '&' };
			string[] modeNames = fileName.ToLower().Split(separators);
			List<ModeData> modesToActivate = new List<ModeData>();

			// Gets mode(s) to activate
			for (int i = 0; i < allModes.Count; i++)
			{
				for (int j = 0; j < modeNames.Length; j++)
				{
					ModeData mode = GetMode(modeNames[j]);

					if (mode != null && allModes[i] == mode)
					{
						modesToActivate.Add(mode);
						break;
					}
				}
			}

			// Activates each mode
			if (modesToActivate.Count > 0 && CanActivateMode(modesToActivate))
			{
				for (int i = 0; i < modesToActivate.Count; i++)
				{
					ActivateMode(modesToActivate[i].mode, activate, saver, modesToActivate[i].uiData.initialDifficulty);
				}
			}
		}

		// Activates the given mode by index
		public static void ActivateMode(int index, bool activate, RealDataSaver saver)
		{
			for (int i = 0; i < allModes.Count; i++)
			{
				if (allModes[i].modeIndex == index)
				{
					ActivateMode(allModes[i].mode, activate, saver);
					return;
				}
			}

			// If vanilla
			DeactivateModes();
		}

		// Activates the given mode via file load
		public static void ResumeMode(string mode)
		{
			for (int i = 0; i < allModes.Count; i++)
			{
				if (allModes[i].mode.ToString() == mode)
				{
					allModes[i].modeActivationFuncs.Activate();
					activeModes.Add(allModes[i].mode);
				}
			}
		}

		// Deactivates all modes to return to vanilla game
		public static void DeactivateModes()
		{
			for (int i = 0; i < activeModes.Count; i++)
			{
				DeactivateMode(activeModes[i]);
			}
		}

		// Deactivates specified mode
		static void DeactivateMode(ModeType mode)
		{
			if (activeModes.Contains(mode))
			{
				for (int i = 0; i < activeModes.Count; i++)
				{
					if (activeModes[i] == mode)
					{
						allModes[i].modeActivationFuncs.Deactivate();
						activeModes.Remove(mode);
					}
				}
			}
		}

		// Returns the mode from name
		static ModeData GetMode(string name)
		{
			for (int i = 0; i < allModes.Count; i++)
			{
				if (allModes[i].fileNames != null && allModes[i].fileNames.Contains(name.ToLower())) { return allModes[i]; }
			}

			// If vanilla
			return null;
		}

		// Returns the mode from index
		static ModeData GetMode(int index)
		{
			for (int i = 0; i < allModes.Count; i++)
			{
				if (allModes[i].modeIndex == index) { return allModes[i]; }
			}

			// If vanilla
			return null;
		}

		// Returns true if the mode doesn't conflict with other active modes. False otherwise
		static bool CanActivateMode(List<ModeData> modesToAllow)
		{
			List<ModeType> allowedModes = new List<ModeType>();

			for (int i = 0; i < modesToAllow.Count; i++)
			{
				for (int j = 0; j < allModes.Count; j++)
				{
					if (allModes[j].doNotAllowWithTheseModes != null)
					{
						List<ModeType> disallowedModes = allModes[j].doNotAllowWithTheseModes;

						for (int k = 0; k < disallowedModes.Count; k++)
						{
							if (allowedModes.Contains(disallowedModes[k])) { return false; }
						}

						allowedModes.Add(modesToAllow[i].mode);
						break;
					}

					allowedModes.Add(modesToAllow[i].mode);
					break;
				}
			}

			return true;
		}

		// Deactivates all modes when loading to main menu
		static void OnSceneLoad(Scene scene, bool isGameplayScene)
		{
			if (scene.buildIndex < 2)
			{
				// Unsubscribe to events
				DeactivateModes();
				GameStateNew.OnSceneLoad -= OnSceneLoad;
			}
		}

		// Is vanilla?
		public static bool IsVanilla
		{
			get { return activeModes.Count < 1; }
		}

		// Is Heart Rush active?
		public static bool IsHeartRush
		{
			get { return activeModes.Contains(ModeType.HeartRush); }
		}

		// Is Boss Rush active?
		public static bool IsBossRush
		{
			get { return activeModes.Contains(ModeType.BossRush); }
		}

		// Is Dungeon Rush active?
		public static bool IsDungeonRush
		{
			get { return activeModes.Contains(ModeType.DungeonRush); }
		}

		// Is Expert active?
		public static bool IsExpert
		{
			get { return activeModes.Contains(ModeType.Expert); }
		}

		// Is Chaos active?
		public static bool IsChaos
		{
			get { return activeModes.Contains(ModeType.Chaos); }
		}
	}
}