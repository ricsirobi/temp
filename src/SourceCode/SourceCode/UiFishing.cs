using System;
using System.Collections.Generic;
using UnityEngine;

public class UiFishing : KAUI
{
	[Serializable]
	public class FishingRewardData
	{
		public int _PointType;

		public string _IconName;
	}

	public Vector3 _BaitPointerOffset = new Vector2(-15f, -15f);

	public Vector3 _CastPointerOffset = new Vector2(-67f, 56f);

	public Vector3 _FishModelPosition = new Vector3(123f, 17f, -48f);

	public Vector3 _FishModelScale = new Vector3(200f, 200f, 200f);

	public Vector3 _FishModelRotation = new Vector3(0f, 90f, 0f);

	public GameObject _MetersUi;

	public List<FishingRewardData> _FishingRewards;

	private float mBarWidth;

	private KAWidget mMarker;

	private KAWidget mBaitLoseZone;

	private KAWidget mLineSnapZone;

	private KAWidget mBarBg;

	private KAWidget mBtnStartFishing;

	private KAWidget mBtnStopFishing;

	private KAWidget mBtnFishingRod;

	private KAWidget mBtnSelectBait;

	private KAWidget mBtnReel;

	private KAWidget mBtnDrag;

	private KAWidget mBtnStrike;

	private KAWidget mReelGear;

	private KAWidget mReelGearFrame;

	private KAWidget mFishName;

	private KAWidget mCurrentState;

	private KAWidget mReelBG;

	private KAWidget mAvatarHPMeterProgress;

	private KAWidget mAvatarHPTxt;

	private KAWidget mAvatarLevel;

	private KAWidget mAvatarName;

	private KAWidget mAvatarXpMeterProgressBar;

	private KAWidget mPortrait;

	private FishingZone mZone;

	private KAWidget mAnim;

	private bool mShowReelbar;

	private bool mReelPressed;

	private bool mReelClicked;

	private bool mShowFishingRod;

	private bool mDragPressed;

	private GameObject mFishModel;

	public bool pDragPressed => mDragPressed;

	protected override void Start()
	{
		base.Start();
		mBaitLoseZone = FindItem("FishingBaitLoseZone");
		mLineSnapZone = FindItem("FishingLineSnapZone");
		mMarker = FindItem("FishingMarker");
		mBtnStartFishing = FindItem("BtnStartFishing");
		mBtnStopFishing = FindItem("BtnStopFishing");
		mBtnFishingRod = FindItem("BtnFishingRod");
		mBtnSelectBait = FindItem("BtnSelectBait");
		mBtnReel = FindItem("BtnReel");
		mBtnStrike = FindItem("BtnStrike");
		mBarBg = FindItem("FishingBG");
		mReelGear = FindItem("FishingReelGear");
		mReelGearFrame = FindItem("FishingReelGearFrame");
		mFishName = FindItem("FishName");
		mAnim = FindItem("FishAnim");
		mCurrentState = FindItem("CurrentState");
		mBtnDrag = FindItem("BtnDrag");
		mReelBG = FindItem("FishingReelBkg");
		mBarWidth = mBarBg.pBackground.width;
		mBtnStartFishing.SetVisibility(inVisible: false);
		mBtnStopFishing.SetVisibility(inVisible: false);
		mBtnReel.SetVisibility(inVisible: false);
		mBtnSelectBait.SetVisibility(inVisible: false);
		mReelPressed = false;
		mDragPressed = false;
		mReelClicked = false;
		ShowReelbar(show: false, 0f, 0f);
		SetFishName(null);
		ShowFishingRodUI(bShow: false, reel: false, strike: false);
		ShowStrikePopupText(show: false);
		mBtnStrike.SetVisibility(inVisible: false);
		mBtnReel.SetVisibility(inVisible: false);
		ShowBaitPointer(show: false);
		ShowCastPointer(show: false);
		ShowUDTStatus();
	}

	protected override void Update()
	{
		base.Update();
		if (Application.isEditor)
		{
			DrawDebugLines();
		}
		CharacterController component = AvAvatar.pObject.GetComponent<CharacterController>();
		if (!(mBtnFishingRod != null))
		{
			return;
		}
		if (AvAvatar.pToolbar != null && AvAvatar.pToolbar.activeSelf && component != null && (component.isGrounded || SanctuaryManager.pCurPetInstance == null || SanctuaryManager.pCurPetInstance.pIsMounted) && ObContextSensitive.pExclusiveUI == null && UiBuddyList.pInstance == null)
		{
			if (mShowFishingRod != mBtnFishingRod.GetVisibility())
			{
				mBtnFishingRod.SetVisibility(mShowFishingRod);
			}
		}
		else if (mBtnFishingRod.GetVisibility())
		{
			mBtnFishingRod.SetVisibility(inVisible: false);
		}
	}

	public void ShowReelbar(bool show, float baitLoseWidth, float lineSnapWidth)
	{
		mShowReelbar = show;
		if (null != mBarBg)
		{
			mBarBg.SetVisibility(show);
		}
		if (null != mBaitLoseZone)
		{
			mBaitLoseZone.SetVisibility(show);
		}
		if (null != mLineSnapZone)
		{
			mLineSnapZone.SetVisibility(show);
		}
		if (null != mMarker)
		{
			mMarker.SetVisibility(show);
		}
		if (null != mAnim)
		{
			mAnim.SetVisibility(show);
		}
		if (null != mFishName)
		{
			mFishName.SetVisibility(show);
		}
		if (null != mBtnDrag)
		{
			mBtnDrag.SetVisibility(show);
			if (null != mReelBG)
			{
				mReelBG.SetVisibility(show);
			}
		}
		if (null != mZone._CurrentFishingRod && show)
		{
			FishingRod component = mZone._CurrentFishingRod.GetComponent<FishingRod>();
			if (null != mBtnDrag)
			{
				mBtnDrag.SetVisibility(component._AllowDrag);
			}
			if (null != mReelBG)
			{
				mReelBG.SetVisibility(component._AllowDrag);
			}
		}
		if (show)
		{
			SetZoneWidth(baitLoseWidth, lineSnapWidth);
		}
		if (null != mFishModel && show)
		{
			mFishModel.SetActive(show);
			mFishModel.GetComponent<Animation>().Play("Idle");
		}
		mReelPressed = false;
		mDragPressed = false;
	}

	public void SetZoneColors(Color baitLoseColor, Color lineSnapColor)
	{
		mBaitLoseZone.pBackground.color = baitLoseColor;
		mLineSnapZone.pBackground.color = lineSnapColor;
	}

	public void SetZoneWidth(float baitLoseWidth, float lineSnapWidth)
	{
		float num = baitLoseWidth * mBarWidth;
		mBaitLoseZone.pBackground.width = (int)num;
		Vector3 position = mBaitLoseZone.transform.position;
		position.x = mBarBg.transform.position.x - mBarWidth / 2f + num / 2f;
		mBaitLoseZone.transform.position = position;
		float num2 = lineSnapWidth * mBarWidth;
		mLineSnapZone.pBackground.width = (int)num2;
		position = mLineSnapZone.transform.position;
		position.x = mBarBg.transform.position.x + mBarWidth / 2f - num2 / 2f;
		mLineSnapZone.transform.position = position;
	}

	public void SetMarkerPosition(float mPercentage)
	{
		Vector3 localPosition = mMarker.transform.localPosition;
		localPosition.x = mBarWidth * mPercentage - mBarWidth * 0.5f;
		mMarker.transform.localPosition = localPosition;
	}

	public void RotateReelGear(float speedAngle)
	{
		mReelGear.transform.Rotate(Vector3.forward, speedAngle * 20f);
	}

	public void UpdateZoneColor(bool baitLose, float currentTime, float totalTime)
	{
		Color color = mBaitLoseZone.pBackground.color;
		if (!baitLose)
		{
			color = mLineSnapZone.pBackground.color;
		}
		if (currentTime > 0f)
		{
			color.a = Mathf.Lerp(color.a, 1f, currentTime / totalTime);
		}
		else
		{
			color.a = Mathf.Lerp(color.a, 0.2f, 1f);
		}
		if (baitLose)
		{
			mBaitLoseZone.pBackground.color = color;
		}
		else
		{
			mLineSnapZone.pBackground.color = color;
		}
	}

	public void SetFishName(string fishName)
	{
		if (!(null == mFishName))
		{
			if (string.IsNullOrEmpty(fishName))
			{
				mFishName.SetVisibility(inVisible: false);
				return;
			}
			mFishName.SetVisibility(inVisible: true);
			mFishName.SetText(fishName);
		}
	}

	public void SetFishingZone(FishingZone zone)
	{
		mZone = zone;
	}

	public void ShowStartFishingButton(bool show)
	{
		if (null != mBtnStartFishing)
		{
			mBtnStartFishing.SetVisibility(show);
		}
	}

	public void ShowStopFishingButton(bool show)
	{
		if (null != mBtnStopFishing)
		{
			mBtnStopFishing.SetVisibility(show);
		}
	}

	public void ShowFishingRodButton(bool show)
	{
		mShowFishingRod = show;
	}

	public void ShowStrikeButton(bool show)
	{
		mBtnStrike.SetVisibility(show);
	}

	public void AnimateStrikeButton(bool bAnim)
	{
		if (bAnim)
		{
			mBtnStrike.PlayAnim("Flash");
		}
		else
		{
			mBtnStrike.StopAnim("Flash");
		}
	}

	public void AnimateReelButton(bool bAnim)
	{
		if (bAnim)
		{
			mBtnReel.PlayAnim("Flash");
		}
		else
		{
			mBtnReel.StopAnim("Flash");
		}
	}

	public void AnimateStartFishingButton(bool bAnim)
	{
		if (bAnim)
		{
			mBtnStartFishing.PlayAnim("Flash");
		}
		else
		{
			mBtnStartFishing.StopAnim("Flash");
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mBtnStartFishing == inWidget)
		{
			mZone.StartFishing();
		}
		else if (mBtnStopFishing == inWidget)
		{
			if (_MetersUi != null)
			{
				_MetersUi.SetActive(value: false);
			}
			mZone.StopFishing();
			StopTut();
			mBtnStopFishing.SetVisibility(inVisible: false);
		}
		else if (mBtnFishingRod == inWidget)
		{
			if (_MetersUi != null)
			{
				_MetersUi.SetActive(value: true);
				UpdateAvatarMeters();
				TakePicture();
			}
			if (mBtnStopFishing != null)
			{
				mBtnStopFishing.SetVisibility(inVisible: true);
			}
			mBtnFishingRod.SetVisibility(inVisible: false);
			mZone.EquipFishingRod();
		}
		else if (mBtnSelectBait == inWidget)
		{
			mZone.RandomBait();
		}
		else if (mBtnReel == inWidget || mBtnStrike == inWidget)
		{
			mReelClicked = true;
		}
	}

	public void SetReelPressed(bool isPress)
	{
		mReelPressed = isPress;
	}

	public void SetReelClicked(bool isClicked)
	{
		mReelClicked = isClicked;
	}

	public bool IsReelPressed()
	{
		return mReelPressed;
	}

	public bool IsReelClicked()
	{
		return mReelClicked;
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		base.OnPressRepeated(inWidget, inPressed);
		if (mBtnReel == inWidget)
		{
			mReelPressed = inPressed;
			return;
		}
		if (mBtnDrag == inWidget)
		{
			mDragPressed = inPressed;
			return;
		}
		mReelPressed = false;
		mDragPressed = false;
	}

	public void PlayFishAnim(string name)
	{
		if (null != mFishModel)
		{
			mFishModel.GetComponent<Animation>().Play(name, PlayMode.StopAll);
		}
	}

	public void SetStateText(string text)
	{
		if (null != mCurrentState)
		{
			mCurrentState.SetText(text);
		}
	}

	public void ShowStrikePopupText(bool show)
	{
		KAWidget kAWidget = FindItem("FishingStrikePopUp");
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(show);
		}
		SetStateText("");
	}

	public void ShowFishingRodUI(bool bShow, bool reel, bool strike)
	{
		if (mReelGear != null)
		{
			mReelGear.SetVisibility(bShow);
		}
		if (mReelGearFrame != null)
		{
			mReelGearFrame.SetVisibility(bShow);
		}
		ShowStrikeButton(reel);
		ShowReelButton(strike);
	}

	public void ShowReelButton(bool show)
	{
		mBtnReel.SetVisibility(show);
	}

	private void DrawDebugLines()
	{
		if (!(null == mBaitLoseZone) && !(null == mLineSnapZone))
		{
			float x = mBaitLoseZone.GetPosition().x + (float)mBaitLoseZone.pBackground.width / 2f;
			float y = mBaitLoseZone.GetPosition().y + (float)mBaitLoseZone.pBackground.height / 2f + 10f;
			float y2 = mBaitLoseZone.GetPosition().y - (float)mBaitLoseZone.pBackground.height / 2f - 10f;
			Debug.DrawLine(new Vector3(x, y, 0f), new Vector3(x, y2, 0f), Color.green);
			x = mLineSnapZone.GetPosition().x - (float)mLineSnapZone.pBackground.width / 2f;
			y = mLineSnapZone.GetPosition().y + (float)mLineSnapZone.pBackground.height / 2f + 10f;
			y2 = mLineSnapZone.GetPosition().y - (float)mLineSnapZone.pBackground.height / 2f - 10f;
			Debug.DrawLine(new Vector3(x, y, 0f), new Vector3(x, y2, 0f), Color.green);
		}
	}

	public void ShowBaitPointer(bool show)
	{
		KAWidget kAWidget = FindItem("AniBaitPointer");
		if ((bool)kAWidget)
		{
			kAWidget.SetVisibility(show);
		}
	}

	public void ShowCastPointer(bool show)
	{
		KAWidget kAWidget = FindItem("AniCastPointer");
		if ((bool)kAWidget)
		{
			kAWidget.SetVisibility(show);
		}
	}

	public void SetPositionCastPointer()
	{
		KAWidget kAWidget = FindItem("AniCastPointer");
		if ((bool)kAWidget)
		{
			kAWidget.SetPosition(mBtnStartFishing.GetPosition().x + _CastPointerOffset.x, mBtnStartFishing.GetPosition().y + _CastPointerOffset.y);
		}
	}

	public void UpdateBaitPointer(Vector3 baitBarrelPosition)
	{
		KAWidget kAWidget = FindItem("AniBaitPointer");
		Vector3 vector = Camera.main.WorldToScreenPoint(baitBarrelPosition);
		kAWidget.SetToScreenPosition(vector.x + _BaitPointerOffset.x, (float)Screen.height - vector.y + _BaitPointerOffset.y);
	}

	public void ShowFish(GameObject fish)
	{
		mFishModel = fish;
		if (!(null == mFishModel))
		{
			mFishModel.transform.parent = mAnim.transform;
			mFishModel.transform.localPosition = _FishModelPosition;
			mFishModel.transform.rotation = Quaternion.Euler(_FishModelRotation);
			UtUtilities.SetLayerRecursively(mFishModel, mAnim.gameObject.layer);
			mFishModel.transform.localScale = _FishModelScale;
			if (mShowReelbar)
			{
				mFishModel.SetActive(value: true);
				mFishModel.GetComponent<Animation>().Play("Idle");
			}
			else
			{
				mFishModel.SetActive(value: false);
			}
		}
	}

	public void RemoveFish(bool hideOnly = false)
	{
		if (hideOnly)
		{
			mFishModel.SetActive(value: false);
		}
		else if (null != mFishModel)
		{
			UnityEngine.Object.Destroy(mFishModel);
		}
	}

	public void UpdateAvatarMeters()
	{
		if (mAvatarHPMeterProgress == null)
		{
			mAvatarHPMeterProgress = FindItem("MeterBarHPs");
		}
		if (mAvatarHPTxt == null)
		{
			mAvatarHPTxt = FindItem("TxtMeterBarHPs");
		}
		if (mAvatarLevel == null)
		{
			mAvatarLevel = FindItem("TxtAvatarLevel");
		}
		if (mAvatarName == null)
		{
			mAvatarName = FindItem("TxtAvatarName");
		}
		if (mAvatarName != null && AvatarData.pInstance != null)
		{
			mAvatarName.SetText(AvatarData.pInstance.DisplayName);
		}
		if (mAvatarXpMeterProgressBar == null)
		{
			mAvatarXpMeterProgressBar = FindItem("AvatarXpMeter");
		}
		if (mAvatarXpMeterProgressBar != null)
		{
			mAvatarXpMeterProgressBar.SetProgressLevel(0f);
		}
		SetAvatarHP();
		if (UserRankData.pInstance == null)
		{
			return;
		}
		UserRank userRankByType = UserRankData.GetUserRankByType(10);
		if (userRankByType != null)
		{
			if (mAvatarLevel != null)
			{
				mAvatarLevel.SetText(userRankByType.RankID.ToString());
			}
			UserRank nextRankByType = UserRankData.GetNextRankByType(10, userRankByType.RankID);
			UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(10);
			if (nextRankByType != null && userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue)
			{
				float progressLevel = 1f;
				if (userRankByType.RankID != nextRankByType.RankID)
				{
					progressLevel = ((float)userAchievementInfoByType.AchievementPointTotal.Value - (float)userRankByType.Value) / (float)(nextRankByType.Value - userRankByType.Value);
				}
				if (mAvatarXpMeterProgressBar != null)
				{
					mAvatarXpMeterProgressBar.SetProgressLevel(progressLevel);
				}
				else
				{
					UtDebug.LogError("mAvatarXpMeterProgressBar is NULL");
				}
			}
		}
		ShowUDTStatus();
	}

	private void SetAvatarHP()
	{
		if (!(AvAvatar.pObject == null))
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (!(component == null))
			{
				mAvatarHPMeterProgress.SetProgressLevel(component._Stats._CurrentHealth / component._Stats._MaxHealth);
				mAvatarHPTxt.SetText(Mathf.CeilToInt(component._Stats._CurrentHealth) + " / " + (int)component._Stats._MaxHealth);
			}
		}
	}

	public void TakePicture()
	{
		if (mPortrait == null)
		{
			mPortrait = FindItem("AvatarPic");
		}
		AvPhotoManager avPhotoManager = AvPhotoManager.Init("PfToolbarPhotoMgr");
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
		avPhotoManager.TakePhoto(UserInfo.pInstance.UserID, texture2D, ProfileAvPhotoCallback, null);
	}

	public void ProfileAvPhotoCallback(Texture tex, object inUserData)
	{
		if (mPortrait != null)
		{
			mPortrait.SetTexture(tex);
		}
	}

	private void StopTut()
	{
		AnimateStrikeButton(bAnim: false);
		AnimateReelButton(bAnim: false);
		AnimateStartFishingButton(bAnim: false);
		ShowBaitPointer(show: false);
		ShowCastPointer(show: false);
	}

	private void ShowUDTStatus()
	{
		KAWidget kAWidget = FindItem("UDTStarsIcon");
		KAWidget kAWidget2 = FindItem("TxtUDTPoints");
		if (kAWidget != null)
		{
			UDTUtilities.UpdateUDTStars(kAWidget.transform, "ToolbarStarsIconBkg", "ToolbarStarsIconFrameBkg");
		}
		UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(12);
		if (!(kAWidget2 == null))
		{
			kAWidget2.SetText((userAchievementInfoByType != null) ? userAchievementInfoByType.AchievementPointTotal.Value.ToString() : "0");
		}
	}
}
