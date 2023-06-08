using UnityEngine;

namespace StatsMonitor;

internal abstract class SMView
{
	internal GameObject gameObject;

	private RectTransform _rectTransform;

	protected StatsMonitorWidget _statsMonitorWidget;

	internal RectTransform RTransform
	{
		get
		{
			if (_rectTransform != null)
			{
				return _rectTransform;
			}
			_rectTransform = gameObject.GetComponent<RectTransform>();
			if (_rectTransform == null)
			{
				_rectTransform = gameObject.AddComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	internal float Width
	{
		get
		{
			return RTransform.rect.width;
		}
		set
		{
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
		}
	}

	internal float Height
	{
		get
		{
			return RTransform.rect.height;
		}
		set
		{
			RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
		}
	}

	internal float X
	{
		get
		{
			return RTransform.anchoredPosition.x;
		}
		set
		{
			RTransform.anchoredPosition = new Vector2(value, Y);
		}
	}

	internal float Y
	{
		get
		{
			return RTransform.anchoredPosition.y;
		}
		set
		{
			RTransform.anchoredPosition = new Vector2(X, value);
		}
	}

	internal Vector2 Pivot
	{
		get
		{
			return RTransform.pivot;
		}
		set
		{
			RTransform.pivot = value;
		}
	}

	internal Vector2 AnchorMin
	{
		get
		{
			return RTransform.anchorMin;
		}
		set
		{
			RTransform.anchorMin = value;
		}
	}

	internal Vector2 AnchorMax
	{
		get
		{
			return RTransform.anchorMax;
		}
		set
		{
			RTransform.anchorMax = value;
		}
	}

	internal void SetScale(float h = 1f, float v = 1f)
	{
		RTransform.localScale = new Vector3(h, v, 1f);
	}

	internal void SetRTransformValues(float x, float y, float width, float height, Vector2 pivotAndAnchor)
	{
		RTransform.anchoredPosition = new Vector2(x, y);
		RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
		RTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		Vector2 vector2 = (AnchorMax = pivotAndAnchor);
		Vector2 pivot = (AnchorMin = vector2);
		Pivot = pivot;
	}

	internal void Invalidate(SMViewInvalidationType type = SMViewInvalidationType.All)
	{
		if (gameObject == null)
		{
			gameObject = CreateChildren();
		}
		RTransform.anchoredPosition3D = new Vector3(RTransform.anchoredPosition.x, RTransform.anchoredPosition.y, 0f);
		SetScale();
		if (type == SMViewInvalidationType.Style || type == SMViewInvalidationType.All)
		{
			UpdateStyle();
		}
		if (type == SMViewInvalidationType.Layout || type == SMViewInvalidationType.All)
		{
			UpdateLayout();
		}
	}

	internal virtual void Reset()
	{
	}

	internal virtual void Update()
	{
	}

	internal virtual void Dispose()
	{
		Destroy(gameObject);
		gameObject = null;
	}

	internal static void Destroy(Object obj)
	{
		Object.Destroy(obj);
	}

	protected virtual GameObject CreateChildren()
	{
		return null;
	}

	protected virtual void UpdateStyle()
	{
	}

	protected virtual void UpdateLayout()
	{
	}
}
