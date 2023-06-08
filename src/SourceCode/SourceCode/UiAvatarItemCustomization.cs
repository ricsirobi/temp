using System;
using System.Collections.Generic;
using UnityEngine;

public class UiAvatarItemCustomization : KAUI
{
	public class InitData
	{
		public KAUISelectItemData _SelectedItemData;

		public UserItemData[] _ItemDataList;

		public OnItemCustomizationClosed _Callback;

		public InitData(KAUISelectItemData selectedItemData, UserItemData[] itemDataList, OnItemCustomizationClosed callback)
		{
			_SelectedItemData = selectedItemData;
			_ItemDataList = itemDataList;
			_Callback = callback;
		}
	}

	private class TextureAssignData
	{
		public Material _Material;

		public string _ShaderPropName;
	}

	private class ThumbnailUserData
	{
		public KAWidget _ThumbnailWidget;

		public UserItemData _UserItemData;
	}

	[Serializable]
	public class CustomizationUiSetup
	{
		public string _ShaderPropName;

		public KAWidget _CustomizeWidget;
	}

	public delegate void OnCustomizationPairDataLoaded();

	public const string CustomizeTicketAttributeName = "CustTkt";

	public const string FreeCustomizeAttributeName = "FreeTkt";

	public GameObject _CameraObject;

	public Transform _ItemMarker;

	public KAWidget _LeftRotateBtn;

	public KAWidget _RightRotateBtn;

	public string _DragWidgetName = "DragWidget";

	public float _RotationSpeed = 40f;

	public Vector3 _CameraOffset = new Vector3(0f, 1.195f, 0f);

	public UiItemCustomizationMenu _Menu;

	public UiItemStats _UiItemStats;

	public UiItemElements _UiItemElement;

	public List<CustomizationUiSetup> _CustomizationUiSetup;

	public OnItemCustomizationClosed _OnItemCustomizationClosed;

	public LocaleString _ResetCustomizationText = new LocaleString("Do you want to reset the changes?");

	public LocaleString _PurchaseFailText = new LocaleString("[Review] Transaction failed. Please try again.");

	public LocaleString _PurchaseConfirmationText = new LocaleString("[Review] This customization will cost you {cost} Gems. Do you want to continue?");

	public LocaleString _PurchaseProcessingText = new LocaleString("[Review] Processing purchase...");

	public LocaleString _NotEnoughCurrencyText = new LocaleString("[Review] You don't have enough Gems. Do you want to buy more Gems?");

	public LocaleString _TicketLooseWarningText = new LocaleString("[Review] This will loose your current customization. Do you want to continue?");

	public LocaleString _MatchDragonColorTitleText = new LocaleString("[Review] Match Dragon Colors");

	public LocaleString _MatchDragonColorText = new LocaleString("[Review] Are you sure you want to match your flight suit color to your current dragon?");

	public LocaleString _MatchClanColorTitleText = new LocaleString("[Review] Match Clan Colors");

	public LocaleString _MatchClanColorText = new LocaleString("[Review] Are you sure you want to match your flight suit color to your current clan colors?");

	public const int _PairDataID = 2014;

	public KAUIGenericDB _UiSyncDragonColorConfirmationDB;

	public KAUIGenericDB _UiSyncClanColorConfirmationDB;

	private KAWidget mColorPalette;

	private KAWidget mColorSelector;

	private KAWidget mSelectedColorBtn;

	private KAWidget mOkBtn;

	private KAWidget mResetBtn;

	private KAWidget mSyncBtn;

	private KAWidget mSyncClanColorBtn;

	private KAWidget mCustomizeLockBtn;

	private GameObject mCurrentCustomizeObject;

	private Dictionary<int, List<ItemCustomizationData>> mCustomizationChanges = new Dictionary<int, List<ItemCustomizationData>>();

	private KAUISelectItemData mSelectedItemData;

	private KAUISelectItemData mLastSelectedItemData;

	private UserItemData[] mUserItemDataArray;

	private static Dictionary<string, InitData> mInitData = new Dictionary<string, InitData>();

	private static PairData mPairData = null;

	private static ItemCustomizationUiData mItemCustomizationUiData;

	private GameObject mCustomAvatarObj;

	private CustomAvatarState mCustomAvatarState;

	private AvatarData.InstanceInfo mInstanceInfo;

	private bool mHasCustomizationTicket;

	private UserItemData mCurrentCustomTicketItem;

	private ItemPurchase mItemPurchase;

	private KAUIGenericDB mKAUIGenericDB;

	private AvatarDataPart[] mCachedParts;

	private int mItemsToDownload;

	public static void LoadPairData(OnCustomizationPairDataLoaded inCallback = null)
	{
		if (mPairData == null)
		{
			PairData.Load(2014, PairDataHandler, inCallback);
		}
		else
		{
			inCallback?.Invoke();
		}
	}

	private static void PairDataHandler(bool success, PairData inData, object inUserData)
	{
		if (inData != null)
		{
			mPairData = inData;
			if (inUserData != null)
			{
				((OnCustomizationPairDataLoaded)inUserData)();
			}
		}
		else
		{
			UtDebug.LogError("PairData Loading Failed!!!!!!!");
		}
	}

	public static void ClearPairData()
	{
		mPairData = null;
	}

	private static ItemCustomizationData[] GetCustomizationDataV1(UserItemData userItemData)
	{
		ItemCustomizationData[] customizationConfig = ItemCustomizationSettings.pInstance.GetCustomizationConfig(userItemData.Item);
		if (customizationConfig == null)
		{
			return null;
		}
		List<ItemCustomizationData> list = new List<ItemCustomizationData>(customizationConfig);
		if (mPairData != null)
		{
			string text = userItemData.UserInventoryID.ToString();
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			string value = mPairData.GetValue(text);
			if (string.IsNullOrEmpty(value) || value.Equals("LIST_NOT_VALID") || value.Equals("___VALUE_NOT_FOUND___"))
			{
				return null;
			}
			string[] separator = new string[1] { "#" };
			string[] array = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 0)
			{
				return null;
			}
			ItemCustomizationData itemCustomizationData = list.Find((ItemCustomizationData x) => x._ShaderPropName == "_PrimaryColor");
			if (itemCustomizationData != null)
			{
				itemCustomizationData._Value = array[0];
			}
			ItemCustomizationData itemCustomizationData2 = list.Find((ItemCustomizationData x) => x._ShaderPropName == "_SecondaryColor");
			if (itemCustomizationData2 != null && array.Length > 1)
			{
				itemCustomizationData2._Value = array[1];
			}
		}
		return list.ToArray();
	}

	private static void SaveCustomizationsFromV1(UserItemData[] userItemDataArray)
	{
		foreach (UserItemData userItemData in userItemDataArray)
		{
			if (userItemData.UserItemAttributes != null)
			{
				continue;
			}
			ItemCustomizationData[] customizationDataV = GetCustomizationDataV1(userItemData);
			if (customizationDataV == null || customizationDataV.Length == 0)
			{
				continue;
			}
			PairData pairData = new PairData();
			pairData.Init();
			ItemCustomizationData[] array = customizationDataV;
			foreach (ItemCustomizationData itemCustomizationData in array)
			{
				if (!string.IsNullOrEmpty(itemCustomizationData._Value))
				{
					pairData.SetValue(itemCustomizationData._ShaderPropName, itemCustomizationData._Value);
				}
			}
			if (pairData.pPairList != null && pairData.pPairList.Count > 0)
			{
				pairData.PrepareArray();
				CommonInventoryData.pInstance.UpdateItemAttributes(userItemData.UserInventoryID, pairData);
			}
		}
	}

	public static void Init(CommonInventoryResponseItem[] itemList, KAUISelectItemData selectedData, OnItemCustomizationClosed callback, bool multiItemCustomizationUI)
	{
		if (CommonInventoryData.pInstance == null || itemList.Length == 0)
		{
			return;
		}
		List<UserItemData> list = new List<UserItemData>();
		foreach (CommonInventoryResponseItem commonInventoryResponseItem in itemList)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItemByUserInventoryID(commonInventoryResponseItem.CommonInventoryID);
			if (userItemData != null)
			{
				list.Add(userItemData);
			}
		}
		Init(list.ToArray(), selectedData, callback, multiItemCustomizationUI);
	}

	public static void Init(UserItemData[] itemData, KAUISelectItemData selectedItemData, OnItemCustomizationClosed callback, bool multiItemCustomizationUI)
	{
		if (itemData.Length == 0)
		{
			callback(null);
			return;
		}
		mItemCustomizationUiData = ItemCustomizationSettings.pInstance.GetItemCustomizationUiData(itemData[0].Item, multiItemCustomizationUI);
		if (mItemCustomizationUiData == null)
		{
			return;
		}
		if (!mItemCustomizationUiData._PaidCustomization && ItemCustomizationSettings.pInstance.MultiItemPays(itemData[0].Item))
		{
			for (int i = 0; i < itemData.Length; i++)
			{
				UpdateUserItemAttributes(itemData[i], "FreeTkt", true.ToString(), save: true);
			}
		}
		string resourcePath = mItemCustomizationUiData._ResourcePath;
		if (string.IsNullOrEmpty(resourcePath))
		{
			UtDebug.LogError("Ui Resource path not found for specified item.");
			callback(null);
			return;
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = resourcePath.Split('/');
		string text = RsResourceManager.FormatBundleURL(array[0] + "/" + array[1]);
		mInitData[text + "/" + array[2]] = new InitData(selectedItemData, itemData, callback);
		RsResourceManager.LoadAssetFromBundle(text, array[2], OnLoadingComplete, typeof(GameObject));
	}

	private static void OnLoadingComplete(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			UiAvatarItemCustomization component = UnityEngine.Object.Instantiate((GameObject)inObject).GetComponent<UiAvatarItemCustomization>();
			if (component != null && CommonInventoryData.pInstance != null && mInitData.ContainsKey(inURL))
			{
				KAUI.SetExclusive(component, Color.clear);
				component.CustomizeItem(mInitData[inURL]);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (mInitData.ContainsKey(inURL) && mInitData[inURL]._Callback != null)
			{
				mInitData[inURL]._Callback(null);
			}
			break;
		}
	}

	private void CustomizeItem(InitData initData)
	{
		if (initData == null || (initData._ItemDataList.Length == 0 && initData._SelectedItemData == null))
		{
			if (initData == null)
			{
				UtDebug.LogError("Item customization init data cannot be NULL");
			}
			_OnItemCustomizationClosed(null);
		}
		else
		{
			_OnItemCustomizationClosed = (OnItemCustomizationClosed)Delegate.Combine(_OnItemCustomizationClosed, initData._Callback);
			mSelectedItemData = initData._SelectedItemData;
			mUserItemDataArray = initData._ItemDataList;
			SaveCustomizationsFromV1(mUserItemDataArray);
			_Menu.PopulateMenu(mUserItemDataArray, mSelectedItemData);
			EnableOkBtn(inEnable: false);
		}
	}

	public static ItemCustomizationData[] GetCustomizationData(int inventoryID)
	{
		return GetCustomizationData(CommonInventoryData.pInstance.FindItemByUserInventoryID(inventoryID));
	}

	public static ItemCustomizationData[] GetCustomizationData(UserItemData userItemData)
	{
		if (userItemData != null)
		{
			ItemCustomizationData[] customizationConfig = ItemCustomizationSettings.pInstance.GetCustomizationConfig(userItemData.Item);
			if (customizationConfig != null && customizationConfig.Length != 0)
			{
				PairData userItemAttributes = userItemData.UserItemAttributes;
				if (userItemAttributes != null)
				{
					userItemAttributes.Init();
					List<ItemCustomizationData> list = new List<ItemCustomizationData>();
					ItemCustomizationData[] array = customizationConfig;
					foreach (ItemCustomizationData itemCustomizationData in array)
					{
						ItemCustomizationData copy = itemCustomizationData.GetCopy();
						if (userItemAttributes.KeyExists(itemCustomizationData._ShaderPropName))
						{
							copy._Value = userItemAttributes.GetStringValue(itemCustomizationData._ShaderPropName, "");
						}
						list.Add(copy);
					}
					return list.ToArray();
				}
				ItemCustomizationData[] array2 = GetCustomizationDataV1(userItemData);
				if (array2 == null)
				{
					array2 = ItemCustomizationSettings.pInstance.GetCustomizationConfig(userItemData.Item);
				}
				return array2;
			}
		}
		return null;
	}

	private static void OnDataReady(GameObject customizedObject, int inventoryID)
	{
		ItemCustomizationData[] customizationData = GetCustomizationData(inventoryID);
		ApplyCustomization(customizedObject, customizationData);
	}

	public static void ApplyCustomization(GameObject customizedObject, int inventoryID)
	{
		LoadPairData(delegate
		{
			OnDataReady(customizedObject, inventoryID);
		});
	}

	public static void ApplyCustomization(GameObject customizedObject, ItemCustomizationData[] custDataArray)
	{
		if (custDataArray != null)
		{
			foreach (ItemCustomizationData custData in custDataArray)
			{
				ApplyCustomization(customizedObject, custData);
			}
		}
	}

	public static void ApplyCustomization(GameObject customizedObject, ItemCustomizationData custData)
	{
		if (!(customizedObject != null) || custData == null)
		{
			return;
		}
		Renderer[] componentsInChildren = customizedObject.GetComponentsInChildren<Renderer>();
		if (componentsInChildren == null)
		{
			return;
		}
		Renderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Material[] materials = array[i].materials;
			foreach (Material material in materials)
			{
				switch (custData._Type)
				{
				case CustomizePropertyType.COLOR:
				{
					Color color = Color.white;
					if (custData._UseGroupLogo && AvatarData.pInstance._Group != null)
					{
						if (custData._LogoColorSource == LogoColorSource.BACKGROUND_COLOR)
						{
							AvatarData.pInstance._Group.GetBGColor(out color);
						}
						else
						{
							AvatarData.pInstance._Group.GetFGColor(out color);
						}
					}
					else
					{
						HexUtil.HexToColor(custData._Value, out color);
					}
					material.SetColor(custData._ShaderPropName, color);
					break;
				}
				case CustomizePropertyType.TEXTURE:
				{
					string text = "";
					text = ((!custData._UseGroupLogo || AvatarData.pInstance._Group == null) ? custData._Value : AvatarData.pInstance._Group.Logo);
					TextureAssignData textureAssignData = new TextureAssignData();
					textureAssignData._Material = material;
					textureAssignData._ShaderPropName = custData._ShaderPropName;
					string[] array2 = text.Split('/');
					if (array2.Length == 3)
					{
						RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], OnTextureLoaded, typeof(Texture2D), inDontDestroy: false, textureAssignData);
					}
					break;
				}
				}
			}
		}
	}

	public static void OnTextureLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE && inObject != null)
		{
			Texture2D value = (Texture2D)inObject;
			TextureAssignData textureAssignData = (TextureAssignData)inUserData;
			textureAssignData._Material.SetTexture(textureAssignData._ShaderPropName, value);
		}
	}

	protected override void Start()
	{
		base.Start();
		mResetBtn = FindItem("BtnReset");
		mColorPalette = FindItem("ColorPicker");
		mColorSelector = FindItem("ColorSelector");
		mOkBtn = FindItem("BtnOk");
		if (mItemCustomizationUiData._PaidCustomization)
		{
			mOkBtn.SetState(KAUIState.DISABLED);
		}
		mSyncBtn = FindItem("SyncColorBtn");
		mSyncClanColorBtn = FindItem("SyncClanColorBtn");
		if (string.IsNullOrEmpty(UserProfile.pProfileData.GetGroupID()))
		{
			mSyncClanColorBtn.SetDisabled(isDisabled: true);
		}
		mCustomizeLockBtn = FindItem("ColorLockBtn");
		if ((bool)_UiSyncDragonColorConfirmationDB)
		{
			_UiSyncDragonColorConfirmationDB.SetDestroyOnClick(isDestroy: false);
			_UiSyncDragonColorConfirmationDB.SetTitle(_MatchDragonColorTitleText.GetLocalizedString());
			_UiSyncDragonColorConfirmationDB.SetText(_MatchDragonColorText.GetLocalizedString(), interactive: false);
			_UiSyncDragonColorConfirmationDB.SetMessage(base.gameObject, "OnSyncDragonColorYes", "OnSyncDragonColorNo", "", "");
			_UiSyncDragonColorConfirmationDB.SetVisibility(inVisible: false);
		}
		if ((bool)_UiSyncClanColorConfirmationDB)
		{
			_UiSyncClanColorConfirmationDB.SetDestroyOnClick(isDestroy: false);
			_UiSyncClanColorConfirmationDB.SetTitle(_MatchClanColorTitleText.GetLocalizedString());
			_UiSyncClanColorConfirmationDB.SetText(_MatchClanColorText.GetLocalizedString(), interactive: false);
			_UiSyncClanColorConfirmationDB.SetMessage(base.gameObject, "OnSyncClanColorYes", "OnSyncClanColorNo", "", "");
			_UiSyncClanColorConfirmationDB.SetVisibility(inVisible: false);
		}
		mHasCustomizationTicket = !mItemCustomizationUiData._PaidCustomization;
		if (_CustomizationUiSetup.Count > 0)
		{
			mSelectedColorBtn = _CustomizationUiSetup[0]._CustomizeWidget;
		}
		if (mItemCustomizationUiData._PaidCustomization)
		{
			mItemPurchase = new ItemPurchase();
			mItemPurchase.SetMessages(null, _PurchaseFailText, null, _PurchaseProcessingText, _NotEnoughCurrencyText);
			mItemPurchase.Init(mItemCustomizationUiData._CustomizationTicketStoreId, mItemCustomizationUiData._CustomizationTicketId, OnTicketPurchase, ItemPurchaseSource.AVATAR_ITEM_CUSTOMIZATION.ToString(), "Customization_Item");
			mItemPurchase.LoadItemInfo();
		}
		RefreshLockBtn();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		mItemCustomizationUiData = null;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mOkBtn)
		{
			SaveChanges();
			mHasCustomizationTicket = false;
			OnClosed(mSelectedItemData);
		}
		else if (inWidget == mResetBtn)
		{
			ShowResetDialog();
		}
		else if (inWidget.name == "BtnBack" || inWidget.name == "CloseBtn")
		{
			if (mHasCustomizationTicket)
			{
				mKAUIGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _TicketLooseWarningText.GetLocalizedString(), "", base.gameObject, "OnBackPressConfirm", "KillGenericDB", "", "", inDestroyOnClick: true);
				KAUI.SetExclusive(mKAUIGenericDB);
			}
			else
			{
				OnClosed(null);
			}
		}
		else if (inWidget == mColorPalette)
		{
			EnableOkBtn(inEnable: true);
		}
		else if (mSyncBtn != null && inWidget == mSyncBtn)
		{
			SyncDragonColor();
		}
		else if ((bool)mSyncClanColorBtn && inWidget == mSyncClanColorBtn)
		{
			SyncClanColor();
		}
		CustomizationUiSetup customizationUiSetup = _CustomizationUiSetup.Find((CustomizationUiSetup setup) => setup._CustomizeWidget == inWidget);
		if (customizationUiSetup == null)
		{
			return;
		}
		if (!mHasCustomizationTicket)
		{
			ShowConfirmationDB();
			if (!mHasCustomizationTicket)
			{
				return;
			}
		}
		mSelectedColorBtn = customizationUiSetup._CustomizeWidget;
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		base.OnPressRepeated(inWidget, inPressed);
		if (!(mCurrentCustomizeObject != null) && !(mCustomAvatarObj != null))
		{
			return;
		}
		if (inWidget == mColorPalette)
		{
			if (!mHasCustomizationTicket)
			{
				ShowConfirmationDB();
				if (!mHasCustomizationTicket)
				{
					return;
				}
			}
			if (!inPressed)
			{
				return;
			}
			GameObject obj = inWidget.transform.Find("Background").gameObject;
			Vector2 vector = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			UITexture component = obj.GetComponent<UITexture>();
			UISlicedSprite uISlicedSprite = mColorSelector.pBackground as UISlicedSprite;
			Vector3 vector2 = Vector3.zero;
			if (component != null)
			{
				vector2 = new Vector3(component.width, component.height, 1f);
			}
			Vector3 position = obj.transform.position - vector2 * 0.5f;
			Vector3 position2 = obj.transform.position + vector2 * 0.5f;
			position = UICamera.currentCamera.WorldToScreenPoint(position);
			position2 = UICamera.currentCamera.WorldToScreenPoint(position2);
			float b = (vector.x - position.x) / (position2.x - position.x);
			float b2 = (vector.y - position.y) / (position2.y - position.y);
			float num = (float)uISlicedSprite.width / (position2.x - position.x);
			float num2 = (float)uISlicedSprite.height / (position2.y - position.y);
			b = Mathf.Min(Mathf.Max(num * 0.5f, b), 1f - num * 0.5f);
			b2 = Mathf.Min(Mathf.Max(num2 * 0.5f, b2), 1f - num2 * 0.5f);
			Vector3 vector3 = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(position.x + b * (position2.x - position.x), position.y + b2 * (position2.y - position.y), 0f));
			mColorSelector.transform.position = new Vector3(vector3.x, vector3.y, mColorSelector.transform.position.z);
			Color pixelBilinear = ((Texture2D)inWidget.GetTexture()).GetPixelBilinear(b, b2);
			CustomizationUiSetup customizationUiSetup = _CustomizationUiSetup.Find((CustomizationUiSetup s) => s._CustomizeWidget == mSelectedColorBtn);
			if (customizationUiSetup != null)
			{
				mSelectedColorBtn.pBackground.color = pixelBilinear;
				UpdateCustomization(mSelectedItemData._UserInventoryID, customizationUiSetup._ShaderPropName, HexUtil.ColorToHex(pixelBilinear));
			}
			UpdateResetBtnState();
		}
		float num3 = 0f;
		if (inWidget == _LeftRotateBtn)
		{
			num3 = _RotationSpeed * Time.deltaTime;
		}
		else if (inWidget == _RightRotateBtn)
		{
			num3 = (0f - _RotationSpeed) * Time.deltaTime;
		}
		if (num3 != 0f)
		{
			if (mCustomAvatarObj != null && mItemCustomizationUiData._ShowAvatar)
			{
				mCustomAvatarObj.transform.Rotate(0f, num3, 0f);
			}
			else
			{
				mCurrentCustomizeObject.transform.Rotate(0f, num3, 0f);
			}
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		base.OnDrag(inWidget, inDelta);
		if (string.IsNullOrEmpty(_DragWidgetName) || !(inWidget.name == _DragWidgetName))
		{
			return;
		}
		float num = (0f - _RotationSpeed) * Time.deltaTime * inDelta.x;
		if (num != 0f)
		{
			if (mCustomAvatarObj != null && mItemCustomizationUiData._ShowAvatar)
			{
				mCustomAvatarObj.transform.Rotate(0f, num, 0f);
			}
			else
			{
				mCurrentCustomizeObject.transform.Rotate(0f, num, 0f);
			}
		}
	}

	public void OnSelectItem(KAWidget inWidget)
	{
		mSelectedItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (mLastSelectedItemData == null || !mItemCustomizationUiData._PaidCustomization)
		{
			OnConfirmSelectItem();
		}
		else if (mHasCustomizationTicket)
		{
			mKAUIGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _TicketLooseWarningText.GetLocalizedString(), "", base.gameObject, "OnConfirmSelectItem", "OnCancelConfirmSelectItem", "", "", inDestroyOnClick: true);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
		else
		{
			OnConfirmSelectItem();
		}
	}

	private void OnCancelConfirmSelectItem()
	{
		mSelectedItemData = mLastSelectedItemData;
		_Menu.SetSelectedItem(mSelectedItemData.GetItem());
		KillGenericDB();
	}

	private void OnConfirmSelectItem()
	{
		if (mSelectedItemData == null)
		{
			return;
		}
		mLastSelectedItemData = mSelectedItemData;
		if (mCurrentCustomizeObject != null)
		{
			UnityEngine.Object.Destroy(mCurrentCustomizeObject);
		}
		if (mCustomAvatarObj == null)
		{
			InitAvatar();
		}
		if (_UiItemStats != null)
		{
			_UiItemStats.ShowStats(mSelectedItemData._UserItemData);
		}
		if (_UiItemElement != null)
		{
			_UiItemElement.ShowElements(mSelectedItemData._UserItemData);
		}
		if (mItemCustomizationUiData != null && mItemCustomizationUiData._ShowAvatar && mCustomAvatarState != null && mCustomAvatarObj != null)
		{
			EquipItem(mSelectedItemData);
			mHasCustomizationTicket = false;
			RefreshLockBtn();
			return;
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		if (!string.IsNullOrEmpty(mSelectedItemData._ItemData.AssetName) && mSelectedItemData._ItemData.AssetName != "NULL")
		{
			string[] array = mSelectedItemData._ItemData.AssetName.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnCustomizeObjectLoaded, typeof(GameObject));
		}
	}

	private void OnCustomizeObjectLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject original = UnityEngine.Object.Instantiate((GameObject)inObject);
			mCurrentCustomizeObject = UnityEngine.Object.Instantiate(original);
			mCurrentCustomizeObject.transform.parent = _ItemMarker.parent;
			mCurrentCustomizeObject.transform.localPosition = _ItemMarker.localPosition;
			InitItem();
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.Log("Item bundle not found " + inURL);
			break;
		}
	}

	private void InitItem()
	{
		bool flag = mCustomizationChanges.ContainsKey(mSelectedItemData._UserInventoryID);
		if (mCurrentCustomizeObject != null)
		{
			if (flag)
			{
				List<ItemCustomizationData> list = mCustomizationChanges[mSelectedItemData._UserInventoryID];
				ApplyCustomization(mCurrentCustomizeObject, list.ToArray());
			}
			else
			{
				ApplyCustomization(mCurrentCustomizeObject, mSelectedItemData._UserInventoryID);
			}
		}
		else if (mCustomAvatarObj != null)
		{
			string partName = AvatarData.GetPartName(mSelectedItemData._ItemData);
			if (flag)
			{
				List<ItemCustomizationData> list2 = mCustomizationChanges[mSelectedItemData._UserInventoryID];
				ApplyCustomizationOnPart(mCustomAvatarObj, partName, mInstanceInfo.mInstance, list2.ToArray());
			}
			else
			{
				ApplyCustomizationOnPart(mCustomAvatarObj, partName, mInstanceInfo.mInstance, mSelectedItemData._UserInventoryID);
			}
		}
		UpdateResetBtnState();
		ItemCustomizationData[] customizationData = GetCustomizationData(mSelectedItemData._UserInventoryID);
		ResetButtonColor(customizationData);
	}

	private void UpdateCustomization(int inventoryID, string shaderPropName, string value)
	{
		List<ItemCustomizationData> list = null;
		if (mCustomizationChanges.ContainsKey(inventoryID))
		{
			list = mCustomizationChanges[inventoryID];
		}
		else
		{
			ItemCustomizationData[] customizationData = GetCustomizationData(inventoryID);
			if (customizationData == null)
			{
				return;
			}
			list = new List<ItemCustomizationData>(customizationData);
			mCustomizationChanges.Add(inventoryID, list);
		}
		ItemCustomizationData itemCustomizationData = list.Find((ItemCustomizationData data) => data._ShaderPropName == shaderPropName);
		if (itemCustomizationData != null && itemCustomizationData._Value != value)
		{
			itemCustomizationData._Value = value;
		}
		if (mCurrentCustomizeObject != null)
		{
			ApplyCustomization(mCurrentCustomizeObject, itemCustomizationData);
		}
		else
		{
			ApplyCustomizationOnPart(mCustomAvatarObj, AvatarData.GetPartName(mSelectedItemData._ItemData), mInstanceInfo.mInstance, itemCustomizationData);
		}
	}

	private void SaveChanges()
	{
		if (mCustomizationChanges.Keys.Count <= 0)
		{
			return;
		}
		foreach (int key in mCustomizationChanges.Keys)
		{
			PairData pairData = new PairData();
			pairData.Init();
			foreach (ItemCustomizationData item in mCustomizationChanges[key])
			{
				if (!string.IsNullOrEmpty(item._Value))
				{
					pairData.SetValue(item._ShaderPropName, item._Value);
				}
			}
			if (pairData.pPairList != null && pairData.pPairList.Count > 0)
			{
				pairData.PrepareArray();
				CommonInventoryData.pInstance.UpdateItemAttributes(key, pairData);
			}
		}
	}

	private void ResetCurrentCustomization()
	{
		mCustomizationChanges.Remove(mSelectedItemData._UserInventoryID);
		if (mCurrentCustomizeObject != null)
		{
			ApplyCustomization(mCurrentCustomizeObject, mSelectedItemData._UserInventoryID);
		}
		else
		{
			ApplyCustomizationOnPart(mCustomAvatarObj, AvatarData.GetPartName(mSelectedItemData._ItemData), mInstanceInfo.mInstance, mSelectedItemData._UserInventoryID);
		}
		ItemCustomizationData[] customizationData = GetCustomizationData(mSelectedItemData._UserInventoryID);
		ResetButtonColor(customizationData);
	}

	private void EnableOkBtn(bool inEnable)
	{
		if (!(mOkBtn != null) || !mItemCustomizationUiData._PaidCustomization)
		{
			return;
		}
		if (inEnable)
		{
			if (mOkBtn.GetState() == KAUIState.DISABLED)
			{
				mOkBtn.SetState(KAUIState.INTERACTIVE);
			}
		}
		else if (mOkBtn.GetState() == KAUIState.INTERACTIVE)
		{
			mOkBtn.SetState(KAUIState.DISABLED);
		}
	}

	private void UpdateResetBtnState()
	{
		if (mResetBtn != null)
		{
			if (mCustomizationChanges.ContainsKey(mSelectedItemData._UserInventoryID))
			{
				mResetBtn.SetState(KAUIState.INTERACTIVE);
			}
			else
			{
				mResetBtn.SetState(KAUIState.DISABLED);
			}
		}
	}

	private void CustomizeAvatarShaders(Color primary, Color secondary)
	{
		foreach (CustomizationUiSetup item in _CustomizationUiSetup)
		{
			if (item._ShaderPropName == "_PrimaryColor")
			{
				UpdateCustomization(mSelectedItemData._UserInventoryID, item._ShaderPropName, HexUtil.ColorToHex(primary));
				item._CustomizeWidget.pBackground.color = primary;
			}
			else if (item._ShaderPropName == "_SecondaryColor")
			{
				UpdateCustomization(mSelectedItemData._UserInventoryID, item._ShaderPropName, HexUtil.ColorToHex(secondary));
				item._CustomizeWidget.pBackground.color = secondary;
			}
			EnableOkBtn(inEnable: true);
		}
	}

	private void SyncClanColor()
	{
		if (!mHasCustomizationTicket)
		{
			ShowConfirmationDB();
		}
		else if (_UiSyncClanColorConfirmationDB != null)
		{
			_UiSyncClanColorConfirmationDB.SetVisibility(inVisible: true);
			KAUI.SetExclusive(_UiSyncClanColorConfirmationDB);
		}
		else
		{
			OnSyncClanColorYes();
		}
	}

	private void OnSyncClanColorYes()
	{
		if (_UiSyncClanColorConfirmationDB != null)
		{
			_UiSyncClanColorConfirmationDB.SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(_UiSyncClanColorConfirmationDB);
		}
		if (UserProfile.pProfileData.HasGroup() && UserProfile.pProfileData.Groups[0] != null)
		{
			AvatarData.pInstance._Group.GetFGColor(out var color);
			AvatarData.pInstance._Group.GetBGColor(out var color2);
			CustomizeAvatarShaders(color, color2);
		}
	}

	private void OnSyncClanColorNo()
	{
		if (!(_UiSyncClanColorConfirmationDB == null))
		{
			_UiSyncClanColorConfirmationDB.SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(_UiSyncClanColorConfirmationDB);
		}
	}

	private void SyncDragonColor()
	{
		if (!mHasCustomizationTicket)
		{
			ShowConfirmationDB();
		}
		else if (_UiSyncDragonColorConfirmationDB != null)
		{
			_UiSyncDragonColorConfirmationDB.SetVisibility(inVisible: true);
			KAUI.SetExclusive(_UiSyncDragonColorConfirmationDB);
		}
		else
		{
			OnSyncDragonColorYes();
		}
	}

	private void OnSyncDragonColorYes()
	{
		if (_UiSyncDragonColorConfirmationDB != null)
		{
			_UiSyncDragonColorConfirmationDB.SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(_UiSyncDragonColorConfirmationDB);
		}
		if (SanctuaryManager.pCurPetInstance.pData != null)
		{
			Color color = SanctuaryManager.pCurPetInstance.pData.GetColor(0);
			Color color2 = SanctuaryManager.pCurPetInstance.pData.GetColor(1);
			CustomizeAvatarShaders(color, color2);
		}
	}

	private void OnSyncDragonColorNo()
	{
		if (!(_UiSyncDragonColorConfirmationDB == null))
		{
			_UiSyncDragonColorConfirmationDB.SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(_UiSyncDragonColorConfirmationDB);
		}
	}

	private void ResetButtonColor(ItemCustomizationData[] custDataArray)
	{
		foreach (CustomizationUiSetup item in _CustomizationUiSetup)
		{
			if (custDataArray != null)
			{
				foreach (ItemCustomizationData itemCustomizationData in custDataArray)
				{
					if (item._ShaderPropName == itemCustomizationData._ShaderPropName)
					{
						HexUtil.HexToColor(itemCustomizationData._Value, out var color);
						item._CustomizeWidget.pBackground.color = color;
					}
				}
			}
			else
			{
				item._CustomizeWidget.pBackground.color = Color.gray;
			}
		}
	}

	private void ShowResetDialog()
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "MessageBox");
		kAUIGenericDB.SetMessage(base.gameObject, "ResetCurrentItem", "", "", "");
		kAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		kAUIGenericDB.SetText(_ResetCustomizationText.GetLocalizedString(), interactive: false);
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		KAUI.SetExclusive(kAUIGenericDB);
	}

	private void ResetCurrentItem()
	{
		ResetCurrentCustomization();
		if (mResetBtn != null)
		{
			mResetBtn.SetState(KAUIState.DISABLED);
		}
	}

	private void OnBackPressConfirm()
	{
		OnClosed(null);
	}

	private void OnClosed(KAUISelectItemData inItemData)
	{
		if (!mItemCustomizationUiData._PaidCustomization && ItemCustomizationSettings.pInstance.MultiItemPays(mSelectedItemData._ItemData))
		{
			UpdateUserItemAttributes(mSelectedItemData._UserItemData, "FreeTkt", false.ToString(), save: true);
		}
		if (mInstanceInfo != null && mInstanceInfo.mInstance != null)
		{
			mInstanceInfo.mInstance.Part = mCachedParts;
		}
		if (inItemData != null && _Menu != null)
		{
			List<KAWidget> items = _Menu.GetItems();
			string partName = AvatarData.GetPartName(mSelectedItemData._ItemData);
			AvatarDataPart avatarDataPart = AvatarData.FindPart(partName);
			AvatarDataPart avatarDataPart2 = AvatarData.FindPart("DEFAULT_" + partName);
			if (items != null && items.Count > 0)
			{
				foreach (KAWidget item in items)
				{
					if (item.GetUserData() is KAUISelectItemData kAUISelectItemData)
					{
						if (avatarDataPart != null && avatarDataPart.UserInventoryId.GetValueOrDefault(-1) == kAUISelectItemData._UserInventoryID)
						{
							SaveAvatarPartAttributes(partName, kAUISelectItemData._UserInventoryID);
							break;
						}
						if (avatarDataPart2 != null && avatarDataPart2.UserInventoryId.GetValueOrDefault(-1) == kAUISelectItemData._UserInventoryID)
						{
							SaveAvatarPartAttributes("DEFAULT_" + partName, kAUISelectItemData._UserInventoryID);
							break;
						}
					}
				}
			}
		}
		mHasCustomizationTicket = false;
		KAUI.RemoveExclusive(this);
		mInitData.Clear();
		_OnItemCustomizationClosed(inItemData);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public static void SetThumbnail(KAWidget inWidget, int inventoryID)
	{
		UserItemData userItemData = CommonInventoryData.pInstance.FindItemByUserInventoryID(inventoryID);
		if (userItemData != null && inWidget != null && !string.IsNullOrEmpty(userItemData.Item.AssetName) && userItemData.Item.AssetName != "NULL")
		{
			ShowLoadingProgress(inWidget, inShow: true);
			ThumbnailUserData thumbnailUserData = new ThumbnailUserData();
			thumbnailUserData._ThumbnailWidget = inWidget;
			thumbnailUserData._UserItemData = userItemData;
			string[] array = userItemData.Item.AssetName.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoaded, typeof(GameObject), inDontDestroy: false, thumbnailUserData);
		}
	}

	private static void OnAssetLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE && inObject != null && inUserData != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			ThumbnailUserData thumbnailUserData = (ThumbnailUserData)inUserData;
			if (thumbnailUserData._UserItemData != null)
			{
				AvPhotoManager avPhotoManager = AvPhotoManager.Init("PfAvPartPhotoMgr");
				gameObject.transform.localPosition = avPhotoManager.transform.localPosition;
				gameObject.transform.localPosition += ItemCustomizationSettings.pInstance.GetThumbnailCameraOffset(thumbnailUserData._UserItemData.Item);
				ApplyCustomization(gameObject, thumbnailUserData._UserItemData.UserInventoryID);
				Texture2D dstTexture = new Texture2D(256, 256, TextureFormat.ARGB32, mipChain: false);
				avPhotoManager.TakeAShot(gameObject, ref dstTexture, gameObject.transform);
				thumbnailUserData._ThumbnailWidget.SetTexture(dstTexture);
			}
			ShowLoadingProgress(thumbnailUserData._ThumbnailWidget, inShow: false);
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	private static void ShowLoadingProgress(KAWidget inWidget, bool inShow)
	{
		if (inWidget != null)
		{
			KAWidget kAWidget = inWidget.FindChildItem("Loading");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inShow);
			}
		}
	}

	public static void SaveAvatarPartAttributes(string partName, int inventoryID)
	{
		if (string.IsNullOrEmpty(partName))
		{
			return;
		}
		ItemCustomizationData[] customizationData = GetCustomizationData(inventoryID);
		if (customizationData == null)
		{
			return;
		}
		ItemCustomizationData[] array = customizationData;
		foreach (ItemCustomizationData itemCustomizationData in array)
		{
			AvatarPartAttribute avatarPartAttribute = new AvatarPartAttribute();
			avatarPartAttribute.Key = itemCustomizationData._ShaderPropName;
			string text = "";
			switch (itemCustomizationData._Type)
			{
			case CustomizePropertyType.COLOR:
				text = "color";
				break;
			case CustomizePropertyType.TEXTURE:
				text = "tex";
				break;
			}
			avatarPartAttribute.Value = text + "/" + itemCustomizationData._Value;
			AvatarData.SetAttribute(partName, avatarPartAttribute);
		}
	}

	public static void ApplyCustomizationOnPart(GameObject avatarObj, string partName, AvatarData avatarData, ItemCustomizationData[] custDataArray)
	{
		if (custDataArray != null)
		{
			foreach (ItemCustomizationData custData in custDataArray)
			{
				ApplyCustomizationOnPart(avatarObj, partName, avatarData, custData);
			}
		}
	}

	private static void OnDataReadyOnPart(GameObject avatarObj, string partName, AvatarData avatarData, int inventoryId)
	{
		ItemCustomizationData[] customizationData = GetCustomizationData(inventoryId);
		ApplyCustomizationOnPart(avatarObj, partName, avatarData, customizationData);
	}

	public static void ApplyCustomizationOnPart(GameObject avatarObj, string partName, AvatarData avatarData, int inventoryId)
	{
		LoadPairData(delegate
		{
			OnDataReadyOnPart(avatarObj, partName, avatarData, inventoryId);
		});
	}

	public static void ApplyCustomizationOnPart(GameObject avatarObj, string partName, AvatarData avatarData, ItemCustomizationData custData)
	{
		Transform transform = avatarObj.transform.Find(AvatarData.GetParentBone(partName));
		Transform transform2 = transform.Find(partName);
		if (transform2 != null)
		{
			ApplyCustomization(transform2.gameObject, custData);
		}
		else
		{
			if (!(partName == AvatarData.pPartSettings.AVATAR_PART_WING))
			{
				return;
			}
			for (int i = 0; i < avatarData.Part.Length; i++)
			{
				AvatarDataPart avatarDataPart = avatarData.Part[i];
				if (avatarDataPart.IsDefault() || avatarDataPart.Textures == null)
				{
					continue;
				}
				string[] textures = avatarDataPart.Textures;
				foreach (string text in textures)
				{
					if (string.IsNullOrEmpty(text) || !AvatarData.CustomizeTextureType(text))
					{
						continue;
					}
					transform = avatarObj.transform.Find(AvatarData.GetParentBone(avatarDataPart.PartType));
					if (avatarDataPart.PartType.Equals(AvatarData.pPartSettings.AVATAR_PART_HAND))
					{
						transform2 = transform.Find(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT);
						if (transform2 != null && transform2.gameObject != null)
						{
							ApplyCustomization(transform2.gameObject, custData);
						}
						transform2 = transform.Find(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT);
						if (transform2 != null && transform2.gameObject != null)
						{
							ApplyCustomization(transform2.gameObject, custData);
						}
					}
					else if (avatarDataPart.PartType.Equals(AvatarData.pPartSettings.AVATAR_PART_FEET))
					{
						transform2 = transform.Find(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT);
						if (transform2 != null && transform2.gameObject != null)
						{
							ApplyCustomization(transform2.gameObject, custData);
						}
						transform2 = transform.Find(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT);
						if (transform2 != null && transform2.gameObject != null)
						{
							ApplyCustomization(transform2.gameObject, custData);
						}
					}
					else if (avatarDataPart.PartType.Equals(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND))
					{
						transform2 = transform.Find(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT);
						if (transform2 != null && transform2.gameObject != null)
						{
							ApplyCustomization(transform2.gameObject, custData);
						}
						transform2 = transform.Find(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT);
						if (transform2 != null && transform2.gameObject != null)
						{
							ApplyCustomization(transform2.gameObject, custData);
						}
					}
					else if (avatarDataPart.PartType.Equals(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD))
					{
						transform2 = transform.Find(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT);
						if (transform2 != null && transform2.gameObject != null)
						{
							ApplyCustomization(transform2.gameObject, custData);
						}
						transform2 = transform.Find(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT);
						if (transform2 != null && transform2.gameObject != null)
						{
							ApplyCustomization(transform2.gameObject, custData);
						}
					}
					else
					{
						transform2 = transform.Find(avatarDataPart.PartType);
						if (transform2 != null && transform2.gameObject != null)
						{
							ApplyCustomization(transform2.gameObject, custData);
						}
					}
					break;
				}
			}
		}
	}

	private void InitAvatar()
	{
		if (mItemCustomizationUiData != null && mItemCustomizationUiData._ShowAvatar)
		{
			LoadAvatar();
		}
	}

	private void LoadAvatar()
	{
		if (mCustomAvatarObj == null)
		{
			mCustomAvatarObj = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfAvatar"));
		}
		mCustomAvatarObj.name = "PfAvatarItemCustomization";
		mCustomAvatarObj.transform.parent = _ItemMarker.parent;
		mCustomAvatarObj.transform.localPosition = _ItemMarker.localPosition;
		mInstanceInfo = AvatarData.ApplyCurrent(mCustomAvatarObj);
		if (mInstanceInfo != null)
		{
			mInstanceInfo.mAvatar = mCustomAvatarObj;
		}
		ProcessLoadedAvatar(mCustomAvatarObj, AvatarData.GetGender() == Gender.Male);
		mCustomAvatarObj.SetActive(value: true);
		if (mCustomAvatarState == null)
		{
			mCustomAvatarState = new CustomAvatarState();
		}
		mCustomAvatarState.FromAvatarData(mInstanceInfo.mInstance);
		mCustomAvatarState.UpdateShaders(mCustomAvatarObj, mInstanceInfo);
		CustomAvatarState.mCurrentInstance = mCustomAvatarState;
		mCachedParts = mInstanceInfo.GetClonedParts();
		Transform transform = UtUtilities.FindChildTransform(mCustomAvatarObj, "DisplayName");
		if (transform != null)
		{
			GameObject gameObject = transform.gameObject;
			if (gameObject != null)
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	private void ProcessLoadedAvatar(GameObject inObject, bool inMale)
	{
		if (inObject.GetComponent<AvAvatarController>() != null)
		{
			inObject.GetComponent<AvAvatarController>().enabled = false;
		}
		if (inObject.GetComponent<AvAvatarProperties>() != null)
		{
			inObject.GetComponent<AvAvatarProperties>().enabled = false;
		}
		if (inObject.GetComponent<AvSpellCast>() != null)
		{
			inObject.GetComponent<AvSpellCast>().enabled = false;
		}
		inObject.GetComponent<Collider>().enabled = false;
		if (_ItemMarker.transform != null)
		{
			inObject.transform.localScale = Vector3.one * _ItemMarker.localScale.x;
			inObject.transform.rotation = _ItemMarker.transform.rotation;
			inObject.transform.position = _ItemMarker.transform.position;
		}
	}

	protected override void Update()
	{
		base.Update();
		if ((mInstanceInfo == null || mInstanceInfo.pIsReady) && mItemsToDownload == 0 && mCustomAvatarState != null && mCustomAvatarState.mIsDirty && mCustomAvatarObj != null)
		{
			mCustomAvatarState.UpdateShaders(mCustomAvatarObj, mInstanceInfo);
			mCustomAvatarState.mIsDirty = false;
			InitItem();
		}
	}

	private void EquipItem(KAUISelectItemData selectedItemData)
	{
		mSelectedItemData = selectedItemData;
		mInstanceInfo.RestoreDefault();
		ApplyItem(mSelectedItemData._ItemData);
	}

	private void ApplyItem(ItemData inItemData)
	{
		ApplyItems(inItemData);
		if (inItemData.Relationship != null)
		{
			mItemsToDownload = inItemData.Relationship.Length;
			ItemDataRelationship[] relationship = inItemData.Relationship;
			for (int i = 0; i < relationship.Length; i++)
			{
				ItemData.Load(relationship[i].ItemId, OnItemDataReady, null);
			}
		}
	}

	public void OnItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		ApplyItems(dataItem);
		mItemsToDownload--;
	}

	private void ApplyItems(ItemData inItemData)
	{
		string itemPartType = AvatarData.GetItemPartType(inItemData);
		if (itemPartType == AvatarData.pPartSettings.AVATAR_PART_LEGS || itemPartType == AvatarData.pPartSettings.AVATAR_PART_FEET || itemPartType == AvatarData.pPartSettings.AVATAR_PART_TOP || itemPartType == AvatarData.pPartSettings.AVATAR_PART_HAIR || itemPartType == AvatarData.pPartSettings.AVATAR_PART_HAT || itemPartType == AvatarData.pPartSettings.AVATAR_PART_FACEMASK || itemPartType == AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT || itemPartType == AvatarData.pPartSettings.AVATAR_PART_BACK || itemPartType == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND || itemPartType == AvatarData.pPartSettings.AVATAR_PART_HAND || itemPartType == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
		{
			if (mInstanceInfo.IsDefaultSaved(itemPartType))
			{
				mInstanceInfo.RestorePartData();
			}
			bool flag = false;
			if (mSelectedItemData._ItemData.Relationship != null && mSelectedItemData._ItemData.Relationship.Length != 0)
			{
				flag = Array.Exists(mSelectedItemData._ItemData.Relationship, (ItemDataRelationship r) => r.Type.Equals("GroupParent"));
			}
			if (flag)
			{
				mInstanceInfo.SetGroupPart(mSelectedItemData._ItemData.ItemID, save: false);
			}
			ApplyItem(inItemData, itemPartType);
		}
		AvatarData.SetAttributes(mInstanceInfo, itemPartType, mSelectedItemData._ItemData.Attribute);
	}

	public void ApplyItem(ItemData inItem, string partName, bool inUpdateInventory = false)
	{
		if (mCustomAvatarState == null)
		{
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_SCAR)
		{
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DECAL1, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_FACE_DECAL)
		{
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DECAL2, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_HEAD)
		{
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName != AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			mCustomAvatarState.SetTextureData(partName, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatarState.SetTextureData(partName, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatarState.SetTextureData(partName, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND)
		{
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
		{
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DETAILEYES, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.EYEMASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_HAIR)
		{
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAIR, CustomAvatarState.pCustomAvatarSettings.MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAIR, CustomAvatarState.pCustomAvatarSettings.HIGHLIGHT, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_HIGHLIGHT));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_FEET)
		{
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_HAND)
		{
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatarState.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (inUpdateInventory)
		{
			mCustomAvatarState.SetInventoryId(partName, mSelectedItemData._UserItemData.UserInventoryID);
		}
	}

	private void RefreshLockBtn()
	{
		if (mCustomizeLockBtn != null)
		{
			mCustomizeLockBtn.SetVisibility(!mHasCustomizationTicket);
		}
	}

	public void ShowConfirmationDB()
	{
		if (mItemCustomizationUiData != null && mItemCustomizationUiData._PaidCustomization)
		{
			int itemCost = mItemPurchase.GetItemCost();
			mKAUIGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _PurchaseConfirmationText.GetLocalizedString().Replace("{cost}", itemCost.ToString()), "", base.gameObject, "CheckCustomizationAllowed", "KillGenericDB", "", "", inDestroyOnClick: true);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
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

	private void CheckCustomizationAllowed()
	{
		if (mItemCustomizationUiData == null || !mItemCustomizationUiData._PaidCustomization)
		{
			return;
		}
		if (CommonInventoryData.pInstance.GetQuantity(mItemCustomizationUiData._CustomizationTicketId) <= 0)
		{
			if (mItemPurchase != null)
			{
				mItemPurchase.ProcessPurchase();
			}
		}
		else
		{
			UseTicketIfExist();
		}
	}

	private void OnTicketPurchase(ItemPurchase.Status state)
	{
		switch (state)
		{
		case ItemPurchase.Status.Success:
			UseTicketIfExist();
			break;
		case ItemPurchase.Status.DataLoaded:
			if (mCustomizeLockBtn != null)
			{
				mCustomizeLockBtn.SetText(mItemPurchase.GetItemCost().ToString());
			}
			break;
		}
	}

	public void UseTicketIfExist()
	{
		UserItemData userItemData = (mCurrentCustomTicketItem = CommonInventoryData.pInstance.FindItem(mItemCustomizationUiData._CustomizationTicketId));
		string text = "";
		if (mCurrentCustomTicketItem.UserItemAttributes == null)
		{
			SetState(KAUIState.DISABLED);
			KAUICursorManager.SetDefaultCursor("Loading");
			UpdateUserItemAttributes(mCurrentCustomTicketItem, "CustTkt", mSelectedItemData._ItemData.ItemID.ToString(), save: true, OnInventorySaveCallBack);
			return;
		}
		mCurrentCustomTicketItem.UserItemAttributes.Init();
		if (mCurrentCustomTicketItem.UserItemAttributes.KeyExists("CustTkt"))
		{
			text = mCurrentCustomTicketItem.UserItemAttributes.GetStringValue("CustTkt", "");
			if (!string.IsNullOrEmpty(text))
			{
				int result = -1;
				int.TryParse(text, out result);
				if (result == mSelectedItemData._ItemData.ItemID)
				{
					SetState(KAUIState.DISABLED);
					KAUICursorManager.SetDefaultCursor("Loading");
					CommonInventoryData.pInstance.UseItem(mCurrentCustomTicketItem, 1, OnItemRemoveFromInventory, userItemData.Item);
				}
			}
		}
		else
		{
			SetState(KAUIState.DISABLED);
			KAUICursorManager.SetDefaultCursor("Loading");
			UpdateUserItemAttributes(mCurrentCustomTicketItem, "CustTkt", mSelectedItemData._ItemData.ItemID.ToString(), save: true, OnInventorySaveCallBack);
		}
	}

	private void OnInventorySaveCallBack(bool inSaveSuccess)
	{
		if (inSaveSuccess)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			CommonInventoryData.pInstance.UseItem(mCurrentCustomTicketItem, 1, OnItemRemoveFromInventory, mCurrentCustomTicketItem.Item);
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetState(KAUIState.INTERACTIVE);
		}
	}

	private void OnItemRemoveFromInventory(bool success, object inUserData)
	{
		if (success)
		{
			CommonInventoryData.pInstance.RemoveItem(mCurrentCustomTicketItem, 1, checkCategory: false);
			mHasCustomizationTicket = true;
			RefreshLockBtn();
		}
		SetState(KAUIState.INTERACTIVE);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	public static bool UpdateUserItemAttributes(UserItemData itemData, string key, string value, bool save, InventorySetAttributeEventHandler inCallback = null)
	{
		if (itemData == null)
		{
			return false;
		}
		if (itemData.UserItemAttributes == null)
		{
			itemData.UserItemAttributes = new PairData();
		}
		itemData.UserItemAttributes.Init();
		itemData.UserItemAttributes.SetValue(key, value);
		if (itemData.UserItemAttributes.pPairList != null && itemData.UserItemAttributes.pPairList.Count > 0 && save)
		{
			itemData.UserItemAttributes.PrepareArray();
			CommonInventoryData.pInstance.UpdateItemAttributes(itemData.UserInventoryID, itemData.UserItemAttributes, inCallback);
		}
		return true;
	}
}
