using UnityEngine;

public class UiGSLevelSelection : KAUI
{
	public GauntletRailShootManager _GameManager;

	public RenderSettingsController _RenderSettings;

	public AudioClip _NonMemberVO;

	public LocaleString _MMODisabledOnDeviceText = new LocaleString("MMO disabled. You can enable it from settings.");

	public LocaleString _MMODisabledOnServerText = new LocaleString("MMO disabled. You can enable it from web account.");

	public LocaleString _CrossbowLockedText = new LocaleString("You must finish the quest from Heather about Gronckle Iron to play this mode!");

	private KAWidget mBtnTraining;

	private KAWidget mBtnRedPlanet;

	private KAWidget mBtnHeadOnHead;

	private KAWidget mBtnCrossbowLevel;

	private KAWidget mBtnBack;

	private KAWidget mIcoLock;

	private bool mIsMember;

	private KAUIGenericDB mUiGenericDB;

	protected override void Start()
	{
		base.Start();
		mBtnTraining = FindItem("BtnTraining");
		mBtnRedPlanet = FindItem("BtnRedPlanet");
		mBtnHeadOnHead = FindItem("BtnHeadOnHead");
		mBtnCrossbowLevel = FindItem("BtnCrossbowLevel");
		mBtnBack = FindItem("BtnBack");
		mIcoLock = mBtnRedPlanet.FindChildItem("IconLock");
	}

	public void InitializeSettings()
	{
		mIsMember = SubscriptionInfo.pIsMember;
		mIcoLock.SetVisibility(!mIsMember);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		Input.ResetInputAxes();
		if (!(_GameManager != null))
		{
			return;
		}
		if (item == mBtnTraining)
		{
			if (_GameManager.IsPetTooTired())
			{
				SetInteractive(interactive: false);
				_GameManager.ProcessPetTired(base.gameObject);
				return;
			}
			if (SubscriptionInfo.pIsMember && SanctuaryManager.pCurPetData != null && SanctuaryManager.pCurPetData.pStage > RaisedPetStage.CHILD)
			{
				_GameManager._ChooseDragonUI.SetVisibility(visible: true);
			}
			else
			{
				_GameManager.SetGameType(GSGameType.TRAINING);
			}
			SetVisibility(visible: false);
		}
		else if (item == mBtnRedPlanet)
		{
			if (mIsMember)
			{
				_GameManager.SetGameType(GSGameType.REDPLANET);
				SetVisibility(visible: false);
			}
			else if (_NonMemberVO != null)
			{
				SnChannel.Play(_NonMemberVO, "VO_Pool", inForce: false);
			}
		}
		else if (item == mBtnHeadOnHead)
		{
			if (!UserInfo.pInstance.MultiplayerEnabled)
			{
				ShowMessage(_MMODisabledOnServerText);
				return;
			}
			if (!MainStreetMMOClient.pIsMMOEnabled)
			{
				ShowMessage(_MMODisabledOnDeviceText);
				return;
			}
			_GameManager.pGameType = GSGameType.HEADTOHEAD;
			_GameManager._MultiplayerMenuScreen.SetVisibility(inVisible: true);
			SetVisibility(visible: false);
		}
		else if (item == mBtnCrossbowLevel)
		{
			if (!_GameManager.pIsCrossbowUnlocked)
			{
				ShowMessage(_CrossbowLockedText);
			}
			else if (_GameManager.CanPlayCrossBowLevel())
			{
				_GameManager.SetGameType(GSGameType.CROSSBOW_ARROW);
				SetVisibility(visible: false);
			}
		}
		else if (item == mBtnBack)
		{
			_GameManager.ExitGame();
			SetVisibility(visible: false);
		}
	}

	public void PetEnergyProcessed()
	{
		SetInteractive(interactive: true);
	}

	private void ShowMessage(LocaleString inText)
	{
		if (mUiGenericDB == null)
		{
			GameObject gameObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
			mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
			mUiGenericDB._MessageObject = base.gameObject;
			mUiGenericDB._CloseMessage = "OnCloseDB";
			mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
			mUiGenericDB.SetTextByID(inText._ID, inText._Text, interactive: false);
			KAUI.SetExclusive(mUiGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}
	}

	public void OnCloseDB()
	{
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB);
			Object.Destroy(mUiGenericDB.gameObject);
			mUiGenericDB = null;
		}
	}

	public override void SetVisibility(bool visible)
	{
		if (visible && _RenderSettings != null && _RenderSettings.skybox != null)
		{
			RenderSettings.skybox = _RenderSettings.skybox;
		}
		base.SetVisibility(visible);
	}
}
