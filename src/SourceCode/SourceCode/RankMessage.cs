internal class RankMessage : GenericMessage
{
	private bool mReinitUserRank = true;

	private bool mUiUpdated;

	private int mRankType = -1;

	private UserRank mCurrentRank;

	private string mParticleURL = string.Empty;

	private string mTitleText;

	private string mMessageText;

	public RankMessage(MessageInfo messageInfo)
		: base(messageInfo)
	{
		Start();
	}

	private void Start()
	{
		TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(mMessageInfo);
		if (taggedMessageHelper.MemberMessage.ContainsKey("Particle"))
		{
			mParticleURL = taggedMessageHelper.MemberMessage["Particle"];
		}
		if (taggedMessageHelper.MemberMessage.ContainsKey("Line1"))
		{
			mTitleText = taggedMessageHelper.MemberMessage["Line1"];
		}
		if (taggedMessageHelper.MemberMessage.ContainsKey("Line2"))
		{
			mMessageText = taggedMessageHelper.MemberMessage["Line2"];
		}
		mMessageInfo.MemberMessage = mTitleText + " " + mMessageText;
		if (taggedMessageHelper.MemberMessage.ContainsKey("Type"))
		{
			string text = taggedMessageHelper.MemberMessage["Type"];
			if (!string.IsNullOrEmpty(text))
			{
				int.TryParse(text, out mRankType);
			}
		}
	}

	public override void Show()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		WsUserMessage.pInstance.ReInitUserRankData();
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetBusy(busy: true);
		}
	}

	public override void Update()
	{
		if (mReinitUserRank && UserRankData.pIsReady)
		{
			mReinitUserRank = false;
			mCurrentRank = UserRankData.GetUserRankByType(mRankType);
		}
		if (!mUiUpdated && !mReinitUserRank)
		{
			mUiUpdated = true;
			KAUICursorManager.SetDefaultCursor("Arrow");
			PlayParticleEffect();
			Save(delete: false);
			base.Show();
		}
	}

	private void PlayParticleEffect()
	{
		if (!string.IsNullOrEmpty(mParticleURL))
		{
			if (mRankType == 8 && SanctuaryManager.pCurPetInstance != null)
			{
				LevelUpFx.PlayDragonLevelUp(mCurrentRank, SanctuaryManager.pCurPetInstance.transform.position, inPlaySound: true, mParticleURL);
			}
			else
			{
				LevelUpFx.PlayPlayerLevelUp(mCurrentRank, inPlaySound: true, mParticleURL);
			}
		}
	}

	public override void Close()
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetBusy(busy: false);
		}
	}
}
