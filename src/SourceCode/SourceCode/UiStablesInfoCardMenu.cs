public class UiStablesInfoCardMenu : KAUIMenu
{
	public class NestPetUserData : KAWidgetUserData
	{
		public RaisedPetData pData;

		public int _NestID;

		public NestPetUserData(RaisedPetData data, int nestID)
		{
			pData = data;
			_NestID = nestID;
		}
	}

	public UiStablesInfoCard _UiStablesInfoCard;

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_UiStablesInfoCard != null)
		{
			_UiStablesInfoCard.OnClick(inWidget);
		}
	}

	public void LoadNestList(StableData pData)
	{
		ClearItems();
		if (pData == null)
		{
			return;
		}
		foreach (NestData nest in pData.NestList)
		{
			KAWidget kAWidget = AddWidget(_Template.name, null);
			kAWidget.SetVisibility(inVisible: true);
			KAWidget kAWidget2 = kAWidget.FindChildItem("TxtRoomName");
			kAWidget2.SetText("Empty Room");
			kAWidget.FindChildItem("DragonIco").SetVisibility(inVisible: false);
			KAWidget kAWidget3 = kAWidget.FindChildItem("TxtLvl");
			RaisedPetData raisedPetData = null;
			if (nest.PetID > 0)
			{
				raisedPetData = RaisedPetData.GetByID(nest.PetID);
				if (raisedPetData != null)
				{
					kAWidget2.SetText(raisedPetData.Name);
					if (TimedMissionManager.pInstance != null && TimedMissionManager.pInstance.IsPetEngaged(raisedPetData.RaisedPetID))
					{
						KAWidget kAWidget4 = kAWidget.FindChildItem("BusyIcon");
						if (kAWidget4 != null)
						{
							kAWidget4.SetVisibility(inVisible: true);
						}
					}
					PetRankData.LoadUserRank(raisedPetData, OnPetRankReady, forceLoad: false, kAWidget3);
				}
			}
			NestPetUserData userData = new NestPetUserData(raisedPetData, nest.ID);
			kAWidget.SetUserData(userData);
			if (raisedPetData != null)
			{
				int slotIdx = (raisedPetData.ImagePosition.HasValue ? raisedPetData.ImagePosition.Value : 0);
				ImageData.Load("EggColor", slotIdx, base.gameObject);
			}
			if (((UiDragonsStable)_UiStablesInfoCard._UiCardParent).pUiStablesListCard.pCurrentMode == UiStablesListCard.Mode.NestAllocation && raisedPetData != null)
			{
				kAWidget.SetDisabled(isDisabled: true);
			}
			KAWidget kAWidget5 = kAWidget.FindChildItem("BtnMoveIn");
			if (raisedPetData == null)
			{
				kAWidget.FindChildItem("TxtEmptyNest").SetVisibility(inVisible: true);
				kAWidget2.SetVisibility(inVisible: false);
				kAWidget5.SetVisibility(inVisible: true);
				kAWidget3.SetVisibility(inVisible: false);
			}
			else
			{
				kAWidget5.SetVisibility(inVisible: false);
			}
		}
	}

	private void OnPetRankReady(UserRank rank, object userData)
	{
		KAWidget kAWidget = (KAWidget)userData;
		if (kAWidget != null && rank != null)
		{
			kAWidget.SetText(rank.RankID.ToString());
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (img.mIconTexture == null)
		{
			return;
		}
		foreach (KAWidget item in GetItems())
		{
			NestPetUserData nestPetUserData = (NestPetUserData)item.GetUserData();
			if (nestPetUserData != null && nestPetUserData.pData != null && (nestPetUserData.pData.ImagePosition.HasValue ? nestPetUserData.pData.ImagePosition.Value : 0) == img.mSlotIndex)
			{
				KAWidget kAWidget = item.FindChildItem("DragonIco");
				kAWidget.SetTexture(img.mIconTexture);
				kAWidget.SetVisibility(inVisible: true);
				break;
			}
		}
	}
}
