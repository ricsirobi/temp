using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class ListView : MonoBehaviour
{
	private IBaseAdapter adapter;

	public void ResizeToParent()
	{
		RectTransform component = GetComponent<RectTransform>();
		RectTransform component2 = base.transform.root.gameObject.GetComponent<RectTransform>();
		float height = component2.rect.height;
		float width = component2.rect.width;
		component.offsetMin = new Vector2((0f - width) / 2f, (0f - height) / 2f);
		component.offsetMax = new Vector2(width / 2f, height / 2f);
	}

	private void Clear()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in base.transform)
		{
			list.Add(item.gameObject);
		}
		list.ForEach(delegate(GameObject child)
		{
			Object.Destroy(child);
		});
	}

	public void SetAdapter(IBaseAdapter adapter)
	{
		this.adapter = adapter;
	}

	public void DrawList()
	{
		Clear();
		GameObject prefab = adapter.GetPrefab();
		int count = adapter.GetCount();
		if (count <= 0)
		{
			return;
		}
		int num = 1;
		RectTransform component = prefab.GetComponent<RectTransform>();
		RectTransform component2 = base.gameObject.GetComponent<RectTransform>();
		float num2 = component2.rect.width / (float)num;
		float num3 = num2 / component.rect.width;
		float num4 = component.rect.height * num3;
		int num5 = count / num;
		if (count % num5 > 0)
		{
			num5++;
		}
		float num6 = num4 * (float)num5;
		component2.offsetMin = new Vector2(component2.offsetMin.x, (0f - num6) / 2f);
		component2.offsetMax = new Vector2(component2.offsetMax.x, num6 / 2f);
		int num7 = 0;
		for (int i = 0; i < count; i++)
		{
			if (i % num == 0)
			{
				num7++;
			}
			GameObject view = adapter.GetView(i);
			view.transform.SetParent(base.gameObject.transform);
			RectTransform component3 = view.GetComponent<RectTransform>();
			float x = (0f - component2.rect.width) / 2f + num2 * (float)(i % num);
			float y = component2.rect.height / 2f - num4 * (float)num7;
			component3.offsetMin = new Vector2(x, y);
			x = component3.offsetMin.x + num2;
			y = component3.offsetMin.y + num4;
			component3.offsetMax = new Vector2(x, y);
		}
	}

	public void DrawList(RectTransform parentRectTransform)
	{
		Clear();
		LayoutElement component = GetComponent<LayoutElement>();
		GameObject prefab = adapter.GetPrefab();
		int count = adapter.GetCount();
		if (count <= 0)
		{
			return;
		}
		int num = 1;
		RectTransform component2 = prefab.GetComponent<RectTransform>();
		RectTransform component3 = base.gameObject.GetComponent<RectTransform>();
		float num2 = parentRectTransform.rect.width / (float)num;
		float num3 = num2 / component2.rect.width;
		float num4 = component2.rect.height * num3;
		int num5 = count / num;
		if (count % num5 > 0)
		{
			num5++;
		}
		float num6 = num4 * (float)num5;
		component3.offsetMin = new Vector2(parentRectTransform.offsetMin.x, (0f - num6) / 2f);
		component3.offsetMax = new Vector2(parentRectTransform.offsetMax.x, num6 / 2f);
		int num7 = 0;
		for (int i = 0; i < count; i++)
		{
			if (i % num == 0)
			{
				num7++;
			}
			GameObject view = adapter.GetView(i);
			view.transform.SetParent(base.gameObject.transform);
			component.minHeight += view.GetComponent<RectTransform>().rect.height;
			RectTransform component4 = view.GetComponent<RectTransform>();
			float x = (0f - parentRectTransform.rect.width) / 2f + num2 * (float)(i % num);
			float y = component3.rect.height / 2f - num4 * (float)num7;
			component4.offsetMin = new Vector2(x, y);
			x = component4.offsetMin.x + num2;
			y = component4.offsetMin.y + num4;
			component4.offsetMax = new Vector2(x, y);
		}
	}
}
