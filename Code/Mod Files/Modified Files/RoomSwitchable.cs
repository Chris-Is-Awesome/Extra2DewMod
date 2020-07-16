using System;
using System.Collections.Generic;
using UnityEngine;
using ModStuff;

[AddComponentMenu("Ittle 2/Entity/Actions/Room switchable")]
public class RoomSwitchable : EntityAction
{
	Vector3 startPos;
	Vector3 endPos;
	Vector3 startDir;
	Vector3 endDir;

	[SerializeField]
	public float _transitionSpeed;

	[SerializeField]
	string _transitionAnim;

	[SerializeField]
	float _animSpeedScale = 1f;

	[SerializeField]
	bool _updateRoom;

	[SerializeField]
	CameraContainer _levelCamera;

	[SerializeField]
	string _levelCameraName;

	[SerializeField]
	bool _canLevelTransition = true;

	[SerializeField]
	public float _levelTransitionSpeed = 2f;

	[SerializeField]
	bool _scaleTransitionSpeed;

	[SerializeField]
	UpdatableLayer _levelTransitionPause;

	[SerializeField]
	string _startEventName;

	float timer;

	float timeScale;

	LevelRoom startRoom;

	LevelRoom targetRoom;

	EntityAnimator.AnimatorState animState;

	bool isLevelTransition;

	bool shouldRelease;

	FadeEffectData currentFadeEffect;

	CameraContainer currentCamera;

	EntityHittable hittable;

	HittableObjectData.DisableTag hitDisable;

	Entity.FloorAttacher floorDisable;

	List<EntityComponent> disables = new List<EntityComponent>();

	RoomSwitchable.DataContext dataContext = new RoomSwitchable.DataContext();

	ObjectUpdater.PauseTag pauseTag;

	public void RegisterLevelTransitionDisable(EntityComponent comp)
	{
		if (!this.disables.Contains(comp))
		{
			this.disables.Add(comp);
		}
	}

	public void UnregisterLevelTransitionDisable(EntityComponent comp)
	{
		this.disables.Remove(comp);
	}

	protected override void DoEnable(Entity owner)
	{
		if (this._levelCamera == null && !string.IsNullOrEmpty(this._levelCameraName))
		{
			CameraContainer[] array = UnityEngine.Object.FindObjectsOfType<CameraContainer>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].name == this._levelCameraName)
				{
					this._levelCamera = array[i];
					break;
				}
			}
		}
		this.hittable = owner.GetEntityComponent<EntityHittable>();
	}

	protected override void DoReadSaveData(IDataSaver local, IDataSaver level)
	{
		this.dataContext = RoomSwitchable.DataContext.Load(local, level);
		LevelRoom roomForPosition = LevelRoom.GetRoomForPosition(this.owner.WorldTracePosition, null);
		if (roomForPosition != null)
		{
			this.dataContext.RegisterRoomVisit(roomForPosition);
		}
	}

	protected override void DoWriteSaveData(IDataSaver local, IDataSaver level)
	{
		this.dataContext.Save(local, level);
	}

	protected override void DoAction(EntityAction.ActionData dataObj)
	{
		RoomSwitchable.SwitchActionData switchActionData = (RoomSwitchable.SwitchActionData)dataObj;
		Vector3 vector = switchActionData.to - switchActionData.from;

		this.startPos = this.owner.WorldPosition;
		if (switchActionData.relative)
		{
			this.endPos = this.startPos + vector;
		}
		else
		{
			this.endPos = switchActionData.to;
		}
		this.endPos.y = this.startPos.y + vector.y;
		this.startDir = switchActionData.inDir;
		this.endDir = switchActionData.outDir;
		if (switchActionData.isLevel)
		{
			// Boss Rush
			if (ModeControllerNew.IsBossRush)
			{
				BossRush brm = BossRush.Instance;
				startPos = brm.GetStartPosition();
				endPos = brm.GetEndPosition();
				startDir = brm.GetFacingDirection();
				endDir = startDir;
				brm.GiveDungeonItem();
				brm.RestorePlayerHP();
			}

			// Boss Rush: Set spawn point, facing direction, equips, HP and update HUD text for boss progress counter
			ModeController mc = GameObject.Find("ModeController").GetComponent<ModeController>();
			if (mc.isBossRush)
			{
				ModeController.BossRush brm = mc.bossRushManager;
				startPos = brm.GetPlayerSpawnPoint();
				endPos = brm.GetPlayerEndWalkPosition();
				startDir = brm.GetPlayerFacingDirection();
				endDir = startDir;
				brm.SetItemsForBoss();
				brm.StartOfGame();
			}
			// Dungeon Rush: Save scene data
			if (mc.isDungeonRush)
			{
				ModeController.DungeonRush drm = mc.dungeonRushManager;
				drm.StartOfGame();
			}

			this.timer = ((!this._scaleTransitionSpeed) ? this._levelTransitionSpeed : (vector.magnitude / this._levelTransitionSpeed));
		}
		else
		{
			this.timer = ((!this._scaleTransitionSpeed) ? this._transitionSpeed : (vector.magnitude / this._transitionSpeed));
		}
		this.timeScale = 1f / this.timer;
		this.currentCamera = null;
		if (!switchActionData.isLevel)
		{
			this.currentCamera = this._levelCamera;
			if (this.currentCamera != null)
			{
				this.currentCamera.StartTransition(this.startPos, this.endPos, switchActionData.toRoom);
			}
		}
		if (switchActionData.toRoom != null)
		{
			switchActionData.toRoom.SetImportantPoint(this.endPos);
		}
		if (this._updateRoom)
		{
			this.startRoom = switchActionData.fromRoom;
			this.targetRoom = switchActionData.toRoom;
			if (this.startRoom != null)
			{
				this.startRoom.StartTransition(true);
				this.startRoom.UpdateTransition(1f);
			}
			if (this.targetRoom != null)
			{
				this.targetRoom.StartTransition(false);
				this.targetRoom.UpdateTransition(0f);
			}
		}
		else
		{
			this.startRoom = (this.targetRoom = null);
		}
		if (!string.IsNullOrEmpty(switchActionData.switchAnim))
		{
			this.owner.PlayAnimation(switchActionData.switchAnim, 0);
			this.animState = null;
		}
		else
		{
			this.animState = this.owner.PlayAnimation(this._transitionAnim, 0);
		}
		this.isLevelTransition = switchActionData.isLevel;
		this.shouldRelease = switchActionData.release;
		if (this.hittable != null)
		{
			this.hitDisable = this.hittable.PushDisableTag();
		}
		this.floorDisable = this.owner.ReleaseFromFloor(this);
		if (this.targetRoom != null)
		{
			this.dataContext.RegisterRoomVisit(this.targetRoom);
		}
		EntityEventsOwner.RoomEventData data = new EntityEventsOwner.RoomEventData(this.endPos, this.endDir);
		this.owner.LocalEvents.SendRoomChange(this.owner, this.targetRoom, this.startRoom, data);
		this.currentFadeEffect = switchActionData.fadeEffect;
		if (switchActionData.fadeEffect != null)
		{
			Vector3 value = CoordinateTransformer.ToViewport("Main Camera", this.owner.WorldPosition);
			OverlayFader.StartFade(switchActionData.fadeEffect, true, delegate()
			{
				this.FadeDone();
			}, new Vector3?(value));
		}
		if (this.isLevelTransition && this._levelTransitionPause != null)
		{
			this.pauseTag = ObjectUpdater.Instance.RequestPause(this._levelTransitionPause);
		}
	}

	protected override void StopAction(bool cancel)
	{
		this.owner.StopMoving();
		this.owner.WorldPosition = this.endPos;
		this.owner.TurnTo(this.endDir, 0f);
		this.owner.PlayAnimation(this.owner.DefaultAnimName, 0);
		if (this.currentCamera != null)
		{
			this.currentCamera.FinishTransition();
		}
		if (this.startRoom != null)
		{
			this.startRoom.FinishTransition(true);
		}
		if (this.targetRoom != null)
		{
			this.targetRoom.FinishTransition(false);
		}
		if (this.currentFadeEffect != null)
		{
			Vector3 value = CoordinateTransformer.ToViewport("Main Camera", this.owner.WorldPosition);
			OverlayFader.StartFade(this.currentFadeEffect, false, null, new Vector3?(value));
		}
		if (this.hitDisable != null)
		{
			this.hitDisable.Free();
		}
		this.hitDisable = null;
		if (this.floorDisable != null)
		{
			this.floorDisable();
		}
		this.floorDisable = null;
		this.ReleasePause();
		EntityEventsOwner.RoomEventData data = new EntityEventsOwner.RoomEventData(this.endPos, this.endDir);
		this.owner.LocalEvents.SendRoomChangeDone(this.owner, this.startRoom, this.targetRoom, data);
	}

	protected override bool DoUpdate()
	{
		if (this.timer < 0f && this.isLevelTransition)
		{
			return !this.shouldRelease;
		}
		if (this.currentFadeEffect == null)
		{
			this.timer -= Time.deltaTime;
			float num = 1f - Mathf.Max(0f, this.timer * this.timeScale);
			this.owner.WorldPosition = this.startPos + (this.endPos - this.startPos) * num;
			Vector3 dir = Vector3.Slerp(this.startDir, this.endDir, num);
			this.owner.TurnTo(dir, 720f);
			if (this.currentCamera != null)
			{
				this.currentCamera.UpdateTransition(num);
			}
			if (this.targetRoom != null)
			{
				this.targetRoom.UpdateTransition(Mathf.Clamp01(num * 1.25f - 0.25f));
			}
			if (this.startRoom != null)
			{
				this.startRoom.UpdateTransition(Mathf.Clamp01((1f - num) * 1.25f - 0.25f));
			}
		}
		if (this.animState != null)
		{
			this.animState.speed = ((!this._scaleTransitionSpeed) ? this._animSpeedScale : (this._transitionSpeed * this._animSpeedScale));
		}
		return this.timer > 0f || (this.isLevelTransition && !this.shouldRelease);
	}

	void FadeDone()
	{
		this.timer = 0f;
	}

	public void SetLevelCamera(CameraContainer cam)
	{
		this._levelCamera = cam;
		this._levelCameraName = cam.name;
	}

	public void StartTransition(Vector3 from, Vector3 to, LevelRoom fromRoom, LevelRoom toRoom, Vector3 inDir, Vector3 outDir, bool relative)
	{
		RoomSwitchable.SwitchActionData data = new RoomSwitchable.SwitchActionData(from, to, inDir, outDir, fromRoom, toRoom, relative, false, true);
		this.owner.DoAction(base.ActionName, data);
	}

	public void StartWarpTransition(Vector3 from, Vector3 to, LevelRoom fromRoom, LevelRoom toRoom, Vector3 inDir, Vector3 outDir, string anim, FadeEffectData fadeEffect)
	{
		RoomSwitchable.SwitchActionData switchActionData = new RoomSwitchable.SwitchActionData(from, to, inDir, outDir, fromRoom, toRoom, false, false, true);
		switchActionData.switchAnim = anim;
		switchActionData.fadeEffect = fadeEffect;
		this.owner.DoAction(base.ActionName, switchActionData);
	}

	public bool StartLevelTransition(Vector3 from, Vector3 to, Vector3 inDir, string anim = null)
	{
		if (!this._canLevelTransition)
		{
			return false;
		}
		RoomSwitchable.SwitchActionData switchActionData = new RoomSwitchable.SwitchActionData(from, to, inDir, inDir, null, null, true, true, false);
		switchActionData.switchAnim = anim;
		this.owner.DoAction(base.ActionName, switchActionData);
		for (int i = 0; i < this.disables.Count; i++)
		{
			EntityComponent entityComponent = this.disables[i];
			if (entityComponent != null)
			{
				entityComponent.enabled = false;
			}
		}
		return true;
	}

	public void FinishLevelTransition(Vector3 from, Vector3 to, Vector3 inDir, string anim = null)
	{
		if (!this._canLevelTransition)
		{
			return;
		}
		RoomSwitchable.SwitchActionData switchActionData = new RoomSwitchable.SwitchActionData(from, to, inDir, inDir, null, null, true, true, true);
		switchActionData.switchAnim = anim;
		this.owner.DoAction(base.ActionName, switchActionData);
		for (int i = 0; i < this.disables.Count; i++)
		{
			EntityComponent entityComponent = this.disables[i];
			if (entityComponent != null)
			{
				entityComponent.enabled = true;
			}
		}
	}

	public void SendStartEvent()
	{
		if (!string.IsNullOrEmpty(this._startEventName))
		{
			GenericGameEvents.Instance.SendEvent(this._startEventName, null);
		}
	}

	void ReleasePause()
	{
		if (this.pauseTag != null)
		{
			this.pauseTag.Release();
			this.pauseTag = null;
		}
	}

	void OnDestroy()
	{
		this.ReleasePause();
	}

	public RoomSwitchable.DataContext RoomData
	{
		get
		{
			return this.dataContext;
		}
	}

	public class DataContext
	{
		List<string> visitedRooms = new List<string>();

		string latestRoom;

		public void RegisterRoomVisit(LevelRoom room)
		{
			string roomName = room.RoomName;
			if (!this.visitedRooms.Contains(roomName))
			{
				this.visitedRooms.Add(roomName);
			}
			this.latestRoom = roomName;
		}

		public bool IsVisited(string roomName)
		{
			return this.visitedRooms.Contains(roomName);
		}

		public int NumVisitedRooms
		{
			get
			{
				return this.visitedRooms.Count;
			}
		}

		public string LatestRoom
		{
			get
			{
				return this.latestRoom;
			}
		}

		public static RoomSwitchable.DataContext Load(IDataSaver local, IDataSaver level)
		{
			RoomSwitchable.DataContext dataContext = new RoomSwitchable.DataContext();
			if (level.HasLocalSaver("seenrooms"))
			{
				IDataSaver localSaver = level.GetLocalSaver("seenrooms");
				foreach (string item in localSaver.GetAllDataKeys())
				{
					if (!dataContext.visitedRooms.Contains(item))
					{
						dataContext.visitedRooms.Add(item);
					}
				}
			}
			return dataContext;
		}

		public void Save(IDataSaver local, IDataSaver level)
		{
			if (this.visitedRooms.Count > 0)
			{
				IDataSaver localSaver = level.GetLocalSaver("seenrooms");
				for (int i = 0; i < this.visitedRooms.Count; i++)
				{
					localSaver.SaveBool(this.visitedRooms[i], true);
				}
			}
		}
	}

	public class SwitchActionData : EntityAction.ActionData
	{
		public Vector3 from;

		public Vector3 to;

		public Vector3 inDir;

		public Vector3 outDir;

		public LevelRoom fromRoom;

		public LevelRoom toRoom;

		public string switchAnim;

		public bool relative;

		public bool isLevel;

		public bool release;

		public FadeEffectData fadeEffect;

		public SwitchActionData(Vector3 P0, Vector3 P1, Vector3 D0, Vector3 D1, LevelRoom r0, LevelRoom r1, bool relative, bool level = false, bool release = true)
		{
			this.from = P0;
			this.to = P1;
			this.inDir = D0;
			this.outDir = D1;
			this.fromRoom = r0;
			this.toRoom = r1;
			this.isLevel = level;
			this.relative = relative;
			this.release = release;
		}
	}
}
