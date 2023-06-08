using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HUDAction
{
	public string _UiObjectName;

	public GameObject _UiObject;

	public List<HUDActionItem> _Items;

	[NonSerialized]
	public KAUI pUI;
}
