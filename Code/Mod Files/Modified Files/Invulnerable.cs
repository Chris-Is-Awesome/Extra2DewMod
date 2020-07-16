using System;
using UnityEngine;

[AddComponentMenu("Ittle 2/Entity/Hittable/Invulnerable")]
public class Invulnerable : EntityHitListener, IUpdatable, IBaseUpdateable
{
	[SerializeField]
	Invulnerable.AngleData[] _angles;

	[SerializeField]
	bool _anyDamage;

	[SerializeField]
	bool _elementImmunity;

	[SerializeField]
	DamageType[] _damageElements;

	[SerializeField]
	bool _invertElements;

	[SerializeField]
	string[] _attackSrcTags;

	[SerializeField]
	bool _invertSrcTags;

	[SerializeField]
	float _invulnerableTime;

	[SerializeField]
	bool _stopHitChain;

	[SerializeField]
	BaseQuickEffect _blockEffect;

	Invulnerable.State state;

	protected override void DoActivate()
	{
		this.state = new Invulnerable.State();
	}

	static bool ContainsElement(DamageType t, DamageType[] els)
	{
		for (int i = 0; i < els.Length; i++)
		{
			if (els[i] == t)
			{
				return true;
			}
		}
		return false;
	}

	static float TotalDmg(HitData.DamageInstance.Data[] data)
	{
		float num = 0f;
		if (data != null)
		{
			for (int i = data.Length - 1; i >= 0; i--)
			{
				num += data[i].damage;
			}
		}
		return num;
	}

	bool AngleBlocks(Vector3 attack)
	{
		if (this._angles != null)
		{
			for (int i = this._angles.Length - 1; i >= 0; i--)
			{
				Invulnerable.AngleData angleData = this._angles[i];
				Vector3 vector;
				if (angleData.absAngle)
				{
					vector = angleData.forward;
				}
				else
				{
					vector = this.owner.RealTransform.TransformDirection(angleData.forward);
				}
				float num = Vector3.Dot(vector, attack);
				if (angleData.abs)
				{
					num = Mathf.Abs(num);
				}
				float num2 = Mathf.Cos(angleData.angle * 0.5f * 0.0174532924f);
				if (num >= num2)
				{
					return true;
				}
			}
		}
		return false;
	}

	bool BlockDamage(HitData.DamageInstance.Data[] dmgData)
	{
		bool flag = false;
		bool flag2 = true;
		for (int i = 0; i < dmgData.Length; i++)
		{
			DamageType type = dmgData[i].type;
			if (this._anyDamage || (Invulnerable.ContainsElement(type, this._damageElements) ^ this._invertElements))
			{
				dmgData[i].damage = 0f;
				flag = true;
			}
			else
			{
				flag2 = false;
			}
		}
		return flag && flag2;
	}

	bool AttackSrcBlock(string src)
	{
		return !string.IsNullOrEmpty(src) && this._attackSrcTags != null && this._attackSrcTags.Length != 0 && Array.IndexOf<string>(this._attackSrcTags, base.tag) != -1;
	}

	public override bool HandleHit(ref HitData data, ref HitResult inResult)
	{
		bool flag = this.state.hitTimer > 0f;
		if (!flag)
		{
			Vector3 attack = data.Point - this.owner.WorldPosition;
			attack.y = 0f;
			attack.Normalize();
			if (this.AngleBlocks(attack) || (this.AttackSrcBlock(data.SrcTag) ^ this._invertSrcTags))
			{
				EffectFactory.Instance.PlayQuickEffect(this._blockEffect, data.Point, data.Normal, this.owner.SoundContext);
				inResult.result = HitResult.ResultMode.Hard;
				flag = true;
			}
		}
		if ((flag || this._elementImmunity) && this.BlockDamage(data.GetDamageData()))
		{
			// Allow killing normally invincible enemies during LikeABoss
			if (DebugCommands.Instance.likeABoss) { return true; }

			return !this._stopHitChain;
		}
		if (this._invulnerableTime > 0f && Invulnerable.TotalDmg(data.GetDamageData()) > 0f)
		{
			this.state.hitTimer = this._invulnerableTime;
			this.state.isHit = true;
			base.enabled = true;
		}
		return true;
	}

	void IUpdatable.UpdateObject()
	{
		if (!this.state.isHit || this.state.hitTimer < 0f)
		{
			base.enabled = false;
			return;
		}
		this.state.hitTimer -= Time.deltaTime;
	}

	public class State
	{
		public float hitTimer;

		public bool isHit;

		public State()
		{
			this.hitTimer = 0f;
		}
	}

	[Serializable]
	public class AngleData
	{
		public Vector3 forward = Vector3.forward;

		public float angle = 90f;

		public bool abs;

		public bool absAngle;
	}
}
