using System.Collections.Generic;
using UnityEngine;

public class UiWorkBook : KAUI
{
	public KAUISubjectTreeGroupData[] _SubjectTreeGroupDataList;

	public string _TopicHeaderTxtName = "TopicDataHeaderTxt";

	public string _TopicQuestTxtName = "TopicDataQuestTxt";

	public string _TreeQuestKey = "Quest";

	public string _TreeItemURLKey = "ImageURL";

	public string _DefaultButtonSelected = "Math";

	private KAUITreeListMenu mTreeListMenu;

	private GameObject mCurrentSubPrefab;

	private GameObject mTopicObj;

	private KAWidget mLeftScrollBtn;

	private KAWidget mRightScrollBtn;

	private int mCurrentPage;

	private int mNumPages;

	protected override void Start()
	{
		base.Start();
		mTreeListMenu = GetComponentInChildren<KAUITreeListMenu>();
		mTreeListMenu.pTreeEvents.OnClick += ProcessTreeItemClickEvent;
		((UiWorkBookMenu)GetMenu("UiWorkBookMenu")).pEvents.OnClick += OnSubjectButtonClicked;
		mLeftScrollBtn = FindItem("BtnAmScrollLt");
		mRightScrollBtn = FindItem("BtnAmScrollRt");
	}

	private List<WorkbookUserData> MakeTreeListData(KAUITreeDataGroup[] inTreeGroupData)
	{
		List<WorkbookUserData> list = new List<WorkbookUserData>();
		foreach (KAUITreeDataGroup kAUITreeDataGroup in inTreeGroupData)
		{
			WorkbookUserData workbookUserData = new WorkbookUserData(null, kAUITreeDataGroup._Name, kAUITreeDataGroup._DisplayText, kAUITreeDataGroup._Collapsed, null, "", null);
			foreach (KAUITreeDataGroupChild child in kAUITreeDataGroup._ChildList)
			{
				List<WorkbookUserData.WorkbookMapData> list2 = new List<WorkbookUserData.WorkbookMapData>();
				foreach (TreeMapData data in child._DataList)
				{
					list2.Add(new WorkbookUserData.WorkbookMapData(data._Key, data._Text));
				}
				workbookUserData.AddChild(new WorkbookUserData(workbookUserData, child._Name, child._DisplayText, inCollapsed: true, null, child._URL, list2));
			}
			list.Add(workbookUserData);
		}
		return list;
	}

	private void OnSubjectButtonClicked(KAWidget inWidget)
	{
		KAUISubjectTreeGroupData[] subjectTreeGroupDataList = _SubjectTreeGroupDataList;
		foreach (KAUISubjectTreeGroupData kAUISubjectTreeGroupData in subjectTreeGroupDataList)
		{
			if (!(kAUISubjectTreeGroupData._SubjectName == inWidget.name))
			{
				continue;
			}
			mTreeListMenu.ClearData();
			foreach (WorkbookUserData item in MakeTreeListData(kAUISubjectTreeGroupData._TreeGroupData))
			{
				mTreeListMenu.AddItem("", item, inRefreshTree: false);
			}
			RsResourceManager.LoadAssetFromBundle("RS_DATA/" + kAUISubjectTreeGroupData._PrefabURL, kAUISubjectTreeGroupData._PrefabURL, OnPrefabLoaded, typeof(GameObject));
			KAUICursorManager.SetDefaultCursor("Loading");
			break;
		}
	}

	public void OnPrefabLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			mCurrentSubPrefab = (GameObject)inObject;
			mTreeListMenu.PopulateTreeList();
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	public void ProcessTreeItemClickEvent(KAWidget inWidget, object inObject)
	{
		if (!(inWidget.GetUserData() is WorkbookUserData workbookUserData))
		{
			return;
		}
		string text = "";
		string text2 = "";
		if (workbookUserData != null)
		{
			WorkbookUserData workbookUserData2 = workbookUserData;
			if (workbookUserData2 != null)
			{
				text = workbookUserData2._DisplayText.GetLocalizedString();
				LocaleString value = workbookUserData2.GetValue(_TreeQuestKey);
				if (value != null)
				{
					text2 = value._Text;
				}
				Object.Destroy(mTopicObj);
				if (!string.IsNullOrEmpty(workbookUserData2._URL))
				{
					Transform transform = mCurrentSubPrefab.transform.Find(workbookUserData2._URL);
					mTopicObj = Object.Instantiate(transform.gameObject);
					mTopicObj.transform.position = new Vector3(mTopicObj.transform.position.x, mTopicObj.transform.position.y, -1f);
					mNumPages = mTopicObj.transform.childCount;
					mCurrentPage = 0;
					SelectPage(mCurrentPage);
				}
			}
		}
		else
		{
			Object.Destroy(mTopicObj);
		}
		KAWidget kAWidget = FindItem(_TopicHeaderTxtName);
		if (kAWidget != null)
		{
			kAWidget.SetText(text);
		}
		KAWidget kAWidget2 = FindItem(_TopicQuestTxtName);
		if (kAWidget2 != null)
		{
			kAWidget2.SetText(text2);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		mTreeListMenu.OnClick(inWidget);
		if (inWidget.name == "Close")
		{
			OnClose();
		}
		else if (mLeftScrollBtn != null && inWidget.name == "BtnAmScrollLt")
		{
			MovePrevious();
		}
		else if (mRightScrollBtn != null && inWidget.name == "BtnAmScrollRt")
		{
			MoveNext();
		}
	}

	private void OnClose()
	{
		SetVisibility(inVisible: false);
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			mCurrentPage = 0;
			UiWorkBookMenu uiWorkBookMenu = (UiWorkBookMenu)GetMenu("UiWorkBookMenu");
			OnSubjectButtonClicked(uiWorkBookMenu.FindItem(_DefaultButtonSelected));
		}
		else
		{
			Object.Destroy(mTopicObj);
		}
	}

	private void MoveNext()
	{
		mCurrentPage++;
		if (mCurrentPage >= mNumPages)
		{
			mCurrentPage = mNumPages - 1;
		}
		SelectPage(mCurrentPage);
	}

	private void MovePrevious()
	{
		mCurrentPage--;
		if (mCurrentPage < 0)
		{
			mCurrentPage = 0;
		}
		SelectPage(mCurrentPage);
	}

	private void SelectPage(int inPageNum)
	{
		if (mTopicObj == null)
		{
			return;
		}
		foreach (Transform item in mTopicObj.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		mTopicObj.transform.GetChild(inPageNum).gameObject.SetActive(value: true);
	}
}
