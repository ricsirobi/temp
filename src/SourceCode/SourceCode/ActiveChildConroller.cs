using UnityEngine;
using UnityEngine.UI;

public class ActiveChildConroller : MonoBehaviour
{
	private int count;

	private bool isActive = true;

	public void ActivateOne()
	{
		count++;
		if (!isActive)
		{
			GetComponent<LayoutElement>().gameObject.SetActive(value: true);
			isActive = true;
		}
	}

	public void DeactivateOne()
	{
		count--;
		if (count <= 0 && isActive)
		{
			GetComponent<LayoutElement>().gameObject.SetActive(value: false);
			isActive = false;
		}
	}
}
