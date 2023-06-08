using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UiChestMenu : KAUIMenu, IAdResult
{
	public LocaleString _MissedObjectiveTitle = new LocaleString("[REVIEW] - Missed Objective!");

	public KAWidget _RewardChestTemplate;

	public KAWidget _QuizChestTemplate;

	public AudioClip _CorrectAnswerSFX;

	public AudioClip _WrongAnswerSFX;

	public AdEventType _AdEventType;

	public UiAdQuizPopUp _UiAdQuizPopUp;

	private List<UiChest> mOpenedQuestions = new List<UiChest>();

	private KAUIGenericDB mGenericDB;

	public int pNumNoAnswers { get; set; }

	public int pNumCorrectAnswers { get; set; }

	public int pNumWrongAnswers { get; set; }

	public QuizResultData pQuizResultData { get; set; }

	public UiChest pVideoAdchestItem { get; set; }

	public override void OnClick(KAWidget item)
	{
		if (item.name == "BtnAds")
		{
			_UiAdQuizPopUp.SetVisibility(t: true);
		}
		else
		{
			UiChest uiChest = (UiChest)item;
			if (uiChest.pIsLockedChest)
			{
				OpenMissedObjectivePopUp(uiChest);
			}
			else if (uiChest.pQuestion != null)
			{
				uiChest.SetInteractive(isInteractive: false);
				if (!mOpenedQuestions.Contains(uiChest))
				{
					mOpenedQuestions.Add(uiChest);
					pNumNoAnswers++;
				}
				if (uiChest.pHasTriviaQuestion)
				{
					UiEndDB.pSTTriviaQuizData.LoadQuizDB(uiChest.pQuestion, base.gameObject);
				}
				else
				{
					UiEndDB.pSTQuizData.LoadQuizDB(uiChest.pQuestion, base.gameObject);
				}
			}
		}
		base.OnClick(item);
	}

	private void OpenMissedObjectivePopUp(UiChest chest)
	{
		mGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", chest.pMissedObjectiveText, _MissedObjectiveTitle.GetLocalizedString(), base.gameObject, "", "", "DestroyDB", "", inDestroyOnClick: true);
	}

	private void DestroyDB()
	{
		if (mGenericDB != null)
		{
			Object.Destroy(mGenericDB.gameObject);
		}
	}

	private void OnQuizDBClose(QuizResultData quizResultData)
	{
		UiChest uiChest = (UiChest)mSelectedItem.FindChildItem("BtnChest");
		pQuizResultData = quizResultData;
		if (!(uiChest == null) || pQuizResultData != null)
		{
			if (pQuizResultData._Status.Equals("Pass"))
			{
				pNumCorrectAnswers++;
				pNumNoAnswers--;
				uiChest.InitChestOpen();
			}
			else if (pQuizResultData._Status.Equals("Fail"))
			{
				pNumWrongAnswers++;
				pNumNoAnswers--;
				uiChest.InitChestLock();
				_ParentUi.SendMessage("LogRewardEvents", 0, SendMessageOptions.DontRequireReceiver);
			}
			else if (!uiChest._BtnAds.GetVisibility())
			{
				uiChest.SetInteractive(isInteractive: true);
			}
		}
	}

	private void OnQuizAnswered(bool isCorrect)
	{
		if (isCorrect)
		{
			if (_CorrectAnswerSFX != null)
			{
				SnChannel.Play(_CorrectAnswerSFX, "STSFX_Pool", inForce: true);
			}
		}
		else if (_WrongAnswerSFX != null)
		{
			SnChannel.Play(_WrongAnswerSFX, "STSFX_Pool", inForce: true);
		}
	}

	public void OnAdWatched()
	{
		AdManager.pInstance.LogAdWatchedEvent(_AdEventType, "DragonTactics");
		AdManager.pInstance.SyncAdAvailableCount(_AdEventType, isConsumed: true);
		pVideoAdchestItem.InitChestOpen();
		pVideoAdchestItem.ShowAdBtn(enable: false);
	}

	public void OnAdFailed()
	{
		UtDebug.LogError("OnAdFailed for event: " + _AdEventType);
		pVideoAdchestItem.ShowAdBtn(enable: true);
	}

	public void OnAdSkipped()
	{
		pVideoAdchestItem.ShowAdBtn(enable: true);
	}

	public void OnAdClosed()
	{
	}

	public void OnAdFinished(string eventDataRewardString)
	{
	}

	public void OnAdCancelled()
	{
	}
}
