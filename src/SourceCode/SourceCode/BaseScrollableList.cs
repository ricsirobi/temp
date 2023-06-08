using UnityEngine;

public class BaseScrollableList : MonoBehaviour
{
	public GameObject itemPrefab;

	public int itemCount = 10;

	public int columnCount = 1;

	private void Start()
	{
		RectTransform component = itemPrefab.GetComponent<RectTransform>();
		RectTransform component2 = base.gameObject.GetComponent<RectTransform>();
		float num = component2.rect.width / (float)columnCount;
		float num2 = num / component.rect.width;
		float num3 = component.rect.height * num2;
		int num4 = itemCount / columnCount;
		if (itemCount % num4 > 0)
		{
			num4++;
		}
		float num5 = num3 * (float)num4;
		component2.offsetMin = new Vector2(component2.offsetMin.x, (0f - num5) / 2f);
		component2.offsetMax = new Vector2(component2.offsetMax.x, num5 / 2f);
		int num6 = 0;
		for (int i = 0; i < itemCount; i++)
		{
			if (i % columnCount == 0)
			{
				num6++;
			}
			GameObject gameObject = Object.Instantiate(itemPrefab);
			gameObject.name = base.gameObject.name + " item at (" + i + "," + num6 + ")";
			gameObject.transform.parent = base.gameObject.transform;
			RectTransform component3 = gameObject.GetComponent<RectTransform>();
			float x = (0f - component2.rect.width) / 2f + num * (float)(i % columnCount);
			float y = component2.rect.height / 2f - num3 * (float)num6;
			component3.offsetMin = new Vector2(x, y);
			x = component3.offsetMin.x + num;
			y = component3.offsetMin.y + num3;
			component3.offsetMax = new Vector2(x, y);
		}
	}
}
