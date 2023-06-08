using System;
using System.Collections.Generic;
using UnityEngine;

public class UiConsumable : KAUI
{
	public KAWidget[] _CarryConsumableBtns;

	public KAWidget[] _GameConsumableBtns;

	private List<HotKey> mHotKeys = new List<HotKey>();

	private IConsumable mConsumableInstance;

	private bool mCallbackAssigned;

	protected string mGameName;

	protected int mGameMode;

	public int _CarryConsumableCategoryID;

	public int _MaxSlots = 2;

	public float _IconCycleSpeed = 10f;

	public int _MaxNumTexturesToLoad = 3;

	public int _NumOfIconCycles = 3;

	public AudioClip _PowerupCyclingSound;

	public string _DefaultIconTextureURL = "RS_DATA/DragonsBoostsDO.unity3d/IcoDWDragonsBoostQuestion";

	public bool _DefaultButtonClickable;

	public AudioClip _CannotUseSFX;

	[HideInInspector]
	public bool _IsGamePaused;

	private Vector3 mConsumablePos = new Vector3(0f, 0f, 0f);

	private bool mIsCarryDataSet;

	private List<Texture2D> mTexturesList = new List<Texture2D>();

	private int mNumTexturesToBeLoaded;

	private List<string> mDisplayDisableConsumableNames = new List<string>();

	private List<string> mClickDisableConsumableNames = new List<string>();

	private bool mIsConsumableCoolingDown;

	private bool mIsInventoryUpdated;

	private Dictionary<string, int> mConsumableCountDict = new Dictionary<string, int>();

	public const string TYPE_GAME = "Game";

	public const string TYPE_CARRY = "Carry";

	public string pGameName => mGameName;

	public void SetGameData(IConsumable inConsumable, string gameName, int gameMode = 0, bool inForceUpdate = false)
	{
		mGameName = gameName;
		mConsumableInstance = inConsumable;
		mGameMode = gameMode;
		if (inForceUpdate)
		{
			ResetButtonStates();
		}
		if (mTexturesList.Count == 0)
		{
			InitIconTextures();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mConsumablePos = base.transform.position;
	}

	protected override void Start()
	{
		base.Start();
		ResetButtonStates();
		ShowKeys(!UtPlatform.IsMobile());
	}

	private void InitIconTextures()
	{
		ConsumableGame consumableGameData = ConsumableData.GetConsumableGameData(mGameName);
		if (consumableGameData == null)
		{
			return;
		}
		ConsumableType[] consumableTypes = consumableGameData.ConsumableTypes;
		if (consumableTypes.Length == 0)
		{
			return;
		}
		for (int num = consumableTypes.Length; num > 0; num--)
		{
			Consumable[] consumables = consumableGameData.ConsumableTypes[num - 1].Consumables;
			for (int i = 0; i < consumables.Length; i++)
			{
				string[] array = consumables[i].Sprite.Split('/');
				if (array.Length >= 3)
				{
					mNumTexturesToBeLoaded++;
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnResLoadingEvent, typeof(Texture2D));
					if (mNumTexturesToBeLoaded > _MaxNumTexturesToLoad)
					{
						return;
					}
				}
			}
		}
	}

	public void OnResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mTexturesList.Add((Texture2D)inObject);
			mNumTexturesToBeLoaded--;
			if (mNumTexturesToBeLoaded == 0)
			{
				KAWidget[] gameConsumableBtns = _GameConsumableBtns;
				foreach (KAWidget kAWidget in gameConsumableBtns)
				{
					InitScrollTextures(kAWidget.name);
				}
			}
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Missing texture " + inURL);
			break;
		}
	}

	private void InitScrollTextures(string inName)
	{
		KAWidget kAWidget = FindItem(inName);
		if (!(kAWidget != null))
		{
			return;
		}
		KAWidget kAWidget2 = kAWidget.FindChildItem("IcoConsumable");
		if (!(kAWidget2 != null))
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			KAWidget component = UnityEngine.Object.Instantiate(kAWidget2.gameObject).GetComponent<KAWidget>();
			component.name = "IcoConsumable_Effect_" + i;
			if (mTexturesList != null && i < mTexturesList.Count)
			{
				component.GetUITexture().mainTexture = mTexturesList[i];
			}
			component.SetVisibility(inVisible: false);
			component.transform.parent = kAWidget2.transform.parent;
			component.transform.localPosition = Vector3.zero;
			component.transform.localEulerAngles = Vector3.zero;
			SetRect(component, i - 1);
		}
	}

	public void RegisterConsumable(Consumable inConsumable, bool showScrollEffect = true)
	{
		KAWidget kAWidget = AssignWidget(inConsumable, 1, updateAssignedIcon: false);
		if (!(kAWidget != null && showScrollEffect))
		{
			return;
		}
		KAWidget[] array = new KAWidget[2];
		for (int i = 0; i < 2; i++)
		{
			KAWidget component = kAWidget.transform.Find("IcoConsumable_Effect_" + i).GetComponent<KAWidget>();
			if (component != null)
			{
				SetRect(component, i - 1);
				array[i] = component;
			}
		}
		ShowScrollIcons(kAWidget, array, isVisible: true);
		ConsumableUserData obj = (ConsumableUserData)kAWidget.GetUserData();
		obj.pNumOfScrollIterations = 0;
		obj.pEffectScrollWidgets = array;
	}

	protected override void Update()
	{
		base.Update();
		if (CommonInventoryData.pIsReady && !mCallbackAssigned)
		{
			CommonInventoryData pInstance = CommonInventoryData.pInstance;
			pInstance.mUpdateCallback = (InventoryUpdateEventHandler)Delegate.Combine(pInstance.mUpdateCallback, new InventoryUpdateEventHandler(OnInventoryUpdate));
			mCallbackAssigned = true;
		}
		if (GetVisibility() && mHotKeys != null && mHotKeys.Count > 0)
		{
			foreach (HotKey mHotKey in mHotKeys)
			{
				if (KAInput.GetButtonUp(mHotKey._Key))
				{
					OnClick(mHotKey._Widget);
					break;
				}
			}
		}
		if (!mIsCarryDataSet && mConsumableInstance != null)
		{
			mIsCarryDataSet = true;
			UpdateCarryConsumableUI();
		}
		if (!_IsGamePaused)
		{
			mIsConsumableCoolingDown = false;
			CoolDown(_CarryConsumableBtns);
			CoolDown(_GameConsumableBtns);
			if (mIsInventoryUpdated && !mIsConsumableCoolingDown)
			{
				ResetButtonStates(updateCarryConsumables: true);
				mIsInventoryUpdated = false;
			}
		}
		KAWidget[] gameConsumableBtns = _GameConsumableBtns;
		foreach (KAWidget kAWidget in gameConsumableBtns)
		{
			ConsumableUserData consumableUserData = (ConsumableUserData)kAWidget.GetUserData();
			if (consumableUserData == null || consumableUserData.pNumOfScrollIterations <= -1)
			{
				continue;
			}
			for (int j = 0; j < 2; j++)
			{
				KAWidget kAWidget2 = consumableUserData.pEffectScrollWidgets[j];
				if (!(kAWidget2 != null))
				{
					continue;
				}
				Rect uvRect = kAWidget2.GetUITexture().uvRect;
				uvRect.y += Time.deltaTime * _IconCycleSpeed;
				uvRect.y = Mathf.Clamp(uvRect.y, j - 1, j);
				if (uvRect.y >= (float)j && uvRect.y > 0f)
				{
					uvRect.y *= -1f;
					consumableUserData.pCurrTextureIndex++;
					if (consumableUserData.pCurrTextureIndex >= mTexturesList.Count)
					{
						consumableUserData.pCurrTextureIndex = 0;
						consumableUserData.pNumOfScrollIterations++;
						if (consumableUserData.pNumOfScrollIterations == _NumOfIconCycles)
						{
							UpdateIcoConsumable(kAWidget, consumableUserData._Consumable.Sprite);
							ShowScrollIcons(kAWidget, consumableUserData.pEffectScrollWidgets, isVisible: false);
							consumableUserData.Reset();
							return;
						}
					}
					kAWidget2.SetTexture(mTexturesList[consumableUserData.pCurrTextureIndex]);
					if (_PowerupCyclingSound != null)
					{
						SnChannel.Play(_PowerupCyclingSound, "SFX_Pool", inForce: true);
					}
					KAWidget kAWidget3 = consumableUserData.pEffectScrollWidgets[0];
					consumableUserData.pEffectScrollWidgets[0] = consumableUserData.pEffectScrollWidgets[1];
					consumableUserData.pEffectScrollWidgets[1] = kAWidget3;
				}
				kAWidget2.GetUITexture().uvRect = uvRect;
			}
		}
	}

	private void SetRect(KAWidget inWidget, float rectY)
	{
		Rect uvRect = inWidget.GetUITexture().uvRect;
		uvRect.y = rectY;
		inWidget.GetUITexture().uvRect = uvRect;
	}

	private void ShowScrollIcons(KAWidget inWidget, KAWidget[] inEffectWidgets, bool isVisible)
	{
		KAWidget kAWidget = inWidget.FindChildItem("IcoConsumable");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(!isVisible);
		}
		for (int i = 0; i < inEffectWidgets.Length; i++)
		{
			inEffectWidgets[i].SetVisibility(isVisible);
		}
		if (_PowerupCyclingSound != null)
		{
			SnChannel.StopPool("SFX_Pool");
		}
	}

	private void CoolDown(KAWidget[] consumableBtns)
	{
		foreach (KAWidget kAWidget in consumableBtns)
		{
			if (kAWidget.GetState() == KAUIState.INTERACTIVE)
			{
				continue;
			}
			ConsumableUserData consumableUserData = (ConsumableUserData)kAWidget.GetUserData();
			if (consumableUserData == null || !(consumableUserData.pCoolDown > 0f) || !(consumableUserData.pLastUsedTime > DateTime.MinValue))
			{
				continue;
			}
			consumableUserData.mTimer += Time.deltaTime;
			KAWidget kAWidget2 = kAWidget.FindChildItem("Fill");
			if (kAWidget2 != null)
			{
				if (consumableUserData._Consumable != null)
				{
					kAWidget2.SetVisibility(inVisible: true);
				}
				else
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
				float num = (consumableUserData.pCoolDown - consumableUserData.mTimer) / consumableUserData.pCoolDown;
				if (num <= 0f)
				{
					num = 0f;
					consumableUserData.pCoolDown = 0f;
					consumableUserData.mTimer = 0f;
					kAWidget.SetDisabled(isDisabled: false);
					if (consumableUserData._Consumable == null)
					{
						UpdateIcoConsumable(kAWidget, _DefaultIconTextureURL);
					}
				}
				kAWidget2.SetProgressLevel(num);
			}
			mIsConsumableCoolingDown = true;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!inWidget.IsActive())
		{
			return;
		}
		ConsumableUserData consumableUserData = (ConsumableUserData)inWidget.GetUserData();
		Consumable consumable = null;
		if (consumableUserData != null && consumableUserData._Consumable != null)
		{
			consumable = consumableUserData._Consumable;
		}
		if (consumableUserData == null || consumable == null)
		{
			UtDebug.LogError("User data is null ");
			OnConsumablesEmpty();
		}
		else
		{
			if (consumableUserData.pNumOfScrollIterations != -1)
			{
				return;
			}
			if (!string.IsNullOrEmpty(mClickDisableConsumableNames.Find((string x) => x == consumable.name)))
			{
				if (_CannotUseSFX != null)
				{
					SnChannel.Play(_CannotUseSFX, "SFX_Pool", inForce: true);
				}
				return;
			}
			mConsumableInstance.OnConsumableUpdated(consumable);
			float coolDown = consumable.CoolDown;
			consumableUserData.pCoolDown = coolDown;
			consumableUserData.pLastUsedTime = DateTime.Now;
			consumableUserData.mTimer = 0f;
			inWidget.SetDisabled(isDisabled: true);
			KAWidget kAWidget = inWidget.FindChildItem("TxtQuanity");
			if (consumable._Type.Equals("Carry") && kAWidget != null)
			{
				int result = 0;
				int.TryParse(kAWidget.GetText(), out result);
				if (result >= 1)
				{
					CommonInventoryData.pInstance.RemoveItem(consumable.ItemID, updateServer: true);
					if (mConsumableCountDict.ContainsKey(consumable.name))
					{
						mConsumableCountDict[consumable.name]++;
					}
				}
				if (result - 1 >= 1)
				{
					kAWidget.SetText((result - 1).ToString());
					return;
				}
				if (_DefaultButtonClickable)
				{
					inWidget.SetDisabled(isDisabled: false);
				}
				consumableUserData.pCoolDown = 0f;
				UpdateIcoConsumable(inWidget, _DefaultIconTextureURL);
			}
			if (kAWidget != null)
			{
				kAWidget.SetText("");
			}
			consumableUserData._Consumable = null;
			KAWidget kAWidget2 = inWidget.FindChildItem("IcoConsumable");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
		}
	}

	protected virtual void OnConsumablesEmpty()
	{
	}

	protected void UpdateCarryConsumableUI()
	{
		UpdateCarryConsumableUI(ConsumableData.GetConsumableByTypeAndMode(mGameName, "Carry", mGameMode));
	}

	protected void UpdateCarryConsumableUI(List<Consumable> consumableList)
	{
		if (consumableList == null || consumableList.Count == 0)
		{
			return;
		}
		foreach (Consumable cData in consumableList)
		{
			if (!string.IsNullOrEmpty(mDisplayDisableConsumableNames.Find((string x) => x == cData.name)))
			{
				continue;
			}
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(cData.ItemID);
			if (userItemData != null)
			{
				if (!mConsumableCountDict.ContainsKey(cData.name))
				{
					mConsumableCountDict.Add(cData.name, 0);
				}
				int num = Math.Min(cData.MaxCount - mConsumableCountDict[cData.name], userItemData.Quantity);
				num = ((cData.MaxCount == 0) ? userItemData.Quantity : num);
				if (num != 0)
				{
					AssignWidget(cData, num, updateAssignedIcon: true);
				}
			}
		}
	}

	private KAWidget AssignWidget(Consumable inConsumable, int inQuantity, bool updateAssignedIcon)
	{
		KAWidget widget = GetWidget(inConsumable);
		if (widget == null)
		{
			UtDebug.Log("Couldn't assign widget!!!!");
			return null;
		}
		ConsumableUserData consumableUserData = (ConsumableUserData)widget.GetUserData();
		float pCoolDown = 0f;
		DateTime pLastUsedTime = DateTime.MaxValue;
		float mTimer = 0f;
		if (consumableUserData != null)
		{
			pCoolDown = consumableUserData.pCoolDown;
			pLastUsedTime = consumableUserData.pLastUsedTime;
			mTimer = consumableUserData.mTimer;
		}
		if (widget != null)
		{
			if (inConsumable._Type.Equals("Carry"))
			{
				KAWidget kAWidget = widget.FindChildItem("TxtQuanity");
				if (kAWidget != null)
				{
					int maxCount = inConsumable.MaxCount;
					if (maxCount > 0 && maxCount <= inQuantity)
					{
						kAWidget.SetText(maxCount.ToString());
					}
					else
					{
						kAWidget.SetText(inQuantity.ToString());
					}
				}
			}
			ConsumableUserData consumableUserData2 = new ConsumableUserData(inConsumable);
			consumableUserData2.pCoolDown = pCoolDown;
			consumableUserData2.pLastUsedTime = pLastUsedTime;
			consumableUserData2.mTimer = mTimer;
			widget.name = inConsumable.name;
			widget.SetUserData(consumableUserData2);
			if (updateAssignedIcon)
			{
				UpdateIcoConsumable(widget, inConsumable.Sprite);
			}
		}
		return widget;
	}

	private void UpdateIcoConsumable(KAWidget inWidget, string inSprite)
	{
		KAWidget kAWidget = inWidget.FindChildItem("IcoConsumable");
		if (kAWidget != null && !string.IsNullOrEmpty(inSprite) && inSprite.Split('/').Length >= 3)
		{
			kAWidget.SetTextureFromBundle(inSprite, base.gameObject);
		}
	}

	private void OnTextureLoaded(KAWidget inWidget)
	{
		inWidget.SetVisibility(GetVisibility());
	}

	private KAWidget GetWidget(Consumable inConsumable)
	{
		string type = inConsumable._Type;
		if (!(type == "Game"))
		{
			if (type == "Carry")
			{
				return Array.Find(_CarryConsumableBtns, (KAWidget t) => t.name.Contains(inConsumable.name)) ?? GetAvailableWidget(_CarryConsumableBtns);
			}
			return null;
		}
		return GetAvailableWidget(_GameConsumableBtns);
	}

	public KAWidget GetAvailableWidget(KAWidget[] widgets)
	{
		DateTime? dateTime = null;
		KAWidget result = null;
		foreach (KAWidget kAWidget in widgets)
		{
			ConsumableUserData consumableUserData = (ConsumableUserData)kAWidget.GetUserData();
			if (consumableUserData == null || consumableUserData._Consumable == null)
			{
				return kAWidget;
			}
			if (consumableUserData == null)
			{
				continue;
			}
			bool flag = false;
			if (!dateTime.HasValue)
			{
				flag = true;
			}
			else
			{
				DateTime value = dateTime.Value;
				DateTime? acquireTime = consumableUserData._AcquireTime;
				if (value > acquireTime)
				{
					flag = true;
				}
			}
			if (flag)
			{
				dateTime = consumableUserData._AcquireTime;
				result = kAWidget;
			}
		}
		return result;
	}

	public void SetConsumableIconVisibility(bool inVisibility)
	{
		SetIcoConsumableVisibility(_CarryConsumableBtns, inVisibility);
		SetIcoConsumableVisibility(_GameConsumableBtns, inVisibility);
	}

	private void SetIcoConsumableVisibility(KAWidget[] inWidgets, bool inVisibility)
	{
		foreach (KAWidget kAWidget in inWidgets)
		{
			if (kAWidget != null)
			{
				KAWidget kAWidget2 = kAWidget.FindChildItem("IcoConsumable");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisibility);
				}
			}
		}
	}

	public void ResetButtonStates(bool updateCarryConsumables = false)
	{
		int num = 1;
		List<KAWidget> list = new List<KAWidget>();
		list.AddRange(_CarryConsumableBtns);
		if (!updateCarryConsumables)
		{
			list.AddRange(_GameConsumableBtns);
		}
		foreach (KAWidget item in list)
		{
			if (!(item != null))
			{
				continue;
			}
			ConsumableUserData consumableUserData = (ConsumableUserData)item.GetUserData();
			int num2 = 0;
			if (consumableUserData != null && consumableUserData._Consumable != null && mConsumableCountDict.ContainsKey(consumableUserData._Consumable.name))
			{
				num2 = mConsumableCountDict[consumableUserData._Consumable.name];
			}
			item.SetUserData(null);
			if (num2 == 0)
			{
				KAWidget kAWidget = item.FindChildItem("TxtQuanity");
				if (kAWidget != null)
				{
					kAWidget.SetText("");
				}
				UpdateIcoConsumable(item, _DefaultIconTextureURL);
			}
			item.SetInteractive(isInteractive: true);
			KAWidget kAWidget2 = item.FindChildItem("Fill");
			if (kAWidget2 != null)
			{
				kAWidget2.SetProgressLevel(0f);
				kAWidget2.SetVisibility(inVisible: false);
			}
			AssignHotkey(num, item);
			num++;
		}
		mIsCarryDataSet = false;
		mDisplayDisableConsumableNames = new List<string>();
		mClickDisableConsumableNames = new List<string>();
	}

	private void AssignHotkey(int index, KAWidget widget)
	{
		string text = "Consumable" + index;
		foreach (HotKey mHotKey in mHotKeys)
		{
			if (mHotKey._Key == text)
			{
				mHotKey._Widget = widget;
				return;
			}
		}
		HotKey hotKey = new HotKey();
		hotKey._Key = "Consumable" + index;
		hotKey._Widget = widget;
		mHotKeys.Add(hotKey);
	}

	public void PositionUI(Vector3 pos)
	{
		if (pos != Vector3.zero)
		{
			base.transform.position = pos;
		}
		else
		{
			base.transform.position = mConsumablePos;
		}
	}

	public void AddDisabledConsumables(List<string> inDisplayDisableConsumableNames, List<string> inClickDisableConsumableNames)
	{
		mDisplayDisableConsumableNames = inDisplayDisableConsumableNames;
		mClickDisableConsumableNames = inClickDisableConsumableNames;
	}

	private void ShowKeys(bool isVisible)
	{
		KAWidget[] carryConsumableBtns = _CarryConsumableBtns;
		for (int i = 0; i < carryConsumableBtns.Length; i++)
		{
			carryConsumableBtns[i].FindChildItem("TxtKey").SetVisibility(isVisible);
		}
		carryConsumableBtns = _GameConsumableBtns;
		for (int i = 0; i < carryConsumableBtns.Length; i++)
		{
			carryConsumableBtns[i].FindChildItem("TxtKey").SetVisibility(isVisible);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		SetConsumableIconVisibility(inVisible);
	}

	protected virtual void OnInventoryUpdate()
	{
		mIsInventoryUpdated = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (CommonInventoryData.pIsReady)
		{
			CommonInventoryData pInstance = CommonInventoryData.pInstance;
			pInstance.mUpdateCallback = (InventoryUpdateEventHandler)Delegate.Remove(pInstance.mUpdateCallback, new InventoryUpdateEventHandler(OnInventoryUpdate));
		}
	}
}
