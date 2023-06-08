using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UiMiniGameDragonAgeUp : UiDragonAgeUp
{
	public static Action<GameObject> _OnMiniGameLoadStart;

	public List<PetAgeDisplayText> _PetCurrentAgeData;

	public KAWidget _TemplateWidget;

	public LocaleString _UnmountablePetBlockedText;

	private KAWidget mPlayBtn;

	protected override void OnAgeUpStoreLoaded(StoreData sd)
	{
		base.OnAgeUpStoreLoaded(sd);
		if (_AgeUpItemMenu.GetNumItems() != 0)
		{
			PopulateCurrentAgeData();
		}
	}

	public override List<PetAgeUpData> GetAgeUpData(RaisedPetStage fromStage)
	{
		if (_AgeUpData != null && _AgeUpData.Length != 0)
		{
			List<PetAgeUpData> list = new List<PetAgeUpData>();
			PetAgeUpData[] array = Array.FindAll(_AgeUpData, (PetAgeUpData data) => data._FromPetStage == fromStage && data._FromPetStage < data._ToPetStage);
			array.OrderBy((PetAgeUpData x) => x._ToPetStage);
			if (array != null || array.Length != 0)
			{
				list.Add(array[0]);
				return list;
			}
		}
		else
		{
			UtDebug.LogError("Age-up data is empty");
		}
		return null;
	}

	private void PopulateCurrentAgeData()
	{
		PetAgeDisplayText petAgeDisplayText = _PetCurrentAgeData.Find((PetAgeDisplayText ele) => ele._Age == base.pRaisedPetData.pStage);
		KAWidget kAWidget = DuplicateWidget(_TemplateWidget);
		kAWidget.FindChildItem("TxtHeader").SetTextByID(petAgeDisplayText._TitleText._ID, petAgeDisplayText._TitleText._Text);
		kAWidget.FindChildItem("TxtInfo").SetText(mAllowUnmountablePet ? petAgeDisplayText._DisplayText.GetLocalizedString() : _UnmountablePetBlockedText.GetLocalizedString());
		mPlayBtn = kAWidget.FindChildItem("PlayBtn");
		mPlayBtn.SetVisibility(mAllowUnmountablePet);
		if (_AgeUpItemMenu != null)
		{
			_AgeUpItemMenu.AddWidgetAt(0, kAWidget);
		}
		kAWidget.SetVisibility(inVisible: true);
		mAllowTriggerAction = mPlayBtn.GetVisibility();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mPlayBtn)
		{
			KAUI.RemoveExclusive(this);
			if (_OnMiniGameLoadStart != null)
			{
				_OnMiniGameLoadStart(null);
			}
			else if (mCallbackObj != null)
			{
				mCallbackObj.SendMessage("DoTriggerAction", base.gameObject);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public override void AgeUpPurchaseDone(CommonInventoryResponse ret)
	{
		DestroyDB();
		if (ret != null && ret.Success)
		{
			PerformAgeUp(mProcessedAgeItem.pPetAgeUpData._AgeUpItemID);
			return;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		ShowKAUIDialog("PfKAUIGenericDB", "Purchase Falied", "", "", "DestroyDB", "", destroyDB: true, _PurchaseFailText, base.gameObject);
	}

	protected override void OnUserRankReady(UserRank rank, object userData)
	{
		base.OnUserRankReady(rank, userData);
		KAUICursorManager.SetDefaultCursor("Arrow");
		ShowKAUIDialog("PfKAUIGenericDB", "Purchase Success", "", "", "DestroyAllDB", "", destroyDB: true, _PurchaseSuccessText, base.gameObject);
	}

	private void DestroyAllDB()
	{
		DestroyDB();
		KAUI.RemoveExclusive(this);
		if (mUiDragonAgeUpDoneCallback != null)
		{
			mUiDragonAgeUpDoneCallback();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
