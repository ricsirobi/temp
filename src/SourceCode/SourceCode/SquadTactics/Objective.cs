using System;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class Objective
{
	public bool _IsMandatory;

	public ObjectiveType _Objective;

	public Character.Team _Team;

	public int _TurnLimit;

	public LocaleString _DescriptionText;

	[Header("Visit / Survive")]
	public Vector2 _Node;

	public List<int> _TeamSlot;

	[Header("Defeat")]
	public List<string> _UnitList;

	public bool _IsHiddenByParent;

	public bool _IsLockedByTurns;

	public int _TurnsToUnlock;

	[Header("Child objectives")]
	public List<Objective> _ChildObjectives;

	public int pObjectiveId { get; set; }

	public int pParentId { get; set; }

	public int pNoOfTurnsToUnlock { get; set; }

	public ObjectiveStatus pObjectiveStatus { get; set; }

	public ObjectiveHiddenStatus pHiddenStatus { get; set; }

	public GameObject pMarker { get; set; }

	public bool HasChildObjectives()
	{
		if (_ChildObjectives != null)
		{
			return _ChildObjectives.Count > 0;
		}
		return false;
	}

	public bool IsTurnsCompleted()
	{
		if (!_IsLockedByTurns)
		{
			return true;
		}
		return pNoOfTurnsToUnlock <= 0;
	}

	public bool SurviveFromStart()
	{
		if (_Objective == ObjectiveType.SURVIVE)
		{
			return _TurnLimit <= 0;
		}
		return false;
	}
}
