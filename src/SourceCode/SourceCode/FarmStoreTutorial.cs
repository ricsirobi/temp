using UnityEngine;
using UnityEngine.SceneManagement;

public class FarmStoreTutorial : FarmingTutorialBase
{
	public string _StoreObjName = "PfUiStoresDO";

	public int _FreeCoins = 50;

	private bool mCanShowTutorial;

	private bool mHighlightTutorialSeed;

	public int _TutorialSeedItemID = 7230;

	public GameObject _FarmPlantTutorial;

	public string _StoreInitTutorial;

	public StoreLoader.Selection _SeedsStoreInfo;

	public override void Start()
	{
		base.Start();
	}

	protected override void EnableAllItems()
	{
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("StoreScene"))
		{
			OnWaitListCompleted();
			mCanShowTutorial = true;
		}
	}

	public override void OnWaitListCompleted()
	{
		base.OnWaitListCompleted();
		TryPlayingTutorial();
	}

	public void TryPlayingTutorial()
	{
		if (!mDeleteInstance)
		{
			if (KAUIStore.pIsInItsOwnScene && RsResourceManager.pLastLevel == _FarmingSceneName)
			{
				mCanShowTutorial = true;
			}
			else if (TutorialManager.HasTutorialPlayed(_StoreInitTutorial) && !TutorialManager.HasTutorialPlayed(_TutIndexKeyName))
			{
				mCanShowTutorial = true;
			}
		}
		else
		{
			DeleteInstance();
		}
	}

	private KAWidget GetWidgetByName(string widgetName)
	{
		GameObject gameObject = GameObject.Find(_StoreObjName);
		if (gameObject != null)
		{
			KAUIStore componentInChildren = gameObject.GetComponentInChildren<KAUIStore>();
			if (componentInChildren != null)
			{
				return componentInChildren.FindItem(widgetName, recursive: false);
			}
		}
		return null;
	}

	private void SetSeedsCategory()
	{
		GameObject gameObject = GameObject.Find(_StoreObjName);
		if (!(gameObject != null))
		{
			return;
		}
		KAUIStore componentInChildren = gameObject.GetComponentInChildren<KAUIStore>();
		if (componentInChildren != null)
		{
			if (_SeedsStoreInfo != null)
			{
				componentInChildren.UpdateStore(KAUIStore.StoreMode.Choose, _SeedsStoreInfo._Store, _SeedsStoreInfo._Category, 393);
			}
			KAWidget kAWidget = componentInChildren.FindItem("BtnMainMenuUp");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(isDisabled: true);
			}
		}
	}

	protected override void OnStepStarted(int stepIdx, string stepName)
	{
		base.OnStepStarted(stepIdx, stepName);
		GameObject gameObject = null;
		switch (stepIdx)
		{
		case 1:
			SetSeedsCategory();
			mHighlightTutorialSeed = true;
			DisableBuyDBButtons(inDisable: true);
			break;
		case 2:
		{
			DisableBuyDBButtons(inDisable: true);
			mStartBlinking = true;
			KAWidget widgetByName2 = GetWidgetByName("BtnPreviewBuy");
			if (widgetByName2 != null)
			{
				mBlinkObj = widgetByName2.gameObject;
			}
			break;
		}
		case 3:
		{
			Money.AddMoney(_FreeCoins, bForceUpdate: true);
			gameObject = GameObject.Find(_StoreObjName);
			if (!(gameObject != null))
			{
				break;
			}
			KAUIStoreBuyPopUp componentInChildren2 = gameObject.GetComponentInChildren<KAUIStoreBuyPopUp>();
			if (componentInChildren2 != null)
			{
				KAWidget kAWidget2 = componentInChildren2.FindItem("BtnBuy");
				if (kAWidget2 != null)
				{
					mStartBlinking = true;
					mBlinkObj = kAWidget2.gameObject;
				}
			}
			break;
		}
		case 4:
		{
			gameObject = GameObject.Find(_StoreObjName);
			if (!(gameObject != null))
			{
				break;
			}
			KAUIStoreSyncPopUp componentInChildren = gameObject.GetComponentInChildren<KAUIStoreSyncPopUp>();
			if (componentInChildren != null)
			{
				KAWidget kAWidget = componentInChildren.FindItem("btnContinue");
				if (kAWidget != null)
				{
					mStartBlinking = true;
					mBlinkObj = kAWidget.gameObject;
				}
			}
			break;
		}
		case 5:
		{
			KAWidget widgetByName = GetWidgetByName("btnClose");
			if (widgetByName != null)
			{
				mStartBlinking = true;
				mBlinkObj = widgetByName.gameObject;
			}
			break;
		}
		}
	}

	protected override void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		base.OnStepEnded(stepIdx, stepName, tutQuit);
		switch (stepIdx)
		{
		case 0:
		{
			GameObject gameObject = GameObject.Find(_StoreObjName);
			if (!(gameObject != null))
			{
				break;
			}
			KAUIStore componentInChildren = gameObject.GetComponentInChildren<KAUIStore>();
			if (componentInChildren != null)
			{
				if (_SeedsStoreInfo != null)
				{
					componentInChildren.UpdateStore(KAUIStore.StoreMode.Choose, _SeedsStoreInfo._Store, _SeedsStoreInfo._Category, 393);
				}
				KAWidget kAWidget = componentInChildren.FindItem("BtnMainMenuUp");
				if (kAWidget != null)
				{
					kAWidget.SetDisabled(isDisabled: true);
				}
			}
			break;
		}
		case 1:
			mStartBlinking = false;
			DisableBuyDBButtons(inDisable: true);
			break;
		case 2:
			mStartBlinking = false;
			mBlinkObj = null;
			break;
		case 3:
			DisableCategoryButtons();
			break;
		case 4:
			DisableSeeds(highlightTutorialSeed: false);
			break;
		case 5:
			if (_FarmPlantTutorial != null)
			{
				_FarmPlantTutorial.SendMessage("ShowTutorial");
			}
			break;
		}
	}

	private void DisableCategoryButtons()
	{
		GameObject gameObject = GameObject.Find(_StoreObjName);
		if (!(gameObject != null))
		{
			return;
		}
		KAUIStore component = gameObject.GetComponent<KAUIStore>();
		if (component != null)
		{
			KAWidget kAWidget = component.FindItem("BtnMainMenuUp");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(isDisabled: true);
			}
		}
		KAUIStoreCategoryMenu componentInChildren = gameObject.GetComponentInChildren<KAUIStoreCategoryMenu>();
		if (componentInChildren != null && componentInChildren.GetItemCount() > 0)
		{
			for (int i = 0; i < componentInChildren.GetItemCount(); i++)
			{
				componentInChildren.FindItemAt(i).SetDisabled(isDisabled: true);
			}
		}
		KAUIStoreMainMenu componentInChildren2 = gameObject.GetComponentInChildren<KAUIStoreMainMenu>();
		if (componentInChildren2 != null && componentInChildren2.GetItemCount() > 0)
		{
			for (int j = 0; j < componentInChildren2.GetItemCount(); j++)
			{
				componentInChildren2.FindItemAt(j).SetDisabled(isDisabled: true);
			}
		}
	}

	private void DisableSeeds(bool highlightTutorialSeed)
	{
		GameObject gameObject = GameObject.Find(_StoreObjName);
		if (!(gameObject != null))
		{
			return;
		}
		KAUIStoreChooseMenu componentInChildren = gameObject.GetComponentInChildren<KAUIStoreChooseMenu>();
		if (!(componentInChildren != null) || componentInChildren.GetItemCount() <= 0)
		{
			return;
		}
		componentInChildren.DisablePageScroll(highlightTutorialSeed);
		if (highlightTutorialSeed)
		{
			mHighlightTutorialSeed = false;
		}
		for (int i = 0; i < componentInChildren.GetItemCount(); i++)
		{
			KAWidget kAWidget = componentInChildren.FindItemAt(i);
			if (highlightTutorialSeed)
			{
				bool flag = false;
				KAWidget kAWidget2 = kAWidget.FindChildItem("AniIconImage").FindChildItemAt(0);
				if (kAWidget2 != null && kAWidget2.GetUserData() is KAStoreItemData kAStoreItemData && kAStoreItemData._ItemData != null)
				{
					flag = kAStoreItemData._ItemData.ItemID == _TutorialSeedItemID;
				}
				if (!flag)
				{
					kAWidget.SetDisabled(isDisabled: true);
				}
				else if (highlightTutorialSeed)
				{
					mBlinkObj = kAWidget.gameObject;
					mStartBlinking = true;
				}
			}
			else
			{
				kAWidget.SetDisabled(isDisabled: true);
				mStartBlinking = false;
				mBlinkObj = null;
			}
		}
	}

	public override void SetTutDBButtonStates(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose)
	{
		switch (mCurrentTutIndex)
		{
		case 0:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, inBtnClose);
			break;
		case 7:
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: true, inBtnClose: false);
			break;
		}
	}

	public override void Update()
	{
		base.Update();
		if (mCanShowTutorial)
		{
			if (KAUIStore.pIsInItsOwnScene && RsResourceManager.pLastLevel == _FarmingSceneName)
			{
				mCurrentTutIndex = 1;
				TutorialManager.MarkTutorialDone(_StoreInitTutorial);
				ShowTutorial();
			}
			if (AvAvatar.pState == AvAvatarState.IDLE)
			{
				ShowTutorial();
			}
			else if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("StoreScene"))
			{
				TryPlayingTutorial();
			}
			mCanShowTutorial = false;
		}
		if (mHighlightTutorialSeed)
		{
			DisableSeeds(highlightTutorialSeed: true);
			DisableCategoryButtons();
		}
	}

	public void HighlightSeed()
	{
		if (mBlinkObj != null)
		{
			mBlinkObj.transform.localScale = Vector3.one;
			TweenScale componentInChildren = mBlinkObj.GetComponentInChildren<TweenScale>();
			if (componentInChildren != null)
			{
				Object.Destroy(componentInChildren);
			}
		}
		DisableSeeds(highlightTutorialSeed: true);
		DisableCategoryButtons();
	}

	public override void OnGenericInfoBoxExitInit()
	{
	}

	private void DisableBuyDBButtons(bool inDisable)
	{
		GameObject gameObject = GameObject.Find(_StoreObjName);
		if (gameObject != null)
		{
			KAUIStoreBuyPopUp componentInChildren = gameObject.GetComponentInChildren<KAUIStoreBuyPopUp>();
			KAWidget kAWidget = componentInChildren.FindItem("BtnClose");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(inDisable);
			}
			KAWidget kAWidget2 = componentInChildren.FindItem("Quantity");
			if (kAWidget2 != null)
			{
				kAWidget2.SetDisabled(inDisable);
			}
		}
	}
}
