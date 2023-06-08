using System;
using System.Collections;
using UnityEngine;

public class UiAvatarCSM : KAUI
{
	public string _BuddyListResourceName = "RS_DATA/PfUiBuddyListDO.unity3d/PfUiBuddyListDO";

	public UiActions _UiActions;

	public float _CSMRadius = 200f;

	public int _ClanOpenTaskID = 3453;

	public int _JournalOpenTaskID = 3455;

	public int _CSMOpenTaskID = 5797;

	public ParticleSystem _FTUEAvatarSelect;

	public Vector3 _AvatarSelectParticleOffset;

	public KAUI _MiniGamePanelUI;

	private KAWidget mBtnCSMClose;

	private KAWidget mBtnCSMBackpack;

	private KAWidget mBtnCSMBuddies;

	private KAWidget mBtnCSMEmotes;

	private KAWidget mBtnCSMJournal;

	private KAWidget mBtnCSMMyAvatar;

	private KAWidget mBtnCSMClan;

	private KAWidget mBtnCSMMessages;

	private KAWidget mBtnCSMProfile;

	private KAWidget mBtnCSMMyDragons;

	private KAWidget mBtnCSMMiniGames;

	private KAWidget mBtnCSMSettings;

	private KAWidget mBtnCSMHatchery;

	private bool mInitBackpack;

	private bool mOpenCSM;

	private ParticleSystem mFTUEAvatarSelect;

	private KAWidget mBtnCSMDragonTactics;

	private KAWidget mBtnCSMFlightSchool;

	private KAWidget mBtnCSMDragonRacing;

	private KAWidget mBtnCSMFireballFrenzy;

	private KAWidget mBtnCSMMiniGameBack;

	private KAWidget mBtnCSMMiniGameStables;

	private KAWidget mBtnCSMMiniGameFarming;

	private KAWidget mBtnCSMMiniGameHideout;

	private UiToolbar mToolbar;

	protected override void Start()
	{
		base.Start();
		Initialize();
		if (AvAvatar.pObject != null)
		{
			Collider[] componentsInChildren = AvAvatar.pObject.GetComponentsInChildren<CapsuleCollider>();
			Collider[] array = componentsInChildren;
			if (array != null)
			{
				componentsInChildren = array;
				foreach (Collider collider in componentsInChildren)
				{
					if (collider.gameObject.tag == "AvatarCSMCollider")
					{
						ObClickable component = collider.gameObject.GetComponent<ObClickable>();
						if (component != null)
						{
							component._ObjectClickedCallback = OnClick;
						}
					}
				}
			}
		}
		CoCommonLevel.WaitListCompleted += OnWaitListCompleted;
		UiJournal.JournalClosed += UpdateAvatarParticleOnLoad;
		MissionManager.AddMissionEventHandler(OnMissionEvent);
	}

	private void Initialize()
	{
		_MiniGamePanelUI.gameObject.SetActive(value: false);
		mBtnCSMClose = FindItem("BtnCSMClose");
		mBtnCSMBackpack = FindItem("BtnCSMBackpack");
		mBtnCSMBuddies = FindItem("BtnCSMBuddies");
		mBtnCSMMyDragons = FindItem("BtnCSMMyDragons");
		mBtnCSMJournal = FindItem("BtnCSMJournal");
		mBtnCSMMyAvatar = FindItem("BtnCSMMyAvatar");
		mBtnCSMClan = FindItem("BtnCSMClan");
		mBtnCSMMessages = FindItem("BtnCSMMessages");
		mBtnCSMMiniGames = FindItem("BtnCSMMiniGames");
		mBtnCSMSettings = FindItem("BtnCSMSettings");
		mBtnCSMHatchery = FindItem("BtnCSMHatchery");
		mBtnCSMDragonTactics = _MiniGamePanelUI.FindItem("BtnCSMDragonTactics");
		mBtnCSMFlightSchool = _MiniGamePanelUI.FindItem("BtnCSMFlightSchool");
		mBtnCSMDragonRacing = _MiniGamePanelUI.FindItem("BtnCSMDragonRacing");
		mBtnCSMFireballFrenzy = _MiniGamePanelUI.FindItem("BtnCSMFireballFrenzy");
		mBtnCSMMiniGameBack = _MiniGamePanelUI.FindItem("BtnCSMMiniGameBack");
		mBtnCSMMiniGameStables = _MiniGamePanelUI.FindItem("BtnCSMStables");
		mBtnCSMMiniGameHideout = _MiniGamePanelUI.FindItem("BtnCSMHideout");
		mBtnCSMMiniGameFarming = _MiniGamePanelUI.FindItem("BtnCSMFarming");
		_MiniGamePanelUI.pEvents.OnClick += OnClick;
	}

	private void RepositionItems(GameObject obj = null)
	{
		KAButton[] componentsInChildren = (obj ?? base.gameObject).GetComponentsInChildren<KAButton>();
		int num = componentsInChildren.Length;
		float num2 = 360 / (num - 1);
		float num3 = 90f;
		KAButton[] array = componentsInChildren;
		foreach (KAButton kAButton in array)
		{
			if (!(kAButton == mBtnCSMClose) && !(kAButton == mBtnCSMMiniGameBack))
			{
				Vector2 vector = new Vector2(_CSMRadius * Mathf.Cos(MathF.PI / 180f * num3), _CSMRadius * Mathf.Sin(MathF.PI / 180f * num3));
				kAButton.SetPosition(vector.x, vector.y);
				num3 -= num2;
			}
		}
	}

	private void OnClick(GameObject gameobject)
	{
		mOpenCSM = true;
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget == mBtnCSMClose)
		{
			Close(resetAvatarState: true);
		}
		else if (inWidget == mBtnCSMBackpack)
		{
			OpenBackpack();
		}
		else if (inWidget == mBtnCSMBuddies)
		{
			if (UiBuddyList.pInstance != null)
			{
				UnityEngine.Object.Destroy(UiBuddyList.pInstance.gameObject);
				return;
			}
			Close(resetAvatarState: true);
			UiBuddyList.ShowBuddyList(_BuddyListResourceName);
			base.gameObject.BroadcastMessage("OnCloseWindows", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		else if (inWidget == mBtnCSMEmotes)
		{
			if (_UiActions != null)
			{
				Close(resetAvatarState: true);
				_UiActions.ShowEmoticons();
			}
		}
		else if (inWidget == mBtnCSMJournal)
		{
			Close(resetAvatarState: true);
			JournalLoader.Load("", "", setDefaultMenuItem: false, null);
		}
		else if (inWidget == mBtnCSMMyAvatar)
		{
			Close(resetAvatarState: true);
			JournalLoader.Load("EquipBtn", "", setDefaultMenuItem: true, null);
		}
		else if (inWidget == mBtnCSMClan)
		{
			Close();
			base.gameObject.BroadcastMessage("OnCloseWindows", base.gameObject, SendMessageOptions.DontRequireReceiver);
			UiClans.ShowClan(UserInfo.pInstance.UserID);
		}
		else if (inWidget == mBtnCSMProfile)
		{
			Close();
			ProfileLoader.ShowProfile(UserInfo.pInstance.UserID);
		}
		else if (inWidget == mBtnCSMMessages)
		{
			Close();
			MessageBoardLoader.Load(UserInfo.pInstance.UserID);
		}
		else if (inWidget == mBtnCSMMiniGames)
		{
			EnableButtons(enable: false, includeParent: false);
			LoadMinigamesMenu();
		}
		else if (inWidget == mBtnCSMMiniGameBack)
		{
			OnMiniGameBack();
		}
		else if (inWidget == mBtnCSMMiniGameStables)
		{
			LoadScene("DragonStableINTDO");
		}
		else if (inWidget == mBtnCSMMiniGameFarming)
		{
			LoadScene("FarmingDO");
		}
		else if (inWidget == mBtnCSMMiniGameHideout)
		{
			LoadScene("MyRoomINTDO");
		}
		else if (inWidget == mBtnCSMSettings)
		{
			Close(resetAvatarState: true);
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = GameConfig.GetKeyData("OptionsAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], mToolbar.OptionsBundleReady, typeof(GameObject));
		}
		else if (inWidget == mBtnCSMMyDragons)
		{
			Close(resetAvatarState: true);
			if (mToolbar != null && mToolbar.IsActive())
			{
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.EnableAllInputs(inActive: false);
				AvAvatar.SetUIActive(inActive: false);
				UiDragonsStable.OpenDragonListUI(base.gameObject);
			}
		}
		else if (inWidget == mBtnCSMFlightSchool || inWidget == mBtnCSMDragonRacing || inWidget == mBtnCSMFireballFrenzy || inWidget == mBtnCSMDragonTactics)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
			inWidget.SendMessage("OnButtonClick");
			Close();
		}
		else if (inWidget == mBtnCSMHatchery)
		{
			if (RsResourceManager.pCurrentLevel == "HatcheryINTDO")
			{
				UnityEngine.Object.FindObjectOfType<HatcheryManager>()?.Init();
				Close(resetAvatarState: true);
				return;
			}
			Close(resetAvatarState: true);
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array2 = GameConfig.GetKeyData("HatcheryAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], HatcheryBundleReady, typeof(GameObject));
		}
	}

	public void HatcheryBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			UnityEngine.Object.Instantiate((GameObject)inObject).name = "PfUiMultiEggHatchingWorld";
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	public void LoadScene(string sceneName)
	{
		if (RsResourceManager.pCurrentLevel != sceneName)
		{
			RsResourceManager.LoadLevel(sceneName);
		}
	}

	private void OpenBackpack()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		AvAvatar.pState = AvAvatarState.PAUSED;
		mInitBackpack = true;
	}

	protected override void Update()
	{
		base.Update();
		if (InteractiveTutManager._CurrentActiveTutorialObject == null)
		{
			if (mOpenCSM && !IsActive())
			{
				OpenCSM();
			}
			else if (Input.GetMouseButtonUp(0) && IsActive() && (KAUIManager.pInstance.pSelectedWidget == null || (KAUIManager.pInstance.pSelectedWidget != null && KAUIManager.pInstance.pSelectedWidget.pUI != this)) && mFTUEAvatarSelect == null && ((mBtnCSMClose != null && mBtnCSMClose.IsActive()) || (mBtnCSMMiniGameBack != null && mBtnCSMMiniGameBack.IsActive())))
			{
				Close(resetAvatarState: true);
			}
		}
		if (mToolbar == null && AvAvatar.pToolbar != null)
		{
			mToolbar = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		}
		if (mInitBackpack && CommonInventoryData.pIsReady)
		{
			mInitBackpack = false;
			Close();
			UiBackpack.Init(mToolbar._InventoryCategory);
		}
		if (mToolbar.AllowHotKeys() && !FUEManager.pIsFUERunning && AllowedStates())
		{
			if (KAInput.GetButtonUp("DragonBook"))
			{
				JournalLoader.Load("FieldGuideBtn", "", setDefaultMenuItem: false, null);
			}
			else if (KAInput.GetButtonUp("QuestLog"))
			{
				JournalLoader.Load("QuestBtn", string.Empty, setDefaultMenuItem: false, null);
			}
			else if (KAInput.GetButtonUp("Inventory") && UiBackpack.pInstance == null)
			{
				OpenBackpack();
			}
			else if (KAInput.GetButtonUp("Clan"))
			{
				UiClans.ShowClan(UserInfo.pInstance.UserID);
			}
		}
	}

	private bool AllowedStates()
	{
		if (AvAvatar.pObject != null && AvAvatar.pObject.GetComponent<AvAvatarController>().pPlayerCarrying)
		{
			return false;
		}
		if (AvAvatar.pLevelState != AvAvatarLevelState.RACING && AvAvatar.pLevelState != AvAvatarLevelState.TARGETPRACTICE && AvAvatar.pSubState != AvAvatarSubState.GLIDING && AvAvatar.pSubState != AvAvatarSubState.FLYING && AvAvatar.pLevelState != AvAvatarLevelState.FLIGHTSCHOOL)
		{
			return AvAvatar.pSubState != AvAvatarSubState.WALLCLIMB;
		}
		return false;
	}

	public void Close(bool resetAvatarState = false)
	{
		if (resetAvatarState)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
		}
		if (_MiniGamePanelUI != null && _MiniGamePanelUI.GetVisibility())
		{
			_MiniGamePanelUI.gameObject.SetActive(value: false);
			_MiniGamePanelUI.SetVisibility(inVisible: false);
		}
		KAUI.RemoveExclusive(this);
		EnableButtons(enable: false);
	}

	public void OnMessageCount(int count)
	{
		KAWidget kAWidget = (UtPlatform.IsMobile() ? mBtnCSMMessages : mBtnCSMProfile);
		if (!(kAWidget == null))
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("AniAlert");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(count != 0);
				kAWidget2.SetText(count.ToString());
			}
		}
	}

	public void OpenCSM()
	{
		if (MissionManager.pInstance != null)
		{
			Task task = MissionManagerDO.GetPlayerActiveTask() ?? MissionManagerDO.GetNextActiveTask();
			if (task != null && (task.TaskID == _JournalOpenTaskID || task.TaskID == _ClanOpenTaskID))
			{
				UpdateCSMForTask(isTaskActive: true);
			}
			MissionManager.pInstance.CheckForTaskCompletion("Action", "OpenCSM");
		}
		if (_UiActions != null && _UiActions.GetVisibility())
		{
			_UiActions.SetVisibility(inVisible: false);
		}
		if (AvAvatar.pState == AvAvatarState.MOVING)
		{
			AvAvatar.pObject.GetComponent<AvAvatarController>().pVelocity = Vector3.zero;
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUI.SetExclusive(this);
		EnableButtons();
		RepositionItems();
		mOpenCSM = false;
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		switch (inEvent)
		{
		case MissionEvent.OFFER_COMPLETE:
		{
			MissionManager.Action action = (MissionManager.Action)inObject;
			if (action._Object != null && action._Object.GetType() == typeof(Task))
			{
				Task task3 = (Task)action._Object;
				if (task3 != null && (task3.TaskID == _ClanOpenTaskID || task3.TaskID == _JournalOpenTaskID || task3.TaskID == _CSMOpenTaskID) && _FTUEAvatarSelect != null)
				{
					mFTUEAvatarSelect = UnityEngine.Object.Instantiate(_FTUEAvatarSelect, AvAvatar.pObject.transform);
					mFTUEAvatarSelect.transform.parent = AvAvatar.pObject.transform;
					mFTUEAvatarSelect.transform.localPosition = _AvatarSelectParticleOffset;
					StartCoroutine(PlayParticle());
				}
			}
			break;
		}
		case MissionEvent.TASK_COMPLETE:
		{
			Task task2 = (Task)inObject;
			if (task2 != null)
			{
				KAWidget pointerBtn = null;
				if (task2.TaskID == _JournalOpenTaskID)
				{
					pointerBtn = mBtnCSMJournal;
				}
				else if (task2.TaskID == _ClanOpenTaskID)
				{
					pointerBtn = mBtnCSMClan;
				}
				UpdateCSMForTask(pointerBtn, isTaskActive: false);
			}
			break;
		}
		case MissionEvent.TASK_STARTED:
			if (inObject is Task task && task.TaskID != _JournalOpenTaskID && task.TaskID != _ClanOpenTaskID && (MissionManager.IsTaskActive("Action", "Name", "OpenJournal") || MissionManager.IsTaskActive("Action", "Name", "OpenClan")))
			{
				UpdateCSMForTask(isTaskActive: false);
			}
			break;
		}
	}

	private void UpdateCSMForTask(bool isTaskActive)
	{
		KAWidget pointerBtn = null;
		bool flag = InteractiveTutManager._CurrentActiveTutorialObject == null;
		if (MissionManager.IsTaskActive("Action", "Name", "OpenJournal") && flag)
		{
			pointerBtn = mBtnCSMJournal;
		}
		if (MissionManager.IsTaskActive("Action", "Name", "OpenClan") && flag)
		{
			pointerBtn = mBtnCSMClan;
		}
		UpdateCSMForTask(pointerBtn, isTaskActive);
	}

	private void UpdateCSMForTask(KAWidget pointerBtn, bool isTaskActive)
	{
		if (mFTUEAvatarSelect != null)
		{
			UnityEngine.Object.Destroy(mFTUEAvatarSelect.gameObject);
		}
		if (!(pointerBtn != null))
		{
			return;
		}
		KAButton[] componentsInChildren = base.gameObject.GetComponentsInChildren<KAButton>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetDisabled(isTaskActive);
		}
		if (isTaskActive)
		{
			pointerBtn.SetDisabled(!isTaskActive);
		}
		KAWidget kAWidget = pointerBtn.FindChildItem("AniPointer");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(isTaskActive);
			if (isTaskActive)
			{
				kAWidget.PlayAnim("Play");
			}
			else
			{
				kAWidget.StopAnim("Play");
			}
		}
	}

	private IEnumerator PlayParticle()
	{
		yield return new WaitForEndOfFrame();
		if (mFTUEAvatarSelect != null)
		{
			mFTUEAvatarSelect.Play();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		UiJournal.JournalClosed -= UpdateAvatarParticleOnLoad;
		if (mFTUEAvatarSelect != null)
		{
			UnityEngine.Object.Destroy(mFTUEAvatarSelect.gameObject);
		}
	}

	public void MakeAvatarParticleActive(bool isVisible)
	{
		if (mFTUEAvatarSelect != null)
		{
			mFTUEAvatarSelect.gameObject.SetActive(isVisible);
		}
	}

	public void OnWaitListCompleted()
	{
		CoCommonLevel.WaitListCompleted -= OnWaitListCompleted;
		if (AvAvatar.pLevelState != AvAvatarLevelState.RACING && AvAvatar.pLevelState != AvAvatarLevelState.TARGETPRACTICE && AvAvatar.pSubState != AvAvatarSubState.GLIDING && AvAvatar.pLevelState != AvAvatarLevelState.FLIGHTSCHOOL && !IsActive())
		{
			UpdateAvatarParticleOnLoad();
		}
	}

	private void UpdateAvatarParticleOnLoad()
	{
		Task task = MissionManagerDO.GetPlayerActiveTask() ?? MissionManagerDO.GetNextActiveTask();
		if (task != null && (task.TaskID == _JournalOpenTaskID || task.TaskID == _ClanOpenTaskID || task.TaskID == _CSMOpenTaskID))
		{
			if (mFTUEAvatarSelect == null && _FTUEAvatarSelect != null)
			{
				mFTUEAvatarSelect = UnityEngine.Object.Instantiate(_FTUEAvatarSelect, AvAvatar.pObject.transform);
				mFTUEAvatarSelect.transform.parent = AvAvatar.pObject.transform;
				mFTUEAvatarSelect.transform.localPosition = _AvatarSelectParticleOffset;
				StartCoroutine(PlayParticle());
			}
			else
			{
				MakeAvatarParticleActive(isVisible: true);
			}
		}
		else if (mFTUEAvatarSelect != null)
		{
			UnityEngine.Object.Destroy(mFTUEAvatarSelect.gameObject);
		}
	}

	private void LoadMinigamesMenu()
	{
		_MiniGamePanelUI.SetVisibility(inVisible: true);
		_MiniGamePanelUI.gameObject.SetActive(value: true);
		RepositionItems(_MiniGamePanelUI.gameObject);
	}

	private void EnableButtons(bool enable = true, bool includeParent = true)
	{
		KAButton[] componentsInChildren = base.gameObject.GetComponentsInChildren<KAButton>(includeInactive: false);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetVisibility(enable);
		}
		if (includeParent)
		{
			SetVisibility(enable);
		}
	}

	private void OnMiniGameBack()
	{
		if (!(_MiniGamePanelUI == null))
		{
			_MiniGamePanelUI.SetVisibility(inVisible: false);
			_MiniGamePanelUI.gameObject.SetActive(value: false);
			mOpenCSM = true;
			SetVisibility(inVisible: false);
		}
	}

	public void OnStableUIOpened(UiDragonsStable UiStable)
	{
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "StableClicked");
		}
	}
}
