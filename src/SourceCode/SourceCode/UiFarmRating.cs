using UnityEngine;

public class UiFarmRating : KAUI
{
	public int _MaxRating = 5;

	public GameObject _RatingUiLauncher;

	private KAWidget mSubmitBtn;

	private KAWidget mLeaderBoardBtn;

	private KAToggleButton[] mStarArray = new KAToggleButton[5];

	private GameObject mLeaderBoardUI;

	private int mRank;

	private int mRankOnServer;

	private KAWidget mTxtDescription;

	private bool mInitialized;

	private KAWidget mCloseBtn;

	protected override void Start()
	{
		base.Start();
		mSubmitBtn = FindItem("SubmitBtn");
		mLeaderBoardBtn = FindItem("LeaderboardBtn");
		mTxtDescription = FindItem("TxtDescription");
		mCloseBtn = FindItem("BtnClose");
		bool flag = MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt();
		if (flag)
		{
			mSubmitBtn.SetVisibility(inVisible: true);
			mCloseBtn.SetVisibility(inVisible: false);
			_RatingUiLauncher.SetActive(value: false);
			WsWebService.GetDisplayNameByUserID(MainStreetMMOClient.pInstance.GetOwnerIDForCurrentLevel(), ServiceEventHandler, null);
		}
		for (int i = 0; i < _MaxRating; i++)
		{
			mStarArray[i] = (KAToggleButton)FindItem("Star0" + (i + 1));
			if (flag)
			{
				mStarArray[i].SetInteractive(isInteractive: true);
			}
		}
	}

	private void OnEnable()
	{
		if (!mInitialized)
		{
			return;
		}
		if (!MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			if (FarmManager.pCurrentFarmData != null)
			{
				mTxtDescription.SetText(FarmManager.pCurrentFarmData.Name);
			}
			mTxtDescription.SetVisibility(inVisible: true);
		}
		UpdateStarClick(mRankOnServer);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_RATING_FOR_RATED_USERS:
		case WsServiceType.GET_AVERAGE_RATING_FOR_ROOM:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				if (inObject != null)
				{
					SetVisibility(inVisible: true);
					mRankOnServer = Mathf.Clamp((int)inObject, 0, _MaxRating);
					UpdateStarClick(mRankOnServer);
				}
				break;
			case WsServiceEvent.ERROR:
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				UtDebug.LogError("GetFarmRating call failed!");
				break;
			}
			break;
		case WsServiceType.GET_DISPLAYNAME_BY_USER_ID:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				string text = inObject as string;
				if (!string.IsNullOrEmpty(text))
				{
					string text2 = mTxtDescription.GetText();
					text2 = text2.Replace("{{UserName}}", text);
					mTxtDescription.SetText(text2);
					mTxtDescription.SetVisibility(inVisible: true);
				}
				break;
			}
			case WsServiceEvent.ERROR:
				UtDebug.Log("GetDisplayNameByUserID call failed");
				break;
			}
			break;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mSubmitBtn)
		{
			mSubmitBtn.SetDisabled(isDisabled: true);
			if (mRank != 0)
			{
				mRankOnServer = mRank;
				FarmManager obj = MyRoomsIntMain.pInstance as FarmManager;
				string text = "";
				UtDebug.Log("current RoomID " + text);
				RatingInfo.SubmitRatingForUserID(obj._LeaderboardCategoryID, text, UserInfo.pInstance.UserID, MainStreetMMOClient.pInstance.GetOwnerIDForLevel(RsResourceManager.pCurrentLevel), mRank, null, null);
			}
		}
		else if (item == mLeaderBoardBtn)
		{
			if (mLeaderBoardUI == null)
			{
				UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
				string[] array = GameConfig.GetKeyData("FarmLeaderboardAsset").Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadEvent, typeof(GameObject));
			}
			else
			{
				mLeaderBoardUI.SetActive(value: true);
				AvAvatar.pState = AvAvatarState.PAUSED;
			}
		}
		else if (item == mCloseBtn)
		{
			SetVisibility(inVisible: false);
		}
		else
		{
			if (!item.name.Contains("Star"))
			{
				return;
			}
			for (int i = 0; i < mStarArray.Length; i++)
			{
				if (item == mStarArray[i])
				{
					if (mRank != i + 1)
					{
						mSubmitBtn.SetDisabled(mRankOnServer == i + 1);
						UpdateStarClick(i + 1);
					}
					break;
				}
			}
		}
	}

	public void OnClick(GameObject go)
	{
		if (go == _RatingUiLauncher)
		{
			SetVisibility(inVisible: true);
		}
	}

	public void OnDrag(GameObject go)
	{
	}

	protected override void Update()
	{
		if (mInitialized)
		{
			return;
		}
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if (farmManager != null)
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			string roomID = "";
			if (MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
			{
				string ownerIDForCurrentLevel = MainStreetMMOClient.pInstance.GetOwnerIDForCurrentLevel();
				WsWebService.GetRatingForRatedUser(farmManager._LeaderboardCategoryID, roomID, ownerIDForCurrentLevel, ServiceEventHandler, null);
			}
			else
			{
				WsWebService.GetAverageRatingForRoom(farmManager._LeaderboardCategoryID, roomID, ServiceEventHandler, null);
			}
			mInitialized = true;
		}
	}

	public override void OnHover(KAWidget item, bool isHover)
	{
		base.OnHover(item, isHover);
		if (!item.name.Contains("Star"))
		{
			return;
		}
		for (int i = 0; i < mStarArray.Length; i++)
		{
			if (item == mStarArray[i])
			{
				UpdateStarHover(i, isHover);
			}
		}
	}

	private void UpdateStarHover(int starIndex, bool isHover)
	{
		if (starIndex > mStarArray.Length)
		{
			UtDebug.Log("starRating is out of bound: " + starIndex);
			return;
		}
		for (int i = 0; i < mStarArray.Length; i++)
		{
			if (isHover)
			{
				mStarArray[i].OnHover(i <= starIndex && isHover);
			}
			else
			{
				mStarArray[i].SetChecked(i < mRank);
			}
		}
	}

	private void UpdateStarClick(int starIndex)
	{
		if (starIndex > mStarArray.Length)
		{
			UtDebug.Log("starRating is out of bound: " + starIndex);
			return;
		}
		mRank = starIndex;
		for (int i = 0; i < mStarArray.Length; i++)
		{
			mStarArray[i].SetChecked(i < mRank);
		}
	}

	private void OnAssetLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			GameObject gameObject = (GameObject)inObject;
			mLeaderBoardUI = Object.Instantiate(gameObject);
			mLeaderBoardUI.name = gameObject.name;
			mLeaderBoardUI.transform.parent = base.transform.parent;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			Debug.LogError("Asset not found! " + inURL);
			break;
		}
	}
}
