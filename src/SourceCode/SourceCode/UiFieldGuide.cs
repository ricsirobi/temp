using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class UiFieldGuide : KAUI, IJournal
{
	public string _DialogAssetName = "PfKAUIGenericDBSm";

	public LocaleString _SubtopicLockedText = new LocaleString("You have not unlocked this content item yet.");

	public LocaleString _FieldGuideText = new LocaleString("Field Guide");

	public UiStandardGuide _UiStandardGuide;

	[NonSerialized]
	public List<FieldGuideChapter> mUnlockedChapters = new List<FieldGuideChapter>();

	private static FieldGuideData mFieldGuideData;

	private static bool mIsInitialized = false;

	public static string mLastSelectedChapterName = string.Empty;

	private static List<FieldGuideChapter> mRecentlyUnlockedChapters = new List<FieldGuideChapter>();

	private KAWidget mBkgTopics;

	private KAWidget mSkillTitle;

	private KAWidget mTopicsTitle;

	private KAWidget mChapterImageBig;

	private KAWidget mBtnStandardPage;

	private UiTopicMenu mTopicMenu;

	private string mSelectedSubjectName = "Science";

	private FieldGuideSubject mSelectedSubject;

	private FieldGuideTopic mSelectedTopic;

	private FieldGuideSubTopic mSelectedSubTopic;

	private FieldGuideLesson mSelectedLesson;

	private FieldGuideChapter mSelectedChapter;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mCanHide;

	public static FieldGuideData pFieldGuideData => mFieldGuideData;

	public static bool pIsInitialized
	{
		get
		{
			return mIsInitialized;
		}
		set
		{
			mIsInitialized = value;
		}
	}

	public static List<FieldGuideChapter> pRecentlyUnlockedChapters
	{
		get
		{
			return mRecentlyUnlockedChapters;
		}
		set
		{
			mRecentlyUnlockedChapters = value;
		}
	}

	public FieldGuideSubject pSelectedSubject
	{
		get
		{
			return mSelectedSubject;
		}
		set
		{
			mSelectedSubject = value;
		}
	}

	public FieldGuideTopic pSelectedTopic
	{
		get
		{
			return mSelectedTopic;
		}
		set
		{
			mSelectedTopic = value;
		}
	}

	public FieldGuideSubTopic pSelectedSubTopic
	{
		get
		{
			return mSelectedSubTopic;
		}
		set
		{
			mSelectedSubTopic = value;
		}
	}

	public FieldGuideLesson pSelectedLesson
	{
		get
		{
			return mSelectedLesson;
		}
		set
		{
			mSelectedLesson = value;
		}
	}

	public FieldGuideChapter pSelectedChapter
	{
		get
		{
			return mSelectedChapter;
		}
		private set
		{
			mSelectedChapter = value;
			ShowItems(inVisible: true);
			PopulatePage();
			GetMenu("UiLessonDetailMenu").SendMessage("Populate");
		}
	}

	protected override void Start()
	{
		base.Start();
		pIsInitialized = false;
		InitializeItems();
		if (mFieldGuideData == null)
		{
			LoadFieldGuideData();
		}
	}

	public static void LoadFieldGuideData()
	{
		RsResourceManager.Load(GameConfig.GetKeyData("FieldGuideDataFile"), XmlLoadEventHandler);
	}

	private static void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (!inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			return;
		}
		using StringReader textReader = new StringReader((string)inObject);
		mFieldGuideData = (FieldGuideData)new XmlSerializer(typeof(FieldGuideData)).Deserialize(textReader);
	}

	private void InitializeItems()
	{
		mTopicsTitle = FindItem("TxtTopicsTitle");
		mSkillTitle = FindItem("TxtSkillTitle");
		mBkgTopics = FindItem("BkgTopics");
		mChapterImageBig = FindItem("ChapterImageBig");
		mTopicMenu = (UiTopicMenu)GetMenu("UiTopicMenu");
		mBtnStandardPage = FindItem("BtnStandardPage");
	}

	private void PopulateData()
	{
		FieldGuideSubject[] subjects = mFieldGuideData.Subjects;
		foreach (FieldGuideSubject fieldGuideSubject in subjects)
		{
			if (mSelectedSubjectName.Equals(fieldGuideSubject.Name._Text))
			{
				pSelectedSubject = fieldGuideSubject;
			}
		}
		GetMenu("UiTopicMenu").SendMessage("Populate");
	}

	public void ShowItems(bool inVisible)
	{
		if (mSkillTitle != null)
		{
			mSkillTitle.SetVisibility(inVisible);
		}
		if (mTopicsTitle != null)
		{
			mTopicsTitle.SetVisibility(inVisible);
		}
		if (UtPlatform.IsMobile() && inVisible)
		{
			AdManager.DisplayAd(AdEventType.FIELD_GUIDE, AdOption.FULL_SCREEN);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		GetMenuByIndex(0).SetVisibility(inVisible);
		ShowItems(inVisible);
		if (inVisible)
		{
			mSkillTitle.SetVisibility(inVisible: false);
			PopulateData();
			mBkgTopics.SetVisibility(inVisible);
		}
	}

	private void PopulatePage()
	{
		if (pSelectedChapter.Title.IsUnlocked())
		{
			mSkillTitle.SetText(pSelectedChapter.Title.Data.GetLocalizedString());
		}
		else
		{
			mSkillTitle.SetVisibility(inVisible: false);
		}
	}

	private void CheckForTopicName(string topicName)
	{
		bool flag = false;
		FieldGuideSubject[] subjects = mFieldGuideData.Subjects;
		foreach (FieldGuideSubject fieldGuideSubject in subjects)
		{
			if (!fieldGuideSubject.Name._Text.Equals(mSelectedSubjectName))
			{
				continue;
			}
			pSelectedSubject = fieldGuideSubject;
			FieldGuideTopic[] topics = fieldGuideSubject.Topics;
			foreach (FieldGuideTopic fieldGuideTopic in topics)
			{
				if (fieldGuideTopic.Name._Text.Equals(topicName))
				{
					pSelectedTopic = fieldGuideTopic;
					GetMenu("UiTopicMenu").SendMessage("Populate");
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			pSelectedTopic = null;
			GetMenu("UiTopicMenu").SendMessage("Populate");
			mSkillTitle.SetVisibility(inVisible: false);
		}
	}

	public void CheckForLesson(string lessonName)
	{
		FieldGuideSubject[] subjects = mFieldGuideData.Subjects;
		for (int i = 0; i < subjects.Length; i++)
		{
			FieldGuideTopic[] topics = subjects[i].Topics;
			for (int j = 0; j < topics.Length; j++)
			{
				FieldGuideSubTopic[] subTopics = topics[j].SubTopics;
				for (int k = 0; k < subTopics.Length; k++)
				{
					FieldGuideLesson[] lessons = subTopics[k].Lessons;
					foreach (FieldGuideLesson fieldGuideLesson in lessons)
					{
						if (fieldGuideLesson.Name._Text.Equals(lessonName))
						{
							pSelectedLesson = fieldGuideLesson;
							return;
						}
					}
				}
			}
		}
	}

	public void CheckForChapter(string chapterTitle)
	{
		FieldGuideSubject[] subjects = mFieldGuideData.Subjects;
		for (int i = 0; i < subjects.Length; i++)
		{
			FieldGuideTopic[] topics = subjects[i].Topics;
			for (int j = 0; j < topics.Length; j++)
			{
				FieldGuideSubTopic[] subTopics = topics[j].SubTopics;
				for (int k = 0; k < subTopics.Length; k++)
				{
					FieldGuideLesson[] lessons = subTopics[k].Lessons;
					for (int l = 0; l < lessons.Length; l++)
					{
						FieldGuideChapter[] chapters = lessons[l].Chapters;
						foreach (FieldGuideChapter fieldGuideChapter in chapters)
						{
							if (fieldGuideChapter.Title.Data._Text.Equals(chapterTitle))
							{
								mLastSelectedChapterName = chapterTitle;
								pSelectedChapter = fieldGuideChapter;
								return;
							}
						}
					}
				}
			}
		}
	}

	public void MoveToNextChapter()
	{
		FieldGuideChapter fieldGuideChapter = null;
		int num = mUnlockedChapters.IndexOf(pSelectedChapter);
		if (num < mUnlockedChapters.Count - 1)
		{
			fieldGuideChapter = mUnlockedChapters[num + 1];
		}
		if (fieldGuideChapter != null && mTopicMenu != null)
		{
			mTopicMenu.OpenChapter(fieldGuideChapter.Title.Data._Text);
		}
	}

	public void MoveToPreviousChapter()
	{
		FieldGuideChapter fieldGuideChapter = null;
		int num = mUnlockedChapters.IndexOf(pSelectedChapter);
		if (num > 0)
		{
			fieldGuideChapter = mUnlockedChapters[num - 1];
		}
		if (fieldGuideChapter != null && mTopicMenu != null)
		{
			mTopicMenu.OpenChapter(fieldGuideChapter.Title.Data._Text);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget == null)
		{
			return;
		}
		base.OnClick(inWidget);
		if (inWidget == mBtnStandardPage)
		{
			if (!(_UiStandardGuide == null))
			{
				SetVisibility(inVisible: false);
				_UiStandardGuide.SetVisibility(inVisible: true);
			}
		}
		else
		{
			CheckForTopicName(inWidget.name);
		}
	}

	public void HighlightImage(string assetbundlePath, string assetName)
	{
		mChapterImageBig.SetTextureFromBundle(assetbundlePath, assetName);
		mChapterImageBig.SetVisibility(inVisible: true);
	}

	public override void OnHover(KAWidget inWidget, bool inIsHover)
	{
		base.OnHover(inWidget, inIsHover);
		if (!(inWidget == null))
		{
			if (!inIsHover)
			{
				mChapterImageBig.SetVisibility(inVisible: false);
			}
			else if (inWidget == mChapterImageBig)
			{
				mCanHide = false;
			}
		}
	}

	public void HideHighlightImage()
	{
		mCanHide = true;
	}

	public void LateUpdate()
	{
		if (mCanHide)
		{
			mCanHide = false;
			mChapterImageBig.SetVisibility(inVisible: false);
		}
	}

	public void ShowDialog(string assetName, string dbName, string title, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString localeString)
	{
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.Destroy();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (!(mKAUIGenericDB == null))
		{
			mKAUIGenericDB.SetMessage(base.gameObject, yesMessage, noMessage, okMessage, closeMessage);
			mKAUIGenericDB.SetDestroyOnClick(destroyDB);
			mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mKAUIGenericDB.SetTextByID(localeString._ID, localeString._Text, interactive: false);
			mKAUIGenericDB.SetTitle(title);
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	public void ShowDialog(string assetName, string dbName, LocaleString title, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString localeString)
	{
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.Destroy();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (!(mKAUIGenericDB == null))
		{
			mKAUIGenericDB.SetMessage(base.gameObject, yesMessage, noMessage, okMessage, closeMessage);
			mKAUIGenericDB.SetDestroyOnClick(destroyDB);
			mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mKAUIGenericDB.SetTextByID(localeString._ID, localeString._Text, interactive: false);
			mKAUIGenericDB.SetTitle(title.GetLocalizedString());
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	public void ProcessClose()
	{
	}

	public bool IsBusy()
	{
		return mFieldGuideData == null;
	}

	public bool IsReadyToClose()
	{
		return true;
	}

	public void ActivateUI(int uiIndex, bool addToList)
	{
	}

	public void Exit()
	{
	}

	public void Clear()
	{
	}
}
