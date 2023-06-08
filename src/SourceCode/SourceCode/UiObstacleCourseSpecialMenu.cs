using System;

public class UiObstacleCourseSpecialMenu : UiLevelGroupSelectMenuDragons
{
	public override void OnClick(KAWidget item)
	{
		mLastPlayedLevelNum = mLevelManager.pLastUnlockedHeroLevel;
		base.OnClick(item);
		int selectedItemIndex = GetSelectedItemIndex();
		mLevelManager.mIsCurrentLevelSpecial = true;
		if (!mIsLevelReady || !SanctuaryManager.pCurPetInstance.IsActionAllowed(PetActions.FLIGHTSCHOOL))
		{
			return;
		}
		TimeSpan timeSpan = ServerTime.pCurrentTime - mLevelManager.pLastPlayedTime;
		if (selectedItemIndex != mLevelManager.pLastUnlockedHeroLevel || mLevelManager.pLastUnlockedHeroLevel < mLevelManager._InitialNonMemberUnlockedLevel || !(timeSpan.TotalMinutes < (double)mLevelManager._LevelUnlockTimeInMinutes) || SubscriptionInfo.pIsMember || selectedItemIndex == 0)
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
