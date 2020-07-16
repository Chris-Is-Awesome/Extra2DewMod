using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace ModStuff
{
	public static class ModSaverNew
	{
		#region PlayerPrefs

		// Save data to PlayerPrefs
		public static void SaveToPrefs(string prefName, object value)
		{
			switch (value)
			{
				case int _value:
					PlayerPrefs.SetInt(prefName, _value);
					break;
				case float _value:
					PlayerPrefs.SetFloat(prefName, _value);
					break;
				case string _value:
					PlayerPrefs.SetString(prefName, _value);
					break;
				default:
					ErrorHelper.SetErrorText(value.GetType().ToString() + " is not a valid type for PlayerPrefs");
					return;
			}
		}

		// Load int from PlayerPrefs
		public static int LoadFromPrefs(string prefName, int value)
		{
			return PlayerPrefs.GetInt(prefName, value);
		}

		// Load float from PlayerPrefs
		public static float LoadFromPrefs(string prefName, float value)
		{
			return PlayerPrefs.GetFloat(prefName, value);
		}

		// Load string from PlayerPrefs
		public static string LoadFromPrefs(string prefName, string value)
		{
			return PlayerPrefs.GetString(prefName, value);
		}

		// Does the specified key exist? True if it does, false if not
		public static bool HasPref(string prefName)
		{
			return PlayerPrefs.HasKey(prefName);
		}

		// Delete the specified key
		public static void DeletePref(string prefName)
		{
			if (HasPref(prefName)) { PlayerPrefs.DeleteKey(prefName); }
		}

		// Deletes all keys
		public static void DeleteAllPrefs()
		{
			PlayerPrefs.DeleteAll();
		}

		#endregion PlayerPrefs

		#region Entity

		// Saves int to player data ( only ints are supported by Entity.SetStateVariable() )
		public static void SaveToEnt(string keyPath, int value, bool doSave = false)
		{
			ModMaster.GetEntComp<Entity>("PlayerEnt").SetStateVariable(keyPath, value);
			if (doSave) { GetOwner().SaveAll(); }
		}

		public static int LoadFromEnt(string keyPath)
		{
			return ModMaster.GetEntComp<Entity>("PlayerEnt").GetStateVariable(keyPath);
		}

		#endregion Entity

		#region Save File

		// Saves a string to a new save file
		public static void SaveToNewSaveFile(string keyPath, string value, IDataSaver saver)
		{
			if (string.IsNullOrEmpty(keyPath))
			{
				ErrorHelper.SetErrorText("Argument 'keyPath' cannot be null/empty!", ErrorHelper.ErrorType.InvalidArg);
				return;
			}

			IDataSaver _saver = SaverOwner.GetSaverAndNameByPath(keyPath, saver, out string key, false);

			if (_saver != null)
			{
				_saver.SaveData(key, value);
				return;
			}

			ErrorHelper.SetErrorText("IDataSaver '_saver' is null. This should not happen!", ErrorHelper.ErrorType.NullRef);
		}

		// Saves a string to the existing save file
		public static void SaveToSaveFile(string keyPath, string value, bool doSave = true)
		{
			if (string.IsNullOrEmpty(keyPath))
			{
				ErrorHelper.SetErrorText("Argument 'keyPath' cannot be null/empty!", ErrorHelper.ErrorType.InvalidArg);
				return;
			}

			if (string.IsNullOrEmpty(value))
			{
				ErrorHelper.SetErrorText("Argument 'value' cannot be null/empty!", ErrorHelper.ErrorType.InvalidArg);
				return;
			}

			IDataSaver saver = GetSaver(keyPath);

			if (saver != null)
			{
				string key = GetKeyName(keyPath);
				saver.SaveData(key, value);

				if (doSave) { GetOwner().SaveAll(); }
				return;
			}

			ErrorHelper.SetErrorText("IDataSaver 'saver' is null. This should not happen!", ErrorHelper.ErrorType.NullRef);
		}

		// Returns a string for the data retreived from the save file
		public static string LoadFromSaveFile(string keyPath)
		{
			if (string.IsNullOrEmpty(keyPath))
			{
				ErrorHelper.SetErrorText("Argument 'keyPath' cannot be null/empty!\nReturning empty string", ErrorHelper.ErrorType.InvalidArg);
				return string.Empty;
			}

			IDataSaver saver = GetSaver(keyPath);

			if (saver != null)
			{
				string key = GetKeyName(keyPath);
				return saver.LoadData(key);
			}

			ErrorHelper.SetErrorText("IDataSaver 'saver' is null. This should not happen!\nReturning empty string", ErrorHelper.ErrorType.NullRef);
			return string.Empty;
		}

		// Returns true if key exists, false otherwise
		public static bool HasSaveKey(string keyPath)
		{
			IDataSaver saver = GetSaver(keyPath);

			if (saver != null)
			{
				string key = GetKeyName(keyPath);
				return saver.HasData(key);
			}

			ErrorHelper.SetErrorText("IDataSaver for key " + keyPath + " was not found\nReturning false", ErrorHelper.ErrorType.NullRef);
			return false;
		}

		// Returns a list of all save keys under the specified header
		public static List<string> GetSaveKeys(string keyHeader)
		{
			return MainMenu.GetSortedSaveKeys(GetOwner(), keyHeader);
		}

		// Deletes the specified save key
		public static void DeleteSaveKey(string keyPath)
		{
			IDataSaver saver = GetSaver(keyPath);

			if (saver != null)
			{
				saver.ClearValue(GetKeyName(keyPath));
				return;
			}

			ErrorHelper.SetErrorText("IDataSaver for key " + keyPath + " was not found\nReturning false", ErrorHelper.ErrorType.NullRef);
		}

		#endregion Save File

		#region Custom File

		// Writes data to custom file
		public static bool SaveToCustomFile<T>(string filePath, string keyPath, T value, bool doSerialize = false)
		{
			if (IsFilePathValid(filePath))
			{
				if (value is string)
				{
					WriteLineToFile(filePath, keyPath, value, doSerialize);
					return true;
				}

				WriteLineToFile(filePath, keyPath, value.ToString(), doSerialize);
				return true;
			}

			return false;
		}

		// Loads string from custom file
		public static string LoadFromCustomFile(string filePath, string keyPath)
		{
			return ReadLineFromFile(filePath, keyPath);
		}

		#endregion Custom File

		#region Core

		// Returns the primary SaverOwner
		public static SaverOwner GetOwner()
		{
			foreach (SaverOwner owner in Resources.FindObjectsOfTypeAll<SaverOwner>())
			{
				if (owner.name == "MainSaver") { return owner; }
			}

			return null;
		}

		// Returns IDataSaver for the given key
		public static IDataSaver GetSaver(string keyPath)
		{
			string headerName = GetHeaderName(keyPath);

			if (headerName.StartsWith("/local/") || headerName.StartsWith("/global/")) { return GetOwner().GetSaver(headerName); }

			string type = "/local/";

			List<string> globalTypes = new List<string>()
			{
				"sound/musicVol",
				"sound/soundVol",
				"extras/soundtest",
				"extras/gallery",
				"extras/secretGallery"
			};

			if (globalTypes.Contains(keyPath)) { type = "/global/"; } // If global, make it so

			return GetOwner().GetSaver(type + headerName);
		}

		// Returns the header name from a key path
		static string GetHeaderName(string keyPath)
		{
			if (!string.IsNullOrEmpty(keyPath) && keyPath.Contains("/"))
			{
				return keyPath.Remove(keyPath.LastIndexOf('/'));
			}

			ErrorHelper.SetErrorText(keyPath + " is not a valid key path\nReturning an empty string");
			return string.Empty;
		}

		// Returns the key name from a key path
		static string GetKeyName(string keyPath)
		{
			if (!string.IsNullOrEmpty(keyPath) && keyPath.Contains("/"))
			{
				return keyPath.Remove(0, keyPath.LastIndexOf('/') + 1);
			}

			ErrorHelper.SetErrorText(keyPath + " is not a valid key path\nReturning an empty string");
			return string.Empty;
		}

		// Returns true if the given file path is valid, false otherwise. Outputs a debug message if false
		static bool IsFilePathValid(string filePath)
		{
			if (!string.IsNullOrEmpty(filePath))
			{
				if (filePath.IndexOfAny(Path.GetInvalidPathChars()) < 1)
				{
					return true;
				}

				PlayerPrefs.SetString("test", "ERROR: The file path '" + filePath + "'\ncontains invalid characters.\n[ModStuff.ModSaver.SaveToCustomFile()]");
				return false;
			}

			PlayerPrefs.SetString("test", "ERROR: The file path given is null or empty.\n[ModStuff.ModSaver.SaveToCustomFile()]");
			return false;
		}

		// Writes text to a file
		static void WriteLineToFile<T>(string filePath, string key, T value, bool doSerialize = false)
		{
			if (!doSerialize)
			{
				try
				{
					// Replace key if it exists
					if (File.Exists(filePath))
					{
						string allText = File.ReadAllText(filePath);

						if (allText.Contains(key))
						{
							allText = allText.Replace(key + ": " + ReadLineFromFile(filePath, key), key + ": " + value);
							File.WriteAllText(filePath, allText);
							return;
						}
					}

					// Create key if it does not exist
					using (StreamWriter sw = new StreamWriter(filePath, true))
					{
						sw.WriteLine(key + ": " + value);
						sw.Close();
					}
				}
				catch (Exception ex)
				{
					PlayerPrefs.SetString("test", "ERROR: Something went wrong. I don't know exactly what...\n" + ex.Message + "\n[ModStuff.ModSaver.WriteLineToFile()]");
					return;
				}
			}
		}

		// Reads line from a file and returns the value as a string
		static string ReadLineFromFile(string filePath, string key)
		{
			try
			{
				if (File.Exists(filePath))
				{
					using (StreamReader sr = new StreamReader(filePath))
					{
						for (int i = 0; i < File.ReadAllLines(filePath).Length; i++)
						{
							string line = sr.ReadLine();

							if (line.Contains(key))
							{
								return line.Remove(0, line.LastIndexOf(' ') + 1);
							}
						}
					}
				}

				PlayerPrefs.SetString("test", "ERROR: The file at path '" + filePath + "'\ncould not be found. Did you forget to create it, you dorkus?\nReturning empty string.\n[ModStuff.ModSaver.ReadLineFromFile()]");
				return "";
			}
			catch (Exception ex)
			{
				PlayerPrefs.SetString("test", "ERROR: Something went wrong. I don't know exactly what...\n" + ex.Message + "\nReturning empty string.\n[ModStuff.ModSaver.ReadLineFromFile()]");
				return "";
			}
		}

		#endregion Core
	}
}
