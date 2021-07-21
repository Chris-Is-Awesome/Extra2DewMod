using System;
using System.Collections.Generic;
using UnityEngine;
using ModStuff;

[AddComponentMenu("Ittle 2/Entity/Killable")]
public class Killable : EntityHitListener, ILateUpdatable, IBaseUpdateable
{
	[SerializeField]
	Killable.Data _data;

	[SerializeField]
	public BaseQuickEffect _deathEffect;

	[SerializeField]
	public BaseQuickEffect _startDeathEffect;

	[SerializeField]
	string _deathAnim;

	[SerializeField]
	float _deathTime;

	[SerializeField]
	GameAction _deathAction;

	[SerializeField]
	GameAction _signalAction;

	[SerializeField]
	KillableOverrideData _deathOverride;

	[SerializeField]
	KillableOverrideData.OverrideData[] _customOverrideData;

	Dictionary<string, Component> overrideLookup = new Dictionary<string, Component>();

	Killable.State state;

	GameState gameState;

	ModeController mc;
	void Start()
	{
		// Setup for GameState
		gameState = Singleton<GameState>.Instance;

		// Set current and max HP when Heart Rush is enabled
		if (mc == null) { mc = GameObject.Find("ModeController").GetComponent<ModeController>(); }

		if (mc.isHeartRush && MaxHp == 20)
		{
			mc.heartRushManager.StartOfGame(this);
		}
	}

	protected override void DoEnable(Entity owner)
	{
		base.DoEnable(owner);
		if (this._customOverrideData != null)
		{
			for (int i = 0; i < this._customOverrideData.Length; i++)
			{
				this.overrideLookup.Add(this._customOverrideData[i].name, this._customOverrideData[i].value);
			}
		}
	}

	protected override void DoActivate()
	{
		this.state = new Killable.State(this._data);
	}

	protected override void DoReadSaveData(IDataSaver local, IDataSaver level)
	{
		if (local.HasData("hp"))
		{
			this.state.hp = local.LoadFloat("hp");
		}
		if (local.HasData("maxHp"))
		{
			this._data.hp = local.LoadFloat("maxHp");
		}
	}

	protected override void DoWriteSaveData(IDataSaver local, IDataSaver level)
	{
		local.SaveFloat("maxHp", this._data.hp);
		if (this.state.hp <= 0f)
		{
			local.SaveFloat("hp", this._data.hp);
			return;
		}
		local.SaveFloat("hp", this.state.hp);
	}

	public float CurrentHp
	{
		get
		{
			return this.state.hp;
		}
		set
		{
			this.state.hp = Mathf.Min(this._data.hp, value);
		}
	}

	public float MaxHp
	{
		get
		{
			return this._data.hp;
		}
		set
		{
			this._data.hp = value;
		}
	}

	string DeathAnim
	{
		get
		{
			if (this.state.deathData != null)
			{
				return this.state.deathData.deathAnim;
			}
			return this._deathAnim;
		}
	}

	float DeathTime
	{
		get
		{
			if (this.state.deathData != null)
			{
				return this.state.deathData.deathTime;
			}
			return this._deathTime;
		}
	}

	BaseQuickEffect DeathEffect
	{
		get
		{
			if (this.state.deathData != null)
			{
				return this.state.deathData.deathEffect;
			}
			return this._deathEffect;
		}
	}

	BaseQuickEffect StartDeathEffect
	{
		get
		{
			if (this.state.deathData != null)
			{
				return this.state.deathData.startDeathEffect;
			}
			return this._startDeathEffect;
		}
	}

	GameAction DeathAction
	{
		get
		{
			if (this.state.deathData != null)
			{
				return this.state.deathData.deathAction;
			}
			return this._deathAction;
		}
	}

	GameAction SignalAction
	{
		get
		{
			if (this.state.deathData != null)
			{
				return this.state.deathData.signalAction;
			}
			return this._signalAction;
		}
	}

	TransformUpdater DeathTransformer
	{
		get
		{
			if (this.state.deathData != null)
			{
				return this.state.deathData.deathTransformer;
			}
			return null;
		}
	}

	string DeathTag
	{
		get
		{
			if (this.state.deathData != null)
			{
				return this.state.deathData.deathTag;
			}
			return string.Empty;
		}
	}

	public bool SilentDeath
	{
		get
		{
			return this.state.deathData != null && this.state.deathData.silentDeath;
		}
	}

	public T GetOverrideComponent<T>(string name) where T : Component
	{
		if (string.IsNullOrEmpty(name))
		{
			return default(T);
		}
		Component component;
		this.overrideLookup.TryGetValue(name, out component);
		return component as T;
	}

	public void SignalDeath()
	{
		this.entityHittable.Disable = true;
		this.owner.DeathSignal = true;
		this.owner.LocalEvents.SendDetailedDeath(this.owner, new Killable.DetailedDeathData(this.DeathTag, this.CurrentHp));
		this.state.dead = true;
		base.enabled = true;
		if (!this.SilentDeath)
		{
			GameAction signalAction = this.SignalAction;
			if (signalAction != null)
			{
				signalAction.Execute(new GameAction.ActionData(this.owner.WorldPosition, 1f, null, this.owner)
				{
					rotation = this.owner.RealTransform.rotation
				});
			}
			this.state.timer = this.DeathTime;
			string deathAnim = this.DeathAnim;
			if (!string.IsNullOrEmpty(deathAnim))
			{
				this.owner.PlayAnimation(deathAnim, 0);
			}
			EffectFactory.Instance.PlayQuickEffect(this.StartDeathEffect, this.owner.WorldPosition, this.owner.ForwardVector, this.owner.SoundContext);
			TransformUpdater deathTransformer = this.DeathTransformer;
			if (deathTransformer != null)
			{
				EntityGraphics entityComponent = this.owner.GetEntityComponent<EntityGraphics>();
				if (entityComponent != null)
				{
					entityComponent.StopShadow();
				}
				this.state.deathTransformCtx = deathTransformer.MakeUpdater(this.owner.RealTransform);
				return;
			}
		}
		else
		{
			this.state.timer = 0f;
		}
	}

	string attackerName; // Used for knowing who the attacker is outside of HandleHit()
	public override bool HandleHit(ref HitData data, ref HitResult inResult)
	{
		inResult.result = HitResult.ResultMode.Normal;
		HitData.DamageInstance.Data[] damageData = data.GetDamageData();
		float hp = this.state.hp;

		// Added vars
		float dmg = 0; // Damage to be dealt
		bool ittleIsAttacker = data.Attacker != null && data.Attacker.name == "PlayerEnt";
		DebugCommands debugger = Singleton<DebugCommands>.Instance;
		bool isLikeABoss = debugger.likeABoss; // If one hit kill

		// Get damage, if applicable (fixes bug with some projectiles)
		if (damageData.Length > 0) { dmg = damageData[0].damage; }
		// Handles damage overflow for updating stats so overkill damage doesn't get added
		if (data.IsDamageMoreThan(state.hp)) { dmg = state.hp; }
		// Sets attacker name to be known outside of this function
		if (data.Attacker != null) { attackerName = data.Attacker.name; }
		// If Ittle is attacker
		if (ittleIsAttacker && !isLikeABoss)
		{
			ModMaster.UpdateStats("DamageGiven", dmg);

			// Invoke OnDamageDone events
			GameStateNew.OnDamageDealt(dmg, transform.parent.GetComponent<Entity>());
		}
		// Else if Ittle is vicim
		else if (!ittleIsAttacker)
		{
			if (dmg != -1000 && transform.parent.name == "PlayerEnt")   // Is not checkpoint
			{
				// If Heart Rush mode is enabled, take away from player's max HP
				if (mc.isHeartRush)
				{
					mc.heartRushManager.DamageTaken(this, dmg);
					return false;
				}

				// Updates damage taken stat
				ModMaster.UpdateStats("DamageTaken", dmg);

				// Invoke OnDamageDone events
				GameStateNew.OnDamageDealt(dmg, transform.parent.GetComponent<Entity>());
			}
		}
		// Does the damage (this is vanilla except for the likeaboss part)
		for (int i = 0; i < damageData.Length; i++)
		{
			// If attacker is player and likeaboss is enabled, do extra damage
			if (ittleIsAttacker && isLikeABoss)
			{
				state.hp -= debugger.likeABossDmg;
			}
			else
			{
				state.hp -= damageData[i].damage;
			}
		}

		// Vanilla code
		this.state.hp = Mathf.Min(this.MaxHp, this.state.hp);
		if (hp != this.state.hp)
		{
			HitData.DamageInstance.CritData[] critData = data.GetCritData();
			for (int j = 0; j < critData.Length; j++)
			{
				if (this.state.hp < critData[j].maxHp && UnityEngine.Random.value < critData[j].chance)
				{
					this.state.hp -= critData[j].damage;
				}
			}
		}

		// If player died
		if (this.state.hp <= 0f)
		{
			// Update kills stat
			if (ittleIsAttacker)
			{
				ModMaster.UpdateStats("Kills");

				// Invoke OnEnemyKilled events
				GameStateNew.OnEnemyKilled(transform.parent.GetComponent<Entity>());
			}

			// Vanilla code
			inResult.died = true;
			if (this._deathOverride != null)
			{
				this.state.deathData = this._deathOverride.GetData(this, damageData);
			}
			else
			{
				this.state.deathData = null;
			}
			this.SignalDeath();
		}
		return true;
	}

	public void ForceDeath(float dmg, Killable.DeathData deathData, bool absdmg)
	{
		// Kill enemy by proximity
		if (transform.parent != null && transform.parent.name != "PlayerEnt")
		{
			float distance = Vector3.Distance(transform.position, GameObject.Find("PlayerEnt").transform.position);

			if (distance < 1.75f)
			{
				// Invoke OnEnemyKill events
				ModMaster.UpdateStats("Kills", 1);

				// Invoke OnEnemyKilled events
				GameStateNew.OnEnemyKilled(transform.parent.GetComponent<Entity>());
			}
		}

		if (absdmg)
		{
			this.state.hp = dmg;
		}
		else
		{
			// Take damage from enviromental damagers
			if (mc.isHeartRush && transform.parent.name == "PlayerEnt")
			{
				mc.heartRushManager.DamageTaken(this, dmg);
			}

			// Update stats
			float realDmg = dmg;

			if ((state.hp - dmg) <= 0)
			{
				realDmg = Mathf.Abs(state.hp);
			}

			this.state.hp -= dmg;

			// Update stats
			if (transform.parent.name == "PlayerEnt")
			{
				ModMaster.UpdateStats("DamageTaken", realDmg);

				// Invoke OnDamageDone events
				GameStateNew.OnDamageDealt(dmg, transform.parent.GetComponent<Entity>());
			}
		}
		this.state.deathData = deathData;
		this.SignalDeath();
	}

	public void PushDieEvent(Killable.OnDieFunc func)
	{
		this.state.PushDieEvent(func);
	}

	void DoDie()
	{
		if (!this.SilentDeath)
		{
			EffectFactory.Instance.PlayQuickEffect(this.DeathEffect, this.owner.WorldPosition, this.owner.ForwardVector, this.owner.SoundContext);
			IGameActionEvent gameActionEvent = this.state.MakeEventAction();
			GameAction deathAction = this.DeathAction;
			if (deathAction != null)
			{
				GameAction.ActionData actionData = new GameAction.ActionData(this.owner.WorldPosition, 1f, null, this.owner);
				actionData.rotation = this.owner.RealTransform.rotation;
				actionData.AddExtra("onDespawn", gameActionEvent);
				deathAction.Execute(actionData);
				gameActionEvent = actionData.StealExtra<IGameActionEvent>("onDespawn");
			}
			if (gameActionEvent != null)
			{
				gameActionEvent.Fire(this.owner.WorldPosition);
			}
		}

		// Player death events
		if (transform.parent.name == "PlayerEnt" && (CurrentHp <= 0 || MaxHp <= 0))
		{
			ModMaster.UpdateStats("Deaths");
			gameState.hasPlayerSpawned = false;
			gameState.hasPlayerDied = true;
			gameState.OnPlayerDeath();
		}

		// If Boss Rush and boss is defeated, go to next boss room
		if (mc.isBossRush && (transform.parent.GetComponent<PresenceActivator>() != null && transform.parent.GetComponent<PresenceActivator>()._tag._tagId == "pres_Boss" || transform.parent.name == "DreamMoth" || transform.parent.name == "ThatGuy2"))
		{
			mc.bossRushManager.OnBossKilled(transform.parent.name.ToLower());
		}

		// Dungeon Rush
		if (mc.isDungeonRush && transform.parent.name == "ThatGuy2")
		{
			mc.dungeonRushManager.OnDungeonComplete();
		}

		this.owner.Deactivate();
	}

	void ILateUpdatable.LateUpdateObject()
	{
		if (this.state == null || !this.state.dead)
		{
			base.enabled = false;
			return;
		}
		if (this.state.deathTransformCtx != null)
		{
			this.state.timer -= Time.deltaTime;
			if (!this.state.deathTransformCtx.Update())
			{
				return;
			}
			this.state.deathTransformCtx = null;
		}
		if (this.state.timer > 0f)
		{
			this.state.timer -= Time.deltaTime;
			return;
		}
		this.DoDie();
		base.enabled = false;
	}

	public delegate void OnDieFunc(Vector3 pos);

	[Serializable]
	public class Data
	{
		public float hp = 1f;
	}

	public class DeathData
	{
		public BaseQuickEffect deathEffect;

		public BaseQuickEffect startDeathEffect;

		public string deathAnim;

		public float deathTime;

		public GameAction deathAction;

		public GameAction signalAction;

		public TransformUpdater deathTransformer;

		public bool silentDeath;

		public string deathTag;

		public DeathData(KillableOverrideData.DeathData data, Killable killable, bool inherit = false)
		{
			this.deathEffect = (killable.GetOverrideComponent<BaseQuickEffect>(data.deathEffectName) ?? data.deathEffect);
			this.startDeathEffect = (killable.GetOverrideComponent<BaseQuickEffect>(data.startDeathEffectName) ?? data.startDeathEffect);
			this.deathAnim = data.deathAnim;
			this.deathTime = data.deathTime;
			this.deathAction = (killable.GetOverrideComponent<GameAction>(data.deathActionName) ?? data.deathAction);
			this.signalAction = data.signalAction;
			this.deathTransformer = data.deathTransformer;
			this.deathTag = data.deathTag;
			if (inherit)
			{
				if (this.deathEffect == null)
				{
					this.deathEffect = killable._deathEffect;
				}
				if (this.startDeathEffect == null)
				{
					this.startDeathEffect = killable._startDeathEffect;
				}
				if (string.IsNullOrEmpty(this.deathAnim))
				{
					this.deathAnim = killable._deathAnim;
				}
				if (this.deathTime < 0f)
				{
					this.deathTime = killable._deathTime;
				}
				if (this.deathAction == null)
				{
					this.deathAction = killable._deathAction;
				}
				if (this.signalAction == null)
				{
					this.signalAction = killable._signalAction;
				}
			}
		}

		public DeathData(bool silentDeath)
		{
			this.silentDeath = silentDeath;
		}
	}

	public struct DetailedDeathData
	{
		public string deathTag;

		public float hp;

		public DetailedDeathData(string deathTag, float hp)
		{
			this.deathTag = deathTag;
			this.hp = hp;
		}
	}

	class DieEventAction : IGameActionEvent
	{
		List<Killable.OnDieFunc> onDieEvent;

		public DieEventAction(List<Killable.OnDieFunc> onDieEvent)
		{
			this.onDieEvent = onDieEvent;
		}

		void IGameActionEvent.Fire(Vector3 pos)
		{
			for (int i = this.onDieEvent.Count - 1; i >= 0; i--)
			{
				this.onDieEvent[i](pos);
			}
			this.onDieEvent.Clear();
			this.onDieEvent = null;
		}
	}

	public class State
	{
		public float hp;

		public bool dead;

		public float timer;

		public Killable.DeathData deathData;

		public TransformUpdater.Updater deathTransformCtx;

		List<Killable.OnDieFunc> onDieEvent;

		public State(Killable.Data data)
		{
			this.hp = data.hp;
			this.dead = false;
			this.timer = 0f;
			this.deathData = null;
			this.deathTransformCtx = null;
		}

		public void PushDieEvent(Killable.OnDieFunc func)
		{
			if (this.onDieEvent == null)
			{
				this.onDieEvent = new List<Killable.OnDieFunc>();
			}
			this.onDieEvent.Add(func);
		}

		public IGameActionEvent MakeEventAction()
		{
			if (this.onDieEvent != null)
			{
				List<Killable.OnDieFunc> list = this.onDieEvent;
				this.onDieEvent = null;
				return new Killable.DieEventAction(list);
			}
			return null;
		}
	}
}
