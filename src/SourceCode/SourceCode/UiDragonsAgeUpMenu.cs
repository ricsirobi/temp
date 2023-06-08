using System;
using System.Linq;

public class UiDragonsAgeUpMenu : KAUIMenu
{
	private UiDragonsAgeUp mUiAgeUp;

	protected override void Start()
	{
		base.Start();
		mUiAgeUp = (UiDragonsAgeUp)_ParentUi;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == null || inWidget.pParentWidget == null)
		{
			return;
		}
		UiDragonsAgeUpMenuItem uiDragonsAgeUpMenuItem = (UiDragonsAgeUpMenuItem)inWidget.pParentWidget.pParentWidget;
		AgeUpUserData data = (AgeUpUserData)uiDragonsAgeUpMenuItem.GetUserData();
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(data.pData.PetTypeID);
		if (data.pData.pStage == RaisedPetStage.TITAN || (Array.Find(sanctuaryPetTypeInfo._GrowthStates, (RaisedPetGrowthState p) => p.Name == "Titan") == null && data.pData.pStage == RaisedPetStage.ADULT) || (sanctuaryPetTypeInfo._AgeUpMissionID > 0 && !MissionManager.IsMissionCompleted(sanctuaryPetTypeInfo._AgeUpMissionID)))
		{
			return;
		}
		if (data.pFreeAgeUp)
		{
			mUiAgeUp.DoFreeAgeUp(data);
			return;
		}
		int num = 0;
		PetAgeUpData petAgeUpData = mUiAgeUp.pAgeUpData.First((PetAgeUpData e) => e._FromPetStage == data.pData.pStage);
		if (petAgeUpData != null)
		{
			num += mUiAgeUp.GetAgeupItemQuantity(petAgeUpData._AgeUpItemID);
			num += mUiAgeUp.GetAgeupItemQuantity(petAgeUpData._AgeUpTicketID);
		}
		if (num > 0)
		{
			if (FUEManager.pInstance != null && FUEManager.pIsFUERunning && (bool)FUEManager.pInstance._AgeUpTutorial && FUEManager.pInstance._AgeUpTutorial.IsShowingTutorial())
			{
				FUEManager.pInstance._AgeUpTutorial.StartNextTutorial();
			}
			mUiAgeUp.pUiDragonAgeUpConfirm.Init(mUiAgeUp, data.pData, petAgeUpData);
		}
		else
		{
			mUiAgeUp.pUiAgeUpBuy.Init(mUiAgeUp, data.pData, petAgeUpData);
		}
	}
}
