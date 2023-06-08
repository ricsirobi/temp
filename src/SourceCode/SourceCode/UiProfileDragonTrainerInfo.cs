using System;
using UnityEngine;

public class UiProfileDragonTrainerInfo : KAUI
{
	public int _TrainerTitleCategoryID = 466;

	public string _TitleAttributeKey = "UDTTitle";

	public int[] _PetTypeIDs;

	public LocaleString _DayText = new LocaleString("Days");

	private UiProfileDragonTrainerTitleMenu mTrainerTitleMenu;

	public UiProfileDragonInfoMenu _DragonInfoMenu;

	private KAWidget mBtnUDTPoints;

	private KAWidget mTrainerTitleText;

	private bool mIsTitleChanged;

	protected override void Start()
	{
		base.Start();
		mTrainerTitleMenu = (UiProfileDragonTrainerTitleMenu)GetMenu("UiProfileDragonTrainerTitleMenu");
		mBtnUDTPoints = FindItem("BtnUDTPoints");
		mTrainerTitleText = FindItem("DragonTrainerTitleTxt");
	}

	private void OnSetVisibility(bool t)
	{
		SetVisibility(t);
	}

	private void OnCloseUI()
	{
		if (mIsTitleChanged)
		{
			AvatarData.Save();
		}
	}

	public void ProfileDataReady(UserProfile p)
	{
		SetUDTPoints();
		if (_DragonInfoMenu != null)
		{
			_DragonInfoMenu.Init();
		}
		UiProfileDragonTrainerTitleMenu uiProfileDragonTrainerTitleMenu = mTrainerTitleMenu;
		uiProfileDragonTrainerTitleMenu.onItemSelected = (KAUIDropDownMenu.OnItemSelected)Delegate.Combine(uiProfileDragonTrainerTitleMenu.onItemSelected, new KAUIDropDownMenu.OnItemSelected(OnTrainerTitleSelect));
		mTrainerTitleMenu.Init(this);
		AvatarPartAttribute titleAttribute = GetTitleAttribute();
		if (titleAttribute != null)
		{
			mTrainerTitleText.SetText(titleAttribute.Value);
		}
		else
		{
			mTrainerTitleText.SetText("--");
		}
	}

	private void SetUDTPoints()
	{
		if (!(mBtnUDTPoints == null))
		{
			UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(12);
			if (!IsCurrentPlayer())
			{
				userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(UiProfile.pUserProfile.AvatarInfo.Achievements, 12);
			}
			if (userAchievementInfoByType != null)
			{
				mBtnUDTPoints.SetText(userAchievementInfoByType.AchievementPointTotal.Value.ToString());
			}
			else
			{
				mBtnUDTPoints.SetText("0");
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (KAInput.GetMouseButtonUp(0) && IsCurrentPlayer() && mTrainerTitleMenu != null && mTrainerTitleMenu.GetVisibility() && !mTrainerTitleMenu.BoundsCheck() && KAUI._GlobalExclusiveUI == this)
		{
			UpdateTrainerTitleMenuState();
		}
	}

	private void UpdateTrainerTitleMenuState()
	{
		mTrainerTitleMenu.UpdateState(!mTrainerTitleMenu.GetVisibility());
		if (mTrainerTitleMenu.GetVisibility())
		{
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, base.transform.localPosition.z - 5f);
			KAUI.SetExclusive(this);
		}
		else
		{
			KAUI.RemoveExclusive(this);
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, base.transform.localPosition.z + 5f);
		}
		KAInput.ResetInputAxes();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mTrainerTitleText)
		{
			if (mTrainerTitleMenu != null && IsCurrentPlayer())
			{
				UpdateTrainerTitleMenuState();
			}
		}
		else if (inWidget.name == "BtnUDTPoints" && IsCurrentPlayer())
		{
			if (UiJournal.pInstance == null)
			{
				AvAvatar.pState = AvAvatarState.IDLE;
			}
			JournalLoader.Load("UDTLeaderBoardBtn", "", setDefaultMenuItem: false, base.gameObject, resetLastSceneRef: false);
		}
	}

	public void OnTrainerTitleSelect(KAWidget widget, KAUIDropDownMenu dropDown)
	{
		if (dropDown is UiProfileDragonTrainerTitleMenu && mTrainerTitleText.GetText() != widget.GetText())
		{
			mTrainerTitleText.SetText(widget.GetText());
			SetTitle(widget.GetText());
			mIsTitleChanged = true;
		}
		KAUI.RemoveExclusive(this);
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, base.transform.localPosition.z + 5f);
	}

	private AvatarPartAttribute GetTitleAttribute()
	{
		AvatarDataPart avatarDataPart = new AvatarData.InstanceInfo
		{
			mInstance = UiProfile.pUserProfile.AvatarInfo.AvatarData
		}.FindPart("Version");
		if (avatarDataPart != null)
		{
			AvatarPartAttribute[] attributes = avatarDataPart.Attributes;
			foreach (AvatarPartAttribute avatarPartAttribute in attributes)
			{
				if (avatarPartAttribute.Key == _TitleAttributeKey)
				{
					return avatarPartAttribute;
				}
			}
		}
		return null;
	}

	private void SetTitle(string inTitle)
	{
		AvatarPartAttribute avatarPartAttribute = new AvatarPartAttribute();
		avatarPartAttribute.Key = _TitleAttributeKey;
		avatarPartAttribute.Value = inTitle;
		AvatarData.SetAttribute(AvatarData.pInstanceInfo, "Version", avatarPartAttribute);
	}

	public bool IsCurrentPlayer()
	{
		if (UiProfile.pUserProfile == null || UserInfo.pInstance == null)
		{
			return false;
		}
		return UiProfile.pUserProfile.UserID == UserInfo.pInstance.UserID;
	}

	private void RefreshUI()
	{
		if (_DragonInfoMenu != null)
		{
			_DragonInfoMenu.ReInit();
		}
	}
}
