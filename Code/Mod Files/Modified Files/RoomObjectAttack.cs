using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Ittle 2/Entity/Attacks/Spawn room object")]
public class RoomObjectAttack : Attack
{
	[SerializeField]
	private RoomObject _objPrefab;

	[SerializeField]
	private string _multiplierName;

	[SerializeField]
	private float _baseMultiplier = 1f;

	[SerializeField]
	private BaseQuickEffect _spawnEffect;

	[SerializeField]
	private Vector3 _placeOffset = Vector3.zero;

	[SerializeField]
	private bool _placeOnGrid = true;

	[SerializeField]
	private float _gridSize = 1f;

	[SerializeField]
	private bool _placeTrace = true;

	[SerializeField]
	private Vector3 _fallbackPlaceOffset = Vector3.zero;

	[SerializeField]
	private Vector3 _traceBox = Vector3.one;

	[SerializeField]
	private LayerMask _traceLayer = default(LayerMask);

	[SerializeField]
	private bool _singleObject = true;

	[SerializeField]
	private bool _singleOverride;

	[SerializeField]
	private BaseQuickEffect _overrideEffect;

	[SerializeField]
	private bool _useFallback;

	[SerializeField]
	private string _attackTypeName = "object";

	private RoomObject.EventFunc callback;

	private RoomObject.SwitchFunc replaceCallback;

	private RoomObject tracker;

	public RoomObjectAttack ()
	{
	}

	public override string GetAttackTypeString ()
	{
		return this._attackTypeName;
	}

	protected override void DoEnable (Entity owner)
	{
		base.DoEnable(owner);
		this.callback = new RoomObject.EventFunc(this.ObjectDeactivated);
		this.replaceCallback = new RoomObject.SwitchFunc(this.ObjectReplaced);
	}

	private Vector3 GridSnap (Vector3 P)
	{
		P.x = Mathf.Round(P.x / this._gridSize) * this._gridSize;
		P.z = Mathf.Round(P.z / this._gridSize) * this._gridSize;
		return P;
	}

	private bool IsPositionFree (Vector3 P)
	{
		if (!this._singleOverride || this.tracker == null)
		{
			return !PhysicsUtility.CheckBox(P, this._traceBox, this._traceLayer, true);
		}
		List<BC_Collider> list = PhysicsUtility.OverlapBox(P, this._traceBox, this._traceLayer, true);
		for (int i = 0; i < list.Count; i++)
		{
			RoomObject component = list[i].GetComponent<RoomObject>();
			if (component != this.tracker)
			{
				return false;
			}
		}
		return true;
	}

	private void ObjectDeactivated (RoomObject obj)
	{
		obj.OnDisabled -= this.callback;
		if (obj == this.tracker)
		{
			this.tracker = null;
		}
	}

	private void ObjectReplaced (RoomObject obj, RoomObject replace)
	{
		obj.OnReplaced -= this.replaceCallback;
		obj.OnDisabled -= this.callback;
		if (obj == this.tracker)
		{
			this.tracker = replace;
			replace.OnReplaced += this.replaceCallback;
			replace.OnDisabled += this.callback;
		}
	}

	protected override void DoAttack (Vector3 dir, EntityAction.ActionData inData)
	{
		bool flag = this._singleObject && this.tracker != null && this.tracker.gameObject.activeInHierarchy;
		if (!this._singleOverride && flag)
		{
			return;
		}
		Vector3 vector = this.owner.WorldPosition + this.owner.RealTransform.rotation * this._placeOffset;
		if (this._placeOnGrid)
		{
			vector = this.GridSnap(vector);
		}
		if (this._placeTrace && !this.IsPositionFree(vector))
		{
			if (!this._useFallback)
			{
				return;
			}
			vector = this.owner.WorldPosition + this.owner.RealTransform.rotation * this._fallbackPlaceOffset;
			if (this._placeOnGrid)
			{
				vector = this.GridSnap(vector);
			}
			if (!this.IsPositionFree(vector))
			{
				return;
			}
		}
		if (flag && this._singleOverride)
		{
			EffectFactory.Instance.PlayQuickEffect(this._overrideEffect, this.tracker.transform.position, null);
			RoomObject roomObject = this.tracker;
			this.tracker = null;
			roomObject.DeactivateObject();
			this.ObjectReplaced(roomObject, null);
		}
		this.tracker = RoomObjectFactory.Instance.GetObject(this._objPrefab, vector);
		this.tracker.SetSpawner(this.owner);
		//If its timer was changed, use custom timer/explosion
		if (use_custom_timer) { tracker.transform.Find("Timer").GetComponent<TimerTrigger>().SetTime(custom_time); }
		if (use_custom_radius) { tracker.transform.Find("Timer").GetComponent<GA_Explode>()._explosionData._radius = custom_radius; }
		if (!string.IsNullOrEmpty(this._multiplierName))
		{
			this.tracker.SetStateData(this._multiplierName, this._baseMultiplier);
		}
		if (this._singleObject)
		{
			this.tracker.OnDisabled += this.callback;
			this.tracker.OnReplaced += this.replaceCallback;
		}
		EffectFactory.Instance.PlayQuickEffect(this._spawnEffect, vector, null);
	}

	//Mod code
	private bool original_singleObject;
	private bool original_singleOverride;
	private bool original_placeOnGrid;
	private bool use_custom_timer;
	private bool use_custom_radius;
	private float custom_time;
	private float custom_radius;

	//Toggle between not unique and default value
	public void SetInfinite (bool mode)
	{
		if (mode)
		{
			_singleObject = false;
			_singleOverride = false;
		}
		else
		{
			_singleObject = original_singleObject;
			_singleOverride = original_singleOverride;
		}
		return;
	}

	//Toggle between not gridlocked and default value
	public void UnlockFromGrid (bool mode)
	{
		if (mode)
		{
			_placeOnGrid = false;
		}
		else
		{
			_placeOnGrid = original_placeOnGrid;
		}

		return;
	}

	//Change fuse timer. If time is less than 0, disable custom timer
	public void SetTimer (float time)
	{
		if (time < 0)
		{
			use_custom_timer = false;
			return;
		}
		use_custom_timer = true;
		custom_time = time;
	}

	public void SetExplosion (float radius)
	{
		if (radius < 0)
		{
			use_custom_radius = false;
			return;
		}
		use_custom_radius = true;
		custom_radius = radius;
	}

	void Awake ()
	{
		original_singleObject = _singleObject;
		original_singleOverride = _singleOverride;
		original_placeOnGrid = _placeOnGrid;
	}
}
