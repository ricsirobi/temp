using UnityEngine;
using Xsolla;

public class Selfdestruction : MonoBehaviour
{
	public void DestroyRoot()
	{
		Object.Destroy(base.gameObject.GetComponentInParent<XsollaPaystationController>().gameObject);
		HttpTlsRequest[] array = (HttpTlsRequest[])Object.FindObjectsOfType(typeof(HttpTlsRequest));
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i].gameObject);
		}
		StyleManager[] array2 = (StyleManager[])Object.FindObjectsOfType(typeof(StyleManager));
		for (int i = 0; i < array2.Length; i++)
		{
			Object.Destroy(array2[i].gameObject);
		}
		TransactionHelper.Clear();
	}

	public void Selfdestroy()
	{
		Object.Destroy(base.gameObject);
	}

	public void DestroyObject(GameObject go)
	{
		Object.Destroy(go);
	}
}
