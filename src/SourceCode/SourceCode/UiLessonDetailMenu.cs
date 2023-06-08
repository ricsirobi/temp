using System;
using UnityEngine;

public class UiLessonDetailMenu : KAUITreeListMenu
{
	public KAWidget _TextBreakTemplate;

	public KAWidget _LessonSubTitleTemplate;

	public KAWidget _VocabularyTemplate;

	public KAWidget _ExperimentTemplate;

	public KAWidget _GeneralInformationTemplate;

	public KAWidget _ImageTemplate;

	public LocaleString _VocabularyWordsText = new LocaleString("Vocabulary Words");

	public LocaleString _GeneralInformationText = new LocaleString("General Information");

	public LocaleString _ExperimentsText = new LocaleString("Experiments");

	private bool mIsDragProcessed;

	private void Populate()
	{
		ClearItems();
		mTreeGroupData.Clear();
		FieldGuideMenuGrid obj = (FieldGuideMenuGrid)mCurrentGrid;
		FieldGuideChapter pSelectedChapter = ((UiFieldGuide)_ParentUi).pSelectedChapter;
		obj.mChapter = null;
		PopulateChapter(pSelectedChapter);
		PopulateTreeList();
		obj.mChapter = pSelectedChapter;
		obj.Reposition();
		ResetScrollBar();
	}

	private void PopulateChapter(FieldGuideChapter chapter)
	{
		KAUITreeListItemData kAUITreeListItemData = null;
		KAUITreeListItemData kAUITreeListItemData2 = null;
		kAUITreeListItemData = new KAUITreeListItemData(null, "VocabWords", _VocabularyWordsText);
		mTreeGroupData.Add(kAUITreeListItemData);
		FieldGuideVocabulary[] vocabularies = chapter.Vocabularies;
		foreach (FieldGuideVocabulary fieldGuideVocabulary in vocabularies)
		{
			if (fieldGuideVocabulary.IsUnlocked())
			{
				kAUITreeListItemData = new KAUITreeListItemData(null, fieldGuideVocabulary.Data._Text + "_v", fieldGuideVocabulary.Data);
				mTreeGroupData.Add(kAUITreeListItemData);
			}
		}
		if (chapter.Images != null && chapter.Images.Length != 0)
		{
			bool flag = false;
			FieldGuideImage[] images = chapter.Images;
			foreach (FieldGuideImage fieldGuideImage in images)
			{
				if (fieldGuideImage.IsUnlocked())
				{
					if (!flag)
					{
						flag = true;
						kAUITreeListItemData2 = new KAUITreeListItemData(null, "tbi", new LocaleString(""));
						mTreeGroupData.Add(kAUITreeListItemData2);
					}
					KAUITreeListItemData item = new KAUITreeListItemData(null, fieldGuideImage.Data._Text + "_img", fieldGuideImage.Name);
					mTreeGroupData.Add(item);
				}
			}
		}
		kAUITreeListItemData2 = new KAUITreeListItemData(null, "tbg", new LocaleString(""));
		mTreeGroupData.Add(kAUITreeListItemData2);
		kAUITreeListItemData = new KAUITreeListItemData(null, "GenInfo", _GeneralInformationText);
		mTreeGroupData.Add(kAUITreeListItemData);
		if (chapter.GeneralInformation.IsUnlocked())
		{
			kAUITreeListItemData = new KAUITreeListItemData(null, chapter.GeneralInformation.Data._Text + "_g", chapter.GeneralInformation.Data);
			mTreeGroupData.Add(kAUITreeListItemData);
		}
		if (chapter.Experiments == null)
		{
			return;
		}
		kAUITreeListItemData = new KAUITreeListItemData(null, "Experiments", _ExperimentsText);
		mTreeGroupData.Add(kAUITreeListItemData);
		bool flag2 = false;
		FieldGuideItem[] experiments = chapter.Experiments;
		foreach (FieldGuideItem fieldGuideItem in experiments)
		{
			if (fieldGuideItem.IsUnlocked())
			{
				if (!flag2)
				{
					flag2 = true;
					kAUITreeListItemData2 = new KAUITreeListItemData(null, "tbe", new LocaleString(""));
					mTreeGroupData.Add(kAUITreeListItemData2);
				}
				kAUITreeListItemData = new KAUITreeListItemData(null, fieldGuideItem.Data._Text + "_e", fieldGuideItem.Data);
				mTreeGroupData.Add(kAUITreeListItemData);
			}
		}
	}

	protected override KAWidget GetTemplateItem(string itemName)
	{
		if (itemName.Equals("tbe") || itemName.Equals("tbg") || itemName.Equals("tbi"))
		{
			return _TextBreakTemplate;
		}
		if (itemName.Contains("_v"))
		{
			return _VocabularyTemplate;
		}
		if (itemName.Contains("_g"))
		{
			return _GeneralInformationTemplate;
		}
		if (itemName.Contains("_e"))
		{
			return _ExperimentTemplate;
		}
		if (itemName.Contains("_img"))
		{
			return _ImageTemplate;
		}
		if (itemName.Equals("VocabWords") || itemName.Equals("GenInfo") || itemName.Equals("Experiments"))
		{
			return _LessonSubTitleTemplate;
		}
		return _Template;
	}

	private void OnOK()
	{
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget == null)
		{
			return;
		}
		base.OnClick(inWidget);
		string[] separator = new string[1] { "_" };
		string[] array = inWidget.name.Split(separator, StringSplitOptions.None);
		if (array == null || array.Length != 2)
		{
			return;
		}
		if (array[1] == "v")
		{
			FieldGuideVocabulary[] vocabularies = ((UiFieldGuide)_ParentUi).pSelectedChapter.Vocabularies;
			foreach (FieldGuideVocabulary fieldGuideVocabulary in vocabularies)
			{
				if (fieldGuideVocabulary.Data._Text.Equals(array[0]))
				{
					((UiFieldGuide)_ParentUi).ShowDialog(((UiFieldGuide)_ParentUi)._DialogAssetName, ((UiFieldGuide)_ParentUi)._DialogAssetName, fieldGuideVocabulary.Data, string.Empty, string.Empty, "OnOK", string.Empty, destroyDB: true, fieldGuideVocabulary.Definition);
				}
			}
		}
		else
		{
			if (!(array[1] == "e"))
			{
				return;
			}
			FieldGuideItem[] experiments = ((UiFieldGuide)_ParentUi).pSelectedChapter.Experiments;
			foreach (FieldGuideItem fieldGuideItem in experiments)
			{
				if (fieldGuideItem.Data._Text.Equals(array[0]))
				{
					OpenQuestPage(fieldGuideItem);
				}
			}
		}
	}

	private void OpenQuestPage(FieldGuideItem experiment)
	{
		UiJournal componentInChildren = base.transform.root.gameObject.GetComponentInChildren<UiJournal>();
		if (componentInChildren != null)
		{
			componentInChildren.ActivateUI(1);
			if (experiment.Type == 1)
			{
				Mission mission = MissionManager.pInstance.GetMission(experiment.UnlockID);
				(componentInChildren._MainCategoriesList[1]._CategoryScreen as UiJournalQuest).ShowQuestDetails(mission);
			}
			else
			{
				Task task = MissionManager.pInstance.GetTask(experiment.UnlockID);
				(componentInChildren._MainCategoriesList[1]._CategoryScreen as UiJournalQuest).ShowQuestDetails(task);
			}
		}
	}

	public override void OnHover(KAWidget inWidget, bool inIsHover)
	{
		if (inWidget == null)
		{
			return;
		}
		base.OnHover(inWidget, inIsHover);
		if (inIsHover)
		{
			if (inWidget.name.Contains("_img"))
			{
				string[] separator = new string[2] { "/", "_" };
				string[] array = inWidget.name.Split(separator, StringSplitOptions.None);
				((UiFieldGuide)_ParentUi).HighlightImage(array[0] + "_" + array[1] + "/" + array[2], array[3]);
			}
		}
		else
		{
			((UiFieldGuide)_ParentUi).HideHighlightImage();
		}
	}

	public override void OnPress(KAWidget inWidget, bool inPressed)
	{
		if (UtPlatform.IsMobile())
		{
			mDragPanel.Press(inPressed);
		}
		mIsDragProcessed = false;
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		if (!mIsDragProcessed && UtPlatform.IsMobile())
		{
			mDragPanel.Drag();
		}
	}

	public override void OnDrop(KAWidget inDroppedWidget, KAWidget inTargetWidget)
	{
		mIsDragProcessed = false;
	}

	public void MoveToNextChapter()
	{
		((UiFieldGuide)_ParentUi).MoveToNextChapter();
	}

	public void MoveToPreviousChapter()
	{
		((UiFieldGuide)_ParentUi).MoveToPreviousChapter();
	}
}
