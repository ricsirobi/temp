using System;
using UnityEngine;

public class OpenPaystation : MonoBehaviour
{
	public string token;

	public bool isSandbox;

	public event Action<string> TryOpenPaystation;

	public void InitPaystation()
	{
		if (token != null && !"".Equals(token))
		{
			string url = (isSandbox ? ("https://sandbox-secure.xsolla.com/paystation2/?access_token=" + token) : ("https://secure.xsolla.com/paystation2/?access_token=" + token));
			Application.OpenURL(url);
			OnOpenPaystation(url);
		}
	}

	private void OnOpenPaystation(string url)
	{
		if (this.TryOpenPaystation != null)
		{
			this.TryOpenPaystation(url);
		}
	}
}
