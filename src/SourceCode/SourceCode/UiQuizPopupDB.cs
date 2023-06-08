using System.Collections.Generic;
using UnityEngine;

public class UiQuizPopupDB : KAUI
{
	public class QuizAnswerWidgetData : KAWidgetUserData
	{
		public QuizAnswer _Answer;
	}

	public float _QuizCloseTime;

	public float _MinimumSelectionColliderWidth = 80f;

	public bool _ResizeSelectionColliderWidth;

	public bool _ShowSolutionOnWrongEntry = true;

	[Tooltip("If there is more than one correct answer but we only want to allow one to be selected.")]
	public bool _AllowOnlyOneAnswer;

	public Color _CorrectAnswerColor = Color.green;

	public Color _IncorrectAnswerColor = Color.red;

	public GameObject _MessageObject;

	public string _CloseMessage = "OnQuizDBClose";

	public string _QuizAnsweredMessage = "OnQuizAnswered";

	public QuizQuestion[] _QuizQuestions;

	public bool _ShuffleAnswers = true;

	public LocaleString _Title = new LocaleString("Quiz");

	public float _VerticalPadding = 40f;

	public KAUI _OptionsContainerUI;

	private QuizResultData mQuizResultData = new QuizResultData();

	private KAWidget mQuestionText;

	private KAWidget mAnswerOptionTemplate;

	private KAWidget mOkBtn;

	private KAWidget mCloseBtn;

	private KAWidget mInfoText;

	private KAWidget mTitleText;

	private Task mCurrentTask;

	private string mNPCName = "";

	private bool mShowingQuestion = true;

	private bool mIsInitialized;

	private bool mCheckForTaskCompletion;

	private bool mIsQuestionAttemped;

	private int mCorrectAnswers;

	private int mSelectedCorrectAnswers;

	private int mCurrentQuestion = -1;

	private int mImageToLoad;

	private List<KAWidget> mOptionItems = new List<KAWidget>();

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	public void Initialize()
	{
		if (!mIsInitialized)
		{
			mQuestionText = FindItem("QuestionText");
			mAnswerOptionTemplate = FindItem("AnswerOptionTemplate");
			mOkBtn = FindItem("OkBtn");
			mCloseBtn = FindItem("CancelBtn");
			mTitleText = FindItem("TitleText");
			mInfoText = FindItem("InfoText");
			mTitleText.SetText(_Title.GetLocalizedString());
			if (_OptionsContainerUI == null)
			{
				_OptionsContainerUI = this;
			}
			else
			{
				_OptionsContainerUI.pEvents.OnClick += OnClick;
			}
			ShowQuestion(0);
			mCheckForTaskCompletion = true;
			mIsInitialized = true;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mOkBtn)
		{
			if (mShowingQuestion)
			{
				ShowSolution(mCurrentQuestion);
			}
			else
			{
				ShowQuestion(mCurrentQuestion + 1);
			}
			return;
		}
		if (item == mCloseBtn)
		{
			CleanupAndClose();
			return;
		}
		int optionClicked = GetOptionClicked(item);
		if (optionClicked < 0)
		{
			return;
		}
		SelectAnswerOption(optionClicked);
		if (mOkBtn == null && SelectedOptions() == mCorrectAnswers)
		{
			if (mShowingQuestion)
			{
				ShowSolution(mCurrentQuestion);
			}
			else
			{
				ShowQuestion(mCurrentQuestion + 1);
			}
			if (_QuizCloseTime > 0f)
			{
				Invoke("CleanupAndClose", _QuizCloseTime);
			}
		}
	}

	public void SetupScreen(Task inTask, string inNPCName)
	{
		mCurrentTask = inTask;
		mNPCName = inNPCName;
		Initialize();
		SetState(KAUIState.INTERACTIVE);
		KAUI.SetExclusive(this);
	}

	public void ShowQuestion(int inQuestionIdx)
	{
		if (_QuizQuestions == null || _QuizQuestions.Length == 0 || inQuestionIdx < 0)
		{
			UtDebug.LogError("Invalid Question: " + inQuestionIdx);
			return;
		}
		if (inQuestionIdx >= _QuizQuestions.Length)
		{
			SaveAndExitQuiz();
			return;
		}
		if (_OptionsContainerUI != this)
		{
			_OptionsContainerUI.SetVisibility(inVisible: false);
		}
		mCurrentQuestion = inQuestionIdx;
		ClearAnswerItems();
		mQuestionText.SetText(_QuizQuestions[inQuestionIdx].QuestionText.GetLocalizedString());
		mQuestionText.SetVisibility(inVisible: true);
		if (_QuizQuestions[inQuestionIdx].QuestionInstructionText != null)
		{
			mInfoText.SetText(_QuizQuestions[inQuestionIdx].QuestionInstructionText.GetLocalizedString());
		}
		mInfoText.SetVisibility(inVisible: true);
		KAWidget kAWidget = FindItem("QuestionImage");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
			if (!string.IsNullOrEmpty(_QuizQuestions[inQuestionIdx].ImageURL))
			{
				mImageToLoad++;
				kAWidget.SetTextureFromURL(_QuizQuestions[inQuestionIdx].ImageURL, base.gameObject);
			}
		}
		AddAnswers(inQuestionIdx);
		if (mOkBtn != null)
		{
			mOkBtn.SetState(KAUIState.DISABLED);
		}
		SetState(KAUIState.INTERACTIVE);
		mShowingQuestion = true;
		if (mImageToLoad > 0)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
		}
		else if (_OptionsContainerUI != this)
		{
			_OptionsContainerUI.SetVisibility(inVisible: true);
		}
	}

	private int GetOptionClicked(KAWidget item)
	{
		int num = 0;
		foreach (KAWidget mOptionItem in mOptionItems)
		{
			KAWidget kAWidget = mOptionItem.FindChildItem("AnswerToggle");
			KAWidget kAWidget2 = mOptionItem.FindChildItem("AnswerText");
			if ((kAWidget != null && kAWidget == item) || (kAWidget2 != null && kAWidget2 == item))
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
		if (mCorrectAnswers == 1 || _AllowOnlyOneAnswer)
		{
			foreach (KAWidget mOptionItem in mOptionItems)
			{
				((KAToggleButton)mOptionItem.FindChildItem("AnswerToggle")).SetChecked(isChecked: false);
			}
		}
		((KAToggleButton)mOptionItems[ansSelected].FindChildItem("AnswerToggle")).SetChecked(isChecked: true);
		if (mOkBtn != null)
		{
			mOkBtn.SetState(KAUIState.INTERACTIVE);
		}
	}

	private void ShowSolution(int inQuestionIdx)
	{
		if (_QuizQuestions == null || _QuizQuestions.Length == 0 || inQuestionIdx < 0 || inQuestionIdx >= _QuizQuestions.Length)
		{
			UtDebug.LogError("Invalid Question: " + inQuestionIdx);
			return;
		}
		bool flag = true;
		mSelectedCorrectAnswers = 0;
		string text = "";
		foreach (KAWidget mOptionItem in mOptionItems)
		{
			if (!(mOptionItem.GetUserData() is QuizAnswerWidgetData quizAnswerWidgetData))
			{
				continue;
			}
			KAWidget kAWidget = mOptionItem.FindChildItem("AnswerText");
			KAToggleButton kAToggleButton = (KAToggleButton)mOptionItem.FindChildItem("AnswerToggle");
			if (quizAnswerWidgetData._Answer.IsCorrect)
			{
				if (_ShowSolutionOnWrongEntry || kAToggleButton.IsChecked())
				{
					if (kAToggleButton.IsChecked())
					{
						mSelectedCorrectAnswers++;
						mQuizResultData._SelectedAnswers.Add(kAWidget.GetText());
					}
					if (kAToggleButton != null)
					{
						kAToggleButton._CheckedInfo._ColorInfo._UseColorEffect = false;
						kAToggleButton._HoverInfo._ColorInfo._UseColorEffect = false;
					}
					kAWidget._HoverInfo._ColorInfo._UseColorEffect = false;
					UILabel label = kAWidget.GetLabel();
					if (label != null)
					{
						label.color = _CorrectAnswerColor;
					}
				}
			}
			else if (kAToggleButton.IsChecked())
			{
				if (kAToggleButton != null)
				{
					kAToggleButton._CheckedInfo._ColorInfo._UseColorEffect = false;
					kAToggleButton._HoverInfo._ColorInfo._UseColorEffect = false;
				}
				kAWidget._HoverInfo._ColorInfo._UseColorEffect = false;
				UILabel label2 = kAWidget.GetLabel();
				if (label2 != null)
				{
					label2.color = _IncorrectAnswerColor;
				}
				flag = false;
				mQuizResultData._SelectedAnswers.Add(kAWidget.GetText());
			}
			mOptionItem.SetState(KAUIState.NOT_INTERACTIVE);
			if (string.IsNullOrEmpty(text) && flag && kAToggleButton.IsChecked() && quizAnswerWidgetData._Answer._SelectedResponseText != null && !string.IsNullOrEmpty(quizAnswerWidgetData._Answer._SelectedResponseText._Text))
			{
				text = quizAnswerWidgetData._Answer._SelectedResponseText.GetLocalizedString();
			}
		}
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage(_QuizAnsweredMessage, IsQuizAnsweredCorrect(), SendMessageOptions.DontRequireReceiver);
		}
		mCheckForTaskCompletion = (mCheckForTaskCompletion && IsQuizAnsweredCorrect()) || _ShowSolutionOnWrongEntry;
		if (!string.IsNullOrEmpty(text))
		{
			mInfoText.SetText(text);
		}
		else if (_QuizQuestions[mCurrentQuestion].CorrectAnswerText != null && _QuizQuestions[mCurrentQuestion].IncorrectAnswerText != null)
		{
			LocaleString localeString = (IsQuizAnsweredCorrect() ? _QuizQuestions[mCurrentQuestion].CorrectAnswerText : _QuizQuestions[mCurrentQuestion].IncorrectAnswerText);
			mInfoText.SetText(localeString.GetLocalizedString());
		}
		mIsQuestionAttemped = true;
		mShowingQuestion = false;
	}

	private void SaveAndExitQuiz()
	{
		if (mCurrentTask != null && !string.IsNullOrEmpty(mNPCName) && mCheckForTaskCompletion)
		{
			mCurrentTask.CheckForCompletion("Meet", mNPCName, "", "");
		}
		CleanupAndClose();
	}

	private void AddAnswers(int inQuestionIdx)
	{
		int num = 0;
		if (inQuestionIdx < 0 || inQuestionIdx >= _QuizQuestions.Length)
		{
			return;
		}
		Vector3 position = mAnswerOptionTemplate.transform.position;
		List<QuizAnswer> list = new List<QuizAnswer>(_QuizQuestions[inQuestionIdx].Answers);
		if (_ShuffleAnswers)
		{
			UtUtilities.Shuffle(list);
		}
		foreach (QuizAnswer item in list)
		{
			KAWidget kAWidget = DuplicateWidget(mAnswerOptionTemplate);
			if (!(kAWidget != null))
			{
				continue;
			}
			kAWidget.transform.localPosition = position;
			position.y -= _VerticalPadding;
			kAWidget.SetVisibility(inVisible: true);
			KAWidget kAWidget2 = kAWidget.FindChildItem("AnswerText");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(item.AnswerText.GetLocalizedString());
			}
			if (_ResizeSelectionColliderWidth && kAWidget2 != null)
			{
				UILabel label = kAWidget2.GetLabel();
				BoxCollider component = kAWidget2.GetComponent<BoxCollider>();
				if (label != null && component != null && label.alignment == NGUIText.Alignment.Left)
				{
					component.size = new Vector3((label.printedSize.x > _MinimumSelectionColliderWidth) ? label.printedSize.x : _MinimumSelectionColliderWidth, component.size.y, component.size.z);
					component.center = new Vector3(component.size.x / 2f, component.center.y, component.center.z);
				}
			}
			KAWidget kAWidget3 = kAWidget.FindChildItem("AnswerImage");
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(inVisible: false);
				if (!string.IsNullOrEmpty(item.ImageURL))
				{
					mImageToLoad++;
					kAWidget3.SetTextureFromURL(item.ImageURL, base.gameObject);
				}
				else if (!string.IsNullOrEmpty(item._SpriteName))
				{
					kAWidget3.SetSprite(item._SpriteName);
					kAWidget3.SetVisibility(inVisible: true);
				}
			}
			_OptionsContainerUI.AddWidget(kAWidget);
			if (_OptionsContainerUI == this)
			{
				kAWidget.transform.parent = mAnswerOptionTemplate.transform.parent;
			}
			QuizAnswerWidgetData quizAnswerWidgetData = new QuizAnswerWidgetData();
			quizAnswerWidgetData._Index = num;
			quizAnswerWidgetData._Answer = item;
			kAWidget.SetUserData(quizAnswerWidgetData);
			mOptionItems.Add(kAWidget);
			num++;
			if (item.IsCorrect)
			{
				mCorrectAnswers++;
			}
		}
	}

	private void OnTextureLoaded(KAWidget imageWidget)
	{
		if (imageWidget != null)
		{
			imageWidget.SetVisibility(inVisible: true);
		}
		mImageToLoad--;
		if (mImageToLoad <= 0)
		{
			if (_OptionsContainerUI != this)
			{
				_OptionsContainerUI.SetVisibility(inVisible: true);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
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

	private int SelectedOptions()
	{
		int num = 0;
		foreach (KAWidget mOptionItem in mOptionItems)
		{
			if (((KAToggleButton)mOptionItem.FindChildItem("AnswerToggle")).IsChecked())
			{
				num++;
			}
		}
		return num;
	}

	private bool IsQuizAnsweredCorrect()
	{
		if (mSelectedCorrectAnswers != mCorrectAnswers)
		{
			if (_AllowOnlyOneAnswer)
			{
				return mSelectedCorrectAnswers == 1;
			}
			return false;
		}
		return true;
	}

	private void CleanupAndClose()
	{
		if (_MessageObject != null && !string.IsNullOrEmpty(_CloseMessage))
		{
			if (mIsQuestionAttemped)
			{
				mQuizResultData._Status = (IsQuizAnsweredCorrect() ? "Pass" : "Fail");
			}
			_MessageObject.SendMessage(_CloseMessage, mQuizResultData, SendMessageOptions.DontRequireReceiver);
		}
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
		RsResourceManager.UnloadUnusedAssets();
	}
}
