using System;
using System.Collections.Generic;
using UnityEngine;
using ModStuff;

[AddComponentMenu("Ittle 2/Item/Item")]
public class Item : ItemBase, IBC_TriggerEnterListener, IBC_CollisionEventListener
{
	[SerializeField]
	ItemId _itemId;

	[SerializeField]
	bool _important;

	BC_Collider betterCollider;

	List<ItemComponent> allComps;

	ItemInstanceState[] instState;

	Item.OnDespawnFunc onDespawn;

	Item.OnPickedUpFunc onPickedUp;

	LevelRoom currRoom;

	bool activated;

	bool wasActivated;

	public ItemId ItemId
	{
		get
		{
			return this._itemId;
		}
	}

	public event Item.OnDespawnFunc OnDespawn
	{
		add
		{
			if (this.onDespawn == null)
			{
				this.onDespawn = value;
			}
			else
			{
				this.onDespawn = (Item.OnDespawnFunc)Delegate.Combine(this.onDespawn, value);
			}
		}
		remove
		{
			if (this.onDespawn != null)
			{
				this.onDespawn = (Item.OnDespawnFunc)Delegate.Remove(this.onDespawn, value);
			}
		}
	}

	public event Item.OnPickedUpFunc OnPickedUp
	{
		add
		{
			if (this.onPickedUp == null)
			{
				this.onPickedUp = value;
			}
			else
			{
				this.onPickedUp = (Item.OnPickedUpFunc)Delegate.Combine(this.onPickedUp, value);
			}
		}
		remove
		{
			if (this.onPickedUp != null)
			{
				this.onPickedUp = (Item.OnPickedUpFunc)Delegate.Remove(this.onPickedUp, value);
			}
		}
	}

	void Awake()
	{
		this.betterCollider = PhysicsUtility.RegisterColliderEvents(base.gameObject, this);
		this.instState = base.GetComponentsInChildren<ItemInstanceState>();
		this.allComps = new List<ItemComponent>(base.GetComponentsInChildren<ItemComponent>());
		for (int i = 0; i < this.allComps.Count; i++)
		{
			this.allComps[i].Enable(this);
		}
	}

	void OnDestroy()
	{
		if (this.betterCollider != null)
		{
			this.betterCollider.UnregisterEventListener(this);
		}
	}

	void Start()
	{
		if (!this.wasActivated)
		{
			this.Activate(null);
		}
	}

	public void Activate(ItemInstanceState.StateData data)
	{
		this.wasActivated = true;
		if (this.activated)
		{
			return;
		}
		this.currRoom = LevelRoom.GetRoomForPosition(base.transform.position, null);
		if (this.currRoom != null)
		{
			this.currRoom.OnDeactivated += this.Deactivate;
		}
		base.gameObject.SetActive(true);
		if (this.instState != null && data != null)
		{
			for (int i = 0; i < this.instState.Length; i++)
			{
				this.instState[i].Apply(data);
			}
		}
		this.activated = true;
		for (int j = 0; j < this.allComps.Count; j++)
		{
			this.allComps[j].Activate();
		}
	}

	public void Deactivate()
	{
		if (!this.activated)
		{
			base.gameObject.SetActive(false);
			return;
		}
		if (this.currRoom != null)
		{
			this.currRoom.OnDeactivated -= this.Deactivate;
		}
		for (int i = this.allComps.Count - 1; i >= 0; i--)
		{
			this.allComps[i].Deactivate();
		}
		if (this.instState != null)
		{
			for (int j = 0; j < this.instState.Length; j++)
			{
				this.instState[j].Reset();
			}
		}
		Item.OnDespawnFunc onDespawnFunc = this.onDespawn;
		this.onDespawn = null;
		if (onDespawnFunc != null)
		{
			onDespawnFunc(this);
		}
		this.onPickedUp = null;
		this.activated = false;
		base.gameObject.SetActive(false);
	}

	public void Pickup(Entity ent, bool fast = false)
	{
		// Trigger events on item pickup
		Singleton<GameState>.Instance.OnItemGet(this);

		// Invoke OnItemGet events
		GameStateNew.OnItemGotten(this);

		for (int i = this.allComps.Count - 1; i >= 0; i--)
		{
			this.allComps[i].Apply(ent, fast);
		}
		if (this._important)
		{
			ent.SaveState();
		}
		Item.OnPickedUpFunc onPickedUpFunc = this.onPickedUp;
		this.onPickedUp = null;
		if (onPickedUpFunc != null)
		{
			onPickedUpFunc(this, ent);
		}
		this.Deactivate();
		ent.LocalEvents.SendItemGet(ent, this);
	}

	public void ActivateGraphics()
	{
		base.gameObject.SetActive(true);
	}

	void IBC_TriggerEnterListener.OnTriggerEnter(BC_TriggerData col)
	{
		if (!this.activated)
		{
			return;
		}
		Entity component = col.collider.GetComponent<Entity>();
		if (component != null && !component.InactiveOrDead)
		{
			this.Pickup(component, false);
		}
	}

	protected override Item DoSelectItem(Entity forEnt, ItemBase prefab)
	{
		return this;
	}

	public delegate void OnDespawnFunc(Item item);

	public delegate void OnPickedUpFunc(Item item, Entity ent);
}
