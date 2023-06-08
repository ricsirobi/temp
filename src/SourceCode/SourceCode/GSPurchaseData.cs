using System;
using UnityEngine;

[Serializable]
public class GSPurchaseData
{
	public GSPurchaseType _Type;

	public int _ItemID;

	public int _StoreID;

	public string _YesMessage;

	public string _NoMessage;

	public string _OnPurchaseCompleteMessage;

	public LocaleString _PruchaseText;

	public LocaleString _SuccessfulText;

	public LocaleString _FailText;

	[HideInInspector]
	public int mGemCost;
}
