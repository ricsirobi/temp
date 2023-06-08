using System;

[Serializable]
public class EnhanceInfo
{
	public int _StoreId = 93;

	public int _ItemId;

	public TierMap[] _TierMap;

	public UserItemData pUserItemData { get; set; }

	public bool pEnhance { get; set; }

	public int pConsumeCount { get; set; }

	public string pCountText { get; set; }
}
