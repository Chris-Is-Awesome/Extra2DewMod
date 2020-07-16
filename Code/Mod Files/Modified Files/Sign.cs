using System;
using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour
{
	private class OverlapFunc : IBC_OverlapFunc
	{
		public void Setup(Vector3 P, float r2)
		{
			this.P = P;
			this.r2 = r2;
			this.result = null;
		}

		public bool HandleHit(BC_OverlapHit hit)
		{
			Transform transform = hit.collider.transform;
			if ((transform.position - this.P).sqrMagnitude < this.r2)
			{
				this.result = transform;
				return true;
			}
			return false;
		}

		public Vector3 P;

		public float r2;

		public Transform result;
	}

	public bool PlayerInRadius(float radius)
	{
		Sign.OverlapFunc overlapFunc = this.savedFunc;
		overlapFunc.Setup(base.transform.position, radius * radius);
		BC_Tracing.Instance.OverlapSphereAll<Sign.OverlapFunc>(base.transform.position, radius, overlapFunc, this._playerLayer.value, false);
		if (overlapFunc.result != null)
		{
			this.playerT = overlapFunc.result;
			return true;
		}
		return false;
	}

	private bool TransformNotInRadius(float radius, Transform t)
	{
		return t == null || (t.position - base.transform.position).sqrMagnitude > radius * radius;
	}

	public void Show()
	{
		this.isShowing = true;
		SpeechBubble pooledWindow = OverlayWindow.GetPooledWindow<SpeechBubble>(this._speechBubblePrefab);
		StringHolder.OutString fullString;
		if (this._configString != null && !useCustomText) //If no custom text, use vanilla text
		{
			Dictionary<string, string> vars = (!(this._varLookup != null)) ? null : this._varLookup.GetValues();
			string forString = null;
			if (this._altStrings != null && this._altStrings.Length != 0 && this._exprData != null && this._exprContext != null)
			{
				int num = this._exprData.Evaluate(this._exprContext);
				if (num > 0 && num <= this._altStrings.Length)
				{
					forString = this._altStrings[num - 1];
				}
			}
			fullString = this._configString.GetFullString(forString, vars);
		}
		else
		{
			fullString = new StringHolder.OutString(this._text);
		}
		Vector3 pos = (!this._reverseTarget) ? base.transform.position : this.playerT.transform.position;
		Transform mrSpeaker = (!this._reverseTarget) ? base.transform : this.playerT;
		pooledWindow.Show(pos, this._objectSize, fullString, this._offset, mrSpeaker, this._reverseTarget);
		if (this._showSound != null)
		{
			SoundPlayer.Instance.PlayPositionedSound(this._showSound, base.transform.position, null);
		}
		this.bubble = pooledWindow;
	}

	public void Hide()
	{
		this.isShowing = false;
		if (this.bubble != null)
		{
			this.bubble.Hide();
			this.bubble = null;
		}
		if (this._hideSound != null)
		{
			SoundPlayer.Instance.PlayPositionedSound(this._hideSound, base.transform.position, null);
		}
	}

	private void OnDisable()
	{
		if (this.isShowing)
		{
			this.Hide();
		}
	}

	private void Update()
	{
		if (!this.isShowing)
		{
			if (this.PlayerInRadius(this._showRadius))
			{
				this.Show();
				return;
			}
		}
		else if (this.TransformNotInRadius(this._hideRadius, this.playerT))
		{
			this.Hide();
		}
	}

    //Mod function to set custom text
    public void ChangeText(string newText)
    {
        _text = newText.Replace('|','\n');
        useCustomText = (newText != "");
        if (PlayerInRadius(_showRadius))
        {
            Hide();
            Show();
        }
    }

    //Variable that enables/disables custom text
    private bool useCustomText;

    [Multiline]
	[SerializeField]
	private string _text;

	[SerializeField]
	private ConfigString _configString;

	[SerializeField]
	public float _showRadius = 1f;

	[SerializeField]
	private float _hideRadius = 1.1f;

	[SerializeField]
	private float _objectSize = 0.45f;

	[SerializeField]
	private bool _reverseTarget;

	[SerializeField]
	private SpeechBubble _speechBubblePrefab;

	[SerializeField]
	private LayerMask _playerLayer;

	[SerializeField]
	private Vector3 _offset = Vector3.up;

	[SerializeField]
	private SoundClip _showSound;

	[SerializeField]
	private SoundClip _hideSound;

	[SerializeField]
	private string[] _altStrings;

	[SerializeField]
	private ExpressionHolder _exprData;

	[SerializeField]
	private ExprVarHolderBase _exprContext;

	[SerializeField]
	private VarLookupHolder _varLookup;

	private SpeechBubble bubble;

	private Transform playerT;

	private bool isShowing;

	private Sign.OverlapFunc savedFunc = new Sign.OverlapFunc();
}
