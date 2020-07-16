using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using ModStuff;
    

[AddComponentMenu("Ittle 2/Level/Scene door")]
public class SceneDoor : MonoBehaviour, IBC_TriggerEnterListener, IBC_CollisionEventListener, IBC_TriggerExitListener
{
	static Dictionary<string, SceneDoor.OnAwakeFunc> awakeListeners = new Dictionary<string, SceneDoor.OnAwakeFunc>();

	static List<SceneDoor> currentDoors = new List<SceneDoor>();

	[SerializeField]
	public string _scene;

	[SerializeField]
	public string _correspondingDoor;

	[SerializeField]
	Vector3 _fallbackPosition = Vector3.zero;

	[SerializeField]
	bool _hasFallbackPos;

	[SerializeField]
	SaverOwner _saver;

	[SerializeField]
	bool _saveStartPos = true;

	[SerializeField]
	public FadeEffectData _fadeData;

	[SerializeField]
	public FadeEffectData _fadeInData;

	[SerializeField]
	Vector3 _spawnOffset = -Vector3.forward;

	[SerializeField]
	string _enterAnim;

	[SerializeField]
	public BaseQuickEffect _enterEffect;

	[SerializeField]
	string _exitAnim;

	[SerializeField]
	public BaseQuickEffect _exitEffect;

	[SerializeField]
	float _moveDist = 5f;

	[SerializeField]
	bool _moveFromCenter;

	[SerializeField]
	bool _markerOnly;

	[SerializeField]
	bool _saveExitScene;

	BC_Collider betterCollider;

	float coolDown;

	bool exited = true;

	public static SceneDoor GetDoorForName(string name)
	{
		for (int i = 0; i < SceneDoor.currentDoors.Count; i++)
		{
			if (SceneDoor.currentDoors[i].name == name)
			{
				return SceneDoor.currentDoors[i];
			}
		}
		return null;
	}

	public static void RegisterListener(string name, SceneDoor.OnAwakeFunc func)
	{
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		SceneDoor.OnAwakeFunc value;
		if (!SceneDoor.awakeListeners.TryGetValue(name, out value))
		{
			value = func;
			SceneDoor.awakeListeners.Add(name, value);
		}
		else
		{
			SceneDoor.awakeListeners[name] = func;
		}
	}

	public static void ClearListeners()
	{
		SceneDoor.awakeListeners.Clear();
	}

	void Awake()
	{
		SceneDoor.currentDoors.Add(this);
		if (string.IsNullOrEmpty(this._correspondingDoor))
		{
			this._correspondingDoor = base.name;
		}
		if (!this._markerOnly)
		{
			this.betterCollider = PhysicsUtility.RegisterColliderEvents(base.gameObject, this);
		}
		SceneDoor.OnAwakeFunc onAwakeFunc;
		if (SceneDoor.awakeListeners.TryGetValue(base.name, out onAwakeFunc))
		{
			this.exited = false;
			SceneDoor.awakeListeners.Clear();
			if (onAwakeFunc != null)
			{
				onAwakeFunc(this);
			}
		}
	}

	void OnDestroy()
	{
		if (this.betterCollider != null)
		{
			this.betterCollider.UnregisterEventListener(this);
		}
		SceneDoor.currentDoors.Remove(this);
	}

	static void CheckAndBuild(Entity player, Camera cam, TileMeshRefOwner refOwner, OverlayFader.OnDoneFunc onFadeIn)
	{
		if (player != null && refOwner != null)
		{
			UnityEngine.Debug.Log("building meshes");
			refOwner.StartCoroutine(refOwner.Apply(cam, player.WorldPosition, 15, delegate
			{
				if (onFadeIn != null)
				{
					onFadeIn();
				}
			}));
		}
	}

	static void CheckStartIn(Entity player, SceneDoor target)
	{
		if (player != null && target != null)
		{
			RoomSwitchable component = player.GetComponent<RoomSwitchable>();
			Vector3 position = target.transform.position;
			Vector3 worldPosition = player.WorldPosition;
			worldPosition.x = position.x;
			worldPosition.z = position.z;
			player.WorldPosition = worldPosition;
			Vector3 inDir = -target.transform.forward;
			Vector3 to = target.transform.TransformPoint(target._spawnOffset);
			component.FinishLevelTransition(position, to, inDir, target._exitAnim);
			EffectFactory.Instance.PlayQuickEffect(target._exitEffect, component.transform.position, component.transform.forward, null);
		}
	}

	public void SaveStartPos(SaverOwner saver = null, string wantedDoor = null, string wantedScene = null)
	{
		if (saver == null)
		{
			saver = this._saver;
		}
		if (string.IsNullOrEmpty(wantedDoor))
		{
			wantedDoor = this._correspondingDoor;
		}
		if (string.IsNullOrEmpty(wantedScene))
		{
			wantedScene = this._scene;
		}
		if (saver != null)
		{
			if (this._saveStartPos)
			{
				IDataSaver localSaver = saver.LocalStorage.GetLocalSaver("start");
				localSaver.SaveData("level", wantedScene);
				localSaver.SaveData("door", wantedDoor);
			}
			if (this._saveExitScene)
			{
				IDataSaver localSaver2 = saver.LocalStorage.GetLocalSaver("exit");
				localSaver2.SaveData("level", Utility.GetCurrentSceneName());
				localSaver2.SaveData("door", base.name);
			}
			else
			{
				saver.LocalStorage.ClearLocalSaver("exit");
			}
		}
	}

	public void DoLoad()
	{
		Entity player = null;
		Camera playerCam = null;
		PlayerController playerCont = null;
		TileMeshRefOwner refOwner = null;
		SceneDoor targetDoor = null;
		PlayerSpawner.DoSpawnFunc onDoSpawn = null;
		Vector3 spawnDir = base.transform.forward;
		Vector3 defSpawnPos = this._fallbackPosition;
		bool hasFallbackPos = this._hasFallbackPos;
		string wantedDoor = this._correspondingDoor;
		FadeEffectData fadeData = this._fadeInData ?? this._fadeData;
		OverlayFader.OnStartFunc onStartFade = null;
		OverlayFader.OnDoneFunc startFadeIn = null;
		startFadeIn = delegate()
		{
			startFadeIn = null;
			if (onStartFade != null)
			{
				if (targetDoor != null)
				{
					onStartFade(new Vector3?(SceneDoor.WorldToScreen(targetDoor.transform.position)));
				}
				else
				{
					onStartFade(null);
				}
			}
			if (player != null)
			{
				BC_Collider component = player.GetComponent<BC_Collider>();
				if (component != null)
				{
					component.enabled = true;
				}
				try
				{
					RoomSwitchable component2 = player.GetComponent<RoomSwitchable>();
					if (component2 != null)
					{
						component2.SendStartEvent();
					}
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception, player);
				}
			}
			if (playerCont != null)
			{
				playerCont.SetEnabled(true);
			}
			SceneDoor.CheckStartIn(player, targetDoor);
		};
		PlayerSpawner.RegisterSpawnListener(delegate(Entity p, GameObject c, PlayerController cont)
		{
			player = p;
			BC_Collider component = p.GetComponent<BC_Collider>();
			if (component != null)
			{
				component.enabled = false;
			}
			playerCont = cont;
			playerCont.SetEnabled(false);
			Camera[] componentsInChildren = c.GetComponentsInChildren<Camera>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (componentsInChildren[j].name == "Main Camera")
				{
					playerCam = componentsInChildren[j];
				}
			}
			onStartFade = OverlayFader.PrepareFadeOut(fadeData, null);
			SceneDoor.CheckAndBuild(player, playerCam, refOwner, startFadeIn);
		});
		PlayerSpawner.RegisterSpawnDelegation(delegate(PlayerSpawner.DoSpawnFunc func, Vector3 pos, Vector3 dir)
		{
			onDoSpawn = func;
			if (!hasFallbackPos)
			{
				defSpawnPos = pos;
			}
		});
		TileMeshRefOwner.RegisterAwakeListener(delegate(TileMeshRefOwner owner)
		{
			refOwner = owner;
			SceneDoor.CheckAndBuild(player, playerCam, refOwner, startFadeIn);
		});
		SceneDoor.RegisterListener(wantedDoor, delegate(SceneDoor door)
		{
			targetDoor = door;
			targetDoor.coolDown = 1f;
			PlayerSpawner.DoSpawnFunc onDoSpawn;
			if (onDoSpawn != null)
			{
				Vector3 pos = targetDoor.transform.TransformPoint(targetDoor._spawnOffset);
				spawnDir = -targetDoor.transform.forward;
				onDoSpawn = onDoSpawn;
				onDoSpawn = null;
				onDoSpawn(pos, spawnDir);
			}
		});
		Stopwatch clock = null;
		LevelLoadListener.RegisterListener(delegate
		{
			PlayerSpawner.DoSpawnFunc onDoSpawn;
			if (onDoSpawn != null)
			{
				if (targetDoor == null)
				{
					UnityEngine.Debug.Log(string.Concat(new object[]
					{
						"didn't find ",
						wantedDoor,
						", spawning at default: ",
						defSpawnPos
					}));
				}
				else
				{
					defSpawnPos = targetDoor.transform.TransformPoint(targetDoor._spawnOffset);
					spawnDir = -targetDoor.transform.forward;
				}
				onDoSpawn = onDoSpawn;
				onDoSpawn = null;
				onDoSpawn(defSpawnPos, spawnDir);
			}
			else if (UnityEngine.Object.FindObjectOfType<PlayerSpawner>() == null)
			{
				UnityEngine.Debug.Log("didn't find a spawner in " + Utility.GetCurrentSceneName());
				SceneDoor.ClearListeners();
				PlayerSpawner.ClearListeners();
				if (onStartFade == null)
				{
					onStartFade = OverlayFader.PrepareFadeOut(fadeData, null);
				}
			}
			else
			{
				UnityEngine.Debug.Log("awaiting spawning in " + Utility.GetCurrentSceneName());
			}
			clock.Stop();
			UnityEngine.Debug.Log(string.Concat(new object[]
			{
				"Loading ",
				this._scene,
				" took ",
				clock.ElapsedMilliseconds,
				" ms"
			}));
			TileMeshRefOwner x = UnityEngine.Object.FindObjectOfType<TileMeshRefOwner>();
			if (x == null && startFadeIn != null)
			{
				startFadeIn();
			}
		});
		int prepareCounter = 0;
		SceneDoor.OnPrepareFunc onPrepareDone = delegate()
		{
			prepareCounter--;
			if (prepareCounter <= 0)
			{
				clock = Stopwatch.StartNew();
				Utility.LoadLevel(this._scene);
			}
		};
		List<SceneDoor.OnPrepareFunc> list = new List<SceneDoor.OnPrepareFunc>();
		if (this._saver != null)
		{
			list.Add(delegate
			{
				this.SaveStartPos(this._saver, wantedDoor, null);
				this._saver.SaveAll(true, delegate(bool success, string msg)
				{
					if (!success)
					{
						UnityEngine.Debug.LogError("Error saving: " + msg);
					}
					else
					{
						UnityEngine.Debug.Log("Game saved");
					}
					onPrepareDone();
				});
			});
		}
		prepareCounter = list.Count;
		if (prepareCounter == 0)
		{
			onPrepareDone();
		}
		else
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i]();
			}
		}
	}

	static Vector3 WorldToScreen(Vector3 pos)
	{
		return CoordinateTransformer.ToViewport("Main Camera", pos);
	}

	void StartFadeout(RoomSwitchable switcher)
	{
		// Dungeon Rush
		if (ModeControllerNew.IsDungeonRush && ModMaster.GetMapType() == "Dungeon" && DungeonRush.Instance.ReloadDungeon())
		{
			_scene = ModMaster.GetMapName();
			_correspondingDoor = _scene + "Inside";

			if (_scene == "GrandLibrary2" || _scene == "Deep19s") { _correspondingDoor = _scene; }
		}

		// Dungeon Rush: Reload dungeon if you try to leave it. No escape!
		ModeController mc = GameObject.Find("ModeController").GetComponent<ModeController>();
		if (mc.isDungeonRush && ModMaster.GetMapType() == "Dungeon" && mc.dungeonRushManager.CanLoadNextDungeon())
		{
			_scene = ModMaster.GetMapName();
			_correspondingDoor = _scene + "Inside";

			if (_scene == "GrandLibrary2" || _scene == "Deep19s") { _correspondingDoor = _scene; }
		}

		Vector3 vector = base.transform.forward;
		Vector3 vector2 = base.transform.position + vector * this._moveDist;
		if (switcher != null)
		{
			Vector3 value = SceneDoor.WorldToScreen(base.transform.position);
			Vector3 position = base.transform.position;
			if (this._moveFromCenter)
			{
				position = switcher.transform.position;
				vector = (vector2 - position).normalized;
			}
			if (switcher.StartLevelTransition(position, vector2, vector, this._enterAnim))
			{
				EffectFactory.Instance.PlayQuickEffect(this._enterEffect, switcher.transform.position, switcher.transform.forward, null);
				OverlayFader.StartFade(this._fadeData, true, delegate()
				{
					this.DoLoad();
				}, new Vector3?(value));
			}
		}
		else
		{
			OverlayFader.StartFade(this._fadeData, true, delegate()
			{
				this.DoLoad();
			}, new Vector3?(Vector3.zero));
		}
	}

	void Update()
	{
		this.coolDown -= Time.deltaTime;
	}

	void IBC_TriggerEnterListener.OnTriggerEnter(BC_TriggerData other)
	{
		if ((!ModOptionsOld.fastTransitions && (this.coolDown > 0f || !this.exited)) || ModOptionsOld.fastTransitions && !exited)
		{
			return;
		}
		RoomSwitchable component = other.collider.GetComponent<RoomSwitchable>();
		if (component != null)
		{
            //DOOR RANDOMIZER
            if (DoorsRandomizer.Instance.GetRandomizedDoor(gameObject.name, out string scene, out string door))
            {
                _scene = scene;
                _correspondingDoor = door;
            }
            ModSpawner.Instance.EmptySpawnerLists();
            this.coolDown = 20f;
		this.StartFadeout(component);
		}
	}

	void IBC_TriggerExitListener.OnTriggerExit(BC_TriggerData other)
	{
		this.exited = true;
	}

	public static void StartLoad(string targetScene, string targetDoor, FadeEffectData fadeData, SaverOwner saver = null, Vector3? fallbackPos = null)
	{
		SceneDoor sceneDoor = new GameObject("DummyDoor").AddComponent<SceneDoor>();
		sceneDoor._scene = targetScene;
		sceneDoor._correspondingDoor = targetDoor;
		if (fallbackPos != null)
		{
			sceneDoor._fallbackPosition = fallbackPos.Value;
			sceneDoor._hasFallbackPos = true;
		}
		sceneDoor._fadeData = fadeData;
		sceneDoor._saver = saver;
		sceneDoor.StartFadeout(null);
	}

	public static bool GetSpawnPoint(string doorName, out Vector3 pos, out Vector3 dir)
	{
		SceneDoor byName = TransformUtility.GetByName<SceneDoor>(doorName);
		if (byName != null)
		{
			pos = byName.transform.TransformPoint(byName._spawnOffset);
			dir = -byName.transform.forward;
			return true;
		}
		pos = (dir = Vector3.zero);
		return false;
	}

	public delegate void OnAwakeFunc(SceneDoor door);

	delegate void OnPrepareFunc();
}
