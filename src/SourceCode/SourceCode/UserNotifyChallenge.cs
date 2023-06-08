public class UserNotifyChallenge : UserNotify
{
	public int[] _TimeBasedGameIDs;

	private int mNumLostChallenges;

	private static bool mDoneOnce;

	public override void OnWaitBeginImpl()
	{
		if (!mDoneOnce)
		{
			mDoneOnce = true;
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			WsWebService.GetActiveChallenges(EventHandler, null);
		}
		else
		{
			OnWaitEnd();
		}
	}

	private void EventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_ACTIVE_CHALLENGES:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				ChallengeInfo[] array = (ChallengeInfo[])inObject;
				if (array != null)
				{
					ChallengeInfo[] array2 = array;
					foreach (ChallengeInfo challengeInfo in array2)
					{
						if (challengeInfo == null)
						{
							continue;
						}
						ChallengeContenderInfo[] challengeContenders = challengeInfo.ChallengeContenders;
						for (int j = 0; j < challengeContenders.Length; j++)
						{
							if (challengeContenders[j].ChallengeState != ChallengeState.Accepted)
							{
								continue;
							}
							mNumLostChallenges++;
							bool flag = false;
							int[] timeBasedGameIDs = _TimeBasedGameIDs;
							for (int k = 0; k < timeBasedGameIDs.Length; k++)
							{
								if (timeBasedGameIDs[k] == challengeInfo.ChallengeGameInfo.GameID)
								{
									flag = true;
								}
							}
							int messageID = (flag ? 606 : 606);
							int points = (flag ? (challengeInfo.Points + 1) : 0);
							int gamePointsType = ((!flag) ? 1 : 2);
							WsWebService.RespondToChallenge(challengeInfo.ChallengeID, messageID, points, gamePointsType, EventHandler, null);
						}
					}
				}
				if (mNumLostChallenges <= 0)
				{
					OnWaitEnd();
				}
				break;
			}
			case WsServiceEvent.ERROR:
				OnWaitEnd();
				break;
			}
			break;
		case WsServiceType.RESPOND_TO_CHALLENGE:
			if ((uint)(inEvent - 2) <= 1u)
			{
				mNumLostChallenges--;
				if (mNumLostChallenges <= 0)
				{
					OnWaitEnd();
				}
			}
			break;
		}
	}

	protected override void OnWaitEnd()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		base.OnWaitEnd();
	}
}
