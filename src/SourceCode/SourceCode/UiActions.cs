public class UiActions : KAUI
{
	public string _DataFile = string.Empty;

	private UiActionsMenu mActionsMenu;

	private KAButton mCloseBtn;

	private UiActionsMenu mUiActionsMenu;

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	protected override void Awake()
	{
		base.Awake();
	}

	public void Initialize()
	{
		if (!EmoticonActionData.pIsReady)
		{
			EmoticonActionData.Init(_DataFile, DataReady);
		}
		mCloseBtn = (KAButton)FindItem("CloseBtn");
		mCloseBtn.SetVisibility(inVisible: false);
		mUiActionsMenu = GetComponentInChildren<UiActionsMenu>();
		mActionsMenu = (UiActionsMenu)GetMenu("UiActionsMenu");
		mCloseBtn.SetVisibility(inVisible: true);
	}

	public void ShowEmoticons()
	{
		SetVisibility(inVisible: true);
		mUiActionsMenu.SetVisibility(inVisible: true);
		mActionsMenu.AddEmoticons();
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "CloseBtn")
		{
			mUiActionsMenu.SetVisibility(inVisible: false);
			SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(this);
		}
	}

	private void DataReady()
	{
		UtDebug.Log(" File downloaded successfully " + _DataFile);
	}
}
