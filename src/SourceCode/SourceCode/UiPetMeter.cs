using UnityEngine;

public class UiPetMeter : KAUI
{
	public InteractiveTutManager _EnergyTutorial;

	public string _PetMeterAnchorName = "PetMeterAnchor";

	public Vector3 _PositionOffset = new Vector3(-130f, 50f, 0f);

	private KAWidget mPetMoodBkg;

	private KAWidget mPetXpMeterProgress;

	private KAWidget mMeterBarEnergyProgress;

	private KAWidget mMeterBarEnergyBkg;

	private KAWidget mMeterBarEnergyTxt;

	private KAWidget mPetLevelTxt;

	private KAWidget mPetNameTxt;

	private GameObject mPetCSM;

	private KAWidget mAniPointer;

	private KAWidget mPetPortrait;

	private CompactUI mCompactUI;

	private bool mIsPetPicTaken;

	private bool mUpdateRankUi;

	protected override void Start()
	{
		base.Start();
		mPetCSM = GameObject.Find("PfPetCSM");
		mPetLevelTxt = FindItem("TxtDragonLevel");
		mPetNameTxt = FindItem("TxtPetName");
		mPetXpMeterProgress = FindItem("DragonXpMeter");
		mMeterBarEnergyTxt = FindItem("TxtMeterBarEnergy");
		mMeterBarEnergyProgress = FindItem("MeterBarEnergy");
		mMeterBarEnergyBkg = FindItem("MeterBarEnergyBkg");
		mPetMoodBkg = FindItem("DragonMood");
		mAniPointer = FindItem("AniPointer");
		mPetPortrait = FindItem("DragonPic");
		AttachToToolbar();
	}

	public void RefreshAll()
	{
		mIsPetPicTaken = false;
		SetPetName(null);
		mUpdateRankUi = true;
	}

	public override void OnClick(KAWidget item)
	{
		if (!SanctuaryManager.pInstance.pDisablePetSwitch && (item == mMeterBarEnergyProgress || item == mPetMoodBkg) && mPetCSM != null)
		{
			mPetCSM.SendMessage("OnActivate");
		}
	}

	private void SetPetEnergy()
	{
		if (!(SanctuaryData.pInstance != null) || !UserRankData.pIsReady || !(SanctuaryManager.pCurPetInstance != null))
		{
			return;
		}
		SanctuaryPetMeterType sanctuaryPetMeterType = SanctuaryPetMeterType.ENERGY;
		float num = SanctuaryData.GetMaxMeter(sanctuaryPetMeterType, SanctuaryManager.pCurPetData);
		float num2 = SanctuaryManager.pCurPetInstance.GetMeterValue(sanctuaryPetMeterType);
		if (num == 0f)
		{
			num = 1f;
		}
		float num3 = num2 / num;
		mMeterBarEnergyProgress.SetProgressLevel(num3);
		if (num <= 1f)
		{
			num2 *= 100f;
			num *= 100f;
		}
		mMeterBarEnergyTxt.SetText(Mathf.RoundToInt(num2) + " / " + Mathf.RoundToInt(num));
		SanctuaryPetTypeSettings typeSettings = SanctuaryManager.pCurPetInstance.GetTypeSettings();
		if (num3 >= typeSettings._TiredThreshold)
		{
			if (mMeterBarEnergyBkg.GetCurrentAnim() != "Normal")
			{
				mMeterBarEnergyBkg.PlayAnim("Normal");
			}
		}
		else if (mMeterBarEnergyBkg.GetCurrentAnim() != "FlashRed")
		{
			mMeterBarEnergyBkg.PlayAnim("FlashRed");
		}
	}

	private void SetPetHappiness()
	{
		if (!(SanctuaryData.pInstance != null) || !UserRankData.pIsReady || !(SanctuaryManager.pCurPetInstance != null))
		{
			return;
		}
		float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HAPPINESS, SanctuaryManager.pCurPetData);
		float meterValue = SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HAPPINESS);
		float num = meterValue / maxMeter;
		if (maxMeter <= 1f)
		{
			meterValue *= 100f;
			maxMeter *= 100f;
		}
		SanctuaryPetTypeSettings typeSettings = SanctuaryManager.pCurPetInstance.GetTypeSettings();
		if (num >= typeSettings._FiredUpThreshold)
		{
			if (mPetMoodBkg.GetCurrentAnim() != "FiredUp")
			{
				mPetMoodBkg.PlayAnim("FiredUp");
			}
		}
		else if (num >= typeSettings._HappyThreshold)
		{
			if (mPetMoodBkg.GetCurrentAnim() != "Happy")
			{
				mPetMoodBkg.PlayAnim("Happy");
			}
		}
		else if (mPetMoodBkg.GetCurrentAnim() != "Angry")
		{
			mPetMoodBkg.PlayAnim("Angry");
		}
	}

	public void SetLevelandRankProgress()
	{
		mUpdateRankUi = false;
		int num = PetRankData.GetUserAchievementInfo(SanctuaryManager.pCurPetData)?.AchievementPointTotal.Value ?? 0;
		UserRank userRank = PetRankData.GetUserRank(SanctuaryManager.pCurPetData);
		UserRank nextRankByType = UserRankData.GetNextRankByType(8, userRank.RankID);
		float progressLevel = 1f;
		if (userRank.RankID != nextRankByType.RankID)
		{
			progressLevel = (float)(num - userRank.Value) / (float)(nextRankByType.Value - userRank.Value);
		}
		if (mPetXpMeterProgress != null)
		{
			mPetXpMeterProgress.SetProgressLevel(progressLevel);
		}
		if (SanctuaryManager.pCurPetData != null)
		{
			mPetLevelTxt.SetText(userRank.RankID.ToString());
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mUpdateRankUi && UserRankData.pIsReady && PetRankData.pIsReady)
		{
			SetLevelandRankProgress();
		}
		if (Application.isEditor && Input.GetKeyDown(KeyCode.Alpha6))
		{
			SanctuaryManager.pInstance.TakePicture(SanctuaryManager.pCurPetInstance.gameObject, base.gameObject, inSendPicture: false);
		}
		if (!mIsPetPicTaken && SanctuaryManager.pCurPetInstance != null)
		{
			mIsPetPicTaken = true;
			SanctuaryManager.pInstance.TakePicture(SanctuaryManager.pCurPetInstance.gameObject, base.gameObject, inSendPicture: false);
		}
		if (!(mCompactUI != null) || mCompactUI.pExpanded)
		{
			return;
		}
		if (IsMeterFlashing())
		{
			if (mCompactUI.pButton.GetCurrentAnim() != "FlashRed")
			{
				mCompactUI.pButton.PlayAnim("FlashRed");
			}
		}
		else if (mCompactUI.pButton.GetCurrentAnim() == "FlashRed")
		{
			mCompactUI.pButton.PlayAnim("Normal");
		}
	}

	private void OnPetPictureDoneFailed()
	{
		UtDebug.LogError("Failed to get Pet Picture");
		mIsPetPicTaken = true;
	}

	private void OnPetPictureDone(object inImage)
	{
		if (inImage != null)
		{
			if (mPetPortrait != null)
			{
				mPetPortrait.SetTexture(inImage as Texture);
			}
		}
		else
		{
			UtDebug.LogError("Failed to get Pet Picture");
		}
		mIsPetPicTaken = true;
	}

	public void SetPetName(string petName)
	{
		if (string.IsNullOrEmpty(petName))
		{
			mPetNameTxt.SetText((SanctuaryManager.pCurPetInstance != null) ? SanctuaryManager.pCurPetInstance.pData.Name : "--");
		}
		else
		{
			mPetNameTxt.SetText(petName);
		}
	}

	public void SetMeter(SanctuaryPetMeterType inType, float val)
	{
		switch (inType)
		{
		case SanctuaryPetMeterType.ENERGY:
			SetPetEnergy();
			break;
		case SanctuaryPetMeterType.HAPPINESS:
			SetPetHappiness();
			break;
		}
		SanctuaryManager.pCurPetInstance.PlayPetMoodParticle(inType, isForcePlay: false);
	}

	public void SetAllMeters()
	{
		SetPetEnergy();
		SetPetHappiness();
		SetPetName(null);
		mUpdateRankUi = true;
	}

	public void AttachToToolbar()
	{
		if (AvAvatar.pToolbar != null)
		{
			Transform transform = UtUtilities.FindChildTransform(AvAvatar.pToolbar, _PetMeterAnchorName);
			if (transform != null)
			{
				base.transform.parent = transform;
				_PositionOffset.z = base.transform.localPosition.z;
				base.transform.localPosition = _PositionOffset;
				mCompactUI = UtUtilities.GetComponentInParent(typeof(CompactUI), base.gameObject) as CompactUI;
				EnableAnchor(enableAnchor: false);
			}
			else
			{
				Debug.LogError("Could not find " + _PetMeterAnchorName + " under PfToolbar");
			}
		}
	}

	public void DetachFromToolbar()
	{
		EnableAnchor(enableAnchor: true);
		base.transform.parent = null;
		Vector3 position = base.transform.position;
		position.x = 0f;
		position.y = 0f;
		base.transform.position = position;
	}

	private void EnableAnchor(bool enableAnchor)
	{
		Transform child = base.transform.GetChild(0);
		if (!(null != child))
		{
			return;
		}
		UIAnchor component = child.GetComponent<UIAnchor>();
		if (null != component)
		{
			component.enabled = enableAnchor;
			if (!component.enabled)
			{
				component.transform.position = Vector3.zero;
				component.transform.localPosition = Vector3.zero;
			}
		}
	}

	public void SetDisabled(bool isDisabled)
	{
		if (isDisabled)
		{
			SetState(KAUIState.DISABLED);
		}
		else
		{
			SetState(KAUIState.INTERACTIVE);
		}
	}

	public void FlashPointer(bool flash)
	{
		if (mAniPointer != null)
		{
			if (flash)
			{
				mAniPointer.PlayAnim("Play");
				mAniPointer.SetVisibility(inVisible: true);
			}
			else
			{
				mAniPointer.SetVisibility(inVisible: false);
				mAniPointer.StopAnim();
			}
		}
	}

	private void OnCompactUIStateChanged(bool isExpanded)
	{
		if (isExpanded && mCompactUI != null)
		{
			mCompactUI.pButton.PlayAnim("Normal");
		}
	}

	private bool IsMeterFlashing()
	{
		return mMeterBarEnergyBkg.GetCurrentAnim() == "FlashRed";
	}
}
