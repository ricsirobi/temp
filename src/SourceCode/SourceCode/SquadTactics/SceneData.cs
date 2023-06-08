using System;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class SceneData
{
	[Header("Spawns")]
	public List<UnitSpawn> _PlayerUnitSpawns;

	public List<UnitSpawn> _EnemyUnitSpawns;

	public List<UnitSpawn> _InanimateUnitSpawns;

	public MinMax _MaxEnemySpawns;

	public MinMax _MaxInanimateSpawns;

	public List<EnemySpawnData> _EnemySpawnData;

	public List<InanimateSpawnData> _InanimateSpawnData;

	[Header("Objectives")]
	public MinMax _MinMaxOptionalObjectives;

	public List<Objective> _Objectives;

	private List<Objective> mAllObjectives = new List<Objective>();

	private List<UnitSpawn> mEnemyUnitSpawns;

	private List<UnitSpawn> mInanimateUnitSpawns;

	private List<Objective> mFinalObjectiveList;

	private int mObjectiveIdTagger = 1;

	public List<Objective> pHeaderObjectiveList => _Objectives;

	public List<Objective> pFinalObjectiveList => mFinalObjectiveList;

	public List<UnitSpawn> GetEnemySpawns()
	{
		return mEnemyUnitSpawns;
	}

	public List<UnitSpawn> GetInanimateSpawns()
	{
		return mInanimateUnitSpawns;
	}

	public void Init()
	{
		mEnemyUnitSpawns = new List<UnitSpawn>();
		int randomInt = _MaxEnemySpawns.GetRandomInt();
		int num = 0;
		for (int i = 0; i < _EnemyUnitSpawns.Count; i++)
		{
			if (_EnemyUnitSpawns[i]._UnitName != "")
			{
				mEnemyUnitSpawns.Add(_EnemyUnitSpawns[i]);
			}
			else if (num < randomInt)
			{
				EnemySpawnData enemySpawnData = _EnemySpawnData[GetRandNumber(_EnemySpawnData.Count)];
				_EnemyUnitSpawns[i]._UnitName = enemySpawnData._UnitName;
				if (enemySpawnData._Variant != string.Empty)
				{
					_EnemyUnitSpawns[i]._Variant = enemySpawnData._Variant;
				}
				_EnemyUnitSpawns[i]._Level = enemySpawnData._Level;
				num++;
				mEnemyUnitSpawns.Add(_EnemyUnitSpawns[i]);
			}
		}
		randomInt = _MaxInanimateSpawns.GetRandomInt();
		num = 0;
		mInanimateUnitSpawns = new List<UnitSpawn>();
		for (int j = 0; j < _InanimateUnitSpawns.Count; j++)
		{
			if (_InanimateUnitSpawns[j]._UnitName != "")
			{
				mInanimateUnitSpawns.Add(_InanimateUnitSpawns[j]);
			}
			else if (num < randomInt)
			{
				InanimateSpawnData inanimateSpawnData = _InanimateSpawnData[GetRandNumber(_InanimateSpawnData.Count)];
				_InanimateUnitSpawns[j]._UnitName = inanimateSpawnData._UnitName;
				_InanimateUnitSpawns[j]._Level = inanimateSpawnData._Level;
				num++;
				mInanimateUnitSpawns.Add(_InanimateUnitSpawns[j]);
			}
		}
		mFinalObjectiveList = new List<Objective>();
		List<Objective> list = new List<Objective>();
		mObjectiveIdTagger = 1;
		AddObjectivesToList(_Objectives, 0);
		if (mAllObjectives == null || mAllObjectives.Count < 1)
		{
			return;
		}
		foreach (Objective mAllObjective in mAllObjectives)
		{
			if (mAllObjective.SurviveFromStart())
			{
				mAllObjective.pObjectiveStatus = ObjectiveStatus.COMPLETED;
			}
			if (mAllObjective._IsMandatory)
			{
				mFinalObjectiveList.Add(mAllObjective);
			}
			else
			{
				list.Add(mAllObjective);
			}
		}
		int num2 = list.Count - _MinMaxOptionalObjectives.GetRandomInt();
		for (int k = 0; k < num2; k++)
		{
			list.Remove(list[GetRandNumber(list.Count)]);
		}
		mFinalObjectiveList.AddRange(list);
	}

	public void AddObjectivesToList(List<Objective> objectives, int parentId)
	{
		if (objectives == null || objectives.Count < 1)
		{
			return;
		}
		if (mAllObjectives == null)
		{
			mAllObjectives = new List<Objective>();
		}
		foreach (Objective objective in objectives)
		{
			objective.pObjectiveId = mObjectiveIdTagger;
			mObjectiveIdTagger++;
			objective.pParentId = parentId;
			objective.pNoOfTurnsToUnlock = objective._TurnsToUnlock;
			objective.pHiddenStatus = (objective._IsHiddenByParent ? ObjectiveHiddenStatus.HIDDEN_BY_PARENT : (objective._IsLockedByTurns ? ObjectiveHiddenStatus.HIDDEN_BY_TURNS : ObjectiveHiddenStatus.UNHIDDEN));
			mAllObjectives.Add(objective);
			AddObjectivesToList(objective._ChildObjectives, objective.pObjectiveId);
		}
	}

	public int GetMandatoryObjectivesCount()
	{
		return mFinalObjectiveList.FindAll((Objective obj) => obj._IsMandatory).Count;
	}

	public string GetFinalObjectiveStr(Objective ob)
	{
		List<string> list = new List<string>();
		foreach (Character pAllPlayerUnit in GameManager.pInstance.pAllPlayerUnits)
		{
			if (pAllPlayerUnit.pCharacterData.pRaisedPetID > 0)
			{
				RaisedPetData raisedPetData = null;
				raisedPetData = RaisedPetData.GetByID(pAllPlayerUnit.pCharacterData.pRaisedPetID);
				if (raisedPetData != null)
				{
					list.Add(raisedPetData.Name);
				}
				else
				{
					list.Add(pAllPlayerUnit.pCharacterData._DisplayNameText.GetLocalizedString());
				}
			}
			else
			{
				list.Add(pAllPlayerUnit.pCharacterData._DisplayNameText.GetLocalizedString());
			}
		}
		foreach (Objective mFinalObjective in mFinalObjectiveList)
		{
			if (ob != mFinalObjective)
			{
				continue;
			}
			string text = mFinalObjective._DescriptionText.GetLocalizedString();
			for (int i = 0; i < list.Count; i++)
			{
				text = ((!(list[i] == "Avatar") || !AvatarData.pIsReady) ? text.Replace("%" + (i + 1), list[i]) : text.Replace("%" + (i + 1), AvatarData.pInstance.DisplayName));
				if (mFinalObjective.pObjectiveStatus == ObjectiveStatus.FAILED)
				{
					return "[s]" + text;
				}
			}
			return text;
		}
		return string.Empty;
	}

	public List<string> GetMissedObjectives(bool won)
	{
		List<string> list = new List<string>();
		foreach (Character pAllPlayerUnit in GameManager.pInstance.pAllPlayerUnits)
		{
			if (pAllPlayerUnit.pCharacterData.pRaisedPetID > 0)
			{
				RaisedPetData raisedPetData = null;
				raisedPetData = RaisedPetData.GetByID(pAllPlayerUnit.pCharacterData.pRaisedPetID);
				if (raisedPetData != null)
				{
					list.Add(raisedPetData.Name);
				}
				else
				{
					list.Add(pAllPlayerUnit.pCharacterData._DisplayNameText.GetLocalizedString());
				}
			}
			else
			{
				list.Add(pAllPlayerUnit.pCharacterData._DisplayNameText.GetLocalizedString());
			}
		}
		List<string> list2 = new List<string>();
		foreach (Objective mFinalObjective in mFinalObjectiveList)
		{
			if ((won || mFinalObjective._IsMandatory) && (mFinalObjective.pObjectiveStatus == ObjectiveStatus.FAILED || mFinalObjective.pObjectiveStatus == ObjectiveStatus.INPROGRESS))
			{
				string text = mFinalObjective._DescriptionText.GetLocalizedString();
				for (int i = 0; i < list.Count; i++)
				{
					text = ((!(list[i] == "Avatar") || !AvatarData.pIsReady) ? text.Replace("%" + (i + 1), list[i]) : text.Replace("%" + (i + 1), AvatarData.pInstance.DisplayName));
				}
				list2.Add(text);
			}
		}
		return list2;
	}

	public List<UnitSpawn> GetPlayerSpawns(int maxSpawns)
	{
		List<UnitSpawn> list = new List<UnitSpawn>();
		int num = 0;
		for (int i = 0; i < _PlayerUnitSpawns.Count; i++)
		{
			if (_PlayerUnitSpawns[i]._UnitName != "")
			{
				list.Add(_PlayerUnitSpawns[i]);
			}
			else if (num < maxSpawns)
			{
				num++;
				list.Add(_PlayerUnitSpawns[i]);
			}
		}
		return list;
	}

	private int GetRandNumber(int count)
	{
		return UnityEngine.Random.Range(0, count);
	}
}
