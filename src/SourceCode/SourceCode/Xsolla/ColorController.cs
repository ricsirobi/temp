using UnityEngine;

namespace Xsolla;

public class ColorController : MonoBehaviour
{
	public Elem[] itemsToColor;

	public void ChangeColor(int elemNo, StyleManager.BaseColor color)
	{
		itemsToColor[elemNo].color = color;
		Elem[] array = itemsToColor;
		for (int i = 0; i < array.Length; i++)
		{
			Elem elem = array[i];
			if (elem.whatToColor != null)
			{
				elem.whatToColor.color = Singleton<StyleManager>.Instance.GetColor(elem.color);
			}
		}
	}

	private void Start()
	{
		Elem[] array = itemsToColor;
		for (int i = 0; i < array.Length; i++)
		{
			Elem elem = array[i];
			if (elem.whatToColor != null)
			{
				elem.whatToColor.color = Singleton<StyleManager>.Instance.GetColor(elem.color);
			}
		}
	}

	private void Update()
	{
	}
}
