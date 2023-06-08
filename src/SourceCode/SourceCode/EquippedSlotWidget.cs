using System;
using UnityEngine;

[Serializable]
public class EquippedSlotWidget
{
	public string _PartType;

	public Texture _DefaultTexture;

	public KAWidget _WidgetSlot;

	public ItemData pEquippedItemData { get; set; }

	public UserItemData pEquippedUserItemData { get; set; }

	public int pPartUiid { get; set; }
}
