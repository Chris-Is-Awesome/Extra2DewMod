using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ModStuff
{
	public static class ModSaver
	{
		// ********** PlayerPrefs ********** \\

		// Save int to PlayerPrefs
		public static void SaveIntToPrefs (string name, int value, bool debug = false)
		{
			// Save value to prefs
			PlayerPrefs.SetInt(name, value);

			if (debug)
			{
				PlayerPrefs.SetString("test", "Set PlayerPref " + name + " to " + PlayerPrefs.GetInt(name).ToString());
			}
		}

		// Save float to PlayerPrefs
		public static void SaveFloatToPrefs (string name, float value, bool debug = false)
		{
			// Save value to prefs
			PlayerPrefs.SetFloat(name, value);

			if (debug)
			{
				PlayerPrefs.SetString("test", "Set PlayerPref " + name + " to " + PlayerPrefs.GetFloat(name).ToString());
			}
		}

		// Save string to PlayerPrefs
		public static void SaveStringToPrefs (string name, string value, bool debug = false)
		{
			// Save value to prefs
			PlayerPrefs.SetString(name, value);

			if (debug)
			{
				PlayerPrefs.SetString("test", "Set PlayerPref " + name + " to " + PlayerPrefs.GetString(name));
			}
		}

		// Load int from PlayerPrefs
		public static int LoadIntFromPrefs(string name, bool debug = false)
		{
			int foundPref;

			// If pref exists
			if (HasPref(name))
			{
				// Load value from prefs
				foundPref = PlayerPrefs.GetInt(name);
			}
			else // If pref not found, error
			{
				return 0;
			}

			if (debug)
			{
				PlayerPrefs.SetString("test", "PlayerPref found for " + name + " is: " + foundPref.ToString());
			}

			// Return pref
			return foundPref;
		}

		// Load float from PlayerPrefs
		public static float LoadFloatFromPrefs (string name, bool debug = false)
		{
			float foundPref;

			// If pref exists
			if (HasPref(name))
			{
				// Load value from prefs
				foundPref = PlayerPrefs.GetFloat(name);
			}
			else // If pref not found, error
			{
				return 0;
			}

			if (debug)
			{
				PlayerPrefs.SetString("test", "PlayerPref found for " + name + " is: " + foundPref.ToString());
			}

			// Return pref
			return foundPref;
		}

		// Load string from PlayerPrefs
		public static string LoadStringFromPrefs (string name, bool debug = false)
		{
			string foundPref;

			// If pref exists
			if (HasPref(name))
			{
				// Load value from prefs
				foundPref = PlayerPrefs.GetString(name);
			}
			else // If pref not found, error
			{
				return null;
			}

			if (debug)
			{
				PlayerPrefs.SetString("test", "PlayerPref found for " + name + " is: " + foundPref.ToString());
			}

			// Return pref
			return foundPref;
		}

		public static bool HasPref (string name)
		{
			return PlayerPrefs.HasKey(name);
		}

		public static void DelPref (string name = null)
		{
			// If deleting specific pref, delete only that one
			if (!string.IsNullOrEmpty(name))
			{
				PlayerPrefs.DeleteKey(name);
				return;
			}

			// If deleting all, delete all
			PlayerPrefs.DeleteAll();
		}

		// ********** Save File ********** \\

		// Save int to entity
		public static void SaveToEnt (string name, int value, bool doSave = false, bool debug = false)
		{
			GetPlayerEnt().SetStateVariable(name, value);
			
			if (doSave) { GetOwner().SaveAll(); }

			if (debug)
			{
				PlayerPrefs.SetString("test", "Set PlayerEnt data point " + name + " to " + GetPlayerEnt().GetStateVariable(name));
			}
		}

		// Save int to file
		public static void SaveIntToFile (string headerName, string saveName, int value, bool doSave = false, bool debug = false)
		{
			if (debug)
			{
				string output = "Saved!\n";
				output += "Location: " + GetDataType(headerName) + headerName + "/\n";
				output += "Name: " + saveName + "\n";
				output += "Value: " + GetSaver(headerName).LoadInt(saveName) + " (prev) -> " + value + " (new)";
				output += "\nReload map for change to take effect!";
				PlayerPrefs.SetString("test", output);
			}
			
			// Save value
			GetSaver(headerName).SaveInt(saveName, value);
			
			if (doSave) { GetOwner().SaveAll(); }
		}

		// Save float to file
		public static void SaveFloatToFile (string headerName, string saveName, float value, bool doSave = false, bool debug = false)
		{
			if (debug)
			{
				string output = "Saved!\n";
				output += "Location: " + GetDataType(headerName) + headerName + "/\n";
				output += "Name: " + saveName + "\n";
				output += "Value: " + GetSaver(headerName).LoadFloat(saveName) + " (prev) -> " + value + " (new)";
				output += "\nReload map for change to take effect!";
				PlayerPrefs.SetString("test", output);
			}

			// Save value
			GetSaver(headerName).SaveFloat(saveName, value);
			
			if (doSave) { GetOwner().SaveAll(); }
		}

		// Save string to file
		public static void SaveStringToFile (string headerName, string saveName, string value, bool doSave = false, bool debug = false)
		{
			if (debug)
			{
				string output = "Saved!\n";
				output += "Location: " + GetDataType(headerName) + headerName + "/\n";
				output += "Name: " + saveName + "\n";
				output += "Value: " + GetSaver(headerName).LoadData(saveName) + " (prev) -> " + value + " (new)";
				output += "\nReload map for change to take effect!";
				PlayerPrefs.SetString("test", output);
			}

			// Save value
			GetSaver(headerName).SaveData(saveName, value);
			
			if (doSave) { GetOwner().SaveAll(); }
		}

		// Save bool to file
		public static void SaveBoolToFile (string headerName, string saveName, bool value, bool doSave = false, bool debug = false)
		{
			if (debug)
			{
				string output = "Saved!\n";
				output += "Location: " + GetDataType(headerName) + headerName + "/\n";
				output += "Name: " + saveName + "\n";
				output += "Value: " + GetSaver(headerName).LoadBool(saveName) + " (prev) -> " + value + " (new)";
				output += "\nReload map for change to take effect!";
				PlayerPrefs.SetString("test", output);
			}

			// Save value
			GetSaver(headerName).SaveBool(saveName, value);
			
			if (doSave) { GetOwner().SaveAll(); }
		}

		// Load int from entity
		public static int LoadFromEnt (string name, bool debug = false)
		{
			int foundValue = GetPlayerEnt().GetStateVariable(name);

			if (debug)
			{
				PlayerPrefs.SetString("test", "Data point " + name + " on PlayerEnt is: " + foundValue);
			}

			return foundValue;
		}

		// Load int from file
		public static int LoadIntFromFile (string headerName, string saveName, bool debug = false)
		{
			int valueFound = GetSaver(headerName).LoadInt(saveName);
			
			if (debug)
			{
				string output = "Loaded!\n";
				output += "Location: " + GetDataType(headerName) + headerName + "/\n";
				output += "Name: " + saveName + "\n";
				output += "Value: " + valueFound;
				PlayerPrefs.SetString("test", output);
			}

			return valueFound;
		}

		// Load float from file
		public static float LoadFloatFromFile (string headerName, string saveName, bool debug = false)
		{
			float valueFound = GetSaver(headerName).LoadFloat(saveName);

			if (debug)
			{
				string output = "Loaded!\n";
				output += "Location: " + GetDataType(headerName) + headerName + "/\n";
				output += "Name: " + saveName + "\n";
				output += "Value: " + valueFound;
				PlayerPrefs.SetString("test", output);
			}

			return valueFound;
		}

		// Load string from file
		public static string LoadStringFromFile (string headerName, string saveName, bool debug = false)
		{
			string valueFound = GetSaver(headerName).LoadData(saveName);

			if (debug)
			{
				string output = "Loaded!\n";
				output += "Location: " + GetDataType(headerName) + headerName + "/\n";
				output += "Name: " + saveName + "\n";
				output += "Value: " + valueFound;
				PlayerPrefs.SetString("test", output);
			}

			return valueFound;
		}

		// Load bool from file
		public static bool LoadBoolFromFile (string headerName, string saveName, bool debug = false)
		{
			bool valueFound = GetSaver(headerName).LoadBool(saveName);

			if (debug)
			{
				string output = "Loaded!\n";
				output += "Location: " + GetDataType(headerName) + headerName + "/\n";
				output += "Name: " + saveName + "\n";
				output += "Value: " + valueFound;
				PlayerPrefs.SetString("test", output);
			}

			return valueFound;
		}

		// Returns true if data point exists, false if not
		public static bool HasData (string headerName, string saveName)
		{
			return GetSaver(headerName).HasData(saveName);
		}

		// Returns the number of occurrences for data point in the save file
		public static int GetDataOccurrences(string path, string saveName)
		{
			string inText = File.ReadAllText(path);
			int occurrences = 0;
			int i = 0;

			while ((i = inText.IndexOf(saveName, i)) != -1)
			{
				i += saveName.Length;
				occurrences++;
			}

			return occurrences;
		}

		/* Commented out because doesn't work
		 * 
		public static Dictionary<string, string> GetDataKeys (string header = "", string identifier = "")
		{
			Dictionary<string, string> dataKeys = new Dictionary<string, string>();

			// Get file path and text
			int fileNum = int.Parse(Regex.Match(ModSaver.GetOwner().GetUniqueLocalSavePath(), "\\d+").Value) - 1;
			string fileName = "file_" + fileNum + ".id2";
			string path = Application.persistentDataPath + "/steam/" + fileName;
			string[] lines = File.ReadAllLines(path);

			// Search for text
			if (File.Exists(path))
			{
				bool lookingForHeader = !string.IsNullOrEmpty(header);
				bool foundHeader = false;

				int count = 0;
				for (int i = 0; i < lines.Length; i++)
				{
					string line = lines[i].ToLower().Trim();

					// If line is a header
					if (line.Contains("{"))
					{
						if (lookingForHeader && !foundHeader)
						{
							if (line.Contains(header))
							{
								foundHeader = true;
							}
						}
					}

					// If line is end of header
					if (line.Contains("}"))
					{
						if (foundHeader)
						{
							foundHeader = false;
							break;
						}
					}

					// If line is a value
					if (line.Contains(":") && line.Contains(";"))
					{
						if (line.Contains(identifier))
						{
							if (!lookingForHeader || foundHeader)
							{
								char[] splitThese = { ':' };
								var charsToRemove = new string[] { " ", ";" };
								string key = line.Split(splitThese)[0];
								string value = line.Split(splitThese)[1];
								foreach (var c in charsToRemove)
								{
									key = key.Replace(c, string.Empty);
									value = value.Replace(c, string.Empty);
								}

								dataKeys.Add(key, value);
								PlayerPrefs.SetString("test", "found " + count++ + " instances");
							}
						}
					}
				}
			}

			return dataKeys;
		}
		*/

		// ********** ModSaver Core ********* \\

		// Returns saveName string with included type (local/global) if not specified
		public static string GetSaveName (string headerName, string saveName = "")
		{
			// If full name is given, don't bother getting data type
			if (headerName.Contains("/local"))
			{
				return headerName + saveName;
			}

			string type = GetDataType(headerName);

			// If data type is not null
			if (!string.IsNullOrEmpty(type))
			{
				return type + headerName + saveName;
			}

			return "";
		}

		// Returns SaverOwner
		public static SaverOwner GetOwner()
		{
			if (ModMaster.GetMapName() == "MainMenu")
			{
				return GameObject.Find("GuiFuncs").GetComponent<MainMenu>()._saver;
			}
			
			if (GameObject.Find("BaseLevelData") != null)
			{
				return GameObject.Find("BaseLevelData").GetComponent<LevelTime>()._saver;
			}

			return null;
		}

		// Returns IDataSaver for given saver, null otherwise
		public static IDataSaver GetSaver (string name)
		{
			return GetOwner().GetSaver(GetSaveName(name));
		}

		// Returns data type for Saver (local or global)
		public static string GetDataType (string name)
		{
			List<string> localDataTypes = new List<string>()
			{
				"levels",
				"start",
				"player",
				"markers",
				"dungeons",
				"dream",
				"world",
				"hideItems",
				"mod",
				"settings",
				"cards",
			};
			List<string> globalDataTypes = new List<string>()
			{
				"", // Is anything global????
			};

			// If local
			for (int i = 0; i < localDataTypes.Count; i++)
			{
				if (name.Contains(localDataTypes[i]))
				{
					return "/local/";
				}
			}

			// If global
			for (int i = 0; i < globalDataTypes.Count; i++)
			{
				if (name.Contains(globalDataTypes[i]))
				{
					return "/global/";
				}
			}

			// If neither, error
			PlayerPrefs.SetString("test", "Data point" + name + " is not a valid type??? This should never happen.\nIt's probably a missing type in one of the type lists.\n[ModStuff.ModSaver.GetDataType()]");
			return null;
		}

		// Returns PlayerEnt if it exists, null otherwise
		public static Entity GetPlayerEnt ()
		{
			// If PlayerEnt found, return it
			if (GameObject.Find("PlayerEnt") != null && GameObject.Find("PlayerEnt").GetComponent<Entity>() != null)
			{
				return GameObject.Find("PlayerEnt").GetComponent<Entity>();
			}

			// If PlayerEnt not found, error
			PlayerPrefs.SetString("test", "PlayerEnt not found. Returning null.\n[ModStuff.ModSaver.GetPlayerEnt()]");
			return null;
		}

		// Returns path to current file
		public static string GetFilePath (string fileName = "")
		{
			int fileNum = int.Parse(Regex.Match(GetOwner().GetUniqueLocalSavePath(), "\\d+").Value) - 1;
			if (string.IsNullOrEmpty(fileName)) { fileName = "file_" + fileNum + ".id2"; }
			string path = Application.persistentDataPath + "/steam/" + fileName;
			return path;
		}
	}
}