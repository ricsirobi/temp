using System;
using UnityEngine;

[Serializable]
public class QuestWidgets
{
	public KAToggleButton _ToggleWidget;

	[HideInInspector]
	public Mission pMission;

	[HideInInspector]
	public int pTaskId;
}
