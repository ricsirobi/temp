using System.Collections.Generic;
using UnityEngine;

public class UiProfileDragonInfoMenu : KAUIMenu
{
	public class PetUserData : KAWidgetUserData
	{
		public RaisedPetData pData;

		public PetUserData(RaisedPetData data)
		{
			pData = data;
		}
	}

	public UiProfileDragonInfoCard _UiDragonsInfoCard;

	private List<UserAchievementInfo> mAchievementInfo;

	public void Init()
	{
		ClearItems();
		SetVisibility(inVisible: true);
		if (UiProfile.pUserProfile.UserID == UserInfo.pInstance.UserID)
		{
			if (RaisedPetData.pActivePets != null)
			{
				StableData.UpdateLocaleNames();
				int num = 0;
				foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
				{
					if (value == null)
					{
						continue;
					}
					RaisedPetData[] array = value;
					foreach (RaisedPetData raisedPetData in array)
					{
						if (raisedPetData.pStage != RaisedPetStage.HATCHING)
						{
							CreateMenuItem(raisedPetData, num++);
						}
					}
				}
			}
		}
		else
		{
			int[] array2 = new int[SanctuaryData.pPetTypes.Length];
			int num2 = 0;
			SanctuaryPetTypeInfo[] pPetTypes = SanctuaryData.pPetTypes;
			foreach (SanctuaryPetTypeInfo sanctuaryPetTypeInfo in pPetTypes)
			{
				array2[num2++] = sanctuaryPetTypeInfo._TypeID;
			}
			WsWebService.GetAllActivePetsByuserId(UiProfile.pUserProfile.UserID, active: true, ServiceEventHandler, null);
		}
		PetRankData.LoadUserAchievementInfoForAllPets(UiProfile.pUserProfile.UserID, OnAllPetRanksReady, null);
	}

	public void ReInit()
	{
		ClearItems();
		if (UiProfile.pUserProfile.UserID == UserInfo.pInstance.UserID && RaisedPetData.pActivePets != null)
		{
			StableData.UpdateLocaleNames();
			int num = 0;
			foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
			{
				if (value != null)
				{
					RaisedPetData[] array = value;
					foreach (RaisedPetData pData in array)
					{
						CreateMenuItem(pData, num++);
					}
				}
			}
		}
		List<KAWidget> items = GetItems();
		foreach (UserAchievementInfo item in mAchievementInfo)
		{
			foreach (KAWidget item2 in items)
			{
				if (!(item2 != null) || item == null)
				{
					continue;
				}
				PetUserData petUserData = (PetUserData)item2.GetUserData();
				if (petUserData != null && petUserData.pData != null && petUserData.pData.EntityID.HasValue && petUserData.pData.EntityID.HasValue && item.UserID.ToString() == petUserData.pData.EntityID.ToString())
				{
					KAWidget kAWidget = item2.FindChildItem("DragonLevelTxt");
					if (kAWidget != null)
					{
						kAWidget.SetText(item.RankID.ToString());
						kAWidget.SetVisibility(inVisible: true);
					}
					break;
				}
			}
		}
	}

	private void CreateMenuItem(RaisedPetData pData, int pos)
	{
		KAWidget kAWidget = DuplicateWidget(_Template);
		kAWidget.name = pData.Name;
		PetUserData userData = new PetUserData(pData);
		kAWidget.SetUserData(userData);
		if (SanctuaryManager.pCurPetData != null && SanctuaryManager.pCurPetData.RaisedPetID == pData.RaisedPetID)
		{
			AddWidgetAt(0, kAWidget);
		}
		else
		{
			AddWidgetAt(pos, kAWidget);
		}
		KAWidget kAWidget2 = kAWidget.FindChildItem("DragonLevelTxt");
		if (pData.EntityID.HasValue && pData.EntityID.HasValue)
		{
			kAWidget2.SetVisibility(inVisible: false);
			if (mAchievementInfo != null)
			{
				foreach (UserAchievementInfo item in mAchievementInfo)
				{
					if (item.UserID.ToString() == pData.EntityID.ToString())
					{
						kAWidget2.SetText(item.RankID.ToString());
						kAWidget2.SetVisibility(inVisible: true);
						break;
					}
				}
			}
		}
		else
		{
			kAWidget2.SetText("1");
		}
		kAWidget.FindChildItem("DragonNameTxt").SetText(pData.Name);
		if (TimedMissionManager.pInstance != null && TimedMissionManager.pInstance.IsPetEngaged(pData.RaisedPetID))
		{
			KAWidget kAWidget3 = kAWidget.FindChildItem("BusyIcon");
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(inVisible: true);
			}
		}
		kAWidget.SetVisibility(inVisible: true);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			switch (inType)
			{
			case WsServiceType.GET_ALL_ACTIVE_PETS_BY_USER_ID:
			{
				RaisedPetData[] array = (RaisedPetData[])inObject;
				RaisedPetData raisedPetData = null;
				if (array == null || array.Length == 0)
				{
					break;
				}
				RaisedPetData.ResolvePetArray(array, isactive: true);
				int num = 0;
				RaisedPetData[] array2 = array;
				foreach (RaisedPetData raisedPetData2 in array2)
				{
					if (!raisedPetData2.IsSelected)
					{
						CreateMenuItem(raisedPetData2, num++);
					}
					else
					{
						raisedPetData = raisedPetData2;
					}
				}
				if (raisedPetData != null)
				{
					CreateMenuItem(raisedPetData, num++);
				}
				break;
			}
			case WsServiceType.GET_IMAGE_BY_USER_ID:
				if (inObject != null)
				{
					ImageData imageData = (ImageData)inObject;
					((KAWidget)inUserData).SetTextureFromURL(imageData.ImageURL);
				}
				break;
			}
			break;
		case WsServiceEvent.ERROR:
			Debug.LogError("!!!" + inType.ToString() + " failed!!!!");
			break;
		}
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (img != null && img.mIconTexture == null)
		{
			return;
		}
		List<KAWidget> items = GetItems();
		if (items == null)
		{
			return;
		}
		foreach (KAWidget item in items)
		{
			PetUserData petUserData = (PetUserData)item.GetUserData();
			if (item != null && petUserData != null && petUserData.pData != null && petUserData.pData.ImagePosition.HasValue && petUserData.pData.ImagePosition.Value == img.mSlotIndex)
			{
				KAWidget kAWidget = item.FindChildItem("DragonProfilePic");
				if (kAWidget != null)
				{
					kAWidget.SetTexture(img.mIconTexture);
				}
				break;
			}
		}
	}

	private void OnAllPetRanksReady(List<UserAchievementInfo> achievementInfo, object UserItemData)
	{
		mAchievementInfo = achievementInfo;
		List<KAWidget> items = GetItems();
		if (items == null || achievementInfo == null)
		{
			return;
		}
		foreach (UserAchievementInfo item in achievementInfo)
		{
			foreach (KAWidget item2 in items)
			{
				if (!(item2 != null) || item == null)
				{
					continue;
				}
				PetUserData petUserData = (PetUserData)item2.GetUserData();
				if (petUserData != null && petUserData.pData != null && petUserData.pData.EntityID.HasValue && petUserData.pData.EntityID.HasValue && item.UserID.ToString() == petUserData.pData.EntityID.ToString())
				{
					KAWidget kAWidget = item2.FindChildItem("DragonLevelTxt");
					if (kAWidget != null)
					{
						kAWidget.SetText(item.RankID.ToString());
						kAWidget.SetVisibility(inVisible: true);
					}
					break;
				}
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_UiDragonsInfoCard != null)
		{
			PetUserData petUserData = (PetUserData)inWidget.GetUserData();
			_UiDragonsInfoCard.ShowInfoCard(petUserData.pData);
		}
	}

	public override void LoadItem(KAWidget widget)
	{
		KAWidget kAWidget = widget.FindChildItem("DragonProfilePic");
		if (!(kAWidget != null) || !(kAWidget.GetTexture() == null))
		{
			return;
		}
		PetUserData petUserData = (PetUserData)widget.GetUserData();
		if (petUserData != null && petUserData.pData != null && petUserData.pData.ImagePosition.HasValue)
		{
			int value = petUserData.pData.ImagePosition.Value;
			if (UserInfo.pInstance.UserID != UiProfile.pUserProfile.UserID)
			{
				WsWebService.GetImageDataByUserId(UiProfile.pUserProfile.UserID, "EggColor", value, ServiceEventHandler, kAWidget);
			}
			else
			{
				ImageData.Load("EggColor", value, base.gameObject);
			}
		}
	}
}
