using System.Collections;
using UnityEngine;

public class HideMe : MonoBehaviour
{
	private void Update()
	{
		if (base.isActiveAndEnabled)
		{
			StartCoroutine(HideMePlease());
		}
	}

	private IEnumerator HideMePlease()
	{
		yield return new WaitForSeconds(2f);
		base.gameObject.SetActive(value: false);
	}
}
