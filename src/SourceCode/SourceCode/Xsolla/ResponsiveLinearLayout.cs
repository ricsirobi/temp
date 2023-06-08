using System.Collections.Generic;
using UnityEngine;

namespace Xsolla;

public class ResponsiveLinearLayout : MonoBehaviour
{
	public List<GameObject> objects;

	private float parentHeight;

	private float totalHeight;

	private float containerFinalHeight;

	private RectTransform containerRectTransform;

	public void AddObject(GameObject go)
	{
		if (go != null)
		{
			objects.Add(go);
		}
	}

	public void Invalidate()
	{
		containerRectTransform = GetComponent<RectTransform>();
		parentHeight = GameObject.FindGameObjectWithTag("Container").GetComponent<RectTransform>().rect.height;
		GetTotlHeight();
		DrawLayout();
	}

	private float GetTotlHeight()
	{
		foreach (GameObject @object in objects)
		{
			RectTransform component = @object.GetComponent<RectTransform>();
			float num = containerRectTransform.rect.width / component.rect.width;
			float num2 = component.rect.height * num;
			containerFinalHeight += num2;
		}
		return containerFinalHeight;
	}

	private void DrawObject(GameObject go)
	{
		RectTransform component = go.GetComponent<RectTransform>();
		float width = containerRectTransform.rect.width;
		float num = width / component.rect.width;
		float num2 = component.rect.height * num;
		totalHeight += num2;
		go.transform.SetParent(base.gameObject.transform);
		RectTransform component2 = go.GetComponent<RectTransform>();
		float x = (0f - containerRectTransform.rect.width) / 2f;
		float y = containerFinalHeight / 2f - totalHeight;
		component2.offsetMin = new Vector2(x, y);
		x = component2.offsetMin.x + width;
		y = component2.offsetMin.y + num2;
		component2.offsetMax = new Vector2(x, y);
	}

	public void DrawLayout()
	{
		foreach (GameObject @object in objects)
		{
			DrawObject(@object);
		}
		if (totalHeight > parentHeight)
		{
			containerRectTransform.offsetMin = new Vector2(containerRectTransform.offsetMin.x, 0f - totalHeight + parentHeight / 2f);
			containerRectTransform.offsetMax = new Vector2(containerRectTransform.offsetMax.x, parentHeight / 2f);
		}
	}
}
