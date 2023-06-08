using UnityEngine;

public class UiEggsDisplayMenu : KAUIMenu
{
	private UiMultiEggHatching mUiMultiEggHatching;

	protected override void Start()
	{
		base.Start();
		mUiMultiEggHatching = (UiMultiEggHatching)_ParentUi;
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (!(UICursorManager.GetCursorName() == "Loading"))
		{
			base.OnClick(inWidget);
			mUiMultiEggHatching.ProcessEggClick(inWidget);
		}
	}

	public void SetupEggDisplayWidget(KAWidget widget)
	{
		EggDisplayWidgetData eggDisplayWidgetData = (EggDisplayWidgetData)widget.GetUserData();
		widget.FindChildItem("TxtEggCount").SetText(eggDisplayWidgetData.EggData.Quantity.ToString());
		widget.FindChildItem("TxtEggName").SetText(eggDisplayWidgetData.EggData.Item.ItemName);
		KAWidget kAWidget = widget.FindChildItem("Icon");
		if (kAWidget != null)
		{
			kAWidget.SetTextureFromBundle(eggDisplayWidgetData.EggData.Item.IconName, null, OnEggIconDownload);
		}
	}

	private void OnEggIconDownload(KAWidget widget, bool success)
	{
		widget.pParentWidget.SetVisibility(inVisible: true);
	}

	public override void OnDragStart(KAWidget inWidget)
	{
		if (!(UICursorManager.GetCursorName() == "Loading"))
		{
			base.OnDragStart(inWidget);
			mUiMultiEggHatching.OnEggPick(inWidget);
		}
	}

	public override void OnDragEnd(KAWidget inWidget)
	{
		base.OnDragEnd(inWidget);
		if (inWidget.GetUserData() != null)
		{
			ClearDragItem();
			mUiMultiEggHatching.OnEggDropped(inWidget);
		}
	}

	private void ClearDragItem()
	{
		if (KAUIManager.pInstance.pDragItem != null)
		{
			KAUIManager.pInstance.pDragItem.DetachFromCursor();
			Object.Destroy(KAUIManager.pInstance.pDragItem.gameObject);
		}
	}
}
