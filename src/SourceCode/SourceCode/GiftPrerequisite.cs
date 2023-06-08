using System;
using UnityEngine;

[Serializable]
public class GiftPrerequisite
{
	[Tooltip("Date range in CSV[from(Inclusive), to(Inclusive)], MM/DD/YYYY (UTC)Input for gender Male = 1, Female = 2 and Unknown = 0")]
	public GiftPrerequisiteType Type;

	[Tooltip("Mandatory Parameter. Default value = string.Empty")]
	public string Value;

	[Tooltip("Used only for Item type. Optional parameter. Default value = 1")]
	public short Quantity;

	public GiftPrerequisite()
	{
		Type = GiftPrerequisiteType.DateRange;
		Value = string.Empty;
		Quantity = 1;
	}
}
