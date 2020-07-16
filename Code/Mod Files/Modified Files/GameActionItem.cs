using System;
using UnityEngine;
using ModStuff;

[AddComponentMenu("Ittle 2/Item/Comps/Game action item")]
public class GameActionItem : ItemComponent
{
	[SerializeField]
	GameAction _action;

	[SerializeField]
	bool _entAsTarget;

	[SerializeField]
	bool _entAsOwner = true;

	protected override void DoApply (Entity toEntity, bool fast)
	{
		// Update stats
		if (gameObject.name == "Item_LightningBall")
		{
			ModMaster.UpdateStats("LightningsUsed");
		}

		Entity target = (!this._entAsTarget) ? null : toEntity;
		Entity owner = (!this._entAsOwner) ? null : toEntity;
		GameAction.ActionData data = new GameAction.ActionData(base.transform.position, 1f, target, owner);
		this._action.Execute(data);
	}
}
