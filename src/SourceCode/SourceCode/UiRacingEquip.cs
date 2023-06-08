using System.Collections.Generic;
using UnityEngine;

public class UiRacingEquip : KAUI
{
	public RacingEquipmentTab[] _Consumables;

	public KAWidget[] _EquipmentWidgets;

	public KAWidget _UpArrow;

	public KAWidget _DownArrow;

	public int _NumEquipmentSlots = 3;

	private KAWidget mTxtTimer;

	private MainMenu mMainMenu;

	private GameScreen mLastScreen;

	private KAWidget mEquipmentButtonClicked;

	private int mCurrentCategory = -1;

	private UserItemData[] mConsumableItems;

	private bool mInSkillsTab;

	private int mEquipmentIndex;

	protected override void Start()
	{
		base.Start();
		if (ConsumableItems.mInstance == null)
		{
			new ConsumableItems();
		}
		InitConsumables();
	}

	private void InitConsumables()
	{
		if (_Consumables == null || _Consumables.Length == 0 || !CommonInventoryData.pIsReady || (ConsumableItems.mInstance.GetConsumablesList() != null && ConsumableItems.mInstance.GetConsumablesList().Count >= _Consumables.Length))
		{
			return;
		}
		RacingEquipmentTab[] consumables = _Consumables;
		foreach (RacingEquipmentTab racingEquipmentTab in consumables)
		{
			UserItemData[] items = CommonInventoryData.pInstance.GetItems(racingEquipmentTab._Category);
			if (items != null && items.Length != 0)
			{
				ItemData item = items[0].Item;
				ConsumableItems.mInstance.AddConsumableItem(racingEquipmentTab._Category, item.ItemID);
			}
		}
	}

	public void Show(MainMenu mainMenu, GameScreen inLastScreen)
	{
		mMainMenu = mainMenu;
		mLastScreen = inLastScreen;
		SetVisibility(inVisible: true);
		SetState(KAUIState.INTERACTIVE);
		KAWidget kAWidget = FindItem("PlayerDragonPic");
		Texture dragonPicture = mainMenu.GetDragonPicture(SanctuaryManager.pCurPetInstance.gameObject, kAWidget, UserInfo.pInstance.UserID);
		kAWidget.SetTexture(dragonPicture, inPixelPerfect: true);
		mTxtTimer = FindItem("TxtTimer");
		if (inLastScreen == GameScreen.GAME_LOBBY_SCREEN)
		{
			mTxtTimer.SetVisibility(inVisible: true);
			mTxtTimer.SetText(string.Empty);
		}
		else
		{
			mTxtTimer.SetVisibility(inVisible: false);
		}
		kAWidget = FindItem("BtnRaceSkills01");
		kAWidget.GetComponent<ExpandButton>().pExpand = true;
		OnClick(kAWidget);
	}

	public override void OnClick(KAWidget inWidget)
	{
		if ("BtnDone" == inWidget.name || "BtnBack" == inWidget.name)
		{
			SetVisibility(inVisible: false);
			SetState(KAUIState.DISABLED);
			mMainMenu.MoveNext(mLastScreen);
		}
		else if (inWidget != mEquipmentButtonClicked && (inWidget.name.Contains("BtnRaceConsumable") || inWidget.name.Contains("BtnRaceSkills")))
		{
			bool flag = inWidget.name.Contains("BtnRaceSkills");
			if (mInSkillsTab != flag)
			{
				mInSkillsTab = flag;
				for (int i = 0; i < _EquipmentWidgets.Length; i++)
				{
					_EquipmentWidgets[i].ResetWidget();
					_EquipmentWidgets[i].SetText("");
					if (mInSkillsTab)
					{
						_EquipmentWidgets[i].name = "Skill";
						_EquipmentWidgets[i].pBackground.gameObject.SetActive(value: true);
						_EquipmentWidgets[i].GetUITexture().gameObject.SetActive(value: false);
					}
					else
					{
						_EquipmentWidgets[i].name = "Consumable";
						_EquipmentWidgets[i].pBackground.gameObject.SetActive(value: false);
						_EquipmentWidgets[i].GetUITexture().gameObject.SetActive(value: true);
					}
				}
				mEquipmentIndex = 0;
			}
			int count = 0;
			if (!mInSkillsTab)
			{
				count = LoadConsumableItems(inWidget);
			}
			if (mEquipmentButtonClicked != null)
			{
				ResetData();
			}
			mEquipmentButtonClicked = inWidget;
			ExpandButton component = inWidget.GetComponent<ExpandButton>();
			if (component != null)
			{
				SetEquipmentButtons(inWidget, component, count);
			}
		}
		else if (inWidget.name == "BtnUpArrow")
		{
			if (mEquipmentIndex + _NumEquipmentSlots < mConsumableItems.Length)
			{
				mEquipmentIndex++;
			}
			UpdateConsumableItems();
		}
		else if (inWidget.name == "BtnDownArrow")
		{
			if (mEquipmentIndex > 0)
			{
				mEquipmentIndex--;
			}
			UpdateConsumableItems();
		}
		else if (inWidget.name == "Consumable")
		{
			mEquipmentButtonClicked.SetTexture(inWidget.GetUITexture().mainTexture, inPixelPerfect: true);
		}
		else if (inWidget.name == "Skill")
		{
			mEquipmentButtonClicked.SetText(inWidget.GetText());
			mEquipmentButtonClicked.GetUITexture().gameObject.SetActive(value: false);
			mEquipmentButtonClicked.pBackground.enabled = true;
			mEquipmentButtonClicked.pBackground.UpdateSprite(inWidget.pBackground.spriteName);
		}
		base.OnClick(inWidget);
	}

	protected override void Update()
	{
		base.Update();
		if (_UpArrow.GetVisibility() && !_UpArrow.collider.enabled)
		{
			_UpArrow.collider.enabled = true;
		}
		for (int i = 0; i < _EquipmentWidgets.Length; i++)
		{
			if (_EquipmentWidgets[i].GetVisibility() && !_EquipmentWidgets[i].collider.enabled)
			{
				_EquipmentWidgets[i].collider.enabled = true;
			}
		}
		if (_DownArrow.GetVisibility() && !_DownArrow.collider.enabled)
		{
			_DownArrow.collider.enabled = true;
		}
	}

	private void ResetData()
	{
		List<KAWidget> pChildWidgets = mEquipmentButtonClicked.pChildWidgets;
		for (int num = pChildWidgets.Count - 1; num >= 0; num--)
		{
			mEquipmentButtonClicked.RemoveChildItem(pChildWidgets[num]);
		}
	}

	private void SetEquipmentButtons(KAWidget inWidget, ExpandButton expandButton, int count)
	{
		Vector3 localPosition = new Vector3(0f, 0f, 1f);
		inWidget.AddChild(_UpArrow);
		_UpArrow.SetVisibility(count == 3);
		_UpArrow.transform.localPosition = localPosition;
		for (int i = 0; i < _EquipmentWidgets.Length; i++)
		{
			inWidget.AddChild(_EquipmentWidgets[i]);
			_EquipmentWidgets[i].SetVisibility((i < count) ? true : false);
			_EquipmentWidgets[i].transform.localPosition = localPosition;
		}
		inWidget.AddChild(_DownArrow);
		_DownArrow.SetVisibility(count == 3);
		_DownArrow.transform.localPosition = localPosition;
		expandButton.Init();
	}

	public int LoadConsumableItems(KAWidget inWidget)
	{
		int num = -1;
		RacingEquipmentTab[] consumables = _Consumables;
		foreach (RacingEquipmentTab racingEquipmentTab in consumables)
		{
			if (racingEquipmentTab._WidgetName == inWidget.name)
			{
				num = racingEquipmentTab._Category;
				break;
			}
		}
		if (num < 0)
		{
			return 0;
		}
		if (num != mCurrentCategory)
		{
			mConsumableItems = CommonInventoryData.pInstance.GetItems(num);
			mCurrentCategory = num;
		}
		return UpdateConsumableItems();
	}

	private int UpdateConsumableItems()
	{
		int num = 0;
		if (mConsumableItems != null && mConsumableItems.Length != 0)
		{
			int num2 = mEquipmentIndex + 3;
			if (num2 > mConsumableItems.Length)
			{
				num2 = mConsumableItems.Length;
			}
			for (int i = mEquipmentIndex; i < num2; i++)
			{
				ItemData item = mConsumableItems[i].Item;
				ConsumableItemData consumableItemData = new ConsumableItemData();
				consumableItemData.SetPixelPerfect(inPixelPerfect: true);
				consumableItemData.Init(item);
				_EquipmentWidgets[num].collider.enabled = true;
				_EquipmentWidgets[num].SetUserData(consumableItemData);
				_EquipmentWidgets[num].GetUITexture().gameObject.SetActive(value: true);
				_EquipmentWidgets[num].pBackground.enabled = false;
				consumableItemData.ShowLoadingItem(inShow: true);
				consumableItemData.LoadResource();
				num++;
			}
		}
		return num;
	}

	public void UpdateTime(float count)
	{
		mTxtTimer.SetText(count.ToString());
	}
}
