using UnityEngine;

public class UiStableQuestResultMenu : KAUIMenu
{
	public LocaleString _DragonLevelText = new LocaleString("Level ");

	public void PopulateItems(TimedMissionSlotData slotData)
	{
		ClearItems();
		for (int i = 0; i < slotData.PetIDs.Count; i++)
		{
			RaisedPetData byID = RaisedPetData.GetByID(slotData.PetIDs[i]);
			if (byID == null)
			{
				continue;
			}
			KAWidget kAWidget = AddWidget(_Template.name, null);
			KAWidget kAWidget2 = kAWidget.FindChildItem("DragonInfo");
			kAWidget.SetVisibility(inVisible: true);
			StablesPetUserData userData = new StablesPetUserData(byID);
			kAWidget.SetUserData(userData);
			int slotIdx = (byID.ImagePosition.HasValue ? byID.ImagePosition.Value : 0);
			ImageData.Load("EggColor", slotIdx, base.gameObject);
			UiStableQuestResult uiStableQuestResult = (UiStableQuestResult)_ParentUi;
			KAWidget kAWidget3 = kAWidget.FindChildItem("TxtLevelUp");
			kAWidget3.SetVisibility(inVisible: false);
			if (uiStableQuestResult.pRewardList != null)
			{
				foreach (AchievementReward pReward in uiStableQuestResult.pRewardList)
				{
					if (pReward.PointTypeID.Value == 8 && pReward.EntityID == byID.EntityID)
					{
						UserAchievementInfo userAchievementInfo = PetRankData.GetUserAchievementInfo(byID);
						int value = userAchievementInfo.AchievementPointTotal.Value - pReward.Amount.Value;
						UserRank userRankByTypeAndValue = UserRankData.GetUserRankByTypeAndValue(8, value);
						if (userRankByTypeAndValue != null && userRankByTypeAndValue.RankID < userAchievementInfo.RankID)
						{
							kAWidget3.SetVisibility(inVisible: true);
						}
						kAWidget.FindChildItem("IcoXPDragon").SetVisibility(inVisible: true);
						kAWidget.FindChildItem("TxtRewardXP").SetVisibility(inVisible: true);
						kAWidget.FindChildItem("TxtRewardXP").SetText("+ " + Mathf.FloorToInt(pReward.Amount.Value));
					}
				}
			}
			kAWidget2.FindChildItem("TxtDragonName").SetText(byID.Name);
			int rankID = PetRankData.GetUserRank(byID).RankID;
			kAWidget.FindChildItem("TxtMeterBarXP").SetText(_DragonLevelText.GetLocalizedString() + rankID);
			int num = PetRankData.GetUserAchievementInfo(byID)?.AchievementPointTotal.Value ?? 0;
			UserRank userRank = PetRankData.GetUserRank(byID);
			UserRank nextRankByType = UserRankData.GetNextRankByType(8, userRank.RankID);
			float progressLevel = 1f;
			if (userRank.RankID != nextRankByType.RankID)
			{
				progressLevel = (float)(num - userRank.Value) / (float)(nextRankByType.Value - userRank.Value);
			}
			kAWidget.FindChildItem("DragonXpMeter").SetProgressLevel(progressLevel);
			kAWidget.FindChildItem("TxtCurrentXP").SetText(userRank.RankID.ToString());
			kAWidget.FindChildItem("TxtNextXP").SetText(nextRankByType.RankID.ToString());
		}
		mCurrentGrid.repositionNow = true;
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (img.mIconTexture == null)
		{
			return;
		}
		foreach (KAWidget item in GetItems())
		{
			StablesPetUserData stablesPetUserData = (StablesPetUserData)item.GetUserData();
			if ((stablesPetUserData.pData.ImagePosition.HasValue ? stablesPetUserData.pData.ImagePosition.Value : 0) == img.mSlotIndex)
			{
				item.FindChildItem("DragonIco").SetTexture(img.mIconTexture);
				break;
			}
		}
	}
}
