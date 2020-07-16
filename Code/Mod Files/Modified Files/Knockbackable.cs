using System;
using UnityEngine;

[AddComponentMenu("Ittle 2/Entity/Hittable/Knockbackable")]
public class Knockbackable : EntityHitListener, IUpdatable, IBaseUpdateable
{
	[SerializeField]
	private float _speed = 4f;

	[SerializeField]
	private float _time = 0.25f;

	[SerializeField]
	private AnimationCurve _speedCurve;

	[SerializeField]
	private float _bounceScale = 0.5f;

	[SerializeField]
	private AnimationCurve _bounceCurve;

	[SerializeField]
	private Transform _bounceRoot;

	private Vector3 baseScale;

	private Vector3 endScale;

	private float timer;

	float speedScale;

	private Vector3 hitVec;

	private bool isHit;

	private bool ignoreBounce;

	public Knockbackable ()
	{
	}

	protected override void DoEnable (Entity owner)
	{
		base.DoEnable(owner);
		if (this._bounceRoot == null)
		{
			EntityGraphics realGraphics = owner.RealGraphics;
			if (realGraphics != null)
			{
				this._bounceRoot = realGraphics.transform;
			}
			else
			{
				this._bounceRoot = owner.RealTransform;
			}
		}
		this.baseScale = this._bounceRoot.localScale;
		this.endScale = this.baseScale * (1f + this._bounceScale);
		this.endScale.y = this.baseScale.y;
	}

	//Used for setsize command
	public void RefreshKnockbackScale (Entity player)
	{
		DoEnable(player);
	}

	protected override void DoActivate ()
	{
		this.isHit = false;
		this._bounceRoot.localScale = this.baseScale;
	}

	public override bool HandleHit (ref HitData data, ref HitResult inResult)
	{
		if (data.CancelKnockback)
		{
			return true;
		}
		if (!data.IsDamageMoreThan(0f))
		{
			return true;
		}
		Vector3 dir = this.owner.WorldPosition - data.Point;
		dir.y = 0f;
		dir.Normalize();
		this.StartKnockback(dir, 1f, false);
		return true;
	}

	public void StartKnockback (Vector3 dir, float power, bool ignoreBounce = false)
	{
		this.isHit = true;
		this.timer = this._time;
		this.ignoreBounce = ignoreBounce;
		this.hitVec = dir;
		this.speedScale = power;
		base.enabled = true;
	}

	void IUpdatable.UpdateObject ()
	{
		if (!this.isHit)
		{
			base.enabled = false;
			return;
		}
		this.timer -= Time.deltaTime;
		float time = 1f - Mathf.Clamp01(this.timer / this._time);
		// If knockback is modded
		Vector3 v = this._speed * this._speedCurve.Evaluate(time) * this.speedScale * this.hitVec * DebugCommands.Instance.knockback_multiplier;
		this.owner.Move(v);
		if (!this.ignoreBounce && this._bounceScale > 0f)
		{
			Vector3 localScale = this.baseScale + (this.endScale - this.baseScale) * this._bounceCurve.Evaluate(time);
			this._bounceRoot.localScale = localScale;
		}
		if (this.timer <= 0f)
		{
			this.isHit = false;
			base.enabled = false;
			if (this._bounceScale > 0f)
			{
				this._bounceRoot.localScale = this.baseScale;
			}
		}
	}
}
