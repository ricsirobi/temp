public class UiMyRoomsInt : KAUI
{
	public UiMyRoomBuilder _MyRoomBuilder;

	public string _MyRoomsStoreName = "";

	private KAWidget mBuildModeBtn;

	private KAWidget mHelpBtn;

	private KAWidget mStoreBtn;

	private bool mInitialized;

	public static bool _SwitchToBuildMode;

	protected KAWidget pBuildModeBtn => mBuildModeBtn;

	protected override void Start()
	{
		base.Start();
		mBuildModeBtn = FindItem("RoomDecorateBtn");
		mHelpBtn = FindItem("HelpBtn");
		mStoreBtn = FindItem("StoreBtn");
	}

	protected override void Update()
	{
		base.Update();
		if (mInitialized || (UtPlatform.IsMobile() && (!(MyRoomsIntLevel.pInstance != null) || !MyRoomsIntLevel.pInstance.pInitialized)))
		{
			return;
		}
		mInitialized = true;
		if (_MyRoomBuilder != null)
		{
			_MyRoomBuilder.gameObject.SetActive(value: false);
		}
		if (MyRoomsIntLevel.pInstance != null && MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			mBuildModeBtn.SetVisibility(inVisible: false);
			if (mHelpBtn != null)
			{
				mHelpBtn.SetVisibility(inVisible: false);
			}
			if (mStoreBtn != null)
			{
				mStoreBtn.SetVisibility(inVisible: false);
			}
		}
		if (_SwitchToBuildMode)
		{
			if (_MyRoomBuilder != null && !MyRoomsIntMain.pDisableBuildMode)
			{
				AvAvatar.pInputEnabled = true;
				_MyRoomBuilder.gameObject.SetActive(value: true);
				_MyRoomBuilder.ShowUI();
			}
			_SwitchToBuildMode = false;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBuildModeBtn)
		{
			if (_MyRoomBuilder != null && !MyRoomsIntMain.pDisableBuildMode)
			{
				AvAvatar.pInputEnabled = true;
				TutorialManager.StopTutorials();
				_MyRoomBuilder.gameObject.SetActive(value: true);
				_MyRoomBuilder.ShowUI();
				if (FishingZone._FishingZoneUi != null)
				{
					FishingZone._FishingZoneUi.SetVisibility(inVisible: false);
				}
			}
		}
		else if (item == mHelpBtn)
		{
			mHelpBtn.SetVisibility(inVisible: false);
		}
		else if (item.name == "btnNewStore")
		{
			StoreLoader.Load(setDefaultMenuItem: true, "", _MyRoomsStoreName, base.gameObject);
		}
	}

	public void EnableHelp()
	{
		if (mHelpBtn != null)
		{
			mHelpBtn.SetVisibility(inVisible: true);
		}
	}

	public void BuildModeClosed()
	{
		_MyRoomBuilder.gameObject.SetActive(value: false);
	}
}
