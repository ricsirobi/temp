public class UiStandardGuide : KAUI, IJournal
{
	public string _StandardGuideDataFileName = "RS_DATA/StandardGuideData.xml";

	public UiFieldGuide _UiFieldGuide;

	public KAUIMenu _TopicMenu;

	public UiStandardDataMenu _DataMenu;

	public LocaleString _LockedDBTitle = new LocaleString("Standard Guide");

	public LocaleString _LockedText = new LocaleString("You have not unlocked this content yet.");

	public LocaleString _TxtLevel;

	public LocaleString _TxtQuestName;

	public LocaleString _TxtDCITitle;

	public LocaleString _TxtDCIDescription;

	public LocaleString _TxtStandard;

	private StandardGuideData[] mStardardGuideData;

	private KAWidget mBtnFieldGuide;

	private KAWidget mTableWidget;

	protected override void Start()
	{
		base.Start();
		mBtnFieldGuide = FindItem("BtnFieldGuidePage");
		mTableWidget = FindItem("Table");
		KAWidget kAWidget = mTableWidget.FindChildItem("TxtLevel");
		KAWidget kAWidget2 = mTableWidget.FindChildItem("TxtQuestName");
		KAWidget kAWidget3 = mTableWidget.FindChildItem("TxtDCITitle");
		KAWidget kAWidget4 = mTableWidget.FindChildItem("TxtDCIDescription");
		KAWidget kAWidget5 = mTableWidget.FindChildItem("TxtStandard");
		kAWidget.SetText(_TxtLevel.GetLocalizedString());
		kAWidget2.SetText(_TxtQuestName.GetLocalizedString());
		kAWidget3.SetText(_TxtDCITitle.GetLocalizedString());
		kAWidget4.SetText(_TxtDCIDescription.GetLocalizedString());
		kAWidget5.SetText(_TxtStandard.GetLocalizedString());
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible && mStardardGuideData == null)
		{
			LoadStandardGuideFromXML();
		}
	}

	public void LoadStandardGuideFromXML()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.Load(_StandardGuideDataFileName, XmlLoadEventHandler);
	}

	private void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		default:
			return;
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				mStardardGuideData = UtUtilities.DeserializeFromXml((string)inObject, typeof(StandardGuideData[])) as StandardGuideData[];
				if (mStardardGuideData != null)
				{
					LoadMenuData();
					KAUICursorManager.SetDefaultCursor("Arrow");
					return;
				}
			}
			break;
		case RsResourceLoadEvent.ERROR:
			break;
		}
		UtDebug.Log("Not able to download standard guide from : " + _StandardGuideDataFileName);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mBtnFieldGuide == inWidget)
		{
			SetVisibility(inVisible: false);
			_UiFieldGuide.SetVisibility(inVisible: true);
		}
		else
		{
			_DataMenu.LoadGuideData(inWidget.name, _LockedText, _LockedDBTitle);
		}
	}

	private void LoadMenuData()
	{
		if (_DataMenu != null)
		{
			_DataMenu.SetupData(mStardardGuideData);
		}
		if (!(_TopicMenu != null))
		{
			return;
		}
		bool flag = false;
		StandardGuideData[] array = mStardardGuideData;
		foreach (StandardGuideData standardGuideData in array)
		{
			KAWidget kAWidget = _TopicMenu.AddWidget("BtnStandardTopicTemplate");
			kAWidget.SetText(standardGuideData.Topic.GetLocalizedString());
			kAWidget.name = standardGuideData.Topic.GetLocalizedString();
			kAWidget.SetVisibility(inVisible: true);
			if (!flag)
			{
				_TopicMenu.OnClick(kAWidget);
				flag = true;
			}
		}
	}

	public void HideTable(bool hide)
	{
		mTableWidget.SetVisibility(!hide);
	}

	public void ProcessClose()
	{
	}

	public bool IsBusy()
	{
		if (GetVisibility())
		{
			return mStardardGuideData == null;
		}
		return false;
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
