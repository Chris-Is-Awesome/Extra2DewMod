using System;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : CameraBehaviour
{
	private static List<FollowTransform> allFollowers = new List<FollowTransform>();

	[SerializeField]
	private Transform _target;

	[SerializeField]
	private Vector3 _rotMask = Vector3.zero;

	[SerializeField]
	private string _tag;

	private Vector3 dist;
	private Vector3 baseDist;
	private Vector3 startRot;
	private Vector3 baseRot;
	private Transform myTrans;
	private Vector3 lastSecondPos;
	private Transform secondTarget;
	private float secondMu;
	private bool hasSecond;
	private Vector3 followScale;
	private Vector3 scaleStart;
	private bool useScale;

	// Note: this type is marked as 'beforefieldinit'.
	static FollowTransform ()
	{
	}

	public FollowTransform ()
	{
	}

	private void Awake ()
	{
		FollowTransform.allFollowers.Add(this);
		this.myTrans = base.transform;
		this.baseRot = (this.startRot = this.myTrans.localEulerAngles);
		if (this._target != null)
		{
			this.baseDist = (this.dist = this.myTrans.position - this._target.position);
		}
	}

	private void OnDestroy ()
	{
		FollowTransform.allFollowers.Remove(this);
	}

	public void GetBaseVectors (out Vector3 targetPos, out Vector3 dist, out Vector3 rot)
	{
		targetPos = ((!(this._target != null)) ? (base.transform.position - this.dist) : this._target.position);
		dist = this.baseDist;
		rot = this.baseRot;
	}

	public void SetVectors (Vector3 dist, Vector3 rot)
	{
		this.dist = dist;
		this.startRot = rot;
	}

	public void SetTarget (Transform target)
	{
		this._target = target;
		this.baseDist = (this.dist = this.myTrans.position - this._target.position);
	}

	public override Vector3 GetConstrainedPos (LevelRoom forRoom, Vector3 target)
	{
		return target + this.dist;
	}

	public void SetFollowScale (Vector3 scale)
	{
		if (this._target != null)
		{
			this.scaleStart = this._target.position;
			this.followScale = scale;
			this.useScale = true;
		}
	}

	public void ClearFollowScale ()
	{
		this.useScale = false;
	}

	private Vector3 GetPos (Transform target)
	{
		if (this.useScale)
		{
			return this.scaleStart + MathUtility.CompMul(target.position - this.scaleStart, this.followScale) + this.dist;
		}
		return target.position + this.dist;
	}

	public override void DoLateUpdate ()
	{
		//Check if fpsmode is active, if it is, do camera manipulation
		if (fpsmode)
		{
			FPSDoLateUpdate();
			return;
		}
		if (this._target != null)
		{
			Vector3 vector = this.GetPos(this._target);
			if (this.hasSecond)
			{
				Vector3 b;
				if (this.secondTarget != null && this.secondTarget.gameObject.activeInHierarchy)
				{
					b = (this.lastSecondPos = this.GetPos(this.secondTarget));
				}
				else
				{
					b = this.lastSecondPos;
				}
				vector = Vector3.Lerp(vector, b, this.secondMu);
			}
			this.myTrans.position = vector;
			Vector3 eulerAngles = this._target.eulerAngles;
			eulerAngles.x *= this._rotMask.x;
			eulerAngles.y *= this._rotMask.y;
			eulerAngles.z *= this._rotMask.z;
			this.myTrans.localEulerAngles = eulerAngles + this.startRot;
		}
	}

	public void SetSecondTarget (Transform target, float mu)
	{
		this.secondTarget = target;
		this.secondMu = mu;
		this.hasSecond = (target != null);
	}

	public void SetSecondTargetMu (float mu)
	{
		this.secondMu = mu;
	}

	public Transform GetSecondTarget ()
	{
		return this.secondTarget;
	}

	public static FollowTransform GetFollower (string tag, bool getAny = false)
	{
		for (int i = FollowTransform.allFollowers.Count - 1; i >= 0; i--)
		{
			if (FollowTransform.allFollowers[i]._tag == tag)
			{
				return FollowTransform.allFollowers[i];
			}
		}
		if (getAny && FollowTransform.allFollowers.Count > 0)
		{
			return FollowTransform.allFollowers[0];
		}
		return null;
	}

	//Mod code
	private Boolean fpsmode;
	private PlayerController playerController;

	public void SetupFPSMode (Boolean mode)
	{
		if (playerController == null) { playerController = GameObject.Find("PlayerController").GetComponent<PlayerController>(); }
		fpsmode = mode;
	}

	private void FPSDoLateUpdate ()
	{
		myTrans.localEulerAngles = playerController.cameraRotation;
		myTrans.position = playerController.cameraPosition;
	}
}
