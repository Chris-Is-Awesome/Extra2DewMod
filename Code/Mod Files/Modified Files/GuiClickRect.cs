using System;
using UnityEngine;

public class GuiClickRect : MonoBehaviour
{
	[SerializeField]
	Vector2 _size = Vector2.one;

	[SerializeField]
	Vector2 _center = Vector2.zero;

	[SerializeField]
	float _zIndex;

	[SerializeField]
	bool _clip;

	GuiInteractionLayer interlayer;

	bool checkedLayer;

	GuiClickRect savedParent;

    //ADDED THROUGH METHOD INJECTION
    public void SetSizeAndCenter(Vector2 size, Vector2 center)
    {
        _size = size;
        _center = center;
    }

    GuiInteractionLayer RealLayer
	{
		get
		{
			if (!this.checkedLayer)
			{
				this.checkedLayer = true;
				this.interlayer = TransformUtility.FindInParents<GuiInteractionLayer>(base.transform);
			}
			return this.interlayer;
		}
	}

	bool LayerActive()
	{
		GuiInteractionLayer realLayer = this.RealLayer;
		return realLayer == null || realLayer.IsActive;
	}

    public bool IsActive
	{
		get
		{
			return base.gameObject.activeInHierarchy && this.LayerActive();
		}
	}

	GuiClickRect Parent
	{
		get
		{
			if (this.savedParent == null || !Application.isPlaying)
			{
				this.savedParent = TransformUtility.FindInParents<GuiClickRect>(base.transform.parent);
			}
			return this.savedParent;
		}
	}

	public Rect LocalRect
	{
		get
		{
			Vector2 size = this._size;
			Vector3 localScale = base.transform.localScale;
			size.x *= localScale.x;
			size.y *= localScale.y;
			Vector2 center = this._center;
			Vector3 localPosition = base.transform.localPosition;
			center.x += localPosition.x;
			center.y += localPosition.y;
			return new Rect(-size.x * 0.5f + center.x, -size.y * 0.5f + center.y, size.x, size.y);
		}
	}

	public Rect WorldRect
	{
		get
		{
			Vector2 size = this._size;
			Vector3 localScale = base.transform.localScale;
			Vector3 lossyScale = base.transform.lossyScale;
			size.x *= lossyScale.x;
			size.y *= lossyScale.y;
			Vector2 center = this._center;
			Vector3 position = base.transform.position;
			center.x = center.x * (lossyScale.x / localScale.x) + position.x;
			center.y = center.y * (lossyScale.y / localScale.y) + position.y;
			return new Rect(-size.x * 0.5f + center.x, -size.y * 0.5f + center.y, size.x, size.y);
		}
	}

	Rect Constrain(Rect R)
	{
		if (this._clip)
		{
			Rect worldRect = this.WorldRect;
			if (!R.Overlaps(worldRect))
			{
				return new Rect(0f, 0f, 0f, 0f);
			}
			R.xMin = Mathf.Max(worldRect.xMin, R.xMin);
			R.xMax = Mathf.Min(worldRect.xMax, R.xMax);
			R.yMin = Mathf.Max(worldRect.yMin, R.yMin);
			R.yMax = Mathf.Min(worldRect.yMax, R.yMax);
		}
		GuiClickRect parent = this.Parent;
		if (parent != null)
		{
			return parent.Constrain(R);
		}
		return R;
	}

	public Rect ActiveRect
	{
		get
		{
			GuiClickRect parent = this.Parent;
			if (parent != null)
			{
				return parent.Constrain(this.WorldRect);
			}
			return this.WorldRect;
		}
	}

	public bool Clip
	{
		get
		{
			return this._clip;
		}
	}

	public float ZIndex
	{
		get
		{
			GuiClickRect parent = this.Parent;
			if (parent != null)
			{
				return parent.ZIndex + 100f + this._zIndex;
			}
			return this._zIndex;
		}
	}

	public Rect GetActiveScreenRect(Camera cam)
	{
		return this.RectToScreen(this.ActiveRect, cam);
	}

	public Rect GetWorldScreenRect(Camera cam)
	{
		return this.RectToScreen(this.WorldRect, cam);
	}

	public Rect RectToScreen(Rect r, Camera cam)
	{
		Vector3 vector = cam.transform.TransformDirection(Vector3.right);
		Vector3 vector2 = cam.transform.TransformDirection(Vector3.up);
		Vector3 vector3;
		vector3..ctor(r.center.x, r.center.y, base.transform.position.z);
		Vector3 vector4 = vector * r.width * 0.5f;
		Vector3 vector5 = vector2 * r.height * 0.5f;
		Vector3 vector6 = cam.WorldToScreenPoint(vector3 - vector4 - vector5);
		Vector3 vector7 = cam.WorldToScreenPoint(vector3 + vector4 + vector5);
		Vector3 right = Vector3.right;
		Vector3 up = Vector3.up;
		Vector3 vector8 = (vector6 + vector7) * 0.5f;
		Vector3 vector9 = vector7 - vector6;
		float num = Vector3.Dot(vector8, right);
		float num2 = Vector3.Dot(vector8, up);
		float num3 = Vector3.Dot(vector9, right);
		float num4 = Vector3.Dot(vector9, up);
		return new Rect(-num3 * 0.5f + num, -num4 * 0.5f + num2, num3, num4);
	}
}
