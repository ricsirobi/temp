using System.Collections.Generic;
using UnityEngine;

namespace Xsolla;

public class SaveFileScript : MonoBehaviour
{
	private void Start()
	{
		XsollaPaymentImpl xsollaPaymentImpl = base.gameObject.AddComponent<XsollaPaymentImpl>();
		Dictionary<string, object> dictionary = TransactionHelper.LoadRequest();
		if (dictionary != null)
		{
			xsollaPaymentImpl.GetStatus(dictionary);
		}
		else
		{
			Debug.Log("Have no Unfinished requests");
		}
	}
}
