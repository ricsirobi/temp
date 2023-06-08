using System.Collections.Generic;
using UnityEngine;

public class UiObjectiveEndMenuGrid : KAUIMenuGrid
{
	public float _CellYOffset = 50f;

	protected override void GridArrangement()
	{
		List<KAWidget> items = mMenu.GetItems();
		if (items == null || items.Count <= 0)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < items.Count; i++)
		{
			Transform transform = items[i].transform;
			if (NGUITools.GetActive(transform.gameObject) || !hideInactive)
			{
				float z = transform.localPosition.z;
				float x = transform.localPosition.x;
				transform.localPosition = new Vector3(x, 0f - num, z);
				num += _CellYOffset;
			}
		}
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(base.gameObject);
		if (uIScrollView != null)
		{
			uIScrollView.UpdateScrollbars(recalculateBounds: true);
		}
	}
}
