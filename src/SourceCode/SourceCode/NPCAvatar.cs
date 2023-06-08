using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DG.Tweening;
using SWS;
using UnityEngine;

public class NPCAvatar : ChCharacter
{
	[Serializable]
	public class SeasonalDlg
	{
		public string _StartDate;

		public string _EndDate;

		public AudioClip[] _Clip;
	}

	[Serializable]
	public class MissionAvailableActivate
	{
		public List<int> _MissionID;

		public GameObject _Object;
	}

	public static GameObject _Engaged;

	public Vector3 _EngageOffset = new Vector3(0f, 1f, 2f);

	public float _EngageFocusHeight = 1f;

	public string _IdleAnimName = "IdleNorm";

	public string _MouthAnimName = "EngageTalkNorm";

	public string[] _FidgetAnimName;

	public Texture _Icon;

	public int _MissionGroupID = -2;

	public string _MissionBoardAsset = "PfUiNPCMissionBoard";

	public string _MissionBoardAssetMobile = "PfUiNPCMissionBoardFull";

	public List<MissionAvailableActivate> _MissionAvailableActivate;

	public LocaleString _ItemAndText = new LocaleString("and");

	public AudioClip[] _DlgGreeting;

	public AudioClip[] _DlgGeneric;

	public AudioClip[] _DlgHelp;

	public AudioClip[] _DlgThankYou;

	public AudioClip[] _DlgRefuse;

	public AudioClip[] _DlgUpsell;

	public AudioClip[] _DlgBirthday;

	public SeasonalDlg[] _DlgHoliday;

	public GameObject _QuestIcon;

	public GameObject _RewardIcon;

	public WeaponManager _WeaponManager;

	public Transform _HandT;

	public Vector3 _AttachOffset = Vector3.zero;

	public KAWidget _NameTag;

	public float _NameTagFadeTime = 0.25f;

	private UiNPCMissionBoard mMissionListMenu;

	private ObClickable mClickable;

	protected ObProximityAnimate mProximityAnimate;

	private bool mInterruptable;

	private SnChannel mChannel;

	private List<Task> mDeliveryTaskList;

	private Task mCurrentDeliveryTask;

	private Task mCurrentMeetTask;

	private Task mPreviousMeetTask;

	private bool mTaskOfferSetupDone;

	private bool mTaskCompletionSetupDone;

	private bool mMissionAvailableDone;

	private bool mMissionEventAdded;

	private bool mWaitingForMissionEngagement;

	private bool mShowMissionBoard;

	private Task mTask;

	private GameObject mHandObject;

	private Color mCachedNameTagColor;

	private bool mNameTagOn = true;

	protected bool mBirthdayVOPlayed;

	protected bool mHolidayVOPlayed;

	protected List<Task> mTasksForCompletion = new List<Task>();

	private GameObject mMeetObject;

	private bool mCachedQuestIconActiveState;

	private bool mCachedRewardIconActiveState;

	public AIActor_NPC pAIActor { get; set; }

	public override string GetIdleAnimationName()
	{
		return _IdleAnimName;
	}

	public Vector3 GetEngagementPos()
	{
		return base.transform.position + base.transform.TransformDirection(_EngageOffset);
	}

	public override void PlayIdleAnimation()
	{
		if (mState == Character_State.idle)
		{
			base.PlayIdleAnimation();
		}
	}

	public override void DoFidget()
	{
		if ((!(pAIActor != null) || pAIActor.pState == NPC_FSM.NORMAL) && _FidgetAnimName.Length != 0 && mState == Character_State.idle)
		{
			int num = UnityEngine.Random.Range(0, _FidgetAnimName.Length - 1);
			PlayAnim(_FidgetAnimName[num]);
		}
	}

	public override bool IsAnimIdle(string aname, out bool lookatcam)
	{
		lookatcam = false;
		if (aname == _IdleAnimName)
		{
			return true;
		}
		return false;
	}

	public bool ActiveInTask(Task inTask)
	{
		if (!(inTask.pData.Type == "Escort") && !(inTask.pData.Type == "Chase"))
		{
			return inTask.pData.Type == "Follow";
		}
		return true;
	}

	protected virtual void SetupForTask(Task inTask)
	{
		if (inTask != null && inTask.pData != null)
		{
			mTask = inTask;
			RemoveFromHand(mHandObject);
		}
	}

	public virtual void OnActivate()
	{
		Input.ResetInputAxes();
		KAUICursorManager.SetDefaultCursor();
		if ((mTask == null || !ActiveInTask(mTask)) && !HandleMissionBoard())
		{
			StartEngagement(clipGiven: false);
		}
	}

	public override void SetState(Character_State newstate)
	{
		base.SetState(newstate);
		if (mProximityAnimate != null)
		{
			mProximityAnimate.enabled = newstate == Character_State.idle && !mWaitingForMissionEngagement;
		}
	}

	public void StartEngagement(bool clipGiven)
	{
		_Engaged = base.gameObject;
		HideIcons();
		SnChannel.StopPool("VO_Pool");
		if (!clipGiven && mChannel == null)
		{
			PlayClip();
		}
		if (!(_Engaged == null))
		{
			DoAction(base.transform, Character_Action.userAction1);
			PlayIdleAnimation();
			AvAvatar.SetOnlyAvatarActive(active: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			CaAvatarCam component = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
			component.SetLayer(CaAvatarCam.CameraLayer.LAYER_ENGAGEMENT, 1f);
			component.SetLookAt(base.transform, null, 0f);
			component.SetPosition(_EngageOffset, _EngageFocusHeight, 1f);
			if (_Engaged.name.Contains("Headmaster") && !ProductData.pPairData.GetBoolValue(AnalyticEvent.MEET_HEADMASTER.ToString(), defaultVal: false))
			{
				AnalyticAgent.LogEvent("AppsFlyer", AnalyticEvent.MEET_HEADMASTER, new Dictionary<string, string>());
				ProductData.pPairData.SetValueAndSave(AnalyticEvent.MEET_HEADMASTER.ToString(), true.ToString());
			}
		}
	}

	protected virtual bool HandleMissionBoard(bool inRefreshTasks = true)
	{
		mCurrentMeetTask = null;
		mCurrentDeliveryTask = null;
		if (MissionManager.pInstance == null || _Engaged != null)
		{
			return false;
		}
		if (mMissionListMenu != null)
		{
			UtDebug.LogError("Mission Board DB already loaded!");
			return true;
		}
		if (inRefreshTasks)
		{
			mShowMissionBoard = true;
			RefreshTasksForCompletion();
		}
		if (mTasksForCompletion.Count > 0)
		{
			StartEngagement(clipGiven: true);
		}
		if (ExecuteNextTask())
		{
			return true;
		}
		int num = GetOfferedTasks()?.Count ?? 0;
		if (_Engaged == null && num > 0)
		{
			StartEngagement(clipGiven: true);
		}
		if (mShowMissionBoard && ShowMissionBoard())
		{
			mShowMissionBoard = false;
			return true;
		}
		OnMissionBoardClosed();
		if (_Engaged != null)
		{
			EndEngagement();
		}
		return false;
	}

	public bool OnMeetTaskSelected(Task inTask)
	{
		if (MissionManager.pInstance != null && !ProcessSpecializedMeetTask(inTask))
		{
			OnMissionBoardClosed();
			if (inTask != null && inTask.CheckForCompletion("Meet", _Name, "", ""))
			{
				mShowMissionBoard = false;
				mPreviousMeetTask = inTask;
			}
		}
		return false;
	}

	private bool ProcessSpecializedMeetTask(Task inTask)
	{
		Dictionary<string, RsResourceEventHandler> dictionary = new Dictionary<string, RsResourceEventHandler>();
		dictionary.Add("Hypothesis", MeetTaskPopupLoadEvent);
		dictionary.Add("PersonalityTest", MeetTaskPopupLoadEvent);
		dictionary.Add("Quiz", MeetTaskPopupLoadEvent);
		dictionary.Add("Cryptex", MeetTaskPopupLoadEvent);
		dictionary.Add("DragonSelect", MeetTaskPopupLoadEvent);
		bool result = false;
		if (inTask.pData != null && inTask.pData.Type == "Meet" && inTask.pData.Objectives != null)
		{
			mCurrentMeetTask = inTask;
			foreach (TaskObjective objective in inTask.pData.Objectives)
			{
				string text = objective.Get<string>("Type");
				RsResourceEventHandler value = null;
				if (string.IsNullOrEmpty(text) || !dictionary.ContainsKey(text) || !dictionary.TryGetValue(text, out value))
				{
					continue;
				}
				string text2 = objective.Get<string>("Asset");
				if (!string.IsNullOrEmpty(text2))
				{
					KAUICursorManager.SetDefaultCursor("Loading");
					string[] array = text2.Split('/');
					if (array.Length == 1)
					{
						GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(array[0]);
						if (gameObject != null)
						{
							value(array[0], RsResourceLoadEvent.COMPLETE, 1f, gameObject, null);
						}
						else
						{
							value(array[0], RsResourceLoadEvent.ERROR, 0f, null, null);
						}
					}
					else
					{
						RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], value.Invoke, typeof(GameObject));
					}
					result = true;
				}
				else
				{
					UtDebug.Log("Asset Name not found for Hypothesis task!");
				}
			}
		}
		return result;
	}

	private void MeetTaskPopupLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (inObject != null)
			{
				SetupMeetTaskPopup(inObject);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			mCurrentMeetTask = null;
			mPreviousMeetTask = null;
			OnMissionBoardClosed();
			UtDebug.LogError("Error loading Hypothesis DB! " + inURL);
			break;
		}
	}

	private string GetMeetTaskType(Task inTask)
	{
		if (inTask != null && inTask.pData != null && inTask.pData.Type == "Meet" && inTask.pData.Objectives != null)
		{
			foreach (TaskObjective objective in inTask.pData.Objectives)
			{
				string text = objective.Get<string>("Type");
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
			}
		}
		return null;
	}

	private void SetupMeetTaskPopup(object inPopupObject)
	{
		string meetTaskType = GetMeetTaskType(mCurrentMeetTask);
		if (!string.IsNullOrEmpty(meetTaskType))
		{
			mMeetObject = UnityEngine.Object.Instantiate((GameObject)inPopupObject);
			mMeetObject.name = ((GameObject)inPopupObject).name;
			switch (meetTaskType)
			{
			case "PersonalityTest":
			{
				UiDragonQuestionnaire component2 = mMeetObject.GetComponent<UiDragonQuestionnaire>();
				if (component2 != null)
				{
					component2._MessageObject = base.gameObject;
					component2._CloseMessage = "OnMeetTaskPopupClosed";
					component2.SetupScreen(mCurrentMeetTask, _Name);
					component2.gameObject.SetActive(value: true);
				}
				break;
			}
			case "Hypothesis":
			{
				UiMissionHypothesisDB component4 = mMeetObject.GetComponent<UiMissionHypothesisDB>();
				if (component4 != null)
				{
					component4._MessageObject = base.gameObject;
					component4._CloseMessage = "OnMeetTaskPopupClosed";
					component4.SetupScreen(mCurrentMeetTask, _Name);
				}
				break;
			}
			case "Quiz":
			{
				UiQuizPopupDB component5 = mMeetObject.GetComponent<UiQuizPopupDB>();
				if (component5 != null)
				{
					component5._MessageObject = base.gameObject;
					component5._CloseMessage = "OnMeetTaskPopupClosed";
					component5.SetupScreen(mCurrentMeetTask, _Name);
				}
				break;
			}
			case "Cryptex":
			{
				UiCryptex component3 = mMeetObject.GetComponent<UiCryptex>();
				if (component3 != null)
				{
					component3._MessageObject = base.gameObject;
					component3._CloseMessage = "OnMeetTaskPopupClosed";
					component3.SetupScreen(mCurrentMeetTask, _Name);
				}
				break;
			}
			case "DragonSelect":
			{
				UiDragonSelect component = mMeetObject.GetComponent<UiDragonSelect>();
				if (component != null)
				{
					component._MessageObject = base.gameObject;
					component._CloseMessage = "OnMeetTaskPopupClosed";
					component.SetupScreen(mCurrentMeetTask, _Name);
				}
				break;
			}
			}
			DestroyMissionBoard();
		}
		else
		{
			mCurrentMeetTask = null;
			OnMissionBoardClosed();
			UtDebug.LogError("Invalid specialized meet task ");
		}
	}

	private void OnMeetTaskPopupClosed()
	{
		mPreviousMeetTask = mCurrentMeetTask;
		mCurrentMeetTask = null;
		OnMissionBoardClosed();
	}

	public bool OnDeliveryTaskSelected(Task inTask)
	{
		if (inTask != null)
		{
			mCurrentDeliveryTask = inTask;
			if (inTask.pData != null && inTask.pData.Title != null)
			{
				inTask.pData.Title.GetLocalizedString();
			}
			string text = "";
			bool flag = true;
			List<int> list = new List<int>();
			List<TaskObjective> list2 = new List<TaskObjective>();
			if (inTask.pData != null && inTask.pData.Objectives != null)
			{
				foreach (TaskObjective objective in inTask.pData.Objectives)
				{
					string value = objective.Get<string>("NPC");
					if (!string.IsNullOrEmpty(value) && _Name.Equals(value) && objective.Get<int>("ItemID") > 0)
					{
						list2.Add(objective);
					}
				}
			}
			for (int i = 0; i < list2.Count; i++)
			{
				int num = list2[i].Get<int>("ItemID");
				UserItemData userItemData = CommonInventoryData.pInstance.FindItem(num);
				int num2 = list2[i].Get<int>("Quantity");
				if (i > 0)
				{
					if (list2.Count > 2)
					{
						text += ",";
					}
					text += " ";
					if (i == list2.Count - 1)
					{
						text = text + _ItemAndText.GetLocalizedString() + " ";
					}
				}
				text = text + num2 + " ";
				if (userItemData != null)
				{
					if (userItemData.Quantity < num2)
					{
						flag = false;
					}
					text += ((num2 > 1 && !string.IsNullOrEmpty(userItemData.Item.ItemNamePlural)) ? userItemData.Item.ItemNamePlural : userItemData.Item.ItemName);
				}
				else
				{
					flag = false;
					text = text + ((num2 > 1) ? "%ip" : "%i") + num;
					list.Add(num);
				}
			}
			if (flag)
			{
				string localizedString = MissionManager.pInstance._DeliveryConfirmationText.GetLocalizedString();
				localizedString = localizedString.Replace("%items_list%", text);
				ShowDeliveryConfirmationDB(localizedString, MissionManagerDO.GetQuestHeading(inTask));
			}
			else if (list.Count > 0)
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				StringBuilder inUserData = new StringBuilder(text);
				foreach (int item in list)
				{
					ItemData.Load(item, OnLoadItemDataReady, inUserData);
				}
			}
			else
			{
				string localizedString2 = MissionManager.pInstance._DeliveryInsufficientText.GetLocalizedString();
				localizedString2 = localizedString2.Replace("%items_list%", text);
				ShowDeliveryInsufficientDB(localizedString2, MissionManagerDO.GetQuestHeading(inTask));
			}
		}
		return false;
	}

	private void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		string text = "";
		if (inUserData is StringBuilder stringBuilder && dataItem != null)
		{
			stringBuilder.Replace("%ip" + itemID, (!string.IsNullOrEmpty(dataItem.ItemNamePlural)) ? dataItem.ItemNamePlural : dataItem.ItemName);
			stringBuilder.Replace("%i" + itemID, dataItem.ItemName);
			text = stringBuilder.ToString();
		}
		if (!text.Contains("%i"))
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			string localizedString = MissionManager.pInstance._DeliveryInsufficientText.GetLocalizedString();
			localizedString = localizedString.Replace("%items_list%", text);
			ShowDeliveryInsufficientDB(localizedString, MissionManagerDO.GetQuestHeading(mCurrentDeliveryTask));
		}
	}

	private void ShowDeliveryInsufficientDB(string inMessage, string inHeader)
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "PfKAUIGenericDB");
		kAUIGenericDB.SetText(inMessage, interactive: false);
		kAUIGenericDB.SetTitle(inHeader);
		kAUIGenericDB._MessageObject = base.gameObject;
		kAUIGenericDB._OKMessage = "DestroyMessageDB";
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public void DestroyMessageDB()
	{
		EndEngagement();
		HandleMissionBoard(inRefreshTasks: false);
	}

	private void ShowDeliveryConfirmationDB(string inMessage, string inHeader)
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "PfKAUIGenericDB");
		kAUIGenericDB.SetText(inMessage, interactive: false);
		kAUIGenericDB.SetTitle(inHeader);
		kAUIGenericDB._MessageObject = base.gameObject;
		kAUIGenericDB._YesMessage = "DeliveryConfirmationYES";
		kAUIGenericDB._NoMessage = "DeliveryConfirmationNO";
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public void DeliveryConfirmationYES()
	{
		OnMissionBoardClosed();
		if (mCurrentDeliveryTask != null && mCurrentDeliveryTask.CheckForCompletion("Delivery", _Name, "", ""))
		{
			mShowMissionBoard = false;
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForCompletion();
			}
			if (_DlgThankYou != null && _DlgThankYou.Length != 0)
			{
				PlayVO(_DlgThankYou[UnityEngine.Random.Range(0, _DlgThankYou.Length)], interruptable: true);
			}
		}
	}

	public void DeliveryConfirmationNO()
	{
		EndEngagement();
		HandleMissionBoard(inRefreshTasks: false);
	}

	public void DestroyMissionBoard()
	{
		if (mMissionListMenu != null)
		{
			mMissionListMenu.RemoveCloseButtonHandler(OnMissionBoardClosed);
			mMissionListMenu.SetVisibility(inVisibility: false);
			UiNPCMissionBoard.DestroyNPCMissionBoard(mMissionListMenu);
			mMissionListMenu = null;
		}
	}

	protected virtual void OnMissionBoardClosed()
	{
		TurnOnNameTagImmediate();
		DestroyMissionBoard();
		if (mChannel != null)
		{
			mChannel.Stop();
		}
		if (!mWaitingForMissionEngagement)
		{
			EndEngagement();
		}
	}

	public void PlayClip()
	{
		List<AudioClip> list = new List<AudioClip>();
		AudioClip audioClip = null;
		if (MissionManager.pInstance != null)
		{
			List<MissionAction> offers = MissionManager.pInstance.GetOffers(MissionActionType.VO, _Name, unplayed: false);
			if (offers.Count > 0)
			{
				MissionAction missionAction = offers[UnityEngine.Random.Range(0, offers.Count)];
				LoadAndPlayVO(missionAction.Asset, interruptable: true);
				return;
			}
			audioClip = MissionManager.pInstance.GetHelp(_Name);
			if (audioClip != null)
			{
				list.Add(audioClip);
			}
		}
		if (audioClip == null)
		{
			if (!mBirthdayVOPlayed && _DlgBirthday != null && _DlgBirthday.Length != 0 && UserInfo.IsBirthdayWeek())
			{
				list.Add(_DlgBirthday[UnityEngine.Random.Range(0, _DlgBirthday.Length)]);
				mBirthdayVOPlayed = true;
			}
			else
			{
				audioClip = (mHolidayVOPlayed ? null : GetHolidayVO());
				if (audioClip != null)
				{
					list.Add(audioClip);
					mHolidayVOPlayed = true;
				}
				else if (_DlgGreeting != null && _DlgGreeting.Length != 0)
				{
					list.Add(_DlgGreeting[UnityEngine.Random.Range(0, _DlgGreeting.Length)]);
				}
			}
			if (_DlgGeneric != null && _DlgGeneric.Length != 0)
			{
				list.Add(_DlgGeneric[UnityEngine.Random.Range(0, _DlgGeneric.Length)]);
			}
			if (_DlgHelp != null && _DlgHelp.Length != 0)
			{
				list.Add(_DlgHelp[UnityEngine.Random.Range(0, _DlgHelp.Length)]);
			}
		}
		if (list.Count == 0)
		{
			EndEngagement();
		}
		else
		{
			PlayVO(list.ToArray(), interruptable: true);
		}
	}

	private AudioClip GetHolidayVO()
	{
		AudioClip result = null;
		DateTime dateTime = DateTime.Today;
		if (ServerTime.pIsReady)
		{
			dateTime = ServerTime.pCurrentTime.ToLocalTime();
		}
		SeasonalDlg[] dlgHoliday = _DlgHoliday;
		foreach (SeasonalDlg seasonalDlg in dlgHoliday)
		{
			if (DateTime.TryParse(seasonalDlg._StartDate, UtUtilities.GetCultureInfo("en-US"), DateTimeStyles.None, out var result2) && DateTime.TryParse(seasonalDlg._EndDate, UtUtilities.GetCultureInfo("en-US"), DateTimeStyles.None, out var result3) && dateTime >= result2 && dateTime <= result3)
			{
				return seasonalDlg._Clip[UnityEngine.Random.Range(0, seasonalDlg._Clip.Length)];
			}
		}
		return result;
	}

	public virtual void EndEngagement()
	{
		ShowIcons();
		splineMove component = GetComponent<splineMove>();
		if (component == null || component.tween == null || !component.tween.IsPlaying())
		{
			SetState(Character_State.idle);
		}
		if (!mWaitingForMissionEngagement)
		{
			_Engaged = null;
			mChannel = null;
			AvAvatar.SetOnlyAvatarActive(active: true);
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
			CaAvatarCam component2 = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
			component2.SetLayer(CaAvatarCam.CameraLayer.LAYER_AVATAR, 1f);
			component2.SetLookAt(AvAvatar.mTransform, null, 0f);
		}
	}

	public override void Start()
	{
		base.Start();
		CoAnimController.OnCutSceneStart = (Action)Delegate.Combine(CoAnimController.OnCutSceneStart, new Action(HideIcons));
		CoAnimController.OnCutSceneDone = (Action)Delegate.Combine(CoAnimController.OnCutSceneDone, new Action(ShowIcons));
		CoAnimController.OnCutSceneStart = (Action)Delegate.Combine(CoAnimController.OnCutSceneStart, new Action(TurnOffNameTagImmediate));
		CoAnimController.OnCutSceneDone = (Action)Delegate.Combine(CoAnimController.OnCutSceneDone, new Action(TurnOnNameTagImmediate));
		if (_NameTag != null)
		{
			UiOptions.OnSetNPCNameTagVisbility = (Action<bool>)Delegate.Combine(UiOptions.OnSetNPCNameTagVisbility, new Action<bool>(FadeNameTag));
			mCachedNameTagColor = _NameTag.GetLabel().color;
			FadeNameTag(inShowNameTag: false);
			mNameTagOn = false;
		}
		mClickable = base.gameObject.GetComponent<ObClickable>();
		if (mClickable != null)
		{
			mClickable._MessageObject = AvAvatar.pObject;
		}
		if (_QuestIcon != null)
		{
			AddComponentsToIcon(_QuestIcon);
		}
		if (_RewardIcon != null)
		{
			AddComponentsToIcon(_RewardIcon);
		}
		mProximityAnimate = GetComponent<ObProximityAnimate>();
		if (_Engaged == null)
		{
			Initialize(null);
		}
	}

	private void ShowIcons()
	{
		if (_QuestIcon != null)
		{
			_QuestIcon.SetActive(mCachedQuestIconActiveState);
		}
		if (_RewardIcon != null)
		{
			_RewardIcon.SetActive(mCachedRewardIconActiveState);
		}
	}

	private void HideIcons()
	{
		if (_QuestIcon != null)
		{
			mCachedQuestIconActiveState = _QuestIcon.activeSelf;
			_QuestIcon.SetActive(value: false);
		}
		if (_RewardIcon != null)
		{
			mCachedRewardIconActiveState = _RewardIcon.activeSelf;
			_RewardIcon.SetActive(value: false);
		}
	}

	private void FadeNameTag(bool inShowNameTag)
	{
		if ((bool)_NameTag && mNameTagOn)
		{
			_NameTag.StopColorBlendTo();
			_NameTag.ColorBlendTo(_NameTag.GetLabel().color, inShowNameTag ? mCachedNameTagColor : Color.clear, _NameTagFadeTime);
		}
	}

	public void TurnOnNameTag()
	{
		mNameTagOn = true;
		if (AvatarData.pDisplayNPCName)
		{
			FadeNameTag(inShowNameTag: true);
		}
	}

	public void TurnOnNameTagImmediate()
	{
		_NameTag?.SetVisibility(inVisible: true);
	}

	public void TurnOffNameTag()
	{
		FadeNameTag(inShowNameTag: false);
		mNameTagOn = false;
	}

	public void TurnOffNameTagImmediate()
	{
		_NameTag?.SetVisibility(inVisible: false);
	}

	public void OnDisable()
	{
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		mMissionEventAdded = false;
	}

	private void OnDestroy()
	{
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		CoAnimController.OnCutSceneStart = (Action)Delegate.Remove(CoAnimController.OnCutSceneStart, new Action(HideIcons));
		CoAnimController.OnCutSceneDone = (Action)Delegate.Remove(CoAnimController.OnCutSceneDone, new Action(ShowIcons));
		CoAnimController.OnCutSceneStart = (Action)Delegate.Remove(CoAnimController.OnCutSceneStart, new Action(TurnOffNameTagImmediate));
		CoAnimController.OnCutSceneDone = (Action)Delegate.Remove(CoAnimController.OnCutSceneDone, new Action(TurnOnNameTagImmediate));
		UiOptions.OnSetNPCNameTagVisbility = (Action<bool>)Delegate.Remove(UiOptions.OnSetNPCNameTagVisbility, new Action<bool>(FadeNameTag));
	}

	public override void Update()
	{
		base.Update();
		if (!mMissionEventAdded)
		{
			MissionManager.AddMissionEventHandler(OnMissionEvent);
			mMissionEventAdded = true;
		}
		if (_Engaged != null && mInterruptable && mChannel != null)
		{
			if ((KAInput.pInstance.IsTouchInput() && KAInput.GetMouseButtonUp(0)) || Input.GetKeyUp(KeyCode.Space))
			{
				mChannel.Stop();
				KAUICursorManager.SetDefaultCursor("Arrow");
			}
		}
		else
		{
			if (!MissionManager.pIsReady)
			{
				return;
			}
			if (pAIActor == null || pAIActor.pState == NPC_FSM.NORMAL)
			{
				if (mClickable != null && !mClickable._Active)
				{
					mClickable._Active = true;
				}
				if (!mTaskCompletionSetupDone)
				{
					CheckTasksAndActivateRewardsIcon();
					mTaskCompletionSetupDone = true;
				}
				if (!mTaskOfferSetupDone && (_RewardIcon == null || !_RewardIcon.activeSelf))
				{
					CheckTasksAndActivateQuestIcon();
					mTaskOfferSetupDone = true;
				}
				if (!mMissionAvailableDone)
				{
					CheckMissionAvailable();
					mMissionAvailableDone = true;
				}
			}
			else if (pAIActor != null && pAIActor.pState != 0)
			{
				if (mClickable != null && mClickable._Active)
				{
					mClickable._Active = false;
				}
				if (_QuestIcon != null && _QuestIcon.activeSelf)
				{
					_QuestIcon.SetActive(value: false);
				}
				if (_RewardIcon != null && _RewardIcon.activeSelf)
				{
					_RewardIcon.SetActive(value: false);
				}
				TurnOffNameTagImmediate();
			}
		}
	}

	public void LoadAndPlayVO(string asset, bool interruptable)
	{
		if (_Engaged == null)
		{
			StartEngagement(clipGiven: true);
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		mInterruptable = interruptable;
		string[] array = asset.Split('/');
		if (SnChannel.pTurnOffSoundGroup)
		{
			VOLoadEvent(array[0] + "/" + array[1], RsResourceLoadEvent.COMPLETE, 1f, null, null);
		}
		else
		{
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], VOLoadEvent, typeof(AudioClip));
		}
	}

	public void PlayVO(AudioClip clip, bool interruptable)
	{
		PlayVO(new AudioClip[1] { clip }, interruptable);
	}

	public void PlayVO(AudioClip[] clips, bool interruptable)
	{
		if (_Engaged == null)
		{
			StartEngagement(clipGiven: true);
		}
		mInterruptable = interruptable;
		if (SnChannel.pTurnOffSoundGroup)
		{
			SnEvent inEvent = default(SnEvent);
			inEvent.mType = SnEventType.STOP;
			if (clips != null && clips.Length != 0)
			{
				inEvent.mClip = clips[0];
			}
			OnSnEvent(inEvent);
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			mChannel = SnChannel.Play(clips, "VO_Pool", 0, inForce: true, base.gameObject);
		}
	}

	private void OnSnEvent(SnEvent inEvent)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (inEvent.mType == SnEventType.PLAY)
		{
			PlayAnim(_MouthAnimName, -1, 1f, 1);
		}
		else if (inEvent.mType == SnEventType.END_QUEUE || inEvent.mType == SnEventType.STOP)
		{
			EndEngagement();
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.OnSnEvent(inEvent);
				MissionManager.pInstance.Update();
			}
		}
	}

	private void VOLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (inObject != null)
			{
				PlayVO((AudioClip)inObject, mInterruptable);
				break;
			}
			SnEvent inEvent2 = default(SnEvent);
			inEvent2.mType = SnEventType.STOP;
			inEvent2.mClip = null;
			OnSnEvent(inEvent2);
			break;
		}
		case RsResourceLoadEvent.ERROR:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			SnEvent inEvent = default(SnEvent);
			inEvent.mType = SnEventType.STOP;
			OnSnEvent(inEvent);
			break;
		}
		}
	}

	public void StartMissionEngagement()
	{
		mWaitingForMissionEngagement = true;
		if (_Engaged != base.gameObject)
		{
			StartEngagement(clipGiven: true);
		}
	}

	public void EndMissionEngagement()
	{
		mWaitingForMissionEngagement = false;
		EndEngagement();
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (mTask != null && (inEvent == MissionEvent.TASK_COMPLETE || inEvent == MissionEvent.TASK_FAIL) && mTask == (Task)inObject)
		{
			mTask = null;
		}
		switch (inEvent)
		{
		case MissionEvent.OFFER_COMPLETE:
		case MissionEvent.TASK_COMPLETE:
		case MissionEvent.MISSION_COMPLETE:
			mTaskOfferSetupDone = false;
			mTaskCompletionSetupDone = false;
			mMissionAvailableDone = false;
			TurnOnNameTagImmediate();
			break;
		case MissionEvent.REFRESH_MISSIONS:
			mTaskOfferSetupDone = false;
			mMissionAvailableDone = false;
			break;
		case MissionEvent.TASK_STARTED:
			if (inObject is Task task && mPreviousMeetTask != null)
			{
				if (IsConsecutiveMeetTask(task))
				{
					AddTaskForCompletion(task);
				}
				mPreviousMeetTask = null;
			}
			break;
		}
		if (!mWaitingForMissionEngagement)
		{
			return;
		}
		switch (inEvent)
		{
		case MissionEvent.OFFER:
		case MissionEvent.TASK_END:
		case MissionEvent.MISSION_END:
			if (!(((MissionAction)inObject).NPC != base.name))
			{
				break;
			}
			goto case MissionEvent.OFFER_COMPLETE;
		case MissionEvent.OFFER_COMPLETE:
		case MissionEvent.TASK_END_COMPLETE:
		case MissionEvent.MISSION_END_COMPLETE:
			EndMissionEngagement();
			if (!MissionManager.MissionActionPending())
			{
				HandleMissionBoard(inRefreshTasks: false);
			}
			break;
		}
	}

	protected virtual void CheckTasksAndActivateQuestIcon()
	{
		if (_QuestIcon == null)
		{
			return;
		}
		if (MissionManager.pInstance != null && _QuestIcon != null)
		{
			List<Task> tasks = MissionManager.pInstance.GetTasks(_MissionGroupID);
			if (tasks != null && tasks.Count > 0)
			{
				List<Task> list = tasks.FindAll((Task t) => !t.pPayload.Started && t._Mission.pMustAccept);
				if (list != null && list.Count > 0)
				{
					_QuestIcon.SetActive(value: true);
					return;
				}
			}
		}
		_QuestIcon.SetActive(value: false);
	}

	private void CheckTasksAndActivateRewardsIcon()
	{
		if (!(_RewardIcon == null))
		{
			_RewardIcon.SetActive(MissionManagerDO.IsNPCQuestRewardAvailable(_Name));
			if (_RewardIcon.activeSelf && _QuestIcon != null)
			{
				mTaskOfferSetupDone = false;
				_QuestIcon.SetActive(value: false);
			}
		}
	}

	private void CheckMissionAvailable()
	{
		if (_MissionAvailableActivate == null || _MissionAvailableActivate.Count == 0)
		{
			return;
		}
		foreach (MissionAvailableActivate item in _MissionAvailableActivate)
		{
			if (!(item._Object != null))
			{
				continue;
			}
			foreach (int item2 in item._MissionID)
			{
				Mission mission = MissionManager.pInstance.GetMission(item2);
				bool flag = mission != null && !mission.pCompleted && !MissionManager.pInstance.IsLocked(mission) && mission.pMustAccept && !mission.Accepted;
				item._Object.SetActive(flag);
				if (flag)
				{
					break;
				}
			}
		}
	}

	private void AddComponentsToIcon(GameObject inIcon)
	{
		if (inIcon.GetComponent<BoxCollider>() == null)
		{
			inIcon.AddComponent<BoxCollider>().isTrigger = true;
			inIcon.layer = LayerMask.NameToLayer("IgnoreGroundRay");
		}
		ObClickableNPCIcon component = inIcon.GetComponent<ObClickableNPCIcon>();
		if (component == null)
		{
			component = inIcon.AddComponent<ObClickableNPCIcon>();
			component._MessageObject = base.gameObject;
			component._AvatarWalkTo = false;
			if (mClickable != null)
			{
				component._Range = mClickable._Range;
				component._RangeAngle = mClickable._RangeAngle;
				component._RangeOffset = mClickable._RangeOffset;
				component._RollOverCursorName = mClickable._RollOverCursorName;
			}
			component._RangeOffset.y -= inIcon.transform.localPosition.y;
		}
	}

	protected virtual List<Task> GetOfferedTasks()
	{
		List<Task> tasks = MissionManager.pInstance.GetTasks(_MissionGroupID);
		List<Task> list = null;
		if (tasks != null && tasks.Count > 0)
		{
			list = tasks.FindAll((Task t) => !t.pPayload.Started);
		}
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				int parentMissionId = list[i]._Mission.MissionID;
				int taskId = list[i].TaskID;
				if (!list[i]._Mission.MissionRule.Criteria.Ordered)
				{
					list.RemoveAll((Task t) => t._Mission.MissionID == parentMissionId && t.TaskID != taskId);
				}
			}
		}
		return list;
	}

	protected bool IsConsecutiveMeetTask(Task inNewTask)
	{
		if (inNewTask != null && mPreviousMeetTask != null)
		{
			Mission rootMission = MissionManager.pInstance.GetRootMission(inNewTask);
			Mission rootMission2 = MissionManager.pInstance.GetRootMission(mPreviousMeetTask);
			if (rootMission != null && rootMission2 != null && rootMission.MissionID == rootMission2.MissionID && inNewTask.pData != null && inNewTask.pData.Type == "Meet" && inNewTask.pData.Objectives != null)
			{
				foreach (TaskObjective objective in inNewTask.pData.Objectives)
				{
					string text = objective.Get<string>("NPC");
					if (!string.IsNullOrEmpty(text) && text == _Name)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	protected string GetTaskType(Task inTask)
	{
		if (inTask != null && inTask.pData != null)
		{
			return inTask.pData.Type;
		}
		return "";
	}

	private int GetTaskPriority(string inTaskType)
	{
		if (!(inTaskType == "Meet"))
		{
			if (!(inTaskType == "Delivery"))
			{
				return -1;
			}
			return 1;
		}
		return 2;
	}

	protected void RefreshTasksForCompletion()
	{
		mTasksForCompletion.Clear();
		mDeliveryTaskList = MissionManager.pInstance.GetTasks("Delivery", "NPC", _Name);
		List<Task> tasks = MissionManager.pInstance.GetTasks("Meet", "NPC", _Name);
		if (mDeliveryTaskList != null)
		{
			foreach (Task mDeliveryTask in mDeliveryTaskList)
			{
				AddTaskForCompletion(mDeliveryTask);
			}
		}
		if (tasks == null)
		{
			return;
		}
		foreach (Task item in tasks)
		{
			AddTaskForCompletion(item);
		}
	}

	protected void AddTaskForCompletion(Task inTask)
	{
		if (inTask == null)
		{
			return;
		}
		if (mTasksForCompletion.Count <= 0)
		{
			mTasksForCompletion.Add(inTask);
		}
		else
		{
			if (mTasksForCompletion.Find((Task a) => a.TaskID == inTask.TaskID) != null)
			{
				return;
			}
			if (!IsConsecutiveMeetTask(inTask))
			{
				string taskType = GetTaskType(inTask);
				int taskPriority = GetTaskPriority(taskType);
				if (taskPriority < 0)
				{
					UtDebug.LogError("Invalid NPCAvatar Task priority");
					return;
				}
				for (int num = mTasksForCompletion.Count - 1; num >= 0; num--)
				{
					string taskType2 = GetTaskType(mTasksForCompletion[num]);
					int taskPriority2 = GetTaskPriority(taskType2);
					if (taskPriority >= taskPriority2)
					{
						mTasksForCompletion.Insert(num, inTask);
						break;
					}
				}
			}
			else
			{
				mTasksForCompletion.Insert(0, inTask);
			}
		}
	}

	private bool ExecuteNextTask()
	{
		if (mTasksForCompletion.Count > 0)
		{
			Task task = mTasksForCompletion[0];
			mTasksForCompletion.RemoveAt(0);
			string taskType = GetTaskType(task);
			if (taskType == "Meet")
			{
				OnMeetTaskSelected(task);
				return true;
			}
			if (taskType == "Delivery")
			{
				OnDeliveryTaskSelected(task);
				return true;
			}
			UtDebug.LogError("Invalid NPCAvatar TaskType " + taskType + " for task " + task.TaskID);
		}
		return false;
	}

	protected virtual bool ShowMissionBoard()
	{
		List<Task> offeredTasks = GetOfferedTasks();
		int num = offeredTasks?.Count ?? 0;
		TurnOffNameTagImmediate();
		if (num > 0)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			MissionManager.pInstance.LoadMissionData(_MissionGroupID, OnMissionStaticLoad);
			return true;
		}
		return LoadMissionBoard(offeredTasks);
	}

	public string GetMissionBoardAsset()
	{
		string result = _MissionBoardAsset;
		if (UtPlatform.IsMobile())
		{
			result = _MissionBoardAssetMobile;
		}
		return result;
	}

	private bool LoadMissionBoard(List<Task> tasksToOffer)
	{
		int num = tasksToOffer?.Count ?? 0;
		if (num > 0)
		{
			mMissionListMenu = UiNPCMissionBoard.CreateNPCMissionBoard(GetMissionBoardAsset());
			if (mMissionListMenu != null)
			{
				if (tasksToOffer != null && tasksToOffer.Count == 1 && num == 1)
				{
					mMissionListMenu.ShowQuestDetails(tasksToOffer[0], OnMissionBoardClosed);
				}
				else
				{
					mMissionListMenu.PopulateItems(tasksToOffer, OnMissionBoardClosed);
				}
				return true;
			}
		}
		return false;
	}

	public void OnMissionStaticLoad(List<Mission> missions)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		List<Task> offeredTasks = GetOfferedTasks();
		LoadMissionBoard(offeredTasks);
	}

	public void ShowNodeText(string text)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfChatBubble"));
		if (gameObject != null)
		{
			Transform transform = UtUtilities.FindChildTransform(base.gameObject, "Head_J");
			gameObject.transform.parent = transform.transform;
			gameObject.transform.position = Vector3.zero;
			ChatBubble component = gameObject.GetComponent<ChatBubble>();
			if (component != null)
			{
				component._LocalOffset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0.5f, 1f), 0f);
				component.WriteChat(text);
			}
		}
	}

	public void DoFire(Transform target, bool useDirection, Vector3 direction, float speed = 30f)
	{
		if (_WeaponManager != null)
		{
			_WeaponManager.Fire(target, useDirection, direction, speed);
		}
	}

	public void AttachInHand(GameObject obj)
	{
		if (obj != null)
		{
			obj.transform.parent = _HandT;
			obj.transform.localPosition = _AttachOffset;
			mHandObject = obj;
		}
	}

	public void DetachCarryingObject()
	{
		RemoveFromHand(mHandObject);
	}

	public void RemoveFromHand(GameObject obj)
	{
		if (obj != null)
		{
			obj.transform.parent = null;
			obj.transform.localPosition = new Vector3(0f, -5000f, 0f);
			mHandObject = null;
		}
	}

	public void Fire(GameObject obj)
	{
		Transform transform = obj.transform;
		Vector3 vector = ((transform.childCount > 0) ? transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).position : transform.position);
		DoFire(null, useDirection: true, (vector - base.transform.position).normalized);
	}

	public void PlayAnimOnce(string anim)
	{
		if (base.animation != null)
		{
			SetState(Character_State.action);
			PlayAnim(anim);
			StartCoroutine(SetState(Character_State.idle, base.animation.GetClip(anim).length));
		}
	}

	public void PlayAnimLoop(string anim)
	{
		if (base.animation != null)
		{
			SetState((anim == GetIdleAnimationName()) ? Character_State.idle : Character_State.action);
			PlayAnim(anim, -1, 1f, 1);
		}
	}

	private IEnumerator SetState(Character_State state, float waitSecs)
	{
		yield return new WaitForSeconds(waitSecs);
		SetState(state);
	}
}
