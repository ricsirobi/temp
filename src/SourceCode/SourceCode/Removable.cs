using System;
using System.Collections.Generic;
using UnityEngine;

public class Removable : FarmItemBase
{
	public FarmItemStage[] _Stages;

	private FarmItemStage mCurrentStage;

	private RemovableInfo mInfo;

	private float mDestroyTimer;

	private bool mIsDestroyTimerStarted;

	private bool mIsDestroyed;

	private DateTime mDestroyStartTime;

	public FarmItemStage pCurrentStage
	{
		get
		{
			return mCurrentStage;
		}
		set
		{
			mCurrentStage = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (_Stages != null && _Stages.Length != 0)
		{
			mCurrentStage = _Stages[0];
		}
		SetState();
	}

	public void SetInfo(RemovableInfo inRemovableInfo, FarmManager inFarmManager)
	{
		base.pFarmManager = inFarmManager;
		mInfo = inRemovableInfo;
		base.transform.position = inRemovableInfo._Position;
		base.transform.Rotate(Vector3.up, inRemovableInfo.yRotation);
		Deserialize();
	}

	private void SetState()
	{
		if (!mDestroyStartTime.Equals(DateTime.MinValue))
		{
			if ((ServerTime.pCurrentTime - mDestroyStartTime.AddSeconds(mInfo._DestroyInSecs)).TotalSeconds > 0.0)
			{
				HideObject();
			}
			else
			{
				base.pFarmManager._GridManager.AddItem(base.gameObject, null, inUnlockGridCells: false);
			}
			mDestroyStartTime.AddSeconds(0f - mInfo._DestroyInSecs);
		}
		else
		{
			base.pFarmManager._GridManager.AddItem(base.gameObject, null, inUnlockGridCells: false);
		}
	}

	protected override void OnContextAction(string inActionName)
	{
		if (inActionName == "Destroy")
		{
			RemoveObject();
		}
	}

	public void RemoveObject()
	{
		if (!mIsDestroyTimerStarted && Money.pGameCurrency > mInfo._CostToRemove && UserRankData.pInstance.RankID >= mInfo._RequiredFarmHouseLevel)
		{
			base.pCanShowContextMenu = false;
			Money.AddMoney(-mInfo._CostToRemove, bForceUpdate: true);
			mIsDestroyTimerStarted = true;
			mDestroyStartTime = ServerTime.pCurrentTime;
			mDestroyTimer = mInfo._DestroyInSecs;
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		if (mIsDestroyTimerStarted && !mIsDestroyed)
		{
			base.pShowStatus = true;
			Invoke("HideStatus", _StatusInterval);
		}
	}

	protected override void HideStatus()
	{
		base.pShowStatus = false;
	}

	protected override void Update()
	{
		base.Update();
		if (mIsDestroyTimerStarted && !mIsDestroyed)
		{
			mDestroyTimer -= Time.deltaTime;
			ContextData contextData = GetContextData("Status");
			if (contextData != null)
			{
				contextData._DisplayName._Text = UtUtilities.GetTimerString((int)mDestroyTimer);
			}
			Refresh();
			if (mDestroyTimer <= 0f)
			{
				base.pCanShowContextMenu = false;
				mIsDestroyed = true;
				HideObject();
			}
		}
	}

	private void HideObject()
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		base.gameObject.GetComponent<Collider>().enabled = false;
		DestroyMenu(checkProximity: false);
		Invoke("DestroyCSMMenu", 1f);
	}

	protected void DestroyCSMMenu()
	{
		DestroyMenu(checkProximity: false);
	}

	public string Serialize()
	{
		int num = 1;
		if (mInfo._Type == RemovableType.ROCK)
		{
			num = 2;
		}
		_ = string.Empty;
		return mInfo._ID + ";" + num + ";" + base.transform.position.x + ";" + base.transform.position.y + ";" + base.transform.position.z + ";" + mDestroyStartTime.ToString(UtUtilities.GetCultureInfo("en-US"));
	}

	public void Deserialize()
	{
		string[] separator = new string[1] { ";" };
		if (mInfo.pDeserializeString != null)
		{
			string[] array = mInfo.pDeserializeString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			if (array != null && array.Length == 6)
			{
				mDestroyStartTime = DateTime.Parse(array[5], UtUtilities.GetCultureInfo("en-US"));
				SetState();
			}
		}
	}

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		if (base.pFarmManager != null && !mIsDestroyed)
		{
			List<ContextSensitiveState> farmItemContextData = pCurrentStage.GetFarmItemContextData(base.pFarmManager.pIsBuildMode);
			inStatesArrData = GetSensitiveData(farmItemContextData);
			UpdateScaleData(ref inStatesArrData);
		}
	}

	private void MakeSensitiveData(ContextSensitiveState contextData)
	{
		if (contextData._CurrentContextNamesList != null && contextData._CurrentContextNamesList.Length != 0)
		{
			List<string> list = new List<string>(contextData._CurrentContextNamesList);
			if (!base.pCanShowContextMenu && list.Contains("Destroy"))
			{
				list.Remove("Destroy");
			}
			contextData._CurrentContextNamesList = list.ToArray();
		}
	}

	protected override void ProcessSensitiveData(ref List<string> menuItemNames)
	{
		if (!base.pCanShowContextMenu && menuItemNames.Contains("Destroy"))
		{
			menuItemNames.Remove("Destroy");
		}
		if ((mDestroyTimer < 1f || mIsDestroyed || !mIsDestroyTimerStarted) && menuItemNames.Contains("Status"))
		{
			menuItemNames.Remove("Status");
		}
	}
}
