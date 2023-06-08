using UnityEngine;

public class UiDragonsStable : UiCardParent
{
	public enum Mode
	{
		DragonSelection = 1,
		StableSelection,
		DragonNestAllocation,
		DragonInfo
	}

	public LocaleString _SelectDragonText = new LocaleString("Select a dragon");

	public LocaleString _SelectStableText = new LocaleString("Select a stable room");

	public LocaleString _SelectNestText = new LocaleString("Select a nest");

	private UiStablesListCard mUiStablesListCard;

	private UiStablesInfoCard mUiStablesInfoCard;

	private UiDragonsListCard mUiDragonsListCard;

	private UiDragonsInfoCard mUiDragonsInfoCard;

	private UiCard mLastMessageCard;

	private static Mode mCurrentMode = Mode.DragonSelection;

	private static int pSelectedPetID;

	private KAWidget mExitBtn;

	private bool mIsTransition;

	private static GameObject mMsgObject = null;

	public static bool mIsNeedExclusive = false;

	public static bool mSetForceDragonSelection = false;

	public static OnStablesUILoad pOnStablesUILoadHandler = null;

	public static OnStablesUIClosed pOnStablesUIClosed = null;

	public UiStablesListCard pUiStablesListCard => mUiStablesListCard;

	public UiStablesInfoCard pUiStablesInfoCard => mUiStablesInfoCard;

	public UiDragonsListCard pUiDragonsListCard => mUiDragonsListCard;

	public UiDragonsInfoCard pUiDragonsInfoCard => mUiDragonsInfoCard;

	public static Mode pCurrentMode => mCurrentMode;

	public bool pIsTransition => mIsTransition;

	private bool ShowCustomization { get; set; }

	protected override void Awake()
	{
		base.Awake();
		mUiStablesListCard = GetComponentInChildren<UiStablesListCard>();
		mUiStablesInfoCard = GetComponentInChildren<UiStablesInfoCard>();
		mUiDragonsListCard = GetComponentInChildren<UiDragonsListCard>();
		mUiDragonsInfoCard = GetComponentInChildren<UiDragonsInfoCard>();
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

	public static void LoadDragonsStableUI()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = GameConfig.GetKeyData("DragonsStableAsset").Split('/');
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
			UiDragonsStable component = obj.GetComponent<UiDragonsStable>();
			if (mSetForceDragonSelection)
			{
				component.pUiDragonsListCard.pCurrentMode = UiDragonsListCard.Mode.ForceDragonSelection;
				component.pUiDragonsListCard.SetCloseButtonVisibility(visible: false);
				component.pUiDragonsInfoCard.SetCloseButtonVisibility(visible: false);
			}
			if (mIsNeedExclusive)
			{
				KAUI.SetExclusive(component);
				AvAvatar.EnableAllInputs(inActive: false);
			}
			if (pOnStablesUILoadHandler != null)
			{
				pOnStablesUILoadHandler(isSuccess: true);
			}
			if (mMsgObject != null)
			{
				mMsgObject.SendMessage("OnStableUIOpened", component, SendMessageOptions.DontRequireReceiver);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (pOnStablesUILoadHandler != null)
			{
				pOnStablesUILoadHandler(isSuccess: false);
			}
			break;
		}
	}

	public static void OpenDragonListUI(GameObject msgObject, Mode mode = Mode.DragonSelection, bool setExclusive = true, bool forceSelection = false)
	{
		mMsgObject = msgObject;
		mIsNeedExclusive = setExclusive;
		if (forceSelection)
		{
			mSetForceDragonSelection = true;
		}
		LoadDragonsStableUI();
		mCurrentMode = mode;
	}

	public static void OpenStableListUI(GameObject msgObject, bool setExclusive = true)
	{
		mMsgObject = msgObject;
		mIsNeedExclusive = setExclusive;
		LoadDragonsStableUI();
		mCurrentMode = Mode.StableSelection;
	}

	public static void OpenDragonInfoUI(GameObject msgObject, int petId = -1, bool setExclusive = true)
	{
		mMsgObject = msgObject;
		mIsNeedExclusive = setExclusive;
		LoadDragonsStableUI();
		mCurrentMode = Mode.DragonInfo;
		pSelectedPetID = petId;
	}

	private void InitUI()
	{
		UiCard.Transition childTrans = new UiCard.Transition(UiCard.Transition.Type.SlideOut, UiCard.Transition.Direction.Right, UiCard.Transition.ZOrder.Top);
		switch (mCurrentMode)
		{
		case Mode.DragonSelection:
			mSetForceDragonSelection = false;
			mUiDragonsListCard.PushCard(null, null, childTrans);
			mUiDragonsListCard.SetVisibility(inVisible: true);
			SetMessage(mUiDragonsListCard, _SelectDragonText);
			break;
		case Mode.StableSelection:
			mUiStablesListCard.PushCard(null, null, childTrans);
			mUiStablesListCard.SetVisibility(inVisible: true);
			SetMessage(mUiStablesListCard, _SelectStableText);
			break;
		case Mode.DragonNestAllocation:
			mUiDragonsListCard.PushCard(null, null, childTrans);
			mUiDragonsListCard.SetVisibility(inVisible: true);
			pUiDragonsInfoCard.SetMessageObject(mMsgObject);
			SetMessage(mUiDragonsListCard, _SelectDragonText);
			break;
		case Mode.DragonInfo:
			_CardEndPos = new Vector3(240f, _CardEndPos.y, _CardEndPos.z);
			pUiDragonsInfoCard.pSelectedPetID = pSelectedPetID;
			pUiDragonsInfoCard.PushCard(null, null, childTrans);
			pUiDragonsInfoCard.SetVisibility(inVisible: true);
			pUiDragonsInfoCard.RefreshUI();
			pUiDragonsInfoCard.SetMessageObject(mMsgObject);
			break;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mExitBtn == inWidget)
		{
			Exit();
		}
	}

	public void Interactivity(bool IsInteractive)
	{
		SetInteractive(IsInteractive);
		mUiStablesListCard.SetInteractive(IsInteractive);
		mUiStablesInfoCard.SetInteractive(IsInteractive);
		mUiDragonsListCard.SetInteractive(IsInteractive);
		mUiDragonsInfoCard.SetInteractive(IsInteractive);
	}

	public override void Exit()
	{
		if (mIsNeedExclusive)
		{
			KAUI.RemoveExclusive(this);
			AvAvatar.EnableAllInputs(inActive: true);
		}
		OnStableUIClosed();
		if (pOnStablesUIClosed != null)
		{
			pOnStablesUIClosed();
		}
		if (mMsgObject != null)
		{
			mMsgObject.SendMessage("OnStableUIClosed", SendMessageOptions.DontRequireReceiver);
		}
		Object.Destroy(base.gameObject);
	}

	private void OnStableUIClosed()
	{
		SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
		if (ShowCustomization)
		{
			SanctuaryManager.pInstance.LoadPetCustomizationScreen(string.Empty);
			ShowCustomization = false;
		}
		else if (pCurPetInstance != null && (!pCurPetInstance.pData.pIsNameCustomized || string.IsNullOrEmpty(pCurPetInstance.pData.Name.Trim())) && SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.LoadPetNameSelectionScreen();
		}
		else
		{
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.EnableAllInputs(inActive: true);
			AvAvatar.SetUIActive(inActive: true);
		}
	}

	public void SelectPet(int pID)
	{
		RaisedPetData byID = RaisedPetData.GetByID(pID);
		int result = -1;
		RaisedPetAttribute raisedPetAttribute = byID.FindAttrData("_LastCustomizedStage");
		if (raisedPetAttribute != null && !string.IsNullOrEmpty(raisedPetAttribute.Value))
		{
			int.TryParse(raisedPetAttribute.Value, out result);
		}
		ShowCustomization = (int)byID.pStage > result && SanctuaryData.GetPetCustomizationType(byID.PetTypeID) != PetCustomizationType.None;
	}
}
