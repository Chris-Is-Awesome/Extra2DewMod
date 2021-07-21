using System;
using UnityEngine;

public class ItemMessageBox : OverlayWindow
{
	[SerializeField]
	BetterTextMesh _text;

	[SerializeField]
	NineSlice _background;

	[SerializeField]
	float _border;

	[SerializeField]
	Renderer _itemPic;

	[SerializeField]
	TextLoader _textLoader;

	[SerializeField]
	ObjectTweener _tweener;

	[SerializeField]
	float _showTime = 1f;

	Material mat;

	Texture texture;

	float timer;

	bool countdown;

	Vector2 minSize;

	Vector3 backOrigin;

	void Awake()
	{
		this.mat = this._itemPic.material;
		this.minSize = this._background.ScaledSize;
		this.backOrigin = this._background.transform.localPosition;
	}

	protected override void OnDestroy()
	{
		if (this.texture != null)
		{
			Resources.UnloadAsset(this.texture);
		}
		if (this.mat != null)
		{
			Object.Destroy(this.mat);
		}
	}

    //Added by method injection
    public void Show(Texture2D texture2D, StringHolder.OutString text)
    {
        if (this._tweener != null)
        {
            this._tweener.Show(true);
        }
        else
        {
            base.gameObject.SetActive(true);
        }
        this.texture = texture2D;
        this.mat.mainTexture = this.texture;
        this._text.StringText = text;
        Vector2 scaledTextSize = this._text.ScaledTextSize;
        Vector3 vector = this._text.transform.localPosition - this.backOrigin;
        scaledTextSize.y += Mathf.Abs(vector.y) + this._border;
        scaledTextSize.y = Mathf.Max(this.minSize.y, scaledTextSize.y);
        scaledTextSize.x = this._background.ScaledSize.x;
        this._background.ScaledSize = scaledTextSize;
        this.timer = this._showTime;
        this.countdown = (this._showTime > 0f);
    }

    public void Show(string itemPicRes, StringHolder.OutString text)
	{
		if (this._tweener != null)
		{
			this._tweener.Show(true);
		}
		else
		{
			base.gameObject.SetActive(true);
		}
		Texture2D texture2D = Resources.Load(itemPicRes) as Texture2D;
		if (this.texture != texture2D)
		{
			Resources.UnloadAsset(this.texture);
		}
		this.texture = texture2D;
		this.mat.mainTexture = this.texture;
		this._text.StringText = text;
		Vector2 scaledTextSize = this._text.ScaledTextSize;
		Vector3 vector = this._text.transform.localPosition - this.backOrigin;
		scaledTextSize.y += Mathf.Abs(vector.y) + this._border;
		scaledTextSize.y = Mathf.Max(this.minSize.y, scaledTextSize.y);
		scaledTextSize.x = this._background.ScaledSize.x;
		this._background.ScaledSize = scaledTextSize;
		this.timer = this._showTime;
		this.countdown = (this._showTime > 0f);
	}

	public void Show(ItemId item)
	{
		StringHolder strings = this._textLoader.GetStrings();
		if (strings != null)
		{
			this.Show(item.ItemGetPic, strings.GetFullString(item.ItemGetString, null));
		}
		else
		{
			this.Show(item.ItemGetPic, new StringHolder.OutString(item.ItemGetString));
		}
	}

	public void Hide(bool fast = false)
	{
		if (this._tweener != null && !fast)
		{
			this._tweener.Hide(true);
		}
		else
		{
			base.gameObject.SetActive(false);
		}
	}

	void Update()
	{
		if (!this.countdown)
		{
			return;
		}
		this.timer -= Time.deltaTime;
		if (this.timer < 0f)
		{
			this.countdown = false;
			this.Hide(false);
		}
	}
}
