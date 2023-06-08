using System;
using UnityEngine;

public class UiTopicMenu : KAUITreeListMenu
{
	public KAWidget _TopicTemplate;

	public float _SubTopicItemOffset = -15f;

	public float _LessonItemOffset = -10f;

	public float _ChapterItemOffset = -10f;

	private float mNewScrollValue;

	private string mClickedWidget = "";

	protected override void Start()
	{
		base.Start();
		SetVisibility(inVisible: false);
	}

	public void Populate()
	{
		((UiFieldGuide)_ParentUi).mUnlockedChapters.Clear();
		mTreeGroupData.Clear();
		FieldGuideSubject pSelectedSubject = ((UiFieldGuide)_ParentUi).pSelectedSubject;
		ClearItems();
		FieldGuideChapter fieldGuideChapter = null;
		if (UiFieldGuide.pRecentlyUnlockedChapters != null && UiFieldGuide.pRecentlyUnlockedChapters.Count > 0)
		{
			fieldGuideChapter = UiFieldGuide.pRecentlyUnlockedChapters[UiFieldGuide.pRecentlyUnlockedChapters.Count - 1];
		}
		UiFieldGuide.pRecentlyUnlockedChapters.Clear();
		FieldGuideTopic[] topics = pSelectedSubject.Topics;
		foreach (FieldGuideTopic fieldGuideTopic in topics)
		{
			KAUITreeListItemData kAUITreeListItemData = new KAUITreeListItemData(null, fieldGuideTopic.Name._Text + "_topic", fieldGuideTopic.Name);
			int num = 0;
			FieldGuideSubTopic[] subTopics = fieldGuideTopic.SubTopics;
			foreach (FieldGuideSubTopic fieldGuideSubTopic in subTopics)
			{
				int num2 = 0;
				KAUITreeListItemData kAUITreeListItemData2 = new KAUITreeListItemData(kAUITreeListItemData, fieldGuideSubTopic.Name._Text + "_subt", fieldGuideSubTopic.Name);
				FieldGuideLesson[] lessons = fieldGuideSubTopic.Lessons;
				foreach (FieldGuideLesson fieldGuideLesson in lessons)
				{
					if (!fieldGuideLesson.IsUnlocked())
					{
						continue;
					}
					KAUITreeListItemData kAUITreeListItemData3 = new KAUITreeListItemData(kAUITreeListItemData2, fieldGuideLesson.Name._Text + "_lesson", fieldGuideLesson.Name);
					FieldGuideChapter[] chapters = fieldGuideLesson.Chapters;
					foreach (FieldGuideChapter fieldGuideChapter2 in chapters)
					{
						if (fieldGuideChapter2.IsUnlocked())
						{
							KAUITreeListItemData inChild = new KAUITreeListItemData(kAUITreeListItemData3, fieldGuideChapter2.Title.Data._Text + "_chapter", fieldGuideChapter2.Title.Data);
							kAUITreeListItemData3.AddChild(inChild);
							((UiFieldGuide)_ParentUi).mUnlockedChapters.Add(fieldGuideChapter2);
						}
					}
					kAUITreeListItemData2.AddChild(kAUITreeListItemData3);
					if (num == 0 && num2 == 0)
					{
						SetDefaultSelectedWidget(fieldGuideLesson.Name._Text);
					}
					num2++;
				}
				kAUITreeListItemData.AddChild(kAUITreeListItemData2);
				num++;
			}
			mTreeGroupData.Add(kAUITreeListItemData);
		}
		PopulateTreeList();
		if (fieldGuideChapter != null)
		{
			OpenChapter(fieldGuideChapter.Title.Data._Text);
		}
		else if (!string.IsNullOrEmpty(UiFieldGuide.mLastSelectedChapterName))
		{
			OpenChapter(UiFieldGuide.mLastSelectedChapterName);
		}
	}

	protected override void IndentTreeNode(KAUITreeListItemData inItemData, float inParentIndent)
	{
		KAWidget item = inItemData._Item;
		if (item != null)
		{
			if (item.name.Contains("_subt"))
			{
				item.SetPosition(inParentIndent + _SubTopicItemOffset, item.GetPosition().y);
			}
			else if (item.name.Contains("_lesson"))
			{
				item.SetPosition(inParentIndent + _LessonItemOffset, item.GetPosition().y);
			}
			else if (item.name.Contains("_chapter"))
			{
				item.SetPosition(inParentIndent + _ChapterItemOffset, item.GetPosition().y);
			}
		}
		foreach (KAUITreeListItemData child in inItemData._ChildList)
		{
			IndentTreeNode(child, inParentIndent + _GroupChildItemOffsetX);
		}
	}

	private void ScrollToSelectedItem()
	{
		float num = GetSelectedItemIndex();
		float num2 = mItemInfo.Count;
		mVerticalScrollbar.pScrollbar.value = num / num2;
	}

	public void OpenChapter(string chapterName)
	{
		((UiFieldGuide)_ParentUi).CheckForChapter(chapterName);
		SetDefaultSelectedWidget(chapterName + "_chapter");
		ScrollToSelectedItem();
	}

	private void UpdateSprites(string itemName)
	{
		string[] separator = new string[1] { "_" };
		string[] array = itemName.Split(separator, StringSplitOptions.None);
		if (array == null || array.Length != 2)
		{
			return;
		}
		FieldGuideTopic[] topics = ((UiFieldGuide)_ParentUi).pSelectedSubject.Topics;
		foreach (FieldGuideTopic fieldGuideTopic in topics)
		{
			if (fieldGuideTopic.Name._Text.Equals(array[0]))
			{
				KAWidget kAWidget = _TopicTemplate.FindChildItem("Icon");
				if (kAWidget != null)
				{
					kAWidget.pBackground.UpdateSprite(fieldGuideTopic.IconName);
					break;
				}
			}
		}
	}

	protected override KAWidget GetTemplateItem(string itemName)
	{
		if (itemName.Contains("_topic"))
		{
			UpdateSprites(itemName);
			return _TopicTemplate;
		}
		return _Template;
	}

	private FieldGuideSubTopic GetSubtopic(string subtopicName)
	{
		FieldGuideTopic[] topics = ((UiFieldGuide)_ParentUi).pSelectedSubject.Topics;
		for (int i = 0; i < topics.Length; i++)
		{
			FieldGuideSubTopic[] subTopics = topics[i].SubTopics;
			foreach (FieldGuideSubTopic fieldGuideSubTopic in subTopics)
			{
				if (fieldGuideSubTopic.Name._Text.Equals(subtopicName))
				{
					return fieldGuideSubTopic;
				}
			}
		}
		return null;
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget == null)
		{
			return;
		}
		if (inWidget.name.Equals(_ItemExpandTemplateName) && mVerticalScrollbar != null)
		{
			mNewScrollValue = mVerticalScrollbar.pScrollbar.value;
		}
		base.OnClick(inWidget);
		if (inWidget.name.Contains("_topic") || inWidget.name.Contains("_lesson") || inWidget.name.Contains("_subt"))
		{
			mClickedWidget = inWidget.name;
			if (inWidget.name.Contains("_subt"))
			{
				string[] separator = new string[1] { "_" };
				string[] array = inWidget.name.Split(separator, StringSplitOptions.None);
				if (array != null && array.Length == 2)
				{
					FieldGuideSubTopic subtopic = GetSubtopic(array[0]);
					if (subtopic != null && !subtopic.IsUnlocked())
					{
						((UiFieldGuide)_ParentUi).ShowDialog(((UiFieldGuide)_ParentUi)._DialogAssetName, ((UiFieldGuide)_ParentUi)._DialogAssetName, ((UiFieldGuide)_ParentUi)._FieldGuideText, string.Empty, string.Empty, "OnOK", string.Empty, destroyDB: true, ((UiFieldGuide)_ParentUi)._SubtopicLockedText);
					}
				}
			}
			if (mVerticalScrollbar != null)
			{
				mNewScrollValue = mVerticalScrollbar.pScrollbar.value;
			}
			ExpandCollapseList(inWidget);
		}
		else if (inWidget.name.Contains("_chapter"))
		{
			string[] separator2 = new string[1] { "_" };
			string[] array2 = inWidget.name.Split(separator2, StringSplitOptions.None);
			if (array2 != null && array2.Length == 2)
			{
				((UiFieldGuide)_ParentUi).CheckForChapter(array2[0]);
			}
		}
	}

	public override void ExpandCollapseList(KAWidget parentItem)
	{
		base.ExpandCollapseList(parentItem);
		KAUITreeListItemData kAUITreeListItemData = FindFirstItem(mTreeGroupData, parentItem.name);
		if (kAUITreeListItemData != null && !string.IsNullOrEmpty(mClickedWidget))
		{
			KAWidget kAWidget = FindItem(mClickedWidget);
			if (kAWidget != null)
			{
				KAToggleButton component = kAWidget.GetComponent<KAToggleButton>();
				if (component != null)
				{
					component.SetChecked(!kAUITreeListItemData._Collapsed);
				}
			}
			mClickedWidget = null;
		}
		if (mVerticalScrollbar != null)
		{
			mVerticalScrollbar.pScrollbar.value = mNewScrollValue;
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
	}
}
