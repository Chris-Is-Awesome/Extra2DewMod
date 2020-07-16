using System;
using UnityEngine;
using ModStuff;

[AddComponentMenu("Ittle 2/Item/Comps/Health affecting item")]
public class HealthAffectingItem : ItemComponent
{
	[SerializeField]
	float _value;

	[SerializeField]
	HealthAffectingItem.Mode _mode;

	public HealthAffectingItem()
	{
	}

	protected override void DoApply(Entity toEntity, bool fast)
	{
		// During Heart Rush, add hearts depending on heart type
		ModeController mc = GameObject.Find("ModeController").GetComponent<ModeController>();
		if (mc.isHeartRush)
		{
			// If crayon
			if (_value == 1)
			{
				Killable playerHp = toEntity.GetEntityComponent<Killable>();
				mc.heartRushManager.OnCrayonPickup(playerHp);
			}
			return;
		}

		// Update stats
		if (gameObject.name.Contains("Item_") && gameObject.name.Contains("Heart"))
		{
			// Updates hearts collected count
			ModMaster.UpdateStats("HeartsCollected");

			// Updates health healed, if you can heal at all
			Killable playerHPData = toEntity.GetEntityComponent<Killable>();
			bool canHeal = playerHPData.MaxHp > playerHPData.CurrentHp;

			if (canHeal)
			{
				float maxHeal = playerHPData.MaxHp - playerHPData.CurrentHp;
				float hpHealed = _value;

				if (hpHealed > maxHeal)
				{
					hpHealed = maxHeal;
				}

				ModMaster.UpdateStats("HealthHealed", (int)hpHealed);
			}
		}

		Killable entityComponent = toEntity.GetEntityComponent<Killable>();
		if (entityComponent != null)
		{
			if (this._mode == HealthAffectingItem.Mode.AddHealth)
			{
				entityComponent.CurrentHp += this._value;
			}
			else if (this._mode == HealthAffectingItem.Mode.Restore)
			{
				entityComponent.CurrentHp = entityComponent.MaxHp;
			}
			else if (this._mode == HealthAffectingItem.Mode.AddMaxHealth || this._mode == HealthAffectingItem.Mode.AddMaxAndRestore)
			{
				entityComponent.MaxHp += this._value;
				if (this._mode == HealthAffectingItem.Mode.AddMaxAndRestore)
				{
					entityComponent.CurrentHp = entityComponent.MaxHp;
				}
			}
		}
	}

	public enum Mode
	{
		AddHealth,
		Restore,
		AddMaxHealth,
		AddMaxAndRestore
	}
}
