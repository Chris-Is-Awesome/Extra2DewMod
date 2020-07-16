using System;
using UnityEngine;

[AddComponentMenu("Ittle 2/Entity/Attacks/Projectile")]
public class ProjectileAttack : Attack, IUpdatable, IBaseUpdateable
{
	[SerializeField]
	private Projectile _projPrefab;

	[SerializeField]
	private BaseQuickEffect _attackEffect;

	[SerializeField]
	private BaseQuickEffect _startEffect;

	[SerializeField]
	private BaseEffect _contEffect;

	[SerializeField]
	private Range _contEffectRange;

	[SerializeField]
	private Vector3 _placeOffset = Vector3.zero;

	[SerializeField]
	private bool _rotateOffset;

	[SerializeField]
	private HitData.BaseDamage _damage;

	[SerializeField]
	private LayerMask _traceLayer;

	[SerializeField]
	private int _numProjectiles = 1;

	[SerializeField]
	private float _spreadAngle;

	[SerializeField]
	private float _rotateAngle;

	[SerializeField]
	private float _deviation;

	[SerializeField]
	private float _fireTime;

	[SerializeField]
	private bool _singleProjectile;

	[SerializeField]
	private float _projCooldown;

	[SerializeField]
	private bool _placeTrace;

	[SerializeField]
	private Vector3 _placeTraceOffset = Vector3.zero;

	[SerializeField]
	private LayerMask _placeTraceLayer;

	[SerializeField]
	private string _attackTypeName = "proj";

	[SerializeField]
	private string _attackSrcTag = "proj";

	private Projectile.OnEventFunc callback;

	private Projectile currentProjectile;

	private float cooldownTimer;

	private float fireTimer;

	private int shotsFired;

	private bool playedStartEffect;

	private bool playedContEffect;

	private PoolReference<BaseEffect> effectHandle;

	//Variable added for multiple projectiles
	public int debug_projectile_count = 1;

	public ProjectileAttack ()
	{
	}

	public override string GetAttackTypeString ()
	{
		return this._attackTypeName;
	}

	protected override void DoEnable (Entity owner)
	{
		base.DoEnable(owner);
		this.callback = new Projectile.OnEventFunc(this.ProjectileDisabled);
		if (this._contEffectRange.max <= this._contEffectRange.min)
		{
			this._contEffectRange = new Range(this._contEffectRange.min, this._contEffectRange.min + 10f);
		}
	}

	private bool IsPositionFree (Vector3 P)
	{
		Vector3 to = this.owner.RealTransform.TransformPoint(this._placeTraceOffset);
		return !PhysicsUtility.SphereCastAny(P, to, 0.1f, this._placeTraceLayer, false);
	}

	private void ProjectileDisabled (Projectile proj)
	{
		proj.OnDisabled -= this.callback;
		if (proj == this.currentProjectile)
		{
			this.currentProjectile = null;
			this.cooldownTimer = this._projCooldown;
		}
	}

	private bool DoFire (Vector3 dir, int n, bool withEffect, Transform target)
	{
		Vector3 point = this._placeOffset;
		if (this._numProjectiles > 1 || this._deviation > 0f)
		{
			Vector3 up = this.owner.RealTransform.up;
			float num = this._rotateAngle + this._spreadAngle * ((float)n - (float)(this._numProjectiles - 1) * 0.5f) / (float)this._numProjectiles;
			if (this._deviation > 0f)
			{
				num += UnityEngine.Random.Range(-this._deviation, this._deviation) * 0.5f;
			}
			Quaternion rotation = Quaternion.AngleAxis(num, up);
			dir = rotation * dir;
			if (this._rotateOffset)
			{
				point = rotation * point;
			}
		}
		HitData damage = new HitData(this.owner.WorldTracePosition, dir, this.owner, this._damage);
		damage.SetSrcTagIfEmpty(this._attackSrcTag);
		this.owner.LocalMods.ModifyAttack(ref damage);
		Projectile.ProjectileData data = new Projectile.ProjectileData(damage, this._traceLayer.value, dir, this.owner, 1f, target);
		Vector3 p = this.owner.WorldPosition + this.owner.RealTransform.rotation * point;
		if (this._placeTrace && !this.IsPositionFree(p))
		{
			return false;
		}
		//For loop added for multiple projectiles
		for (int i = 0; i < debug_projectile_count; i++)
		{
			this.currentProjectile = ProjectileFactory.Instance.GetProjectile(this._projPrefab, null, p, data);
		}
		if (this._singleProjectile)
		{
			this.currentProjectile.OnDisabled += this.callback;
		}
		return true;
	}

	private bool IsInCooldown ()
	{
		return this.cooldownTimer > 0f;
	}

	protected override void DoAttack (Vector3 dir, EntityAction.ActionData inData)
	{
		if (this._singleProjectile)
		{
			if (this.currentProjectile != null && this.currentProjectile.gameObject.activeInHierarchy)
			{
				return;
			}
			if (this.IsInCooldown())
			{
				return;
			}
		}
		else if (this._projCooldown > 0f)
		{
			if (this.IsInCooldown())
			{
				return;
			}
			this.cooldownTimer = this._projCooldown;
			base.enabled = true;
		}
		if (this._fireTime <= 0f)
		{
			bool flag = false;
			for (int i = 0; i < this._numProjectiles; i++)
			{
				flag |= this.DoFire(dir, i, true, inData.target);
			}
			if (flag)
			{
				EffectFactory.Instance.PlayQuickEffect(this._attackEffect, this.owner.WorldTracePosition, dir, this.owner.SoundContext);
				return;
			}
		}
		else
		{
			this.DoFire(dir, 0, true, inData.target);
			this.shotsFired = 1;
			this.fireTimer = 0f;
		}
	}

	public override void AttackStarted ()
	{
		base.AttackStarted();
		this.playedStartEffect = false;
		this.playedContEffect = false;
		this.shotsFired = 0;
	}

	public override void AttackStopped (bool cancel, Vector3 dir, EntityAction.ActionData inData)
	{
		if (this.effectHandle != null && this.effectHandle.IsValid)
		{
			this.effectHandle.Object.Stop();
		}
		this.effectHandle = null;
	}

	public override void DoUpdate (float t, Vector3 dir, EntityAction.ActionData inData)
	{
		if (!this.playedStartEffect)
		{
			this.playedStartEffect = true;
			EffectFactory.Instance.PlayQuickEffect(this._startEffect, this.owner.WorldPosition, dir, this.owner.SoundContext);
		}
		if (this._contEffect != null)
		{
			float min = this._contEffectRange.min;
			float max = this._contEffectRange.max;
			if (t >= min && t < max && this.effectHandle == null && !this.playedContEffect)
			{
				this.playedContEffect = true;
				this.effectHandle = EffectFactory.Instance.CreateEffect<BaseEffect>(this._contEffect, this.owner.MakeEffectData());
				if (this.effectHandle.IsValid)
				{
					this.effectHandle.Object.SetDirection(dir);
				}
			}
			else if (t >= max && this.effectHandle != null)
			{
				if (this.effectHandle.IsValid)
				{
					this.effectHandle.Object.Stop();
				}
				this.effectHandle = null;
			}
		}
		if (this._numProjectiles > 1 && this._fireTime > 0f && this.shotsFired > 0 && this.shotsFired < this._numProjectiles)
		{
			this.fireTimer = (t - base.AttackActionTime) / this._fireTime;
			while (this.fireTimer * (float)this._numProjectiles > (float)this.shotsFired)
			{
				if (this.shotsFired >= this._numProjectiles)
				{
					return;
				}
				this.DoFire(dir, this.shotsFired, true, inData.target);
				this.shotsFired++;
			}
			return;
		}
		base.DoUpdate(t, dir, inData);
	}

	void IUpdatable.UpdateObject ()
	{
		this.cooldownTimer -= Time.deltaTime;
		if (this.cooldownTimer < 0f)
		{
			base.enabled = false;
		}
	}

	//Function added for multiple projectiles
	private void Start ()
	{
		if (debug_projectile_count == 0) { debug_projectile_count = 1; }
	}
}
