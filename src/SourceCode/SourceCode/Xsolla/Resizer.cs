using System.Collections.Generic;
using UnityEngine;

namespace Xsolla;

public static class Resizer
{
	public static void ResizeToParrent(GameObject go)
	{
		RectTransform component = go.GetComponent<RectTransform>();
		RectTransform component2 = go.transform.parent.gameObject.GetComponent<RectTransform>();
		float height = component2.rect.height;
		float width = component2.rect.width;
		float num = width / height;
		float x = component2.localScale.x;
		float width2 = component.rect.width;
		if (num < 1f)
		{
			component.offsetMin = new Vector2((0f - width) / 2f, (0f - height) / 2f);
			component.offsetMax = new Vector2(width / 2f, height / 2f);
			return;
		}
		float num2 = width / 3f;
		if (width2 < num2)
		{
			component.offsetMin = new Vector2((0f - num2) / 2f, (0f - height) / 2f);
			component.offsetMax = new Vector2(num2 / 2f, height / 2f);
		}
		else
		{
			component.offsetMin = new Vector2((0f - width2) / 2f, (0f - height) / 2f / x);
			component.offsetMax = new Vector2(width2 / 2f, height / 2f / x);
		}
	}

	public static void DestroyChilds(Transform parentTransform)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in parentTransform)
		{
			list.Add(item.gameObject);
		}
		list.ForEach(delegate(GameObject child)
		{
			Object.Destroy(child);
		});
	}
}
