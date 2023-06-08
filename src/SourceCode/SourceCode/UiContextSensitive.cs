using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UiContextSensitive : KAUI
{
	public KAWidget _ButtonTemplate;

	public GameObject _MenuTemplate;

	private List<GameObject> mMenuList = new List<GameObject>();

	private List<UiContextSensetiveMenu> mUIContextSensitiveMenuList = new List<UiContextSensetiveMenu>();

	public bool pUseMenuForParentCategory;

	public bool pUseMenuForSubCategory;

	public bool pOverrideSubCategoryXOffset;

	private Camera mMainCamera;

	private List<KAWidget> mMenuGridWidgets = new List<KAWidget>();

	private GameObject mFollowingTarget;

	private List<ContextData> mCurrentContextDataList;

	public List<GameObject> pMenuList => mMenuList;

	public List<UiContextSensetiveMenu> pUIContextSensitiveMenuList => mUIContextSensitiveMenuList;

	public ObContextSensitive pContextSensitiveObj { get; set; }

	public ContextSensitiveStateType pCurrentStateType { get; set; }

	public Vector3 pOffsetPos { get; set; }

	public IContextSensitiveStyle pUIStyle { get; set; }

	public bool pIs3DUI { get; set; }

	public Camera pMainCamera
	{
		get
		{
			return mMainCamera;
		}
		set
		{
			mMainCamera = value;
		}
	}

	public List<KAWidget> pMenuGridWidgets => mMenuGridWidgets;

	protected override void Start()
	{
		base.Start();
		if (pContextSensitiveObj != null)
		{
			mFollowingTarget = pContextSensitiveObj._UIFollowingTarget;
		}
		else
		{
			UtDebug.LogError("pContextSensitiveObj is null for " + base.name);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		ContextWidgetUserData contextWidgetUserData = (ContextWidgetUserData)inWidget.GetUserData();
		if (contextWidgetUserData != null && contextWidgetUserData._Data != null)
		{
			if (contextWidgetUserData._Data.pChildrenDataList != null && contextWidgetUserData._Data.pChildrenDataList.Count > 0)
			{
				if (contextWidgetUserData._Data.pTarget != null)
				{
					contextWidgetUserData._Data.pTarget.SendMessage("OnContextParentAction", contextWidgetUserData._Data._Name);
				}
				if (!contextWidgetUserData._Data.pIsChildOpened)
				{
					List<ContextData> inDataList = ((contextWidgetUserData._Data.pParent == null) ? mCurrentContextDataList : contextWidgetUserData._Data.pParent.pChildrenDataList);
					DeselectItems(inDataList);
				}
				SetToggle(contextWidgetUserData._Data, !contextWidgetUserData._Data.pIsChildOpened, contextWidgetUserData._Data.pIsChildOpened);
				ClearMenuList();
				RefreshItemsList();
			}
			else if (contextWidgetUserData._Data.pTarget != null)
			{
				contextWidgetUserData._Data.pTarget.SendMessage("OnContextAction", contextWidgetUserData._Data._Name);
			}
			if (contextWidgetUserData._Data._DeactivateOnClick)
			{
				pContextSensitiveObj.CloseMenu();
			}
		}
		if (inWidget.name == "CloseBtn")
		{
			pContextSensitiveObj.CloseMenu();
		}
	}

	private void SetToggle(ContextData inData, bool isOpened, bool isRecurssively)
	{
		inData.pIsChildOpened = isOpened;
		if (!isRecurssively)
		{
			return;
		}
		foreach (ContextData pChildrenData in inData.pChildrenDataList)
		{
			SetToggle(pChildrenData, isOpened, isRecurssively: true);
		}
	}

	private void DeselectItems(List<ContextData> inDataList)
	{
		if (inDataList == null)
		{
			return;
		}
		foreach (ContextData inData in inDataList)
		{
			SetToggle(inData, isOpened: false, isRecurssively: true);
		}
	}

	public virtual void AddContextDataList(ContextData[] inDataList, bool enableRefreshItems)
	{
		if (inDataList == null)
		{
			return;
		}
		mCurrentContextDataList = new List<ContextData>();
		foreach (ContextData contextData in inDataList)
		{
			if (FUEManager.IsCSMEnabled(base.gameObject.name, contextData._Name))
			{
				mCurrentContextDataList.Add(contextData);
			}
		}
		if (mCurrentContextDataList.Count > 0 && enableRefreshItems)
		{
			RefreshItemsList();
		}
	}

	public virtual void AddContextDataIntoList(ContextData inData, bool enableRefreshItems)
	{
		if (mCurrentContextDataList.Find((ContextData x) => x._Name == inData._Name) == null)
		{
			mCurrentContextDataList.Add(inData);
			if (mCurrentContextDataList.Count > 0 && enableRefreshItems)
			{
				RefreshItemsList();
			}
		}
	}

	public virtual void RemoveContextDataFromList(ContextData inData, bool enableRefreshItems)
	{
		if (mCurrentContextDataList.Find((ContextData x) => x._Name == inData._Name) != null)
		{
			mCurrentContextDataList.Remove(inData);
			if (mCurrentContextDataList.Count > 0 && enableRefreshItems)
			{
				RefreshItemsList();
			}
		}
	}

	private void RefreshItemsList()
	{
		pUIStyle.UpdatePositionData(mCurrentContextDataList.ToArray());
		Vector3 localScale = Vector3.zero;
		Quaternion rotation = Quaternion.identity;
		if (pIs3DUI)
		{
			localScale = base.transform.localScale;
			rotation = base.transform.rotation;
			base.transform.localScale = Vector3.one;
			base.transform.rotation = Quaternion.identity;
		}
		ClearDisplayItems();
		UiContextSensetiveMenu inMenu = null;
		GameObject inMenuGO = null;
		if (null != _MenuTemplate && pUseMenuForParentCategory)
		{
			Vector2 inGridCellDimensions = mCurrentContextDataList[0]._2DScaleInPixels + pContextSensitiveObj._UIStyleData._WidgetOffsetInPixels;
			CreateMenu(base.name + "_Menu", base.transform, inGridCellDimensions, ref inMenuGO, ref inMenu);
			inMenuGO.transform.localPosition = Vector3.zero;
		}
		foreach (ContextData mCurrentContextData in mCurrentContextDataList)
		{
			if (!pUseMenuForParentCategory)
			{
				AddDisplayItem(mCurrentContextData, null, null);
			}
			else
			{
				AddDisplayItem(mCurrentContextData, inMenu, inMenuGO);
			}
		}
		if (pIs3DUI)
		{
			base.transform.localScale = localScale;
			base.transform.rotation = rotation;
		}
		KAWidget kAWidget = FindItem("CloseBtn");
		if (kAWidget != null)
		{
			kAWidget.SetPosition(pUIStyle.GetCloseButtonPosition().x, pUIStyle.GetCloseButtonPosition().y);
		}
		KAWidget kAWidget2 = FindItem("MenuBackground");
		if (kAWidget2 != null)
		{
			if (pContextSensitiveObj._MenuBackgroundVisibility)
			{
				Rect menuBackgroundRect = pUIStyle.GetMenuBackgroundRect();
				kAWidget2.SetPosition(menuBackgroundRect.x, menuBackgroundRect.y);
				if (kAWidget2.pBackground != null)
				{
					if (!string.IsNullOrEmpty(pContextSensitiveObj._MenuBackgroundSpriteName))
					{
						kAWidget2.SetSprite(pContextSensitiveObj._MenuBackgroundSpriteName);
					}
					kAWidget2.SetScale(new Vector3(menuBackgroundRect.width / (float)kAWidget2.pBackground.width, menuBackgroundRect.height / (float)kAWidget2.pBackground.width, kAWidget2.GetScale().z));
					kAWidget2.pBackground.color = pContextSensitiveObj._MenuBackgroundColor;
				}
			}
			else
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
		}
		if (!pIs3DUI)
		{
			base.transform.position = pOffsetPos;
		}
	}

	private void AddDisplayItem(ContextData inData, UiContextSensetiveMenu inMenu, GameObject inMenuGO)
	{
		KAWidget kAWidget = null;
		KAWidget kAWidget2 = null;
		if (!string.IsNullOrEmpty(inData._ItemTemplateName))
		{
			kAWidget2 = base.FindItem(inData._ItemTemplateName);
		}
		kAWidget = ((kAWidget2 != null) ? DuplicateWidget(kAWidget2) : DuplicateWidget(_ButtonTemplate));
		if (kAWidget != null)
		{
			kAWidget.name = inData._Name;
			string localizedString = inData._DisplayName.GetLocalizedString();
			kAWidget.SetText(localizedString);
			string text = "";
			if (inData._ToolTipText != null && !string.IsNullOrEmpty(inData._ToolTipText.GetLocalizedString()))
			{
				text = inData._ToolTipText.GetLocalizedString();
			}
			else if (inData._DisplayName != null)
			{
				text = inData._DisplayName.GetLocalizedString();
			}
			if (!string.IsNullOrEmpty(text))
			{
				kAWidget.SetToolTipText(text);
			}
			KAWidget kAWidget3 = kAWidget.FindChildItem("Label");
			if (kAWidget3 != null)
			{
				kAWidget3.SetText(inData._LabelText.GetLocalizedString());
			}
			KAWidget kAWidget4 = kAWidget.FindChildItem("BackgroundBtn");
			KAWidget kAWidget5 = ((kAWidget4 != null) ? kAWidget4 : kAWidget);
			kAWidget5.SetUserData(new ContextWidgetUserData(inData));
			if (inMenu == null)
			{
				kAWidget.gameObject.SetActive(value: true);
				kAWidget.transform.parent = _ButtonTemplate.transform.parent;
				kAWidget.transform.localPosition = Vector3.zero;
				kAWidget.SetPosition(inData.pPosition.x, inData.pPosition.y);
			}
			if (kAWidget5.pBackground != null)
			{
				ScaleWidget(kAWidget5, inData);
				kAWidget5.pBackground.color = inData._BackgroundColor;
			}
			KAWidget kAWidget6 = kAWidget.FindChildItem("Icon");
			if (kAWidget6 != null)
			{
				SetItemSprite(kAWidget6, inData);
			}
			if (inMenu == null)
			{
				AddWidget(kAWidget);
			}
			else
			{
				inMenu.AddWidget(kAWidget);
			}
			if (inData.pUserItemData != null)
			{
				KAWidget kAWidget7 = kAWidget.FindChildItem("TxtQuantity");
				if (kAWidget7 != null)
				{
					kAWidget7.SetText($"{inData.pUserItemData.Quantity}");
					kAWidget7.SetVisibility(inVisible: true);
				}
				if (inData.pUserItemData.Item != null && inData.pUserItemData.Item.Attribute != null && inData.pUserItemData.Item.Attribute.Length != 0)
				{
					string text2 = SanctuaryData.FindSanctuaryPetTypeInfo(SanctuaryManager.pCurPetData.PetTypeID)._Name;
					string text3 = "Energy";
					string text4 = "Happiness";
					string attribute = text2 + text3;
					string attribute2 = text2 + text4;
					KAWidget kAWidget8 = kAWidget.FindChildItem("TxtHealth");
					KAWidget kAWidget9 = kAWidget.FindChildItem("TxtHappiness");
					if (kAWidget8 != null)
					{
						if (inData.pUserItemData.Item.HasAttribute(attribute))
						{
							kAWidget8.SetText($"{inData.pUserItemData.Item.GetAttribute(attribute, 0)}");
						}
						else if (inData.pUserItemData.Item.HasAttribute(text3))
						{
							kAWidget8.SetText($"{inData.pUserItemData.Item.GetAttribute(text3, 0)}");
						}
					}
					if (kAWidget9 != null)
					{
						if (inData.pUserItemData.Item.HasAttribute(attribute2))
						{
							kAWidget9.SetText($"{inData.pUserItemData.Item.GetAttribute(attribute2, 0)}");
						}
						else if (inData.pUserItemData.Item.HasAttribute(text4))
						{
							kAWidget9.SetText($"{inData.pUserItemData.Item.GetAttribute(text4, 0)}");
						}
					}
				}
			}
			kAWidget.SetVisibility(inVisible: true);
			mMenuGridWidgets.Add(kAWidget);
			kAWidget.SetInteractive(inData.pEnabled);
		}
		if (!inData.pIsChildOpened || inData.pChildrenDataList == null || inData.pChildrenDataList.Count <= 0)
		{
			return;
		}
		List<ContextData> list = new List<ContextData>(inData.pChildrenDataList);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (!FUEManager.IsCSMEnabled(base.gameObject.name, list[num]._Name))
			{
				list.Remove(list[num]);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		UiContextSensetiveMenu inMenu2 = null;
		GameObject inMenuGO2 = null;
		if (null != _MenuTemplate && pUseMenuForSubCategory)
		{
			Transform inParent = ((inMenuGO != null) ? inMenuGO.transform : base.transform);
			Vector2 _2DScaleInPixels = list[0]._2DScaleInPixels;
			CreateMenu(inData._Name, inParent, _2DScaleInPixels, ref inMenuGO2, ref inMenu2);
		}
		foreach (ContextData item in list)
		{
			if (!pUseMenuForSubCategory)
			{
				AddDisplayItem(item, null, null);
			}
			else
			{
				AddDisplayItem(item, inMenu2, inMenuGO2);
			}
		}
		if (!(_MenuTemplate == null) && pUseMenuForSubCategory && !(inMenu2 == null))
		{
			Vector3 zero = Vector3.zero;
			zero.x = kAWidget.GetPosition().x;
			if (!pOverrideSubCategoryXOffset)
			{
				zero.x -= inMenu2.transform.localPosition.x;
				zero.x -= GetRealMenuWidth(inMenu2) / 2f;
				zero.x += inMenu2._DefaultGrid.cellWidth / 2f;
			}
			zero.y = list[0].pPosition.y;
			zero.z = inData.pPosition.z;
			inMenuGO2.transform.localPosition = zero;
		}
	}

	private void CreateMenu(string inName, Transform inParent, Vector2 inGridCellDimensions, ref GameObject inMenuGO, ref UiContextSensetiveMenu inMenu)
	{
		inMenuGO = Object.Instantiate(_MenuTemplate, inParent, worldPositionStays: true);
		inMenuGO.name = inName + "_scrollableMenuUI";
		inMenu = inMenuGO.GetComponentInChildren<UiContextSensetiveMenu>();
		inMenu.pContextSensitiveUI = this;
		inMenu._DefaultGrid.cellWidth = inGridCellDimensions.x;
		inMenu._DefaultGrid.cellHeight = inGridCellDimensions.y;
		mMenuList.Add(inMenuGO);
		mUIContextSensitiveMenuList.Add(inMenu);
	}

	private float GetRealMenuWidth(KAUIMenu inMenu)
	{
		UIPanel component = inMenu.GetComponent<UIPanel>();
		float num = (float)inMenu.GetItems().Count * inMenu._DefaultGrid.cellWidth;
		if (!(num < component.baseClipRegion.z))
		{
			return component.baseClipRegion.z;
		}
		return num;
	}

	protected virtual void SetItemSprite(KAWidget inItem, ContextData inContextData)
	{
		if (!(inItem == null) && inContextData != null && !string.IsNullOrEmpty(inContextData._IconSpriteName))
		{
			if (inContextData._IconSpriteName.Split('/').Length == 1)
			{
				inItem.SetSprite(inContextData._IconSpriteName);
				return;
			}
			CSMCoBundleUserData cSMCoBundleUserData = new CSMCoBundleUserData(inContextData, inContextData._IconSpriteName, null);
			inItem.SetUserData(cSMCoBundleUserData);
			cSMCoBundleUserData.LoadResource();
		}
	}

	private void IconTextureLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject)
	{
		if (inLoadEvent == RsResourceLoadEvent.COMPLETE || inLoadEvent == RsResourceLoadEvent.ERROR)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	public bool RefreshChildItems(string TopMenuName)
	{
		using (IEnumerator<ContextData> enumerator = mCurrentContextDataList.Where((ContextData parentData) => parentData._Name == TopMenuName).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				ContextData current = enumerator.Current;
				if (current.pChildrenDataList.Count <= 0)
				{
					return false;
				}
				current.pIsChildOpened = true;
			}
		}
		RefreshItemsList();
		return true;
	}

	public void DisableWidget(string name, bool disable)
	{
		using IEnumerator<KAWidget> enumerator = mMenuGridWidgets.Where((KAWidget widget) => widget.name == name).GetEnumerator();
		if (enumerator.MoveNext())
		{
			enumerator.Current.SetDisabled(disable);
		}
	}

	private void ScaleWidget(KAWidget inWidget, ContextData inData)
	{
		Vector3 scale = inWidget.GetScale();
		scale.x = ((inData._2DScaleInPixels.x == 0f) ? 1f : (inData._2DScaleInPixels.x / (float)inWidget.pBackground.width));
		scale.y = ((inData._2DScaleInPixels.y == 0f) ? 1f : (inData._2DScaleInPixels.y / (float)inWidget.pBackground.height));
		inWidget.SetScale(scale);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ClearContextData();
	}

	private void ClearContextData()
	{
		if (mCurrentContextDataList == null)
		{
			return;
		}
		foreach (ContextData mCurrentContextData in mCurrentContextDataList)
		{
			mCurrentContextData.pIsChildOpened = false;
		}
	}

	private void ClearDisplayItems()
	{
		List<KAWidget> list = new List<KAWidget>();
		foreach (KAWidget widget in mItemInfo)
		{
			if (mMenuGridWidgets.Find((KAWidget x) => x.name == widget.name) != null)
			{
				widget.transform.parent = null;
				Object.Destroy(widget.gameObject);
			}
			else
			{
				list.Add(widget);
			}
		}
		mMenuGridWidgets.Clear();
		mItemInfo.Clear();
		mItemInfo = list;
		ClearMenuList();
	}

	private void ClearMenuList()
	{
		if (!pUseMenuForSubCategory || mMenuList == null)
		{
			return;
		}
		foreach (GameObject mMenu in mMenuList)
		{
			Object.Destroy(mMenu);
		}
		mMenuList.Clear();
		mUIContextSensitiveMenuList?.Clear();
	}

	protected override void Update()
	{
		base.Update();
		if (!pIs3DUI)
		{
			return;
		}
		if (base.transform.parent == null)
		{
			base.transform.position = mFollowingTarget.transform.position + pOffsetPos;
		}
		else
		{
			base.transform.localPosition = pOffsetPos;
		}
		if (!(AvAvatar.pObject == null))
		{
			Camera main = mMainCamera;
			if (main == null)
			{
				main = Camera.main;
			}
			Vector3 forward = base.transform.position - main.transform.position;
			forward.Normalize();
			base.transform.rotation = Quaternion.LookRotation(forward);
			Vector3 eulerAngles = base.transform.eulerAngles;
			eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, 0f);
			base.transform.eulerAngles = eulerAngles;
		}
	}

	public void ShowClose(bool inShowClose)
	{
		KAWidget kAWidget = FindItem("CloseBtn");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inShowClose);
		}
	}

	public void SetText(string inWidgetName, string inText)
	{
		KAWidget kAWidget = FindItem(inWidgetName);
		if (kAWidget != null)
		{
			kAWidget.SetText(inText);
		}
	}
}
