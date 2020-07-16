using System;
using UnityEngine;

[AddComponentMenu("Ittle 2/Entity/Moveable")]
public class Moveable : EntityComponent, IUpdatable, IBaseUpdateable
{
	[SerializeField]
	private bool _isDefault;

	[SerializeField]
	private string _name;

	public float _moveSpeed = 4f; //Changed to public non-serialize

	[SerializeField]
	private float _turnSpeed = 1080f;

	[SerializeField]
	private bool _turnToDir = true;

	[SerializeField]
	private bool _moveInTurnDir;

	[SerializeField]
	private float _turnFactor = 1f;

	[SerializeField]
	private Vector3 _turnVector = Vector3.forward;

	[SerializeField]
	private string _moveAnim;

	[SerializeField]
	private string _stopAnim;

	[SerializeField]
	private bool _useDefaultStopAnim = true;

	[SerializeField]
	private float _animSpeedScale = 1f;

	[SerializeField]
	private bool _wallSlideNormalize;

	[SerializeField]
	private MoveEffectObject _moveEffectObject;

	private bool isMoving;

	private bool affectSpeed;

	private Vector3 lastDir;

	private EntityAnimator.AnimatorState animState;

	private CollisionDetector collisionDetector;

	private Vector3 lastPos;

	protected override void DoEnable (Entity owner)
	{
		if (!this._wallSlideNormalize)
		{
			this.collisionDetector = CollisionDetector.GetOrAdd(owner.gameObject, false);
		}
		if (string.IsNullOrEmpty(this._stopAnim) && this._useDefaultStopAnim)
		{
			this._stopAnim = owner.DefaultAnimName;
		}
		if (this._moveEffectObject != null)
		{
			this._moveEffectObject.RegOwner(owner);
		}
	}

	protected override void DoActivate ()
	{
		this.isMoving = false;
		this.animState = null;
	}

	public bool IsDefault
	{
		get
		{
			return this._isDefault;
		}
	}

	public string MoveableName
	{
		get
		{
			return this._name;
		}
	}

	public bool SetMoveDirection (Vector3 dir, bool affectSpeed = false)
	{
		if (!this.owner.RequestPriority(0))
		{
			this.isMoving = false;
			return false;
		}
		this.affectSpeed = affectSpeed;
		bool flag = this.isMoving;
		this.lastDir = dir;
		this.isMoving = true;
		base.enabled = true;
		if (!flag)
		{
			this.animState = this.owner.PlayAnimation(this._moveAnim, 0);
			if (this.animState != null && Mathf.Abs(this._animSpeedScale) < 1E-05f)
			{
				this.animState.speed = 1f;
			}
			if (this._moveEffectObject != null)
			{
				this._moveEffectObject.StartMoveEffect();
			}
		}
		return true;
	}

	public void Stop ()
	{
		bool flag = this.isMoving;
		this.isMoving = false;
		this.animState = null;
		if (flag)
		{
			if (this._moveEffectObject != null)
			{
				this._moveEffectObject.StopMoveEffect();
			}
			this.owner.FreePriority(0);
			if (!string.IsNullOrEmpty(this._stopAnim))
			{
				this.owner.PlayAnimation(this._stopAnim, 0);
			}
		}
	}

	public bool GetMoveDirection (out Vector3 dir)
	{
		if (this.isMoving)
		{
			dir = this.lastDir;
			return true;
		}
		dir = Vector3.zero;
		return false;
	}

	protected float MoveSpeed
	{
		get
		{
			if (!this.affectSpeed)
			{
				return this._moveSpeed;
			}
			return this._moveSpeed * this.owner.LocalMods.MoveSpeedMultiplier;
		}
	}

	protected virtual Vector3 GetVelocity ()
	{
		Vector3 a = this.lastDir;
		if (this._wallSlideNormalize || this.collisionDetector == null || !this.collisionDetector.IsColliding)
		{
			a.Normalize();
		}
		return a * this.MoveSpeed;
	}

	void IUpdatable.UpdateObject ()
	{
		if (!this.isMoving)
		{
			base.enabled = false;
			return;
		}
		Vector3 v = this.GetVelocity();
		if (this._moveInTurnDir)
		{
			float magnitude = v.magnitude;
			v = this.owner.ForwardVector * magnitude;
		}
		this.owner.Move(v);
		if (this.animState != null && Mathf.Abs(this._animSpeedScale) > 1E-05f)
		{
			Vector3 b = this.lastPos;
			this.lastPos = this.owner.WorldPosition;
			float num = (this.lastPos - b).magnitude / Time.deltaTime;
			this.animState.speed = this._animSpeedScale * (num / this._moveSpeed);
		}
		if (this._turnToDir)
		{
			if (this._turnFactor < 0.95f)
			{
				Vector3 normalized = Vector3.Lerp(this._turnVector, this.lastDir, this._turnFactor).normalized;
				this.owner.TurnTo(normalized, this._turnSpeed);
				return;
			}
			this.owner.TurnTo(this.lastDir, this._turnSpeed);
		}
	}
}
