using UnityEngine;
using UnityEngine.UI;

public class ShopItemToggleController : MonoBehaviour
{
	public Toggle toggle;

	private void Update()
	{
		if (toggle != null && toggle.isOn && Input.GetButtonDown("Fire1"))
		{
			toggle.isOn = false;
		}
	}
}
