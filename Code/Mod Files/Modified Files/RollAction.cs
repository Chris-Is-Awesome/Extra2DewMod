using System;
using UnityEngine;
using ModStuff;

[AddComponentMenu("Ittle 2/Entity/Actions/Roll")]
public class RollAction : EntityAction
{
	[SerializeField]
	string _anim;

	[SerializeField]
	string _loopAnim;

	[SerializeField]
	string _loopStopAnim;

	[SerializeField]
	public float _speed = 5f;

	[SerializeField]
	float _rollTime = 0.5f;

	[SerializeField]
	float _changeDirTimeMul = 0.25f;

	[SerializeField]
	float _invincibleTimeMul = 0.5f;

	[SerializeField]
	string _invincibleTimeMod = "rollsafe";

	[SerializeField]
	float _keepRollSlope = 0.25f;

	[SerializeField]
	bool _enableKeepRoll = true;

	[SerializeField]
	LayerMask _rollTraceLayer;

	[SerializeField]
	BaseEffect _invincibleEffect;

	[SerializeField]
	AnimationCurve _speedMulCurve;

	[SerializeField]
	AnimationCurve _loopCurve;

	[SerializeField]
	bool _affectMoveSpeed;

	[SerializeField]
	int _halfTimePrio = 1;

	[SerializeField]
	float _halfTimeMul = 0.75f;

	[SerializeField]
	SoundClip _sound;

	Vector3 rollDir;

	float timer;

	float realInvinciTime;

	EntityAnimator.AnimatorState animState;

	EntityHittable hittable;

	HittableObjectData.DisableTag hitDisable;

	PoolReference<BaseEffect> invincEffect;

	CollisionDetector collisionDetector;

	Vector3 startPos;

	bool keepRolling;

	bool isReleased;

	float time;

	void FreeHit ()
	{
		if (this.hitDisable != null)
		{
			this.hitDisable.Free();
		}
		if (this.invincEffect != null && this.invincEffect.IsValid)
		{
			this.invincEffect.Object.Stop();
		}
		this.hitDisable = null;
		this.invincEffect = null;
	}

	protected override int GetPrio ()
	{
		if (this.timer >= this._halfTimeMul * this._rollTime)
		{
			return this._halfTimePrio;
		}
		return base.GetPrio();
	}

	protected override void DoEnable (Entity owner)
	{
		this.hittable = owner.GetEntityComponent<EntityHittable>();
		if (this._enableKeepRoll)
		{
			this.collisionDetector = CollisionDetector.GetOrAdd(owner.gameObject, false);
		}
	}

	bool CheckSlope (Vector3 dir, out Vector3 newDir)
	{
		Vector3 worldTracePosition = this.owner.WorldTracePosition;
		Vector3 to = worldTracePosition - Vector3.up;
		BC_TraceHit bc_TraceHit;
		if (BC_Tracing.Instance.RayCast(worldTracePosition, to, out bc_TraceHit, this._rollTraceLayer, false))
		{
			Vector3 rhs = -bc_TraceHit.normal;
			float num3 = Vector3.Dot(dir, rhs);
			float num2 = Vector3.Dot(Vector3.up, rhs);
			if (num3 > 0.1f && num2 < 1f - this._keepRollSlope)
			{
				this.keepRolling = true;
				rhs.y = dir.y;
				newDir = rhs.normalized;
				return true;
			}
		}
		newDir = dir;
		return false;
	}

	protected override void DoAction (EntityAction.ActionData data)
	{
		this.time = 0f;
		this.rollDir = data.dir;
		this.timer = 0f;
		if (this._sound != null)
		{
			SoundPlayer.Instance.PlayPositionedSound(this._sound, this.owner.WorldPosition, this.owner.SoundContext);
		}
		this.keepRolling = false;
		if (this._enableKeepRoll)
		{
			this.keepRolling = this.CheckSlope(this.rollDir, out this.rollDir);
		}
		this.owner.TurnTo(this.rollDir, 0f);
		if (this.keepRolling)
		{
			this.animState = this.owner.PlayAnimation(this._loopAnim, 0);
		}
		else
		{
			this.animState = this.owner.PlayAnimation(this._anim, 0);
		}
		if (!string.IsNullOrEmpty(this._invincibleTimeMod))
		{
			this.realInvinciTime = this._invincibleTimeMul * this.owner.LocalMods.GeneralFloatMultipler(this._invincibleTimeMod, 1f);
		}
		else
		{
			this.realInvinciTime = this._invincibleTimeMul;
		}
		this.isReleased = false;
		this.startPos = this.owner.WorldPosition;
		if (this.hittable != null)
		{
			this.hitDisable = this.hittable.PushDisableTag();
			if (this._invincibleEffect != null)
			{
				this.invincEffect = EffectFactory.Instance.CreateEffect<BaseEffect>(this._invincibleEffect, this.owner.MakeEffectData());
			}
		}
	}

	protected override void StopAction (bool cancel)
	{
		this.timer = 0f;
		this.FreeHit();
	}

	protected override bool CheckCanRelease
	{
		get
		{
			return !this.isReleased;
		}
	}

	protected override void DoReleaseAction ()
	{
		this.isReleased = true;
	}

	protected override bool DoUpdate ()
	{
		this.timer += Time.deltaTime;
		float num = Mathf.Clamp01(this.timer / this._rollTime);
		if (this.animState != null && !this.keepRolling)
		{
			this.animState.timeValue = num;
		}
		if (num >= this.realInvinciTime && this.hitDisable != null)
		{
			this.FreeHit();
		}
		if (this._enableKeepRoll && !this.keepRolling && this.CheckSlope(this.rollDir, out this.rollDir))
		{
			this.keepRolling = true;
			this.owner.TurnTo(this.rollDir, 0f);
			this.animState = this.owner.PlayAnimation(this._loopAnim, 0);
			this.animState.timeValue = num;
		}
		float num2 = ((!this.keepRolling) ? this._speedMulCurve : this._loopCurve).Evaluate(num) * this._speed;
		if (this._affectMoveSpeed)
		{
			num2 *= this.owner.LocalMods.MoveSpeedMultiplier;
		}
		this.owner.Move(this.rollDir * num2);
		if (this.keepRolling && !this.isReleased && this.collisionDetector != null && this.collisionDetector.IsColliding)
		{
			this.isReleased = true;
		}
		if (this.timer >= this._rollTime)
		{
			if (!this.keepRolling || this.isReleased)
			{
				if (this.keepRolling)
				{
					this.owner.PlayAnimation(this._loopStopAnim, 0);
				}
				return false;
			}
			this.timer = 0f;
		}
		return true;
	}

	protected override bool DoUpdateData (EntityAction.ActionData data)
	{
		if (this.keepRolling)
		{
			return false;
		}
		if (Mathf.Clamp01(this.timer / this._rollTime) < this._changeDirTimeMul)
		{
			this.rollDir = data.dir;
			this.owner.TurnTo(this.rollDir, 0f);
			return true;
		}
		return false;
	}

	public override bool GetActionData (string dataName, out float value)
	{
		if (dataName == "rolldist")
		{
			value = Vector3.Distance(this.owner.WorldPosition, this.startPos);
			return true;
		}
		return base.GetActionData(dataName, out value);
	}

	// Update stat for best roll time
	void FixedUpdate ()
	{
		if (GameObject.Find("PauseOverlay") == null)
		{
			time += Time.fixedDeltaTime;

			if (keepRolling && time > ModSaver.LoadFloatFromPrefs("BestRollTime"))
			{
				ModSaver.SaveFloatToPrefs("BestRollTime", time);
			}
		}
	}
}
