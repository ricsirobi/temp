using UnityEngine;

public class FullDeskController : MonoBehaviour
{
	public Transform parent;

	private bool isFirstTime = true;

	public bool getIsFirstTime()
	{
		return isFirstTime;
	}

	private void Start()
	{
	}

	public void Open(bool b)
	{
		if (b)
		{
			base.gameObject.transform.SetParent(GetComponentInParent<Canvas>().transform);
		}
		else
		{
			base.gameObject.transform.SetParent(parent);
			GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		}
		base.gameObject.SetActive(b);
	}
}
