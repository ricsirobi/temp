using UnityEngine;

public class KAUIDraggablePanel : UIScrollView
{
	private KAUIMenu mMenu;

	private Vector2 mCachedDragAmount;

	private KAUIMenuGrid mMenuGridRef;

	public override bool shouldMoveVertically
	{
		get
		{
			if (mPanel == null)
			{
				mPanel = GetComponent<UIPanel>();
			}
			return base.shouldMoveVertically;
		}
	}

	public override Bounds bounds
	{
		get
		{
			if (mMenu != null)
			{
				if (mMenu.pIsOptimized)
				{
					return mMenu._OptimizedMenu.pMenuBounds;
				}
				if (mMenuGridRef != mMenu.pMenuGrid || !mCalculatedBounds)
				{
					mMenuGridRef = mMenu.pMenuGrid;
					mBounds = NGUIMath.CalculateRelativeWidgetBounds(mMenu.pDragPanel.transform, mMenu.pMenuGrid.transform);
					mCalculatedBounds = true;
				}
			}
			return base.bounds;
		}
	}

	public void Init(KAUIMenu menu)
	{
		mMenu = menu;
	}

	public override void SetDragAmount(float x, float y, bool updateScrollbars)
	{
		base.SetDragAmount(x, y, updateScrollbars);
		if (mMenu != null)
		{
			Vector2 vector = new Vector2(x, y);
			if (mMenu.pIsOptimized && vector != mCachedDragAmount)
			{
				mMenu._OptimizedMenu.SnapTo(vector);
				mCachedDragAmount = vector;
			}
			mMenu.pRecenterItem = true;
		}
	}

	public Vector2 GetScrollValue()
	{
		UIPanel component = GetComponent<UIPanel>();
		float t = ((horizontalScrollBar != null) ? horizontalScrollBar.value : 0f);
		float t2 = ((verticalScrollBar != null) ? verticalScrollBar.value : 0f);
		Bounds bounds = this.bounds;
		if (bounds.min.x == bounds.max.x || bounds.min.y == bounds.max.x)
		{
			return Vector2.zero;
		}
		Vector4 baseClipRegion = component.baseClipRegion;
		float num = baseClipRegion.z * 0.5f;
		float num2 = baseClipRegion.w * 0.5f;
		float num3 = bounds.min.x + num;
		float num4 = bounds.max.x - num;
		float num5 = bounds.min.y + num2;
		float num6 = bounds.max.y - num2;
		if (component.clipping == UIDrawCall.Clipping.SoftClip)
		{
			num3 -= component.clipSoftness.x;
			num4 += component.clipSoftness.x;
			num5 -= component.clipSoftness.y;
			num6 += component.clipSoftness.y;
		}
		float num7 = Mathf.Lerp(num3, num4, t);
		float num8 = Mathf.Lerp(num6, num5, t2);
		float x = num7 - num3;
		float y = num6 - num8;
		return new Vector2(x, y);
	}
}
