using System;
using UnityEngine;
using ModStuff;

[AddComponentMenu("Ittle 2/Level/Door zone")]
public class DoorZone : MonoBehaviour, IBC_TriggerEnterListener, IBC_CollisionEventListener
{
	[SerializeField]
	LevelRoom _room1;

	[SerializeField]
	LevelRoom _room2;

	[SerializeField]
	Vector3 _endPointOffset = Vector3.forward;

	[SerializeField]
	Vector3 _endPointOffset2 = Vector3.forward;

	[SerializeField]
	bool _symmetric = true;

	BC_Collider betterCollider;

	void Awake()
	{
		this._room1 = LevelRoom.GetRoomForPosition(base.transform.TransformPoint(this._endPointOffset), null);
		if (this._symmetric)
		{
			this._room2 = LevelRoom.GetRoomForPosition(base.transform.TransformPoint(-this._endPointOffset), null);
		}
		else
		{
			this._room2 = LevelRoom.GetRoomForPosition(base.transform.TransformPoint(-this._endPointOffset2), null);
		}
		this.betterCollider = PhysicsUtility.RegisterColliderEvents(base.gameObject, this);
	}

	void OnDestroy()
	{
		PhysicsUtility.UnregisterColliderEvents(this.betterCollider, this);
	}

	void IBC_TriggerEnterListener.OnTriggerEnter(BC_TriggerData data)
	{
		RoomSwitchable component = data.collider.GetComponent<RoomSwitchable>();
		if (component != null)
		{
			Vector3 vector = component.transform.position - base.transform.position;
			float num = Vector3.Dot(vector, base.transform.TransformDirection(this._endPointOffset));
			LevelRoom fromRoom;
			LevelRoom toRoom;
			Vector3 vector2;
			if (num < 0f)
			{
				fromRoom = this._room2;
				toRoom = this._room1;
				vector2 = base.transform.TransformDirection(this._endPointOffset);
			}
			else
			{
				fromRoom = this._room1;
				toRoom = this._room2;
				if (this._symmetric)
				{
					vector2 = base.transform.TransformDirection(-this._endPointOffset);
				}
				else
				{
					vector2 = base.transform.TransformDirection(-this._endPointOffset2);
				}
			}
			Vector3 position = component.transform.position;
			Vector3 vector3 = base.transform.position + vector2;
			Vector3 normalized = vector2.normalized;
			vector3 = position + Vector3.Project(vector3 - position, normalized);
			component.StartTransition(position, vector3, fromRoom, toRoom, normalized, normalized, true);

			// Trigger map-specific events
			Singleton<GameState>.Instance.OnMapChange(fromRoom.RoomName, toRoom.RoomName);
		}
	}
}
