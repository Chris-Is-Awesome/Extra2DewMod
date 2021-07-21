using System;
using System.Collections.Generic;
using UnityEngine;
using ModStuff;
using ModStuff.ItemRandomizer;

[AddComponentMenu("Ittle 2/Room/Event observers/Spawn entity observer")]
public class SpawnEntityEventObserver : RoomEventObserver
{
	[SerializeField]
	SpawnEntityEventObserver.EnemyData _entity;

	[SerializeField]
	int _maxEnts;

	[SerializeField]
	Vector3 _spawnOffset = Vector3.zero;

	[SerializeField]
	BaseQuickEffect _spawnEffect;

	[SerializeField]
	bool _spawnOnFast;

	[SerializeField]
	bool _inheritTransform;

	List<Entity> currEnts = new List<Entity>();

	LevelRoom savedRoom;

	LevelRoom Room
	{
		get
		{
			if (this.savedRoom == null)
			{
				this.savedRoom = TransformUtility.FindInParents<LevelRoom>(base.transform);
			}
			return this.savedRoom;
		}
	}

	void EntDied(Entity ent)
	{
		this.currEnts.Remove(ent);
	}

	void DoSpawn(SpawnEntityEventObserver.EnemyData data)
	{
		if (this._maxEnts > 0 && this.currEnts.Count >= this._maxEnts)
		{
			return;
		}
		Vector3 vector = base.transform.TransformPoint(this._spawnOffset);
		Entity entity = data.DoSpawn(vector, base.transform.forward, this._inheritTransform);
		this.currEnts.Add(entity);
		entity.LocalEvents.DeactivateListener += this.EntDied;
		EffectFactory.Instance.PlayQuickEffect(this._spawnEffect, vector, null);
	}

    bool CheckRandomizerCondition()
    {
        return ItemRandomizerGM.Instance.Core.Randomizing && name.Contains("Chest");
    }

	protected override void OnActivate(bool activate)
	{
        if (CheckRandomizerCondition())
        {
            TryGiveItem();
        }
        else
        {
            if (activate)
            {
                this.DoSpawn(this._entity);
            }
        }
	}

    void TryGiveItem()
    {
        Vector3 startP = base.transform.TransformPoint(this._spawnOffset);
        Item item = null;
        foreach (Item go in Resources.FindObjectsOfTypeAll<Item>())
        {
            item = ItemFactory.Instance.GetItem(go, transform, Vector3.zero, true);
            break;
        }
        if (item == null) return;
        DummyAction dummy = TransformUtility.FindInParents<DummyAction>(this.transform);
        if (dummy != null)
        {
            item.SetItemAsRandomized(dummy._saveName);
        }

        GameObject player = GameObject.Find("PlayerEnt");
        Entity playEnt = player.GetComponent<Entity>();
        if (playEnt != null)
        {
            item.Pickup(playEnt, true);
        }
    }

	protected override void OnFire(bool fast)
	{
		if (!fast || this._spawnOnFast)
		{
            if (CheckRandomizerCondition())
            {
                TryGiveItem();
            }
            else
            {
                this.DoSpawn(this._entity);
            }
		}
	}

	public bool CanSpawn
	{
		get
		{
			return this._maxEnts <= 0 || this.currEnts.Count < this._maxEnts;
		}
	}

	void RoomDeactivate()
	{
		for (int i = 0; i < this.currEnts.Count; i++)
		{
			Entity entity = this.currEnts[i];
			if (entity != null)
			{
				this.currEnts[i] = null;
				entity.Deactivate();
			}
		}
		this.currEnts.Clear();
	}

	protected override void Awake()
	{
		base.Awake();
		LevelRoom room = this.Room;
		if (room != null)
		{
			room.OnDeactivated += this.RoomDeactivate;
		}
	}

	protected override void OnDestroy()
	{
		if (this.savedRoom != null)
		{
			this.savedRoom.OnDeactivated -= this.RoomDeactivate;
		}
		base.OnDestroy();
	}

	[Serializable]
	public class EnemyData
	{
		public Entity entity;

		public AIController controller;

		public EntityObjectAttacher attacher;

		AIController realController;

		public Entity DoSpawn(Vector3 P, Vector3 dir, bool turn)
		{
			if (this.realController == null && this.controller != null)
			{
				this.realController = ControllerFactory.Instance.GetController<AIController>(this.controller);
			}
			Entity entity = EntitySpawner.MakeNewEntity(P, this.entity, this.realController, this.attacher);
			if (turn)
			{
				entity.TurnTo(dir, 0f);
			}
			return entity;
		}
	}
}
