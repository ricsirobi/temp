using UnityEngine;

public class UiSocialCrateReward : KAUI
{
	private GameObject mMessageObject;

	private KAWidget mBtnOK;

	private KAWidget mRewardText;

	private int mItemsLoaded;

	private int mTotalItemCount;

	public UiSocialCrateRewardMenu _Menu;

	public LocaleString _InstructionRewardText = new LocaleString("Check your inventory to use them");

	protected override void Start()
	{
		base.Start();
		mBtnOK = FindItem("OKBtn");
		mBtnOK.SetInteractive(isInteractive: false);
		mRewardText = FindItem("TxtDescReward");
		AddReward();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnOK)
		{
			mMessageObject.SendMessage("OnExit");
		}
	}

	private void AddReward()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(SocialBoxManager.pInstance._SocialBoxItemID, 1);
		CommonInventoryData.pInstance.DoPurchase(2, SocialBoxManager.pInstance._SocialBoxStoreID, SocialBoxPurchaseHandler);
	}

	public void SocialBoxPurchaseHandler(CommonInventoryResponse ret)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (ret != null && ret.Success)
		{
			mTotalItemCount = ret.CommonInventoryIDs.Length;
			for (int i = 0; i < ret.CommonInventoryIDs.Length; i++)
			{
				ItemData.Load(ret.CommonInventoryIDs[i].ItemID, OnLoadItemDataReady, null);
			}
			mRewardText.SetText(_InstructionRewardText.GetLocalizedString());
		}
	}

	private void OnLoadItemDataReady(int itemID, ItemData item, object inUserData)
	{
		string[] array = item.IconName.Split('/');
		KAWidget kAWidget = DuplicateWidget(_Menu._Template);
		kAWidget.FindChildItem("Icon").SetTextureFromBundle(array[0] + "/" + array[1], array[2]);
		_Menu.AddWidget(kAWidget);
		mItemsLoaded++;
		if (mItemsLoaded >= mTotalItemCount)
		{
			mBtnOK.SetInteractive(isInteractive: true);
		}
	}

	public void SetMessageObject(GameObject msg)
	{
		mMessageObject = msg;
	}
}
