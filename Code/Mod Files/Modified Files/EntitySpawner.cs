using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Ittle 2/Room/Entity spawner")]
public class EntitySpawner : RoomObjectComponent, IUpdatable, IBaseUpdateable
{
	[SerializeField]
	Bounds _spawnBounds;

	[SerializeField]
	AIController _controllerPrefab;

	[SerializeField]
	Entity _entityPrefab;

	[SerializeField]
	LayerMask _floorLayer;

	[SerializeField]
	LayerMask _collisionLayer;

	[SerializeField]
	float _collisionRadius;

	[SerializeField]
	bool _spawnOnFloor = true;

	[SerializeField]
	bool _preload = true;

	[SerializeField]
	float _delay = 2f;

	[SerializeField]
	int _maxCount = 1;

	[SerializeField]
	bool _respawn;

	[SerializeField]
	bool _startEnabled = true;

	[SerializeField]
	EntitySpawner.PositionMode _positionMode;

	[SerializeField]
	float _positionDist = 3.5f;

	[SerializeField]
	EntityObjectAttacher _attacher;

	AIController controller;

	public List<Entity> trackEnts = new List<Entity>();

	int totalSpawned = -1;

	float timer;

	EntityEventsOwner.OnEventFunc onDeactivate;

	public bool IsInfinite
	{
		get
		{
			return false;
		}
	}

	public int TargetSpawnCount
	{
		get
		{
			return Mathf.Max(1, this._maxCount);
		}
	}

	public event EntitySpawner.EntityEventFunc OnSpawnedEntity;

	public bool ValidSpawner
	{
		get
		{
			return this._entityPrefab != null;
		}
	}

	static bool IsValid(Entity ent)
	{
		return ent != null && ent.IsActive;
	}

	static void AddToList(List<Entity> ents, Entity ent)
	{
		for (int i = ents.Count - 1; i >= 0; i--)
		{
			if (!EntitySpawner.IsValid(ents[i]))
			{
				ents[i] = ent;
				return;
			}
		}
		ents.Add(ent);
	}

	int CurrentSpawnCount
	{
		get
		{
			if (this._respawn)
			{
				int num = 0;
				for (int i = this.trackEnts.Count - 1; i >= 0; i--)
				{
					if (EntitySpawner.IsValid(this.trackEnts[i]))
					{
						num++;
					}
				}
				return num;
			}
			return this.totalSpawned;
		}
	}

	void EntityGone(Entity ent)
	{
		ent.LocalEvents.DeactivateListener -= this.onDeactivate;
		for (int i = this.trackEnts.Count - 1; i >= 0; i--)
		{
			if (this.trackEnts[i] == ent)
			{
				this.trackEnts[i] = null;
			}
		}
	}

	protected override void DoEnable(RoomObject room)
	{
		this.onDeactivate = new EntityEventsOwner.OnEventFunc(this.EntityGone);
		this.controller = ControllerFactory.Instance.GetController<AIController>(this._controllerPrefab);
		if (this._preload)
		{
			EntityFactory.Instance.Preload(this._entityPrefab);
		}
		this.timer = this._delay;
		if (this.totalSpawned < 0 && base.Room == null)
		{
			this.totalSpawned = 0;
		}
	}

	protected override void DoActivate(LevelRoom room)
	{
		int num = 0;
		for (int i = this.trackEnts.Count - 1; i >= 0; i--)
		{
			Entity entity = this.trackEnts[i];
			this.trackEnts[i] = null;
			if (EntitySpawner.IsValid(entity))
			{
				entity.Deactivate();
				num++;
			}
		}
		this.totalSpawned = 0;
		this.timer = this._delay;
		base.enabled = this._startEnabled;
		if (num > 0)
		{
			Debug.LogWarning(string.Concat(new object[]
			{
				base.name,
				" had ",
				num,
				" active ents"
			}), this);
		}
	}

	protected override void DoDeactivate(LevelRoom room)
	{
		for (int i = this.trackEnts.Count - 1; i >= 0; i--)
		{
			Entity entity = this.trackEnts[i];
			if (EntitySpawner.IsValid(entity))
			{
				this.trackEnts[i] = null;
				entity.Deactivate();
			}
		}
	}

	bool IsPositionFree(Vector3 P)
	{
		return this._collisionLayer.value == 0 || this._collisionRadius <= 0f || !PhysicsUtility.CheckSphere(P, this._collisionRadius, this._collisionLayer, false);
	}

	Vector3 LinearRand()
	{
		Vector3 vector = this._spawnBounds.min;
		Vector3 vector2 = this._spawnBounds.max - vector;
		vector.x += vector2.x * UnityEngine.Random.value;
		vector.y += vector2.y * UnityEngine.Random.value;
		vector.z += vector2.z * UnityEngine.Random.value;
		vector = base.transform.TransformPoint(vector);
		return vector;
	}

	Vector3 NormalSpawnPos()
	{
		Vector3 vector = this.LinearRand();
		if (this._spawnOnFloor)
		{
			PhysicsUtility.GetFloorPoint(vector, 10f, 20f, this._floorLayer.value, out vector);
		}
		return vector;
	}

	static float Dist(Vector3 A, Vector3 B)
	{
		return Mathf.Abs(A.x - B.x) + Mathf.Abs(A.z - B.z);
	}

	Vector3 OppositeRand(Vector3 P)
	{
		Vector3 vector = this.LinearRand();
		int num = 10;
		float num2 = (this._positionDist > 0f) ? this._positionDist : (UnityEngine.Random.Range(0.75f, 1.25f) * this._spawnBounds.extents.magnitude);
		while (EntitySpawner.Dist(P, vector) < num2 && --num > 0)
		{
			vector = this.LinearRand();
		}
		return vector;
	}

	Vector3 OppositeSpawnPos()
	{
		if (this.owner == null || this.owner.Room == null)
		{
			return this.NormalSpawnPos();
		}
		Vector3 importantPoint = this.owner.Room.ImportantPoint;
		Vector3 vector = this.OppositeRand(importantPoint);
		if (this._spawnOnFloor)
		{
			PhysicsUtility.GetFloorPoint(vector, 10f, 20f, this._floorLayer.value, out vector);
		}
		Vector3 vector2 = vector;
		int num = 5;
		do
		{
			Vector3 vector3 = this.OppositeRand(importantPoint);
			vector2.x = vector3.x;
			vector2.z = vector3.z;
		}
		while (!this.IsPositionFree(vector2) && --num > 0);
		return vector2;
	}

	Vector3 FixedSpawnPos()
	{
		Vector3 position = base.transform.position;
		if (this._spawnOnFloor)
		{
			PhysicsUtility.GetFloorPoint(position, 10f, 20f, this._floorLayer.value, out position);
		}
		return position;
	}

	Vector3 GetSpawnPosition()
	{
		if (this._positionMode == EntitySpawner.PositionMode.Opposite)
		{
			return this.OppositeSpawnPos();
		}
		if (this._positionMode == EntitySpawner.PositionMode.Fixed)
		{
			return this.FixedSpawnPos();
		}
		return this.NormalSpawnPos();
	}

	public static Entity MakeNewEntity(Vector3 P, Entity prefab, AIController controller, EntityObjectAttacher attacher)
	{
		Entity entity = EntityFactory.Instance.GetEntity(prefab, null, P);
		if (controller != null)
		{
			controller.ControlEntity(entity);
		}
		if (attacher != null)
		{
			attacher.GetAttacher().Attach(entity);
		}
		return entity;
	}

	Entity MakeEntity()
	{
		Entity entity = EntitySpawner.MakeNewEntity(this.GetSpawnPosition(), this._entityPrefab, this.controller, this._attacher);
		if (this._positionMode == EntitySpawner.PositionMode.Fixed)
		{
			entity.TurnTo(base.transform.forward, 0f);
		}
		else
		{
			entity.TurnTo(UnityEngine.Random.Range(-180f, 180f), 0f);
		}
		return entity;
	}

	Entity DoSpawn()
	{
		Entity entity = this.MakeEntity();
		this.totalSpawned++;
		EntitySpawner.AddToList(this.trackEnts, entity);
		entity.LocalEvents.DeactivateListener += this.onDeactivate;
		if (this.OnSpawnedEntity != null)
		{
			this.OnSpawnedEntity(entity);
		}

		// Invoke OnEntSpawn events
		ModStuff.GameStateNew.OnEntSpawned(entity);

		return entity;
	}

	void IUpdatable.UpdateObject()
	{
		if (this.totalSpawned < 0)
		{
			return;
		}
		this.timer -= Time.deltaTime;
		if (this.timer <= 0f)
		{
			this.timer = this._delay;
			if (this.CurrentSpawnCount < this.TargetSpawnCount)
			{
				this.DoSpawn();
				return;
			}
			if (!this._respawn)
			{
				base.enabled = false;
			}
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.Lerp(Color.green, Color.black, 0.5f);
		Gizmos.DrawWireCube(this._spawnBounds.center, this._spawnBounds.size);
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(this._spawnBounds.center, this._spawnBounds.size);
	}

	public enum PositionMode
	{
		Normal,
		Opposite,
		Fixed
	}

	public delegate void EntityEventFunc(Entity ent);
}
