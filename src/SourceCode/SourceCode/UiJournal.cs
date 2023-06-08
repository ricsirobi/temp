using System;
using System.Collections.Generic;
using UnityEngine;

public class UiJournal : KAUI
{
	public class TabHistory
	{
		public KAWidget mWidget;

		public int mIndex;

		public TabHistory(KAWidget widget, int index)
		{
			mWidget = widget;
			mIndex = index;
		}
	}

	[Serializable]
	public class CategoryInfo
	{
		public KAUI _CategoryScreen;

		public KAWidget _CategoryButton;

		public KAWidget _MobileCategoryButton;

		public string _CategoryName;
	}

	public static string EnterSelection;

	public static string SelectionWidget;

	public static string Message;

	public CategoryInfo[] _MainCategoriesList;

	public GameObject _ExitMessageObject;

	public string _OpenJournalTutBundleName = "RS_DATA/PfJournalTutorialManager.unity3d/PfJournalTutorialManager";

	private GameObject mInstantiatedObject;

	private KAWidget mBackBtn;

	private IJournal mCurrentUI;

	private List<TabHistory> mTabHistory = new List<TabHistory>();

	private KAWidget mPreviousTabWidget;

	private int mPreviousTabUiIndex;

	private bool mIsClosing;

	private bool mInitExit;

	private AvAvatarState mStateBeforeJournal = AvAvatarState.IDLE;

	private AvAvatarSubState mSubStateBeforeJournal;

	[NonSerialized]
	public CoIdleManager mIdleMgr;

	private static string mJournalExitScene;

	private Action mExitAction;

	private bool mDoCheck = true;

	private AISanctuaryPetFSM mPrevPetState;

	private InteractiveTutManager tutManager;

	protected static bool mIsJournalActive;

	protected static UiJournal mInstance;

	private bool mCanDestroyLoadScreen;

	public static string pJournalExitScene
	{
		get
		{
			return mJournalExitScene;
		}
		set
		{
			mJournalExitScene = value;
		}
	}

	public static bool pIsJournalActive => mIsJournalActive;

	public static UiJournal pInstance => mInstance;

	public static event OnJournalClosed JournalClosed;

	protected override void Start()
	{
		base.Start();
		mIsJournalActive = true;
		mInstance = this;
		KAInput.pInstance.EnableInputType("CameraZoom", InputType.MOUSE, inEnable: false);
		AvAvatar.SetUIActive(inActive: false);
		UpdatePetState();
		mStateBeforeJournal = AvAvatar.pState;
		AvAvatar.pState = AvAvatarState.NONE;
		mSubStateBeforeJournal = AvAvatar.pSubState;
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		mBackBtn = FindItem("BackBtn");
		mBackBtn.SetVisibility(inVisible: false);
		mPreviousTabWidget = FindItem("QuestBtn");
		mPreviousTabWidget.SetInteractive(isInteractive: false);
		mPreviousTabWidget.GetComponent<KAToggleButton>().SetChecked(isChecked: true);
		mPreviousTabUiIndex = 0;
		TabHistory item = new TabHistory(mPreviousTabWidget, mPreviousTabUiIndex);
		mTabHistory.Add(item);
		if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("JournalScene"))
		{
			mCanDestroyLoadScreen = true;
		}
		mIdleMgr = GetComponent<CoIdleManager>();
		if (mIdleMgr != null)
		{
			mIdleMgr.StartIdles();
		}
		if (!UtPlatform.IsMobile())
		{
			return;
		}
		for (int i = 0; i < _MainCategoriesList.Length; i++)
		{
			if (_MainCategoriesList[i]._MobileCategoryButton != null)
			{
				_MainCategoriesList[i]._MobileCategoryButton.SetVisibility(inVisible: true);
				if (_MainCategoriesList[i]._CategoryButton != null)
				{
					_MainCategoriesList[i]._CategoryButton.SetVisibility(inVisible: false);
				}
			}
		}
	}

	protected override void Update()
	{
		if (mCanDestroyLoadScreen)
		{
			if (SanctuaryManager.pCurPetInstance != null || SanctuaryManager.pCurPetData == null)
			{
				RsResourceManager.DestroyLoadScreen();
				mCanDestroyLoadScreen = false;
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(EnterSelection))
			{
				for (int i = 0; i < _MainCategoriesList.Length; i++)
				{
					_MainCategoriesList[i]._CategoryScreen.SetVisibility(inVisible: false);
				}
				OnClick(FindItem(EnterSelection));
				mDoCheck = false;
				EnterSelection = "";
			}
			if (string.IsNullOrEmpty(EnterSelection) && mDoCheck)
			{
				mDoCheck = false;
				if (!CheckForQuests("Action", "Name", "OpenJournal", "QuestBtn") && UiFieldGuide.pRecentlyUnlockedChapters != null && UiFieldGuide.pRecentlyUnlockedChapters.Count > 0)
				{
					OnClick(FindItem("FieldGuideBtn"));
				}
			}
			if (mCurrentUI != null)
			{
				if (mCurrentUI.IsBusy() && GetState() == KAUIState.INTERACTIVE)
				{
					SetInteractive(interactive: false);
					if (tutManager != null)
					{
						tutManager.SetTutorialBoardInteractive(flag: false);
					}
				}
				else if (!mCurrentUI.IsBusy() && GetState() == KAUIState.NOT_INTERACTIVE)
				{
					SetInteractive(interactive: true);
					if (tutManager != null)
					{
						tutManager.SetTutorialBoardInteractive(flag: true);
					}
				}
			}
			if (mIsClosing)
			{
				bool flag = true;
				for (int j = 0; j < _MainCategoriesList.Length; j++)
				{
					IJournal journal = (IJournal)_MainCategoriesList[j]._CategoryScreen;
					if (journal != null && journal.IsBusy())
					{
						flag = false;
						break;
					}
				}
				if (flag && !mInitExit)
				{
					ExitJournal();
				}
			}
			if (!mInitExit && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.enabled)
			{
				SanctuaryManager.pCurPetInstance.enabled = false;
			}
		}
		base.Update();
	}

	protected virtual void OnDisable()
	{
		mIsJournalActive = false;
	}

	protected virtual void OnEnable()
	{
		mIsJournalActive = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		mInstance = null;
	}

	protected bool CheckForQuests(string inTaskType, string inTaskKey, string inTaskValue, string inTargetScreenBtn)
	{
		if (MissionManager.IsTaskActive(inTaskType, inTaskKey, inTaskValue))
		{
			KAWidget kAWidget = FindItem(inTargetScreenBtn);
			if (kAWidget != null)
			{
				OnClick(kAWidget);
			}
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForTaskCompletion("Action", inTaskValue);
			}
			if (!string.IsNullOrEmpty(_OpenJournalTutBundleName))
			{
				string[] array = _OpenJournalTutBundleName.Split('/');
				if (array.Length == 1)
				{
					GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(array[0]);
					if (gameObject != null)
					{
						ShowJournalTutorial(gameObject);
					}
				}
				else
				{
					KAUICursorManager.SetDefaultCursor("Loading");
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], TutBundleLoadEvent, typeof(GameObject));
				}
			}
			return true;
		}
		return false;
	}

	public void CloseJournal()
	{
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		for (int i = 0; i < _MainCategoriesList.Length; i++)
		{
			((IJournal)_MainCategoriesList[i]._CategoryScreen)?.ProcessClose();
		}
		mIsClosing = true;
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		for (int i = 0; i < _MainCategoriesList.Length; i++)
		{
			if ((_MainCategoriesList[i]._CategoryButton != null && _MainCategoriesList[i]._CategoryButton == item) || (_MainCategoriesList[i]._MobileCategoryButton != null && _MainCategoriesList[i]._MobileCategoryButton == item))
			{
				ActivateUI(_MainCategoriesList[i]._CategoryButton, i);
				break;
			}
		}
		if (item.name == "CloseBtn")
		{
			CloseJournal();
		}
		else if (item.name == "BackBtn" && mTabHistory.Count > 0)
		{
			int index = mTabHistory.Count - 1;
			TabHistory tabHistory = mTabHistory[index];
			((KAToggleButton)tabHistory.mWidget).SetChecked(isChecked: true);
			ActivateUI(tabHistory.mWidget, tabHistory.mIndex, addToList: false);
			mTabHistory.RemoveAt(index);
			if (mTabHistory.Count == 1)
			{
				mPreviousTabUiIndex = 0;
				mBackBtn.SetVisibility(inVisible: false);
			}
		}
	}

	public void PopUpStoreUI(string store, string category, string tab)
	{
		mExitAction = delegate
		{
			StoreLoader.Load(setDefaultMenuItem: true, category, store, null, UILoadOptions.AUTO, tab);
		};
		CloseJournal();
	}

	public void GoToScene(string scene)
	{
		mExitAction = delegate
		{
			RsResourceManager.LoadLevel(scene);
		};
		CloseJournal();
	}

	public void OpenBlacksmith(string selectionWidget)
	{
		mExitAction = delegate
		{
			if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("JournalScene"))
			{
				AvAvatar.SetActive(inActive: false);
				if (SanctuaryManager.pCurPetInstance != null)
				{
					SanctuaryManager.pCurPetInstance.enabled = false;
				}
			}
			UiBlacksmith.OnClosed = delegate
			{
				JournalLoader.Load("EquipBtn", selectionWidget, setDefaultMenuItem: true, null, resetLastSceneRef: false);
			};
			UiBlacksmith.Init(Mode.ENHANCE);
		};
		CloseJournal();
	}

	private void TutBundleLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			gameObject.name = ((GameObject)inObject).name;
			ShowJournalTutorial(gameObject);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			Debug.LogError("Error loading Tutorial! " + inURL);
			break;
		}
	}

	private void ShowJournalTutorial(GameObject go)
	{
		if (!(go == null))
		{
			tutManager = go.GetComponent<InteractiveTutManager>();
			if (tutManager != null)
			{
				tutManager.ShowTutorial();
				InteractiveTutManager interactiveTutManager = tutManager;
				interactiveTutManager._StepStartedEvent = (StepStartedEvent)Delegate.Combine(interactiveTutManager._StepStartedEvent, new StepStartedEvent(TutorialStepStartEvent));
				InteractiveTutManager interactiveTutManager2 = tutManager;
				interactiveTutManager2._StepEndedEvent = (StepEndedEvent)Delegate.Combine(interactiveTutManager2._StepEndedEvent, new StepEndedEvent(TutorialStepEndedEvent));
			}
		}
	}

	public void TutorialStepStartEvent(int stepIdx, string stepName)
	{
		for (int i = 0; i < _MainCategoriesList.Length; i++)
		{
			if (_MainCategoriesList[i]._CategoryName == stepName && stepName != "Profile")
			{
				OnClick(_MainCategoriesList[i]._CategoryButton);
			}
		}
	}

	public void TutorialStepEndedEvent(int stepIdx, string stepName, bool tutQuit)
	{
		if (stepIdx >= tutManager._TutSteps.Length - 1)
		{
			tutManager = null;
		}
	}

	private void UpdatePetState()
	{
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.AIActor != null)
		{
			mPrevPetState = SanctuaryManager.pCurPetInstance.AIActor._State;
			SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.CUSTOM);
		}
	}

	private void ExitJournal()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mInitExit = true;
		KAInput.pInstance.EnableInputType("CameraZoom", InputType.MOUSE, inEnable: true);
		if (mCurrentUI != null)
		{
			mCurrentUI.Exit();
			mCurrentUI = null;
		}
		for (int i = 0; i < _MainCategoriesList.Length; i++)
		{
			((IJournal)_MainCategoriesList[i]._CategoryScreen).Clear();
		}
		UnityEngine.Object.Destroy(mInstantiatedObject);
		mInstance = null;
		MissionManagerDO missionManagerDO = (MissionManagerDO)MissionManager.pInstance;
		List<string> taskObjectiveScenes = missionManagerDO.GetTaskObjectiveScenes();
		string text = missionManagerDO.UpdatedScene(RsResourceManager.pCurrentLevel, taskObjectiveScenes);
		if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("JournalScene") && mExitAction == null)
		{
			SetVisibility(inVisible: false);
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null && (component.IsValidLastPositionOnGround() || mSubStateBeforeJournal == AvAvatarSubState.UWSWIMMING))
			{
				AvAvatar.pStartLocation = AvAvatar.pSpawnAtSetPosition;
			}
			string text2 = null;
			text2 = ((!string.IsNullOrEmpty(pJournalExitScene)) ? pJournalExitScene : RsResourceManager.pLastLevel);
			string text3 = missionManagerDO.UpdatedScene(text2, taskObjectiveScenes);
			if (!text3.Equals(text2))
			{
				AvAvatar.pStartLocation = null;
			}
			RsResourceManager.LoadLevel(text3);
		}
		else if (mExitAction == null && text != RsResourceManager.pCurrentLevel)
		{
			if (UiJournal.JournalClosed != null)
			{
				UiJournal.JournalClosed();
			}
			if (_ExitMessageObject != null)
			{
				_ExitMessageObject.SendMessage("OnJournalExit", SendMessageOptions.DontRequireReceiver);
			}
			AvAvatar.pStartLocation = null;
			RsResourceManager.LoadLevel(text);
		}
		else
		{
			AvAvatar.pState = mStateBeforeJournal;
			AvAvatar.pSubState = mSubStateBeforeJournal;
			AvAvatar.SetActive(inActive: true);
			if (AvAvatar.pToolbar != null)
			{
				UiToolbar component2 = AvAvatar.pToolbar.GetComponent<UiToolbar>();
				if (component2 != null)
				{
					component2.OnUpdateRank();
				}
			}
			if (AvAvatar.pAvatarCam != null)
			{
				AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>().SetLookAt(AvAvatar.mTransform, null, 0f);
			}
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			RsResourceManager.Unload(GameConfig.GetKeyData("JournalAsset"));
			RsResourceManager.UnloadUnusedAssets();
			if (UiJournal.JournalClosed != null)
			{
				UiJournal.JournalClosed();
			}
			if (_ExitMessageObject != null)
			{
				_ExitMessageObject.SendMessage("OnJournalExit", SendMessageOptions.DontRequireReceiver);
			}
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.gameObject.SetActive(value: true);
			SanctuaryManager.pCurPetInstance.enabled = true;
			if (SanctuaryManager.pCurPetInstance.AIActor != null)
			{
				SanctuaryManager.pCurPetInstance.AIActor.SetState(mPrevPetState);
			}
		}
		if (mExitAction != null)
		{
			mExitAction();
		}
	}

	public void OnAvatarEquipmentLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			SetInteractive(interactive: true);
			UiAvatarEquipment componentInChildren = (mInstantiatedObject = UnityEngine.Object.Instantiate((GameObject)inObject)).GetComponentInChildren<UiAvatarEquipment>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = true;
				componentInChildren._CloseMsgObject = base.gameObject;
				componentInChildren.OnOpen();
			}
			SetVisibility(inVisible: false);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			SetInteractive(interactive: true);
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	public void ActivateUI(string uiCategoryName, bool addToList = true)
	{
		for (int i = 0; i < _MainCategoriesList.Length; i++)
		{
			if (_MainCategoriesList[i]._CategoryName == uiCategoryName)
			{
				OnClick(_MainCategoriesList[i]._CategoryButton);
			}
		}
	}

	public void ActivateUI(int uiIndex, bool addToList = true)
	{
		if (uiIndex >= 0 && uiIndex < _MainCategoriesList.Length)
		{
			OnClick(_MainCategoriesList[uiIndex]._CategoryButton);
		}
	}

	public void OpenTalentTree()
	{
		OnClick(FindItem("TalentTreeBtn"));
	}

	public void CloseTalentTree()
	{
		ActivateUI("Customization");
	}

	private void ActivateUI(KAWidget item, int uiIndex, bool addToList = true)
	{
		for (int i = 0; i < _MainCategoriesList.Length; i++)
		{
			_MainCategoriesList[i]._CategoryScreen.SetVisibility((i == uiIndex) ? true : false);
		}
		item.SetInteractive(isInteractive: false);
		if (mPreviousTabWidget != null)
		{
			mPreviousTabWidget.SetInteractive(isInteractive: true);
		}
		if (mCurrentUI != null)
		{
			mCurrentUI.Exit();
		}
		mCurrentUI = (IJournal)_MainCategoriesList[uiIndex]._CategoryScreen;
		if (!string.IsNullOrEmpty(Message))
		{
			_MainCategoriesList[uiIndex]._CategoryScreen.gameObject.SendMessage(Message);
			Message = "";
		}
		if (!string.IsNullOrEmpty(SelectionWidget))
		{
			KAUI categoryScreen = _MainCategoriesList[uiIndex]._CategoryScreen;
			if (categoryScreen != null)
			{
				categoryScreen.OnClick(categoryScreen.FindItem(SelectionWidget));
			}
			SelectionWidget = "";
		}
		if (addToList)
		{
			TabHistory item2 = new TabHistory(mPreviousTabWidget, mPreviousTabUiIndex);
			mTabHistory.Add(item2);
		}
		mPreviousTabUiIndex = uiIndex;
		mPreviousTabWidget = item;
		UnityEngine.Object.Destroy(mInstantiatedObject);
	}

	private void OnAvatarEquipmentClose()
	{
		SetVisibility(inVisible: true);
	}

	private void OnDragonEquipmentClose()
	{
		SetVisibility(inVisible: true);
	}

	static UiJournal()
	{
		UiJournal.JournalClosed = null;
		EnterSelection = "";
		SelectionWidget = "";
		Message = "";
		mJournalExitScene = "";
		mIsJournalActive = false;
	}
}
