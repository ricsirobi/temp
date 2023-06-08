using UnityEngine;

public class UiEquipment : KAUISelect
{
	public int _EquipmentCategoryID = 242;

	public CategorySlotData[] _SlotItems;

	public float _RotationSpeed = 40f;

	public string _LeftRotateBtn = "BtnRotatelLt";

	public string _RightRotateBtn = "BtnRotatelRt";

	public string _DragWidgetName = "DragWidget";

	public string _EquipmentCloseMsg = "OnEquipmentClose";

	public GameObject _CloseMsgObject;

	protected UiEquipmentMenu mMenu;

	protected CategorySlotData mCurrentSlot;

	protected GameObject mEquipmentOwner;

	protected static Vector3 mAvatarOldPos = Vector3.zero;

	protected static Quaternion mAvatarOldRot = Quaternion.identity;

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	public override void Initialize()
	{
		base.Initialize();
		mMenu = (UiEquipmentMenu)GetMenu("KAUIEquipmentMenu");
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (mAvatarOldPos == Vector3.zero)
		{
			mAvatarOldRot = AvAvatar.mTransform.rotation;
			mAvatarOldPos = AvAvatar.position;
		}
		if (mMenu != null)
		{
			mMenu.ChangeCategory(_EquipmentCategoryID, forceChange: true);
		}
	}

	public override void OnClose()
	{
		if (_CloseMsgObject != null && !string.IsNullOrEmpty(_EquipmentCloseMsg))
		{
			_CloseMsgObject.SendMessage(_EquipmentCloseMsg);
		}
		Object.Destroy(base.gameObject.transform.parent.gameObject);
		ResetAvatar();
	}

	public void ResetAvatar()
	{
		Input.ResetInputAxes();
		AvAvatar.mTransform.rotation = mAvatarOldRot;
		AvAvatar.SetPosition(mAvatarOldPos);
		mAvatarOldPos = Vector3.zero;
	}

	public virtual void SetItem(KAWidget widget)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)widget.GetUserData();
		CategorySlotData[] slotItems = _SlotItems;
		foreach (CategorySlotData categorySlotData in slotItems)
		{
			if (kAUISelectItemData._ItemData.HasCategory(categorySlotData._Category))
			{
				mCurrentSlot = categorySlotData;
				if (FindItem(mCurrentSlot._SlotItemName).GetNumChildren() > 2)
				{
					RemoveItem(mCurrentSlot);
				}
				AddItem(mCurrentSlot, widget);
				break;
			}
		}
	}

	public virtual void AddItem(CategorySlotData sd, KAWidget widget)
	{
		KAWidget kAWidget = FindItem(sd._SlotItemName);
		KAWidget kAWidget2 = kAWidget.FindChildItem("Data");
		kAWidget2.SetState(KAUIState.NOT_INTERACTIVE);
		kAWidget2.SetTexture(widget.GetTexture());
		kAWidget2.SetScale(new Vector3(100f, 100f, 0f));
		kAWidget2.SetVisibility(inVisible: true);
		widget.SetVisibility(inVisible: false);
		kAWidget.AddChild(widget);
		mMenu.RemoveWidget(widget);
	}

	public virtual void RemoveItem(CategorySlotData sd)
	{
		KAWidget kAWidget = FindItem(sd._SlotItemName);
		KAWidget kAWidget2 = null;
		foreach (KAWidget pChildWidget in kAWidget.pChildWidgets)
		{
			if (pChildWidget.name != "Data" && pChildWidget.name != "Slot")
			{
				kAWidget2 = pChildWidget;
				break;
			}
		}
		KAWidget kAWidget3 = kAWidget.FindChildItem("Data");
		kAWidget3.SetTexture(null);
		kAWidget3.SetVisibility(inVisible: false);
		kAWidget2.SetVisibility(inVisible: true);
		mMenu.AddWidget(kAWidget2);
		kAWidget.RemoveChildItem(kAWidget2);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		string text = inWidget.name;
		CategorySlotData[] slotItems = _SlotItems;
		foreach (CategorySlotData categorySlotData in slotItems)
		{
			if (text == categorySlotData._SlotItemName)
			{
				RemoveItem(categorySlotData);
				break;
			}
		}
	}

	public override void OnPress(KAWidget inWidget, bool inPressed)
	{
		base.OnPress(inWidget, inPressed);
		if (inPressed)
		{
			float num = 0f;
			if (!string.IsNullOrEmpty(_LeftRotateBtn) && inWidget.name == _LeftRotateBtn)
			{
				num = _RotationSpeed * Time.deltaTime;
			}
			else if (!string.IsNullOrEmpty(_RightRotateBtn) && inWidget.name == _RightRotateBtn)
			{
				num = (0f - _RotationSpeed) * Time.deltaTime;
			}
			if (num != 0f && mEquipmentOwner != null)
			{
				mEquipmentOwner.transform.Rotate(0f, num, 0f);
			}
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		base.OnDrag(inWidget, inDelta);
		if (!string.IsNullOrEmpty(_DragWidgetName) && inWidget.name == _DragWidgetName)
		{
			float num = (0f - _RotationSpeed) * Time.deltaTime * inDelta.x;
			if (num != 0f && mEquipmentOwner != null)
			{
				mEquipmentOwner.transform.Rotate(0f, num, 0f);
			}
		}
	}

	public override void OnDrop(KAWidget inDroppedWidget, KAWidget inTargetWidget)
	{
		base.OnDrop(inDroppedWidget, inTargetWidget);
		ItemDataCategory[] category = ((KAUISelectItemData)inDroppedWidget.GetUserData())._ItemData.Category;
		for (int i = 0; i < category.Length; i++)
		{
			if (category[i].CategoryName == inTargetWidget.name)
			{
				SetItem(inDroppedWidget);
				break;
			}
		}
	}
}
