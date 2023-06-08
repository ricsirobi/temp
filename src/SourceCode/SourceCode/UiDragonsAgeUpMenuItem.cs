using System;
using UnityEngine;

public class UiDragonsAgeUpMenuItem : KAWidget
{
	[SerializeField]
	private KAButton m_BtnBaby;

	[SerializeField]
	private KAButton m_BtnTeen;

	[SerializeField]
	private KAButton m_BtnAdult;

	[SerializeField]
	private KAButton m_BtnTitan;

	[SerializeField]
	private KAButton m_BtnSpecial;

	[SerializeField]
	private KAButton m_BtnFreeAgeup;

	[SerializeField]
	private KAButton m_BtnAgeUpMissionLocked;

	public void SetupWidget()
	{
		AgeUpUserData ageUpUserData = (AgeUpUserData)GetUserData();
		int slotIdx = (ageUpUserData.pData.ImagePosition.HasValue ? ageUpUserData.pData.ImagePosition.Value : 0);
		ImageData.Load("EggColor", slotIdx, base.gameObject);
		PetRankData.LoadUserAchievementInfo(ageUpUserData.pData, OnPetInfoReady, forceLoad: false, ageUpUserData.pData);
		KAWidget kAWidget = FindChildItem("LockedIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(SanctuaryManager.IsPetLocked(ageUpUserData.pData));
		}
		kAWidget = FindChildItem("TxtDragonName");
		if (kAWidget != null)
		{
			kAWidget.SetText(ageUpUserData.pData.Name);
		}
		kAWidget = FindChildItem("TxtAge");
		if (kAWidget != null)
		{
			kAWidget.SetText(SanctuaryData.GetDisplayTextFromPetAge(ageUpUserData.pData.pStage) + " " + SanctuaryData.FindSanctuaryPetTypeInfo(ageUpUserData.pData.PetTypeID)._NameText.GetLocalizedString());
		}
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(ageUpUserData.pData.PetTypeID);
		int num = string.Compare(sanctuaryPetTypeInfo._AgeData[sanctuaryPetTypeInfo._AgeData.Length - 1]._Name, RaisedPetStage.TITAN.ToString(), ignoreCase: true);
		if (sanctuaryPetTypeInfo._IsUniquePet)
		{
			if (Array.Find(sanctuaryPetTypeInfo._GrowthStates, (RaisedPetGrowthState p) => p.Name == "Titan") == null && ageUpUserData.pData.pStage == RaisedPetStage.ADULT)
			{
				m_BtnSpecial.SetVisibility(inVisible: true);
			}
			else if (((sanctuaryPetTypeInfo._AgeUpMissionID > 0 && MissionManager.IsMissionCompleted(sanctuaryPetTypeInfo._AgeUpMissionID)) || sanctuaryPetTypeInfo._AgeUpMissionID == 0) && ageUpUserData.pData.pStage == RaisedPetStage.TEEN)
			{
				m_BtnTeen.SetVisibility(inVisible: true);
			}
			else if (sanctuaryPetTypeInfo._AgeUpMissionID > 0 && !MissionManager.IsMissionCompleted(sanctuaryPetTypeInfo._AgeUpMissionID) && ageUpUserData.pData.pStage != RaisedPetStage.ADULT)
			{
				m_BtnAgeUpMissionLocked.SetVisibility(inVisible: true);
			}
		}
		else if (ageUpUserData.pFreeAgeUp)
		{
			m_BtnFreeAgeup.SetVisibility(inVisible: true);
		}
		else
		{
			m_BtnBaby.SetVisibility(ageUpUserData.pData.pStage < RaisedPetStage.ADULT);
			m_BtnAdult.SetVisibility(ageUpUserData.pData.pStage == RaisedPetStage.ADULT);
			m_BtnTitan.SetVisibility(ageUpUserData.pData.pStage >= RaisedPetStage.TITAN);
		}
		if (m_BtnBaby.GetVisibility())
		{
			SetAgeDataTags(m_BtnBaby, num, sanctuaryPetTypeInfo._AgeData[sanctuaryPetTypeInfo._AgeData.Length - 1]._NewlyAdded);
			m_BtnBaby.SetInteractive(num == 0 || ageUpUserData.pData.pStage != RaisedPetStage.ADULT);
		}
		if (m_BtnAdult.GetVisibility())
		{
			SetAgeDataTags(m_BtnAdult, num, sanctuaryPetTypeInfo._AgeData[sanctuaryPetTypeInfo._AgeData.Length - 1]._NewlyAdded);
			m_BtnAdult.SetInteractive(num == 0 || ageUpUserData.pData.pStage != RaisedPetStage.ADULT);
		}
	}

	private void SetAgeDataTags(KAWidget btn, int titanAvailable, bool newTitan)
	{
		KAWidget kAWidget = btn.FindChildItem("IcoComingSoon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(titanAvailable != 0);
		}
		if (titanAvailable == 0)
		{
			KAWidget kAWidget2 = btn.FindChildItem("IcoNew");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(newTitan);
			}
		}
	}

	private void OnImageLoaded(ImageDataInstance img)
	{
		if (img.mIconTexture != null)
		{
			FindChildItem("DragonIco").SetTexture(img.mIconTexture);
		}
	}

	private void OnPetInfoReady(UserAchievementInfo achievementInfo, object userData)
	{
		UserRank userRank = PetRankData.GetUserRank((RaisedPetData)userData);
		((AgeUpUserData)GetUserData())?.SetFreeAgeup();
		KAWidget kAWidget = FindChildItem("TxtLevel");
		if (kAWidget != null)
		{
			kAWidget.SetText(userRank.RankID.ToString());
		}
		SetVisibility(inVisible: true);
	}

	public void PlayAnimOnBtns(string inAnimName)
	{
		m_BtnBaby.PlayAnim(inAnimName);
		m_BtnTeen.PlayAnim(inAnimName);
		m_BtnAdult.PlayAnim(inAnimName);
		m_BtnTitan.PlayAnim(inAnimName);
		m_BtnSpecial.PlayAnim(inAnimName);
		m_BtnFreeAgeup.PlayAnim(inAnimName);
		m_BtnAgeUpMissionLocked.PlayAnim(inAnimName);
	}
}
