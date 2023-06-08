using System.Collections.Generic;
using UnityEngine;

namespace Xsolla;

public class GridView : MonoBehaviour
{
	public int itemCount;

	public int columnCount;

	public IBaseAdapter adapter;

	public void SetAdapter(IBaseAdapter adapter, int columnCount)
	{
		if (!(this == null))
		{
			this.adapter = adapter;
			itemCount = adapter.GetCount();
			this.columnCount = columnCount;
			Draw();
		}
	}

	public void Draw()
	{
		if (this == null)
		{
			return;
		}
		Clear();
		RectTransform component = adapter.GetPrefab().GetComponent<RectTransform>();
		RectTransform component2 = base.gameObject.GetComponent<RectTransform>();
		float num = component2.rect.width / (float)columnCount;
		float num2 = num / component.rect.width;
		float num3 = component.rect.height * num2;
		int num4 = itemCount / columnCount;
		if (num4 == 0)
		{
			num4 = 1;
		}
		else if (itemCount % num4 > 0)
		{
			num4++;
		}
		float num5 = num3 * (float)num4;
		component2.offsetMin = new Vector2(component2.offsetMin.x, component2.offsetMax.y - num5);
		component2.offsetMax = new Vector2(component2.offsetMax.x, component2.offsetMax.y);
		int num6 = 0;
		for (int i = 0; i < itemCount; i++)
		{
			if (i % columnCount == 0)
			{
				num6++;
			}
			GameObject view = adapter.GetView(i);
			view.transform.SetParent(base.gameObject.transform);
			RectTransform component3 = view.GetComponent<RectTransform>();
			float x = (0f - component2.rect.width) / 2f + num * (float)(i % columnCount);
			float y = component2.rect.height / 2f - num3 * (float)num6;
			component3.offsetMin = new Vector2(x, y);
			x = component3.offsetMin.x + num;
			y = component3.offsetMin.y + num3;
			component3.offsetMax = new Vector2(x, y);
		}
	}

	private void Clear()
	{
		List<GameObject> list = new List<GameObject>();
		if (this == null)
		{
			return;
		}
		for (int i = 0; i < base.transform.childCount; i++)
		{
			list.Add(base.transform.GetChild(i).gameObject);
		}
		foreach (GameObject item in list)
		{
			if (item != null)
			{
				Object.Destroy(item);
			}
		}
	}
}
