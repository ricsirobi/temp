using UnityEngine;

namespace SquadTactics;

public class UiSettings : KAUI
{
	public string _ConfirmationDBName = "PfUiSTGenericDB";

	private KAWidget mBtnClose;

	private KAWidget mBtnRestart;

	private KAWidget mBtnQuit;

	private KAToggleButton mBtnToggleCinematics;

	private KAToggleButton mBtnToggleElement;

	private KAUIGenericDB mGenericDB;

	protected override void Start()
	{
		base.Start();
		SetVisibility(inVisible: false);
		mBtnClose = FindItem("BtnClose");
		mBtnRestart = FindItem("BtnRestart");
		mBtnQuit = FindItem("BtnQuit");
		mBtnToggleCinematics = (KAToggleButton)FindItem("BtnToggleCinematics");
		mBtnToggleElement = (KAToggleButton)FindItem("BtnToggleElement");
	}

	public void ShowUi(bool show)
	{
		GameManager.pInstance._HUD.SetVisibility(!show);
		if (show)
		{
			mParentVisible = true;
			mBtnToggleCinematics.SetChecked(GameManager.pInstance.pAllowCinematicCamera);
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
			}
			if (GameManager.pInstance._Tutorial != null)
			{
				mBtnToggleElement.SetState(KAUIState.DISABLED);
			}
			else
			{
				mBtnToggleElement.SetChecked(GameManager.pInstance._HUD.pAllowAdvantageDisplay);
			}
			KAUI.SetExclusive(this);
		}
		else
		{
			if (mGenericDB != null)
			{
				DestroyGenericDB();
			}
			if (MissionManager.pInstance != null && GameManager.pInstance._Tutorial == null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
			}
			KAUI.RemoveExclusive(this);
			mParentVisible = GameManager.pInstance._HUD.GetVisibility();
		}
		SetVisibility(show);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnClose)
		{
			ShowUi(show: false);
		}
		else if (inWidget == mBtnRestart)
		{
			ShowConfirmDB(isQuit: false);
		}
		else if (inWidget == mBtnQuit)
		{
			ShowConfirmDB(isQuit: true);
		}
		else if (inWidget == mBtnToggleElement)
		{
			GameManager.pInstance._HUD.pAllowAdvantageDisplay = !GameManager.pInstance._HUD.pAllowAdvantageDisplay;
			mBtnToggleElement.SetChecked(GameManager.pInstance._HUD.pAllowAdvantageDisplay);
		}
		else if (inWidget == mBtnToggleCinematics)
		{
			GameManager.pInstance.pAllowCinematicCamera = !GameManager.pInstance.pAllowCinematicCamera;
			mBtnToggleCinematics.SetChecked(GameManager.pInstance.pAllowCinematicCamera);
		}
	}

	private void ShowConfirmDB(bool isQuit)
	{
		string localizedString = GameManager.pInstance._HUD._HUDStrings._LevelRestartConfirmText.GetLocalizedString();
		if (isQuit)
		{
			localizedString = GameManager.pInstance._HUD._HUDStrings._LevelQuitConfirmText.GetLocalizedString();
		}
		mGenericDB = GameUtilities.CreateKAUIGenericDB(_ConfirmationDBName, "Confirm DB");
		mGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mGenericDB.SetText(localizedString, interactive: false);
		mGenericDB._NoMessage = "OnConfirmNo";
		mGenericDB._YesMessage = (isQuit ? "OnConfirmQuit" : "OnConfirmRestart");
		mGenericDB._MessageObject = base.gameObject;
		KAUI.SetExclusive(mGenericDB);
	}

	private void OnConfirmNo()
	{
		DestroyGenericDB();
	}

	private void DestroyGenericDB()
	{
		RemoveExclusiveAndDestroyObject();
		mGenericDB = null;
	}

	private void OnConfirmQuit()
	{
		RemoveExclusiveAndDestroyObject();
		GameManager.pInstance.LoadMainMenu();
	}

	private void OnConfirmRestart()
	{
		RemoveExclusiveAndDestroyObject();
		GameManager.pInstance.Restart();
	}

	private void RemoveExclusiveAndDestroyObject()
	{
		KAUI.RemoveExclusive(mGenericDB);
		Object.Destroy(mGenericDB.gameObject);
	}
}
