using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class KAUIMenuGrid : UITable
{
	public bool _IsGrid = true;

	public bool _ArrangeItemsPagewise;

	public UIGrid.Arrangement arrangement;

	public int maxPerLine;

	public float cellWidth = 200f;

	public float cellHeight = 200f;

	[NonSerialized]
	public Vector2 pDragDelta = Vector2.zero;

	protected KAUIMenu mMenu;

	public bool _AlignBottom;

	public bool _DynamicSpacing;

	private void Awake()
	{
		mMenu = NGUITools.FindInParents<KAUIMenu>(base.gameObject);
	}

	protected override void Start()
	{
		Init();
	}

	public int GetPageCount(UIPanel panel)
	{
		float z = panel.baseClipRegion.z;
		float w = panel.baseClipRegion.w;
		float num = 1f;
		num = ((arrangement != UIGrid.Arrangement.Vertical) ? (w / cellHeight) : (z / cellWidth));
		float f = (float)((maxPerLine == 0) ? 1 : maxPerLine) * num;
		f = Mathf.CeilToInt(f);
		int num2 = base.transform.childCount;
		if (hideInactive)
		{
			num2 = 0;
			for (int i = 0; i < base.transform.childCount; i++)
			{
				if (NGUITools.GetActive(base.transform.GetChild(i).gameObject))
				{
					num2++;
				}
			}
		}
		int num3 = Mathf.CeilToInt((float)num2 / f);
		if (num3 <= 0)
		{
			num3 = 1;
		}
		return num3;
	}

	private void OnEnable()
	{
		base.repositionNow = true;
	}

	public virtual void UpdateItemColliders()
	{
		if (mMenu == null || mPanel == null || base.transform.childCount == 0)
		{
			return;
		}
		Vector3[] worldCorners = mPanel.worldCorners;
		int itemCount = mMenu.GetItemCount();
		for (int i = 0; i < itemCount; i++)
		{
			KAWidget itemAt = mMenu.GetItemAt(i);
			Transform transform = itemAt.transform;
			bool clipped = false;
			if (transform.position.x < worldCorners[0].x)
			{
				clipped = true;
			}
			else if (transform.position.x > worldCorners[2].x)
			{
				clipped = true;
			}
			else if (transform.position.y > worldCorners[2].y)
			{
				clipped = true;
			}
			else if (transform.position.y < worldCorners[0].y)
			{
				clipped = true;
			}
			SetWidgetClip(itemAt, clipped);
		}
	}

	private void SetWidgetClip(KAWidget widget, bool clipped)
	{
		widget.SetClipped(clipped);
		List<KAWidget> pChildWidgets = widget.pChildWidgets;
		if (pChildWidgets == null)
		{
			return;
		}
		foreach (KAWidget item in pChildWidgets)
		{
			SetWidgetClip(item, clipped);
		}
	}

	protected virtual void GridArrangement()
	{
		int num = 0;
		int num2 = 0;
		List<Transform> list = new List<Transform>();
		if (Application.isEditor && !Application.isPlaying)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				list.Add(base.transform.GetChild(i));
			}
		}
		else
		{
			list = mMenu.GetTransforms(this);
		}
		if (sorting != 0)
		{
			if (sorting == Sorting.Alphabetic)
			{
				list.Sort(UIGrid.SortByName);
			}
			else if (sorting == Sorting.Horizontal)
			{
				list.Sort(UIGrid.SortHorizontal);
			}
			else if (sorting == Sorting.Vertical)
			{
				list.Sort(UIGrid.SortVertical);
			}
			else if (onCustomSort != null)
			{
				list.Sort(onCustomSort);
			}
			else
			{
				Sort(list);
			}
		}
		int num3 = 1;
		if (_AlignBottom)
		{
			num3 = -1;
		}
		if (_ArrangeItemsPagewise)
		{
			float num4 = 0f;
			float num5 = 0f;
			float z = mPanel.baseClipRegion.z;
			float w = mPanel.baseClipRegion.w;
			int j = 0;
			for (int count = list.Count; j < count; j++)
			{
				Transform transform = list[j];
				if (!NGUITools.GetActive(transform.gameObject) && hideInactive)
				{
					continue;
				}
				float z2 = transform.localPosition.z;
				Vector3 localPosition = ((arrangement == UIGrid.Arrangement.Horizontal) ? new Vector3(cellWidth * (float)num + num4, (float)num3 * ((0f - cellHeight) * (float)num2), z2) : new Vector3(cellWidth * (float)num2, (float)num3 * ((0f - cellHeight) * (float)num - num4), z2));
				transform.localPosition = localPosition;
				if (++num >= maxPerLine && maxPerLine > 0)
				{
					num = 0;
					num2++;
					num5 += ((arrangement == UIGrid.Arrangement.Horizontal) ? cellHeight : cellWidth);
					if (num5 >= ((arrangement == UIGrid.Arrangement.Horizontal) ? w : z))
					{
						num5 = 0f;
						num2 = 0;
						num4 += ((arrangement == UIGrid.Arrangement.Horizontal) ? ((float)maxPerLine * cellWidth) : ((float)maxPerLine * cellHeight));
					}
				}
			}
			return;
		}
		float num6 = 0f;
		int k = 0;
		for (int count2 = list.Count; k < count2; k++)
		{
			Transform transform2 = list[k];
			if (NGUITools.GetActive(transform2.gameObject) || !hideInactive)
			{
				float z3 = transform2.localPosition.z;
				if (maxPerLine == 1 && _DynamicSpacing && transform2.GetComponent<KAWidget>().GetVisibility())
				{
					transform2.localPosition = ((arrangement == UIGrid.Arrangement.Horizontal) ? new Vector3(cellWidth * (float)num, _DynamicSpacing ? num6 : ((float)num3 * ((0f - cellHeight) * (float)num2)), z3) : new Vector3(_DynamicSpacing ? num6 : (cellWidth * (float)num2), (float)num3 * ((0f - cellHeight) * (float)num), z3));
					Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(transform2);
					num6 += ((arrangement == UIGrid.Arrangement.Horizontal) ? ((float)(-num3) * ((bounds.size.y == float.PositiveInfinity) ? 0f : bounds.size.y)) : ((bounds.size.x == float.PositiveInfinity) ? 0f : bounds.size.x));
				}
				else
				{
					transform2.localPosition = ((arrangement == UIGrid.Arrangement.Horizontal) ? new Vector3(cellWidth * (float)num, (float)num3 * ((0f - cellHeight) * (float)num2), z3) : new Vector3(cellWidth * (float)num2, (float)num3 * ((0f - cellHeight) * (float)num), z3));
				}
				if (++num >= maxPerLine && maxPerLine > 0)
				{
					num = 0;
					num2++;
				}
			}
		}
	}

	[ContextMenu("Execute")]
	public override void Reposition()
	{
		mReposition = false;
		if (_IsGrid)
		{
			GridArrangement();
		}
		else
		{
			List<Transform> transforms = mMenu.GetTransforms(this);
			if (transforms.Count > 0)
			{
				RepositionVariableSize(transforms);
			}
		}
		if (keepWithinPanel && mPanel != null)
		{
			mPanel.ConstrainTargetToBounds(base.transform, immediate: true);
			UIScrollView component = mPanel.GetComponent<UIScrollView>();
			if (component != null)
			{
				component.UpdateScrollbars(recalculateBounds: true);
			}
		}
		if (onReposition != null)
		{
			onReposition();
		}
		UpdateItemColliders();
	}
}
