using System;
using System.Collections.Generic;
using UnityEngine;
using ModStuff;

[AddComponentMenu("Ittle 2/Entity/Statusable")]
public class EntityStatusable : EntityHitListener, IUpdatable, IBaseUpdateable
{
	[SerializeField]
	StatusType[] _immunes;

	[SerializeField]
	StatusType[] _saveable;

	public List<Status> currentStatuses;

	List<EntityStatusable.IStatusAffector> affectors;

	// Added to make status debuffs saveable when activated via mod
	public void Awake()
	{
		StatusManager sm = StatusManager.Instance;

		if (sm.hasSetStatus && sm.statuses.Count > 0)
		{
			_saveable = StatusManager.Instance.statuses.ToArray();
			Array.Clear(_immunes, 0, _immunes.Length);
		}
	}

	protected override void DoActivate()
	{
		this.currentStatuses = new List<Status>();
		this.affectors = new List<EntityStatusable.IStatusAffector>();
	}

	protected override void DoDeactivate()
	{
		if (this.currentStatuses != null)
		{
			for (int i = this.currentStatuses.Count - 1; i >= 0; i--)
			{
				this.currentStatuses[i].Stop(true);
				this.currentStatuses[i] = null;
			}
			this.currentStatuses.Clear();
		}
		if (this.affectors != null)
		{
			this.affectors.Clear();
		}
	}

	bool HasSaveableStatuses()
	{
		if (this.currentStatuses != null && this._saveable != null)
		{
			for (int i = this.currentStatuses.Count - 1; i >= 0; i--)
			{
				if (Array.IndexOf<StatusType>(this._saveable, this.currentStatuses[i].Type) != -1)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void DoWriteSaveData(IDataSaver local, IDataSaver level)
	{
		if (this.HasSaveableStatuses())
		{
			IDataSaver emptyLocalSaver = local.GetEmptyLocalSaver("status");
			for (int i = 0; i < this.currentStatuses.Count; i++)
			{
				Status status = this.currentStatuses[i];
				if (Array.IndexOf<StatusType>(this._saveable, status.Type) != -1)
				{
					IDataSaver localSaver = emptyLocalSaver.GetLocalSaver(status.Type.name);
					status.SaveState(localSaver);
				}
			}
		}
		else
		{
			local.ClearLocalSaver("status");
		}
	}

	StatusType GetSaveableStatus(string name)
	{
		for (int i = this._saveable.Length - 1; i >= 0; i--)
		{
			if (this._saveable[i].name == name)
			{
				return this._saveable[i];
			}
		}
		return null;
	}

	protected override void DoReadSaveData(IDataSaver local, IDataSaver level)
	{
		if (local.HasLocalSaver("status"))
		{
			IDataSaver localSaver = local.GetLocalSaver("status");
			string[] localSaverNames = localSaver.GetLocalSaverNames();
			for (int i = 0; i < localSaverNames.Length; i++)
			{
				StatusType saveableStatus = this.GetSaveableStatus(localSaverNames[i]);
				if (saveableStatus != null)
				{
					Status status = this.AddStatus(saveableStatus);
					if (status != null)
					{
						status.ReadState(localSaver.GetLocalSaver(localSaverNames[i]));
					}
				}
			}
		}
	}

	bool IsImmune(StatusType type)
	{
		if (this.affectors != null)
		{
			for (int i = 0; i < this.affectors.Count; i++)
			{
				if (this.affectors[i].PreventStatus(type))
				{
					return true;
				}
			}
		}
		if (this._immunes != null)
		{
			for (int j = this._immunes.Length - 1; j >= 0; j--)
			{
				if (this._immunes[j] == type)
				{
					return true;
				}
			}
		}
		return false;
	}

	Status AddStatus(StatusType type)
	{
		if (this.IsImmune(type))
		{
			return null;
		}
		for (int i = this.currentStatuses.Count - 1; i >= 0; i--)
		{
			Status status = this.currentStatuses[i];
			if (type.Overrides(status))
			{
				status.Stop(true);
				this.currentStatuses.RemoveAt(i);
			}
			else if (type.Cancels(status))
			{
				status.Stop(true);
				this.currentStatuses.RemoveAt(i);
				return null;
			}
		}
		Status status2 = type.Apply(this.owner);
		this.currentStatuses.Add(status2);
		return status2;
	}

	public void AddStatusType(StatusType type)
	{
		this.AddStatus(type);
	}

	public void ClearStatus(StatusType type)
	{
		for (int i = this.currentStatuses.Count - 1; i >= 0; i--)
		{
			Status status = this.currentStatuses[i];
			if (status.Type == type)
			{
				status.Stop(true);
				this.currentStatuses.RemoveAt(i);
			}
		}
	}

	void HandleDamageType(DamageType type)
	{
		StatusType[] statuses = type.GetStatuses();
		if (statuses != null)
		{
			for (int i = 0; i < statuses.Length; i++)
			{
				this.AddStatus(statuses[i]);
			}
		}
	}

	public override bool HandleHit(ref HitData data, ref HitResult inResult)
	{
		HitData.DamageInstance.Data[] damageData = data.GetDamageData();
		for (int i = 0; i < damageData.Length; i++)
		{
			this.HandleDamageType(damageData[i].type);
		}
		HitData.DamageInstance.StatusData[] statusData = data.GetStatusData();
		for (int j = 0; j < statusData.Length; j++)
		{
			if (UnityEngine.Random.value < statusData[j].chance)
			{
				this.AddStatus(statusData[j].type);
			}
		}
		return true;
	}

	public void RegisterAffector(EntityStatusable.IStatusAffector affector)
	{
		if (this.affectors == null)
		{
			this.affectors = new List<EntityStatusable.IStatusAffector>();
		}
		int priority = affector.Priority;
		for (int i = 0; i < this.affectors.Count; i++)
		{
			if (this.affectors[i].Priority < priority)
			{
				this.affectors.Insert(i, affector);
				return;
			}
		}
		this.affectors.Add(affector);
	}

	public void UnregisterAffector(EntityStatusable.IStatusAffector affector)
	{
		if (this.affectors == null)
		{
			return;
		}
		this.affectors.Remove(affector);
	}

	public void RefreshStatusGfx()
	{
		if (this.currentStatuses != null)
		{
			for (int i = this.currentStatuses.Count - 1; i >= 0; i--)
			{
				this.currentStatuses[i].RefreshGfx();
			}
		}
	}

	void IUpdatable.UpdateObject()
	{
		if (this.currentStatuses == null)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		for (int i = this.currentStatuses.Count - 1; i >= 0; i--)
		{
			if (this.currentStatuses[i].Update(deltaTime))
			{
				this.currentStatuses.RemoveAt(i);
			}
		}
	}

	public interface IStatusAffector
	{
		int Priority { get; }

		bool PreventStatus(StatusType type);
	}
}
