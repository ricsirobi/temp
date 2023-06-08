using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UiResultXPMenu : KAUIMenu
{
	public LocaleString _DragonLevelText = new LocaleString("Level ");

	public void PopulateItems(List<AchievementReward> Rewards)
	{
		ClearItems();
		int num = 0;
		KAWidget kAWidget = AddWidget(_Template.name, null);
		kAWidget.SetVisibility(inVisible: true);
		KAWidgetUserData userData = new KAWidgetUserData(num);
		kAWidget.SetUserData(userData);
		PopulateXPData(kAWidget, null, Rewards);
		for (int i = 0; i < GameManager.pInstance.pRaisedPetEntityMap.Count; i++)
		{
			RaisedPetData byID = RaisedPetData.GetByID(GameManager.pInstance.pRaisedPetEntityMap[i].RaisedPetID);
			if (byID != null)
			{
				KAWidget kAWidget2 = AddWidget(_Template.name, null);
				kAWidget2.SetVisibility(inVisible: true);
				num++;
				KAWidgetUserData userData2 = new KAWidgetUserData(num);
				kAWidget2.SetUserData(userData2);
				PopulateXPData(kAWidget2, byID, Rewards);
			}
		}
		mCurrentGrid.repositionNow = true;
	}

	private void PopulateXPData(KAWidget inWidget, RaisedPetData petData, List<AchievementReward> Rewards)
	{
		KAWidget kAWidget = inWidget.FindChildItem("DragonInfo");
		KAWidget kAWidget2 = inWidget.FindChildItem("TxtLevelUp");
		kAWidget2.SetVisibility(inVisible: false);
		UserRank userRank = null;
		UserRank userRank2 = null;
		UserAchievementInfo userAchievementInfo = null;
		int num = 0;
		string empty = string.Empty;
		if (petData == null)
		{
			userRank = UserRankData.GetUserRankByValue(UserRankData.pInstance.AchievementPointTotal.Value);
			userRank2 = UserRankData.GetNextRank(userRank.RankID);
			userAchievementInfo = UserRankData.pInstance;
			num = UserRankData.pInstance.AchievementPointTotal.Value;
			empty = AvatarData.pInstance.DisplayName;
			KAWidget kAWidget3 = inWidget.FindChildItem("IconBackground");
			AvPhotoSetter @object = new AvPhotoSetter(kAWidget3);
			GameManager.pInstance.pStillPhotoManager.TakePhotoUI(UserInfo.pInstance.UserID, (Texture2D)kAWidget3.GetTexture(), @object.PhotoCallback, null);
		}
		else
		{
			userRank = PetRankData.GetUserRank(petData);
			userRank2 = UserRankData.GetNextRankByType(8, userRank.RankID);
			userAchievementInfo = PetRankData.GetUserAchievementInfo(petData);
			num = userAchievementInfo?.AchievementPointTotal.Value ?? 0;
			empty = petData.Name;
			int slotIdx = (petData.ImagePosition.HasValue ? petData.ImagePosition.Value : 0);
			ImageData.Load("EggColor", slotIdx, base.gameObject);
		}
		if (Rewards != null)
		{
			foreach (AchievementReward Reward in Rewards)
			{
				int num2 = 0;
				UserRank userRank3 = null;
				if (petData == null && Reward.PointTypeID.Value == 1)
				{
					num2 = num - Reward.Amount.Value;
					inWidget.FindChildItem("IconXPAvatar").SetVisibility(inVisible: true);
					userRank3 = UserRankData.GetUserRankByValue(num2);
				}
				else if (petData != null && Reward.PointTypeID.Value == 8 && Reward.EntityID == petData.EntityID)
				{
					num2 = num - Reward.Amount.Value;
					inWidget.FindChildItem("IconXPDragon").SetVisibility(inVisible: true);
					userRank3 = UserRankData.GetUserRankByTypeAndValue(8, num2);
				}
				if (num2 > 0)
				{
					if (userRank3 != null && userRank3.RankID < userAchievementInfo.RankID)
					{
						kAWidget2.SetVisibility(inVisible: true);
					}
					inWidget.FindChildItem("TxtRewardXP").SetVisibility(inVisible: true);
					inWidget.FindChildItem("TxtRewardXP").SetText("+ " + Mathf.FloorToInt(Reward.Amount.Value));
				}
			}
		}
		kAWidget.FindChildItem("TxtDragonName").SetText(empty);
		inWidget.FindChildItem("TxtMeterBarXP").SetText(_DragonLevelText.GetLocalizedString() + userRank.RankID);
		float progressLevel = 1f;
		if (userRank.RankID != userRank2.RankID)
		{
			progressLevel = (float)(num - userRank.Value) / (float)(userRank2.Value - userRank.Value);
		}
		inWidget.FindChildItem("DragonXpMeter").SetProgressLevel(progressLevel);
		inWidget.FindChildItem("TxtCurrentXPLvl").SetText(userRank.RankID.ToString());
		inWidget.FindChildItem("TxtXPInfo").SetText(num + "/" + userRank2.Value);
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (img.mIconTexture == null)
		{
			return;
		}
		foreach (KAWidget item in GetItems())
		{
			KAWidgetUserData userData = item.GetUserData();
			if (userData._Index != 0)
			{
				RaisedPetData byID = RaisedPetData.GetByID(GameManager.pInstance.pRaisedPetEntityMap[userData._Index - 1].RaisedPetID);
				if (byID != null && (byID.ImagePosition.HasValue ? byID.ImagePosition.Value : 0) == img.mSlotIndex)
				{
					item.FindChildItem("IconBackground").SetTexture(img.mIconTexture);
					break;
				}
			}
		}
	}
}
