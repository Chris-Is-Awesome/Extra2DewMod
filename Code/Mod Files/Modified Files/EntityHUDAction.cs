using System;
using UnityEngine;

[PersistentScriptableObject]
public class EntityHUDAction : ScriptableObject
{
	[BitMask(typeof(EntityHUDAction.PreventMode))]
	[SerializeField]
	EntityHUDAction.PreventMode _preventMode;

	[SerializeField]
	UpdatableLayer _checkPauseLayer;

	[SerializeField]
	EntityOverlayWindow _result;

	[SerializeField]
	string _wndArg;

	[SerializeField]
	bool _exclusive;

	public bool CanShow(Entity ent)
	{
		if ((this._preventMode & EntityHUDAction.PreventMode.EntityDead) != EntityHUDAction.PreventMode.None && (ent == null || ent.InactiveOrDead))
		{
			return false;
		}
		if ((this._preventMode & EntityHUDAction.PreventMode.Pause) != EntityHUDAction.PreventMode.None && ObjectUpdater.Instance.IsPaused(this._checkPauseLayer))
		{
			return false;
		}
		if ((this._preventMode & EntityHUDAction.PreventMode.Cutscene) != EntityHUDAction.PreventMode.None)
		{
			if (DialoguePlayer.InstanceExists && DialoguePlayer.Instance.IsRunning)
			{
				return false;
			}
			if (ent != null)
			{
				RoomSwitchable entityComponent = ent.GetEntityComponent<RoomSwitchable>();
				if (entityComponent != null && entityComponent.IsActive)
				{
					return false;
				}
			}
		}
		return this._result.CanShow(ent);
	}

	public EntityOverlayWindow DoShow(Entity ent, EntityOverlayWindow.OnDoneFunc onDone = null, string arg2 = null)
	{
		EntityOverlayWindow pooledWindow = OverlayWindow.GetPooledWindow<EntityOverlayWindow>(this._result);
		pooledWindow.Show(ent, this._wndArg, onDone, arg2);
		return pooledWindow;
	}

	public bool Exclusive
	{
		get
		{
			return this._exclusive;
		}
	}

    public void SetWnd(string wnd)
    {
        _wndArg = wnd;
    }

	[Flags]
	public enum PreventMode
	{
		None = 0,
		Pause = 1,
		Cutscene = 2,
		InGame = 4,
		EntityDead = 8
	}
}
