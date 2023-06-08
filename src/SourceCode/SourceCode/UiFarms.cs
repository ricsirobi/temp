using UnityEngine;

public class UiFarms : UiCardParent
{
	public LocaleString _SelectFarmText = new LocaleString("Select a farm room");

	private UiFarmsListCard mUiFarmsListCard;

	private UiFarmsInfoCard mUiFarmsInfoCard;

	private UiCard mLastMessageCard;

	private KAWidget mExitBtn;

	public int _FarmExpansionDefaultItemID = 13105;

	public string _FarmExpansionDefaultRoomID = "";

	public string _FarmDefaultScene = "FarmingDO";

	private static string mUserID;

	private bool mIsTransition;

	private static GameObject mMsgObject;

	public static bool mIsNeedExclusive;

	public static OnFarmsUILoad pOnFarmsUILoadHandler;

	public static OnFarmsUIClosed pOnFarmsUIClosed;

	public UiFarmsListCard pUiFarmsListCard => mUiFarmsListCard;

	public UiFarmsInfoCard pUiFarmsInfoCard => mUiFarmsInfoCard;

	public static string pUserID => mUserID;

	public bool pIsUserFarm => string.IsNullOrEmpty(mUserID);

	public bool pIsTransition => mIsTransition;

	protected override void Awake()
	{
		base.Awake();
		mUiFarmsListCard = GetComponentInChildren<UiFarmsListCard>();
		mUiFarmsInfoCard = GetComponentInChildren<UiFarmsInfoCard>();
		mExitBtn = FindItem("ExitBtn");
	}

	protected override void Start()
	{
		base.Start();
		InitUI();
	}

	public void SetMessage(UiCard messsageCard, LocaleString message)
	{
		if (mLastMessageCard != null && mLastMessageCard != messsageCard)
		{
			mLastMessageCard.HideMessage();
		}
		messsageCard.ShowMessage(message);
		mLastMessageCard = messsageCard;
	}

	public static void LoadFarmsUI()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = GameConfig.GetKeyData("FarmSelectionAsset").Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadingEvent, typeof(GameObject));
	}

	public static void OnAssetLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = Object.Instantiate((GameObject)inObject);
			RsResourceManager.ReleaseBundleData(inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			UiFarms component = obj.GetComponent<UiFarms>();
			if (mIsNeedExclusive)
			{
				KAUI.SetExclusive(component);
				AvAvatar.EnableAllInputs(inActive: false);
			}
			if (pOnFarmsUILoadHandler != null)
			{
				pOnFarmsUILoadHandler(isSuccess: true);
			}
			if (mMsgObject != null)
			{
				mMsgObject.SendMessage("OnFarmUIOpened", component, SendMessageOptions.DontRequireReceiver);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (pOnFarmsUILoadHandler != null)
			{
				pOnFarmsUILoadHandler(isSuccess: false);
			}
			break;
		}
	}

	public static void OpenFriendFarmListUI(string userID, GameObject msgObject, bool setExclusive = true)
	{
		mUserID = userID;
		mMsgObject = msgObject;
		mIsNeedExclusive = setExclusive;
		LoadFarmsUI();
	}

	public static void OpenFarmListUI(GameObject msgObject, bool setExclusive = true)
	{
		mUserID = null;
		mMsgObject = msgObject;
		mIsNeedExclusive = setExclusive;
		LoadFarmsUI();
	}

	private void InitUI()
	{
		UiCard.Transition childTrans = new UiCard.Transition(UiCard.Transition.Type.SlideOut, UiCard.Transition.Direction.Left, UiCard.Transition.ZOrder.Top);
		mUiFarmsListCard.PushCard(null, null, childTrans);
		mUiFarmsListCard.SetVisibility(inVisible: true);
		SetMessage(mUiFarmsListCard, _SelectFarmText);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mExitBtn == inWidget)
		{
			Exit();
		}
	}

	public override void Exit()
	{
		if (mIsNeedExclusive)
		{
			KAUI.RemoveExclusive(this);
			AvAvatar.EnableAllInputs(inActive: true);
		}
		if (pOnFarmsUIClosed != null)
		{
			pOnFarmsUIClosed();
		}
		mUserID = null;
		Object.Destroy(base.gameObject);
		if (mMsgObject != null)
		{
			mMsgObject.SendMessage("OnFarmUIClosed", SendMessageOptions.DontRequireReceiver);
		}
	}
}
