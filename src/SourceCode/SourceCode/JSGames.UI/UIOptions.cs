using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace JSGames.UI;

public class UIOptions : UI
{
	public UnityAction OnClosed;

	public LocaleString _ServerErrorTitleText = new LocaleString("Error");

	public UI _HotKeysPopUp;

	public UI _GraphicSettingsUI;

	public UIButton _BtnClose;

	public UIWidget _BtnCredits;

	public UIWidget _BtnRestorePurchase;

	public UIButton _BtnLogout;

	public UIToggleButton _BtnMusic;

	public UIToggleButton _BtnSound;

	public UIToggleButton _BtnFullScreen;

	public bool _CanGuestPurchase;

	public static Action ShowPurchaseRestoreDB;

	public static Action PurchaseRestore;

	private bool mIsFullScreen;

	private static bool mIsCalibratedXRead;

	private static float mCalibratedX;

	private static bool mIsCalibratedYRead;

	private static float mCalibratedY;

	private static bool mIsCalibratedZRead;

	private static float mCalibratedZ;

	public static float pCalibratedX
	{
		get
		{
			if (!mIsCalibratedXRead && UserInfo.pIsReady)
			{
				mCalibratedX = PlayerPrefs.GetFloat("CalibratedX" + UserInfo.pInstance.ParentUserID, 0f);
				mIsCalibratedXRead = true;
			}
			return mCalibratedX;
		}
		set
		{
			PlayerPrefs.SetFloat("CalibratedX" + UserInfo.pInstance.ParentUserID, value);
			mCalibratedX = value;
			mIsCalibratedXRead = true;
		}
	}

	public static float pCalibratedY
	{
		get
		{
			if (!mIsCalibratedYRead && UserInfo.pIsReady)
			{
				mCalibratedY = PlayerPrefs.GetFloat("CalibratedY" + UserInfo.pInstance.ParentUserID, 0f);
				mIsCalibratedYRead = true;
			}
			return mCalibratedY;
		}
		set
		{
			PlayerPrefs.SetFloat("CalibratedY" + UserInfo.pInstance.ParentUserID, value);
			mCalibratedY = value;
			mIsCalibratedYRead = true;
		}
	}

	public static float pCalibratedZ
	{
		get
		{
			if (!mIsCalibratedZRead && UserInfo.pIsReady)
			{
				mCalibratedZ = PlayerPrefs.GetFloat("CalibratedZ" + UserInfo.pInstance.ParentUserID, 0f);
				mIsCalibratedZRead = true;
			}
			return mCalibratedZ;
		}
		set
		{
			PlayerPrefs.SetFloat("CalibratedZ" + UserInfo.pInstance.ParentUserID, value);
			mCalibratedZ = value;
			mIsCalibratedZRead = true;
		}
	}

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	private void Initialize()
	{
		if (UtPlatform.IsAndroid())
		{
			_BtnFullScreen.gameObject.SetActive(value: false);
			_BtnRestorePurchase.gameObject.SetActive(value: false);
		}
		else if (UtPlatform.IsiOS())
		{
			_BtnFullScreen.gameObject.SetActive(value: false);
			_BtnRestorePurchase.gameObject.SetActive(value: true);
		}
		else if (UtPlatform.IsStandAlone())
		{
			_BtnRestorePurchase.gameObject.SetActive(value: false);
			_BtnFullScreen.gameObject.SetActive(value: true);
		}
		if (_BtnMusic != null)
		{
			_BtnMusic.pChecked = SnChannel.pTurnOffMusicGroup;
		}
		if (_BtnSound != null)
		{
			_BtnSound.pChecked = SnChannel.pTurnOffSoundGroup;
		}
		if (_BtnFullScreen != null)
		{
			_BtnFullScreen.pChecked = Screen.fullScreen;
		}
		SetExclusive();
	}

	protected override void OnClick(UIWidget widget, PointerEventData pointerData)
	{
		base.OnClick(widget, pointerData);
		if (widget == _BtnFullScreen)
		{
			Screen.fullScreen = !Screen.fullScreen;
			_BtnFullScreen.pChecked = !Screen.fullScreen;
		}
		else if (widget == _BtnMusic)
		{
			SnChannel.pTurnOffMusicGroup = !SnChannel.pTurnOffMusicGroup;
			_BtnMusic.pChecked = SnChannel.pTurnOffMusicGroup;
		}
		else if (widget == _BtnSound)
		{
			SnChannel.pTurnOffSoundGroup = !SnChannel.pTurnOffSoundGroup;
			_BtnSound.pChecked = SnChannel.pTurnOffSoundGroup;
		}
		else if (widget == _BtnLogout)
		{
			GameUtilities.LoadLoginLevel();
			Destroy();
		}
		else if (widget == _BtnCredits)
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			string[] array = GameConfig.GetKeyData("Credits").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], CreditsBundleReady, typeof(GameObject));
		}
		else if (widget.name == "BtnInviteFriend")
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			string[] array2 = GameConfig.GetKeyData("FBSelectFriendAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], FBInviteBundleReady, typeof(GameObject));
		}
		else if (UtPlatform.IsiOS() && widget == _BtnRestorePurchase)
		{
			if (!_CanGuestPurchase && ShowPurchaseRestoreDB != null)
			{
				ShowPurchaseRestoreDB();
			}
			else if (PurchaseRestore != null)
			{
				PurchaseRestore();
			}
		}
		else if (widget == _BtnClose)
		{
			if (OnClosed != null)
			{
				OnClosed();
			}
			UnityEngine.Object.DestroyImmediate(base.gameObject);
		}
	}

	public void FBInviteBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		_ = 2;
	}

	public void CreditsBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			UnityEngine.Object.Instantiate((GameObject)inObject).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (Screen.fullScreen != mIsFullScreen)
		{
			mIsFullScreen = Screen.fullScreen;
			if (_BtnFullScreen != null)
			{
				_BtnFullScreen.pChecked = mIsFullScreen;
			}
		}
	}

	public void Destroy()
	{
		pVisible = false;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnDisable()
	{
		RemoveExclusive();
	}
}
