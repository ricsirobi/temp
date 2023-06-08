using UnityEngine;

internal class AchievementMessage : GenericMessage
{
	private GameObject mDb;

	private TaggedMessageHelper mTaggedMessageHelper;

	private AudioClip mVO;

	private AudioClip mSFX;

	private int mAssetsLoading;

	private bool mShowReward;

	private Texture2D mItemTexture;

	private bool mUiUpdated;

	private int mItemID = -1;

	private bool mLoadingItem;

	public AchievementMessage(MessageInfo messageInfo, string inAssetPath)
		: base(messageInfo)
	{
		Start();
		string text = inAssetPath;
		if (!string.IsNullOrEmpty(mMessageInfo.MemberImageUrl))
		{
			text = mMessageInfo.MemberImageUrl;
		}
		string[] array = text.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], LoadObjectEvent, typeof(GameObject));
	}

	private void Start()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mTaggedMessageHelper = new TaggedMessageHelper(mMessageInfo);
		string text = "";
		string text2 = "";
		if (SubscriptionInfo.pIsMember)
		{
			if (mTaggedMessageHelper.MemberAudioUrl.ContainsKey("Audio1"))
			{
				text = mTaggedMessageHelper.MemberAudioUrl["Audio1"];
			}
			if (mTaggedMessageHelper.MemberAudioUrl.ContainsKey("Audio2"))
			{
				text2 = mTaggedMessageHelper.MemberAudioUrl["Audio2"];
			}
		}
		else
		{
			if (mTaggedMessageHelper.NonMemberAudioUrl.ContainsKey("Audio1"))
			{
				text = mTaggedMessageHelper.NonMemberAudioUrl["Audio1"];
			}
			if (mTaggedMessageHelper.NonMemberAudioUrl.ContainsKey("Audio2"))
			{
				text2 = mTaggedMessageHelper.NonMemberAudioUrl["Audio2"];
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			string[] array = text.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnVOLoadEvent, typeof(AudioClip));
			mAssetsLoading++;
		}
		if (!string.IsNullOrEmpty(text2))
		{
			string[] array2 = text2.Split('/');
			RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], OnSFXLoadEvent, typeof(AudioClip));
			mAssetsLoading++;
		}
		RewardData rewardData = null;
		if (!string.IsNullOrEmpty(mMessageInfo.Data))
		{
			rewardData = UtUtilities.DeserializeFromXml(mMessageInfo.Data, typeof(RewardData)) as RewardData;
		}
		if (rewardData != null && rewardData.Rewards != null)
		{
			Reward[] rewards = rewardData.Rewards;
			foreach (Reward reward in rewards)
			{
				if (reward.Type == "Inventory Item")
				{
					mItemID = reward.ItemID.Value;
				}
			}
		}
		if (mItemID != -1)
		{
			mShowReward = true;
			CommonInventoryData.ReInit();
		}
	}

	public override void Update()
	{
		if (!mLoadingItem && mShowReward && CommonInventoryData.pIsReady)
		{
			mLoadingItem = true;
			string icon = CommonInventoryData.pInstance.GetIcon(mItemID);
			if (!string.IsNullOrEmpty(icon))
			{
				string[] array = icon.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnItemLoaded, typeof(Texture2D));
			}
			else
			{
				mShowReward = false;
			}
		}
		if (mUiUpdated || !(mDb != null) || mAssetsLoading != 0 || (mShowReward && !(mItemTexture != null)))
		{
			return;
		}
		mUiUpdated = true;
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mVO != null)
		{
			SnChannel.Play(mVO, "VO_Pool", inForce: true, null);
		}
		if (mSFX != null)
		{
			SnChannel.Play(mSFX, "SFX_Pool", inForce: true, null);
		}
		mUiGenericDB = Object.Instantiate(mDb);
		mUiGenericDB.name = mDb.name;
		UiAchievementDB component = mUiGenericDB.GetComponent<UiAchievementDB>();
		if (component != null)
		{
			component.SetMessageInfo(mMessageInfo);
			KAUI.SetExclusive(component, WsUserMessage.pInstance._MaskColor);
			if (mTaggedMessageHelper.SubType == "MedalItem")
			{
				component.ShowMedal();
			}
			else if (mTaggedMessageHelper.SubType == "TrophyItem" || mTaggedMessageHelper.SubType == "Trophy")
			{
				component.ShowTrophy();
			}
			else if (mTaggedMessageHelper.SubType == "Cash")
			{
				component.ShowGems();
			}
			component.pDialogClosedCallback = OnOk;
			string title = string.Empty;
			string achievement = string.Empty;
			string prompt = string.Empty;
			if (mTaggedMessageHelper.MemberMessage.ContainsKey("Line1"))
			{
				title = mTaggedMessageHelper.MemberMessage["Line1"];
			}
			if (mTaggedMessageHelper.MemberMessage.ContainsKey("Line2"))
			{
				achievement = mTaggedMessageHelper.MemberMessage["Line2"];
			}
			if (mTaggedMessageHelper.MemberMessage.ContainsKey("Line3"))
			{
				prompt = mTaggedMessageHelper.MemberMessage["Line3"];
			}
			component.SetText(title, achievement, prompt);
		}
	}

	public void OnOk()
	{
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB.GetComponent<UiAchievementDB>());
		}
		if (mMessageInfo != null)
		{
			RewardManager.SetReward(mMessageInfo.Data, inImmediateShow: false, deduct: true);
		}
		WsUserMessage.pInstance.OnClose();
	}

	private void OnItemLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				mItemTexture = (Texture2D)inObject;
			}
			else
			{
				mShowReward = false;
			}
			break;
		case RsResourceLoadEvent.ERROR:
			mShowReward = false;
			UtDebug.LogError("Error!!! downloading item ICON");
			break;
		}
	}

	private void OnVOLoadEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mVO = (AudioClip)inObject;
			mAssetsLoading--;
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Error: Unable to load sound: " + inURL);
			mAssetsLoading--;
			break;
		}
	}

	private void OnSFXLoadEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mSFX = (AudioClip)inObject;
			mAssetsLoading--;
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Error: Unable to load sound: " + inURL);
			mAssetsLoading--;
			break;
		}
	}

	private void LoadObjectEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mDb = (GameObject)inObject;
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			WsUserMessage.pInstance.OnClose();
			UtDebug.LogError("Error loading Trophy Message DB");
			break;
		}
	}

	public override void Show()
	{
		WsUserMessage.pInstance.ReInitUserRankData();
		WsUserMessage.pInstance.ReInitUserMoney();
	}
}
