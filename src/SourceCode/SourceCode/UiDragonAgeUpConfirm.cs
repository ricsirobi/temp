using UnityEngine;

public class UiDragonAgeUpConfirm : KAUI
{
	[SerializeField]
	private int m_AdultLevel = 10;

	[SerializeField]
	private int m_TitanLevel = 20;

	[SerializeField]
	private KAWidget m_TxtTitle;

	[SerializeField]
	private KAWidget m_TxtBoosted;

	private string mTitleText;

	private UiDragonsAgeUp mUiDragonsAgeUp;

	private RaisedPetData mRaisedPetData;

	private PetAgeUpData mPetAgeData;

	private int mSelectedAgeupItemId;

	protected override void Start()
	{
		base.Start();
		mTitleText = m_TxtTitle.GetText();
	}

	public void Init(UiDragonsAgeUp parent, RaisedPetData data, PetAgeUpData ageData)
	{
		mUiDragonsAgeUp = parent;
		mRaisedPetData = data;
		mPetAgeData = ageData;
		mSelectedAgeupItemId = -1;
		KAWidget kAWidget = FindItem("IcoDragon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		int slotIdx = (data.ImagePosition.HasValue ? data.ImagePosition.Value : 0);
		ImageData.Load("EggColor", slotIdx, base.gameObject);
		string text = string.Format(mTitleText, (mPetAgeData._ToPetStage == RaisedPetStage.TITAN) ? SanctuaryData.GetDisplayTextFromPetAge(RaisedPetStage.TITAN) : SanctuaryData.GetDisplayTextFromPetAge(RaisedPetStage.ADULT));
		m_TxtTitle.SetText(text);
		UserRank userRank = PetRankData.GetUserRank(mRaisedPetData);
		if ((userRank.RankID >= m_AdultLevel && mPetAgeData._ToPetStage == RaisedPetStage.ADULT) || (userRank.RankID >= m_TitanLevel && mPetAgeData._ToPetStage == RaisedPetStage.TITAN))
		{
			m_TxtBoosted.SetVisibility(inVisible: false);
		}
		else
		{
			m_TxtBoosted.SetVisibility(inVisible: true);
		}
		m_TxtBoosted.SetText((mPetAgeData._ToPetStage == RaisedPetStage.TITAN) ? m_TitanLevel.ToString() : m_AdultLevel.ToString());
		KAUI.SetExclusive(this);
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetVisibility(inVisible: true);
	}

	private void OnImageLoaded(ImageDataInstance img)
	{
		KAWidget kAWidget = FindItem("IcoDragon");
		if (kAWidget != null && img.mIconTexture != null)
		{
			kAWidget.SetTexture(img.mIconTexture);
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _BackButtonName)
		{
			Exit(refreshUI: false);
		}
		else if (inWidget.name == "YesBtn")
		{
			DoAgeup();
		}
	}

	private void Exit(bool refreshUI = true)
	{
		if (refreshUI)
		{
			mUiDragonsAgeUp.UpdateUpgradeAvailibility();
			mUiDragonsAgeUp.ShowDragons();
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		KAUI.RemoveExclusive(this);
		SetVisibility(inVisible: false);
	}

	private void DoAgeup()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		if (mUiDragonsAgeUp.GetAgeupItemQuantity(mPetAgeData._AgeUpItemID) > 0)
		{
			mSelectedAgeupItemId = mPetAgeData._AgeUpItemID;
		}
		else if (mUiDragonsAgeUp.GetAgeupItemQuantity(mPetAgeData._AgeUpTicketID) > 0)
		{
			mSelectedAgeupItemId = mPetAgeData._AgeUpTicketID;
		}
		SanctuaryManager.pInstance.SetAge(mRaisedPetData, RaisedPetData.GetAgeIndex(mPetAgeData._ToPetStage));
		CommonInventoryRequest[] array = new CommonInventoryRequest[1]
		{
			new CommonInventoryRequest()
		};
		array[0].ItemID = mSelectedAgeupItemId;
		array[0].Quantity = -1;
		mRaisedPetData.SaveDataReal(OnSetAgeDone, array);
		SetInteractive(interactive: false);
	}

	public void OnSetAgeDone(SetRaisedPetResponse response)
	{
		if (response != null && response.RaisedPetSetResult == RaisedPetSetResult.Success)
		{
			if (CommonInventoryData.pInstance.RemoveItem(mSelectedAgeupItemId, updateServer: false) < 0)
			{
				if (ParentData.pInstance.pInventory.pData.RemoveItem(mSelectedAgeupItemId, updateServer: false) >= 0)
				{
					ParentData.pInstance.pInventory.pData.ClearSaveCache();
				}
			}
			else
			{
				CommonInventoryData.pInstance.ClearSaveCache();
			}
			if (mPetAgeData._ToPetStage == RaisedPetStage.TITAN)
			{
				UserAchievementTask.Set(SanctuaryManager.pInstance._DragonTitanAchievemetID);
			}
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForTaskCompletion("Action", "AgeUp", mPetAgeData._ToPetStage.ToString());
			}
			if (FUEManager.pInstance != null && FUEManager.pIsFUERunning && (bool)FUEManager.pInstance._AgeUpTutorial && FUEManager.pInstance._AgeUpTutorial.IsShowingTutorial())
			{
				FUEManager.pInstance._AgeUpTutorial.StartNextTutorial();
			}
			PetRankData.LoadUserRank(mRaisedPetData, OnUserRankReady, forceLoad: true);
		}
		else
		{
			SanctuaryManager.pInstance.SetAge(mRaisedPetData, RaisedPetData.GetAgeIndex(mPetAgeData._FromPetStage), inSave: false);
			SetInteractive(interactive: true);
			Exit();
		}
	}

	protected virtual void OnUserRankReady(UserRank rank, object userData)
	{
		SetInteractive(interactive: true);
		Exit();
		mUiDragonsAgeUp.FinishAgeUp(mRaisedPetData, mPetAgeData._FromPetStage);
	}
}
