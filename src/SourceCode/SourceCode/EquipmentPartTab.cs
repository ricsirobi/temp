using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentPartTab
{
	public string _BtnName = string.Empty;

	public string _CatName = string.Empty;

	public Texture _Icon;

	public int _CategoryID;

	public KATooltipInfo _ToolTipInfo;

	public List<int> _ChildCategoryIDs;
}
