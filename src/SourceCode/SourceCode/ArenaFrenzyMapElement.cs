using System;
using System.Collections.Generic;
using UnityEngine;

public class ArenaFrenzyMapElement : MonoBehaviour
{
	private class TargetRespawnInfo
	{
		public ArenaFrenzyTarget _Target;

		public float _DeactivationTime;

		public TargetRespawnInfo(ArenaFrenzyTarget inTarget, float inStartTime)
		{
			_Target = inTarget;
			_DeactivationTime = inStartTime;
		}
	}

	[Serializable]
	public class TargetInfo
	{
		public ArenaFrenzyTarget _Target;

		public int _AppearanceChance;
	}

	public TargetInfo[] _TargetData;

	public GameObject[] _TargetParents;

	public string _TargetMarkerPrefix = "PfMarker_Target";

	public MinMax _NumTargets;

	private ArenaFrenzyGame mGameManager;

	private List<GameObject> mTargets = new List<GameObject>();

	private List<TargetRespawnInfo> mTargetsForRespawn = new List<TargetRespawnInfo>();

	public List<GameObject> pTargets => mTargets;

	public int pFreeTargetSlots
	{
		get
		{
			if (mTargets == null)
			{
				return _TargetParents.Length;
			}
			return _TargetParents.Length - mTargets.Count;
		}
	}

	public virtual void Init(ArenaFrenzyGame inGameManager)
	{
		mGameManager = inGameManager;
	}

	public void Update()
	{
		if (mTargetsForRespawn == null || mTargetsForRespawn.Count <= 0)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		List<TargetRespawnInfo> respawnedTargets = new List<TargetRespawnInfo>();
		foreach (TargetRespawnInfo item in mTargetsForRespawn)
		{
			if (item._Target._RespawnTime > 0f && realtimeSinceStartup - item._DeactivationTime > item._Target._RespawnTime)
			{
				respawnedTargets.Add(item);
				RespawnTarget(item._Target);
			}
		}
		mTargetsForRespawn.RemoveAll((TargetRespawnInfo info) => respawnedTargets.Contains(info));
	}

	public void HandleTargetHit(ArenaFrenzyTarget inTarget, bool isLocal, UserProfileData inProfileData = null)
	{
		if (!(ArenaFrenzyGame.pInstance != null) || mTargetsForRespawn.Exists((TargetRespawnInfo info) => info._Target == inTarget))
		{
			return;
		}
		mTargetsForRespawn.Add(new TargetRespawnInfo(inTarget, Time.realtimeSinceStartup));
		if (ArenaFrenzyGame.pInstance != null)
		{
			if (inProfileData != null)
			{
				ArenaFrenzyGame.pInstance.OnTargetHit(inTarget, inProfileData.ID);
			}
			else
			{
				Debug.LogError("InProfile data is null");
			}
		}
	}

	public void HandleTargetHitMMO(ArenaFrenzyTarget inTarget, string userID)
	{
		if (ArenaFrenzyGame.pInstance != null)
		{
			if (!mTargetsForRespawn.Exists((TargetRespawnInfo info) => info._Target == inTarget))
			{
				mTargetsForRespawn.Add(new TargetRespawnInfo(inTarget, Time.realtimeSinceStartup));
			}
			inTarget.gameObject.SetActive(value: false);
			ArenaFrenzyGame.pInstance.AddScore(userID, inTarget);
		}
	}

	public ArenaFrenzyTarget GetTarget(int inTargetID)
	{
		foreach (GameObject mTarget in mTargets)
		{
			if (mTarget != null)
			{
				ArenaFrenzyTarget component = mTarget.GetComponent<ArenaFrenzyTarget>();
				if (component != null && component.pTargetID == inTargetID)
				{
					return component;
				}
			}
		}
		return null;
	}

	public void SetupTarget(ArenaFrenzyTarget.ArenaFrenzyTargetType inTargetType)
	{
		List<GameObject> list = new List<GameObject>();
		list.AddRange(_TargetParents);
		list.RemoveAll((GameObject ts) => ts.transform.childCount > 0);
		if (list.Count > 0)
		{
			Transform inParent = list[mGameManager.NextRandom() % list.Count].transform;
			mTargets.Add(SetupTarget(inTargetType, inParent));
		}
	}

	private GameObject SetupTarget(ArenaFrenzyTarget.ArenaFrenzyTargetType inTargetType, Transform inParent)
	{
		TargetInfo[] inTargets = Array.FindAll(_TargetData, (TargetInfo tData) => tData._Target._TargetType == inTargetType);
		ArenaFrenzyTarget arenaFrenzyTarget = SelectTarget(inTargets);
		GameObject gameObject = null;
		if (arenaFrenzyTarget != null)
		{
			gameObject = UnityEngine.Object.Instantiate(arenaFrenzyTarget.gameObject);
			gameObject.transform.parent = inParent;
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			ArenaFrenzyTarget component = gameObject.GetComponent<ArenaFrenzyTarget>();
			component.SetParentMapElement(this);
			component.pTargetID = ArenaFrenzyGame.pInstance.GetNextTargetID();
		}
		return gameObject;
	}

	private void RespawnTarget(ArenaFrenzyTarget inTarget)
	{
		inTarget.gameObject.SetActive(value: true);
	}

	private ArenaFrenzyTarget SelectTarget(TargetInfo[] inTargets)
	{
		ArenaFrenzyTarget result = null;
		if (inTargets != null && inTargets.Length != 0)
		{
			result = inTargets[0]._Target;
			if (inTargets.Length > 1)
			{
				int num = 0;
				TargetInfo[] array = inTargets;
				foreach (TargetInfo targetInfo in array)
				{
					num += targetInfo._AppearanceChance;
				}
				int num2 = mGameManager.NextRandom() % num;
				num = 0;
				array = inTargets;
				foreach (TargetInfo targetInfo2 in array)
				{
					num += targetInfo2._AppearanceChance;
					if (num > num2)
					{
						result = targetInfo2._Target;
						break;
					}
				}
			}
		}
		return result;
	}
}
