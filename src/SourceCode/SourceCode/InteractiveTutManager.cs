using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractiveTutManager : MonoBehaviour
{
	public static bool _Save = true;

	public static TutorialDialogShow OnTutorialDialogShowEvent;

	public static BlinkTextEvent _BlinkEvent = null;

	public static GameObject _CurrentActiveTutorialObject = null;

	public int _PairID;

	public string _TutIndexKeyName;

	public InteractiveTutStep[] _TutSteps;

	public bool _CanRepeatOnIdle = true;

	public float _IdleRepeatInterval = 30f;

	public WaitEndEvent _WaitEnd;

	public bool _IsSavedStepToBeLoaded = true;

	public StepStartedEvent _StepStartedEvent;

	public StepEndedEvent _StepEndedEvent;

	public TutorialCompleteEvent _TutorialCompleteEvent;

	public Vector2 _TutorialBoardPos;

	public LocaleString _TutorialDBHeader = new LocaleString("Tutorial");

	public string _TutorialDBRes = "RS_DATA/PfUiTutMsgDBDM.unity3d/PfUiTutMsgDBDM";

	public string _TutorialDBResMobile = "";

	public string _RecordIdxName = "";

	public int _MaxTimesToBeShown = 1;

	public LocaleString _CloseMessage = new LocaleString("Do you want to skip tutorial?");

	protected ObStatus mStatus;

	protected int mCurrentTutIndex;

	protected int mCurrentTutDBIndex;

	protected bool mSkipTutorial;

	protected bool mIsPairDataReady;

	protected float mIdleRepeatTimer;

	protected bool mIsTutorialPaused = true;

	protected PairData mPairData;

	protected int mTimesShown;

	protected bool mCompletedTutorial;

	protected string mTutorialMsg = "";

	protected bool mIsShowingTutorial;

	protected UiInteractiveTutMsgDB mTutorialDB;

	protected GameObject mTutorialDBObj;

	protected GameObject mUiGenericDBObj;

	protected bool mDownloadError;

	protected TutorialStepHandler mTutorialStepHandler;

	protected bool mInitShowTutorial;

	public virtual void Awake()
	{
		if (UtPlatform.IsMobile() && !string.IsNullOrEmpty(_TutorialDBResMobile))
		{
			_TutorialDBRes = _TutorialDBResMobile;
		}
		FilterStepsByPlatform();
		CheckForTextTutorialLoad();
	}

	public virtual void Start()
	{
		mStatus = GetComponent<ObStatus>();
		if (_PairID > 0)
		{
			PairData.Load(_PairID, TutPairDataEventHandler, null);
		}
		else
		{
			TutPairDataEventHandler(success: false, null, null);
		}
		StartCoroutine(BlinkTexts(0.5f, flag: true));
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		_BlinkEvent = null;
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		_BlinkEvent = null;
	}

	private void TutPairDataEventHandler(bool success, PairData inData, object inUserData)
	{
		mIsPairDataReady = true;
		mPairData = inData;
		if (mPairData != null)
		{
			mTimesShown = mPairData.GetIntValue(_RecordIdxName, 0);
			if (_IsSavedStepToBeLoaded)
			{
				mCurrentTutIndex = mPairData.GetIntValue(_TutIndexKeyName, 0);
			}
		}
		mCurrentTutDBIndex = mCurrentTutIndex;
		if (mStatus != null)
		{
			mStatus.pIsReady = true;
		}
	}

	protected void CheckForTextTutorialLoad()
	{
		if (!TutorialComplete())
		{
			if (!string.IsNullOrEmpty(_TutorialDBRes))
			{
				mTutorialDBObj = (GameObject)RsResourceManager.LoadAssetFromResources(_TutorialDBRes.Split('/')[^1]);
				CanPlayTutorial();
			}
		}
		else
		{
			DeleteInstance();
		}
	}

	public virtual void Save()
	{
		if (!_Save || mPairData == null || mCurrentTutIndex <= -1)
		{
			return;
		}
		if (mCurrentTutIndex < _TutSteps.Length)
		{
			if (_TutSteps[mCurrentTutIndex]._SaveProgress)
			{
				mPairData.SetValueAndSave(_TutIndexKeyName, mCurrentTutIndex.ToString());
			}
		}
		else
		{
			mPairData.SetValueAndSave(_TutIndexKeyName, mCurrentTutIndex.ToString());
		}
	}

	public virtual bool PlayHelpVO()
	{
		if (mCurrentTutIndex < _TutSteps.Length)
		{
			PlayTutorialStepVO(_TutSteps[mCurrentTutIndex]);
			return true;
		}
		return false;
	}

	private void PlayTutorialStepVO(InteractiveTutStep tutStep)
	{
		if (tutStep == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(tutStep._VO_URL))
		{
			if (tutStep._VO != null)
			{
				SnChannel.Play(tutStep._VO, "VO_Pool", 0, inForce: true, base.gameObject);
			}
			return;
		}
		string[] array = tutStep._VO_URL.Split('/');
		if (array.Length == 3)
		{
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], SoundDownloadEventHandler, typeof(AudioClip));
		}
		else
		{
			UtDebug.LogError("Sound file path name is not complete!!!");
		}
	}

	public void SoundDownloadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				if (mCurrentTutIndex < _TutSteps.Length)
				{
					_TutSteps[mCurrentTutIndex]._VO = (AudioClip)inObject;
					SnChannel.Play((AudioClip)inObject, "VO_Pool", 0, inForce: true, base.gameObject);
				}
			}
			else
			{
				UtDebug.LogError("Sound file missing!!!");
			}
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Error downloading the tutorial sound file!!!");
			break;
		}
	}

	private void OnSnEvent(SnEvent snEvent)
	{
		if (snEvent.mType != SnEventType.END && (snEvent.mType != SnEventType.STOP || !mSkipTutorial))
		{
			return;
		}
		mSkipTutorial = false;
		if (mTutorialStepHandler.pTutStep._DisableBoardButtons && !IsInteractivePage(mCurrentTutIndex))
		{
			if (mTutorialStepHandler != null)
			{
				mTutorialStepHandler.FinishTutorialStep();
			}
			if (SnUtility.SafeClipCompare(snEvent.mClip, _TutSteps[mCurrentTutIndex]._VO) && !_TutSteps[mCurrentTutIndex]._WaitForUserEvent)
			{
				StartNextTutorial();
			}
		}
	}

	public virtual bool TutorialComplete()
	{
		return ProductData.TutorialComplete(_TutIndexKeyName);
	}

	public virtual void PauseTutorial()
	{
		mIsTutorialPaused = true;
		SnChannel.StopPool("VO_Pool");
	}

	public virtual void PlayTutorial()
	{
		mIsTutorialPaused = false;
		StartTutorialStep();
	}

	public virtual void StartNextTutorial()
	{
		if (_StepEndedEvent != null)
		{
			_StepEndedEvent(mCurrentTutIndex, _TutSteps[mCurrentTutIndex]._Name, tutQuit: false);
		}
		mCurrentTutIndex++;
		mCurrentTutDBIndex = mCurrentTutIndex;
		if (KAUI._GlobalExclusiveUI != null && mTutorialDB != null)
		{
			KAUI.RemoveExclusive(mTutorialDB);
		}
		if (mCurrentTutIndex >= _TutSteps.Length)
		{
			mCompletedTutorial = true;
			if (mUiGenericDBObj != null)
			{
				UnityEngine.Object.Destroy(mUiGenericDBObj);
			}
			if (mTutorialDB != null)
			{
				mTutorialDB.Exit();
			}
		}
		else
		{
			StartTutorialStep();
		}
	}

	public virtual void StartTutorialStep()
	{
		if (mIsTutorialPaused)
		{
			return;
		}
		mIdleRepeatTimer = _IdleRepeatInterval;
		if (mCurrentTutIndex < 0)
		{
			mCurrentTutIndex = 0;
		}
		mCurrentTutDBIndex = mCurrentTutIndex;
		SnChannel.StopPool("VO_Pool");
		if (mCurrentTutIndex < _TutSteps.Length)
		{
			SetupTutorialStep();
			if (_StepStartedEvent != null)
			{
				_StepStartedEvent(mCurrentTutIndex, _TutSteps[mCurrentTutIndex]._Name);
			}
		}
	}

	public virtual string GetFormattedStepText(LocaleString unformattedString)
	{
		return unformattedString.GetLocalizedString();
	}

	public virtual void SetupTutorialStep()
	{
		if (mTutorialStepHandler != null)
		{
			mTutorialStepHandler.FinishTutorialStep();
		}
		Save();
		InteractiveTutStep interactiveTutStep = _TutSteps[mCurrentTutIndex];
		mTutorialMsg = GetFormattedStepText(interactiveTutStep._StepText);
		if (mTutorialMsg != null)
		{
			mTutorialMsg = mTutorialMsg.Replace("{{CoinReward}}", (interactiveTutStep._StepRewards.Length != 0) ? interactiveTutStep._StepRewards[0]._Count.ToString() : "0");
		}
		PlayTutorialStepVO(interactiveTutStep);
		if (mTutorialDBObj != null)
		{
			ShowTutorialDB(mTutorialDBObj);
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		mTutorialStepHandler = TutorialStepHandler.InitTutorialStepHandler(interactiveTutStep);
		TutorialStepHandler tutorialStepHandler = mTutorialStepHandler;
		tutorialStepHandler._StepProgressCallback = (TutorialStepHandler.OnTutorialStepProgress)Delegate.Combine(tutorialStepHandler._StepProgressCallback, new TutorialStepHandler.OnTutorialStepProgress(StepProgressCallback));
		mTutorialStepHandler.SetupTutorialStep();
		SpecificTutorialActions();
	}

	protected bool CanHaveBackButton(int aCurrentTutDBIndex)
	{
		while (aCurrentTutDBIndex > 0)
		{
			aCurrentTutDBIndex--;
			if (aCurrentTutDBIndex < 0)
			{
				aCurrentTutDBIndex = 0;
			}
			if (!string.IsNullOrEmpty(_TutSteps[aCurrentTutDBIndex]._StepText.GetLocalizedString()))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void SetTutDBButtonStates(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose)
	{
		string iconName = _TutSteps[mCurrentTutDBIndex]._IconName;
		mTutorialDB.SetButtonsActive(inBtnNext, inBtnBack, inBtnYes, inBtnNo, inBtnDone, inBtnClose, iconName);
	}

	private KATransformData GetOrientationTransformData(InteractiveTutStep inTutStep, bool isPortrait)
	{
		if (inTutStep._OrientationInfo != null)
		{
			if (isPortrait)
			{
				if (inTutStep._OrientationInfo._Portrait._SmallScreenData._Apply && KAUIManager.IsSmallScreen())
				{
					return inTutStep._OrientationInfo._Portrait._SmallScreenData;
				}
				if (inTutStep._OrientationInfo._Portrait._OrientationData._Apply)
				{
					return inTutStep._OrientationInfo._Portrait._OrientationData;
				}
			}
			else
			{
				if (inTutStep._OrientationInfo._Landscape._SmallScreenData._Apply && KAUIManager.IsSmallScreen())
				{
					return inTutStep._OrientationInfo._Landscape._SmallScreenData;
				}
				if (inTutStep._OrientationInfo._Landscape._OrientationData._Apply)
				{
					return inTutStep._OrientationInfo._Landscape._OrientationData;
				}
			}
		}
		return null;
	}

	private void ShowTutorialDB(GameObject inObject)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (AvAvatar.pAvatarCam != null)
		{
			AvAvatar.pAvatarCam.SendMessage("EnableCameraControl", false, SendMessageOptions.DontRequireReceiver);
		}
		if (OnTutorialDialogShowEvent != null)
		{
			OnTutorialDialogShowEvent(isShown: true);
		}
		int delayFrameCount = 0;
		if (mTutorialDB == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(inObject);
			gameObject.name = inObject.name;
			mTutorialDB = (UiInteractiveTutMsgDB)gameObject.GetComponent(typeof(UiInteractiveTutMsgDB));
			delayFrameCount = 1;
		}
		if (string.IsNullOrEmpty(mTutorialMsg))
		{
			if (mTutorialDB != null)
			{
				mTutorialDB.Show(inShow: false);
				if (KAUI._GlobalExclusiveUI != null)
				{
					KAUI.RemoveExclusive(mTutorialDB);
				}
			}
			return;
		}
		mTutorialDB.SetText(base.gameObject, _TutorialDBHeader.GetLocalizedString(), mTutorialMsg);
		KATransformData orientationTransformData = GetOrientationTransformData(_TutSteps[mCurrentTutDBIndex], KAUIManager.IsPortrait());
		if (_TutSteps[mCurrentTutDBIndex]._BoardPosition.x != 0f || _TutSteps[mCurrentTutDBIndex]._BoardPosition.y != 0f || orientationTransformData != null)
		{
			if (orientationTransformData != null)
			{
				mTutorialDB.ApplyOrientationData(orientationTransformData, KAUIManager.IsPortrait());
			}
			else
			{
				mTutorialDB.SetPosition(_TutSteps[mCurrentTutDBIndex]._BoardPosition.x, _TutSteps[mCurrentTutDBIndex]._BoardPosition.y, delayFrameCount);
			}
		}
		else
		{
			mTutorialDB.SetPosition(_TutorialBoardPos.x, _TutorialBoardPos.y);
			mTutorialDB.OnOrientation(KAUIManager.IsPortrait());
		}
		mTutorialDB.pInitMessageBeforeExit = true;
		if (_TutSteps[mCurrentTutIndex]._DisableBoardButtons)
		{
			mTutorialDB.SetInteractiveNextBackBtns(flag: false);
			SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: false, inBtnNo: false, inBtnDone: false, _TutSteps[mCurrentTutDBIndex]._CanSkipStep);
		}
		else
		{
			bool flag = mCurrentTutDBIndex < _TutSteps.Length - 1;
			bool inBtnBack = CanHaveBackButton(mCurrentTutDBIndex);
			bool inBtnDone = !flag;
			mTutorialDB.SetInteractiveNextBackBtns(flag: true);
			SetTutDBButtonStates(flag, inBtnBack, inBtnYes: false, inBtnNo: false, inBtnDone, _TutSteps[mCurrentTutDBIndex]._CanSkipStep);
		}
		mTutorialDB.Show(inShow: true);
		if (mCurrentTutDBIndex == mCurrentTutIndex)
		{
			if (!IsInteractivePage(mCurrentTutIndex))
			{
				KAUI.SetExclusive(mTutorialDB, Color.clear);
			}
			else
			{
				mTutorialDB.SetInteractiveNextBackBtns(flag: false);
			}
		}
		else
		{
			KAUI.SetExclusive(mTutorialDB, Color.clear);
		}
	}

	public virtual void OnGenericInfoBoxExit()
	{
		if (_StepEndedEvent != null && mCurrentTutIndex < _TutSteps.Length)
		{
			_StepEndedEvent(mCurrentTutIndex, _TutSteps[mCurrentTutIndex]._Name, tutQuit: false);
		}
		mCurrentTutIndex++;
		mCurrentTutDBIndex = mCurrentTutIndex;
		Exit();
	}

	public virtual void OnGenericInfoBoxExitInit()
	{
		if (!mCompletedTutorial)
		{
			ShowDialog();
		}
		else
		{
			mTutorialDB.Exit();
		}
	}

	public void OnNextButtonClicked()
	{
		if (mCurrentTutIndex == mCurrentTutDBIndex)
		{
			StartNextTutorial();
			return;
		}
		while (mCurrentTutDBIndex < mCurrentTutIndex)
		{
			mCurrentTutDBIndex++;
			if (mCurrentTutDBIndex > mCurrentTutIndex)
			{
				mCurrentTutDBIndex = mCurrentTutIndex;
			}
			if (mTutorialDBObj != null)
			{
				mTutorialMsg = GetFormattedStepText(_TutSteps[mCurrentTutDBIndex]._StepText);
				if (mTutorialMsg != null)
				{
					mTutorialMsg = mTutorialMsg.Replace("{{CoinReward}}", (_TutSteps[mCurrentTutDBIndex]._StepRewards.Length != 0) ? _TutSteps[mCurrentTutDBIndex]._StepRewards[0]._Count.ToString() : "0");
				}
				if (!string.IsNullOrEmpty(mTutorialMsg))
				{
					ShowTutorialDB(mTutorialDBObj);
					break;
				}
			}
		}
	}

	public void OnBackButtonClicked()
	{
		while (mCurrentTutDBIndex > 0)
		{
			mCurrentTutDBIndex--;
			if (mCurrentTutDBIndex < 0)
			{
				mCurrentTutDBIndex = 0;
			}
			if (mTutorialDBObj != null)
			{
				mTutorialMsg = GetFormattedStepText(_TutSteps[mCurrentTutDBIndex]._StepText);
				if (mTutorialMsg != null)
				{
					mTutorialMsg = mTutorialMsg.Replace("{{CoinReward}}", (_TutSteps[mCurrentTutDBIndex]._StepRewards.Length != 0) ? _TutSteps[mCurrentTutDBIndex]._StepRewards[0]._Count.ToString() : "0");
				}
				if (!string.IsNullOrEmpty(mTutorialMsg))
				{
					ShowTutorialDB(mTutorialDBObj);
					break;
				}
			}
		}
	}

	public void StepProgressCallback(float completed, float max)
	{
		if (completed >= max)
		{
			if (mTutorialStepHandler != null)
			{
				TutorialStepHandler tutorialStepHandler = mTutorialStepHandler;
				tutorialStepHandler._StepProgressCallback = (TutorialStepHandler.OnTutorialStepProgress)Delegate.Remove(tutorialStepHandler._StepProgressCallback, new TutorialStepHandler.OnTutorialStepProgress(StepProgressCallback));
			}
			StartNextTutorial();
		}
	}

	public virtual void SpecificTutorialActions()
	{
	}

	public virtual void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) && !mIsTutorialPaused && mCurrentTutIndex < _TutSteps.Length && !_TutSteps[mCurrentTutIndex]._WaitForUserEvent)
		{
			mSkipTutorial = true;
			SnChannel.StopPool("VO_Pool");
		}
		if (!mIsTutorialPaused && _CanRepeatOnIdle && mCurrentTutIndex < _TutSteps.Length)
		{
			mIdleRepeatTimer -= Time.deltaTime;
			if (mIdleRepeatTimer < 0f)
			{
				mIdleRepeatTimer = _IdleRepeatInterval;
				StartTutorialStep();
			}
		}
		if (mTutorialStepHandler != null)
		{
			mTutorialStepHandler.StepUpdate();
		}
	}

	private IEnumerator BlinkTexts(float waitSec, bool flag)
	{
		yield return new WaitForSeconds(waitSec);
		if (_BlinkEvent != null)
		{
			_BlinkEvent(flag);
		}
		StartCoroutine(BlinkTexts(0.5f, !flag));
	}

	public virtual void LateUpdate()
	{
		if (mTutorialStepHandler != null)
		{
			mTutorialStepHandler.StepLateUpdate();
		}
	}

	public virtual void OnUserEventDone(int tutorialStep)
	{
		if (mCurrentTutIndex == tutorialStep)
		{
			StartNextTutorial();
		}
	}

	protected void ShowDialog()
	{
		SetTutorialBoardVisible(flag: false);
		mUiGenericDBObj = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		if ((bool)mUiGenericDBObj)
		{
			KAUIGenericDB component = mUiGenericDBObj.GetComponent<KAUIGenericDB>();
			component._MessageObject = base.gameObject;
			component._YesMessage = "OnShowDialogYes";
			component._NoMessage = "OnShowDialogNo";
			component.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			component.SetTextByID(_CloseMessage._ID, _CloseMessage._Text, interactive: false);
			KAUI.SetExclusive(component, Color.clear);
		}
	}

	private void OnShowDialogYes()
	{
		mCompletedTutorial = true;
		UnityEngine.Object.Destroy(mUiGenericDBObj);
		if (_StepEndedEvent != null)
		{
			_StepEndedEvent(mCurrentTutIndex, _TutSteps[mCurrentTutIndex]._Name, tutQuit: false);
		}
		Exit();
		KAUI.RemoveExclusive(mTutorialDB);
	}

	private void OnShowDialogNo()
	{
		SetTutorialBoardVisible(flag: true);
		UnityEngine.Object.Destroy(mUiGenericDBObj);
		if (mTutorialDBObj != null)
		{
			ShowTutorialDB(mTutorialDBObj);
		}
		else
		{
			KAUI.RemoveExclusive(mTutorialDB);
		}
	}

	public void SetTutorialBoardVisible(bool flag)
	{
		if (mTutorialDB != null)
		{
			mTutorialDB.SetVisibility(flag);
		}
	}

	public void SetTutorialBoardInteractive(bool flag)
	{
		if (mTutorialDB != null)
		{
			mTutorialDB.SetInteractive(flag);
		}
	}

	public virtual void DisableOrEnableAllItems(KAUI ui, bool flag)
	{
		if (ui == null)
		{
			return;
		}
		for (int i = 0; i < ui.GetItemCount(); i++)
		{
			KAWidget kAWidget = ui.FindItemAt(i);
			if (kAWidget != null)
			{
				kAWidget.SetState(flag ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
			}
		}
	}

	protected virtual void EnableAllItems()
	{
		InteractiveTutStep[] tutSteps = _TutSteps;
		for (int i = 0; i < tutSteps.Length; i++)
		{
			InteractiveTutInterface[] interfaces = tutSteps[i]._Interfaces;
			for (int j = 0; j < interfaces.Length; j++)
			{
				KAUI kAUI = TutorialStepHandler.ResolveInterface(interfaces[j]._Interface);
				if (kAUI != null)
				{
					DisableOrEnableAllItems(kAUI, flag: false);
					kAUI.SetState(KAUIState.INTERACTIVE);
				}
			}
		}
	}

	public virtual void Exit()
	{
		CleanUp();
		EnableAllItems();
		if (AvAvatar.pAvatarCam != null)
		{
			AvAvatar.pAvatarCam.SendMessage("EnableCameraControl", true, SendMessageOptions.DontRequireReceiver);
		}
		if (OnTutorialDialogShowEvent != null)
		{
			OnTutorialDialogShowEvent(isShown: false);
		}
		AvAvatar.pState = AvAvatarState.IDLE;
		if (mTutorialDB != null)
		{
			KAUI.RemoveExclusive(mTutorialDB);
			UnityEngine.Object.Destroy(mTutorialDB);
		}
		mTutorialDB = null;
		mTutorialMsg = null;
		mIsShowingTutorial = false;
		RestoreUI();
		mTimesShown++;
		if (mCompletedTutorial || mTimesShown >= _MaxTimesToBeShown)
		{
			if (_Save)
			{
				ProductData.AddTutorial(_TutIndexKeyName);
			}
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForTaskCompletion("Action", "CompleteTutorial", _TutIndexKeyName);
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
			}
			if (_TutorialCompleteEvent != null)
			{
				_TutorialCompleteEvent();
			}
		}
		if (_Save && mPairData != null && !string.IsNullOrEmpty(_RecordIdxName) && _MaxTimesToBeShown > 1)
		{
			mPairData.SetValueAndSave(_RecordIdxName, mTimesShown.ToString());
		}
		if (_WaitEnd != null)
		{
			_WaitEnd();
		}
		DeleteInstance();
	}

	protected void CleanUp()
	{
		if (mTutorialStepHandler != null)
		{
			mTutorialStepHandler.FinishTutorialStep();
		}
	}

	protected virtual void RestoreUI()
	{
	}

	public virtual void DeleteInstance()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void OnClick(GameObject go)
	{
		if (mTutorialStepHandler != null && mTutorialStepHandler.GetType() == typeof(TutorialStepHandlerObjClick))
		{
			(mTutorialStepHandler as TutorialStepHandlerObjClick).ObjectClicked(go);
		}
	}

	private void OnTutorialDBReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromBundle(_TutorialDBRes);
			if (gameObject != null)
			{
				mTutorialDBObj = gameObject;
				CanPlayTutorial();
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mDownloadError = true;
			break;
		}
	}

	public void InitTutorial(bool isFilterSteps = false)
	{
		if (isFilterSteps)
		{
			FilterStepsByPlatform();
		}
		mCurrentTutIndex = 0;
		mCompletedTutorial = false;
		mIsShowingTutorial = false;
	}

	private void CanPlayTutorial()
	{
		if (mInitShowTutorial)
		{
			ShowTutorial();
		}
	}

	public virtual void ShowTutorial()
	{
		mInitShowTutorial = true;
		if (!mIsShowingTutorial && !mDownloadError && (string.IsNullOrEmpty(_TutorialDBRes) || !(mTutorialDBObj == null)))
		{
			mIsShowingTutorial = true;
			mInitShowTutorial = false;
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
			}
			_CurrentActiveTutorialObject = base.gameObject;
			KAUICursorManager.SetDefaultCursor("Loading");
			if (!mIsPairDataReady)
			{
				StartCoroutine(WaitForPairData());
			}
			else
			{
				PlayTutorial();
			}
		}
	}

	private IEnumerator WaitForPairData()
	{
		if (!mIsPairDataReady)
		{
			yield return null;
		}
		PlayTutorial();
	}

	public bool IsShowingTutorial()
	{
		return mIsShowingTutorial;
	}

	private bool IsInteractivePage(int pageNo)
	{
		bool result = false;
		if (pageNo >= 0 && pageNo < _TutSteps.Length)
		{
			result = _TutSteps[pageNo]._StepDetails._StepType != InteractiveTutStepTypes.STEP_TYPE_NON_INTERACTIVE;
		}
		return result;
	}

	public virtual void TutorialManagerAsyncMessage(string message)
	{
		if (mTutorialStepHandler != null && mTutorialStepHandler.GetType() == typeof(TutorialStepHandlerAsync))
		{
			((TutorialStepHandlerAsync)mTutorialStepHandler).AsyncMessageRecieved(message);
		}
	}

	private void FilterStepsByPlatform()
	{
		if (KAInput.pInstance.IsTouchInput())
		{
			_TutSteps = Array.FindAll(_TutSteps, (InteractiveTutStep stepData) => stepData._IsMobileOnly || (!stepData._IsMobileOnly && !stepData._IsWebOnly));
		}
		else
		{
			_TutSteps = Array.FindAll(_TutSteps, (InteractiveTutStep stepData) => stepData._IsWebOnly || (!stepData._IsMobileOnly && !stepData._IsWebOnly));
		}
	}

	public string GetStepName()
	{
		if (mCurrentTutIndex >= 0 && mCurrentTutIndex < _TutSteps.Length)
		{
			return _TutSteps[mCurrentTutIndex]._Name;
		}
		return null;
	}
}
