using UnityEngine;

public class UiDragonName : KAUI
{
	public LocaleString _PetNameInvalid = new LocaleString("Name is invalid please try some other names");

	private KAWidget mEditLabel;

	private KAWidget mOKBtn;

	private bool mDragonNameEntered;

	private string mDragonName;

	private KAUIGenericDB mUiGenericDB;

	private bool mUseTicketItem;

	private int mTicketID;

	private RaisedPetData mSelectedPetData;

	protected override void Start()
	{
		base.Start();
		mEditLabel = FindItem("TxtEditName");
		mOKBtn = FindItem("BtnOk");
		KAUI.SetExclusive(this);
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		if (inItem == mEditLabel)
		{
			if (!mDragonNameEntered)
			{
				mDragonNameEntered = true;
			}
		}
		else
		{
			if (!(inItem == mOKBtn))
			{
				return;
			}
			mDragonName = ((KAEditBox)mEditLabel).GetText().Trim();
			if (string.IsNullOrEmpty(mDragonName) || !mDragonNameEntered || mDragonName == ((KAEditBox)mEditLabel)._DefaultText.GetLocalizedString())
			{
				ShowPetMessage(_PetNameInvalid.GetLocalizedString());
				return;
			}
			SetState(KAUIState.NOT_INTERACTIVE);
			KAUICursorManager.SetDefaultCursor("Loading");
			if (!mUseTicketItem)
			{
				SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
				pCurPetInstance.pData.pIsNameCustomized = true;
				pCurPetInstance.pData.Name = mDragonName;
				pCurPetInstance.pData.SaveDataReal(PetSaveEventHandler);
			}
			else
			{
				mSelectedPetData.pIsNameCustomized = true;
				mSelectedPetData.Name = mDragonName;
				CommonInventoryRequest[] array = new CommonInventoryRequest[1]
				{
					new CommonInventoryRequest()
				};
				array[0].ItemID = mTicketID;
				array[0].Quantity = -1;
				mSelectedPetData.SaveDataReal(PetSaveEventHandler, array);
			}
		}
	}

	public void PetSaveEventHandler(SetRaisedPetResponse response)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (response != null)
		{
			if (response.RaisedPetSetResult == RaisedPetSetResult.InvalidPetName)
			{
				ShowPetMessage(_PetNameInvalid.GetLocalizedString());
				return;
			}
			if (mUseTicketItem)
			{
				CommonInventoryData.pInstance.RemoveItem(mTicketID, updateServer: false);
				CommonInventoryData.pInstance.ClearSaveCache();
			}
			if (SanctuaryManager.pInstance.pPetMeter != null)
			{
				SanctuaryManager.pInstance.pPetMeter.SetPetName(SanctuaryManager.pCurPetInstance.pData.Name);
			}
		}
		if (mUseTicketItem)
		{
			mSelectedPetData = null;
		}
		else
		{
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
		}
		if (SanctuaryManager.pInstance.OnNameSelectionDone != null)
		{
			SanctuaryManager.pInstance.OnNameSelectionDone();
		}
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
	}

	private void ShowPetMessage(string inText)
	{
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Dragon Name Invalid");
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._OKMessage = "OnPetNameErrorOkClick";
		mUiGenericDB.SetText(inText, interactive: false);
		mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnPetNameErrorOkClick()
	{
		SetState(KAUIState.INTERACTIVE);
		Object.Destroy(mUiGenericDB.gameObject);
		mUiGenericDB = null;
	}

	public void SetNameChangeTicket(int ticketID, RaisedPetData pData)
	{
		mUseTicketItem = true;
		mTicketID = ticketID;
		mSelectedPetData = pData;
	}
}
