using System.Collections.Generic;
using UnityEngine;

public class UiDailyQuestRewards : KAUI
{
	public KAWidget _ChestWidget;

	public KAWidget _ItemWidget;

	public Vector3 _RewardEndScale;

	public Vector3 _RewardEndPos;

	public float _RewardMoveDuration;

	public string _OpenAnimation;

	public string _RollOverCursorName;

	public float _RewardDisappearTime = 5f;

	public bool _DisableRewardAnim = true;

	private List<AchievementReward> mDailyRewards;

	private Animation mChestAnim;

	private bool mDoInit;

	private bool mIsChestAnimPlaying;

	private bool mIsChestOpened;

	private float mRewardDisappearTimer;

	private bool mIsWidgetMoving;

	protected override void Update()
	{
		base.Update();
		if (mDoInit)
		{
			mDoInit = false;
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			if (_ItemWidget != null && !_DisableRewardAnim)
			{
				_ItemWidget.OnMoveToDone += OnWidgetMoveDone;
			}
		}
		else if (mIsChestAnimPlaying)
		{
			if (!mChestAnim.IsPlaying(_OpenAnimation))
			{
				mIsChestAnimPlaying = false;
				OnChestOpened();
			}
		}
		else if (Input.GetMouseButtonUp(0))
		{
			if (!mIsChestOpened)
			{
				OpenChest();
			}
			else if (WidgetMovementAllowed())
			{
				UpdateRewardWidget();
			}
		}
		else if (mRewardDisappearTimer > 0f)
		{
			mRewardDisappearTimer -= Time.deltaTime;
			if (mRewardDisappearTimer <= 0f && _ItemWidget.GetVisibility() && !mIsWidgetMoving)
			{
				_ItemWidget.SetVisibility(inVisible: false);
				OnChestOpened();
			}
		}
	}

	private void UpdateRewardWidget()
	{
		if (_DisableRewardAnim)
		{
			_ItemWidget.SetVisibility(inVisible: false);
			OnChestOpened();
		}
		else
		{
			_ItemWidget.ScaleTo(_ItemWidget.GetScale(), _RewardEndScale, _RewardMoveDuration);
			_ItemWidget.MoveTo(_RewardEndPos, _RewardMoveDuration);
			mIsWidgetMoving = true;
		}
	}

	private bool WidgetMovementAllowed()
	{
		if (!_DisableRewardAnim && _ItemWidget.GetVisibility())
		{
			return !mIsWidgetMoving;
		}
		return false;
	}

	public override void OnHover(KAWidget inWidget, bool inHover)
	{
		if (inWidget == _ItemWidget || inWidget == _ChestWidget)
		{
			if (inHover && _RollOverCursorName.Length > 0 && !UICursorManager.GetCursorName().Equals(_RollOverCursorName))
			{
				UICursorManager.SetCursor(_RollOverCursorName, showHideSystemCursor: true);
			}
			else if (!inHover && !UICursorManager.GetCursorName().Equals("Arrow"))
			{
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			}
		}
		base.OnHover(inWidget, inHover);
	}

	private void OpenChest()
	{
		_ChestWidget.SetDisabled(isDisabled: true);
		mIsChestOpened = true;
		mChestAnim = _ChestWidget.GetComponentInChildren<Animation>();
		if (mChestAnim != null)
		{
			mChestAnim.Play(_OpenAnimation);
			if (mChestAnim.IsPlaying(_OpenAnimation))
			{
				mIsChestAnimPlaying = true;
			}
			else
			{
				OnChestOpened();
			}
		}
		else
		{
			OnChestOpened();
		}
	}

	private void OnWidgetMoveDone(object item)
	{
		_ItemWidget.SetVisibility(inVisible: false);
		_ItemWidget.SetPosition(_ItemWidget.pOrgPosition.x, _ItemWidget.pOrgPosition.y);
		_ItemWidget.SetScale(_ItemWidget.pOrgScale);
		mIsWidgetMoving = false;
		OnChestOpened();
	}

	private void OnChestOpened()
	{
		if (_ChestWidget.gameObject.activeSelf)
		{
			_ChestWidget.gameObject.SetActive(value: false);
		}
		if (mDailyRewards != null && mDailyRewards.Count > 0)
		{
			AchievementReward[] inRewards = new AchievementReward[1] { mDailyRewards[0] };
			RewardWidget component = _ItemWidget.GetComponent<RewardWidget>();
			if (component != null)
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				component.SetRewards(inRewards, MissionManager.pInstance._RewardData, null, OnSetReward);
			}
			mDailyRewards.RemoveAt(0);
		}
		else
		{
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			Object.Destroy(base.gameObject);
		}
	}

	public void Init(SetTimedMissionTaskStateResult missionResult)
	{
		mDoInit = true;
		mDailyRewards = new List<AchievementReward>(missionResult.DailyReward);
	}

	public void OnSetReward(RewardWidget.SetRewardStatus inSetRewardStatus)
	{
		if (inSetRewardStatus == RewardWidget.SetRewardStatus.COMPLETE)
		{
			mRewardDisappearTimer = _RewardDisappearTime;
			_ItemWidget.SetVisibility(inVisible: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}
}
