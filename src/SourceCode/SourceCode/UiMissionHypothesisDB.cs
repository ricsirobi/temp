using System;
using System.Collections.Generic;
using UnityEngine;

public class UiMissionHypothesisDB : KAUI
{
	[Serializable]
	public class Hypothesis
	{
		public LocaleString _HypothesisText = new LocaleString("");

		public bool _IsCorrect;
	}

	public LocaleString _IntroMessage = new LocaleString("Select your hypothesis");

	public static string _HypothesisAnswerKey = "Hypothesis";

	public static string _HypothesisAnswerIDKey = "HypothesisID";

	public static string _HypothesisCorrectKey = "HypothesisCorrect";

	public List<Hypothesis> _Hypotheses = new List<Hypothesis>();

	public float _VerticalPadding = 80f;

	public GameObject _MessageObject;

	public string _CloseMessage = "OnHypothesisDBClose";

	private KAWidget mQuestionText;

	private KAWidget mAnswerOptionTemplate;

	private KAWidget mOkBtn;

	private KAWidget mCloseBtn;

	private List<KAWidget> mOptionItems = new List<KAWidget>();

	private KAWidget mSelectedOption;

	private Task mCurrentTask;

	private string mNPCName = "";

	protected override void Start()
	{
		base.Start();
		Initialize();
		if (mCurrentTask == null)
		{
			SetState(KAUIState.DISABLED);
		}
		KAUI.SetExclusive(this);
	}

	public void Initialize()
	{
		mQuestionText = FindItem("QuestionText");
		mAnswerOptionTemplate = FindItem("AnswerOptionTemplate");
		mOkBtn = FindItem("OkBtn");
		mCloseBtn = FindItem("CancelBtn");
		mOkBtn.SetState(KAUIState.DISABLED);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mOkBtn)
		{
			SaveHypothesis();
		}
		if (item == mCloseBtn)
		{
			CleanupAndClose();
		}
		int optionClicked = GetOptionClicked(item);
		if (optionClicked >= 0)
		{
			SelectAnswerOption(optionClicked);
		}
	}

	public void SetupScreen(Task inTask, string inNPCName)
	{
		mCurrentTask = inTask;
		mNPCName = inNPCName;
		Initialize();
		if (mCurrentTask == null)
		{
			SetState(KAUIState.DISABLED);
			return;
		}
		SetState(KAUIState.INTERACTIVE);
		ClearAnswerItems();
		mQuestionText.SetText(GetTaskHypothesisQuestion(mCurrentTask));
		mQuestionText.SetVisibility(inVisible: true);
		AddHypotheses(mCurrentTask);
	}

	private string GetTaskHypothesisQuestion(Task inTask)
	{
		if (inTask != null && inTask.pData != null && inTask.pData.Title != null)
		{
			return inTask.pData.Title.GetLocalizedString();
		}
		return _IntroMessage.GetLocalizedString();
	}

	private int GetOptionClicked(KAWidget item)
	{
		int num = 0;
		foreach (KAWidget mOptionItem in mOptionItems)
		{
			KAToggleButton kAToggleButton = (KAToggleButton)mOptionItem.FindChildItem("AnswerToggle");
			KAWidget kAWidget = mOptionItem.FindChildItem("AnswerText");
			if ((kAToggleButton != null && kAToggleButton == item) || (kAWidget != null && kAWidget == item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	private void SelectAnswerOption(int ansSelected)
	{
		if (ansSelected >= mOptionItems.Count || ansSelected < 0)
		{
			return;
		}
		foreach (KAWidget mOptionItem in mOptionItems)
		{
			((KAToggleButton)mOptionItem.FindChildItem("AnswerToggle")).SetChecked(isChecked: false);
		}
		((KAToggleButton)mOptionItems[ansSelected].FindChildItem("AnswerToggle")).SetChecked(isChecked: true);
		mSelectedOption = mOptionItems[ansSelected];
		mOkBtn.SetState(KAUIState.INTERACTIVE);
	}

	private void AddHypotheses(Task inTask)
	{
		int num = 0;
		if (_Hypotheses == null || _Hypotheses.Count <= 0)
		{
			return;
		}
		Vector3 position = mAnswerOptionTemplate.transform.position;
		UtUtilities.Shuffle(_Hypotheses);
		foreach (Hypothesis hypothesis in _Hypotheses)
		{
			KAWidget kAWidget = DuplicateWidget(mAnswerOptionTemplate);
			if (kAWidget != null)
			{
				kAWidget.transform.localPosition = position;
				position.y -= _VerticalPadding;
				kAWidget.SetVisibility(inVisible: true);
				KAWidget kAWidget2 = kAWidget.FindChildItem("AnswerText");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(hypothesis._HypothesisText.GetLocalizedString());
				}
				AddWidget(kAWidget);
				kAWidget.transform.parent = mAnswerOptionTemplate.transform.parent;
				KAWidgetUserData kAWidgetUserData = new KAWidgetUserData();
				kAWidgetUserData._Index = num;
				kAWidget.SetUserData(kAWidgetUserData);
				mOptionItems.Add(kAWidget);
				num++;
			}
		}
	}

	private void SaveHypothesis()
	{
		if (!(mSelectedOption != null))
		{
			return;
		}
		KAWidgetUserData userData = mSelectedOption.GetUserData();
		if (userData != null)
		{
			mCurrentTask.pPayload.Set(_HypothesisAnswerKey, _Hypotheses[userData._Index]._HypothesisText._Text);
			mCurrentTask.pPayload.Set(_HypothesisAnswerIDKey, _Hypotheses[userData._Index]._HypothesisText._ID.ToString());
			mCurrentTask.pPayload.Set(_HypothesisCorrectKey, _Hypotheses[userData._Index]._IsCorrect.ToString());
			if (mCurrentTask != null && !string.IsNullOrEmpty(mNPCName))
			{
				mCurrentTask.CheckForCompletion("Meet", mNPCName, "", "");
			}
		}
		CleanupAndClose();
	}

	private void ClearAnswerItems()
	{
		if (mOptionItems == null)
		{
			return;
		}
		foreach (KAWidget mOptionItem in mOptionItems)
		{
			RemoveWidget(mOptionItem);
		}
		mOptionItems.Clear();
	}

	private void CleanupAndClose()
	{
		if (_MessageObject != null && !string.IsNullOrEmpty(_CloseMessage))
		{
			_MessageObject.SendMessage(_CloseMessage, SendMessageOptions.DontRequireReceiver);
		}
		KAUI.RemoveExclusive(this);
		UnityEngine.Object.Destroy(base.gameObject);
		RsResourceManager.UnloadUnusedAssets();
	}
}
