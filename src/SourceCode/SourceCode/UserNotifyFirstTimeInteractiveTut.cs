using System;

public class UserNotifyFirstTimeInteractiveTut : UserNotify
{
	public int _FirstMissionID = 999;

	public InteractiveTutManager _TutCommon;

	public InteractiveTutManager _TutMobile;

	public InteractiveTutManager _TutOnline;

	private bool mBeginImpl;

	private InteractiveTutManager TutorialPending()
	{
		bool flag = false;
		Mission mission = MissionManager.pInstance.GetMission(_FirstMissionID);
		if (mission != null && (mission.Accepted || mission.pCompleted))
		{
			flag = true;
		}
		if ((_TutCommon != null && _TutCommon.TutorialComplete()) || flag)
		{
			if (UtPlatform.IsMobile())
			{
				if (!_TutMobile.TutorialComplete())
				{
					return _TutMobile;
				}
				return null;
			}
			if (UtPlatform.IsWSA())
			{
				if (UtUtilities.IsKeyboardAttached())
				{
					if (!_TutOnline.TutorialComplete())
					{
						return _TutOnline;
					}
					return null;
				}
				if (!_TutMobile.TutorialComplete())
				{
					return _TutMobile;
				}
				return null;
			}
			if (!_TutOnline.TutorialComplete())
			{
				return _TutOnline;
			}
			return null;
		}
		return _TutCommon;
	}

	public override void OnWaitBeginImpl()
	{
		mBeginImpl = true;
		InteractiveTutManager interactiveTutManager = TutorialPending();
		if (interactiveTutManager != null)
		{
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.ActivateAll(active: false);
			}
			if (interactiveTutManager == _TutCommon)
			{
				FirstTimeInteractiveTut firstTimeInteractiveTut = (FirstTimeInteractiveTut)_TutCommon;
				if (UtPlatform.IsMobile())
				{
					InteractiveTutManager nextMobileTut = firstTimeInteractiveTut._NextMobileTut;
					nextMobileTut._WaitEnd = (WaitEndEvent)Delegate.Combine(nextMobileTut._WaitEnd, new WaitEndEvent(OnWaitEnd));
				}
				else if (UtPlatform.IsWSA())
				{
					if (UtUtilities.IsKeyboardAttached())
					{
						InteractiveTutManager nextOnlineTut = firstTimeInteractiveTut._NextOnlineTut;
						nextOnlineTut._WaitEnd = (WaitEndEvent)Delegate.Combine(nextOnlineTut._WaitEnd, new WaitEndEvent(OnWaitEnd));
					}
					else
					{
						InteractiveTutManager nextMobileTut2 = firstTimeInteractiveTut._NextMobileTut;
						nextMobileTut2._WaitEnd = (WaitEndEvent)Delegate.Combine(nextMobileTut2._WaitEnd, new WaitEndEvent(OnWaitEnd));
					}
				}
				else
				{
					InteractiveTutManager nextOnlineTut2 = firstTimeInteractiveTut._NextOnlineTut;
					nextOnlineTut2._WaitEnd = (WaitEndEvent)Delegate.Combine(nextOnlineTut2._WaitEnd, new WaitEndEvent(OnWaitEnd));
				}
			}
			else
			{
				interactiveTutManager._WaitEnd = (WaitEndEvent)Delegate.Combine(interactiveTutManager._WaitEnd, new WaitEndEvent(OnWaitEnd));
			}
			interactiveTutManager.gameObject.SetActive(value: true);
			interactiveTutManager.ShowTutorial();
		}
		else
		{
			OnWaitEnd();
		}
	}

	public void Update()
	{
		if (mBeginImpl)
		{
			ObClickable.pGlobalActive = false;
		}
	}

	private new void OnWaitEnd()
	{
		mBeginImpl = false;
		ObClickable.pGlobalActive = true;
		KAUICursorManager.SetDefaultCursor("Arrow");
		AvAvatar.pState = AvAvatarState.IDLE;
		base.OnWaitEnd();
	}
}
