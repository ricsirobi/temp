using UnityEngine;

internal class SocialRankMessage : GenericMessage
{
	private bool mReinitUserRank = true;

	private bool mUiUpdated;

	private bool mShowRewardBtn;

	private UserRank mCurrentRank;

	private UserRank mPreviousRank;

	private Texture2D mRewardTexture;

	private GameObject mDb;

	private int mAssetsLoading;

	private bool mLoadingItem;

	private int mItemID = -1;

	public SocialRankMessage(MessageInfo messageInfo, string inAssetPath)
		: base(messageInfo)
	{
		Start();
		string[] array = inAssetPath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], LoadObjectEvent, typeof(GameObject));
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
			UtDebug.LogError("Error loading Rank DB......");
			break;
		}
	}

	private void Start()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		string text = string.Empty;
		TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(mMessageInfo);
		if (taggedMessageHelper.MemberMessage.ContainsKey("ItemID"))
		{
			text = taggedMessageHelper.MemberMessage["ItemID"];
		}
		if (!string.IsNullOrEmpty(text))
		{
			int.TryParse(text, out mItemID);
		}
		if (mItemID != -1)
		{
			mShowRewardBtn = true;
			CommonInventoryData.ReInit();
		}
	}

	public void OnItemLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				mRewardTexture = (Texture2D)inObject;
			}
			else
			{
				mShowRewardBtn = false;
			}
			break;
		case RsResourceLoadEvent.ERROR:
			mShowRewardBtn = false;
			Debug.LogError("Error!!! downloading Reward ICON");
			break;
		}
	}

	public override void Show()
	{
		WsUserMessage.pInstance.ReInitUserRankData();
	}

	public override void Update()
	{
		if (mReinitUserRank && UserRankData.pIsReady)
		{
			mReinitUserRank = false;
			mCurrentRank = UserRankData.GetUserRankByType(3);
			if (mCurrentRank != null)
			{
				string image = mCurrentRank.Image;
				if (!string.IsNullOrEmpty(image))
				{
					RsResourceManager.Load(image, CurrentRankImageLoadingEvent);
					mAssetsLoading++;
				}
			}
			mPreviousRank = UserRankData.GetUserRankByType(3, mCurrentRank.RankID - 1);
			if (mPreviousRank != null)
			{
				string image2 = mPreviousRank.Image;
				if (!string.IsNullOrEmpty(image2))
				{
					RsResourceManager.Load(image2, PreviousRankImageLoadingEvent);
					mAssetsLoading++;
				}
			}
			string attribute = UserRankData.GetAttribute(3, mCurrentRank.RankID, "StarChart", "");
			if (!string.IsNullOrEmpty(attribute) && attribute.Length > 0)
			{
				string[] array = attribute.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnTextureLoaded, typeof(Texture));
				mAssetsLoading++;
			}
		}
		if (!mLoadingItem && mShowRewardBtn && CommonInventoryData.pIsReady)
		{
			mLoadingItem = true;
			string icon = CommonInventoryData.pInstance.GetIcon(mItemID);
			if (!string.IsNullOrEmpty(icon))
			{
				string[] array2 = icon.Split('/');
				RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], OnItemLoaded, typeof(Texture2D));
			}
			else
			{
				mShowRewardBtn = false;
			}
		}
		if (!mUiUpdated && mDb != null && mAssetsLoading == 0 && !mReinitUserRank && (!mShowRewardBtn || mRewardTexture != null))
		{
			InstantiateDB();
		}
	}

	private void InstantiateDB()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mUiUpdated = true;
	}

	public void CurrentRankImageLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mAssetsLoading--;
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed Loading: " + inURL);
			mAssetsLoading--;
			break;
		}
	}

	public void PreviousRankImageLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mAssetsLoading--;
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed Loading: " + inURL);
			mAssetsLoading--;
			break;
		}
	}

	public void OnTextureLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mAssetsLoading--;
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed Loading: " + inURL);
			mAssetsLoading--;
			break;
		}
	}
}
