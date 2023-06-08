using System.Collections.Generic;
using UnityEngine;

namespace Xsolla;

public class LinearLayout : MonoBehaviour
{
	public List<GameObject> objects;

	private float totalHeight;

	private float containerFinalHeight;

	private float parentHeight;

	private RectTransform parentRectTransform;

	private RectTransform containerRectTransform;

	public override string ToString()
	{
		return $"[LinearLayout: objects={objects}, totalHeight={totalHeight}, containerFinalHeight={containerFinalHeight}, parentHeight={parentHeight}, parentRectTransform={parentRectTransform}, containerRectTransform={containerRectTransform}]";
	}

	public void ReplaceObject(int position, GameObject gameObject)
	{
		objects[position] = gameObject;
	}

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
		parentRectTransform = base.transform.parent.gameObject.GetComponent<RectTransform>();
		parentHeight = parentRectTransform.rect.height;
		GetTotalHeight();
		DrawLayout();
	}

	private float GetTotalHeight()
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

	public void DrawLayout()
	{
		float num = 0f;
		foreach (GameObject @object in objects)
		{
			DrawObject(@object);
			num += @object.transform.localScale.y;
		}
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
}
