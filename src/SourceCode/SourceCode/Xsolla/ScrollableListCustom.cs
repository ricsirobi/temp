using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class ScrollableListCustom : MonoBehaviour
{
	public GameObject itemPrefab;

	private List<GameObject> items;

	public void SetData(Action<string> onItemClickAction, Dictionary<string, string> objects)
	{
		int count = objects.Count;
		int num = 1;
		items = new List<GameObject>(num);
		RectTransform component = itemPrefab.GetComponent<RectTransform>();
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
			GameObject gameObject = UnityEngine.Object.Instantiate(itemPrefab);
			gameObject.name = base.gameObject.name + " item at (" + i + "," + num7 + ")";
			gameObject.transform.SetParent(base.gameObject.transform);
			Text obj = gameObject.GetComponentsInChildren<Text>(includeInactive: true)[0];
			KeyValuePair<string, string> o = objects.ElementAt(i);
			obj.text = o.Value;
			if (onItemClickAction != null)
			{
				gameObject.GetComponents<Button>()[0].onClick.AddListener(delegate
				{
					onItemClickAction(o.Key);
				});
			}
			items.Add(gameObject);
			RectTransform component3 = gameObject.GetComponent<RectTransform>();
			float x = (0f - component2.rect.width) / 2f + num2 * (float)(i % num);
			float y = component2.rect.height / 2f - num4 * (float)num7;
			component3.offsetMin = new Vector2(x, y);
			x = component3.offsetMin.x + num2;
			y = component3.offsetMin.y + num4;
			component3.offsetMax = new Vector2(x, y);
		}
	}

	public List<GameObject> GetItems()
	{
		return items;
	}
}
