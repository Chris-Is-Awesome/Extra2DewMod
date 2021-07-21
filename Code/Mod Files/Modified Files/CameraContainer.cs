using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using ModStuff;

public class CameraContainer : MonoBehaviour
{
	[SerializeField]
	private CameraBehaviour[] _behaviours;
	[SerializeField]
	private PositionInterpolator _interpolator;

	private LevelTime _timer;
	private string _timerName = "currTime";
	private Camera cam;

	private Gradient _gradient;
	private bool fpsmode;
	private bool alreadyhassky;

	//Enable/disable FPS mode
	public void SetupFPSMode (bool mode)
	{
        fpsmode = mode;
	}

	public void SetEnabled (bool enable)
	{
		base.enabled = enable;
	}

	private void Awake ()
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			if (this._behaviours[i] != null)
			{
				this._behaviours[i].SetOwner(this);
			}
		}
		if (this._interpolator == null)
		{
			this._interpolator = base.gameObject.AddComponent<PositionInterpolator>();
		}
        _gradient = CameraSkyColor.Instance.SetSky(out alreadyhassky);
    }

	public void Init (GameObject prefab)
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			this._behaviours[i].Init(prefab);
		}
	}

	public void StartTransition (Vector3 worldFrom, Vector3 worldTo, LevelRoom toRoom = null)
	{
		this.SetRoom(toRoom);
		if (fpsmode) { return; }
		this.SetEnabled(false);
		Vector3 position = base.transform.position;
		Vector3 to;
		if (toRoom != null)
		{
			to = this.GetConstrainedPos(toRoom, worldTo);
		}
		else
		{
			to = worldTo + (position - worldFrom);
		}
		this._interpolator.StartInterpolation(position, to);
	}

	public void FinishTransition ()
	{
		this._interpolator.Interpolate(1f);
		this.SetEnabled(true);
	}

	public void UpdateTransition (float t)
	{
		this._interpolator.Interpolate(t);
	}

	public Vector3 GetConstrainedPos (LevelRoom forRoom, Vector3 target)
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			target = this._behaviours[i].GetConstrainedPos(forRoom, target);
		}
		return target;
	}

	public void SetRoom (LevelRoom room)
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			this._behaviours[i].SetRoom(room);
		}
	}

	private void Update ()
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			this._behaviours[i].DoUpdate();
		}
		if (this.cam == null)
		{
			this.cam = base.gameObject.transform.Find("Cameras").Find("Main Camera").GetComponent<Camera>();
		}
		if (this.cam != null)
		{
			LevelTime timer = this.Timer;
            //If there is a sky, dont do anything
            if (alreadyhassky) { return; }
            //If a color is forced, apply it
            else if (CameraSkyColor.Instance.UseForceColor) this.cam.backgroundColor = CameraSkyColor.Instance.ForceColor;
            //If the default camera is active, set background to black
            else if (!fpsmode) { this.cam.backgroundColor = Color.black; }
            //Else, apply the gradient
            else if (timer != null && fpsmode)
			{
				this.cam.backgroundColor = this._gradient.Evaluate(timer.GetTimePercent(this._timerName));
			}
		}
	}

	private void LateUpdate ()
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			this._behaviours[i].DoLateUpdate();
		}
	}

	private void FixedUpdate ()
	{
		for (int i = 0; i < this._behaviours.Length; i++)
		{
			this._behaviours[i].DoFixedUpdate();
		}
	}

	private LevelTime Timer
	{
		get
		{
			if (this._timer == null)
			{
				this._timer = LevelTime.Instance;
			}
			return this._timer;
		}
	}


}