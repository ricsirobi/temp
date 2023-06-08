using System.Collections.Generic;
using UnityEngine;

public class FieldGuideTopicMenuGrid : KAUIMenuGrid
{
	public float _CellYOffset = 20f;

	public float _CellXOffset = 20f;

	public float _TopicItemOffsetAbove = 10f;

	public float _TopicItemOffsetBelow = -10f;

	protected override void GridArrangement()
	{
		List<KAWidget> items = mMenu.GetItems();
		if (items == null || items.Count <= 0)
		{
			return;
		}
		float num = _CellYOffset;
		for (int i = 0; i < items.Count; i++)
		{
			Transform transform = items[i].transform;
			if (NGUITools.GetActive(transform.gameObject) || !hideInactive)
			{
				float z = transform.localPosition.z;
				float x = transform.localPosition.x;
				if (transform.name.Contains("_topic"))
				{
					num += _TopicItemOffsetAbove;
				}
				transform.localPosition = new Vector3(x, 0f - num, z);
				Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(transform);
				num = ((!transform.name.Contains("_topic")) ? (num + (_CellYOffset + bounds.size.y)) : (num + (_TopicItemOffsetBelow + bounds.size.y)));
			}
		}
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(base.gameObject);
		if (uIScrollView != null)
		{
			uIScrollView.UpdateScrollbars(recalculateBounds: true);
		}
	}
}
