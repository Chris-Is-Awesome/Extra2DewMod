using System;
using UnityEngine;
using ModStuff;

[AddComponentMenu("Ittle 2/Level/Room door")]
[ExecuteInEditMode]
public class RoomDoor : MonoBehaviour, IBC_TriggerEnterListener, IBC_CollisionEventListener
{
	[SerializeField]
	RoomDoor _target;

	[SerializeField]
	Vector3 _endPointOffset = Vector3.zero;

	[SerializeField]
	bool _relativeOffset;

	[SerializeField]
	bool _twoWay = true;

	[SerializeField]
	LevelRoom _ownerRoom;

	[SerializeField]
	bool _doConnect;

	[SerializeField]
	string _searchTag;

	[SerializeField]
	FadeEffectData _warpFadeEffect;

	[SerializeField]
	string _warpAnim;

	[SerializeField]
	BaseQuickEffect _warpEffect;

	[SerializeField]
	bool _markerOnly;

	BC_Collider betterCollider;

	void Awake()
	{
		if (this._ownerRoom == null)
		{
			this._ownerRoom = TransformUtility.FindInParents<LevelRoom>(base.transform);
		}
		if (Application.isPlaying && !this._markerOnly)
		{
			this.betterCollider = PhysicsUtility.RegisterColliderEvents(base.gameObject, this);
		}
	}

	void OnDestroy()
	{
		PhysicsUtility.UnregisterColliderEvents(this.betterCollider, this);
	}

	void IBC_TriggerEnterListener.OnTriggerEnter(BC_TriggerData other)
	{
		if (this._target != null)
		{
			RoomSwitchable component = other.collider.GetComponent<RoomSwitchable>();
			if (component != null)
			{
				Vector3 vector = this._target.transform.rotation * this._target._endPointOffset;
				Vector3 to = this._target.transform.position + vector;
				if (this._warpFadeEffect != null)
				{
					EffectFactory.Instance.PlayQuickEffect(this._warpEffect, component.transform.position, component.transform.forward, null);
					component.StartWarpTransition(base.transform.position, to, this._ownerRoom, this._target._ownerRoom, -base.transform.forward, this._target.transform.forward, this._warpAnim, this._warpFadeEffect);
					// Trigger room-specific events
					Singleton<GameState>.Instance.OnRoomChange(_ownerRoom.RoomName, _target._ownerRoom.RoomName);
				}
				else
				{
					component.StartTransition(base.transform.position, to, this._ownerRoom, this._target._ownerRoom, -base.transform.forward, this._target.transform.forward, this._relativeOffset);
					// Trigger room-specific events
					Singleton<GameState>.Instance.OnRoomChange(_target._ownerRoom.RoomName, _ownerRoom.RoomName);
				}
			}
		}
	}

	void Unused()
	{
		bool twoWay = this._twoWay;
		this._twoWay = this._doConnect;
		this._doConnect = twoWay;
		this._searchTag = this._searchTag.Substring(1);
	}
}
