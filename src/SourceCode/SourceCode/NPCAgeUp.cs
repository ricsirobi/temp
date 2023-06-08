using UnityEngine;

public class NPCAgeUp : NPCAvatar
{
	public int _RuneID = 12826;

	public int _AgeUpRequiredRank = 20;

	public int _AgeUpRequiredRunes = 200;

	public RaisedPetStage _AgeUpStage = RaisedPetStage.TITAN;

	public LocaleString _AlreadyAgedUpText = new LocaleString("Your dragon is already a Titan!");

	private bool mIsEngaged;

	private int mPrevAge;

	private KAUIGenericDB mKAUIGenericDB;

	public override void OnActivate()
	{
		if (SanctuaryManager.pCurPetData != null)
		{
			mPrevAge = RaisedPetData.GetAgeIndex(SanctuaryManager.pCurPetData.pStage);
			Input.ResetInputAxes();
			KAUICursorManager.SetDefaultCursor();
			SetUpUI();
		}
	}

	protected override void CheckTasksAndActivateQuestIcon()
	{
		if (_QuestIcon != null)
		{
			_QuestIcon.SetActive(value: false);
		}
	}

	public override void Update()
	{
		base.Update();
		if (_RewardIcon != null)
		{
			bool flag = SanctuaryManager.pCurPetData != null && SanctuaryManager.pCurPetData.pStage < _AgeUpStage;
			if (_RewardIcon.activeSelf != flag)
			{
				_RewardIcon.SetActive(flag);
			}
		}
	}

	private void SetUpUI()
	{
		int ageIndex = RaisedPetData.GetAgeIndex(_AgeUpStage);
		if (SanctuaryManager.pCurPetInstance.pAge >= ageIndex)
		{
			ShowKAUIDialog("PfKAUIGenericDB", "Already Aged Up", "", "", "DestroyDB", "", destroyDB: true, _AlreadyAgedUpText, base.gameObject);
			return;
		}
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(SanctuaryManager.pCurPetData.PetTypeID);
		if (sanctuaryPetTypeInfo != null)
		{
			if (ageIndex > sanctuaryPetTypeInfo._AgeData.Length - 1)
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				string[] array = GameConfig.GetKeyData("TitanInfoAsset").Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnTitanInfoLoaded, typeof(GameObject));
			}
			else
			{
				PetRankData.ResetCache();
				PetRankData.InitAchievementInfo(PetAchievementInfoReadyCallback);
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.SetUIActive(inActive: false);
				KAUICursorManager.SetDefaultCursor("Loading");
			}
		}
	}

	public void PetAchievementInfoReadyCallback()
	{
		int ageIndex = RaisedPetData.GetAgeIndex(_AgeUpStage);
		if ((PetRankData.GetUserRank(SanctuaryManager.pCurPetData)?.RankID ?? 1) < _AgeUpRequiredRank || SanctuaryManager.pCurPetInstance.pAge < ageIndex - 1)
		{
			DragonAgeUpConfig.ShowAgeUpUI(OnDragonAgeUpUIDone);
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = GameConfig.GetKeyData("RunesAgeUpAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnRunesAgeUpLoaded, typeof(GameObject));
		}
		mIsEngaged = true;
		StartEngagement(clipGiven: true);
	}

	private void OnRunesAgeUpLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			UiRunesAgeUp component = Object.Instantiate((GameObject)inObject).GetComponent<UiRunesAgeUp>();
			component.Init(_RuneID, _AgeUpRequiredRunes, _AgeUpStage, OnNPCAgeUpHandled);
			component.pNPCName = _Name;
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			OnNPCAgeUpHandled();
			break;
		}
	}

	private void OnTitanInfoLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			Object.Instantiate((GameObject)inObject).GetComponent<UiTitanInfo>().pCallback = OnNPCAgeUpHandled;
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			OnNPCAgeUpHandled();
			break;
		}
	}

	public override void EndEngagement()
	{
		if (!mIsEngaged)
		{
			base.EndEngagement();
		}
	}

	public void ShowKAUIDialog(string assetName, string dbName, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString localeString, GameObject msgObject = null)
	{
		if (mKAUIGenericDB != null)
		{
			DestroyDB();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (mKAUIGenericDB != null)
		{
			if (msgObject == null)
			{
				msgObject = base.gameObject;
			}
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			mKAUIGenericDB.SetMessage(msgObject, yesMessage, noMessage, okMessage, closeMessage);
			mKAUIGenericDB.SetDestroyOnClick(destroyDB);
			mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mKAUIGenericDB.SetTextByID(localeString._ID, localeString._Text, interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	public void DestroyDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
			AvAvatar.pState = AvAvatar.pPrevState;
			AvAvatar.SetUIActive(inActive: true);
		}
	}

	private void OnNPCAgeUpHandled()
	{
		if (mPrevAge < RaisedPetData.GetAgeIndex(SanctuaryManager.pCurPetData.pStage))
		{
			DragonAgeUpConfig.ShowAgeUpUI(OnDragonAgeUpUIDone, RaisedPetData.GetGrowthStage(mPrevAge), SanctuaryManager.pCurPetData, new RaisedPetStage[1] { SanctuaryManager.pCurPetData.pStage }, ageUpDone: true);
		}
		else
		{
			mIsEngaged = false;
			EndEngagement();
		}
	}

	private void OnDragonAgeUpUIDone()
	{
		mIsEngaged = false;
		EndEngagement();
	}
}
