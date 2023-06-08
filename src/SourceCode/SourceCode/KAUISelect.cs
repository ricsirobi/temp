using UnityEngine;

public class KAUISelect : KAUI
{
	public InventoryTab[] _InventoryTabList;

	public KAWidget _EmptyItemTemplate;

	[Header("Number of empty slots required to drop item")]
	public int _DefaultEmptySlotsCount;

	[Header("Number of extra empty slots required when slots are full")]
	public int _AdditionalEmptySlotsCount;

	public string _InventoryTabMenuName = "KAUISelectTabMenu";

	public string _InventoryMenuName = "KAUISelectMenu";

	public LocaleString _InventoryFullText = new LocaleString("[REVIEW] Sorry! Your inventory is full. Either empty any slot or purchase a new one if possible.");

	public Color _MaskColor = Color.clear;

	protected int mDefaultTabIndex = -1;

	protected KAUISelectTabMenu mKAUiSelectTabMenu;

	protected KAUISelectMenu mKAUiSelectMenu;

	protected KAWidget mCurrentTabItem;

	protected KAUIGenericDB mKAUIGenericDB;

	protected KAWidget mPreviousTabWidget;

	public SnSettings _DlgSettings;

	public AudioClip[] _HelpDlgs;

	public TextAsset _TutorialTextAsset;

	public string _LongIntro;

	public string _ShortIntro;

	public string _BackBtnName = "BtnBmBack";

	public string _HelpBtnName = "BtnBmHelp";

	public CoIdleManager mIdleManager;

	public bool _MemberCheck;

	public AudioClip _MemberVO;

	public LocaleString _NonMemberText = new LocaleString("You have to be a member to use this feature.");

	public AudioClip _QuickIntro;

	protected bool mCameraSwitched;

	protected KAUIMenu mSelectedMenu;

	protected int mCurrentHelpIdx;

	public int pDefaultTabIndex
	{
		get
		{
			return mDefaultTabIndex;
		}
		set
		{
			mDefaultTabIndex = value;
		}
	}

	public KAUISelectTabMenu pKAUiSelectTabMenu => mKAUiSelectTabMenu;

	public KAUISelectMenu pKAUiSelectMenu => mKAUiSelectMenu;

	protected override void Awake()
	{
		base.Awake();
		if (_MenuList != null && _MenuList.Length != 0)
		{
			mKAUiSelectMenu = (KAUISelectMenu)GetMenu(_InventoryMenuName);
			mKAUiSelectTabMenu = (KAUISelectTabMenu)GetMenu(_InventoryTabMenuName);
		}
	}

	protected override void Start()
	{
		Initialize();
		base.Start();
	}

	public virtual void Initialize()
	{
		mIdleManager = (CoIdleManager)GetComponent(typeof(CoIdleManager));
		if (_MemberCheck && !SubscriptionInfo.pIsMember)
		{
			SetVisibility(inVisible: false);
		}
		if (_DefaultEmptySlotsCount == 0)
		{
			SetTabMenu(mDefaultTabIndex);
			return;
		}
		for (int i = 0; i < _DefaultEmptySlotsCount; i++)
		{
			AddEmptySlot();
		}
	}

	public virtual void SetTabMenu(int inSelectedIndex)
	{
		if (_InventoryTabList == null || _InventoryTabList.Length == 0)
		{
			return;
		}
		if (mKAUiSelectTabMenu != null)
		{
			mKAUiSelectTabMenu.ClearItems();
		}
		KAWidget kAWidget = null;
		for (int i = 0; i < _InventoryTabList.Length; i++)
		{
			InventoryTab inventoryTab = _InventoryTabList[i];
			inventoryTab.UpdateFromInventorySetting();
			KAWidget kAWidget2 = mKAUiSelectTabMenu.AddWidget(inventoryTab._DisplayNameText._Text);
			if (kAWidget2 != null)
			{
				kAWidget2._TooltipInfo._Text = inventoryTab._RollOverText;
				KAWidget kAWidget3 = kAWidget2.FindChildItem("TxtCatName");
				if (kAWidget3 != null)
				{
					kAWidget3.SetTextByID(inventoryTab._DisplayNameText._ID, inventoryTab._DisplayNameText._Text);
				}
				KAWidget kAWidget4 = kAWidget2.FindChildItem("Icon");
				if (inventoryTab._IconTex != null && kAWidget4 != null)
				{
					kAWidget4.SetTexture(inventoryTab._IconTex);
				}
				if (inventoryTab._RollOverVO != null)
				{
					kAWidget2._HoverInfo._Clip._AudioClip = inventoryTab._RollOverVO;
				}
				kAWidget2.SetVisibility(inVisible: true);
				if (null != kAWidget4)
				{
					kAWidget4.SetVisibility(inVisible: true);
				}
				if (null != kAWidget3)
				{
					kAWidget3.SetVisibility(inVisible: true);
				}
				mKAUiSelectTabMenu.SetVisibility(inVisible: true);
				if (inSelectedIndex == -1 && kAWidget == null)
				{
					kAWidget = kAWidget2;
					inSelectedIndex = i;
				}
				else if (i == inSelectedIndex)
				{
					kAWidget = kAWidget2;
				}
			}
		}
		if (kAWidget != null && inSelectedIndex >= 0)
		{
			mKAUiSelectTabMenu.OnClick(kAWidget);
			mPreviousTabWidget = kAWidget;
			mPreviousTabWidget.SetInteractive(isInteractive: false);
			mPreviousTabWidget.GetComponent<KAToggleButton>().SetChecked(isChecked: true);
		}
	}

	public virtual void ChangeCategory(KAWidget item)
	{
		if (mKAUiSelectTabMenu != null)
		{
			InventoryTab[] inventoryTabList = _InventoryTabList;
			foreach (InventoryTab inventoryTab in inventoryTabList)
			{
				if (inventoryTab._DisplayNameText._Text == item.name)
				{
					mKAUiSelectTabMenu.pSelectedTab = inventoryTab;
					break;
				}
			}
		}
		item.SetInteractive(isInteractive: false);
		if (mPreviousTabWidget != null)
		{
			mPreviousTabWidget.SetInteractive(isInteractive: true);
		}
		mPreviousTabWidget = item;
		SetInventoryMenuItems(item, mKAUiSelectTabMenu.pSelectedTab);
	}

	public virtual void SetInventoryMenuItems(KAWidget inButton, InventoryTab tab)
	{
		mCurrentTabItem = inButton;
		if (tab.pTabData != null)
		{
			mKAUiSelectMenu.ChangeCategory(tab.pTabData._Categories, forceChange: true);
		}
	}

	public virtual void ShowAvailableEmptySlots(InventoryTab inTabData)
	{
		if (inTabData.pTabData == null)
		{
			return;
		}
		int num = ((inTabData.pTabData._MaxNumSlots == -1) ? (inTabData.mNumSlotOccupied + inTabData.mNumSlotUnlocked + 1) : inTabData.pTabData._MaxNumSlots);
		if (num == 0 || inTabData.mNumSlotOccupied >= num)
		{
			return;
		}
		for (int i = inTabData.mNumSlotOccupied; i < num; i++)
		{
			KAWidget kAWidget = AddEmptySlot();
			if (inTabData.pTabData._SlotItemID != 0 && kAWidget != null && i > inTabData.mNumSlotUnlocked - 1)
			{
				if (inTabData.pTabData._MaxNumSlots == -1)
				{
					((KAUISelectItemData)kAWidget.GetUserData()).ShowSlotLock(kAWidget, addIcon: true);
					break;
				}
				((KAUISelectItemData)kAWidget.GetUserData()).ShowSlotLock(kAWidget);
			}
		}
	}

	public virtual KAWidget AddEmptySlot()
	{
		if (_EmptyItemTemplate == null)
		{
			return null;
		}
		KAWidget kAWidget = DuplicateWidget(_EmptyItemTemplate);
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
			mKAUiSelectMenu.AddWidget(kAWidget);
			AddWidgetData(kAWidget, null);
		}
		return kAWidget;
	}

	public virtual void OnOpen()
	{
		PlayTutorial();
		if (AvAvatar.pObject != null)
		{
			AvAvatar.SetActive(inActive: false);
		}
		base.gameObject.SetActive(value: true);
		KAUISelectMenu defaultMenu = GetDefaultMenu();
		if (defaultMenu != null)
		{
			defaultMenu.mMainUI = this;
			defaultMenu.enabled = true;
			InitMenuItems(defaultMenu);
		}
		if (mIdleManager != null)
		{
			mIdleManager.StartIdles();
		}
	}

	protected virtual void OnItemPurchaseComplete()
	{
		RefreshSlotStatus();
	}

	private void RefreshSlotStatus()
	{
		if (mKAUiSelectTabMenu == null || mKAUiSelectTabMenu.pSelectedTab == null || mKAUiSelectTabMenu.pSelectedTab.pTabData == null || mKAUiSelectMenu.GetItemCount() == 0)
		{
			return;
		}
		InventorySetting.TabData pTabData = mKAUiSelectTabMenu.pSelectedTab.pTabData;
		int occupiedSlots = pTabData.GetOccupiedSlots();
		int totalSlots = pTabData.GetTotalSlots();
		int num = totalSlots - mKAUiSelectTabMenu.pSelectedTab.mNumSlotUnlocked;
		if (num == 0)
		{
			return;
		}
		mKAUiSelectTabMenu.pSelectedTab.mNumSlotUnlocked = totalSlots;
		if (occupiedSlots >= totalSlots)
		{
			return;
		}
		if (pTabData._MaxNumSlots == -1)
		{
			KAWidget inWidget = mKAUiSelectMenu.GetItems()[mKAUiSelectMenu.GetItemCount() - 1];
			for (int i = 0; i < num; i++)
			{
				AddEmptySlot();
			}
			mKAUiSelectMenu.MoveItemToBottom(inWidget);
			return;
		}
		foreach (KAWidget item in mKAUiSelectMenu.GetItems())
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
			if (num > 0 && kAUISelectItemData._SlotLocked)
			{
				kAUISelectItemData.ShowSlotLock(item, addIcon: false, locked: false);
				num--;
			}
		}
	}

	public void DestroyKAUIDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB);
			if (AvAvatar.pState == AvAvatarState.PAUSED)
			{
				AvAvatar.pState = AvAvatarState.IDLE;
			}
		}
	}

	public void CheckInventoryFull()
	{
		if (!(mKAUiSelectTabMenu == null) && mKAUiSelectTabMenu.pSelectedTab != null && !string.IsNullOrEmpty(_InventoryFullText._Text) && IsCategoryFull(mKAUiSelectTabMenu.pSelectedTab))
		{
			ShowKAUIDialog("PfKAUIGenericDBSm", "InventoryFullDB", "", "", "", "DestroyKAUIDB", destroyDB: true, _InventoryFullText);
		}
	}

	public virtual bool IsCategoryFull(InventoryTab tab)
	{
		if (tab.pTabData != null && tab.pTabData._MaxNumSlots != 0 && tab.mNumSlotOccupied >= tab.mNumSlotUnlocked)
		{
			return true;
		}
		return false;
	}

	protected void ShowKAUIDialog(string assetName, string dbName, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString message)
	{
		ShowKAUIDialog(assetName, dbName, yesMessage, noMessage, okMessage, closeMessage, destroyDB, message.GetLocalizedString());
	}

	protected void ShowKAUIDialog(string assetName, string dbName, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, string message)
	{
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.Destroy();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.SetMessage(base.gameObject, yesMessage, noMessage, okMessage, closeMessage);
			mKAUIGenericDB.SetDestroyOnClick(destroyDB);
			mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mKAUIGenericDB.SetText(message, interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
	}

	public virtual void PlayTutorial()
	{
		if (_TutorialTextAsset != null)
		{
			if (_LongIntro != null && _LongIntro.Length > 0)
			{
				if (!TutorialManager.StartTutorial(_TutorialTextAsset, _LongIntro, bMarkDone: true, 12u, null))
				{
					TutorialManager.StartTutorial(_TutorialTextAsset, _ShortIntro, bMarkDone: false, 12u, null);
				}
			}
			else if (_ShortIntro != null && _ShortIntro.Length > 0)
			{
				TutorialManager.StartTutorial(_TutorialTextAsset, _ShortIntro, bMarkDone: false, 12u, null);
			}
		}
		else if (_QuickIntro != null)
		{
			SnChannel.Play(_QuickIntro, "VO_Pool", inForce: true);
		}
	}

	public virtual void InitMenuItems(KAUIMenu inMenu)
	{
		if (!(mSelectedMenu == inMenu) && !(inMenu == null))
		{
			SelectMenu(inMenu);
			mSelectedMenu = inMenu;
			KAUISelectMenu obj = (KAUISelectMenu)mSelectedMenu;
			obj.mItemInitialized = false;
			obj.LoadCurrentData(this);
		}
	}

	public virtual void OnClose()
	{
		Input.ResetInputAxes();
		TutorialManager.StopTutorials();
		SnChannel.StopPool("VO_Pool");
		mCameraSwitched = false;
		if (AvAvatar.pObject != null)
		{
			AvAvatar.SetActive(inActive: true);
		}
		base.gameObject.SetActive(value: false);
		KAUISelectMenu kAUISelectMenu = (KAUISelectMenu)mSelectedMenu;
		if (kAUISelectMenu != null)
		{
			kAUISelectMenu.SaveSelection();
			kAUISelectMenu.UnSelectItem();
		}
		KAUISelectMenu defaultMenu = GetDefaultMenu();
		if (defaultMenu != null)
		{
			defaultMenu.enabled = false;
		}
	}

	protected override void Update()
	{
		if (!mCameraSwitched)
		{
			mCameraSwitched = true;
			if (_MemberCheck && !SubscriptionInfo.pIsMember)
			{
				ShowNonMemberDialog();
			}
			else
			{
				if (_MemberCheck)
				{
					SetVisibility(inVisible: true);
				}
				OnOpen();
			}
		}
		base.Update();
	}

	public virtual void ShowNonMemberDialog()
	{
		if (_MemberVO != null)
		{
			SnChannel.Play(_MemberVO, "VO_Pool", inForce: true, null);
		}
		SetVisibility(inVisible: false);
		if (AvAvatar.pObject != null)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
		}
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.Destroy();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "PfKAUIGenericDBSm");
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._CloseMessage = "OnNonMemberDlgClose";
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
			mKAUIGenericDB.SetTextByID(_NonMemberText._ID, _NonMemberText._Text, interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB, _MaskColor);
		}
	}

	public virtual void OnNonMemberDlgClose()
	{
		base.gameObject.SetActive(value: false);
		if (mKAUIGenericDB != null)
		{
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
		Input.ResetInputAxes();
		TutorialManager.StopTutorials();
		SnChannel.StopPool("VO_Pool");
		if (AvAvatar.pObject != null)
		{
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
		}
		mCameraSwitched = false;
	}

	public virtual void SelectItem(KAWidget inWidget)
	{
		if (inWidget != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
			if (kAUISelectItemData != null && kAUISelectItemData._SlotLocked)
			{
				mKAUiSelectMenu.SetSelectedItem(null);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget != null)
		{
			if (inWidget.name == _BackBtnName)
			{
				OnClose();
			}
			else if (inWidget.name == _HelpBtnName)
			{
				PlayHelpDlg();
			}
		}
		base.OnClick(inWidget);
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (_DefaultEmptySlotsCount > 0 && !inVisible)
		{
			mKAUiSelectMenu.ClearItems();
		}
	}

	public void CheckForAdditionalEmptySlots()
	{
		if (_DefaultEmptySlotsCount <= 0)
		{
			return;
		}
		int num = 0;
		foreach (KAWidget item in mKAUiSelectMenu.GetItems())
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
			if (kAUISelectItemData != null && kAUISelectItemData._ItemID != 0)
			{
				num++;
			}
		}
		if (num >= mKAUiSelectMenu.GetNumItems() && _AdditionalEmptySlotsCount > 0)
		{
			for (int i = 0; i < _AdditionalEmptySlotsCount; i++)
			{
				AddEmptySlot();
			}
			mKAUiSelectMenu.RepositionMenu();
		}
	}

	public void UpdateOccupiedSlots(int count)
	{
		InventoryTab inventoryTab = null;
		if (mKAUiSelectTabMenu != null && mKAUiSelectTabMenu.pSelectedTab != null)
		{
			inventoryTab = mKAUiSelectTabMenu.pSelectedTab;
		}
		if (inventoryTab != null)
		{
			inventoryTab.mNumSlotOccupied += count;
			if (inventoryTab.mNumSlotOccupied <= 0)
			{
				inventoryTab.mNumSlotOccupied = 0;
			}
		}
	}

	public virtual void PlayHelpDlg()
	{
		if (_HelpDlgs.Length != 0)
		{
			if (mCurrentHelpIdx >= _HelpDlgs.Length)
			{
				mCurrentHelpIdx = 0;
			}
			SnChannel.Play(_HelpDlgs[mCurrentHelpIdx], _DlgSettings, inForce: true);
			mCurrentHelpIdx++;
		}
		else if (mIdleManager != null)
		{
			mIdleManager.OnIdlePlay();
		}
	}

	public virtual KAUISelectMenu GetDefaultMenu()
	{
		return null;
	}

	public virtual void AddWidgetData(KAWidget inWidget, KAUISelectItemData widgetData)
	{
		if (widgetData != null)
		{
			inWidget.name = widgetData._Item.name;
			inWidget.SetUserData(widgetData);
			widgetData.LoadResource();
			return;
		}
		if (mKAUiSelectMenu != null && mKAUiSelectMenu.GetSelectedItem() == inWidget)
		{
			mKAUiSelectMenu.SetSelectedItem(null);
			mKAUiSelectMenu.OnHover(inWidget, inIsHover: false);
		}
		inWidget.SetInteractive(isInteractive: false);
		inWidget.SetTexture(null);
		inWidget.SetToolTipText("");
		KAUISelectItemData userData = new KAUISelectItemData(mKAUiSelectMenu, new ItemData(), 0, 0, InventoryTabType.ITEM);
		inWidget.SetUserData(userData);
		inWidget.name = "EmptySlot";
	}

	public virtual void ResetDragItem(KAWidget sourceWidget, KAWidget dragItem)
	{
		KAUISelectItemData widgetData = (KAUISelectItemData)dragItem.GetUserData();
		AddWidgetData(sourceWidget, widgetData);
	}
}
