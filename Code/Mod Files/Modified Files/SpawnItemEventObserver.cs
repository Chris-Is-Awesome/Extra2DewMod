using System;
using System.Diagnostics;
using UnityEngine;
using ModStuff;
using ModStuff.ItemRandomizer;

[AddComponentMenu("Ittle 2/Room/Event observers/Spawn item observer")]
public class SpawnItemEventObserver : RoomEventObserver, IUpdatable, IBaseUpdateable
{
	[SerializeField]
	Item _itemPrefab;

	[SerializeField]
	ItemBase _itemSelector;

	[SerializeField]
	ItemInstanceState.StateData _stateData;

	[SerializeField]
	string _targetEntName;

	[SerializeField]
	bool _pickupDirectly = true;

	[SerializeField]
	Vector3 _spawnOffset = Vector3.zero;

	[SerializeField]
	Vector3 _spawnTarget = Vector3.zero;

	[SerializeField]
	float _startScale = 1f;

	[SerializeField]
	float _endScale = 1f;

	[SerializeField]
	AnimationCurve _spawnCurve;

	[SerializeField]
	float _spawnTime;

	[SerializeField]
	SoundClip _spawnSound;

	[SerializeField]
	Transform _spawnRoot;

	Vector3 startP;

	Vector3 endP;

	Vector3 startScale;

	float timer;

	Item showItem;

	public event SpawnItemEventObserver.OnSpawnedFunc OnSpawn;

	protected override void OnActivate (bool activate)
	{
		if (activate)
		{
			this.SpawnItem();
		}
	}

	protected override void OnFire (bool fast)
	{
		if (!fast)
		{
			this.SpawnItem();
		}
	}

    //This was heavily modified for the item randomizer
	void SpawnItem ()
	{
		Entity entity = base.Triggerer ?? TransformUtility.GetByName<Entity>(this._targetEntName);

        Item item = ItemBase.SelectItem(entity, (!(this._itemSelector != null)) ? this._itemPrefab : this._itemSelector);

        //Item_Heart3 is the heart spawned by passel's shifting room. This disables the randomization
        bool dontRandomize = false;
        if (item != null && item.name == "Item_Heart3") dontRandomize = true; 
		if (item == null)
		{
            if(ItemRandomizerGM.Instance.Core.Randomizing)
            {
                foreach (Item go in Resources.FindObjectsOfTypeAll<Item>())
                {
                    item = ItemFactory.Instance.GetItem(go, this._spawnRoot, this.startP, !this._pickupDirectly, this._stateData);
                    break;
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Attempt to spawn null item - remove spawner instead");
                return;
            }
		}
		this.startP = base.transform.TransformPoint(this._spawnOffset);
		this.endP = base.transform.TransformPoint(this._spawnTarget);
		this.showItem = ItemFactory.Instance.GetItem(item, this._spawnRoot, this.startP, !this._pickupDirectly, this._stateData);
        if (!dontRandomize && ItemRandomizerGM.Instance.Core.Randomizing)
        {
            DummyAction dummy = TransformUtility.FindInParents<DummyAction>(this.transform);
            if (dummy != null)
            {
                //ModText.QuickText("name is: " + showItem.name);
                if(this.showItem == null)
                {
                    foreach (Item go in Resources.FindObjectsOfTypeAll<Item>())
                    {
                        this.showItem = ItemFactory.Instance.GetItem(go, this._spawnRoot, this.startP, !this._pickupDirectly, this._stateData);
                        break;
                    }
                }
                showItem.SetItemAsRandomized(dummy._saveName);
            }
        }
        this.startScale = this.showItem.transform.localScale;
		this.showItem.transform.localScale = this.startScale * this._startScale;
		if (this._spawnSound != null)
		{
			SoundPlayer.Instance.PlayPositionedSound(this._spawnSound, base.transform.position, null);
		}
		if (this.OnSpawn != null)
		{
			this.OnSpawn(this.showItem, this);
		}
		this.timer = this._spawnTime;
		base.enabled = true;
		if (this._pickupDirectly)
		{
			if (entity != null)
			{
				this.showItem.Pickup(entity, true);
				this.showItem.ActivateGraphics();
			}
			else
			{
				UnityEngine.Debug.Log("No entity found to spawn the item for " + base.name);
			}
		}
	}

	protected override void OnDisable ()
	{
		if (this.showItem != null && this._pickupDirectly)
		{
			this.showItem.transform.localScale = this.startScale;
			this.showItem.Deactivate();
			this.showItem = null;
		}
		base.OnDisable();
	}

	void IUpdatable.UpdateObject ()
	{
		if (this.showItem == null)
		{
			base.enabled = false;
			return;
		}
		this.timer -= Time.deltaTime;
		float t = this._spawnCurve.Evaluate(1f - Mathf.Clamp01(this.timer / this._spawnTime));
		Vector3 position = Vector3.Lerp(this.startP, this.endP, t);
		this.showItem.transform.position = position;
		this.showItem.transform.localScale = this.startScale * Mathf.Lerp(this._startScale, this._endScale, t);
		if (this.timer <= 0f)
		{
			this.showItem.transform.localScale = this.startScale;
			if (this._pickupDirectly)
			{
				this.showItem.Deactivate();
			}
			this.showItem = null;
			base.enabled = false;
		}
	}

	public delegate void OnSpawnedFunc (Item item, SpawnItemEventObserver spawner);
}
