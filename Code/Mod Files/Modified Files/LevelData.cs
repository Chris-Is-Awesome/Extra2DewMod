using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ModStuff;

public class LevelData : MonoBehaviour
{
	[SerializeField]
	string _levelName;

	[SerializeField]
	LevelRoom _roomPrefab;

	[SerializeField]
	List<Tileset> _tilesets;

	[SerializeField]
	DataSaveLink _saveLink;

	[SerializeField]
	bool _fillRooms;

	[SerializeField]
	bool _makeUnityColliders;

	[SerializeField]
	bool _allowFastLoad;

	[SerializeField]
	bool _alwaysReload;

	static LevelData savedData;

	// Added
	void Start ()
	{
		// Invoke OnSceneLoad events
		GameStateNew.OnSceneLoaded(SceneManager.GetActiveScene(), true);
	}

	public string LevelName
	{
		get
		{
			if (Application.isPlaying && string.IsNullOrEmpty(this._levelName))
			{
				return Utility.GetCurrentSceneName();
			}
			return this._levelName;
		}
		set
		{
			this._levelName = value;
		}
	}

	public LevelRoom RoomPrefab
	{
		get
		{
			return this._roomPrefab;
		}
	}

	public DataSaveLink SaveLink
	{
		get
		{
			return this._saveLink;
		}
	}

	public bool FillRooms
	{
		get
		{
			return this._fillRooms;
		}
	}

	public bool MakeUnityColliders
	{
		get
		{
			return this._makeUnityColliders;
		}
	}

	public bool AllowFastLoad
	{
		get
		{
			return this._allowFastLoad;
		}
	}

	public bool AlwaysReload
	{
		get
		{
			return this._alwaysReload;
		}
	}

	public List<Tileset> GetTilesets()
	{
		if (this._tilesets == null)
		{
			this._tilesets = new List<Tileset>();
		}
		return this._tilesets;
	}

	public static LevelData GetCurrentData()
	{
		if (LevelData.savedData == null)
		{
			LevelData.savedData = GameObject.FindObjectOfType<LevelData>();
		}
		return LevelData.savedData;
	}
}
