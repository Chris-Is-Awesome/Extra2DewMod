using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : CameraBehaviour
{
	private static Dictionary<string, CameraShaker> shakerLookup = new Dictionary<string, CameraShaker>();

	private static List<CameraShaker> allShakers = new List<CameraShaker>();

	[SerializeField]
	private Vector3 _shakeX = Vector3.right;

	[SerializeField]
	private Vector3 _shakeY = Vector3.up;

	[SerializeField]
	private string _shakerId;

	private List<CameraShaker.Shaker> shakers = new List<CameraShaker.Shaker>();

	//Added this variable
	public float extra_shake;

	public CameraShaker ()
	{
	}

	public static CameraShaker GetShakerById (string id, bool fallbackOnAny = false)
	{
		CameraShaker result;
		if (!CameraShaker.shakerLookup.TryGetValue(id, out result) && fallbackOnAny)
		{
			return CameraShaker.GetAnyShaker();
		}
		return result;
	}

	public static CameraShaker GetAnyShaker ()
	{
		if (CameraShaker.allShakers.Count > 0)
		{
			return CameraShaker.allShakers[0];
		}
		return null;
	}

	private void Awake ()
	{
		CameraShaker.shakerLookup[this._shakerId] = this;
		CameraShaker.allShakers.Add(this);
	}

	private void OnDestroy ()
	{
		CameraShaker.shakerLookup.Remove(this._shakerId);
		CameraShaker.allShakers.Remove(this);
	}

	public void StartShake (float freq, float amp, float time, AnimationCurve ampCurve = null)
	{
		base.enabled = true;
		//Added extra shake the the shake argument here
		this.shakers.Add(new CameraShaker.Shaker(time, freq, amp + extra_shake, ampCurve));
	}

	public CameraShaker.ShakeTag StartContShake (float freq, float amp)
	{
		base.enabled = true;
		CameraShaker.Shaker shaker = new CameraShaker.Shaker(freq, amp);
		this.shakers.Add(shaker);
		return new CameraShaker.ShakeTag(shaker);
	}

	public override void DoLateUpdate ()
	{
		if (this.shakers.Count == 0)
		{
			base.enabled = false;
			return;
		}
		Vector2 a = Vector2.zero;
		for (int i = this.shakers.Count - 1; i >= 0; i--)
		{
			Vector2 b;
			if (!this.shakers[i].Update(Time.deltaTime, out b))
			{
				this.shakers.RemoveAt(i);
			}
			else
			{
				a += b;
			}
		}
		Vector3 a2 = base.transform.TransformDirection(this._shakeX);
		Vector3 a3 = base.transform.TransformDirection(this._shakeY);
		base.transform.localPosition += a2 * a.x + a3 * a.y;
	}

	// Note: this type is marked as 'beforefieldinit'.
	static CameraShaker ()
	{
	}

	public class ShakeTag
	{
		private CameraShaker.Shaker shaker;

		public ShakeTag (object shaker)
		{
			this.shaker = (CameraShaker.Shaker)shaker;
		}

		public void SetScale (float mu)
		{
			this.shaker.SetScale(mu);
		}

		public void Stop ()
		{
			CameraShaker.Shaker shaker = this.shaker;
			this.shaker = null;
			if (shaker != null)
			{
				shaker.Stop();
			}
		}
	}

	private class Shaker
	{
		private float timer;

		private float timeScale;

		private float freq;

		private float amp;

		private float xOff;

		private float yOff;

		private AnimationCurve ampCurve;

		private bool cont;

		private bool stopped;

		public Shaker (float time, float freq, float amp, AnimationCurve ampCurve)
		{
			this.timer = time;
			this.timeScale = 1f / time;
			this.freq = freq;
			this.amp = amp;
			this.xOff = UnityEngine.Random.Range(-100f, 100f);
			this.yOff = UnityEngine.Random.Range(-100f, 100f);
			this.ampCurve = ampCurve;
		}

		public Shaker (float freq, float amp)
		{
			this.timer = 100f;
			this.timeScale = 1f;
			this.freq = freq;
			this.amp = amp;
			this.xOff = UnityEngine.Random.Range(-100f, 100f);
			this.yOff = UnityEngine.Random.Range(-100f, 100f);
			this.cont = true;
			this.stopped = false;
		}

		public void SetScale (float mu)
		{
			this.timeScale = mu;
		}

		public void Stop ()
		{
			this.stopped = true;
		}

		public bool Update (float dt, out Vector2 offset)
		{
			this.timer -= dt;
			float num;
			if (this.cont)
			{
				num = this.timeScale;
			}
			else
			{
				num = this.timer * this.timeScale;
			}
			float num2 = Mathf.PerlinNoise(this.xOff, this.timer * this.freq) * 2f - 1f;
			float num3 = Mathf.PerlinNoise(this.timer * this.freq, this.yOff) * 2f - 1f;
			float num4;
			if (this.ampCurve != null)
			{
				num4 = this.amp * this.ampCurve.Evaluate(1f - num);
			}
			else
			{
				num4 = this.amp * num;
			}
			offset = new Vector2(num2 * num4, num3 * num4);
			if (this.cont)
			{
				return !this.stopped;
			}
			return this.timer > 0f;
		}
	}
}
