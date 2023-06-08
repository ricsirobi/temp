using System;
using UnityEngine;

public class UiStandardDataMenu : KAUIMenu
{
	[Serializable]
	public class GenericDBSizes
	{
		public string _Name;

		public int _Count;
	}

	public KAWidget _BtnStandardInfoTemplate;

	public KAWidget _DCIDataTemplate;

	public float _TxtDataYOffset = 10f;

	public float _StandardBtnYOffset = 2f;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	private StandardGuideData[] mGuideData;

	private int mCurrentGuideIdx = -1;

	private KAUIGenericDB mKAUIGenericDB;

	public GenericDBSizes[] _GenericDBSizes;

	public void SetupData(StandardGuideData[] guideData)
	{
		mGuideData = guideData;
	}

	public void LoadGuideData(string topicName, LocaleString lockedText, LocaleString lockedDBTitle)
	{
		for (int i = 0; i < mGuideData.Length; i++)
		{
			if (mGuideData[i].Topic.GetLocalizedString().Equals(topicName))
			{
				LoadGuideData(i);
				break;
			}
		}
		if (GetNumItems() == 0)
		{
			ShowDBMessage(lockedText.GetLocalizedString(), lockedDBTitle.GetLocalizedString());
		}
	}

	public void LoadGuideData(int idx = 0)
	{
		if (mGuideData.Length > idx && mCurrentGuideIdx != idx)
		{
			mCurrentGuideIdx = idx;
			ClearItems();
			StandardGuideMenuGrid standardGuideMenuGrid = (StandardGuideMenuGrid)_DefaultGrid;
			StandardQuestData[] questDatas = mGuideData[idx].QuestDatas;
			foreach (StandardQuestData standardQuestData in questDatas)
			{
				KAWidget kAWidget = AddWidget("TemplateStandardData", null);
				KAWidget kAWidget2 = kAWidget.FindChildItem("TxtLevel");
				KAWidget kAWidget3 = kAWidget.FindChildItem("TxtQuestName");
				KAWidget kAWidget4 = kAWidget.FindChildItem("TxtDCITitle");
				KAWidget kAWidget5 = kAWidget.FindChildItem("DCIDataGroup");
				kAWidget.SetVisibility(inVisible: true);
				kAWidget2.SetText(standardQuestData.PlayerLevel.ToString());
				kAWidget3.SetText(standardQuestData.Name.GetLocalizedString());
				DisciplinaryData[] disciplinaryDatas = standardQuestData.DisciplinaryDatas;
				foreach (DisciplinaryData disciplinaryData in disciplinaryDatas)
				{
					kAWidget4.SetText(disciplinaryData.Title.GetLocalizedString());
					DisciplinaryCoreDetail[] details = disciplinaryData.Details;
					foreach (DisciplinaryCoreDetail disciplinaryCoreDetail in details)
					{
						KAWidget kAWidget6 = CreateWidgetFromTemplate(_DCIDataTemplate, kAWidget5);
						kAWidget6.SetText(disciplinaryCoreDetail.Detail.GetLocalizedString());
						KAWidget kAWidget7 = kAWidget6.FindChildItem("StandardGroup");
						DisciplinaryCoreDetail.StandardData[] standardDatas = disciplinaryCoreDetail.StandardDatas;
						foreach (DisciplinaryCoreDetail.StandardData standard in standardDatas)
						{
							CreateStandardInfoWidget(disciplinaryData.Title.GetLocalizedString(), standard, kAWidget7);
						}
						standardGuideMenuGrid.ArrangeChildWidgets(kAWidget7, _StandardBtnYOffset);
						standardGuideMenuGrid.CenterAlignWithParent(kAWidget6, kAWidget7);
					}
					standardGuideMenuGrid.ArrangeChildWidgets(kAWidget5, _TxtDataYOffset);
				}
			}
			standardGuideMenuGrid.Reposition();
		}
		((UiStandardGuide)_ParentUi).HideTable(GetNumItems() == 0);
	}

	public void CreateStandardInfoWidget(string title, DisciplinaryCoreDetail.StandardData standard, KAWidget parentItem)
	{
		KAWidget kAWidget = CreateWidgetFromTemplate(_BtnStandardInfoTemplate, parentItem);
		kAWidget.name = standard.ID;
		kAWidget.SetText(standard.ID);
		kAWidget.SetUserData(new StandardInfoData(title, standard.Description.GetLocalizedString()));
	}

	public KAWidget CreateWidgetFromTemplate(KAWidget template, KAWidget parentItem)
	{
		KAWidget kAWidget = DuplicateWidget(template);
		kAWidget.SetVisibility(inVisible: true);
		parentItem.AddChild(kAWidget);
		return kAWidget;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		StandardInfoData standardInfoData = (StandardInfoData)inWidget.GetUserData();
		if (standardInfoData != null)
		{
			ShowDBMessage(standardInfoData._Description, standardInfoData._Title);
		}
	}

	private string GetDBName(int length)
	{
		string result = "PfKAUIGenericDBStandards";
		int num = -1;
		GenericDBSizes[] genericDBSizes = _GenericDBSizes;
		foreach (GenericDBSizes genericDBSizes2 in genericDBSizes)
		{
			if (length >= genericDBSizes2._Count && genericDBSizes2._Count > num)
			{
				result = genericDBSizes2._Name;
				num = genericDBSizes2._Count;
			}
		}
		return result;
	}

	private void ShowDBMessage(string text, string title)
	{
		string dBName = GetDBName(text.Length);
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(dBName, "Message");
		mKAUIGenericDB.SetText(text, interactive: false);
		mKAUIGenericDB.SetTitle(title);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "KillGenericDB";
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}
}
