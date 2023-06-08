using System.Collections.Generic;

namespace ExpansionMissionBoard;

public class UiExpansionMissionTemplate : KAWidget
{
	public KAWidget _Icon;

	public KAWidget _Name;

	public KAWidget _Description;

	public KAWidget _Prompt;

	public string _GemsSprite = "AniDWDragonsBaseCurrencyGems";

	public string _CoinsSprite = "AniDWDragonsBaseCurrencyCoins";

	private List<StoreData> mStoreDatas = new List<StoreData>();

	private string mActionText;

	public bool Init(ExpansionMission inExpansionMission, string inActionText)
	{
		if (inExpansionMission == null)
		{
			return false;
		}
		mActionText = inActionText;
		if ((bool)_Name)
		{
			_Name.SetText(inExpansionMission._NameText.GetLocalizedString());
		}
		ExpansionMissionData pTargetMission = inExpansionMission.pTargetMission;
		Mission mission = null;
		if (pTargetMission != null && pTargetMission._MissionID > 0)
		{
			mission = MissionManager.pInstance.GetMission(pTargetMission._MissionID);
		}
		if (mission == null)
		{
			return false;
		}
		if ((bool)_Description)
		{
			List<Task> tasks = new List<Task>();
			MissionManager.pInstance.GetNextTask(mission, ref tasks);
			Task task = ((tasks.Count > 0) ? tasks[0] : null);
			string text = inExpansionMission._DescriptionText?.GetLocalizedString();
			if (inExpansionMission.pState == State.InProgress && task?.pData?.Title != null)
			{
				text = MissionManager.pInstance.FormatText(task.pData.Title.ID, task.pData.Title.Text, task);
			}
			_Description.SetText(text);
		}
		string missionIconURL = inExpansionMission._MissionIconURL;
		if (!string.IsNullOrEmpty(missionIconURL))
		{
			if (missionIconURL.StartsWith("http://"))
			{
				_Icon?.SetTextureFromURL(missionIconURL);
			}
			else
			{
				string[] array = missionIconURL.Split('/');
				if (array.Length >= 3)
				{
					_Icon?.SetTextureFromBundle(array[0] + "/" + array[1], array[2]);
				}
			}
			_Icon?.SetVisibility(inVisible: true);
		}
		if (inExpansionMission.pState != State.Paywall)
		{
			switch (inExpansionMission.pState)
			{
			case State.New:
			case State.Completed:
				_Prompt?.SetText(mActionText);
				break;
			case State.InProgress:
				_Prompt?.SetText(mission.pData?.Title?.GetLocalizedString());
				break;
			case State.Between:
				_Prompt?.SetText(pTargetMission?._MissionDescriptionText.GetLocalizedString());
				break;
			}
		}
		else if (inExpansionMission.pState == State.Paywall)
		{
			ItemStoreDataLoader.Load(new List<int> { inExpansionMission._StoreID }.ToArray(), OnStoreLoaded, inExpansionMission);
		}
		return true;
	}

	public void OnStoreLoaded(List<StoreData> inStoreData, object inUserData)
	{
		if (inStoreData == null)
		{
			return;
		}
		mStoreDatas.AddRange(inStoreData);
		ExpansionMission expansionMission = (ExpansionMission)inUserData;
		ItemData itemData = GetItemData(expansionMission.pTicketID);
		int num = 0;
		if (itemData == null)
		{
			_Prompt?.SetVisibility(inVisible: false);
			return;
		}
		int purchaseType = itemData.GetPurchaseType();
		switch (purchaseType)
		{
		case 2:
			num = itemData.FinalCashCost;
			break;
		case 1:
			num = itemData.FinalCost;
			break;
		}
		if (num > 0)
		{
			_Prompt?.SetSprite((purchaseType == 2) ? _GemsSprite : _CoinsSprite);
			_Prompt?.SetText(num.ToString());
			_Prompt?.SetVisibility(inVisible: true);
		}
		else
		{
			_Prompt?.SetVisibility(inVisible: false);
		}
	}

	public ItemData GetItemData(int inItemID)
	{
		if (mStoreDatas == null)
		{
			return null;
		}
		foreach (StoreData mStoreData in mStoreDatas)
		{
			ItemData[] items = mStoreData._Items;
			foreach (ItemData itemData in items)
			{
				if (itemData.ItemID == inItemID)
				{
					return itemData;
				}
			}
		}
		return null;
	}
}
