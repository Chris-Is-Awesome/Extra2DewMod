using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Ittle 2/Entity entity")]
public class Entity : UpdateBehaviour, IExprContext, IUpdatable, ILateUpdatable, IBaseUpdateable
{
	[SerializeField]
	string _defaultAnim = "idle";

	[SerializeField]
	bool _keepOnFloor = true;

	[SerializeField]
	float _floorTraceOffset = 0.5f;

	[SerializeField]
	LayerMask _floorLayer = default(LayerMask);

	[SerializeField]
	List<string> _levelLocalVars;

	[SerializeField]
	List<string> _tempVars;

	[SerializeField]
	DataSaveLink _saveLink;

	[SerializeField]
	Entity.StartVar[] _startVars;

	[SerializeField]
	Transform _effectTransform;

	Moveable moveable;

	RigidBodyController realBody;

	Transform realTrans;

	EntityAnimator realAnim;

	EntityGraphics realGraphics;

	ObjectComponentMap componentMap;

	Quaternion startRot;

	bool setupFinished;

	List<EntityComponent> allComs = new List<EntityComponent>();

	Dictionary<Type, EntityComponent> compTypeLookup;

	Dictionary<string, EntityAction> actions = new Dictionary<string, EntityAction>();

	Dictionary<string, AttackAction> attacks = new Dictionary<string, AttackAction>();

	Dictionary<string, Moveable> allMoveables = new Dictionary<string, Moveable>();

	EntityStateData state;

	void Awake()
	{
		this.realTrans = base.transform;
		this.realBody = base.GetComponentInChildren<RigidBodyController>();
		this.realAnim = base.GetComponentInChildren<EntityAnimator>();
		if (this.realAnim != null)
		{
			this.realAnim.RealEntity = this;
		}
		this.moveable = base.GetComponentInChildren<Moveable>();
		Moveable[] componentsInChildren = base.GetComponentsInChildren<Moveable>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].IsDefault)
			{
				this.moveable = componentsInChildren[i];
			}
			if (!string.IsNullOrEmpty(componentsInChildren[i].MoveableName))
			{
				this.allMoveables.Add(componentsInChildren[i].MoveableName, componentsInChildren[i]);
			}
		}
		this.startRot = Quaternion.identity;
		if (this._saveLink != null)
		{
			this._saveLink.SetGatherDataListener(new SaverOwner.OnRequestFunc(this.SaveState));
		}
	}

	void Start()
	{
		if (!this.state.activated)
		{
			this.Activate();
		}
	}

	public void Init()
	{
		if (this.setupFinished)
		{
			return;
		}
		this.allComs = new List<EntityComponent>(base.GetComponentsInChildren<EntityComponent>());
		for (int i = 0; i < this.allComs.Count; i++)
		{
			EntityComponent entityComponent = this.allComs[i];
			try
			{
				this.EnableComponent(entityComponent);
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Concat(new string[]
				{
					base.name,
					": Error enabling component ",
					entityComponent.name,
					": ",
					ex.Message,
					"\n",
					ex.StackTrace
				}), this);
			}
		}
		this.allComs.Sort((EntityComponent a, EntityComponent b) => a.ActivationPrio - b.ActivationPrio);
		this.setupFinished = true;
	}

	void EnableComponent(EntityComponent com)
	{
		com.Enable(this);
		AttackAction attackAction = com as AttackAction;
		if (attackAction != null)
		{
			if (this.attacks.ContainsKey(attackAction.ActionName))
			{
				Debug.LogError("An attack " + attackAction.ActionName + " is already registered");
			}
			else
			{
				this.attacks.Add(attackAction.ActionName, attackAction);
			}
		}
		else
		{
			EntityAction entityAction = com as EntityAction;
			if (entityAction != null)
			{
				if (this.actions.ContainsKey(entityAction.ActionName))
				{
					Debug.LogError("An action " + entityAction.ActionName + " is already registered.");
				}
				else
				{
					this.actions.Add(entityAction.ActionName, entityAction);
				}
			}
		}
	}

	public T GetEntityComponent<T>() where T : EntityComponent
	{
		if (this.compTypeLookup == null)
		{
			this.compTypeLookup = new Dictionary<Type, EntityComponent>();
		}
		EntityComponent entityComponent;
		if (this.compTypeLookup.TryGetValue(typeof(T), out entityComponent))
		{
			return (T)((object)entityComponent);
		}
		for (int i = this.allComs.Count - 1; i >= 0; i--)
		{
			T t = this.allComs[i] as T;
			if (t != null)
			{
				this.compTypeLookup[typeof(T)] = t;
				return t;
			}
		}
		return (T)((object)null);
	}

	public T AddEntityComponent<T>() where T : EntityComponent
	{
		T t = base.gameObject.AddComponent<T>();
		if (this.setupFinished)
		{
			bool flag = false;
			for (int i = 0; i < this.allComs.Count; i++)
			{
				if (this.allComs[i].ActivationPrio < t.ActivationPrio)
				{
					this.allComs.Insert(i, t);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.allComs.Add(t);
			}
			this.EnableComponent(t);
		}
		else
		{
			this.allComs.Add(t);
		}
		return t;
	}

	public T GetOrAddEntityComponent<T>() where T : EntityComponent
	{
		T t = this.GetEntityComponent<T>();
		if (t == null)
		{
			t = this.AddEntityComponent<T>();
		}
		return t;
	}

	public Transform RealTransform
	{
		get
		{
			return this.realTrans;
		}
	}

	public Transform EffectTransform
	{
		get
		{
			return (!(this._effectTransform != null)) ? this.realTrans : this._effectTransform;
		}
	}

	public string DefaultAnimName
	{
		get
		{
			return this._defaultAnim;
		}
	}

	public int CurrentPrio
	{
		get
		{
			return (!(this.state.currentAction == null)) ? this.state.currentAction.CurrentPriority : this.state.currentPriority;
		}
	}

	public EntityGraphics RealGraphics
	{
		get
		{
			if (this.realGraphics == null)
			{
				this.realGraphics = this.GetEntityComponent<EntityGraphics>();
			}
			return this.realGraphics;
		}
	}

	public Vector3 WorldPosition
	{
		get
		{
			return this.realTrans.position;
		}
		set
		{
			this.realTrans.position = value;
		}
	}

	public Vector3 WorldTracePosition
	{
		get
		{
			return this.realTrans.position + new Vector3(0f, 0.5f, 0f);
		}
	}

	public Vector3 ForwardVector
	{
		get
		{
			return this.realTrans.forward;
		}
	}

	public void Activate()
	{
		if (!this.setupFinished)
		{
			this.Init();
		}
		if (this.state.activated)
		{
			return;
		}
		base.gameObject.SetActive(true);
		this.state = default(EntityStateData);
		this.state.activated = true;
		this.state.targetRotation = (base.transform.localRotation * Quaternion.Inverse(this.startRot)).eulerAngles.y;
		this.PlayAnimation(this.DefaultAnimName, 0);
		if (this._startVars != null && this._startVars.Length > 0)
		{
			for (int i = 0; i < this._startVars.Length; i++)
			{
				this.SetStateVariable(this._startVars[i].name, this._startVars[i].value);
			}
		}
		for (int j = this.allComs.Count - 1; j >= 0; j--)
		{
			this.allComs[j].Activate();
		}
		this.LoadState();
	}

	public void Deactivate()
	{
		this.CancelAction(null);
		base.gameObject.SetActive(false);
		bool activated = this.state.activated;
		this.state.activated = false;
		if (activated)
		{
			for (int i = this.allComs.Count - 1; i >= 0; i--)
			{
				this.allComs[i].Deactivate();
			}
			this.state.LocalEvents.SendDeactivate(this);
		}
		SoundPlayer weakInstance = SoundPlayer.WeakInstance;
		if (weakInstance != null)
		{
			weakInstance.StopContext(this.SoundContext);
		}
	}

	public void RegisterBaseRotation(Quaternion rot)
	{
		this.startRot = rot;
	}

	public bool RequestPriority(int level)
	{
		int currentPrio = this.CurrentPrio;
		if (currentPrio <= level)
		{
			this.state.currentPriority = level;
			if (this.state.currentAction != null)
			{
				this.SwitchToAction(null, null);
			}
			return true;
		}
		return false;
	}

	public void FreePriority(int level)
	{
		this.state.currentPriority = 0;
	}

	void SwitchToAction(EntityAction action, EntityAction.ActionData data)
	{
		EntityAction currentAction = this.state.currentAction;
		this.state.currentAction = null;
		if (currentAction != null)
		{
			currentAction.Stop(true);
		}
		else if (action != null)
		{
			this.moveable.Stop();
		}
		this.state.currentAction = action;
		if (this.state.currentAction != null)
		{
			this.state.currentAction.Fire(data);
		}
	}

	bool RequestAction(EntityAction action, EntityAction.ActionData data)
	{
		if (!action.CanDoAction(data))
		{
			return false;
		}
		if (this.state.currentAction != null)
		{
			if (action.CanOverride(this.state.currentAction))
			{
				this.SwitchToAction(action, data);
				return true;
			}
			return false;
		}
		else
		{
			int currentPrio = this.CurrentPrio;
			if (action.CurrentPriority >= currentPrio)
			{
				this.SwitchToAction(action, data);
				return true;
			}
			return false;
		}
	}

	public void CancelAction(EntityAction action)
	{
		if (action == null || this.state.currentAction == action)
		{
			this.SwitchToAction(null, null);
		}
	}

	public void ClearAction(EntityAction action)
	{
		if (this.state.currentAction == action)
		{
			this.state.currentAction = null;
		}
	}

	public void ReleaseAction(EntityAction action)
	{
		if (action != null && action == this.state.currentAction)
		{
			action.ReleaseAction();
		}
	}

	public EntityAnimator.AnimatorState PlayAnimation(string anim, int variation = 0)
	{
		if (this.realAnim != null)
		{
			return this.realAnim.PlayAnim(anim, variation);
		}
		return null;
	}

	public EntityAnimator.AnimatorState PushAnimation(string anim, int variation = 0)
	{
		if (this.realAnim != null)
		{
			return this.realAnim.PushAnim(anim, variation);
		}
		return null;
	}

	public EntityAnimator.AnimatorState PopAnimation(EntityAnimator.AnimatorState anim)
	{
		if (this.realAnim != null)
		{
			return this.realAnim.PopAnim(anim);
		}
		return null;
	}

	Moveable CurrentMover
	{
		get
		{
			if (this.state.moverStack != null)
			{
				int count = this.state.moverStack.Count;
				if (count > 0)
				{
					return this.state.moverStack[count - 1];
				}
			}
			return this.moveable;
		}
	}

	public bool SetMoveDirection(Vector3 dir, bool affectSpeed = false)
	{
		return this.CurrentMover.SetMoveDirection(dir, affectSpeed);
	}

	public void StopMoving()
	{
		this.CurrentMover.Stop();
	}

	public bool GetMoveDirection(out Vector3 dir)
	{
		return this.moveable.GetMoveDirection(out dir);
	}

	public Moveable PushMover(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		Moveable moveable;
		this.allMoveables.TryGetValue(name, out moveable);
		if (moveable == null)
		{
			return null;
		}
		if (this.state.moverStack == null)
		{
			this.state.moverStack = new List<Moveable>(1);
		}
		this.state.moverStack.Add(moveable);
		return moveable;
	}

	public void PopMover(Moveable mover)
	{
		if (mover == null || this.state.moverStack == null)
		{
			return;
		}
		for (int i = this.state.moverStack.Count - 1; i >= 0; i--)
		{
			if (this.state.moverStack[i] == mover)
			{
				this.state.moverStack.RemoveAt(i);
				break;
			}
		}
	}

	public bool DeathSignal
	{
		get
		{
			return this.state.deathSignal;
		}
		set
		{
			bool deathSignal = this.state.deathSignal;
			this.state.deathSignal = value;
			if (!deathSignal && value)
			{
				this.CancelAction(null);
				this.StopMoving();
				this.state.LocalEvents.SendDied(this);
			}
		}
	}

	public bool IsActive
	{
		get
		{
			return base.gameObject.activeInHierarchy;
		}
	}

	public bool InactiveOrDead
	{
		get
		{
			return !this.state.activated || this.state.deathSignal;
		}
	}

	public void Move(Vector3 V)
	{
		if (this.realBody)
		{
			this.realBody.SetVelocity(V);
		}
		else
		{
			this.realTrans.position += V * Time.deltaTime;
		}
	}

	public void MovePosition(Vector3 P)
	{
		if (this.realBody)
		{
			this.realBody.MovePosition(P);
		}
		else
		{
			this.realTrans.position = P;
		}
	}

	public void TurnTo(float angle, float speed)
	{
		if (speed <= 0f)
		{
			this.state.targetRotation = angle;
		}
		else
		{
			this.state.targetRotation = Mathf.MoveTowardsAngle(this.state.targetRotation, angle, speed * Time.deltaTime);
		}
		Quaternion quaternion = Quaternion.AngleAxis(this.state.targetRotation, Vector3.up);
		this.realTrans.localRotation = this.startRot * quaternion;
	}

	public void TurnTo(Vector3 dir, float speed)
	{
		float angle = Mathf.Atan2(dir.z, -dir.x) * 57.29578f - 90f;
		this.TurnTo(angle, speed);
	}

	public AttackAction GetAttack(string type)
	{
		AttackAction result = null;
		this.attacks.TryGetValue(type, out result);
		return result;
	}

	public EntityAction GetAction(string type)
	{
		EntityAction result = null;
		this.actions.TryGetValue(type, out result);
		return result;
	}

	public bool CanAttack(string type, EntityAction.ActionData data)
	{
		AttackAction attack = this.GetAttack(type);
		return attack != null && attack.CanDoAction(data);
	}

	public bool CanDoAction(string type, EntityAction.ActionData data)
	{
		EntityAction action = this.GetAction(type);
		return action != null && action.CanDoAction(data);
	}

	public bool Attack(string type, EntityAction.ActionData data)
	{
		AttackAction attack = this.GetAttack(type);
		return attack != null && this.RequestAction(attack, data);
	}

	public bool DoAction(string type, EntityAction.ActionData data)
	{
		EntityAction action = this.GetAction(type);
		return action != null && this.RequestAction(action, data);
	}

	public bool IsInAction
	{
		get
		{
			return this.state.currentAction != null;
		}
	}

	public EntityAction CurrentAction
	{
		get
		{
			return this.state.currentAction;
		}
	}

	public int GetStateVariable(string name)
	{
		int result = 0;
		if (this.state.stateVars != null)
		{
			this.state.stateVars.TryGetValue(name, out result);
		}
		return result;
	}

	public void SetStateVariable(string name, int value)
	{
		if (this.state.stateVars == null)
		{
			this.state.stateVars = new Dictionary<string, int>();
		}
		this.state.stateVars[name] = value;
		this.state.LocalEvents.SendVarSet(this, name, value);
	}

	public bool HasStateVariable(string name)
	{
		return this.state.stateVars != null && this.state.stateVars.ContainsKey(name);
	}

	int IExprContext.GetValue(string key)
	{
		return this.GetStateVariable(key);
	}

	void IExprContext.SetValue(string key, int value)
	{
		this.SetStateVariable(key, value);
	}

    Dictionary<string, ChangedVars> changedVars;
    public void AddLocalTempVar(string varName)
	{
		if (this._tempVars == null)
		{
			this._tempVars = new List<string>();
        }
        if(changedVars == null)
        {
            changedVars = new Dictionary<string, ChangedVars>();
        }
        this._tempVars.Add(varName);
        if(!changedVars.ContainsKey(varName)) changedVars.Add(varName, new ChangedVars(GetStateVariable(varName))); //Added for mod
        //ModStuff.ModText.QuickText("Local var for: " + varName + " with the original value of: " + GetStateVariable(varName).ToString());
    }

    #region Added for mod
    class ChangedVars
    {
        public int originalValue;
        public int currentValue;

        public ChangedVars(int original)
        {
            originalValue = original;
            currentValue = originalValue;
        }
    }

    public bool TryGetModifiedVar(string varName, out int regValue)
    {
        if (changedVars == null)
        {
            changedVars = new Dictionary<string, ChangedVars>();
        }

        if (changedVars.TryGetValue(varName, out ChangedVars vars))
        {
            regValue = vars.currentValue;
            return true;
        }
        
        regValue = 0;
        return false;
    }

    public void UpdateModifiedVar(string varName, int newValue)
    {
        //ModStuff.ModText.QuickText("Updating " + varName + " from " + changedVars[varName].currentValue + " to " + newValue);
        changedVars[varName].currentValue = newValue;
    }
    #endregion

    public void LoadState()
	{
		if (this._saveLink != null)
		{
			IDataSaver linkLocalStorage = this._saveLink.LinkLocalStorage;
			IDataSaver linkLocalLevelStorage = this._saveLink.LinkLocalLevelStorage;
			if (this._levelLocalVars == null)
			{
				this._levelLocalVars = new List<string>();
			}
			this.state.Load(linkLocalStorage, linkLocalLevelStorage, this._levelLocalVars);
			for (int i = this.allComs.Count - 1; i >= 0; i--)
			{
				this.allComs[i].ReadSaveData(linkLocalStorage, linkLocalLevelStorage);
			}
		}
	}

    //Modified for item randomization
	public void SaveState()
	{
		if (this._saveLink != null && this.state.activated)
		{
			IDataSaver linkLocalStorage = this._saveLink.LinkLocalStorage;
			IDataSaver linkLocalLevelStorage = this._saveLink.LinkLocalLevelStorage;
			if (this._levelLocalVars == null)
			{
				this._levelLocalVars = new List<string>();
			}
			if (this._tempVars == null)
			{
				this._tempVars = new List<string>();
            }

            //Clearing the tempVars. The new method will hopefully rewrite them properly
            //_tempVars.Clear();

            //Apply all the saved changes to the player again
            if (this.changedVars != null)
            {
                foreach(KeyValuePair<string, ChangedVars> keyValuePair in changedVars)
                {
                    if(keyValuePair.Value.currentValue != keyValuePair.Value.originalValue)
                    {
                        SetStateVariable(keyValuePair.Key, keyValuePair.Value.currentValue);
                        if (_tempVars.Contains(keyValuePair.Key)) _tempVars.RemoveAll(item => item == keyValuePair.Key);
                    }
                }
            }
            //ModStuff.ModText.QuickText("Tempvars: " + string.Join(" ", _tempVars.ToArray()));
            this.state.Save(linkLocalStorage, linkLocalLevelStorage, this._levelLocalVars, this._tempVars);
			for (int i = this.allComs.Count - 1; i >= 0; i--)
			{
				this.allComs[i].WriteSaveData(linkLocalStorage, linkLocalLevelStorage);
			}
		}
	}

	public ObjectComponentMap ComponentMap
	{
		get
		{
			if (this.componentMap == null)
			{
				this.componentMap = base.GetComponentInChildren<ObjectComponentMap>();
			}
			return this.componentMap;
		}
	}

	public EntityEventsOwner LocalEvents
	{
		get
		{
			return this.state.LocalEvents;
		}
	}

	public TimerOwner LocalTimers
	{
		get
		{
			if (this.state.localTimers == null)
			{
				this.state.localTimers = new TimerOwner();
			}
			return this.state.localTimers;
		}
	}

	public EntityDataModifier LocalMods
	{
		get
		{
			return this.state.LocalMods;
		}
	}

	public DropTableContext DropContext
	{
		get
		{
			return this.state.DropContext;
		}
	}

	public object SoundContext
	{
		get
		{
			return this;
		}
	}

	public BaseEffect.EffectData MakeEffectData()
	{
		return new BaseEffect.EffectData
		{
			target = this.EffectTransform,
			graphics = this.RealGraphics,
			soundContext = this.SoundContext
		};
	}

	public Entity.FloorAttacher ReleaseFromFloor(object obj)
	{
		if (obj != null && this._keepOnFloor)
		{
			if (this.state.floorReleasers == null)
			{
				this.state.floorReleasers = new List<object>();
			}
			this.state.floorReleasers.Add(obj);
			List<object> releasers = this.state.floorReleasers;
			return delegate()
			{
				releasers.Remove(obj);
			};
		}
		return null;
	}

	public bool GetFloorPointForPos(Vector3 P, out Vector3 point)
	{
		if (this.keepOnFloor)
		{
			return PhysicsUtility.GetFloorPoint(P, this._floorTraceOffset, 50f, this._floorLayer.value, out point);
		}
		point = P;
		return false;
	}

	bool keepOnFloor
	{
		get
		{
			return this._keepOnFloor && (this.state.floorReleasers == null || this.state.floorReleasers.Count == 0);
		}
	}

	void KeepOnFloor()
	{
		Vector3 position = this.realTrans.position;
		Vector3 vector;
		if (PhysicsUtility.GetFloorPoint(position, this._floorTraceOffset, 10f, this._floorLayer.value, out vector))
		{
			position.y = vector.y;
			this.realTrans.position = position;
		}
	}

	void IUpdatable.UpdateObject()
	{
		if (this.state.localTimers != null)
		{
			this.state.localTimers.Update(Time.deltaTime);
		}
		if (this.state.currentAction != null && !this.state.currentAction.UpdateAction())
		{
			EntityAction currentAction = this.state.currentAction;
			this.state.currentAction = null;
			currentAction.Stop(false);
		}
	}

	void ILateUpdatable.LateUpdateObject()
	{
		if (this.keepOnFloor)
		{
			this.KeepOnFloor();
		}
	}

	[Serializable]
	public class StartVar
	{
		public string name;

		public int value;
	}

	public delegate void FloorAttacher();
}
