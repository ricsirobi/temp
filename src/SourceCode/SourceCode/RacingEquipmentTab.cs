using System;
using UnityEngine;

[Serializable]
public class RacingEquipmentTab
{
	public LocaleString _RollOverText;

	public AudioClip _RollOverVO;

	public int _Category;

	public string _WidgetName = "";

	public bool _IsDefault;

	[NonSerialized]
	public bool mIsSlotOpen;
}
