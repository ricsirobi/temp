using UnityEngine;

public class UiBackpackItemActionDB : KAUI
{
	protected ItemData mItemData;

	private KAWidget mCloseBtn;

	public ItemData pItemData
	{
		set
		{
			mItemData = value;
		}
	}

	public GameObject pMessageObject { get; set; }

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		mCloseBtn = FindItem("CloseBtn");
		KAWidget kAWidget = FindItem("Image");
		if (!(kAWidget != null) || mItemData == null)
		{
			return;
		}
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		string attribute = mItemData.GetAttribute("ActionImage", "");
		if (attribute.StartsWith("http://"))
		{
			kAWidget.SetTextureFromURL(attribute, base.gameObject);
			return;
		}
		string[] array = attribute.Split('/');
		if (array.Length >= 3)
		{
			kAWidget.SetTextureFromBundle(array[0] + "/" + array[1], array[2], base.gameObject);
		}
	}

	private void OnTextureLoaded(KAWidget inWidget)
	{
		SetInteractive(interactive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn)
		{
			if (mItemData != null && MissionManager.pIsReady)
			{
				MissionManager.pInstance.CheckForTaskCompletion("Action", "OpenItem", mItemData.ItemID);
			}
			Destroy();
		}
	}

	public void Destroy()
	{
		if (pMessageObject != null)
		{
			pMessageObject.SendMessage("OnActionDBClose", SendMessageOptions.DontRequireReceiver);
		}
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
	}
}
