using UnityEngine;

public class UiRaceTrackHUD : KAUI
{
	private KAWidget mRaceTimeTxt;

	private KAWidget mLapCountTxt;

	private KAWidget mPositionsTxt;

	private KAWidget mBkgMapFrame;

	private KAWidget mAvatarXpMeterProgress;

	private KAWidget mAvatarLevelTxt;

	private KAWidget mAvatarNameTxt;

	private KAWidget mAvatarHPMeterProgress;

	private KAWidget mAvatarHPTxt;

	private KAWidget mPortrait;

	private KAWidget mStunTriggerNotch;

	private LevelManager mLevelManager;

	private bool mIsDragonMeterReady;

	private AvPhotoManager mPhotoManager;

	private AvAvatarController mAVAC;

	private KAWidget mChallengePoints;

	private float mChallengePointFlashDuration;

	private float mChallengePointFlashTimer;

	private int mChallengePointFlashTimes;

	protected override void Start()
	{
		base.Start();
		GetWidgetReferences();
		SetUiVisibility(Visibility: false);
		SetPlayerInfoDisplay();
		EnableBackButton(enable: false);
		mChallengePoints = FindItem("ChallengePoints");
		UpdateUDTPoints();
	}

	public void UpdateUDTPoints()
	{
		KAWidget kAWidget = FindItem("UDTStarsIcon");
		if (kAWidget != null)
		{
			UDTUtilities.UpdateUDTStars(kAWidget.transform, "ToolbarStarsIconBkg", "ToolbarStarsIconFrameBkg");
		}
		UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(12);
		KAWidget kAWidget2 = FindItem("TxtUDTPoints");
		if (kAWidget2 != null)
		{
			if (userAchievementInfoByType != null)
			{
				kAWidget2.SetText(userAchievementInfoByType.AchievementPointTotal.Value.ToString());
			}
			else
			{
				kAWidget2.SetText("0");
			}
		}
	}

	public void Init(LevelManager lvlManager)
	{
		mLevelManager = lvlManager;
		GetWidgetReferences();
		SetStunTriggerNotch(mLevelManager._StunPointPercentage);
		KAWidget kAWidget = FindItem("BtnReset");
		if ((bool)kAWidget)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
	}

	private void GetWidgetReferences()
	{
		mRaceTimeTxt = FindItem("TxtRaceHUDTime");
		mLapCountTxt = FindItem("TxtRaceHUDLap");
		mPositionsTxt = FindItem("TxtRaceHUDPlace");
		mBkgMapFrame = FindItem("RaceHUDMapFrame");
		mAvatarXpMeterProgress = FindItem("AvatarXpMeter");
		mAvatarLevelTxt = FindItem("TxtAvatarLevel");
		mAvatarNameTxt = FindItem("TxtAvatarName");
		mPortrait = FindItem("AvatarPic");
		mAvatarHPMeterProgress = FindItem("MeterBarHPs");
		mAvatarHPTxt = FindItem("TxtMeterBarHPs");
		mStunTriggerNotch = FindItem("StunTriggerNotch");
	}

	private void SetStunTriggerNotch(float inPercentage)
	{
		if (!(mStunTriggerNotch == null))
		{
			float x = FindItem("AvatarXpMeterBkg").pBackground.cachedTransform.lossyScale.x;
			Vector3 localPosition = mStunTriggerNotch.transform.localPosition;
			localPosition.x = x * inPercentage - x * 0.5f;
			mStunTriggerNotch.transform.localPosition = localPosition;
		}
	}

	private void SetAvatarHP()
	{
		if (mAVAC == null && AvAvatar.pObject != null)
		{
			mAVAC = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if (mAVAC != null)
		{
			float currentHealth = mAVAC._Stats._CurrentHealth;
			float maxHealth = mAVAC._Stats._MaxHealth;
			mAvatarHPMeterProgress.SetProgressLevel(mAVAC._Stats._CurrentHealth / mAVAC._Stats._MaxHealth);
			mAvatarHPTxt.SetText(Mathf.CeilToInt(currentHealth) + " / " + (int)maxHealth);
		}
	}

	public void SetTime(string time)
	{
		if (mRaceTimeTxt != null)
		{
			mRaceTimeTxt.SetText(time);
		}
	}

	public void SetLap(string lap)
	{
		if (mLapCountTxt != null)
		{
			mLapCountTxt.SetText(lap);
		}
	}

	public void SetPosition(int pos)
	{
		if (mPositionsTxt != null)
		{
			mPositionsTxt.SetText(GameUtilities.FormatPosition(pos));
		}
	}

	public void SetMapFrameVisible(bool Visibility)
	{
		if (mBkgMapFrame != null)
		{
			mBkgMapFrame.SetVisibility(Visibility);
		}
	}

	public void ShowRaceStats(bool time, bool lap, bool position)
	{
		if (mRaceTimeTxt != null)
		{
			mRaceTimeTxt.SetVisibility(time);
		}
		if (mLapCountTxt != null)
		{
			mLapCountTxt.SetVisibility(lap);
		}
		if (mPositionsTxt != null)
		{
			mPositionsTxt.SetVisibility(position);
		}
	}

	public void SetUiVisibility(bool Visibility)
	{
		ShowRaceStats(Visibility, Visibility, Visibility);
		EnableBackButton(Visibility);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnBack")
		{
			mLevelManager.ShowQuitConfirmDB();
		}
		else if (inWidget.name == "BtnPause")
		{
			KAToggleButton kAToggleButton = (KAToggleButton)inWidget;
			mLevelManager.SetPausedState(kAToggleButton.IsChecked());
		}
		else if (inWidget.name == "BtnReset")
		{
			AvAvatar.TeleportToObject(ObAvatarRespawn._Marker);
		}
	}

	public void EnableBackButton(bool enable)
	{
		KAWidget kAWidget = FindItem("BtnBack");
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(enable);
		}
	}

	private void SetPlayerInfoDisplay()
	{
		DisplayName();
		SetAvatarHP();
		SetLevelandRankProgress();
		TakePlayerAShot();
	}

	public void TakePlayerAShot()
	{
		if (mPhotoManager == null)
		{
			mPhotoManager = AvPhotoManager.Init("PfToolbarPhotoMgr");
		}
		Texture2D texture2D = new Texture2D(256, 256, TextureFormat.ARGB32, mipChain: false);
		if (mPortrait != null)
		{
			Texture2D texture2D2 = (Texture2D)mPortrait.GetTexture();
			if (texture2D2 != null)
			{
				Color[] pixels = texture2D2.GetPixels();
				texture2D.SetPixels(pixels);
				texture2D.Apply();
			}
		}
		mPhotoManager.TakePhoto(UserInfo.pInstance.UserID, texture2D, ProfileAvPhotoCallback, null);
	}

	public void ProfileAvPhotoCallback(Texture tex, object inUserData)
	{
		if (mPortrait != null)
		{
			mPortrait.SetTexture(tex);
		}
	}

	private void DisplayName()
	{
		mAvatarNameTxt.SetText(AvatarData.pInstance.DisplayName);
	}

	private void SetLevelandRankProgress()
	{
		int value = UserRankData.pInstance.AchievementPointTotal.Value;
		UserRank userRankByValue = UserRankData.GetUserRankByValue(value);
		UserRank nextRank = UserRankData.GetNextRank(userRankByValue.RankID);
		float num = 0f;
		if (userRankByValue.RankID != nextRank.RankID)
		{
			num = ((float)value - (float)userRankByValue.Value) / ((float)nextRank.Value - (float)userRankByValue.Value);
			mAvatarXpMeterProgress.SetProgressLevel(num);
		}
		if (mAvatarLevelTxt != null)
		{
			mAvatarLevelTxt.SetText(UserRankData.pInstance.RankID.ToString());
		}
	}

	protected override void Update()
	{
		base.Update();
		SetAvatarHP();
		if (!mIsDragonMeterReady && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pInstance.pPetMeter != null)
		{
			mIsDragonMeterReady = true;
			AvAvatar.pToolbar = base.gameObject;
			SanctuaryManager.pInstance.pPetMeter.AttachToToolbar();
			SanctuaryManager.pInstance.pPetMeter.SetAllMeters();
			SanctuaryManager.pInstance.pPetMeter.SetPetName(SanctuaryManager.pCurPetInstance.pData.Name);
		}
		if (!(mChallengePointFlashTimer > 0f))
		{
			return;
		}
		mChallengePointFlashTimer -= 1f;
		if (mChallengePointFlashTimer <= 0f)
		{
			if (mChallengePointFlashTimes > 0)
			{
				ChallengeItemVisible(!GetChallengeItemVisibility());
				mChallengePointFlashTimes--;
				mChallengePointFlashTimer = mChallengePointFlashDuration;
			}
			else
			{
				ChallengeItemVisible(visible: false);
			}
		}
	}

	public void UpdateChallengePoints(int points)
	{
		if (mChallengePoints != null)
		{
			mChallengePoints.SetText(GameUtilities.FormatTime(points));
		}
	}

	public void ChallengeItemVisible(bool visible)
	{
		if (mChallengePoints != null && mChallengePoints.GetParentItem() != null)
		{
			mChallengePoints.GetParentItem().SetVisibility(visible);
		}
	}

	public bool GetChallengeItemVisibility()
	{
		if (mChallengePoints != null && mChallengePoints.GetParentItem() != null)
		{
			return mChallengePoints.GetParentItem().GetVisibility();
		}
		return false;
	}

	public void FlashChallengeItem(float interval, int loopTimes)
	{
		if (mChallengePoints != null && mChallengePoints.GetParentItem() != null)
		{
			mChallengePointFlashTimes = loopTimes;
			mChallengePointFlashDuration = interval;
			mChallengePointFlashTimer = interval;
		}
	}
}
