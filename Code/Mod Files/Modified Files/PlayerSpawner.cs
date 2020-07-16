using System;
using System.Collections.Generic;
using UnityEngine;
using ModStuff;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
	private static PlayerSpawner.OnSpawnedFunc onSpawned;

	private static PlayerSpawner.OnLateSpawnFunc onDelegateSpawn;

	[SerializeField]
	private Entity _playerEnt;

	[SerializeField]
	private GameObject _playerGraphics;

	[SerializeField]
	private GameObject _playerCamera;

	[SerializeField]
	private PlayerController _controller;

	[SerializeField]
	private SaverOwner _gameSaver;

	[SerializeField]
	private MappedInput _mainInput;

	[SerializeField]
	private bool _spawnOnFloor = true;

	[SerializeField]
	private LayerMask _floorLayer;

	[SerializeField]
	private PlayerRespawner _respawner;

	[SerializeField]
	private EntityLocalVarOverrider _varOverrider;

	public PlayerSpawner()
	{
	}

	public static void RegisterSpawnListener(PlayerSpawner.OnSpawnedFunc func)
	{
		if (PlayerSpawner.onSpawned == null)
		{
			PlayerSpawner.onSpawned = func;
			return;
		}
		PlayerSpawner.onSpawned = (PlayerSpawner.OnSpawnedFunc)Delegate.Combine(PlayerSpawner.onSpawned, func);
	}

	public static void RegisterSpawnDelegation(PlayerSpawner.OnLateSpawnFunc func)
	{
		PlayerSpawner.onDelegateSpawn = func;
	}

	public static void ClearListeners()
	{
		PlayerSpawner.onSpawned = null;
		PlayerSpawner.onDelegateSpawn = null;
	}

	private void DoSpawn(Vector3 P, Vector3 dir)
	{
		GameState gameState = Singleton<GameState>.Instance;

		// Trigger scene load events
		gameState.OnSceneChange(ModMaster.GetMapName(), ModMaster.GetMapSpawn());

		Vector3 vector;
		if (this._spawnOnFloor && PhysicsUtility.GetFloorPoint(P, 20f, 50f, this._floorLayer, out vector))
		{
			P.y = vector.y;
		}
		Vector3 vector2 = P + this._playerEnt.transform.localPosition;
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this._playerCamera, vector2 + this._playerCamera.transform.localPosition, this._playerCamera.transform.localRotation);
		gameObject.transform.localScale = this._playerCamera.transform.localScale;
		gameObject.name = this._playerCamera.name;
		CameraContainer component = gameObject.GetComponent<CameraContainer>();
		component.Init(this._playerCamera);
		Entity entity = UnityEngine.Object.Instantiate<Entity>(this._playerEnt, vector2, base.transform.rotation);
		entity.name = this._playerEnt.name;
		if (component != null)
		{
			RoomSwitchable componentInChildren = entity.GetComponentInChildren<RoomSwitchable>();
			if (componentInChildren != null)
			{
				componentInChildren.SetLevelCamera(component);
			}
		}
		if (this._playerGraphics != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this._playerGraphics, entity.transform.position + this._playerGraphics.transform.localPosition, this._playerGraphics.transform.localRotation);
			gameObject2.transform.parent = entity.transform;
			gameObject2.transform.localScale = this._playerGraphics.transform.localScale;
			gameObject2.name = this._playerGraphics.name;
		}
		entity.Init();
		entity.TurnTo(dir, 0f);
		FollowTransform componentInChildren2 = gameObject.GetComponentInChildren<FollowTransform>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.SetTarget(entity.transform);
		}
		LevelCamera componentInChildren3 = gameObject.GetComponentInChildren<LevelCamera>();
		if (componentInChildren3 != null)
		{
			Vector3 spawnPos = entity.WorldTracePosition;

			// Boss Rush:
			if (ModeControllerNew.IsBossRush)
			{
				spawnPos = BossRush.Instance.GetStartPosition();
			}

			// Handles Boss Rush spawn
			ModeController mc = GameObject.Find("ModeController").GetComponent<ModeController>();
			if (mc.isBossRush)
			{
				spawnPos = mc.bossRushManager.GetPlayerSpawnPoint();
			}

			LevelRoom roomForPosition = LevelRoom.GetRoomForPosition(spawnPos, null);
			if (roomForPosition != null)
			{
				componentInChildren3.SetRoom(roomForPosition);
				roomForPosition.SetImportantPoint(vector2);
				LevelRoom.SetCurrentActiveRoom(roomForPosition, false);
			}
		}
		PlayerController controller = ControllerFactory.Instance.GetController<PlayerController>(this._controller);
		controller.ControlEntity(entity);
		controller.name = this._controller.name;
		entity.Activate();

		if (ModSaver.LoadIntFromFile("mod/SetItems", "hasSetItems") != 0) { _varOverrider = null; }

		if (this._varOverrider != null)
		{
			this._varOverrider.Apply(entity);
		}
		EntityHUD componentInChildren4 = gameObject.GetComponentInChildren<EntityHUD>();
		if (componentInChildren4 != null)
		{
			componentInChildren4.Observe(entity, controller);
		}
		EntityObjectAttacher.Attacher attacher = null;
		EntityObjectAttacher component2 = base.GetComponent<EntityObjectAttacher>();
		if (component2 != null)
		{
			attacher = component2.GetAttacher();
		}
		EntityObjectAttacher.AttachTag attachTag = null;
		if (attacher != null)
		{
			attachTag = attacher.Attach(entity);
		}
		PlayerRespawner playerRespawner;
		if (this._respawner != null)
		{
			playerRespawner = UnityEngine.Object.Instantiate<PlayerRespawner>(this._respawner);
			playerRespawner.name = this._respawner.name;
		}
		else
		{
			playerRespawner = new GameObject("PlayerRespawer").AddComponent<PlayerRespawner>();
		}
		playerRespawner.Init(entity, controller, componentInChildren3, componentInChildren2, componentInChildren4, this._gameSaver, attacher, attachTag, this._varOverrider, P, dir);
		PlayerSpawner.OnSpawnedFunc onSpawnedFunc = PlayerSpawner.onSpawned;
		PlayerSpawner.onSpawned = null;
		if (onSpawnedFunc != null)
		{
			onSpawnedFunc(entity, gameObject, controller);
		}

		// Trigger spawn events
		gameState.hasPlayerSpawned = true;
		gameState.OnPlayerSpawn();

		// Invoke OnPlayerSpawn events
		GameStateNew.OnPlayerSpawned(false);

		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Awake()
	{
		if (PlayerSpawner.onDelegateSpawn != null)
		{
			PlayerSpawner.OnLateSpawnFunc onLateSpawnFunc = PlayerSpawner.onDelegateSpawn;
			PlayerSpawner.onDelegateSpawn = null;
			onLateSpawnFunc(new PlayerSpawner.DoSpawnFunc(this.DoSpawn), base.transform.position, base.transform.forward);
			return;
		}
		if (this._gameSaver != null)
		{
			StartMenu.InitGame(this._gameSaver, this._mainInput, null);
		}
		this.DoSpawn(base.transform.position, base.transform.forward);
	}

	public delegate void DoSpawnFunc(Vector3 pos, Vector3 dir);

	public delegate void OnSpawnedFunc(Entity player, GameObject camera, PlayerController controller);

	public delegate void OnLateSpawnFunc(PlayerSpawner.DoSpawnFunc doSpawn, Vector3 defPos, Vector3 defDir);
}
