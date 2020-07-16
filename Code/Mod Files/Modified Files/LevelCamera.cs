using System;
using UnityEngine;

public class LevelCamera : CameraBehaviour
{
	[SerializeField]
	private Camera _realCam;

	[SerializeField]
	private float _xPadding;

	[SerializeField]
	private float _topPadding;

	[SerializeField]
	private float _bottomPadding;

	[SerializeField]
	private LevelRoom _currentRoom;

	[SerializeField]
	private AnimationCurve _releaseCurve;

	private LevelRoom oldRoom;
	private Quaternion baseRot;
	private Vector3 minP;
	private Vector3 maxP;
	private Vector3 baseOffset;
	private Bounds currBounds;
	private bool cancelContrain;
	private bool releaseRoom;
	private float releaseTimer;
	private float releaseTimeScale;
	private Boolean fpsmode; //Added this variable to know if FPS mode is on

	public LevelCamera ()
	{
	}

	//This function enables FPS mode
	public void SetupFPSMode (Boolean mode)
	{
		fpsmode = mode;
	}

	private void SetupBounds (LevelRoom forRoom)
	{
		this.currBounds = this.CalculateBounds(forRoom, out this.minP, out this.maxP);
	}

	private Bounds CalculateBounds (LevelRoom forRoom, out Vector3 min, out Vector3 max)
	{
		Bounds bounds = forRoom.Bounds;
		Vector3 p = bounds.center + Vector3.down * bounds.extents.y;
		global::Plane plane = new global::Plane(Vector3.up, p);
		Vector3 position = base.transform.position;
		Vector3 position2 = position;
		position2.y = bounds.min.y + this.baseOffset.y;
		base.transform.position = position2;
		Quaternion localRotation = base.transform.localRotation;
		base.transform.localRotation = this.baseRot;
		Ray ray = this._realCam.ViewportPointToRay(new Vector3(0f, 0f, 0f));
		Ray ray2 = this._realCam.ViewportPointToRay(new Vector3(1f, 1f, 0f));
		base.transform.localRotation = localRotation;
		Vector3 position3 = base.transform.position;
		Vector3 vector = plane.Intersection(ray) - position3;
		Vector3 vector2 = plane.Intersection(ray2) - position3;
		base.transform.position = position;
		min = default(Vector3);
		max = default(Vector3);
		max.x = (vector2.x - vector.x) * 0.5f;
		min.x = -max.x;
		min.y = 0f;
		max.y = 0f;
		min.z = vector.z;
		max.z = vector2.z;
		return bounds;
	}

	private static float Constrain (float p, float d0, float d1, float min, float max)
	{
		if (d1 - d0 > max - min)
		{
			p = (max + min) * 0.5f - (d0 + d1) * 0.5f;
		}
		else if (p + d0 < min)
		{
			p = min - d0;
		}
		else if (p + d1 > max)
		{
			p = max - d1;
		}
		return p;
	}

	private Vector3 ConstrainPos (Bounds bounds, Vector3 min, Vector3 max, Vector3 P)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		max.x -= this._xPadding;
		min.x += this._xPadding;
		max.z -= this._topPadding;
		min.z += this._bottomPadding;
		P.x = LevelCamera.Constrain(P.x, min.x, max.x, center.x - extents.x, center.x + extents.x);
		P.z = LevelCamera.Constrain(P.z, min.z, max.z, center.z - extents.z, center.z + extents.z);
		return P;
	}

	private void ConstrainPos ()
	{
		base.transform.position = this.ConstrainPos(this.currBounds, this.minP, this.maxP, base.transform.position);
	}

	public override void Init (GameObject prefab)
	{
		this.baseRot = prefab.transform.localRotation;
		this.baseOffset = prefab.transform.localPosition;
	}

	public override Vector3 GetConstrainedPos (LevelRoom room, Vector3 target)
	{
		Vector3 min;
		Vector3 max;
		Bounds bounds = this.CalculateBounds(room, out min, out max);
		return this.ConstrainPos(bounds, min, max, target);
	}

	public override void SetRoom (LevelRoom room)
	{
		this._currentRoom = room;
		this.releaseRoom = false;
	}

	public void ReleaseRoom (float time)
	{
		this.releaseTimer = time;
		this.releaseTimeScale = 1f / time;
		this.releaseRoom = true;
		this._currentRoom = null;
	}

	public void CancelConstrain (bool cancel)
	{
		this.cancelContrain = cancel;
	}

	public Camera GetRealCam ()
	{
		return this._realCam;
	}

	public override void DoLateUpdate ()
	{
		if (fpsmode) { return; } //If not in FPS mode, return
		if (this._currentRoom != this.oldRoom)
		{
			this.oldRoom = this._currentRoom;
			if (this._currentRoom != null)
			{
				this.SetupBounds(this._currentRoom);
			}
		}
		if (this._currentRoom != null && !this.cancelContrain)
		{
			this.ConstrainPos();
		}
		else if (this.releaseRoom)
		{
			this.releaseTimer -= Time.deltaTime;
			float time = 1f - Mathf.Clamp01(this.releaseTimer * this.releaseTimeScale);
			Vector3 position = base.transform.position;
			Vector3 a = this.ConstrainPos(this.currBounds, this.minP, this.maxP, position);
			base.transform.position = Vector3.Lerp(a, position, this._releaseCurve.Evaluate(time));
			if (this.releaseTimer <= 0f)
			{
				this.releaseRoom = false;
			}
		}
	}
}
