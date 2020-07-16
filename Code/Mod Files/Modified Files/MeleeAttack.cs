using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Ittle 2/Entity/Attacks/Melee")]
public class MeleeAttack : Attack
{
	[SerializeField]
	private float _attackEndTime;

	[SerializeField]
	private HitData.BaseDamage _damage;

	[SerializeField]
	private float _traceAngle = 180f;

	[SerializeField]
	private float _traceRadius = 1.5f;

	[SerializeField]
	private float _rotateDirAngle;

	[SerializeField]
	private float _randomDirAngle;

	[SerializeField]
	private float _traceBulge;

	[SerializeField]
	private LayerMask _traceLayer = default(LayerMask);

	[SerializeField]
	private MeleeAttack.TraceData[] _addTraces;

	[SerializeField]
	private BaseQuickEffect _hitEffect;

	[SerializeField]
	private BaseQuickEffect _attackEffect;

	[SerializeField]
	private BaseQuickEffect _startEffect;

	[SerializeField]
	private BaseEffect _contEffect;

	[SerializeField]
	private SoundClip _attackSnd;

	[SerializeField]
	private string _attackTypeName = "melee";

	[SerializeField]
	private string _attackSrcTag = "melee";

	[SerializeField]
	private MeleeAttack.Mode[] _modes;

	private MeleeAttack.Mode currentMode;

	private MeleeAttack.Mode.ModeState currentState;

	private MeleeAttack.AttackData currentOverride;

	private List<object> ignoreObjects = new List<object>();

	private bool playedStartEffect;

	private PoolReference<BaseEffect> effectHandle;

	private bool didHit;

	private MeleeAttack.TraceData[] savedTraceData;

	private AttackUtility.HitResultFunc savedFunc;

	//Added for extra range
	public float extra_bulge = 0f;

	public MeleeAttack ()
	{
	}

	private static MeleeAttack.TraceData[] CombineData (MeleeAttack.TraceData baseData, MeleeAttack.TraceData[] addData)
	{
		MeleeAttack.TraceData[] array;
		if (addData == null || addData.Length == 0)
		{
			array = new MeleeAttack.TraceData[]
			{
				baseData
			};
		}
		else
		{
			array = new MeleeAttack.TraceData[1 + addData.Length];
			array[0] = baseData;
			Array.Copy(addData, 0, array, 1, addData.Length);
		}
		return array;
	}

	public override string GetAttackTypeString ()
	{
		return this._attackTypeName;
	}

	private MeleeAttack.TraceData[] GetBaseTraceData ()
	{
		if (this.savedTraceData == null)
		{
			this.savedTraceData = MeleeAttack.CombineData(new MeleeAttack.TraceData(this._traceLayer, this._traceAngle, this._traceRadius, this._traceBulge, this._rotateDirAngle, this._randomDirAngle), this._addTraces);
		}
		return this.savedTraceData;
	}

	private bool CheckOverride (MeleeAttack.AttackData.SetMode mode)
	{
		return this.currentMode != null && (this.currentOverride.mode & mode) != (MeleeAttack.AttackData.SetMode)0;
	}

	private HitData.BaseDamage Damage
	{
		get
		{
			return (!this.CheckOverride(MeleeAttack.AttackData.SetMode.Damage)) ? this._damage : this.currentOverride.damage;
		}
	}

	private MeleeAttack.TraceData[] GetTraceData ()
	{
		return (!this.CheckOverride(MeleeAttack.AttackData.SetMode.TraceData)) ? this.GetBaseTraceData() : this.currentOverride.GetFullTraceData();
	}

	private BaseQuickEffect HitEffect
	{
		get
		{
			return (!this.CheckOverride(MeleeAttack.AttackData.SetMode.HitEffect)) ? this._hitEffect : this.currentOverride.hitEffect;
		}
	}

	private BaseQuickEffect AttackEffect
	{
		get
		{
			return (!this.CheckOverride(MeleeAttack.AttackData.SetMode.AttackEffect)) ? this._attackEffect : this.currentOverride.attackEffect;
		}
	}

	private BaseQuickEffect StartEffect
	{
		get
		{
			return this._startEffect;
		}
	}

	private BaseEffect ContEffect
	{
		get
		{
			return this._contEffect;
		}
	}

	private SoundClip AttackSound
	{
		get
		{
			return (!this.CheckOverride(MeleeAttack.AttackData.SetMode.AttackSound)) ? this._attackSnd : this.currentOverride.attackSnd;
		}
	}

	protected override void DoActivate ()
	{
		base.DoActivate();
		this.currentMode = null;
		this.currentOverride = null;
		this.currentState = null;
		this.ignoreObjects.Clear();
	}

	protected override void DoDeactivate ()
	{
		if (this.currentState != null)
		{
			this.currentState.Stop();
		}
		this.currentState = null;
		base.DoDeactivate();
	}

	private void ModeStateDone ()
	{
		if (this.currentMode != null && this.currentMode.nextState >= 0)
		{
			this.SwitchMode(this._modes[this.currentMode.nextState], true);
		}
		else
		{
			this.SwitchMode(null, false);
		}
	}

	private bool SwitchMode (MeleeAttack.Mode mode, bool forceEnd = false)
	{
		if (mode != null && !forceEnd && !mode.CanOverride(this.currentMode))
		{
			return false;
		}
		if (this.currentState != null)
		{
			this.currentState.Stop();
		}
		this.currentMode = mode;
		this.currentOverride = ((mode == null || !(mode.data != null)) ? null : mode.data.Data);
		if (this.currentMode != null)
		{
			this.currentState = new MeleeAttack.Mode.ModeState(this.currentMode, this.owner, new MeleeAttack.Mode.ModeState.OnDoneFunc(this.ModeStateDone), this);
		}
		return true;
	}

	private void HandleHitResult (HitData inData, HitResult result, Hittable hittable)
	{
		this.didHit = true;
		this.ignoreObjects.Add(hittable.HitTarget);
		if (this._modes != null && this._modes.Length > 0 && result.HasData)
		{
			bool flag = false;
			for (int i = 0; i < this._modes.Length; i++)
			{
				if (result.HasDataKey(this._modes[i].name) && this._modes[i].value == result.GetKey(this._modes[i].name) && this.SwitchMode(this._modes[i], false))
				{
					flag = true;
					if (this.currentMode != null && this.currentMode.startEffect != null)
					{
						EffectFactory.Instance.PlayQuickEffect(this.currentMode.startEffect, inData.Point, inData.Normal, this.owner.SoundContext);
					}
					break;
				}
			}
			if (!flag && this.currentMode != null && this.currentMode.endOn == MeleeAttack.Mode.EndMode.Hit)
			{
				this.currentState.SendHit();
			}
		}
		bool flag2 = false;
		HitData.DamageInstance.Data[] array = result.hitData.GetDamageData() ?? inData.GetDamageData();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].damage > 0f)
			{
				flag2 = true;
				break;
			}
		}
		if (flag2 && result.result != HitResult.ResultMode.Soft)
		{
			BaseQuickEffect baseQuickEffect;
			if (base.Graphics != null)
			{
				baseQuickEffect = (base.Graphics.HitEffect ?? this.HitEffect);
			}
			else
			{
				baseQuickEffect = this._hitEffect;
			}
			BaseQuickEffect.EffectData effectData = new BaseQuickEffect.EffectData();
			effectData.pos = inData.Point;
			effectData.normal = inData.Normal;
			effectData.sndContext = this.owner.SoundContext;
			if (baseQuickEffect != null)
			{
				EffectFactory.Instance.PlayQuickEffect(baseQuickEffect, effectData);
			}
			this.owner.LocalMods.PlayAdditionalEffect(effectData, EntityDataModifier.AdditionalEffectType.AttackHitEffect);
		}
		if (result.knockback > 0f)
		{
			Knockbackable entityComponent = this.owner.GetEntityComponent<Knockbackable>();
			if (entityComponent != null)
			{
				entityComponent.StartKnockback(inData.Normal, result.knockback, true);
			}
		}
	}

	public override void AttackStarted ()
	{
		base.AttackStarted();
		this.ignoreObjects.Clear();
		this.ignoreObjects.Add(this.owner);
		this.playedStartEffect = false;
		this.didHit = false;
	}

	public override void AttackStopped (bool cancel, Vector3 dir, EntityAction.ActionData inData)
	{
		if (this.effectHandle != null)
		{
			if (this.effectHandle.IsValid)
			{
				this.effectHandle.Object.Stop();
			}
			this.effectHandle = null;
		}
	}

	public override void DoUpdate (float t, Vector3 dir, EntityAction.ActionData inData)
	{
		if (!this.playedStartEffect)
		{
			this.playedStartEffect = true;
			EffectFactory.Instance.PlayQuickEffect(this.StartEffect, this.owner.WorldTracePosition, dir, this.owner.SoundContext);
			SoundClip attackSound = this.AttackSound;
			if (attackSound != null)
			{
				SoundPlayer.Instance.PlayPositionedSound(attackSound, this.owner.WorldTracePosition, this.owner.SoundContext);
			}
			BaseEffect contEffect = this.ContEffect;
			if (contEffect != null)
			{
				this.effectHandle = EffectFactory.Instance.CreateEffect<BaseEffect>(contEffect, this.owner.MakeEffectData());
				if (this.effectHandle.IsValid)
				{
					this.effectHandle.Object.SetDirection(dir);
				}
			}
		}
		if (this._attackEndTime < base.AttackActionTime)
		{
			base.DoUpdate(t, dir, inData);
		}
		else if (t >= base.AttackActionTime && t <= this._attackEndTime)
		{
			this.didAction = true;
			this.DoAttack(dir, inData);
		}
	}

	private static List<BC_TraceHit> DoSingleTrace (Vector3 P, Vector3 dir, MeleeAttack.TraceData data, List<BC_TraceHit> inHits, float extra_bulge, out Vector3 eDir)
	{
		float traceBulge = data.traceBulge;
		float angle = Mathf.Cos(data.traceAngle * 0.0174532924f * 0.5f);
		float traceRadius = data.traceRadius;
		int value = data.traceLayer.value;
		if (Mathf.Abs(data.rotateDirAngle) > 0f)
		{
			dir = Quaternion.AngleAxis(data.rotateDirAngle, Vector3.up) * dir;
		}
		if (data.randomDirAngle > 0f)
		{
			float num = data.randomDirAngle * 0.5f;
			dir = Quaternion.AngleAxis(UnityEngine.Random.Range(-num, num), Vector3.up) * dir;
		}
		eDir = dir;
		List<BC_TraceHit> list;
		//If there is extra range
		if ((traceBulge + extra_bulge) < 0.001f)
		{
			list = PhysicsUtility.ConeTraceAll(P, dir, traceRadius, angle, value, true);
		}
		else
		{
			//Changed bulge to add extra range
			list = PhysicsUtility.ConeBulgeTraceAll(P, dir, traceRadius, angle, traceBulge + extra_bulge, value, true);
		}
		if (inHits == null || inHits.Count == 0)
		{
			return list;
		}
		for (int i = list.Count - 1; i >= 0; i--)
		{
			BC_Collider collider = list[i].collider;
			bool flag = false;
			for (int j = inHits.Count - 1; j >= 0; j--)
			{
				if (inHits[j].collider == collider)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				inHits.Add(list[i]);
			}
		}
		return inHits;
	}

	protected override void DoAttack (Vector3 dir, EntityAction.ActionData inData)
	{
		MeleeAttack.TraceData[] traceData = this.GetTraceData();
		List<BC_TraceHit> list = null;
		Vector3 worldTracePosition = this.owner.WorldTracePosition;
		for (int i = 0; i < traceData.Length; i++)
		{
			Vector3 dir2;
			list = MeleeAttack.DoSingleTrace(worldTracePosition, dir, traceData[i], list, extra_bulge, out dir2);
			if (i == 0)
			{
				BaseQuickEffect attackEffect = this.AttackEffect;
				if (attackEffect != null)
				{
					EffectFactory.Instance.PlayQuickEffect(attackEffect, this.owner.WorldTracePosition, dir2, this.owner.SoundContext);
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			if (this.savedFunc == null)
			{
				this.savedFunc = new AttackUtility.HitResultFunc(this.HandleHitResult);
			}
			HitData baseHitData = new HitData(this.owner.WorldTracePosition, this.owner.ForwardVector, this.owner, this.Damage);
			baseHitData.SetSrcTagIfEmpty(this._attackSrcTag);
			this.owner.LocalMods.ModifyAttack(ref baseHitData);
			List<AttackUtility.HitInput> hits = AttackUtility.ConvertInput(list);
			AttackUtility.HandleHits(hits, this.ignoreObjects, baseHitData, this.savedFunc);
		}
	}

	public override bool HitFlag
	{
		get
		{
			return this.didHit;
		}
	}

	[Serializable]
	public class TraceData
	{
		public LayerMask traceLayer = default(LayerMask);

		public float traceAngle = 180f;

		public float traceRadius = 1.5f;

		public float traceBulge;

		public float rotateDirAngle;

		public float randomDirAngle;

		public TraceData (LayerMask mask, float angle, float radius, float bulge, float rotate, float random)
		{
			this.traceLayer = mask;
			this.traceAngle = angle;
			this.traceRadius = radius;
			this.traceBulge = bulge;
			this.rotateDirAngle = rotate;
			this.randomDirAngle = random;
		}
	}

	[Serializable]
	public class AttackData
	{
		public HitData.BaseDamage damage;

		public BaseQuickEffect hitEffect;

		public BaseQuickEffect attackEffect;

		public SoundClip attackSnd;

		public LayerMask traceLayer = default(LayerMask);

		public float traceAngle = 180f;

		public float traceRadius = 1.5f;

		public float traceBulge;

		public float rotateAngle;

		public float randomAngle;

		public MeleeAttack.TraceData[] addTraces;

		[BitMask(typeof(MeleeAttack.AttackData.SetMode))]
		public MeleeAttack.AttackData.SetMode mode;

		private MeleeAttack.TraceData[] savedTraces;

		public AttackData ()
		{
		}

		public MeleeAttack.TraceData[] GetFullTraceData ()
		{
			if (this.savedTraces == null)
			{
				this.savedTraces = MeleeAttack.CombineData(new MeleeAttack.TraceData(this.traceLayer, this.traceAngle, this.traceRadius, this.traceBulge, this.rotateAngle, this.randomAngle), this.addTraces);
			}
			return this.savedTraces;
		}

		[Flags]
		public enum SetMode
		{
			Damage = 1,
			Layer = 2,
			TraceData = 4,
			Radius = 8,
			HitEffect = 16,
			AttackEffect = 32,
			AttackSound = 64
		}
	}

	[Serializable]
	public class Mode
	{
		public string name = string.Empty;

		public int value;

		public MeleeAttack.Mode.EndMode endOn = MeleeAttack.Mode.EndMode.Hit;

		public int nextState = -1;

		public bool valuePrio = true;

		public float time;

		public int evCount = 1;

		public MeleeAttackData data;

		public BaseQuickEffect startEffect;

		public BaseEffect modeEffect;

		public bool effectOnWeapon;

		public Mode ()
		{
		}

		public bool CanOverride (MeleeAttack.Mode old)
		{
			return old == null || (this.valuePrio && this.value > old.value);
		}

		public class ModeState
		{
			private MeleeAttack.Mode mode;

			private Entity ent;

			private MeleeAttack.Mode.ModeState.OnDoneFunc onDone;

			private Timer timer;

			private int counter;

			private PoolReference<BaseEffect> modeEffect;

			public ModeState (MeleeAttack.Mode mode, Entity owner, MeleeAttack.Mode.ModeState.OnDoneFunc onDone, Attack attack)
			{
				this.onDone = onDone;
				this.mode = mode;
				this.counter = mode.evCount;
				if (mode.endOn == MeleeAttack.Mode.EndMode.Damage)
				{
					owner.LocalEvents.DamageListener += this.OnDamaged;
					this.ent = owner;
				}
				else if (mode.endOn == MeleeAttack.Mode.EndMode.RoomChange)
				{
					owner.LocalEvents.RoomChangeListener += this.RoomChanged;
					this.ent = owner;
				}
				else if (mode.endOn == MeleeAttack.Mode.EndMode.Time)
				{
					this.timer = owner.LocalTimers.StartNew(mode.time, new Timer.OnDoneFunc(this.SendDone));
				}
				if (mode.modeEffect != null)
				{
					BaseEffect.EffectData effectData = owner.MakeEffectData();
					if (mode.effectOnWeapon)
					{
						WeaponGraphics graphics = attack.Graphics;
						if (graphics != null)
						{
							effectData.target = graphics.transform.parent;
						}
					}
					this.modeEffect = EffectFactory.Instance.CreateEffect<BaseEffect>(mode.modeEffect, effectData);
				}
			}

			private void SendDone ()
			{
				MeleeAttack.Mode.ModeState.OnDoneFunc onDoneFunc = this.onDone;
				this.onDone = null;
				this.Stop();
				if (onDoneFunc != null)
				{
					onDoneFunc();
				}
			}

			private void SendCount ()
			{
				this.counter--;
				if (this.counter <= 0)
				{
					this.SendDone();
				}
			}

			private void OnDamaged (Entity ent, HitData data)
			{
				this.SendCount();
			}

			private void RoomChanged (Entity ent, LevelRoom to, LevelRoom from, EntityEventsOwner.RoomEventData data)
			{
				this.SendCount();
			}

			public void SendHit ()
			{
				this.SendCount();
			}

			public void Stop ()
			{
				if (this.timer != null)
				{
					this.timer.Stop();
					this.timer = null;
				}
				this.onDone = null;
				if (this.mode.endOn == MeleeAttack.Mode.EndMode.Damage && this.ent != null)
				{
					this.ent.LocalEvents.DamageListener -= this.OnDamaged;
				}
				if (this.mode.endOn == MeleeAttack.Mode.EndMode.RoomChange && this.ent != null)
				{
					this.ent.LocalEvents.RoomChangeListener -= this.RoomChanged;
				}
				if (this.modeEffect != null && this.modeEffect.IsValid)
				{
					this.modeEffect.Object.Stop();
				}
				this.ent = null;
			}

			public delegate void OnDoneFunc ();
		}

		public enum EndMode
		{
			None,
			Hit,
			Damage,
			RoomChange,
			Time
		}
	}
}
