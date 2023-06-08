using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ExpansionMissionBoard;

[Serializable]
public class ExpansionMission
{
	[FormerlySerializedAs("_Name")]
	public LocaleString _NameText;

	[FormerlySerializedAs("_Description")]
	public LocaleString _DescriptionText;

	public List<ExpansionMissionData> _Missions;

	public int _StoreID;

	public int _TicketID;

	public List<int> _PaywallTaskIDs;

	public string _MissionIconURL;

	[HideInInspector]
	public State pState;

	[HideInInspector]
	public ExpansionMissionData pTargetMission;

	[HideInInspector]
	public int pTicketID;
}
