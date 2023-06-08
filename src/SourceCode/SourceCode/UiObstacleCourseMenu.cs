using System;

public class UiObstacleCourseMenu : UiLevelGroupSelectMenuDragons
{
	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		mLevelManager.pLevelMenu = this;
	}

	public override void OnClick(KAWidget item)
	{
		mLastPlayedLevelNum = mLevelManager.pLastUnlockedLevel;
		base.OnClick(item);
		int selectedItemIndex = GetSelectedItemIndex();
		if (!mIsLevelReady || !SanctuaryManager.pCurPetInstance.IsActionAllowed(PetActions.FLIGHTSCHOOL))
		{
			return;
		}
		TimeSpan timeSpan = ServerTime.pCurrentTime - mLevelManager.pLastPlayedTime;
		if (selectedItemIndex != mLevelManager.pLastUnlockedLevel || mLevelManager.pLastUnlockedLevel < mLevelManager._InitialNonMemberUnlockedLevel || !(timeSpan.TotalMinutes < (double)mLevelManager._LevelUnlockTimeInMinutes) || SubscriptionInfo.pIsMember || selectedItemIndex == 0)
		{
			mLevelManager.pCurrentLevel = selectedItemIndex;
			TutorialManager.StopTutorials();
			if (_MainUI != null)
			{
				_MainUI.gameObject.SetActive(value: false);
			}
		}
	}
}
