using System;
using UnityEngine;

[Serializable]
public class GiftSetup
{
	public GiftType Type;

	[Tooltip("Mandatory Parameter for Item type to set ItemId. Default value = string.Empty")]
	public string Value;

	[Tooltip("Mandatory parameter for gems and coins. Default value = 1")]
	public short Quantity;

	public GiftSetup()
	{
		Type = GiftType.Item;
		Value = string.Empty;
		Quantity = 1;
	}
}
