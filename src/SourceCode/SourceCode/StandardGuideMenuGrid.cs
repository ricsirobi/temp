using System.Collections.Generic;
using UnityEngine;

public class StandardGuideMenuGrid : KAUIMenuGrid
{
	public float _LineOffset = 3f;

	public string _EndLineWidgetName;

	protected override void GridArrangement()
	{
		List<KAWidget> items = mMenu.GetItems();
		if (items == null || items.Count <= 0)
		{
			return;
		}
		float num = items[0].transform.position.y;
		foreach (KAWidget item in items)
		{
			Vector3 position = item.transform.position;
			item.transform.position = new Vector3(position.x, num, position.z);
			num += 0f - (NGUIMath.CalculateRelativeWidgetBounds(item.transform).size.y + _LineOffset);
			if (!string.IsNullOrEmpty(_EndLineWidgetName))
			{
				KAWidget kAWidget = item.FindChildItem(_EndLineWidgetName);
				if (kAWidget != null)
				{
					Vector3 position2 = kAWidget.transform.position;
					Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(kAWidget.transform);
					kAWidget.transform.position = new Vector3(position2.x, num + _LineOffset + bounds.size.y, position2.z);
				}
			}
		}
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(base.gameObject);
		if (uIScrollView != null)
		{
			uIScrollView.UpdateScrollbars(recalculateBounds: true);
		}
	}

	public void ArrangeChildWidgets(KAWidget parentWidget, float yOffset)
	{
		if (!(parentWidget != null) || parentWidget.pChildWidgets == null)
		{
			return;
		}
		Vector3 position = parentWidget.transform.position;
		float num = position.y;
		foreach (KAWidget pChildWidget in parentWidget.pChildWidgets)
		{
			pChildWidget.transform.position = new Vector3(position.x, num, position.z);
			num += 0f - (NGUIMath.CalculateRelativeWidgetBounds(pChildWidget.transform).size.y + yOffset);
		}
	}

	public void CenterAlignWithParent(KAWidget parentWidget, KAWidget childWidget, bool vertical = true)
	{
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(parentWidget.transform);
		Vector3 vector = parentWidget.transform.position + bounds.center;
		Vector3 position = childWidget.transform.position;
		childWidget.transform.position = new Vector3(position.x, vector.y, position.z);
	}
}
