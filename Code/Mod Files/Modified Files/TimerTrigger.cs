using System;
using UnityEngine;

[AddComponentMenu("Ittle 2/Room/Triggers/Timer trigger")]
public class TimerTrigger : RoomTrigger, IUpdatable, IBaseUpdateable
{
	[SerializeField]
	public float _time = 1f;

	[SerializeField]
	private float _unfireTime;

	[SerializeField]
	private bool _loop;

	[SerializeField]
	private bool _startActive = true;

	[SerializeField]
	private float _maxTime;

	[SerializeField]
	private bool _randomTime;

	public float timer;

	private bool gotSignal;

	private bool countdown;

	public TimerTrigger ()
	{
	}

	private float GetTime ()
	{
		return (!this._randomTime) ? this._time : UnityEngine.Random.Range(this._time, this._maxTime);
	}

	//Added this function to change the timer of dynamite
	public void SetTime (float newtimer)
	{
		this.timer = newtimer;
		return;
	}

	protected override void DoActivate (LevelRoom room)
	{
		base.DoActivate(room);
		if (!this.gotSignal)
		{
			this.timer = this.GetTime();
			this.countdown = this._startActive;
		}
	}

	protected override void DoDeactivate (LevelRoom room)
	{
		this.gotSignal = false;
		base.DoDeactivate(room);
	}

	protected override void OnDisableTrigger (bool fast)
	{
		base.enabled = false;
		this.countdown = false;
	}

	public void RestartTimer ()
	{
		this.gotSignal = true;
		if (base.IsFired)
		{
			base.Unfire();
		}
		this.timer = this.GetTime();
		this.countdown = true;
		base.enabled = true;
	}

	public void StartTimer ()
	{
		this.gotSignal = true;
		this.countdown = true;
		base.enabled = true;
	}

	public void PauseTimer ()
	{
		this.gotSignal = true;
		this.countdown = false;
		base.enabled = false;
	}

	public void ZeroTimer ()
	{
		this.gotSignal = true;
		this.timer = 0f;
	}

	void IUpdatable.UpdateObject ()
	{
		if (!this.countdown)
		{
			base.enabled = false;
			return;
		}
		this.timer -= Time.deltaTime;
		if (this.timer <= 0f)
		{
			bool isFired = base.IsFired;
			if (!isFired)
			{
				base.Fire();
			}
			else
			{
				base.Unfire();
			}
			if (!this._loop)
			{
				base.enabled = false;
			}
			else if (isFired)
			{
				this.timer = this.GetTime();
			}
			else
			{
				this.timer = this._unfireTime;
			}
		}
	}
}
