using System;
using UnityEngine;

[Serializable]
public class InventoryTab
{
	public LocaleString _DisplayNameText;

	public LocaleString _RollOverText;

	public InventoryTabType _Type;

	public AudioClip _RollOverVO;

	public Texture _IconTex;

	public bool _AllowTrash;

	[Header("Tab ID from InventoryTabSetting Asset")]
	public string _TabID;

	[NonSerialized]
	public int mNumSlotOccupied;

	[NonSerialized]
	public int mNumSlotUnlocked;

	public InventorySetting.TabData pTabData { get; set; }

	public void UpdateFromInventorySetting()
	{
		if (!string.IsNullOrEmpty(_TabID) && InventorySetting.pInstance != null)
		{
			InventorySetting.TabData tabData = InventorySetting.pInstance.GetTabData(_TabID);
			if (tabData != null)
			{
				pTabData = tabData;
			}
		}
	}
}
