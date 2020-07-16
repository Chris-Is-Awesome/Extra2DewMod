using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using ModStuff;

public abstract class RoomAction : RoomObjectComponent
{
	[SerializeField]
	public string _saveName;

	[SerializeField]
	bool _dontSave;

	[SerializeField]
	bool _worldSave;

	[SerializeField]
	int _triggerCount;

	[SerializeField]
	bool _refire;

	List<RoomTrigger> triggers = new List<RoomTrigger>();

	List<RoomTrigger> firedTriggers = new List<RoomTrigger>();

	Entity triggerer;

	bool hasFired;

	static string GetBaseSaveName(RoomAction action)
	{
		Vector3 position = action.transform.position;
		return string.Concat(new object[]
		{
			action.name,
			'-',
			(int)position.x,
			'-',
			(int)position.z
		});
	}

	public static string GetUniqueSaveName(RoomAction action)
	{
		string text = RoomAction.GetBaseSaveName(action);
		LevelRoom levelRoom = TransformUtility.FindInParents<LevelRoom>(action.transform);
		if (levelRoom != null)
		{
			DummyAction[] componentsInChildren = levelRoom.GetComponentsInChildren<DummyAction>(true);
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = componentsInChildren.Length - 1; i >= 0; i--)
			{
				DummyAction dummyAction = componentsInChildren[i];
				if (dummyAction != null && dummyAction != action && !string.IsNullOrEmpty(dummyAction._saveName))
				{
					hashSet.Add(dummyAction.name);
				}
			}
			string arg = text;
			int num = 1;
			while (hashSet.Contains(text))
			{
				text = arg + num;
				num++;
			}
		}
		return text;
	}

	public bool NeedsSaveName()
	{
		return !this._dontSave && !this._worldSave && string.IsNullOrEmpty(this._saveName);
	}

	public void SetSaveName(string name)
	{
		this._saveName = name;
	}

	protected override void DoEnable(RoomObject owner)
	{
		if (string.IsNullOrEmpty(this._saveName))
		{
			if (!this._worldSave)
			{
				this._saveName = RoomAction.GetBaseSaveName(this);
			}
			else
			{
				UnityEngine.Debug.LogError("World-saved actions must not have default save names");
			}
		}
	}

	protected override void DoActivate(LevelRoom room)
	{
		this.triggerer = null;
		if (this._refire && this.hasFired)
		{
			this.Fire(true);
		}
	}

	protected override void DoInit()
	{
		if (!this._dontSave && !this.hasFired && this.LoadSave())
		{
			this.Fire(true);
		}
	}

	bool LoadSave()
	{
		if (this._worldSave)
		{
			return base.Room.LevelRoot.WorldStorage.LoadBool(this._saveName);
		}
		return base.Room.RoomStorage.LoadBool(this._saveName);
	}

	void StoreSave()
	{
		if (this._worldSave)
		{
			if (!string.IsNullOrEmpty(this._saveName))
			{
				base.Room.LevelRoot.WorldStorage.SaveBool(this._saveName, true);
			}
		}
		else
		{
			base.Room.RoomStorage.SaveBool(this._saveName, true);
		}

		// Invoke OnSaveFlagSaved events
		GameStateNew.OnSaveFlagSaved(_saveName);
	}

	public Entity Triggerer
	{
		get
		{
			return this.triggerer;
		}
	}

	public void RegisterTrigger(RoomTrigger trigger)
	{
		this.triggers.Add(trigger);
		if (this.hasFired)
		{
			trigger.DisableTrigger(true);
		}
		else if (this.OnGotTrigger != null)
		{
			this.OnGotTrigger(this, trigger);
		}
	}

	public void DeregisterTrigger(RoomTrigger trigger)
	{
		this.triggers.Remove(trigger);
	}

	public event RoomAction.OnFireFunc OnFire;

	public event RoomAction.OnTriggerFunc OnGotTrigger;

	public bool HasFired
	{
		get
		{
			return this.hasFired;
		}
	}

	protected abstract bool DoFire(bool fast);

	public void Fire(bool fast)
	{
		if (this.hasFired)
		{
			return;
		}
		if (this.DoFire(fast))
		{
			this.hasFired = true;
			if (!this._dontSave && !fast && base.Room != null)
			{
				this.StoreSave();
			}
			if (this.OnFire != null)
			{
				this.OnFire(this, fast);
			}
			this.SendDisables(fast);
		}
	}

	public void SendDisables(bool fast)
	{
		for (int i = 0; i < this.triggers.Count; i++)
		{
			this.triggers[i].DisableTrigger(fast);
		}
	}

	public void SignalFire(RoomTrigger trigger)
	{
		if (!this.firedTriggers.Contains(trigger))
		{
			this.firedTriggers.Add(trigger);
		}
		if ((this._triggerCount == 0 && this.firedTriggers.Count == this.triggers.Count) || this.firedTriggers.Count == this._triggerCount)
		{
			this.triggerer = null;
			for (int i = 0; i < this.firedTriggers.Count; i++)
			{
				if (this.firedTriggers[i].Triggerer != null)
				{
					this.triggerer = this.firedTriggers[i].Triggerer;
				}
			}
			this.Fire(false);
		}
	}

	public void SignalUnfire(RoomTrigger trigger)
	{
		this.firedTriggers.Remove(trigger);
	}

	public List<Transform> GetHintTargets()
	{
		if (this.hasFired)
		{
			return null;
		}
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < this.triggers.Count; i++)
		{
			this.triggers[i].PopulateHintList(list);
		}
		return list;
	}

	public List<RoomTrigger> GetUnfiredTriggers()
	{
		if (this.hasFired)
		{
			return null;
		}
		List<RoomTrigger> result = new List<RoomTrigger>();
		for (int i = 0; i < this.triggers.Count; i++)
		{
			if (!this.firedTriggers.Contains(this.triggers[i]))
			{
				this.triggers[i].PopulateHintList(result);
			}
		}
		return result;
	}

	public delegate void OnFireFunc(RoomAction action, bool fast);

	public delegate void OnTriggerFunc(RoomAction action, RoomTrigger trigger);
}
