using UnityEngine;

public class ResizeToBiggestChild : MonoBehaviour
{
	private void Start()
	{
		RectTransform component = GetComponent<RectTransform>();
		RectTransform componentInChildren = GetComponentInChildren<RectTransform>();
		component.offsetMin = new Vector2(component.offsetMin.x, component.offsetMax.y - componentInChildren.rect.height);
		component.offsetMax = new Vector2(component.offsetMin.x, component.offsetMax.y);
	}

	private void Update()
	{
	}
}
