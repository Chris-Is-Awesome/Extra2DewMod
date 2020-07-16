using System;
using System.Diagnostics;
using UnityEngine;
using ModStuff;

public abstract class EntityAction : EntityComponent
{
	[SerializeField]
	string _name;

	[SerializeField]
	int _priority;

	[SerializeField]
	ActionSwitcher _actionSwitcher;

	EntityAction.OnStopFunc currOnStopListener;

	EntityAction currOverride;

	bool isActive;

	public event EntityAction.OnActionFunc OnFired;

	public event EntityAction.OnActionFunc OnStopped;

	protected abstract void DoAction (EntityAction.ActionData data);

	protected virtual void StopAction (bool cancel)
	{
	}

	protected virtual int GetPrio ()
	{
		return this._priority;
	}

	protected virtual bool DoUpdate ()
	{
		return false;
	}

	protected virtual void DoReleaseAction ()
	{
	}

	protected virtual bool CheckCanRelease
	{
		get
		{
			return false;
		}
	}

	protected virtual bool DoUpdateData (EntityAction.ActionData data)
	{
		return false;
	}

	protected virtual bool CheckOverride (EntityAction other)
	{
		return this.StartPriority > other.CurrentPriority;
	}

	protected static void DelegateDoAction (EntityAction target, EntityAction.ActionData data)
	{
		target.DoAction(data);
	}

	protected static void DelegateStopAction (EntityAction target, bool cancel)
	{
		target.StopAction(cancel);
	}

	protected static bool DelegateUpdateAction (EntityAction target)
	{
		return target.DoUpdate();
	}

	public virtual bool CanDoAction (EntityAction.ActionData data)
	{
		return !this.owner.LocalMods.PreventAction(this._name);
	}

	public virtual bool GetActionData (string dataName, out float value)
	{
		value = 0f;
		return false;
	}

	public int StartPriority
	{
		get
		{
			return this._priority;
		}
	}

	public int CurrentPriority
	{
		get
		{
			if (this.currOverride != null)
			{
				return this.currOverride.GetPrio();
			}
			return this.GetPrio();
		}
	}

	public bool CanOverride (EntityAction other)
	{
		if (this.currOverride != null)
		{
			return this.currOverride.CheckOverride(other);
		}
		return this.CheckOverride(other);
	}

	public void Fire (EntityAction.ActionData data)
	{
		// Updates stats for attack counts
		if (_name == "firesword")
		{
			ModMaster.UpdateStats("MeleeAttacks");
		}
		else if (_name == "forcewand")
		{
			ModMaster.UpdateStats("ForceAttacks");
		}
		else if (_name == "icering")
		{
			ModMaster.UpdateStats("IceAttacks");
		}
		else if (_name == "dynamite")
		{
			ModMaster.UpdateStats("DynamiteAttacks");
		}

		this.currOverride = null;
		this.isActive = true;
		if (this._actionSwitcher != null)
		{
			this.currOverride = this._actionSwitcher.GetAction(this.owner);
		}
		this.currOnStopListener = data.onStopped;
		base.enabled = true;
		if (this.OnFired != null)
		{
			this.OnFired(this.owner, this);
		}
		if (this.currOverride != null)
		{
			this.currOverride.DoAction(data);
		}
		else
		{
			this.DoAction(data);
		}
	}

	public void Stop (bool cancel)
	{
		base.enabled = false;
		this.isActive = false;
		this.owner.ClearAction(this);
		if (this.OnStopped != null)
		{
			this.OnStopped(this.owner, this);
		}
		if (this.currOverride != null)
		{
			this.currOverride.StopAction(cancel);
		}
		else
		{
			this.StopAction(cancel);
		}
		EntityAction.OnStopFunc onStopFunc = this.currOnStopListener;
		this.currOnStopListener = null;
		if (onStopFunc != null)
		{
			try
			{
				onStopFunc(cancel);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogError("Error in EntityAction.Stop callback: " + ex.Message + "\n" + ex.StackTrace);
			}
		}
	}

	public bool IsReleasable ()
	{
		if (this.currOverride != null)
		{
			return this.currOverride.CheckCanRelease;
		}
		return this.CheckCanRelease;
	}

	public void ReleaseAction ()
	{
		if (this.currOverride != null)
		{
			this.currOverride.DoReleaseAction();
		}
		else
		{
			this.DoReleaseAction();
		}
	}

	public bool UpdateAction ()
	{
		if (this.currOverride != null)
		{
			return this.currOverride.DoUpdate();
		}
		return this.DoUpdate();
	}

	public bool UpdateData (EntityAction.ActionData data)
	{
		return this.isActive && this.DoUpdateData(data);
	}

	public string ActionName
	{
		get
		{
			return this._name;
		}
	}

	public bool IsActive
	{
		get
		{
			return this.isActive;
		}
	}

	public delegate void OnStopFunc (bool cancel);

	public delegate void OnActionFunc (Entity ent, EntityAction action);

	[Serializable]
	public class ActionData
	{
		public Vector3 dir;

		public EntityAction.OnStopFunc onStopped;

		public Transform target;
	}

	[Serializable]
	public class TimePrio
	{
		public float timeMul;

		public int prio;
	}
}
