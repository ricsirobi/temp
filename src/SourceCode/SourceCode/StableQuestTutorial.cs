using System.Collections.Generic;
using UnityEngine;

public class StableQuestTutorial : InteractiveTutManager
{
	private bool mWaitForInit = true;

	private int prevTutID = -1;

	private KAUI mUiObject;

	private bool mBlinking;

	private GameObject mBlinkObj;

	private bool mStartBlinking;

	private float mBlinkTime;

	public Vector3 _MaxBlinkTimeScale = new Vector3(1.1f, 1.1f, 1.1f);

	public Vector3 _MinBlinkTimeScale = new Vector3(0.8f, 0.8f, 0.8f);

	private Vector3 mDefaultScale = Vector3.one;

	public float _BlinkDuration = 0.5f;

	public int _TutorialMissionID = 110;

	public int _TutorialSlotID;

	private TweenScale mTweenScale;

	private void HighlightObject(bool canShowHightlight, GameObject highLightObj)
	{
		ObClickable component = highLightObj.GetComponent<ObClickable>();
		if (component != null)
		{
			if (canShowHightlight && component._HighlightMaterial != null)
			{
				component.Highlight();
			}
			else
			{
				component.UnHighlight();
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (Time.realtimeSinceStartup - mBlinkTime > _BlinkDuration && mStartBlinking && mBlinkObj != null)
		{
			mBlinkTime = Time.realtimeSinceStartup;
			if (!mBlinking)
			{
				mTweenScale = TweenScale.Begin(mBlinkObj, _BlinkDuration, _MaxBlinkTimeScale);
			}
			else
			{
				mTweenScale = TweenScale.Begin(mBlinkObj, _BlinkDuration, _MinBlinkTimeScale);
			}
			mBlinking = !mBlinking;
			mTweenScale.method = UITweener.Method.Linear;
		}
		if (prevTutID == mCurrentTutIndex)
		{
			return;
		}
		switch (mCurrentTutIndex)
		{
		case 0:
			if (!(StableManager.pInstance != null) || !(StableManager.pInstance._StableQuestJobBoardObject != null))
			{
				break;
			}
			if (mWaitForInit)
			{
				StableManager.pInstance._StableQuestJobBoardObject.GetComponent<ObClickable>()._UseGlobalActive = false;
				ObClickable.pGlobalActive = false;
				TimedMissionManager.pInstance.FindMission(_TutorialMissionID).pPlayedCount = 0;
				StableManager.pInstance.UnSubscribeFromOnSlotStateStatusChange();
				TimedMissionManager.pInstance.AssignMission(_TutorialSlotID, _TutorialMissionID);
				StableManager.pInstance.SubscribeToOnSlotStateStatusChange();
				HighlightObject(canShowHightlight: true, StableManager.pInstance._StableQuestJobBoardObject);
				bool flag = false;
				for (int i = 0; i < _TutSteps[mCurrentTutIndex]._PlayerControls.Length; i++)
				{
					if (_TutSteps[mCurrentTutIndex]._PlayerControls[i] == InteractiveTutPlayerActions.DISABLE_CONTROL)
					{
						flag = true;
						break;
					}
				}
				if (flag && StableManager.pInstance._StableQuestJobBoardVisitMarker != null)
				{
					AvAvatar.TeleportToObject(StableManager.pInstance._StableQuestJobBoardVisitMarker);
				}
				mWaitForInit = false;
			}
			else if (StableManager.pInstance.pStableQuestUIInstance != null)
			{
				StableManager.pInstance._StableQuestJobBoardObject.GetComponent<ObClickable>()._UseGlobalActive = true;
				StartNextTutorial();
				mWaitForInit = true;
			}
			break;
		case 1:
			mUiObject = StableManager.pInstance.pStableQuestUIInstance.GetComponent<UiStableQuestMain>()._StableQuestSlotsUI._MenuList[0];
			mUiObject.pEvents.OnClick += OnClick;
			if (((KAUIMenu)mUiObject).GetItemCount() <= 0)
			{
				break;
			}
			foreach (KAWidget item in ((KAUIMenu)mUiObject).GetItems())
			{
				StableQuestSlotWidget stableQuestSlotWidget = (StableQuestSlotWidget)item;
				if (stableQuestSlotWidget.pMissionSlotData.MissionID != 110 && stableQuestSlotWidget.pMissionSlotData.SlotID != 0)
				{
					stableQuestSlotWidget.SetVisibility(inVisible: false);
				}
				else
				{
					SetUpFocusedObject(item.gameObject);
				}
			}
			SetupTutorialStep();
			HighlightObject(canShowHightlight: false, StableManager.pInstance._StableQuestJobBoardObject);
			prevTutID = mCurrentTutIndex;
			break;
		case 2:
		{
			mUiObject = StableManager.pInstance.pStableQuestUIInstance.GetComponent<UiStableQuestMain>()._StableQuestDetailsUI.pDragonsSelectedMenu;
			mUiObject.pEvents.OnClick += OnClick;
			if (((KAUIMenu)mUiObject).GetItemCount() <= 0)
			{
				break;
			}
			using (List<KAWidget>.Enumerator enumerator = ((KAUIMenu)mUiObject).GetItems().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					KAWidget current2 = enumerator.Current;
					SetUpFocusedObject(current2.gameObject);
				}
			}
			SetupTutorialStep();
			prevTutID = mCurrentTutIndex;
			break;
		}
		case 3:
		{
			mUiObject = StableManager.pInstance.pStableQuestUIInstance.GetComponent<UiStableQuestMain>()._StableQuestDetailsUI._PopUpDragonSelectionUI._MenuList[0];
			mUiObject.pEvents.OnClick += OnClick;
			if (((KAUIMenu)mUiObject).GetItemCount() <= 0)
			{
				break;
			}
			using (List<KAWidget>.Enumerator enumerator = ((KAUIMenu)mUiObject).GetItems().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					KAWidget current3 = enumerator.Current;
					SetUpFocusedObject(current3.FindChildItem("BtnSelectToggle").gameObject);
				}
			}
			SetupTutorialStep();
			prevTutID = mCurrentTutIndex;
			break;
		}
		case 4:
			mUiObject = StableManager.pInstance.pStableQuestUIInstance.GetComponent<UiStableQuestMain>()._StableQuestDetailsUI._PopUpDragonSelectionUI;
			mUiObject.pEvents.OnClick += OnClick;
			SetUpFocusedObject(mUiObject.FindItem("BtnConfirm").gameObject);
			SetupTutorialStep();
			prevTutID = mCurrentTutIndex;
			break;
		case 5:
			mUiObject = StableManager.pInstance.pStableQuestUIInstance.GetComponent<UiStableQuestMain>()._StableQuestDetailsUI;
			mUiObject.pEvents.OnClick += OnClick;
			SetUpFocusedObject(mUiObject.FindItem("BtnStart").gameObject);
			SetupTutorialStep();
			prevTutID = mCurrentTutIndex;
			break;
		case 6:
			mUiObject = StableManager.pInstance.pStableQuestUIInstance.GetComponent<UiStableQuestMain>()._StableQuestDetailsUI._PopUpStartMissionUI;
			mUiObject.pEvents.OnClick += OnClick;
			SetUpFocusedObject(mUiObject.FindItem("YesBtn").gameObject);
			SetupTutorialStep();
			prevTutID = mCurrentTutIndex;
			break;
		case 7:
			if (mWaitForInit)
			{
				mUiObject = StableManager.pInstance.pStableQuestUIInstance.GetComponent<UiStableQuestMain>()._StableQuestDetailsUI;
				mUiObject.pEvents.OnClick += OnClick;
				SetUpFocusedObject(mUiObject.FindItem("BtnCompleteNow").gameObject);
				SetupTutorialStep();
				mWaitForInit = false;
			}
			else if (StableManager.pInstance.pStableQuestUIInstance.GetComponent<UiStableQuestMain>()._StableQuestResultsUI.GetVisibility())
			{
				StartNextTutorial();
				mWaitForInit = true;
			}
			break;
		case 8:
			mUiObject = StableManager.pInstance.pStableQuestUIInstance.GetComponent<UiStableQuestMain>()._StableQuestResultsUI;
			mUiObject.pEvents.OnClick += OnClick;
			SetUpFocusedObject(mUiObject.FindItem("BtnClaim").gameObject);
			SetupTutorialStep();
			prevTutID = mCurrentTutIndex;
			break;
		}
	}

	private void SetUpFocusedObject(GameObject focusedObj)
	{
		mBlinkObj = null;
		mStartBlinking = true;
		mDefaultScale = focusedObj.transform.localScale;
		mBlinkObj = focusedObj;
	}

	private void OnClick(KAWidget obj)
	{
		if (!(obj != null))
		{
			return;
		}
		switch (mCurrentTutIndex)
		{
		case 1:
			if (mUiObject != null && obj.name == "BtnSlot")
			{
				GoToNext();
			}
			break;
		case 2:
			if (mUiObject != null)
			{
				GoToNext();
			}
			break;
		case 3:
			if (mUiObject != null && obj.name == "BtnSelectToggle")
			{
				GoToNext();
			}
			break;
		case 4:
			if (mUiObject != null && obj.name == "BtnConfirm")
			{
				GoToNext();
			}
			break;
		case 5:
			if (mUiObject != null && obj.name == "BtnStart")
			{
				GoToNext();
			}
			break;
		case 6:
			if (mUiObject != null && obj.name == "YesBtn")
			{
				GoToNext();
			}
			break;
		case 7:
			if (mUiObject != null && obj.name == "BtnCompleteNow")
			{
				RemoveFocus();
			}
			break;
		case 8:
			if (mUiObject != null && obj.name == "BtnClaim")
			{
				GoToNext();
			}
			break;
		}
	}

	private void GoToNext()
	{
		StartNextTutorial();
		RemoveFocus();
	}

	private void RemoveFocus()
	{
		mUiObject.pEvents.OnClick -= OnClick;
		mStartBlinking = false;
		mBlinkObj.transform.localScale = mDefaultScale;
		mTweenScale.to = mDefaultScale;
		mBlinkObj = null;
		mUiObject = null;
	}

	public override void DeleteInstance()
	{
		base.DeleteInstance();
		ObClickable.pGlobalActive = true;
		UserNotifyStableTutorial.pInstance.SetToWaitState(flag: false);
		UserNotifyStableTutorial.pInstance.CheckForNextTut();
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
	}
}
