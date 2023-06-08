using UnityEngine;

public class UiGSMultiplayerScreen : KAUI
{
	public GauntletRailShootManager _GameManager;

	private KAWidget mBtnJoin;

	private KAWidget mBtnStart;

	private KAWidget mBtnBFF;

	private KAWidget mBtnBack;

	private bool mWaitingForMMO;

	private float mTimer;

	private GauntletRailShootManager mGameManager;

	protected override void Start()
	{
		base.Start();
		mBtnJoin = FindItem("BtnJoin");
		mBtnStart = FindItem("BtnStart");
		mBtnBFF = FindItem("BtnBFF");
		mBtnBack = FindItem("BtnBack");
		mGameManager = GauntletRailShootManager.pInstance;
		mTimer = mGameManager._ConnectionTimeout;
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (!(_GameManager != null))
		{
			return;
		}
		if (item == mBtnBack)
		{
			Input.ResetInputAxes();
			if (GauntletMMOClient.pInstance != null)
			{
				GauntletMMOClient.pInstance.DestroyMMO(inLogout: true);
			}
			_GameManager._MultiplayerMenuScreen.SetVisibility(inVisible: false);
			_GameManager._LevelSelectionScreen.SetVisibility(visible: true);
			return;
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		_GameManager._MultiplayerMenuScreen.SetInteractive(interactive: false);
		if (item == mBtnJoin)
		{
			GauntletMMOClient.Init(GauntletMMORoomType.JOIN_ANY);
		}
		else if (item == mBtnStart)
		{
			GauntletMMOClient.Init(GauntletMMORoomType.HOST_FOR_ANY);
		}
		else if (item == mBtnBFF)
		{
			GauntletMMOClient.Init(GauntletMMORoomType.HOST_FOR_BUDDY);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!GetVisibility() || !mWaitingForMMO)
		{
			return;
		}
		if (MainStreetMMOClient.pInstance.pState != MMOClientState.IN_ROOM)
		{
			if (mTimer <= 0f)
			{
				mGameManager.DisplayGenericDialog(base.gameObject, mGameManager._ConnectionTimeOutText._ID, mGameManager._ConnectionTimeOutText._Text, useDots: false, 1f, IsYesBtn: false, IsNoBtn: false, IsOKBtn: false, IsCloseBtn: false, isExclusiveUI: true);
				EnableUi();
				mWaitingForMMO = false;
			}
			else
			{
				mTimer -= Time.deltaTime;
				DisableUi();
			}
		}
		else
		{
			EnableUi();
			mWaitingForMMO = false;
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		mWaitingForMMO = inVisible;
		if (mWaitingForMMO)
		{
			mTimer = GauntletRailShootManager.pInstance._ConnectionTimeout;
		}
	}

	private void DisableUi()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		_GameManager._MultiplayerMenuScreen.SetInteractive(interactive: false);
	}

	private void EnableUi()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		_GameManager._MultiplayerMenuScreen.SetInteractive(interactive: true);
	}
}
